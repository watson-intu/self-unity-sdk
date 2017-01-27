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
        private Socket m_Socket = null;
        private Thread m_ReceiveThread = null;
        private List<Instance> m_Discovered = new List<Instance>();
        #endregion

        #region Public Types
        public class Instance
        {
            #region Public Properties
            public string Name { get; set; }
            public string Type { get; set; }
            public string MacId { get; set; }
            public string EmbodimentId { get; set; }
            public string InstanceId { get; set; }
            public string GroupId { get; set; }
            public string OrgId { get; set; }
            public DateTime LastPing { get; set; }
            #endregion

            public override string ToString()
            {
                return string.Format("[Instance: Name={0}, Type={1}, MacId={2}, EmbodimentId={3}, InstanceId={4}, GroupId={5}, OrgId={6}, LastPing={7}]",
                    Name, Type, MacId, EmbodimentId, InstanceId, GroupId, OrgId, LastPing );
            }
        }
        public delegate void OnInstance( Instance a_Instance );
        #endregion

        #region Public Properties
        public OnInstance OnDiscovered { get; set; }                            // callback invoked from the non-main thread
        public List<Instance> Discovered { get { return m_Discovered; } }       // the user should lock this list before accessing
        #endregion

        ~SelfDiscovery() 
        {
            Log.Debug("SelfDiscovery", "Destructor entered");
            if (m_ReceiveThread != null && m_ReceiveThread.IsAlive)
            {
                Log.Debug("SelfDiscovery", "Aborting receive thread");
                m_ReceiveThread.Abort();
            }
        }

        public void OnApplicationQuit()
        {
            if (m_ReceiveThread != null && m_ReceiveThread.IsAlive)
            {
                Log.Debug("SelfDiscovery", "Aborting receive thread");
                m_ReceiveThread.Abort();
            }
        }

        #region Public Functions
        public void StartDiscovery()
        {
            Log.Debug("SelfDiscovery", "Discovery started with address: {0} and port {1}", m_MulticastAddress, m_Port);
            IPAddress multicastAddr = IPAddress.Parse( m_MulticastAddress );
            IPEndPoint broadcastEnd = new IPEndPoint( multicastAddr, m_Port );

            m_Discovered.Clear();
            if( m_Socket == null )
            {
                m_Socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
                m_Socket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress, true );
                m_Socket.Bind( new IPEndPoint( IPAddress.Any, m_Port) );
                m_Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastAddr,IPAddress.Any));
                m_Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);

                Log.Debug("SelfDiscovery", "Started listening UDP started with address: {0} and port {1}", m_MulticastAddress, m_Port);

                if (m_ReceiveThread != null && m_ReceiveThread.IsAlive)
                    m_ReceiveThread.Abort();
                
                m_ReceiveThread = new Thread( () => ReceiveThread() );
                m_ReceiveThread.IsBackground = true;
                m_ReceiveThread.Start();
            }

            Dictionary<string,object> message = new Dictionary<string, object>();
            message["action"] = "ping";

            Log.Debug("SelfDiscovery", "Sending Action as Ping as broadcast");
            byte [] packet = Encoding.UTF8.GetBytes( Json.Serialize( message ) );
            m_Socket.SendTo( packet, broadcastEnd );
        }
        public void StopDiscovery()
        {
            Log.Debug( "TestSelfDiscovery", "Stopping Receive thread" );
            if ( m_ReceiveThread != null )
            {
                m_ReceiveThread.Abort();
                m_ReceiveThread = null;
            }
            Log.Debug( "TestSelfDiscovery", "Stopping Socket Connection" );
            if ( m_Socket != null )
            {
                m_Socket.Close();
                m_Socket = null;
            }

            Log.Debug( "TestSelfDiscovery", "Stopped Discovery" );
        }
        #endregion

        #region Private Functions
        private void ReceiveThread()
        {
            Log.Debug("SelfDiscovery", "Started listening in thread");

            while( m_Socket != null )
            {
                IPEndPoint remoteEP = new IPEndPoint( IPAddress.Any, 0 );
                byte [] data = new byte[ 1024 ];
                m_Socket.Receive( data );

                if ( data.Length > 0 )
                {
                    IDictionary json = Json.Deserialize( Encoding.UTF8.GetString( data ) ) as IDictionary;
                    if (json != null)
                    {
                        string action = json["action"] as string;
                        if (action == "pong")
                        {
                            Instance instance = new Instance();
                            instance.Name = json["name"] as string;
                            instance.Type = json["type"] as string;
                            instance.MacId = json["macId"] as string;
                            instance.EmbodimentId = json["embodimentId"] as string;
                            instance.InstanceId = json["instanceId"] as string;
                            instance.GroupId = json["groupId"] as string;
                            instance.OrgId = json["orgId"] as string;
                            instance.LastPing = DateTime.Now;

                            Log.Debug("SelfDiscovery", "Received pong message from Intu : {0}", instance);

                            lock (m_Discovered)
                                m_Discovered.Add(instance);
                            if (OnDiscovered != null)
                                OnDiscovered(instance);
                        }
                        else
                        {
                            Log.Debug("SelfDiscovery", "Received JSON data but not pong action. Action is : {0}", action);
                        }
                    }
                    else
                    {
                        Log.Debug("SelfDiscovery", "Received some data but not in JSON format so ignoring it.");
                    }
                }
            }

            Log.Debug("SelfDiscovery", "Finished listening ");
        }
        #endregion
    }
}
