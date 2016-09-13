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

namespace IBM.Watson.Self.BlackBoard
{
    public class IThing
    {
        #region Public Properties
        public string GUID { get; set; }
        public string State { get; set; }
        public float Importance { get; set; }
        public double CreateTime { get; set; }
        public string Type { get; set; }
        public IDictionary Data { get; set; }
        #endregion

        public void Deserialzie( IDictionary a_Data )
        {
            Type = a_Data["Type_"] as string;

        }
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
        ThingEventType  m_EventType;
        IDictionary     m_Event;
        IThing          m_Thing;
    }
}
