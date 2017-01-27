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
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.Self.Utils;
using UnityEngine;

namespace IBM.Watson.Self.UnitTests
{
    public class TestSelfDiscovery : UnitTest
    {
        bool m_bDiscoveryTested = false;
        int m_WaitForConnection = 2;

        public override IEnumerator RunTest()
        {
            m_WaitForConnection = 2;
            m_bDiscoveryTested = false;

            if(SelfDiscovery.Instance.OnDiscovered == null)
                SelfDiscovery.Instance.OnDiscovered += OnDiscovered;

            SelfDiscovery.Instance.StartDiscovery();

            while(! m_bDiscoveryTested )
            {
                yield return null;
            }

            yield break;
        }

        private void OnDiscovered( SelfDiscovery.SelfInstance a_Instance )
        {
            Log.Debug( "TestSelfDiscovery", "OnNodeAdded: {0}", a_Instance );
            m_WaitForConnection--;
            if (m_WaitForConnection <= 0)
            {
                SelfDiscovery.Instance.StopDiscovery();
                m_bDiscoveryTested = true;
                Test(true);
            }
        }
    }
}

