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
        bool m_bSubscribeTested = false;

        public override IEnumerator RunTest()
        {
            TopicClient client = TopicClient.Instance;

            client.Connect( "ws://localhost:9494", "faef2657-5f1b-436b-8225-2fa68728b1bf", null, OnConnected, OnDisconnected );
            while(! m_bSubscribeTested )
                yield return null;

            yield break;
        }

        private void OnConnected()
        {
            Log.Debug( "TestBlackBoard", "OnConnected" );
            BlackBoard.Instance.SubscribeToType( "Text", OnText );
        }

        private void OnDisconnected()
        {
            Log.Debug( "TestBlackBoard", "OnDisconnected" );
        }

        private void OnText( ThingEvent a_Event )
        {
            m_bSubscribeTested = true;

        }
    }
}

