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

using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.Self.Topics;
using IBM.Watson.DeveloperCloud.Logging;

using System.Collections;
using System.Text;
using System.Collections.Generic;
using MiniJSON;

namespace IBM.Watson.Self.Sensors
{
    /// <summary>
    /// This sensor manager allows the user to register/unregister sensors with the remote
    /// self instance through the TopicClient.
    /// </summary>
    public class SensorManager
    {
        #region Private Data

        Dictionary<string, ISensor > m_Sensors = new Dictionary<string, ISensor>();
        Dictionary<string, bool> m_Overrides = new Dictionary<string, bool>();
        bool m_bDisconnected = false;
        #endregion

        #region Public Properties
        public TopicClient TopicClient { get; protected set;}
        #endregion

        #region Public Interface
        public SensorManager(TopicClient a_TopicClient)
        {
            if (a_TopicClient == null)
                throw new WatsonException("TopicClient needs to be supported and can't be null.");
            TopicClient = a_TopicClient;
            TopicClient.StateChangedEvent += OnStateChanged;
            TopicClient.Subscribe( "sensor-manager", OnSensorManagerEvent );
        }

        ~SensorManager()  
        {
            TopicClient.StateChangedEvent -= OnStateChanged;
            TopicClient.Unsubscribe( "sensor-manager", OnSensorManagerEvent );
        }

        public bool IsRegistered( ISensor a_Sensor )
        {
            return m_Sensors.ContainsKey( a_Sensor.GetSensorId() );
        }

        /// <summary>
        /// Add a sensor with the remote self instance, agents may now subscribe to this 
        /// sensor and OnStart() will be invoked automatically by this framework.
        /// </summary>
        /// <param name="a_Sensor">The sensor object to add.</param>
        /// <param name="a_bOverride">If true, then any remote sensor with the same name will be overridden.</param>
        public void AddSensor( ISensor a_Sensor, bool a_bOverride )
        {
            if (! m_Sensors.ContainsKey( a_Sensor.GetSensorId() ) )
            {
                Dictionary<string,object> register = new Dictionary<string, object>();
                register["event"] = "add_sensor_proxy";
                register["sensorId"] = a_Sensor.GetSensorId();
                register["name"] = a_Sensor.GetSensorName();
                register["data_type"] = a_Sensor.GetDataType();
                register["binary_type"] = a_Sensor.GetBinaryType();
                register["override"] = a_bOverride;

                TopicClient.Publish( "sensor-manager", Json.Serialize( register ) );
                m_Sensors[ a_Sensor.GetSensorId() ] = a_Sensor;
                m_Overrides[a_Sensor.GetSensorId()] = a_bOverride;

                Log.Status( "SensorManager", "Sensor {0} added.", a_Sensor.GetSensorId() );
            }
        }

        public void SendData( ISensor a_Sensor, ISensorData a_Data)
        {
            if (!IsRegistered(a_Sensor))
                throw new WatsonException("SendData() invoked on unregisted sensors.");

            TopicClient.Publish("sensor-proxy-" + a_Sensor.GetSensorId(), a_Data.ToBinary());
        }

        //! Remove the provided sensor from the remote self instance.
        public void RemoveSensor( ISensor a_Sensor )
        {
            if ( m_Sensors.ContainsKey( a_Sensor.GetSensorId() ) )
            {
                m_Sensors.Remove( a_Sensor.GetSensorId() );
                m_Overrides.Remove(a_Sensor.GetSensorId());

                Dictionary<string,object> register = new Dictionary<string, object>();
                register["event"] = "remove_sensor_proxy";
                register["sensorId"] = a_Sensor.GetSensorId();

                TopicClient.Publish( "sensor-manager", Json.Serialize( register ) );
                Log.Status( "SensorManager", "Sensor {0} removed.", a_Sensor.GetSensorId() );
            }
        }

        #endregion

        #region Callback Functions
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
                // re-register all our sensors on reconnect.
                foreach (var kv in m_Sensors)
                {
                    string sensorId = kv.Key;
                    ISensor sensor = kv.Value;

                    Dictionary<string, object> register = new Dictionary<string, object>();
                    register["event"] = "add_sensor_proxy";
                    register["sensorId"] = sensor.GetSensorId();
                    register["name"] = sensor.GetSensorName();
                    register["data_type"] = sensor.GetDataType();
                    register["binary_type"] = sensor.GetBinaryType();
                    register["override"] = m_Overrides[sensorId];

                    TopicClient.Publish("sensor-manager", Json.Serialize(register));
                    Log.Status("SensorManager", "Sensor {0} restored.", sensor.GetSensorId());
                }
                m_bDisconnected = false;
            }
        }
        void OnDisconnected()
        {
            m_bDisconnected = true;
        }

        //! Callback for sensor-manager topic.
        void OnSensorManagerEvent( TopicClient.Payload a_Payload )
        {
            IDictionary json = Json.Deserialize( Encoding.UTF8.GetString( a_Payload.Data ) ) as IDictionary;

            bool bFailed = false;
            string sensorId = json["sensorId"] as string;

            ISensor sensor = null;
            if ( m_Sensors.TryGetValue( sensorId, out sensor ) )
            {
                string event_name = json["event"] as string;
                if (event_name.CompareTo("start_sensor") == 0)
                {
                    if (!sensor.OnStart())
                    {
                        Log.Error("SensorManager", "Failed to start sensor {0}", sensorId);
                        bFailed = true;
                    }
                }
                else if (event_name.CompareTo("stop_sensor") == 0)
                {
                    if (!sensor.OnStop())
                    {
                        Log.Error("SensorManager", "OnStop() returned failure for sensor {0}", sensorId);
                        bFailed = true;
                    }
                }
                else if (event_name.CompareTo("pause_sensor") == 0)
                    sensor.OnPause();
                else if (event_name.CompareTo("resume_sensor") == 0)
                    sensor.OnResume();
            }
            else
            {
                Log.Error( "SensorManager", "Failed to find sensor {0}", sensorId );
                bFailed = true;
            }

            // if we failed, send the message back with a different event
            if ( bFailed )
            {
                json["failed_event"] = json["event"];
                json["event"] = "error";

                TopicClient.Publish( "sensor-manager", Json.Serialize( json ) );
            }
        }
        #endregion

    }

}