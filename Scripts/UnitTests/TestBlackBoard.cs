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
using IBM.Watson.Self.Agents;
using IBM.Watson.DeveloperCloud.Logging;

namespace IBM.Watson.Self.UnitTests
{
    public class TestBlackBoard : UnitTest
    {

        string m_Host = "ws://192.168.1.9:9443";
        string m_GroupId = "e664f5dd5c108e01b140180b50f4eafe";
        string m_SelfId = "aecea8a8afa3d8f224c12576512c7e0f";
        string m_TargetPath = "";
            
        bool m_bSubscribeTested = false;
        bool m_ConnectionClosed = false;

        TopicClient client = null;

        public override IEnumerator RunTest()
        {
            client = TopicClient.Instance;

            client.ConnectedEvent += OnConnected;
            client.DisconnectedEvent += OnDisconnected;

            client.Connect( m_Host, m_GroupId, m_SelfId);

            while(! m_bSubscribeTested )
                yield return null;

            Log.Debug( "TestBlackBoard", "Tested Subscription now disconnecting" );
            client.Disconnect();
                
            while(! m_ConnectionClosed )
                yield return null;
            
            yield break;
        }

        private void OnConnected()
        {
            Log.Debug( "TestBlackBoard", "OnConnected" );
            client.Target = m_TargetPath;
            BlackBoard.Instance.SubscribeToType( "Text", OnText );
            m_ConnectionClosed = false;
        }

        private void OnDisconnected()
        {
            Log.Debug( "TestBlackBoard", "OnDisconnected" );

            if (m_bSubscribeTested)
            {
                client.ConnectedEvent -= OnConnected;
                client.DisconnectedEvent -= OnDisconnected;
            }
            m_ConnectionClosed = true;
        }

        private void OnText( ThingEvent a_Event )
        {
            Log.Debug( "TestBlackBoard", "OnText : {0}", a_Event );
            m_bSubscribeTested = true;

        }
    }
}

