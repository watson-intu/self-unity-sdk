
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


using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.Self.Topics;
using System.Collections.Generic;
using MiniJSON;
using System.Text;
using System.Collections;
using IBM.Watson.DeveloperCloud.Logging;
using System;

namespace IBM.Watson.Self.Agents
{
    public class BlackBoard
    {
        #region Public Types
        public delegate void OnThingEvent( ThingEvent a_Event );
        #endregion

        #region Private Data
        private class Subscriber
        {
            public Subscriber( OnThingEvent a_Callback, ThingEventType a_EventMask )
            {
                m_Callback = a_Callback;
                m_EventMask = a_EventMask;
            }

            public OnThingEvent m_Callback;
            public ThingEventType m_EventMask;
        }
        private Dictionary<string,List<Subscriber>>   m_SubscriberMap = new Dictionary<string, List<Subscriber>>();
        private Dictionary<string,IThing> m_ThingMap = new Dictionary<string, IThing>();
        private Dictionary<string,bool> m_Blackboard= new Dictionary<string, bool>();
        private bool m_bDisconnected = false;
        #endregion

        #region Public Interface
        public static BlackBoard Instance { get { return Singleton<BlackBoard>.Instance; } }

        public BlackBoard()
        {
            TopicClient.Instance.StateChangedEvent += OnStateChanged;
        }

        ~BlackBoard()  // destructor to clean-up events listeners
        {
            TopicClient.Instance.StateChangedEvent -= OnStateChanged;

            foreach( var kv in m_Blackboard )
                TopicClient.Instance.Unsubscribe( kv.Key + "blackboard", OnBlackBoardEvent );
        }

        public void SubscribeToType( string a_Type, OnThingEvent a_Callback, ThingEventType a_EventMask = ThingEventType.TE_ALL, string a_Path = "" )
        {
            if (! m_Blackboard.ContainsKey(a_Path) )
            {
                TopicClient.Instance.Subscribe( a_Path + "blackboard", OnBlackBoardEvent );
                m_Blackboard[ a_Path ] = true;
            }

            if (!m_SubscriberMap.ContainsKey(a_Type))
            {
                m_SubscriberMap[a_Type] = new List<Subscriber>();

                Dictionary<string, object> subscribe = new Dictionary<string, object>();
                subscribe["event"] = "subscribe_to_type";
                subscribe["type"] = a_Type;
                subscribe["event_mask"] = (int)ThingEventType.TE_ALL;       // we want all events, we will filter those events on this side

                TopicClient.Instance.Publish( a_Path + "blackboard", Json.Serialize(subscribe));
            }

            m_SubscriberMap[a_Type].Add( new Subscriber( a_Callback, a_EventMask ) );
        }

        public void UnsubscribeFromType( string a_Type, OnThingEvent a_Callback = null, string a_Path = "" )
        {
            if ( m_SubscriberMap.ContainsKey( a_Type ) )
            {
                if ( a_Callback != null )
                {
                    List<Subscriber> subs = m_SubscriberMap[ a_Type ];
                    for(int i=0;i<subs.Count;++i)
                        if ( subs[i].m_Callback == a_Callback )
                        {
                            subs.RemoveAt(i);
                            break;
                        }

                    if ( subs.Count == 0 )
                        m_SubscriberMap.Remove( a_Type );
                }
                else
                    m_SubscriberMap.Remove( a_Type );
            }

            // only remove if this was the last subscriber for the given type..
            if (!m_SubscriberMap.ContainsKey(a_Type))
            {
                Dictionary<string, object> unsubscribe = new Dictionary<string, object>();
                unsubscribe["event"] = "unsubscribe_from_type";
                unsubscribe["type"] = a_Type;

                TopicClient.Instance.Publish( a_Path + "blackboard", Json.Serialize(unsubscribe));
            }
        }

        public void AddThing( IThing a_Thing, string a_Path = "" )
        {
            Dictionary<string,object> add_object = new Dictionary<string, object>();
            add_object["event"] = "add_object";
            add_object["type"] = string.IsNullOrEmpty( a_Thing.DataType ) ? a_Thing.Type : a_Thing.DataType;
            add_object["thing"] = a_Thing.Serialize();
            if (!string.IsNullOrEmpty( a_Thing.ParentGUID ) )
                add_object["parent"] = a_Thing.ParentGUID;

            TopicClient.Instance.Publish( a_Path + "blackboard", Json.Serialize(add_object) );
        }
        #endregion

        #region Event Handlers

        void OnStateChanged(TopicClient.ClientState a_CurrentState)
        {
            switch (a_CurrentState)
            {
                case TopicClient.ClientState.Connected:
                    OnConnected();
                    break;
                case TopicClient.ClientState.Disconnected:
                    OnDisconnected();
                    break;
                default:
                    break;
            }
        }

        void OnConnected()
        {
            if (m_bDisconnected)
            {
                // restore our subscriptions..
                foreach (var kv in m_SubscriberMap)
                {
                    string type = kv.Key;

                    Dictionary<string, object> subscribe = new Dictionary<string, object>();
                    subscribe["event"] = "subscribe_to_type";
                    subscribe["type"] = type;
                    subscribe["event_mask"] = (int)ThingEventType.TE_ALL;       // we want all events, we will filter those events on this side

                    TopicClient.Instance.Publish("blackboard", Json.Serialize(subscribe));
                    Log.Status("BlackBoard", "Subscription to type {0} restored.", type );
                }
                m_bDisconnected = false;
            }
        }
        void OnDisconnected()
        {
            m_bDisconnected = true;
        }

        void OnBlackBoardEvent( TopicClient.Payload a_Payload )
        {
            IDictionary json = a_Payload.ParseJson();

            bool bFailed = false;
            string event_name = json["event"] as string;
            string type = json["type"] as string;

            ThingEvent te = new ThingEvent();
            te.m_EventType = ThingEventType.TE_NONE;
            te.m_Event = json;

            if ( event_name == "add_object" )
            {
                te.m_EventType = ThingEventType.TE_ADDED;

                // TODO: Create correct type based on type name, fall back to just making an IThing object
                te.m_Thing = new IThing();
                try {
                    te.m_Thing.Deserialize( json["thing"] as IDictionary );
                    Log.Debug( "BlackBoard", "Adding object {0}", te.m_Thing.GUID );

                    if ( json.Contains( "parent" ) )
                        te.m_Thing.ParentGUID = json["parent"] as string;
                    m_ThingMap[ te.m_Thing.GUID ] = te.m_Thing;
                }
                catch( Exception e )
                {
                    Log.Error( "BlackBoard", "Failed to deserialize object: {0}, stack: {1}", e.Message, e.StackTrace );
                    bFailed = true;
                }
            }
            else if ( event_name == "remove_object" )
            {
                te.m_EventType = ThingEventType.TE_REMOVED;

                string guid = json["thing_guid"] as string;
                if ( m_ThingMap.TryGetValue( guid, out te.m_Thing ) )
                {
                    Log.Debug( "BlackBoard", "Removing object {0}", guid );
                    m_ThingMap.Remove( guid );
                }
                else
                    Log.Warning( "BlackBoard", "Failed to find object by guid {0}.", guid );
            }
            else if ( event_name == "set_object_state" )
            {
                string guid = json["thing_guid"] as string;
                if ( m_ThingMap.TryGetValue( guid, out te.m_Thing ) )
                {
                    string state = json["state"] as string;
                    Log.Status( "BlackBoard", "Updating object {0} state to {1}", guid, state );
                    te.m_Thing.State = json["state"] as string;
                }
                else
                    Log.Warning( "BlackBoard", "Failed to find object by guid {0}.", guid );
            }
            else if ( event_name == "set_object_importance" )
            {
                string guid = json["thing_guid"] as string;
                if ( m_ThingMap.TryGetValue( guid, out te.m_Thing ) )
                {
                    float fImportance = (float)json["importance"];
                    Log.Status( "BlackBoard", "Updating object {0} importance to {1}", guid, fImportance );
                    te.m_Thing.Importance = fImportance;
                }
                else
                    Log.Warning( "BlackBoard", "Failed to find object by guid {0}.", guid );
            }

            // if we failed, send the message back with a different event
            if ( bFailed )
            {
                json["failed_event"] = event_name;
                json["event"] = "error";

                TopicClient.Instance.Publish( "blackboard", Json.Serialize( json ) );
            }
            else if ( te.m_EventType != ThingEventType.TE_NONE )
            {
                List<Subscriber> subs = null;
                if ( m_SubscriberMap.TryGetValue( type, out subs ) )
                {
                    for(int i=0;i<subs.Count;++i)
                    {
                        Subscriber sub = subs[i];
                        if ( sub.m_Callback == null )
                            continue;
                        if ( (sub.m_EventMask & te.m_EventType) == 0 )
                            continue;

                        sub.m_Callback( te );
                    }
                }
            }
        }
        #endregion
    }
}
