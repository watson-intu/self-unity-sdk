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

namespace IBM.Watson.Self.Sensors
{
    public class MicrophoneSensor : ISensor
    {
        #region Private Data
        private bool m_IsConnected = false;
        private bool m_IsStarted = false;
        private bool m_IsPaused = false;
        private int m_Rate = 0;
        private int m_Channels = 0;
        #endregion

        #region Public Properties
        public bool IsConnected { get { return m_IsConnected; } }
        public bool IsStarted { get { return m_IsStarted; } }
        public bool IsPaused { get { return m_IsPaused; } }
        #endregion

        #region Public Functions
        public void Connect( int a_Rate, int a_Channels )
        {
            m_Rate = a_Rate;
            m_Channels = a_Channels;
            m_IsConnected = true;

            SensorManager.Instance.AddSensor( this );
        }
        public void Disconnect()
        {
            if ( m_IsConnected )
            {
                SensorManager.Instance.RemoveSensor( this );
                m_IsConnected = false;
            }
        }
        #endregion

        #region ISensor interface
        public override string GetSensorName()
        {
            return "Microphone";
        }
        public override string GetDataType()
        {
            return "AudioData";
        }

        public override string GetBinaryType()
        {
            return string.Format("audio/L16;rate={0};channels={1}", m_Rate, m_Channels);
        }

        public override bool OnStart()
        {
            m_IsStarted = true;
            return true;
        }

        public override bool OnStop()
        {
            m_IsStarted = false;
            return true;
        }

        public override void OnPause()
        {
            m_IsPaused = true;
        }

        public override void OnResume()
        {
            m_IsPaused = false;
        }
        #endregion
    }
}
