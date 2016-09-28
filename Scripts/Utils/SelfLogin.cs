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

using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.Self.Services;

namespace IBM.Watson.Self.Utils
{
    /// <summary>
    /// This class wraps the functions necessary to login and register an embodiment wth the self backend.
    /// </summary>
    public class SelfLogin
    {
        #region Public Types
        public delegate void OnRegistered( string a_GroupId, string a_SelfId );
        public delegate void OnError();
        #endregion

        #region Public Properties
        public string GroupId { get; set; }
        public string OrgId { get; set; }
        public string SelfId { get; set; }
        public string Token { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public OnRegistered OnRegisteredEvent { get; set; }
        public OnError OnErrorEvent { get; set; }
        #endregion

        #region Private Data
        private RobotGateway    m_Gateway = new RobotGateway();
        #endregion

        #region Public Functions
        /// <summary>
        /// Load the current configuration information from the the local configuration file.
        /// </summary>
        public void LoadConfig()
        {
            GroupId = Config.Instance.GetVariableValue( "GroupID" );
            OrgId = Config.Instance.GetVariableValue( "OrgID" );
            Token = Config.Instance.GetVariableValue("BearerToken");
            SelfId = Config.Instance.GetVariableValue("SelfID");
            Name = Config.Instance.GetVariableValue("EmbodimentName");
            if ( string.IsNullOrEmpty( Name ) )
                Name = "SelfUnitySDK";
            Type = Config.Instance.GetVariableValue("EmbodimentType");
            if ( string.IsNullOrEmpty( Type ) )
                Type = "SelfUnitySDK";
        }

        /// <summary>
        /// Save the current configuration information from the local configuration file.
        /// </summary>
        public void SaveConfig()
        {
            Config.Instance.SetVariableValue( "GroupID", GroupId, true );
            Config.Instance.SetVariableValue( "OrgID", OrgId, true );
            Config.Instance.SetVariableValue( "BearerToken", Token, true );
            Config.Instance.SetVariableValue( "SelfID", SelfId, true );
            Config.Instance.SetVariableValue( "EmbodimentName", Name, true );
            Config.Instance.SetVariableValue( "EmbodimentType", Type, true );
            Config.Instance.SaveConfigToFileSystem();
        }

        /// <summary>
        /// Register with the gateway using the loaded groupId, orgId, and bearer token. 
        /// </summary>
        /// <param name="a_bLoadConfig"></param>
        /// <returns></returns>
        public bool RegisterEmbodiment( bool a_bLoadConfig = true )
        {
            if ( a_bLoadConfig )
                LoadConfig();

            return m_Gateway.RegisterEmbodiment( GroupId, OrgId, Token, Name, Type, OnRegisteredEmbodiment );
        }
        #endregion

        /// <summary>
        /// Callback for RobotGateway.RegisterEmbodiment()
        /// </summary>
        /// <param name="a_Token"></param>
        /// <param name="a_EmbodimentId"></param>
        private void OnRegisteredEmbodiment( string a_Token, string a_EmbodimentId )
        {
            if (! string.IsNullOrEmpty( a_Token ) 
                && !string.IsNullOrEmpty( a_EmbodimentId ) )
            {
                Log.Status( "SelfLogin", "Embodiment Registered: {0}", a_EmbodimentId );

                Token = a_Token;
                SelfId = a_EmbodimentId;
                SaveConfig();

                if ( OnRegisteredEvent != null )
                    OnRegisteredEvent( GroupId, SelfId );
            }
            else
            {
                Log.Error( "SelfLogin", "Failed to register embodiment." );
                if ( OnErrorEvent != null )
                    OnErrorEvent();
            }
        }
    }
}
