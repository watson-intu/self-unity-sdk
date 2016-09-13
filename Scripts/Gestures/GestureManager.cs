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
using IBM.Watson.DeveloperCloud.Logging;
using MiniJSON;

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
            string gestureKey = a_Gesture.GetGestureId() + "/" + a_Gesture.GetInstanceId();
            if (! m_Gestures.ContainsKey(gestureKey) )
            {
                if ( a_Gesture.OnStart() )
                {
                    Dictionary<string,object> register = new Dictionary<string, object>();
                    register["event"] = "add_gesture_proxy";
                    register["gestureId"] = a_Gesture.GetGestureId();
                    register["instanceId"] = a_Gesture.GetInstanceId();
                    register["override"] = a_bOverride;

                    TopicClient.Instance.Publish( "gesture-manager", Json.Serialize( register ) );
                    m_Gestures[gestureKey] = a_Gesture;

                    Log.Status( "GestureManager", "Gesture {0} added.", gestureKey );
                }
            }
        }

        //! Remove the provided sensor from the remote self instance.
        public void RemoveGesture( IGesture a_Gesture )
        {
            string gestureKey = a_Gesture.GetGestureId() + "/" + a_Gesture.GetInstanceId();
            if ( m_Gestures.ContainsKey(gestureKey) )
            {
                if ( a_Gesture.OnStop() )
                {
                    m_Gestures.Remove(gestureKey);

                    Dictionary<string,object> register = new Dictionary<string, object>();
                    register["event"] = "remove_gesture_proxy";
                    register["gestureId"] = a_Gesture.GetGestureId();
                    register["instanceId"] = a_Gesture.GetInstanceId();

                    TopicClient.Instance.Publish( "gesture-manager", Json.Serialize( register ) );
                    Log.Status( "GestureManager", "Gesture {0} removed.", gestureKey );
                }
            }
        }

        #endregion

        #region Callback Functions

        //! Callback for sensor-manager topic.
        void OnGestureManagerEvent( TopicClient.Payload a_Payload )
        {
            IDictionary json = Json.Deserialize( Encoding.UTF8.GetString( a_Payload.Data ) ) as IDictionary;

            bool bFailed = false;
            string gestureId = json["gestureId"] as string;
            string instanceId = json["instanceId"] as string;
            string gestureKey = gestureId + "/" + instanceId;
            string event_name = json["event"] as string;

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
        void OnGestureDone(IGesture a_Gesture, bool a_Error)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            response["event"] = "execute_done";
            response["gestureId"] = a_Gesture.GetGestureId();
            response["instanceId"] = a_Gesture.GetInstanceId();
            response["error"] = a_Error;

            TopicClient.Instance.Publish("gesture-manager", Json.Serialize(response));
        }

        #endregion
    }

}
