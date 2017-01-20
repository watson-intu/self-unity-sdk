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

using IBM.Watson.DeveloperCloud.DataTypes;
using IBM.Watson.DeveloperCloud.Widgets;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.Self.Topics;
using System;
using UnityEngine;
using MiniJSON;

namespace IBM.Watson.Self.Widgets
{
	public class SelfRemoteSpeechGesture : Widget
	{
		#region Private Data
		[SerializeField]
		private Output m_Status = new Output( typeof(StatusData), true );
		[SerializeField]
		private Output m_DisableMic = new Output(typeof(DisableMicData));
		#endregion

		#region MonoBehavior interface
		protected override void Start()
		{
			base.Start();
			TopicClient.Instance.StateChangedEvent += OnStateChanged;
			TopicClient.Instance.Subscribe( "audio-out", OnAudioEvent );
		}
		#endregion

		#region Widget interface
		protected override string GetName()
		{
			return "SelfRemoteSpeechGesture";
		}
		#endregion

		#region Callback Functions

		void OnStateChanged(TopicClient.ClientState a_CurrentState)
		{
			Log.Status ("SelfRemoteSpeechGesture", "TopicClient state has changed");
		}

		void OnAudioEvent(TopicClient.Payload a_Payload)
		{
			Log.Status ("SelfRemoteSpeechGesture", "OnAudioEvent() Called!");
			float[] f = ConvertByteToFloat (a_Payload.Data);
			AudioClip clip = AudioClip.Create("test",f.Length,1,22050, false);
			clip.SetData (f, 0);
			PlayClip (clip);
		}

		private float[] ConvertByteToFloat(byte[] array)
		{
			float[] floatArr = new float[array.Length / 2];
			for (int i = 0; i < floatArr.Length; i++) 
			{
				floatArr [i] = (float)BitConverter.ToInt16 (array, i * 2) / 32768.0f;
			}
			return floatArr;
		}

		private void PlayClip(AudioClip clip)
		{
			if (Application.isPlaying && clip != null)
			{
				m_Status.SendData( new StatusData("ANSWERING"));
				m_DisableMic.SendData(new DisableMicData(true));
				GameObject audioObject = new GameObject("AudioObject");
				AudioSource source = audioObject.AddComponent<AudioSource>();
				source.spatialBlend = 0.0f;     // 2D sound
				source.loop = false;            // do not loop
				source.clip = clip;             // clip
				source.Play();
				// automatically destroy the object after the sound has played..
				GameObject.Destroy(audioObject, clip.length);
				Invoke ("onClipFinished", clip.length);
			}
		}

		private void onClipFinished() 
		{
			m_Status.SendData (new StatusData ("LISTENING"));
			m_DisableMic.SendData(new DisableMicData(false));
		}

			
		#endregion

	}
}
