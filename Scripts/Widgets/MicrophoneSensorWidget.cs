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

using IBM.Watson.DeveloperCloud.DataTypes;
using IBM.Watson.DeveloperCloud.Widgets;
using IBM.Watson.Self.Sensors;
using UnityEngine;

namespace IBM.Watson.Self.Widgets
{
    //! This widget works with the existing MicrophoneWidget from the Unity SDK
    public class MicrophoneSensorWidget : Widget
    {
        #region Private Data
		[SerializeField]
 		protected Input m_AudioInput = new Input("Audio", typeof(DeveloperCloud.DataTypes.AudioData), "OnAudioInput");
        private MicrophoneSensor m_Connection = new MicrophoneSensor();
        #endregion

        #region Widget interface
        protected override string GetName()
        {
            return "MicrophoneSensor";
        }
        #endregion

        #region Input Handlers
        private void OnAudioInput(Data data)
		{
			DeveloperCloud.DataTypes.AudioData audioData = data as DeveloperCloud.DataTypes.AudioData;
            if ( audioData != null )
            {
                if (! m_Connection.IsConnected )
                    m_Connection.Connect( audioData.Clip.frequency, audioData.Clip.channels );

                if ( m_Connection.IsStarted && !m_Connection.IsPaused )
                    m_Connection.SendData( new Sensors.AudioData( audioData ) );
            }
		}
        #endregion
    }
}
