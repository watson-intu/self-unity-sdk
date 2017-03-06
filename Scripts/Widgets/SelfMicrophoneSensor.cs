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
using UnityEngine;
using IBM.Watson.DeveloperCloud.Widgets;
using IBM.Watson.Self.Sensors;
using IBM.Watson.DeveloperCloud.DataTypes;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Logging;

namespace IBM.Watson.Self.Widgets
{
    /// <summary>
    /// This widget works with the existing MicrophoneWidget from the Unity SDK
    /// </summary>
    public class SelfMicrophoneSensor : Widget, ISensor
    {
        #region Private Data
        private string m_SensorId = Guid.NewGuid().ToString();
        private bool m_IsAdded = false;
        private bool m_IsStarted = false;
        private bool m_IsPaused = false;
        private int m_Rate = 0;
        private int m_Channels = 0;

        [SerializeField]
        protected Input m_AudioInput = new Input("Audio", typeof(DeveloperCloud.DataTypes.AudioData), "OnAudioInput");
        [SerializeField]
        private Input m_TopicClientInput = new Input("TopicClientInput", typeof(TopicClientData), "OnTopicClientInput");
        [SerializeField]
        protected bool m_bOverride = true;
        #endregion

        #region Public Properties
        public SensorManager SensorManager{ get; private set; }
        #endregion

        #region Widget interface
        protected override string GetName()
        {
            return "SelfMicrophoneSensor";
        }
        #endregion

        #region ISensor interface
        public string GetSensorId()
        {
            return m_SensorId;
        }
        public string GetSensorName()
        {
            return "Microphone";
        }
        public string GetDataType()
        {
            return "AudioData";
        }

        public string GetBinaryType()
        {
            return string.Format("audio/L16;rate={0};channels={1}", m_Rate, m_Channels);
        }

        public bool OnStart()
        {
            m_IsStarted = true;
            return true;
        }

        public bool OnStop()
        {
            m_IsStarted = false;
            return true;
        }

        public void OnPause()
        {
            m_IsPaused = true;
        }

        public void OnResume()
        {
            m_IsPaused = false;
        }
        #endregion

        #region Input Handlers
        private void OnAudioInput(Data data)
        {
            DeveloperCloud.DataTypes.AudioData audioData = data as DeveloperCloud.DataTypes.AudioData;
            if (audioData != null)
            {
                if (!m_IsAdded)
                    Add(audioData.Clip.frequency, audioData.Clip.channels);

                if (IsStarted && !IsPaused)
                {
                    if(SensorManager != null)
                        SensorManager.SendData(this, new Sensors.AudioData(audioData));
                    else
                        Log.Error("SelfMicrophoneSensor", "SensorManager needs to be supported and can't be null.");
                }
            }
        }

        private void OnTopicClientInput(Data data)
        {
            TopicClientData a_TopicClientData = (TopicClientData)data;
            if (a_TopicClientData == null || a_TopicClientData.TopicClient == null)
            {
                throw new WatsonException("TopicClient needs to be supported and can't be null.");
            }
            SensorManager = new SensorManager(a_TopicClientData.TopicClient);
        }
        #endregion

        #region Public Properties
        public bool IsStarted { get { return m_IsStarted; } }
        public bool IsPaused { get { return m_IsPaused; } }
        #endregion

        #region Public Functions
        public void Add(int a_Rate, int a_Channels)
        {
            m_Rate = a_Rate;
            m_Channels = a_Channels;
            m_IsAdded = true;

            if(SensorManager != null)
                SensorManager.AddSensor(this, m_bOverride );
            else
                Log.Error("SelfMicrophoneSensor", "SensorManager needs to be supported and can't be null.");
        }
        public void Remove()
        {
            if (m_IsAdded)
            {
                if(SensorManager != null)
                    SensorManager.RemoveSensor(this);
                else
                    Log.Error("SelfMicrophoneSensor", "SensorManager needs to be supported and can't be null.");
                
                m_IsAdded = false;
            }
        }
        #endregion

    }
}
