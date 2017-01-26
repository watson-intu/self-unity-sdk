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

using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.Self.Topics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
        private UdpClient m_Client = null;
        private Thread m_ReceiveThread = null;
        private List<SelfInstance> m_Discovered = new List<SelfInstance>();
        #endregion

        #region Public Types
        public class SelfInstance
        {
            #region Public Properties
            public string Name { get; set; }
            public string Type { get; set; }
            public string MacId { get; set; }
            public string InstanceId { get; set; }
            public string GroupId { get; set; }
            public string OrgId { get; set; }
            #endregion
        }
        public delegate void OnSelfInstance( SelfInstance a_Instance );
        #endregion

        #region Public Properties
        public OnSelfInstance OnUpdated { get; set; }
        public OnSelfInstance OnAdded { get; set; }
        public OnSelfInstance OnRemoved { get; set; }
        public List<SelfInstance> Discovered { get { return m_Discovered; } }
        #endregion

        #region Public Functions
        public void StartDiscovery()
        {
            if( m_Client == null )
            {
                m_Client = new UdpClient( m_Port );
                m_Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );
                m_Client.Client.Bind( new IPEndPoint( IPAddress.Any, m_Port) );
                m_Client.JoinMulticastGroup( IPAddress.Parse( m_MulticastAddress ) );
            }
        }

        #endregion
    }
}
