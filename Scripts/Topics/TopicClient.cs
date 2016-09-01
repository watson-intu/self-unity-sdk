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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using MiniJSON;
using WebSocketSharp;

namespace IBM.Watson.Self
{

    public class TopicClient
    {
        #region Public Types
        public enum ClientState {
            Inactive,
            Connecting,
            Connected,
            Disconnecting,
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
        public delegate void OnQueryResponse(QueryInfo a_Info);
        public delegate void OnConnected( TopicClient a_Client );
        public delegate void OnDisconnected( TopicClient a_Client );
        public delegate void MessageHandler( IDictionary a_Message );
        #endregion

        #region Private Types
        private class Subscription
        {
            public Subscription()
            { }
            public Subscription( string a_Path, OnPayload a_Callback )
            {
                m_Path = a_Path;
                m_Callback = a_Callback;
            }

            public string m_Path;
            public OnPayload m_Callback;
        };
        #endregion

        #region Private Data
        Uri             m_Host = null;
        string          m_GroupId = null;
        WebSocket       m_Socket = null;
        ClientState     m_eState = ClientState.Inactive;
        List<object>    m_SendQueue = new List<object>();
        uint            m_ReqId = 1;
        Dictionary<uint,OnQueryResponse> 
                        m_QueryRequestMap = new Dictionary<uint, OnQueryResponse>();
        Dictionary<string,MessageHandler>
                        m_MessageHandlers = new Dictionary<string, MessageHandler>();
        Dictionary<string, List<Subscription> >
                        m_SubscriptionMap = new Dictionary<string, List<Subscription>>();
        #endregion

        #region Public Interface
        public static TopicClient Instance { get { return Singleton<TopicClient>.Instance; } }

        public ClientState State { get { return m_eState; } }

        TopicClient()
        {
            m_MessageHandlers["publish"] = HandlePublish;
            m_MessageHandlers["subscribe_failed"] = HandleSubFailed;
            m_MessageHandlers["no_route"] = HandleNoRoute;
            m_MessageHandlers["query"] = HandleQuery;
            m_MessageHandlers["query_response"] = HandleQueryResponse;
        }

        bool Connect( string a_Host,
            string a_GroupId,
            OnConnected a_OnConnected = null,
            OnDisconnected a_OnDisconnected = null )
        {
            if (! a_Host.StartsWith( "ws://", StringComparison.CurrentCultureIgnoreCase )
                && a_Host.StartsWith( "wss://", StringComparison.CurrentCultureIgnoreCase ) )
            {
                Log.Error( "TopicClient", "Host doesn't begin with ws:// or wss://" );
                return false;
            }

            m_Host = new Uri( a_Host );
            m_eState = ClientState.Connecting;
            m_GroupId = a_GroupId;

            m_Socket = new WebSocket( new Uri( m_Host, "/stream").AbsoluteUri );
            m_Socket.Headers = new Dictionary<string, string>();
            m_Socket.Headers.Add("groupId", a_GroupId );
            m_Socket.Headers.Add("selfId", Utility.MacAddress );

            m_Socket.OnMessage += OnSocketMessage;
            m_Socket.OnOpen += OnSocketOpen;
            m_Socket.OnError += OnSocketError;
            m_Socket.OnClose += OnSocketClosed;

            m_Socket.ConnectAsync();
            return true;
        }

        void Disconnect()
        {
            if ( m_Socket != null )
            {
                m_eState = ClientState.Disconnecting;
                m_Socket.CloseAsync();
            }
        }

        //! Publish data for a remote target specified by the provided path.
        void Publish(
            string a_Path,
            string a_Data,
            bool a_bPersisted = false)
        {
            if ( m_Socket == null )
                throw new WatsonException( "Query called before calling Connect." );

            Dictionary<string,object> publish = new Dictionary<string, object>();
            publish["targets"] = new string[] { a_Path };
            publish["origin"] = ".";
            publish["msg"] = "publish_at";
            publish["data"] = a_Data;
            publish["binary"] = false;
            publish["persisted"] = a_bPersisted;

            SendMessage( publish );
        }

        //! Publish binary data to the remote target by the specified path.
        void Publish(
            string a_Path,
            byte [] a_Data,
            bool a_bPersisted = false )
        {
            if ( m_Socket == null )
                throw new WatsonException( "Query called before calling Connect." );

            Dictionary<string,object> publish = new Dictionary<string, object>();
            publish["targets"] = new string[] { a_Path };
            publish["origin"] = ".";
            publish["msg"] = "publish_at";
            publish["data"] = a_Data;
            publish["binary"] = true;
            publish["persisted"] = a_bPersisted;

            SendMessage( publish );
        }

        //! This queries a node specified by the given path.
        void Query(string a_Path,               //! the path to the node, we will invoke the callback with a QueryInfo structure
            OnQueryResponse a_Callback)
        {
            if ( m_Socket == null )
                throw new WatsonException( "Query called before calling Connect." );

            uint reqId = m_ReqId++;
            m_QueryRequestMap[ reqId ] = a_Callback;

            Dictionary<string, object> query = new Dictionary<string, object>();
            query["targets"] = new string[] { a_Path };
            query["origin"] = ".";
            query["msg"] = "query";
            query["request"] = reqId;

            SendMessage( query );
        }

        //! Subscribe to the given topic specified by the provided path.
        void Subscribe( string a_Path,      //! The topic to subscribe, ".." moves up to a parent self
            OnPayload a_Callback)
        {
            if ( m_Socket == null )
                throw new WatsonException( "Query called before calling Connect." );

            if ( m_SubscriptionMap.ContainsKey( a_Path ) )
                m_SubscriptionMap[ a_Path ] = new List<Subscription>();
            m_SubscriptionMap[ a_Path ].Add( new Subscription( a_Path, a_Callback ) );

            Dictionary<string,object> sub = new Dictionary<string, object>();
            sub["targets"] = new string[] { a_Path };
            sub["origin"] = ".";
            sub["msg"] = "subscribe";

            SendMessage( sub );
        }

        //! Unsubscribe from the given topic
        bool Unsubscribe( string a_Path,
            OnPayload a_Callback = null)
        {
            List<Subscription> subs = null;
            if (! m_SubscriptionMap.TryGetValue( a_Path, out subs ) )
                return false;

            bool bSuccess = false;
            for(int i=0;i<subs.Count;)
            {
                if ( a_Callback == null || subs[i].m_Callback == a_Callback )
                {
                    subs.RemoveAt( i );
                    bSuccess = true;
                }
                else
                    i += 1;
            }

            if ( subs.Count == 0 )
            {
                m_SubscriptionMap.Remove( a_Path );

                Dictionary<string,object> unsub = new Dictionary<string, object>();
                unsub["targets"] = new string[] { a_Path };
                unsub["origin"] = ".";
                unsub["msg"] = "unsubscribe";

                SendMessage( unsub );
            }

            return bSuccess;
        }

        //! Helper function for appending a topic onto a origin
        public static string GetPath(string a_Origin, string a_Topic)
        {
	        string sPath;
	        int nLastDot = a_Origin.LastIndexOf( "/." );
	        if (nLastDot > 0 )
		        sPath = a_Origin.Substring( 0, nLastDot ) + a_Topic;
	        else
		        sPath = a_Topic;

	        return sPath;
        }
        #endregion

        #region WebSocket Callbacks
        public void OnSocketMessage(object sender, MessageEventArgs message)
        {
            IDictionary json = null;
            if ( message.IsBinary )
            {
                // the first part up to the first /0 character will be the json..
                byte [] data = message.RawData;

                int headerSize = 0;
                while( data[headerSize] != 0 )
                    headerSize += 1;

                byte [] headerData = new byte[ headerSize - 1 ];
                Buffer.BlockCopy( data, 0, headerData, 0, headerSize - 1 );
                byte [] payload = new byte[ data.Length - headerSize ];
                Buffer.BlockCopy( data, headerSize + 1, payload, 0, payload.Length );

                json = Json.Deserialize( Encoding.UTF8.GetString(headerData) ) as IDictionary;
                json["data"] = payload;
            }
            else if ( message.IsText )
                json = Json.Deserialize( message.Data ) as IDictionary;

            if (json.Contains("control"))
            {
                string control = json["control"] as string;
                if (control == "authenticate")
                {
                    string groupId = json["groupId"] as string;
                    string selfId = json["selfId"] as string;

                    Log.Status("TopicClient", "Received authenicate control, groupId: {0}, selfId: {1}", groupId, selfId);
                    // TODO actually authenticate the other end?
                }

            }
            else if (json.Contains("msg"))
            {
                string msg = json["msg"] as string;

                MessageHandler handler = null;
                if (m_MessageHandlers.TryGetValue(msg, out handler))
                    handler(json);
                else
                    Log.Debug("TopicClient", "Received unhandled message {0}", msg);
            }
            else
                Log.Error("TopicClient", "Unknown message type received: {0}", Json.Serialize(json));
        }

        public void OnSocketOpen(object sender, EventArgs e)
        {
            Log.Status("TopicClient", "Connected to {0}", m_Host );
            m_eState = ClientState.Connected;

            for(int i=0;i<m_SendQueue.Count;++i)
            {
                object send = m_SendQueue[i];
                if ( send is byte[] )
                    m_Socket.Send( send as byte[] );
                else if ( send is string )
                    m_Socket.Send( send as string );
            }
            m_SendQueue.Clear();
        }

        public void OnSocketError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Log.Error( "TopicClient", "WebSocket error: {0}", e.ToString() );
        }
        public void OnSocketClosed(object sender, CloseEventArgs e)
        {
            Log.Status("TopicClient", "Socket closed." );
            m_eState = ClientState.Disconnected;
            m_Socket = null;
        }
        #endregion

        #region Private Functions
        void SendMessage( Dictionary<string,object> a_json )
        {
            if ( a_json.ContainsKey( "binary" ) && ((bool)a_json["binary"]) != false )
            {
                byte [] data = a_json["data"] as byte [];
                a_json["data"] = data.Length;

                byte [] header = Encoding.UTF8.GetBytes( Json.Serialize( a_json ) );
                byte [] frame = new byte [ header.Length + data.Length + 1 ];

                Buffer.BlockCopy( header, 0, frame, 0, header.Length );
                Buffer.BlockCopy( data, 0, frame, header.Length + 1, data.Length );

                if ( m_eState == ClientState.Connected )
                    m_Socket.Send(frame);
                else
                    m_SendQueue.Add(frame);
            }
            else
            {
                string send = Json.Serialize( a_json );
                if ( m_eState == ClientState.Connected )
                    m_Socket.Send( send );
                else
                    m_SendQueue.Add( send );
            }
        }
        #endregion

        #region Message Handlers
        void HandlePublish(IDictionary a_Message)
        {
            
        }
        void HandleSubFailed(IDictionary a_Message)
        {
        }

        void HandleNoRoute(IDictionary a_Message)
        {

        }
        void HandleQuery(IDictionary a_Message)
        {
        }

        void HandleQueryResponse(IDictionary a_Message)
        {
        }
        #endregion
    }

}
