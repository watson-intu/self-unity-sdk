/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;

namespace IBM.Watson.Self.Utils
{
    /// <summary>
    /// This object is created to explore the graph of self instances through the TopicClient object. 
    /// </summary>
    public class SelfDiscovery
    {
        #region Private Data
        private string m_MulticastAddress = "239.255.0.1";
        private int m_Port = 9444;
        private int m_DefaultListeningPort = 9443;
        private Thread m_ReceiveThread = null;
        private List<SelfInstance> m_Discovered = new List<SelfInstance>();
        private int m_IPMulticastTimeToLive = 5;
        private int m_NumberOfInstances = 0;
        private UdpClient m_UdpClient = null;
        private int m_AsyncDiscoveredID = -1;
        #endregion

        #region Public Types
        public class SelfInstance
        {
            #region Public Properties
            public string Name { get; set; }
            public string Type { get; set; }
            public string MacId { get; set; }
            public string IPv4 { get; set; }
            public int Port { get; set; }
            public string EmbodimentId { get; set; }
            public string InstanceId { get; set; }
            public string GroupId { get; set; }
            public string OrgId { get; set; }
            public DateTime LastPing { get; set; }
            #endregion

            public override string ToString()
            {
                return string.Format("[SelfInstance: Name={0}, Type={1}, MacId={2}, IPv4={3}, Port={4}, EmbodimentId={5}, InstanceId={6}, GroupId={7}, OrgId={8}, LastPing={9}]", 
                    Name, Type, MacId, IPv4, Port, EmbodimentId, InstanceId, GroupId, OrgId, LastPing);
            }
        }
        public delegate void OnInstance( SelfInstance a_Instance );
        #endregion

        #region Public Properties
        public static SelfDiscovery Instance { get { return Singleton<SelfDiscovery>.Instance; } }
        public OnInstance OnDiscovered { get; set; }                            // callback invoked from the non-main thread
        public List<SelfInstance> Discovered { get { return m_Discovered; } }       // the user should lock this list before accessing
        #endregion

        #region Destructor to Stop Discovery
        ~SelfDiscovery() 
        {
            StopDiscovery();
        }

        public void OnApplicationQuit()
        {
            StopDiscovery();
        }
        #endregion

        #region Public Functions
        public void StartDiscovery()
        {
            Log.Debug("SelfDiscovery", "Discovery started with multicast address: {0} and port {1}", m_MulticastAddress, m_Port);
            m_Discovered.Clear();
            m_NumberOfInstances = 0;
            bool successOnSocketBind = false;

            if (m_UdpClient == null)
            {
                IPAddress multicastAddr = IPAddress.Parse( m_MulticastAddress );
                m_UdpClient = new UdpClient();
                m_UdpClient.ExclusiveAddressUse = true;
               
                //TODO: Fix it to make it ultimate 
                try
                {
                    m_UdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                }
                catch (Exception e)
                {
                    Log.Error("SelfDiscovery", "Exception UDP client settint reuse address. Message: {0}, StackTrace: {1}", e.Message, e.StackTrace);
                }

                try
                {
                    m_UdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                    m_UdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, m_Port));
                    m_UdpClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastAddr, IPAddress.Any));
                    m_UdpClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, m_IPMulticastTimeToLive);
                    successOnSocketBind = true;
                }
                catch (Exception e)
                {
                    Log.Error("SelfDiscovery", "Exception UDP client setting. Message: {0}, StackTrace: {1}", e.Message, e.StackTrace);
                }

                if (successOnSocketBind)
                {
                    if (m_ReceiveThread != null && m_ReceiveThread.IsAlive)
                        m_ReceiveThread.Abort();

                    m_ReceiveThread = new Thread(() => ReceiveThread());
                    m_ReceiveThread.IsBackground = true;
                    m_ReceiveThread.Start();

                    m_AsyncDiscoveredID = Runnable.Run(AsyncOnDiscovered());
                }
            }
            else
            {
                successOnSocketBind = true;
            }

            if (successOnSocketBind)
            {
                Log.Debug("SelfDiscovery", "Broadcasting Ping");
                Dictionary<string, object> message = new Dictionary<string, object>();
                message["action"] = "ping";
                byte[] packet = Encoding.UTF8.GetBytes(Json.Serialize(message));
                m_UdpClient.EnableBroadcast = true;
                m_UdpClient.Send(packet, packet.Length, new IPEndPoint(IPAddress.Broadcast, m_Port));
            }
            else
            {
                StopDiscovery();
            }

        }

        public void StopDiscovery()
        {
            if (m_AsyncDiscoveredID >= 0)
            {
                Log.Debug( "SelfDiscovery", "Stopping discover co-routine" );
                Runnable.Stop(m_AsyncDiscoveredID);
                m_AsyncDiscoveredID = -1;
            }

            if ( m_ReceiveThread != null && m_ReceiveThread.IsAlive)
            {
                Log.Debug( "SelfDiscovery", "Stopping Receive thread" );
                m_ReceiveThread.Abort();
            }

            if (m_UdpClient != null)
            {
                Log.Debug( "SelfDiscovery", "Stopping UDP Client" );
                m_UdpClient.Close();
                m_UdpClient = null;
            }
        }
        #endregion

        #region Private Functions

        private IEnumerator AsyncOnDiscovered()
        {
            while (m_UdpClient != null)
            {
                if (m_NumberOfInstances != m_Discovered.Count)
                {
                    for (int i = m_NumberOfInstances; i < m_Discovered.Count; i++)
                    {
                        if (OnDiscovered != null)
                            OnDiscovered(m_Discovered[i]);

                        m_NumberOfInstances++;
                    }
                }
                yield return null;
            }
            yield break;
           
        }

        private void ReceiveThread()
        {
            Log.Debug("SelfDiscovery", "Started listening UDP broadcast to port {0}", m_Port);

            while (m_UdpClient != null)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, m_Port);
                byte[] data = m_UdpClient.Receive(ref remoteEP);
                if ( data.Length > 0 )
                {
                    IDictionary json = Json.Deserialize( Encoding.UTF8.GetString( data ) ) as IDictionary;
                    if (json != null)
                    {
                        string action = json["action"] as string;
                        if (action == "pong")
                        {
                            SelfInstance instance = new SelfInstance();
                            instance.Name = json["name"] as string;
                            instance.Type = json["type"] as string;
                            instance.MacId = json["macId"] as string;
                            instance.IPv4 = remoteEP.Address.ToString();
                            instance.EmbodimentId = json["embodimentId"] as string;
                            instance.InstanceId = json["instanceId"] as string;
                            instance.GroupId = json["groupId"] as string;
                            instance.OrgId = json["orgId"] as string;
                            if (json.Contains("port"))
                            {
                                int portNumber = 0;
                                if (int.TryParse(json["port"].ToString(), out portNumber) && portNumber != 0)
                                {
                                    instance.Port = portNumber;
                                }
                                else
                                {
                                    Log.Error("SelfDiscovery", "Port value couldn't be cast {0} Using default port {1}", json["port"].ToString(), m_DefaultListeningPort);
                                    instance.Port = m_DefaultListeningPort;
                                }
                            }
                            else
                            {
                                Log.Error("SelfDiscovery", "Port needs to supported to be connect. Using default port {0}", m_DefaultListeningPort);
                                instance.Port = m_DefaultListeningPort;
                            }

                            instance.LastPing = DateTime.Now;

                            Log.Debug("SelfDiscovery", "Received Pong message from Intu : {0} from IP: {1}", instance,remoteEP.ToString());

                            lock (m_Discovered)
                            {
                                if(!m_Discovered.Exists( e => e.InstanceId == instance.InstanceId))
                                {
                                    m_Discovered.Add(instance);
                                }
                            }
                        }
                        else
                        {
                            Log.Debug("SelfDiscovery", "Received JSON data but not pong action. Action is : {0}", action);
                        }
                    }
                    else
                    {
                        Log.Error("SelfDiscovery", "Received some data but not in JSON format so ignoring it. Message: \n{0}",  Encoding.UTF8.GetString( data ));
                    }
                }
            }

            Log.Debug("SelfDiscovery", "Finished listening ");
        }
        #endregion
    }
}
