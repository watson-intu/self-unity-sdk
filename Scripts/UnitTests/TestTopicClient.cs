/**
* Copyright 2015 IBM Corp. All Rights Reserved.
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
using IBM.Watson.DeveloperCloud.UnitTests;
using IBM.Watson.Self.Topics;
using IBM.Watson.DeveloperCloud.Logging;
using System.Text;

namespace IBM.Watson.Self.UnitTests
{
    public class TestTopicClient : UnitTest
    {
        bool m_bQueryTested = false;
        bool m_bSubFailedTested = false;
        bool m_bSubscribeBinaryTested = false;
        bool m_bSubscribeTextTested = false;
        TopicClient m_TopicClient = null;

        public override IEnumerator RunTest()
        {
            m_TopicClient = new TopicClient();
            if ( m_TopicClient.IsActive )
            {
                m_TopicClient.Disconnect();
                while( m_TopicClient.IsActive ) 
                    yield return null;
            }

            m_TopicClient.StateChangedEvent += OnStateChanged;

            m_TopicClient.Connect();
            while(! m_bQueryTested )
                yield return null;
            while(! m_bSubscribeBinaryTested )
                yield return null;
            while(! m_bSubscribeTextTested )
                yield return null;
            while(! m_bSubFailedTested )
                yield return null;

            m_TopicClient.StateChangedEvent -= OnStateChanged;

            yield break;
        }

        void OnStateChanged(TopicClient.ClientState a_CurrentState)
        {
            Log.Debug( "TestBlackBoard", "OnStateChanged to {0}" , a_CurrentState);

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

        private void OnConnected()
        {
            Log.Debug( "TestTopicClient", "OnConnected" );
            m_TopicClient.Query( ".", OnQuery );
        }

        private void OnDisconnected()
        {
            Log.Debug( "TestTopicClient", "OnDisconnected" );
        }

        private void OnQuery( TopicClient.QueryInfo a_Query )
        {
            Log.Debug( "TopicClient", "OnQuery(). QueryResponse: {0}", a_Query );
            m_bQueryTested = true;

            m_TopicClient.Subscribe( "sensor-Microphone", OnMicrophoneData );
        }

        private void OnMicrophoneData( TopicClient.Payload a_Payload )
        {
            Log.Debug( "TopicClient", "OnMicrophoneData() received. Payload: {0}", a_Payload );
            Test( m_TopicClient.Unsubscribe( "sensor-Microphone", OnMicrophoneData ) );
            m_bSubscribeBinaryTested = true;

            m_TopicClient.Subscribe( "blackboard", OnBlackboard );
            m_TopicClient.Subscribe( "invalid-topic", OnInvalidTopic );
            m_TopicClient.Publish( "conversation", "tell me a joke" );
        }

        private void OnBlackboard( TopicClient.Payload a_Payload )
        {
            Log.Debug( "TopicClient", "OnBlackboard(). Payload: {0} \n Data: {1}", a_Payload, Encoding.UTF8.GetString( a_Payload.Data ) );
            Test( m_TopicClient.Unsubscribe( "blackboard", OnBlackboard ) );
            m_bSubscribeTextTested = true;
        }

        private void OnInvalidTopic( TopicClient.Payload a_Payload )
        {
            Log.Debug( "TopicClient", "OnInvalidTopic(). Payload: {0}", a_Payload );
            Test( a_Payload == null );
            m_bSubFailedTested = true;
        }
    }
}

