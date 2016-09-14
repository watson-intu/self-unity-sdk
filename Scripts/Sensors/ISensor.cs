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
using System;

namespace IBM.Watson.Self.Sensors
{
    /// <summary>
    /// interface for any object that should be a sensor. The user should implement the interface, register the object with SensorManager,
    /// then should invoke SensorManager.SendData() when new sensor data is generated.
    /// </summary>
    public interface ISensor
    {
        //! This should return a unique ID for this sensor
        string GetSensorId();
        //! This should return a text name for this sensor
        string GetSensorName();
        //! This should return the type of data class this sensor sends
        string GetDataType();
        //! This should return the type of binary data
        string GetBinaryType();
        //! This is invoked when this sensor shoudl start calling SendData()
        bool OnStart();
        //! This is invoked when the last subscriber unsubscribe from this sensor
        bool OnStop();   
        void OnPause();
        void OnResume();
    }
}
