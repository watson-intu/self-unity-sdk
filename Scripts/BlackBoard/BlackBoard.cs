
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

namespace IBM.Watson.Self.BlackBoard
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
        #endregion

        #region Public Interface
        public static BlackBoard Instance { get { return Singleton<BlackBoard>.Instance; } }

        public BlackBoard()
        {
            TopicClient.Instance.Subscribe( "blackboard", OnBlackBoardEvent );
        }

        public void SubscribeToType( string a_Type, OnThingEvent a_Callback, ThingEventType a_EventMask = ThingEventType.TE_ALL )
        {
            if (! m_SubscriberMap.ContainsKey( a_Type ) )
                m_SubscriberMap[a_Type] = new List<Subscriber>();
            m_SubscriberMap[a_Type].Add( new Subscriber( a_Callback, a_EventMask ) );

            Dictionary<string,object> subscribe = new Dictionary<string, object>();
            subscribe["event"] = "subscribe_to_type";
            subscribe["type"] = a_Type;
            subscribe["event_mask"] = (int)a_EventMask;

            TopicClient.Instance.Publish( "blackboard", Json.Serialize( subscribe ) );
        }
        public void UnsubscribeFromType( string a_Type, OnThingEvent a_Callback = null )
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

            Dictionary<string,object> unsubscribe = new Dictionary<string, object>();
            unsubscribe["event"] = "unsubscribe_from_type";
            unsubscribe["type"] = a_Type;

            TopicClient.Instance.Publish( "blackboard", Json.Serialize( unsubscribe ) );
        }
        #endregion

        void OnBlackBoardEvent( TopicClient.Payload a_Payload )
        {
            IDictionary json = Json.Deserialize( Encoding.UTF8.GetString( a_Payload.Data ) ) as IDictionary;

            bool bFailed = false;
            string event_name = json["event"] as string;

            if ( event_name == "add_object" )
            {
            }
            else if ( event_name == "remove_object" )
            {
            }
            else if ( event_name == "set_object_state" )
            {
            }
            else if ( event_name == "set_object_importance" )
            {
            }

            string gestureId = json["gestureId"] as string;
            string instanceId = json["instanceId"] as string;
            string gestureKey = gestureId + "/" + instanceId;

            IGesture gesture = null;
            if ( m_Gestures.TryGetValue(gestureKey, out gesture ) )
            {
                if (event_name.CompareTo("execute_gesture") == 0)
                {
                    if (! gesture.Execute( OnGestureDone, json["params"] as IDictionary ) )
                    {
                        Log.Error("GestureManager", "Failed to execute gesture {0}", gestureId );
                        bFailed = true;
                    }
                }
                else if (event_name.CompareTo("abort_gesture") == 0)
                {
                    if (!gesture.Abort())
                    {
                        Log.Error("GestureManager", "Failed to abort gesture {0}", gestureId);
                        bFailed = true;
                    }
                }
            }
            else
            {
                Log.Error( "GestureManager", "Failed to find gesture {0}", gestureKey);
                bFailed = true;
            }

            // if we failed, send the message back with a different event
            if ( bFailed )
            {
                json["failed_event"] = event_name;
                json["event"] = "error";

                TopicClient.Instance.Publish( "gesture-manager", Json.Serialize( json ) );
            }
        }
    }
}
