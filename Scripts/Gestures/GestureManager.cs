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
        Dictionary<string, IGesture> m_Gestures = new Dictionary<string, IGesture>();
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

        //! Add a sensor with the remote self instance, agents may now subscribe to this 
        //! sensor and OnStart() will be invoked automatically by this framework.
        public void AddGesture( IGesture a_Gesture )
        {
            if (! m_Gestures.ContainsKey( a_Gesture.GetGestureId() ) )
            {
                if ( a_Gesture.OnStart() )
                {
                    Dictionary<string,object> register = new Dictionary<string, object>();
                    register["event"] = "add_gesture_proxy";
                    register["gestureId"] = a_Gesture.GetGestureId();
                    register["name"] = a_Gesture.GetGestureName();

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
        //! Callback for sensor-manager topic.
        void OnGestureManagerEvent( TopicClient.Payload a_Payload )
        {
            IDictionary json = Json.Deserialize( Encoding.UTF8.GetString( a_Payload.Data ) ) as IDictionary;

            bool bFailed = false;
            string gestureId = json["gestureId"] as string;

            IGesture gesture = null;
            if ( m_Gestures.TryGetValue( gestureId, out gesture ) )
            {
                string event_name = json["event"] as string;
                if (event_name.CompareTo("execute_gesture") == 0)
                {
                    if (!gesture.OnStart())
                    {
                        Log.Error("GestureManager", "Failed to start gesture {0}", gestureId);
                        bFailed = true;
                    }
                }
                else if (event_name.CompareTo("stop_sensor") == 0)
                {
                    if (!gesture.OnStop())
                    {
                        Log.Error("GestureManager", "OnStop() returned failure for sensor {0}", gestureId);
                        bFailed = true;
                    }
                }
                else if (event_name.CompareTo("pause_sensor") == 0)
                    gesture.OnPause();
                else if (event_name.CompareTo("resume_sensor") == 0)
                    gesture.OnResume();
            }
            else
            {
                Log.Error( "GestureManager", "Failed to find sensor {0}", gestureId );
                bFailed = true;
            }

            // if we failed, send the message back with a different event
            if ( bFailed )
            {
                json["failed_event"] = json["event"];
                json["event"] = "error";

                TopicClient.Instance.Publish( "gesture-manager", Json.Serialize( json ) );
            }
        }
        #endregion
    }

}
