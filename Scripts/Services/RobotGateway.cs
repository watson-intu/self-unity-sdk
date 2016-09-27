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

using IBM.Watson.DeveloperCloud.Connection;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services;
using IBM.Watson.DeveloperCloud.Utilities;
using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace IBM.Watson.Self.Services
{
    public class RobotGateway : IWatsonService 
    {
        private const string SERVICE_ID = "RobotGatewayV1";

        #region IWatsonService interface
        public string GetServiceID()
        {
            return "RobotGatewayV1";
        }
        public void GetServiceStatus(ServiceStatus callback)
        {
            callback( SERVICE_ID, true );
        }
        #endregion

        #region Public Types
        public delegate void OnRegisteredEmbodiment( string a_Token, string a_SelfId );
        #endregion

        #region RegisterEmbodiment
        private class RegisterEmbodimentReq : RESTConnector.Request
        {
            public OnRegisteredEmbodiment Callback { get; set; }
        };

        /// <summary>
        /// Register an embodiment with the gateway.
        /// </summary>
        /// <param name="a_GroupId"></param>
        /// <param name="a_OrgId"></param>
        /// <param name="a_BearerToken"></param>
        /// <param name="a_EmbodimentName"></param>
        /// <param name="a_EmbodimentType"></param>
        /// <param name="a_Callback"></param>
        /// <returns></returns>
        public bool RegisterEmbodiment( string a_GroupId, 
            string a_OrgId, 
            string a_BearerToken,
            string a_EmbodimentName,
            string a_EmbodimentType,
            OnRegisteredEmbodiment a_Callback )
        {
            RESTConnector connection = RESTConnector.GetConnector( SERVICE_ID, "/v1/auth/registerEmbodiment" );
            if ( connection == null )
            {
                Log.Error( "TopicClient", "RobotGatewayV1 service credentials not found." );
                return false;
            }

            Dictionary<string,string> headers = new Dictionary<string, string>();
	        headers["Content-Type"] = "application/json";
	        headers["Authorization"] = "Bearer " + a_BearerToken;
	        headers["groupId"] = a_GroupId;
	        headers["orgId"] = a_OrgId;
            headers["macId" ] = Utility.MacAddress;
	
            Dictionary<string,object> json = new Dictionary<string, object>();
            json["embodimentName"] = a_EmbodimentName;
            json["type"] = a_EmbodimentType;
            json["groupId"] = a_GroupId;
            json["orgId"] = a_OrgId;
            json["macId"] = Utility.MacAddress;
            json["embodimentToken"] = "token";

            RegisterEmbodimentReq req = new RegisterEmbodimentReq();
            req.Send = Encoding.UTF8.GetBytes( Json.Serialize( json ) );
            req.Headers = headers;
            req.Callback = a_Callback;

            req.OnResponse += OnRegisterEmbodiment;
            return connection.Send( req );
        }

        private void OnRegisterEmbodiment(RESTConnector.Request req, RESTConnector.Response resp)
        {
            RegisterEmbodimentReq ereq = req as RegisterEmbodimentReq;

            if (resp.Success)
            {
                try {
                    IDictionary json = Json.Deserialize( Encoding.UTF8.GetString( resp.Data ) ) as IDictionary;
                    string embodimentId = json["_id"] as string;
                    string token = json["embodimentToken"] as string;

                    if ( ereq.Callback != null )
                        ereq.Callback( token, embodimentId );
                }
                catch (Exception e)
                {
                    Log.Error("TopicClient", "OnRegisterEmbodiment Exception: {0}", e.ToString());
                    resp.Success = false;
                }
            }

            if (! resp.Success && ereq.Callback != null )
                ereq.Callback( null, null );
        }
        #endregion
    }
}
