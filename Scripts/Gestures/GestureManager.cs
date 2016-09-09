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


using System.Collections;
using System.Collections.Generic;
using System.Text;

using IBM.Watson.Self.Topics;
using IBM.Watson.DeveloperCloud.Utilities;
using MiniJSON;
using IBM.Watson.DeveloperCloud.Logging;

namespace IBM.Watson.Self.Gestures
{
    public class GestureManager
    {
        #region Private Data
        private Dictionary<string, IGesture> m_Gestures = new Dictionary<string, IGesture>();
        #endregion

        #region Public Interface
        public static GestureManager Instance { get { return Singleton<GestureManager>.Instance; } }

        public GestureManager()
        {
            TopicClient.Instance.Subscribe( "gesture-manager", OnGestureManagerEvent );
        }

        public bool IsRegistered( IGesture a_Gesture )
        {
            return m_Gestures.ContainsKey( a_Gesture.GetGestureId() );
        }

        // register a new gesture with self. If a_bOVerride is true, then any previous gesture with the same ID
        // will be replaced by this new gesture. If false, then this gesture will be added alongside any existing gestures
        // with the same ID.
        public void AddGesture( IGesture a_Gesture, bool a_bOverride = true )
        {
            if (! m_Gestures.ContainsKey( a_Gesture.GetGestureId() ) )
            {
                if ( a_Gesture.OnStart() )
                {
                    Dictionary<string,object> register = new Dictionary<string, object>();
                    register["event"] = "add_gesture_proxy";
                    register["gestureId"] = a_Gesture.GetGestureId();
                    register["override"] = a_bOverride;

                    TopicClient.Instance.Publish( "gesture-manager", Json.Serialize( register ) );
                    m_Gestures[ a_Gesture.GetGestureId() ] = a_Gesture;

                    Log.Status( "GestureManager", "Gesture {0} added.", a_Gesture.GetGestureId() );
                }
            }
        }

        //! Remove the provided sensor from the remote self instance.
        public void RemoveGesture( IGesture a_Gesture )
        {
            if ( m_Gestures.ContainsKey( a_Gesture.GetGestureId() ) )
            {
                if ( a_Gesture.OnStop() )
                {
                    m_Gestures.Remove( a_Gesture.GetGestureId() );

                    Dictionary<string,object> register = new Dictionary<string, object>();
                    register["event"] = "remove_gesture_proxy";
                    register["gestureId"] = a_Gesture.GetGestureId();

                    TopicClient.Instance.Publish( "gesture-manager", Json.Serialize( register ) );
                    Log.Status( "GestureManager", "Gesture {0} removed.", a_Gesture.GetGestureId() );
                }
            }
        }

        #endregion

        #region Callback Functions

        // object used to handle a execute gesture request and return a response when completed.
        private class ExecuteRequest
        {
            private IGesture m_Gesture;
            private int m_Request;

            public ExecuteRequest(IGesture a_Gesture, int a_Request, IDictionary a_Params)
            {
                m_Gesture = a_Gesture;
                m_Request = a_Request;

                if (!m_Gesture.Execute(OnGestureDone, a_Params))
                    throw new WatsonException("Failed to invoke execute");
            }

            void OnGestureDone( IGesture a_Gesture, bool a_Error )
            {
                if (a_Gesture != m_Gesture)
                    throw new WatsonException("a_Gesture != m_Gesture");

                Dictionary<string, object> response = new Dictionary<string, object>();
                response["event"] = "execute_done";
                response["request"] = m_Request;
                response["error"] = a_Error;

                TopicClient.Instance.Publish("gesture-manager", Json.Serialize(response));
            }
        };

        //! Callback for sensor-manager topic.
        void OnGestureManagerEvent( TopicClient.Payload a_Payload )
        {
            IDictionary json = Json.Deserialize( Encoding.UTF8.GetString( a_Payload.Data ) ) as IDictionary;

            bool bFailed = false;
            string gestureId = json["gestureId"] as string;
            string event_name = json["event"] as string;

            IGesture gesture = null;
            if ( m_Gestures.TryGetValue( gestureId, out gesture ) )
            {
                if (event_name.CompareTo("execute_gesture") == 0)
                {
                    int request = (int)json["request"];

                    try {
                        new ExecuteRequest(gesture, request, json["params"] as IDictionary);
                    }
                    catch (WatsonException ex)
                    {
                        Log.Error("GestureManager", "Failed to execute gesture {0}: {1}", gestureId, ex.Message );
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
                Log.Error( "GestureManager", "Failed to find gesture {0}", gestureId );
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
        #endregion
    }

}
