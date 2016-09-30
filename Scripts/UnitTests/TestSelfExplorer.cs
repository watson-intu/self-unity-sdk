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
using IBM.Watson.Self.Utils;

namespace IBM.Watson.Self.UnitTests
{
    public class TestSelfExplorer : UnitTest
    {
        bool m_bExplorerTested = false;
        SelfExplorer m_Explorer = new SelfExplorer();

        public override IEnumerator RunTest()
        {
            TopicClient client = TopicClient.Instance;
            //client.Connect();
            client.Connect( "ws://khanincparentc937325f062f667a7b5c0bef4343aec9.mybluemix.net", "7fa19c667cd4262dc7ea2bfc646d1db7", "95dc218b96fedd16aa7b68e452c95eec" );

            m_Explorer.OnNodeAdded += OnNodeAdded;
            m_Explorer.OnNodeRemoved += OnNodeRemoved;
            m_Explorer.OnNodeReady += OnNodeReady;
            m_Explorer.OnExplorerDone += OnExploreDone;

            m_Explorer.Explore();

            while(! m_bExplorerTested )
                yield return null;

            m_bExplorerTested = false;
            m_Explorer.Explore();
           
            while(! m_bExplorerTested )
                yield return null;
            
            yield break;
        }

        private void OnNodeAdded( SelfExplorer.Node a_Added )
        {
            Log.Debug( "TestSelfExplorer", "OnNodeAdded: {0}", a_Added );
        }
        private void OnNodeRemoved( SelfExplorer.Node a_Added )
        {
            Log.Debug( "TestSelfExplorer", "OnNodeRemoved: {0}", a_Added );
        }
        private void OnNodeReady( SelfExplorer.Node a_Node )
        {
            Log.Debug( "TestSelfExplorer", "OnNodeReady: {0}", a_Node);
        }
        private void OnExploreDone( SelfExplorer a_Explorer )
        {
            Log.Debug( "TestSelfExplorer", "OnExploreDone: {0}", a_Explorer );
            m_bExplorerTested = true;
        }
    }
}

