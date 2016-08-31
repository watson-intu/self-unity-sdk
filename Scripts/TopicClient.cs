/**
* Copyright 2016 IBM Corp. All Rights Reserved.
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

using System;
using IBM.Watson.DeveloperCloud.Utilities;
using WebSocketSharp;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Logging;

namespace IBM.Watson.Self
{

    public class TopicClient
    {
        #region Public Types
        public enum ClientState {
            Inactive,
            Connecting,
            Connected,
            Disconnected
        };
        public struct SubInfo
        {
            public bool m_Subscribed;       // true if subscribing, false if un-subscribing
            public string m_Origin;         // who is the subscriber
            public string m_Topic;          // topic they are subscribing too
        };
        public delegate void OnSubscriber(SubInfo a_Info);

        public struct Payload
        {
            string m_Topic;     // the topic of this payload
            string m_Origin;        // who sent this payload
            byte [] m_Data;          // the payload data
            string m_Type;          // the type of data
            bool m_Persisted;   // true if this was a persisted payload
            string m_RemoteOrigin;  // this is set to the origin that published this payload 
        };
        public delegate void OnPayload(Payload a_Payload);

        public struct TopicInfo
        {
            string m_TopicId;       // the ID of this topic
            string m_Type;          // type of topic
        };

        public struct QueryInfo
        {
            bool m_bSuccess;
            string m_Path;
            string m_GroupId;
            string m_SelfId;
            string m_ParentId;
            string m_Name;
            string m_Type;
            string[] m_Children;
            TopicInfo[] m_Topics;
        };
        public delegate void OnQuery(QueryInfo a_Info);
        public delegate void OnConnected( TopicClient a_Client );
        public delegate void OnDisconnected( TopicClient a_Client );
        #endregion

        #region Private Data
        string m_Host = null;
        string m_GroupId = null;
        WebSocket m_Socket = null;
        ClientState m_eState = ClientState.Inactive;
        #endregion

        #region Public Interface
        public static TopicClient Instance { get { return Singleton<TopicClient>.Instance; } }

        public ClientState State { get { return m_eState; } }

        void Connect( string a_Host,
            string a_GroupId,
            OnConnected a_OnConnected = null,
            OnDisconnected a_OnDisconnected = null )
        {
            if ( m_Socket != null )
                throw new WatsonException( "Connect has already been called." );

            m_eState = ClientState.Connecting;
            m_Host = a_Host;
            m_GroupId = a_GroupId;

            m_Socket = new WebSocket(a_Host);
            m_Socket.Headers = new Dictionary<string, string>();
            m_Socket.Headers.Add("groupId", a_GroupId );
            m_Socket.Headers.Add("selfId", Utility.MacAddress );

            m_Socket.OnMessage += OnSocketMessage;
            m_Socket.OnOpen += OnSocketOpen;
            m_Socket.OnError += OnSocketError;
            m_Socket.OnClose += OnSocketClosed;

            m_Socket.ConnectAsync();
        }

        void Disconnect()
        {
        }

        //! Publish data for a remote target specified by the provided path.
        bool Publish(
            string a_Path,
            string a_Data,
            bool a_bPersisted = false,
            bool a_bBinary = false)
        {
            return true;
        }

        //! This queries a node specified by the given path.
        void Query(string a_Path,               //! the path to the node, we will invoke the callback with a QueryInfo structure
            OnQuery a_Callback)
        {
        }

        //! Subscribe to the given topic specified by the provided path.
        void Subscribe( string a_Path,      //! The topic to subscribe, ".." moves up to a parent self
            OnPayload a_Callback)
        {
        }

        //! Unsubscribe from the given topic
        bool Unsubscribe( string a_Path,
            object a_pObject = null)
        {
            return true;
        }

        //! Helper function for appending a topic onto a origin
        public static string GetPath(string a_Origin, string a_Topic)
        {
            return "";
        }
        #endregion

        #region WebSocket Callbacks
        public void OnSocketMessage(object sender, MessageEventArgs messageEventArgs)
        {
           // SelfMessage response = Utility.DeserializeResponse<SelfMessage>(messageEventArgs.RawData, null);

        }

        public void OnSocketOpen(object sender, EventArgs e)
        {
            Log.Status("TopicClient", "Connected to {0}", m_Host );
            m_eState = ClientState.Connected;
        }

        public void OnSocketError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Log.Error( "TopicClient", "WebSocket error: {0}", e.ToString() );
        }
        public void OnSocketClosed(object sender, CloseEventArgs e)
        {
            Log.Status("TopicClient", "Socket closed." );
            m_eState = ClientState.Disconnected;
        }
        #endregion
    }

}
