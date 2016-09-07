using IBM.Watson.DeveloperCloud.DataTypes;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IBM.Watson.Self.Sensors
{
    //! This widget works with the existing MicrophoneWidget from the Unity SDK
    public class MicrophoneSensor : Widget
    {
        #region Private Data
		[SerializeField]
 		protected Input m_AudioInput = new Input("Audio", typeof(AudioData), "OnAudioInput");
        private SensorConnection m_Connection = new SensorConnection();
        #endregion

        #region Widget interface
        protected override string GetName()
        {
            return "MicrophoneSensor";
        }
        #endregion

		private void OnAudioInput(Data data)
		{
			AudioData audioData = data as AudioData;
            if ( audioData != null )
            {
                if (! m_Connection.IsConnected )
                    m_Connection.Connect( audioData.Clip.frequency, audioData.Clip.channels );

                if ( m_Connection.IsStarted && !m_Connection.IsPaused )
                    m_Connection.SendData( new SensorAudioData( audioData ) );
            }
			// Raise event for Pebble Manager - User is speaking
		}

        #region ISensorData object
        private class SensorAudioData : ISensorData
        {
            public SensorAudioData( AudioData a_Audio )
            {
                m_Audio = a_Audio;
            }

            public override byte[] ToBinary()
            {
                return AudioClipUtil.GetL16(m_Audio.Clip);
            }

            private AudioData m_Audio;
        }
        #endregion

        #region ISensor object
        private class SensorConnection : ISensor
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

                SensorManager.Instance.RegisterSensor( this );
            }
            public void Disconnect()
            {
                if ( m_IsConnected )
                {
                    SensorManager.Instance.UnregisterSensor( this );
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
        #endregion
    }
}
