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
using System.Collections;
using System.Text;

namespace IBM.Watson.Self
{
    //! Interface for any object that should be a sensor
    public abstract class ISensor
    {
        #region ISensor interface
        public abstract string GetSensorName();
        public abstract Type GetSensorDataType();
        public abstract string GetBinaryType();
        public abstract bool OnStart();
        public abstract bool OnStop();
        public abstract void OnPause();
        public abstract void OnResume();
        #endregion

        #region Internal Interface
        protected void SendData( IData a_Data )
        {

        }
        #endregion
    }
}
