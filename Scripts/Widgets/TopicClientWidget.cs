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

using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Widgets;
using IBM.Watson.Self.Topics;

namespace IBM.Watson.Self.Widgets
{
    public class TopicClientWidget : Widget
    {
        #region Private Data
        [SerializeField]
        private string m_Host = "ws://127.0.0.1:9443";
        [SerializeField]
        private string m_EmbodimentId = null;
        [SerializeField]
        private string m_Token = null;
        [SerializeField]
        private string m_ParentInstance = null;


        [SerializeField]
        private Output m_TopicClientOutput = new Output(typeof(TopicClientData), true);
        private TopicClient m_TopicClient = null;
        #endregion

        #region Public Properties

        public TopicClient TopicClient
        {
            get
            {
                if (m_TopicClient == null)
                {
                    m_TopicClient = new TopicClient();
                    m_TopicClient.Connect(m_Host, m_EmbodimentId, m_Token, m_ParentInstance);
                }
                return m_TopicClient;
            }
        }

        #endregion

        protected override void Start()
        {
            base.Start();
            m_TopicClientOutput.OnInputAdded += HandleOnInputAdded;
        }

        public void OnDestroy()
        {
            m_TopicClientOutput.OnInputAdded -= HandleOnInputAdded;
        }

        void HandleOnInputAdded (Input input)
        {
            input.ReceiveData(new TopicClientData(TopicClient));
        }
    	
        #region Widget interface
        protected override string GetName()
        {
            return "SelfTopicClient";
        }
        #endregion
    }
}
