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

namespace IBM.Watson.Self.Topics
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
            public bool Subscribed { get; set; }       // true if subscribing, false if un-subscribing
            public string Origin { get; set; }         // who is the subscriber
            public string Topic { get; set; }          // topic they are subscribing too
        };
        public delegate void OnSubscriber(SubInfo a_Info);

        public class Payload
        {
            public string Topic { get; set; }     // the topic of this payload
            public string Origin { get; set; }        // who sent this payload
            public byte [] Data { get; set; }          // the payload data
            public string Type { get; set; }          // the type of data
            public bool Persisted { get; set; }   // true if this was a persisted payload
            public string RemoteOrigin { get; set; }  // this is set to the origin that published this payload 
        };
        public delegate void OnPayload(Payload a_Payload);

        public class TopicInfo
        {
            public string TopicId { get; set; }       // the ID of this topic
            public string Type { get; set; }          // type of topic
        };

        public class QueryInfo
        {
            public bool bSuccess { get; set; }
            public string Path { get; set; }
            public string GroupId { get; set; }
            public string SelfId { get; set; }
            public string ParentId { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Version { get; set; }
            public string[] Children { get; set; }
            public TopicInfo[] Topics { get; set; }
        };
        public delegate void OnQueryResponse(QueryInfo a_Info);
        public delegate void OnConnected();
        public delegate void OnDisconnected();
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
        string          m_SelfId = null;
        WebSocket       m_Socket = null;
        OnConnected     m_OnConnected = null;
        OnDisconnected  m_OnDisconnected = null;
        ClientState     m_eState = ClientState.Inactive;
        List<object>    m_SendQueue = new List<object>();
        int             m_ReqId = 1;
        Dictionary<int,OnQueryResponse> 
                        m_QueryRequestMap = new Dictionary<int, OnQueryResponse>();
        Dictionary<string,MessageHandler>
                        m_MessageHandlers = new Dictionary<string, MessageHandler>();
        Dictionary<string, List<Subscription> >
                        m_SubscriptionMap = new Dictionary<string, List<Subscription>>();
        #endregion

        #region Public Interface
        public static TopicClient Instance { get { return Singleton<TopicClient>.Instance; } }

        public ClientState State { get { return m_eState; } }

        public TopicClient()
        {
            m_MessageHandlers["publish"] = HandlePublish;
            m_MessageHandlers["subscribe_failed"] = HandleSubFailed;
            m_MessageHandlers["no_route"] = HandleNoRoute;
            m_MessageHandlers["query"] = HandleQuery;
            m_MessageHandlers["query_response"] = HandleQueryResponse;
        }

        public bool Connect( string a_Host,
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
            m_SelfId = Utility.MacAddress;
            m_OnConnected = a_OnConnected;
            m_OnDisconnected = a_OnDisconnected;

            m_Socket = new WebSocket( new Uri( m_Host, "/stream").AbsoluteUri );
            m_Socket.Headers = new Dictionary<string, string>();
            m_Socket.Headers.Add("groupId", a_GroupId );
            m_Socket.Headers.Add("selfId", m_SelfId );

            m_Socket.OnMessage += OnSocketMessage;
            m_Socket.OnOpen += OnSocketOpen;
            m_Socket.OnError += OnSocketError;
            m_Socket.OnClose += OnSocketClosed;

            m_Socket.ConnectAsync();
            return true;
        }

        public void Disconnect()
        {
            if ( m_Socket != null )
            {
                m_eState = ClientState.Disconnecting;
                m_Socket.CloseAsync();
            }
        }

        //! Publish data for a remote target specified by the provided path.
        public void Publish(
            string a_Path,
            string a_Data,
            bool a_bPersisted = false)
        {
            if ( m_Socket == null )
                throw new WatsonException( "Query called before calling Connect." );

            Dictionary<string,object> publish = new Dictionary<string, object>();
            publish["targets"] = new string[] { a_Path };
            publish["origin"] = m_SelfId + "/.";
            publish["msg"] = "publish_at";
            publish["data"] = a_Data;
            publish["binary"] = false;
            publish["persisted"] = a_bPersisted;

            SendMessage( publish );
        }

        //! Publish binary data to the remote target by the specified path.
        public void Publish(
            string a_Path,
            byte [] a_Data,
            bool a_bPersisted = false )
        {
            if ( m_Socket == null )
                throw new WatsonException( "Query called before calling Connect." );

            Dictionary<string,object> publish = new Dictionary<string, object>();
            publish["targets"] = new string[] { a_Path };
            publish["origin"] = m_SelfId + "/.";
            publish["msg"] = "publish_at";
            publish["data"] = a_Data;
            publish["binary"] = true;
            publish["persisted"] = a_bPersisted;

            SendMessage( publish );
        }

        //! This queries a node specified by the given path.
        public void Query(string a_Path,               //! the path to the node, we will invoke the callback with a QueryInfo structure
            OnQueryResponse a_Callback)
        {
            if ( m_Socket == null )
                throw new WatsonException( "Query called before calling Connect." );

            int reqId = m_ReqId++;
            m_QueryRequestMap[ reqId ] = a_Callback;

            Dictionary<string, object> query = new Dictionary<string, object>();
            query["targets"] = new string[] { a_Path };
            query["origin"] = m_SelfId + "/.";
            query["msg"] = "query";
            query["request"] = reqId;

            SendMessage( query );
        }

        //! Subscribe to the given topic specified by the provided path.
        public void Subscribe( string a_Path,      //! The topic to subscribe, ".." moves up to a parent self
            OnPayload a_Callback)
        {
            if ( m_Socket == null )
                throw new WatsonException( "Query called before calling Connect." );

            if (! m_SubscriptionMap.ContainsKey( a_Path ) )
                m_SubscriptionMap[ a_Path ] = new List<Subscription>();
            m_SubscriptionMap[ a_Path ].Add( new Subscription( a_Path, a_Callback ) );

            Dictionary<string,object> sub = new Dictionary<string, object>();
            sub["targets"] = new string[] { a_Path };
            sub["origin"] = m_SelfId + "/.";
            sub["msg"] = "subscribe";

            SendMessage( sub );
        }

        //! Unsubscribe from the given topic
        public bool Unsubscribe( string a_Path,
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
                unsub["origin"] = m_SelfId + "/.";
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
		        sPath = a_Origin.Substring( 0, nLastDot + 1 ) + a_Topic;
	        else
		        sPath = a_Topic;

	        return sPath;
        }
        #endregion

        #region WebSocket Callbacks
        void OnSocketMessage(object sender, MessageEventArgs message)
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
                byte [] payload = new byte[ data.Length - headerSize - 1 ];
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

        void OnSocketOpen(object sender, EventArgs e)
        {
            Log.Status("TopicClient", "Connected to {0}", m_Host );
            m_eState = ClientState.Connected;

            if ( m_OnConnected != null )
                m_OnConnected();

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

        void OnSocketError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Log.Error( "TopicClient", "WebSocket error: {0}", e.Message );
        }
        void OnSocketClosed(object sender, CloseEventArgs e)
        {
            Log.Status("TopicClient", "Socket closed." );
            m_eState = ClientState.Disconnected;

            if ( m_OnDisconnected != null )
                m_OnDisconnected();

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
            Log.Debug( "TopicClient", "HandlePublish()" );   

            string path = OriginToPath( GetPath( (string)a_Message["origin"], (string)a_Message["topic"] ) );
            List<Subscription> subs = null;
            if ( m_SubscriptionMap.TryGetValue( path, out subs ) )
            {
                Payload payload = new Payload();
                payload.Origin = path;
                payload.Topic = (string)a_Message["topic"];
                if ( a_Message.Contains( "remote_origin" ) )
                    payload.RemoteOrigin = (string)a_Message["remote_origin"];

                if ( a_Message["data"] is string )
                    payload.Data = Encoding.UTF8.GetBytes( (string)a_Message["data"] );
                else
                    payload.Data = (byte [])a_Message["data"];
                payload.Type = (string)a_Message["type"];

                for(int i=0;i<subs.Count;++i)
                    subs[i].m_Callback( payload );
            }
        }
        void HandleSubFailed(IDictionary a_Message)
        {
            Log.Debug( "TopicClient", "HandleSubFailed()" );   
        }

        void HandleNoRoute(IDictionary a_Message)
        {
            Log.Debug( "TopicClient", "HandleNoRoute()" );   
        }
        void HandleQuery(IDictionary a_Message)
        {
            Log.Debug( "TopicClient", "HandleQuery()" );   
        }

        //! We are a client, not a manager, so all our origin's should come through our connection which we consider our parent
        string OriginToPath( string a_Origin )
        {
            if ( a_Origin.StartsWith( "../" ) )
                return a_Origin.Substring( 3 );     // remove the ../ from the origin...

            Log.Warning( "TopicClient", "Unexpected origin {0}", a_Origin );
            return a_Origin;
        }

        void HandleQueryResponse(IDictionary a_Message)
        {
            Log.Debug( "TopicClient", "HandleQueryResponse()" );   

            QueryInfo info = new QueryInfo();
            info.bSuccess = true;
            info.Path = OriginToPath( (string)a_Message["origin"] );
            info.SelfId = (string)a_Message["selfId"];
            info.GroupId = (string)a_Message["groupId"];
            if ( a_Message.Contains( "name" ) )
            {
                info.Name = (string)a_Message["name"];
                info.Type = (string)a_Message["type"];
                info.Version = (string)a_Message["version"];
            }
            if ( a_Message.Contains( "parentId" ) )
                info.ParentId = (string)a_Message["parentId"];

            if ( a_Message.Contains( "children" ) )
            {
                IList children = (IList)a_Message["children"];
                info.Children = new string[ children.Count ];
                for(int i=0;i<children.Count;++i)
                    info.Children[i] = (string)children[i];
            }

            if ( a_Message.Contains( "topics" ) )
            {
                IList topics = (IList)a_Message["topics"];
                info.Topics = new TopicInfo[ topics.Count ];
                for(int i=0;i<topics.Count;++i)
                {
                    IDictionary topic = (IDictionary)topics[i];

                    TopicInfo ti = new TopicInfo();
                    ti.TopicId = (string)topic["topicId"];
                    ti.Type = (string)topic["type"];
                    info.Topics[i] = ti;
                }
            }

            int reqId = int.Parse( (string)a_Message["request"] ); 
            OnQueryResponse callback = null;
            if ( m_QueryRequestMap.TryGetValue( reqId, out callback ) )
                callback( info );
        }
        #endregion
    }

}
