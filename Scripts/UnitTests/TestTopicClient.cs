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

using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.UnitTests;
using IBM.Watson.Self.Topics;
using IBM.Watson.DeveloperCloud.Logging;

namespace IBM.Watson.Self.UnitTests
{
    public class TestTopicClient : UnitTest
    {
        bool m_bQueryTested = false;

        public override IEnumerator RunTest()
        {
            TopicClient client = TopicClient.Instance;

            client.Connect( "ws://localhost:9494", "faef2657-5f1b-436b-8225-2fa68728b1bf", OnConnected, OnDisconnected );
            while(! m_bQueryTested )
                yield return null;

            yield break;
        }

        private void OnConnected()
        {
            Log.Debug( "TestTopicClient", "OnConnected" );
            TopicClient.Instance.Query( ".", OnQuery );
        }

        private void OnDisconnected()
        {
            Log.Debug( "TestTopicClient", "OnDisconnected" );
        }

        private void OnQuery( TopicClient.QueryInfo a_Query )
        {
            m_bQueryTested = true;
        }
    }
}

