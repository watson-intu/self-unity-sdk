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
using System.Collections.Generic;
using IBM.Watson.Self.Utils;

namespace IBM.Watson.Self.Agents
{
    public enum ThingCategory
    {
	    TT_INVALID = -1,
	    TT_PERCEPTION,
	    TT_AGENCY,
	    TT_MODEL
    }

    public class IThing : ISerializable
    {
        #region Public Properties
        // IMplementation type
        public string Type { get; set; }
        public IDictionary Body { get; set; }
        public ThingCategory Category { get; set; }
        public string GUID { get; set; }
        public double Importance { get; set; }
        public string State { get; set; }
        public double CreateTime { get; set; }
        public double LifeSpan { get; set; }
        // data driven type
        public string DataType { get; set; }        
        public IDictionary Data { get; set; }
        public string ParentGUID { get; set; }
        #endregion

        #region ISerializable interface
        public void Deserialize( IDictionary a_Json )
        {
            Body = a_Json;
            Type = a_Json["Type_"] as string;
            Category = (ThingCategory)(long)a_Json["m_eCategory"];
            GUID = a_Json["m_GUID"] as string;
            State = a_Json["m_State"] as string;

            if ( a_Json["m_fImportance"] is double )
                Importance = (double)a_Json["m_fImportance"];
            if ( a_Json["m_CreateTime"] is double )
                CreateTime = (double)a_Json["m_CreateTime"];
            if ( a_Json["m_fLifeSpan"] is double )
                LifeSpan = (double)a_Json["m_fLifeSpan"];
            if ( a_Json.Contains( "m_DataType" ) )
                DataType = a_Json["m_DataType"] as string;
            if ( a_Json.Contains( "m_Data" ) )
                Data = a_Json["m_Data"] as IDictionary;
        }
        public IDictionary Serialize()
        {
            Dictionary<string,object> json = new Dictionary<string, object>();
            json["Type_"] = Type;
            json["m_eCategory"] = (int)Category;
            json["m_GUID"] = GUID;
            json["m_fImportance"] = Importance;
            json["m_State"] = State;
            json["m_CreateTime"] = CreateTime;
            json["m_fLifeSpan"] = LifeSpan;

            if (! string.IsNullOrEmpty( DataType ) )
            {
                json["m_DataType"] = DataType;
                json["m_Data"] = Data;
            }

            return json;
        }
        #endregion
    }

    public enum ThingEventType
    {
	    TE_NONE			= 0x0,			// no flags
	    TE_ADDED		= 0x1,			// IThing has been added
	    TE_REMOVED		= 0x2,			// IThing has been removed
	    TE_STATE		= 0x4,			// state of IThing has changed.
	    TE_IMPORTANCE	= 0x8,			// Importance of IThing has changed.

	    TE_ALL = TE_ADDED | TE_REMOVED | TE_STATE | TE_IMPORTANCE,
        TE_ADDED_OR_STATE = TE_ADDED | TE_STATE
    }
    public struct ThingEvent
    {
        public ThingEventType  m_EventType;
        public IDictionary     m_Event;
        public IThing          m_Thing;
    }
}
