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
    //! This sensor manager allows the user to register/unregister sensors with the remote
    //! self instance through the TopicClient.
    public class SensorManager
    {
        #region Private Data
        Dictionary<string, ISensor >    m_Sensors = new Dictionary<string, ISensor>();
        #endregion

        #region Public Interface
        public static SensorManager Instance { get { return Singleton<SensorManager>.Instance; } }

        public SensorManager()
        {
            TopicClient.Instance.Subscribe( "sensor-manager", OnSensorManagerEvent );
        }

        public bool IsRegistered( ISensor a_Sensor )
        {
            return m_Sensors.ContainsKey( a_Sensor.GetSensorId() );
        }

        //! Add a sensor with the remote self instance, agents may now subscribe to this 
        //! sensor and OnStart() will be invoked automatically by this framework.
        public void AddSensor( ISensor a_Sensor )
        {
            if (! m_Sensors.ContainsKey( a_Sensor.GetSensorId() ) )
            {
                Dictionary<string,object> register = new Dictionary<string, object>();
                register["event"] = "add_sensor_proxy";
                register["sensorId"] = a_Sensor.GetSensorId();
                register["name"] = a_Sensor.GetSensorName();
                register["data_type"] = a_Sensor.GetDataType();
                register["binary_type"] = a_Sensor.GetBinaryType();

                TopicClient.Instance.Publish( "sensor-manager", Json.Serialize( register ) );
                m_Sensors[ a_Sensor.GetSensorId() ] = a_Sensor;

                Log.Status( "SensorManager", "Sensor {0} added.", a_Sensor.GetSensorId() );
            }
        }

        public void SendData( ISensor a_Sensor, ISensorData a_Data)
        {
            if (!IsRegistered(a_Sensor))
                throw new WatsonException("SendData() invoked on unregisted sensors.");

            TopicClient.Instance.Publish("sensor-proxy-" + a_Sensor.GetSensorId(), a_Data.ToBinary());
        }

        //! Remove the provided sensor from the remote self instance.
        public void RemoveSensor( ISensor a_Sensor )
        {
            if ( m_Sensors.ContainsKey( a_Sensor.GetSensorId() ) )
            {
                m_Sensors.Remove( a_Sensor.GetSensorId() );

                Dictionary<string,object> register = new Dictionary<string, object>();
                register["event"] = "remove_sensor_proxy";
                register["sensorId"] = a_Sensor.GetSensorId();

                TopicClient.Instance.Publish( "sensor-manager", Json.Serialize( register ) );
                Log.Status( "SensorManager", "Sensor {0} removed.", a_Sensor.GetSensorId() );
            }
        }

        #endregion

        #region Callback Functions
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

                TopicClient.Instance.Publish( "sensor-manager", Json.Serialize( json ) );
            }
        }
        #endregion

    }

}