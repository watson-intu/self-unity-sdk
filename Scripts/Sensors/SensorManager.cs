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
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.Self.Topics;
using System.Collections.Generic;

namespace IBM.Watson.Self.Sensors
{
    //! This sensor manager allows the user to register/unregister sensors with the remote
    //! self instance through the TopicClient.
    public class SensorManager
    {
        #region Public Interface
        static SensorManager Instance { get { return Singleton<SensorManager>.Instance; } }

        public SensorManager()
        {
            TopicClient.Instance.Subscribe( "sensor-manager", OnSensorManagerEvent );
        }

        //! Register a sensor with the remote self instance, agents may now subscribe to this 
        //! sensor and OnStart() will be invoked automatically by this framework.
        public void RegisterSensor( ISensor a_Sensor )
        {
            Dictionary<string,object> register = new Dictionary<string, object>();
            register["event"] = "add_sensor_proxy";
            register["data_type"] = a_Sensor.GetSensorDataType();
            register["name"] = a_Sensor.GetSensorName();
        }

        //! Unregister the provided sensor object.
        public void UnregisterSensor( ISensor a_Sensor )
        {
        }

        #endregion

        void OnSensorManagerEvent( TopicClient.Payload a_Payload )
        {
        }
    }

}