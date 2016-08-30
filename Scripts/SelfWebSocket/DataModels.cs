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

//#define  ENABLE_DEBUGGING

using FullSerializer;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Logging;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IBM.Watson.DeveloperCloud.Services.Self.v1
{
    #region Enum for all Sensor / Joints Data

    /// <summary>
    /// Sensor warning level.
    /// http://doc.aldebaran.com/2-1/family/nao_dcm/actuator_sensor_names.html#term-error
    /// </summary>
    public enum SensorWarningLevel
    {
        /// <summary>
        /// The data is not related any warning level
        /// </summary>
        NaN = -1,
        /// <summary>
        /// show everything - No effect on the robot.
        /// No effect on the robot.
        /// </summary>
        None = 0,   
        /// <summary>
        /// show low warning - temperature is low temperature 
        /// </summary>
        Low,        
        /// <summary>
        /// show medium warnings - 
        /// </summary>
        Medium,     
        /// <summary>
        /// show high warning -   tempereature is high temperature
        /// </summary>
        High,       
        /// <summary>
        /// show only error - not hight temp but error signals
        ///     The Stiffness control on the Chains that includes the joint or actuator in failure is disabled and it will not be possible to reactivate it.
        /// </summary>
        Error, 
        /// <summary>
        /// Show only fata erros - critical error
        /// The robot goes to rest and refuses to wake up. So, the robot will not be usable.
        /// </summary>
        Critical    
    }


    public enum SelfThingType
    {
        None,
        Text,
        Say,
        Health,
        Environment
    }

    public enum SelfSensorType
    {
        None = 0,

        #region All Joints
        HeadYaw,
        HeadRoll,
        HeadPitch,
        LeftElbowYaw,
        LeftElbowRoll,
        LeftElbowPitch,
        RightElbowYaw,
        RightElbowRoll,
        RightElbowPitch,
        LeftHand,
        LeftHandYaw,
        LeftHandRoll,
        LeftHandPitch,
        RightHand,
        RightHandYaw,
        RightHandRoll,
        RightHandPitch,
        RightWristYaw,
        RightWristPitch,
        RightWristRoll,
        LeftWristYaw,
        LeftWristPitch,
        LeftWristRoll,
        LeftShoulderYaw,
        LeftShoulderPitch,
        LeftShoulderRoll,
        RightShoulderYaw,
        RightShoulderPitch,
        RightShoulderRoll,
        LeftHipYawPitch,
        LeftHipPitch,
        LeftHipRoll,
        RightHipYawPitch,
        RightHipPitch,
        RightHipRoll,
        LeftKneeYaw,
        LeftKneePitch,
        LeftKneeRoll,
        RightKneeYaw,
        RightKneePitch,
        RightKneeRoll,
        LeftAnkleYaw,
        LeftAnklePitch,
        LeftAnkleRoll,
        RightAnkleYaw,
        RightAnklePitch,
        RightAnkleRoll,
        KneePitch,
        HipPitch,
        HipRoll,
        WheelFR,
        WheelFL,
        WheelB,
        Brake,
        #endregion

        #region Sensors
        Head = 200,
        Battery,
        Gyrometer,
        Accelerometer,
        LaserSensor,
        //Temp
        LeftaserSensor,
        CameraTop,
        CameraBottom,
        CameraDepth,
        Bumper,
        GyrometerBase,
        AccelerometerBase,
        Sonar,
        #endregion

        #region Other
        Face = 400,
        Ears,
        ChestBoard,
        LeftFoot,
        RightFoot,
        USSensor,
        #endregion
    }

    public enum SelfJointStateType
    {
        None,
        Number,
        UpDown
    }

    public enum SelfEmotionSend
    {
        None = 0,

        #region Posture Information
        posture_sit = 1,
        posture_stand,
        posture_lay,
        posture_crouch,
        posture_rest,
        posture_wake_up,
        #endregion

        #region All System Action
        upgrade_self = 100,
        system_shutdown,
        system_reboot,
        #endregion

        #region Audio Actions
        volume_down = 200,
        volume_up,
        volume_shout,
        #endregion
    }


    #endregion

    #region Enum for All Emotions
    public enum EmotionType
    {
        show_happiness,
        posture_crouch,
        show_laugh,
    }
    #endregion

    #region Data Model For WebSocket Connection Basic Classes

    [fsObject]
    public class SelfTopic
    {
        public enum SelfTopicType
        {
            None,
            Event
        }

        public string topicId { get; set;}
        public string type { get; set;}

        public SelfTopic()
        {
            this.topicId = null;
            this.type = null;
        }

        public SelfTopic(string topicId, string type)
        {
            this.topicId = topicId;
            this.type = type;
        }
    }

    [fsObject]
    public class SelfMessage
    {
        public enum RobotMessageType
        {
            None,
            Failed,
            Authentication,
            Query,
            Publish
        }

        //Generic Message Parts
        public string failed_msg { get; set;}

        //Generic Message Parts
        public string groupId { get; set;}
        public string name { get; set;}
        public string selfId { get; set;}
        public string msg { get; set;}
        public string[] targets { get; set;}
        public string origin { get; set;}
        public string type { get; set;}
        public string topic { get; set;}
        //Authentication Message Part
        public string control { get; set;}

        //Topic Query Response
        public string[] children { get; set;}
        public string request { get; set;}
        public SelfTopic[] topics { get; set;}

        //Public Response
        public string data {get;set;}
        public bool persisted {get;set;}

        //For internal brain use only
        [fsIgnore]
        public double timeAtBrainReceive {get;set;}

        private RobotMessageType m_MessageType = RobotMessageType.None;
        public RobotMessageType MessageType
        {
            get
            {
                if (m_MessageType == RobotMessageType.None)
                {

                    if (!string.IsNullOrEmpty(failed_msg))
                    {
                        m_MessageType = RobotMessageType.Failed;
                    }
                    else if (!string.IsNullOrEmpty(control) && !string.IsNullOrEmpty(groupId) && !string.IsNullOrEmpty(selfId) && control == "authenticate")
                    {
                        m_MessageType = RobotMessageType.Authentication;
                    }
                    else if (!string.IsNullOrEmpty(msg) && (msg == "query"  || msg == "query-response" || msg == "query_response") )
                    {
                        m_MessageType = RobotMessageType.Query;
                    }
                    else if (!string.IsNullOrEmpty(msg) && msg == "publish")
                    {
                        m_MessageType = RobotMessageType.Publish;
                    }
                    else
                    {
                        m_MessageType = RobotMessageType.None;
                    }
                }
                return m_MessageType;
            }
        }

        private string m_EscapedData = null;
        public string EscapedData
        {
            get
            {
                if( string.IsNullOrEmpty(m_EscapedData) && !string.IsNullOrEmpty(data))
                    m_EscapedData = data.Replace("\n", "").Replace("\\\"", "\"");

                return m_EscapedData;
            }
        }

        private string m_GUID = null;
        public string GUID
        {
            get
            {
                if (string.IsNullOrEmpty(m_GUID) && HasSubscriptionData)
                {
                    m_GUID = SubscriptionData.thing.m_GUID;
                }
                return m_GUID;
            }
        }

        private SelfSubscriptionData m_SelfSubscriptionData = null;
        public SelfSubscriptionData SubscriptionData
        {
            get
            {
                if (m_SelfSubscriptionData == null)
                {
                    if (!string.IsNullOrEmpty(data) && topic == "blackboard")
                    {
                        m_SelfSubscriptionData = Utility.DeserializeResponse<SelfSubscriptionData>(EscapedData);

                        #if ENABLE_DEBUGGING
                        Log.Debug("SelfMessage", "Escaped data: {0}", EscapedData );
                        #endif
                    }
                    else
                    {
                        #if ENABLE_DEBUGGING
                        Log.Debug("SelfMessage", "Data is null. There is no subscription data conversion.");
                        #endif
                    }
                }
                return m_SelfSubscriptionData;
            }
            set
            {
                m_SelfSubscriptionData = value;
            }
        }

        private SelfBody m_SelfBodyData = null;
        public SelfBody SelfBodyData
        {
            get
            {
                if (m_SelfBodyData == null)
                {
                    if (!string.IsNullOrEmpty(data) && topic == "body")
                    {
                        m_SelfBodyData = Utility.DeserializeResponse<SelfBody>(EscapedData);

                        #if ENABLE_DEBUGGING
                        Log.Debug("SelfMessage", "Escaped data: {0}", EscapedData );
                        #endif
                    }
                    else
                    {
                        #if ENABLE_DEBUGGING
                        Log.Debug("SelfMessage", "Data is null. There is no self body data conversion.");
                        #endif
                    }
                }
                return m_SelfBodyData;
            }
        }

        public bool HasSubscriptionData
        {
            get
            {
                return !string.IsNullOrEmpty(data) && topic == "blackboard";
            }
        }

        public bool HasBodyData
        {
            get
            {
                return !string.IsNullOrEmpty(data) && topic == "body";
            }
        }

        public bool HasFullData
        {
            get
            {
                return !string.IsNullOrEmpty(selfId) && !string.IsNullOrEmpty(name);
            }
        }

        public bool HasTopics
        {
            get
            {
                return topics != null && topics.Length > 0;
            }
        }

        public override string ToString()
        {
            string dataString = HasSubscriptionData ? SubscriptionData.ToString() : (HasBodyData ? SelfBodyData.ToString() : "");
            return string.Format("[SelfMessage: failed_msg={0}, groupId={1}, name={2}, selfId={3}, msg={4}, targets={5}, origin={6}, type={7}, topic={8}, control={9}, children={10}, request={11}, topics={12}, \nEscapedData={13}, persisted={14}, MessageType={15}, HasSubscriptionData={16}, HasBodyData={17}, HasFullData={18}, HasTopics={19}, \n DeserializedData={20}]", failed_msg, groupId, name, selfId, msg, ( (targets == null) ? "" : string.Join(",",targets)), origin, type, topic, control, ( (children == null) ? "": string.Join(",",children)), request, topics, (string.IsNullOrEmpty(EscapedData)? "" : EscapedData.Replace("{","[").Replace("}","]")), persisted, MessageType, HasSubscriptionData, HasBodyData, HasFullData, HasTopics, dataString);
        }
    }

   


    #endregion

    #region Data Model For Self Subscription Data Model with Custom Serializer

    [fsObject(Converter = typeof(SelfSubscriptionDataConverter))]
    public class SelfSubscriptionData
    {
        public string @event { get; set;}
        public string parent { get; set;}
        [fsIgnore]
        public SelfThing[] m_Children { get; set;}
        [fsIgnore]
        public SelfThing thing { get; set;}

        public override string ToString()
        {
            return string.Format("[SelfSubscriptionData: event={0}, parent={1}, thing={2}]", @event, parent, thing);
        }

        public bool HasData
        {
            get{
                return thing != null && thing.HasData;
            }
        }

        public bool IsAdded
        {
            get
            {
                return !string.IsNullOrEmpty(@event) && @event == "ADDED";
            }
        }

        public bool IsRemoved
        {
            get
            {
                return !string.IsNullOrEmpty(@event) && @event == "REMOVED";
            }
        }

    }

    public class SelfSubscriptionDataConverter : fsConverter {
        private static fsSerializer sm_Serializer = new fsSerializer();

        private static Type[] sm_Types = null; 

        public override bool CanProcess(Type type) {
            return type == typeof(SelfSubscriptionData);
        }

        public override fsResult TrySerialize(object instance,
            out fsData serialized, Type storageType) {


            SelfSubscriptionData myType = (SelfSubscriptionData)instance;

            Dictionary<string, fsData> currentSerialization = new Dictionary<string, fsData>();

            currentSerialization.Add("event", new fsData(myType.@event)) ;
            //currentSerialization.Add("parent", new fsData(myType.parent));

            if (myType.thing != null)
            {
                string thingType = myType.thing.GetThingType();
                fsData tempData = null;

                if (sm_Types == null)
                {
                    sm_Types = Utilities.Utility.FindAllDerivedTypes(typeof(SelfThing));
                }
               
                foreach (Type currentType in sm_Types)
                {
                    try
                    {
                        System.Object selfThingObject = Activator.CreateInstance(currentType);
                        SelfThing selfThing = selfThingObject as SelfThing;
                        if(selfThing.GetThingType() == thingType)
                        {
                            sm_Serializer.TrySerialize(currentType, myType.thing, out tempData);
                            break;
                        }
                    }
                    catch (Exception)
                    { 
                    }
                }

                if (tempData != null)
                {
                    currentSerialization.Add("thing", tempData);
                }

            }

            serialized = new fsData(currentSerialization);

            //serialized = new fsData(myType.Value);
            return fsResult.Success;
        }

        public override fsResult TryDeserialize(fsData storage,
            ref object instance, Type storageType) {

            if (storage.Type != fsDataType.Object) {
                return fsResult.Fail("Expected object fsData type but got " + storage.Type);
            }

            SelfSubscriptionData myType = (SelfSubscriptionData)instance;

            Dictionary<string, fsData> deserializedObject = storage.AsDictionary;

            myType.@event = deserializedObject["event"].AsString;

            if(deserializedObject.ContainsKey("parent"))
                myType.parent = deserializedObject["parent"].AsString;

            if (deserializedObject.ContainsKey("m_Children"))
            {
                List<fsData> listChildrenObject = deserializedObject["m_Children"].AsList;
                List<SelfThing> listChildrenDesirialized = new List<SelfThing>();
                for (int i = 0; listChildrenObject != null && i < listChildrenObject.Count; i++)
                {
                    Dictionary<string, fsData> thingObjectFromChildren = listChildrenObject[i].AsDictionary;
                    string thingTypeChild = thingObjectFromChildren["Type_"].AsString;


                    if (sm_Types == null)
                    {
                        sm_Types = Utilities.Utility.FindAllDerivedTypes(typeof(SelfThing));
                    }

                    foreach (Type currentType in sm_Types)
                    {
                        try
                        {
                            System.Object selfThingObject = Activator.CreateInstance(currentType);
                            SelfThing selfThing = selfThingObject as SelfThing;
                            if(selfThing.GetThingType() == thingTypeChild)
                            {
                                sm_Serializer.TryDeserialize(deserializedObject["thing"], currentType, ref selfThingObject);
                                listChildrenDesirialized.Add(selfThingObject as SelfThing);
                                break;
                            }
                        }
                        catch (Exception e)
                        { 
                            Log.Warning("SelfSubscriptionDataConverter", "Exception on Type from child: {0}", e.Message);
                        }
                    }
                }
                myType.m_Children = listChildrenDesirialized.ToArray();
            }

            Dictionary<string, fsData> thingObject = deserializedObject["thing"].AsDictionary;
            string thingType = thingObject["Type_"].AsString;

            if (sm_Types == null)
            {
                sm_Types = Utilities.Utility.FindAllDerivedTypes(typeof(SelfThing));
            }

            foreach (Type currentType in sm_Types)
            {
                try
                {
                    System.Object selfThingObject = Activator.CreateInstance(currentType);
                    SelfThing selfThing = selfThingObject as SelfThing;
                    if(selfThing.GetThingType() == thingType)
                    {

                        //serializer.Tr.(deserializedObject["thing"], ref selfThing,currentType);
                        sm_Serializer.TryDeserialize(deserializedObject["thing"], currentType, ref selfThingObject);
                        //Deserialize object to our current self thing

                        myType.thing = selfThingObject as SelfThing;
                        break;
                    }
                }
                catch (Exception e)
                { 
                    Log.Warning("SelfSubscriptionDataConverter", "Exception on Type: {0}", e.Message);
                }
            }

            return fsResult.Success;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return new SelfSubscriptionData();
        }

    }

    #endregion

    #region Self Thing Class Definition 

    //[fsObject(MemberSerialization = fsMemberSerialization.OptOut)]
    public abstract class SelfThing
    {
        private static SelfThingConstants ms_constants;
        public static SelfThingConstants Constants
        {
            get {
                if (ms_constants != null)
                {
                    return ms_constants;
                }
                else
                {
                    Log.Error("SelfThing", "SelfThing Constants are null");
                    return null;
                }
            }
            set { ms_constants = value; }
        }

        #region All Constant String Formats - Colors
        /*
        public const string FORMAT_SENSOR_TEMPERATURE =             "Device/SubDeviceList/{0}/Temperature/Sensor/Value";
        public const string FORMAT_SENSOR_ANGLE_POSITION =          "Device/SubDeviceList/{0}/Position/Sensor/Value"; 
        public const string FORMAT_SENSOR_GYROSCOPE =               "Device/SubDeviceList/InertialSensor/Gyroscope{0}/Sensor/Value"; 
        public const string FORMAT_DIAGNOSIS_ACTIVE_ERROR =         "Diagnosis/Active/{0}/Error"; 
        public const string FORMAT_DIAGNOSIS_PASSIVE_ERROR =        "Diagnosis/Passive/{0}/Error"; 
        public const string FORMAT_DIAGNOSIS_TEMPERATURE_ERROR =    "Diagnosis/Temperature/{0}/Error"; 
        public const string FORMAT_DIAGNOSIS_TOUCH_ERROR =          "Diagnosis/Active/{0}/Touch/Error";
        public const string FORMAT_BATTERY_CHARGE =                 "batteryCharge"; //Device/SubDeviceList/Battery/Charge/Sensor/Value";
        public const string FORMAT_BATTERY_CHARGING =               "batteryCharging";
        public const string FORMAT_FREE_MEMORY =                    "freeMemory";
        public const string FORMAT_CPU_USAGE =                      "cpuUsage";
        public const string FORMAT_DISK_USAGE =                     "diskUsage";
        public const string FORMAT_LAST_REBOOT =                    "lastReboot";
        public const string FORMAT_SYSTEM_INFO =                    "System";
        public const string FORMAT_POSTURE_CHANGE =                 "PostureChanged";
        public const string FORMAT_REMOTE_NETWORK =                 "RemoteNetwork";
        public const string FORMAT_NETWORK =                        "Network";
        public const string FORMAT_ROBOT_HAS_FALLEN =               "robotHasFallen";
        public const string FORMAT_SYSTEM_VERSION =                 "systemVersion";
        public const string FORMAT_TIME_ZONE =                      "timezone";
        public const string FORMAT_VOLUME =                         "volume";
        public const string FORMAT_AUDIO_MUTE =                     "audioOut";
        */

        public const float TEMPERATURE_LIMIT_LOW = 20.0f;
        public const float TEMPERATURE_LIMIT_HIGH = 60.0f;

        public const float TIMEOUT_SHOW_WARNING = 60.0f; //If there is warning then it will go away if there is no other warning in this timeout time

        public static Color COLOR_WARNING_NONE =    new Color(  69 / 255.0f,    197/ 255.0f,    76 / 255.0f,    225 / 255.0f);
        public static Color COLOR_WARNING_LOW =     new Color(  94 / 255.0f,    151/ 255.0f,    191/ 255.0f,    225 / 255.0f);
        public static Color COLOR_WARNING_MEDIUM =  new Color(  229/ 255.0f,    224/ 255.0f,    44 / 255.0f,    225 / 255.0f);
        public static Color COLOR_WARNING_HIGH =    new Color(  212/ 255.0f,    91 / 255.0f,    12 / 255.0f,    225 / 255.0f);
        public static Color COLOR_WARNING_ERROR =   new Color(  253/ 255.0f,    47 / 255.0f,    47 / 255.0f,    225 / 255.0f);
        public static Color COLOR_WARNING_CRITICAL = new Color(  133/ 255.0f,    0 / 255.0f,    0 / 255.0f,    225 / 255.0f);

        public static Dictionary<SensorWarningLevel, bool> WarningFilterLevel = new Dictionary<SensorWarningLevel, bool>();
        public static List<SelfSensorType> SensorListToSubscribe = new List<SelfSensorType>();

        #endregion

        #region Public Members for Each Self Thing

        public string Type_ { get; set;}
        public double m_CreateTime { get; set;}
        public string m_GUID { get; set;}
        public int m_fImportance { get; set;}
        public string m_State { get; set;}

        public bool HasData
        {
            get{
                return !string.IsNullOrEmpty(m_GUID);
            }
        }

        private DateTime _CreateTime = default(DateTime);
        public DateTime CreateTime
        {
            get{
                if (_CreateTime == default(DateTime))
                {
                    _CreateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    _CreateTime = _CreateTime.AddSeconds(m_CreateTime);
                }
                return _CreateTime;
            }
        }

        public abstract string GetThingType();
        public abstract string GetText();

        #endregion

        #region Static Functions for all Self Thing objects

        public static IService[] m_ServiceNames;

        public static bool IsServiceName(string serviceName){
            bool isService = false;

            if (m_ServiceNames != null && m_ServiceNames.Length != 0)
            {
                foreach (IService service in m_ServiceNames)
                {
                    if (serviceName == service.m_ServiceId)
                    {
                        isService = true;
                        break;
                    }
                }
            }
            else
            {
                Log.Error("SelfThing", "IsServiceName - Array was not set up correctly at time of parsing body.json : {0}", (m_ServiceNames != null? m_ServiceNames.Length.ToString() : "NULL"));
            }

            return isService;
        }

        public static bool IsEnableWarningFilter(SensorWarningLevel sensorWarningLevel)
        {
            bool isEnable = false;
            if (WarningFilterLevel.ContainsKey(sensorWarningLevel))
            {
                isEnable = WarningFilterLevel[sensorWarningLevel];
            }
            else
            {
                Log.Warning("SelfThing", "IsEnableWarningFilter - doesn't have sensor warning level : {0}", sensorWarningLevel);
            }
            return isEnable;
        }

        private static Dictionary<SelfSensorType, string> ms_SensorNames= null;
        public static string GetSensorName(SelfSensorType jointType)
        {
            string jointName = null;
            if (ms_SensorNames == null)
            {
                ms_SensorNames = new Dictionary<SelfSensorType, string>();

                foreach (SelfSensorType item in Enum.GetValues(typeof(SelfSensorType)))
                {
                    ms_SensorNames.Add(item, item.ToString());
                }
            }

            ms_SensorNames.TryGetValue(jointType, out jointName);

            return jointName;
        }

        private static Dictionary<SelfEmotionSend, string> ms_EmotionNames= null;
        public static string GetEmotionName(SelfEmotionSend emotion)
        {
            string emotionName = null;
            if (ms_EmotionNames == null)
            {
                ms_EmotionNames = new Dictionary<SelfEmotionSend, string>();

                foreach (SelfEmotionSend item in Enum.GetValues(typeof(SelfEmotionSend)))
                {
                    ms_EmotionNames.Add(item, item.ToString());
                }
            }

            ms_EmotionNames.TryGetValue(emotion, out emotionName);

            return emotionName;
        }

        public static string GetShortSensorName(SelfSensorType sensorType)
        {
            string jointName = GetSensorName(sensorType);
            if (!string.IsNullOrEmpty(jointName))
            {
                jointName = jointName.Replace("Right", "R").Replace("Left", "L");
            }
            return jointName;
        }

        public static string GetPrettySensorName(SelfSensorType sensorType)
        {
            return AddSpacesBeforeCapitalLetter(GetSensorName(sensorType));
        }

        public static SelfSensorType GetJointType(string sensorName)
        {
            SelfSensorType sensorType = SelfSensorType.None;
            string jointNameToSearch = sensorName;

            if (ms_SensorNames == null)
            {
                ms_SensorNames = new Dictionary<SelfSensorType, string>();

                foreach (SelfSensorType item in Enum.GetValues(typeof(SelfSensorType)))
                {
                    ms_SensorNames.Add(item, item.ToString());
                }
            }
             
            //Look for unformatted name first
            if (ms_SensorNames.ContainsValue(jointNameToSearch))
            {
                sensorType = ms_SensorNames.FirstOrDefault(x => x.Value == jointNameToSearch).Key;
            }
            else
            {
                //If if doesn't find the unformmated name of sensor, format it and check again
                if (jointNameToSearch.IndexOf("Left") < 0 && jointNameToSearch.IndexOf("L") == 0)
                {
                    jointNameToSearch = "Left" + jointNameToSearch.Substring(1);
                } 
                else if (jointNameToSearch.IndexOf("Right") < 0 && jointNameToSearch.IndexOf("R") == 0)
                {
                    jointNameToSearch = "Right" + jointNameToSearch.Substring(1);
                }

                if (ms_SensorNames.ContainsValue(jointNameToSearch))
                {
                    sensorType = ms_SensorNames.FirstOrDefault(x => x.Value == jointNameToSearch).Key;
                }
                else
                {
                    Log.Warning("SelfThing", "Joint name couldn't find in the dictionary {0}", sensorName);
                }
            }

            return sensorType;
        }

        public static string AddSpacesBeforeCapitalLetter(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                    newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }


        #endregion
    }


    #endregion

    #region Self Thing - all custom class definition per Self c++

    [fsObject]
    public class SelfThingText : SelfThing
    {

        public override string GetThingType() 
        {
            return "Text";
        }

        public override string GetText() 
        {
            return m_Text;
        }

        public SelfThingText()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_fConfidence = 1.0f;
            m_Language = "en-US";
            m_ClassifyIntent = true;
            m_GUID = "";
            m_State = "";
        }

        public SelfThingText(string text)
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_fConfidence = 1.0f;
            m_Language = "en-US";
            m_ClassifyIntent = true;
            m_GUID = "";
            m_State = "";
            m_Text = text;
        }

        public string m_Text { get; set;}
        public bool m_LocalDialog { get; set;}
        public bool m_ClassifyIntent { get; set;}
        public string m_Language { get; set;}
        public float m_fConfidence { get; set; }

        public override string ToString()
        {
            return string.Format("[SelfThingText: Type_={0}, m_CreateTime={1}, m_GUID={2}, HasData={3}, m_Text={4}, m_LocalDialog={5}, m_ClassifyIntent={6}, m_Language={7}]", Type_, m_CreateTime, m_GUID, HasData, m_Text, m_LocalDialog, m_ClassifyIntent, m_Language);
        }
    }

    [fsObject]
    public class SelfThingUrl : SelfThing
    {

        public override string GetThingType() 
        {
            return "Url";
        }

        public override string GetText() 
        {
            return m_URL;
        }

        public SelfThingUrl()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public string m_URL { get; set;}

        public override string ToString()
        {
            return string.Format("[SelfThingText: Type_={0}, m_CreateTime={1}, m_GUID={2}, HasData={3}, m_URL={4}", Type_, m_CreateTime, m_GUID, HasData, m_URL);
        }
    }

    [fsObject]
    public class SelfThingSay : SelfThing
    {
        public override string GetThingType() 
        {
            return "Say";
        }
        public override string GetText() 
        {
            return m_Text;
        }

        public SelfThingSay()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }


        public enum SayState
        {
            Unknown,
            Processing,
            Completed,
        }

        public string m_Text { get; set;}
        public bool m_VoiceOverride { get; set;}
        private SayState m_ThingSayState = SayState.Unknown;

        public SayState ThingSayState
        {
            get
            {
                if (m_ThingSayState == SayState.Unknown)
                {
                    if (!string.IsNullOrEmpty(m_State) && m_State == "PROCESSING")
                    {
                        m_ThingSayState = SayState.Processing;
                    }
                    else if (!string.IsNullOrEmpty(m_State) && m_State == "COMPLETED")
                    {
                        m_ThingSayState = SayState.Completed;
                    }
                    else
                    {
                        Log.Warning("SelfThingSay", "Unknown say state: {0}", m_State);
                        m_ThingSayState = SayState.Unknown;
                    }
                }

                return m_ThingSayState;
            }
        }

        public override string ToString()
        {
            return string.Format("[SelfThingSay: Type_={0}, m_CreateTime={1}, m_GUID={2}, HasData={3}, m_Text={4}]", Type_, m_CreateTime, m_GUID, HasData, m_Text);
        }
    }

    #region SelfIntent
    [fsObject]
    public abstract class SelfIntent : SelfThing
    {
        public double m_Confidence { get; set; }
    }

    [fsObject]
    public class SelfThingHangOnIntent : SelfIntent
    {
        public override string GetThingType()
        {
            return "HangOnIntent";
        }
        public override string GetText() 
        {
            return GetThingType();
        }

        public SelfThingHangOnIntent()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public override string ToString()
        {
            return string.Format("[SelfThingHangOnIntent: Type_={0}, m_CreateTime={1}, m_GUID={2}, HasData={3}]", Type_, m_CreateTime, m_GUID, HasData);
        }
    }

    [fsObject]
    public class SelfThingWeatherIntent : SelfIntent
    {
        public override string GetThingType()
        {
            return "WeatherIntent";
        }
        public override string GetText() 
        {
            return GetThingType();
        }

        public SelfThingWeatherIntent()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public override string ToString()
        {
            return string.Format("[SelfThingWeatherIntent: Type_={0}, m_CreateTime={1}, m_GUID={2}, HasData={3}]", Type_, m_CreateTime, m_GUID, HasData);
        }
    }

    [fsObject]
    public class SelfThingLearningIntent : SelfIntent
    {
        public override string GetThingType()
        {
            return "LearningIntent";
        }
        public override string GetText() 
        {
            return GetThingType();
        }

        public SelfThingLearningIntent()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }
            
        public string m_Text { get; set; }
        public string m_TextParse { get; set;}
        public string m_Target { get; set;}
        public string m_Verb { get; set; }

        public override string ToString()
        {
            return string.Format("[SelfThingLearningIntent: Type_={0}, m_CreateTime={1}, m_GUID={2}, HasData={3}, m_Text={4}, m_TextParse={5}, m_Target={6}, m_Verb={7}]", Type_, m_CreateTime, m_GUID, HasData, m_Text, m_TextParse, m_Target, m_Verb);
        }
    }

    [fsObject]
    public class SelfThingRequestIntent : SelfIntent
    {
        public override string GetThingType()
        {
            return "RequestIntent";
        }
        public override string GetText() 
        {
            return m_Text;
        }

        public SelfThingRequestIntent()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }


        public string m_Name { get; set; }
        public SelfThingRequest[] m_Requests { get; set; }
        public string m_Text { get; set; }
        public string m_TextParse { get; set;}
        public string m_Type { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0;m_Requests != null && i < m_Requests.Length; i++)
            {
                stringBuilder.Append(m_Requests[i].ToString());   
            }

            return string.Format("[SelfThingRequestIntent: m_Name={0}, m_Requests={1}, m_Text={2}, m_TextParse={3}, m_Type={4}]", m_Name, stringBuilder.ToString(), m_Text, m_TextParse, m_Type);
        }
    }
    #endregion


    [fsObject]
    public class SelfThingRequest
    {
        public string m_Target { get; set;}
        public string m_Verb { get; set; }

        public override string ToString()
        {
            return string.Format("[SelfThingRequest: m_Target={0}, m_Verb={1}]", m_Target, m_Verb);
        }
    }

    [fsObject]
    public class SelfThingGoal : SelfThing
    {
        public override string GetThingType()
        {
            return "Goal";
        }
        public override string GetText() 
        {
            return m_Text;
        }

        public SelfThingGoal()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
        }


        public string m_Name { get; set; }
        public string m_Text { get; set; }
        public SelfThingGoalParams m_Params { get; set; }

        public override string ToString()
        {
            return string.Format("[SelfThingGoal: Type_={0}, m_CreateTime={1}, m_GUID={2}, HasData={3}, m_Text={4}, m_Params={5}]", Type_, m_CreateTime, m_GUID, HasData, m_Text, (m_Params != null)? m_Params.ToString() : "NULL");
        }
    }

    [fsObject]
    public class SelfThingGoalParams
    {
        public string Type_ { get; set; }
        public string intent { get; set; }
        public string target { get; set; }
        public string verb { get; set; }
        public SelfThingGoalParamQuestion question { get; set; }
        public SelfThingGoalParamAnswer answer { get; set; }

        public override string ToString()
        {
            return string.Format("[SelfThingGoalParams: Type_={0}, intent={1}, target={2}, verb={3}, \nquestion={4}, \nanswer={5}]", Type_, intent, target, verb, question, answer);
        }
    }
    [fsObject]
    public class SelfThingGoalParamAnswer
    {
        public long client_id { get; set; }
        public float confidence { get; set; }
        public long conversation_id { get; set; }
        public string dialogId { get; set; }
        public string input { get; set; }
        public string[] response { get; set; }
        public double timestamp { get; set; }

        public override string ToString()
        {
            return string.Format("[SelfThingGoalParamAnswer: client_id={0}, confidence={1}, conversation_id={2}, dialogId={3}, input={4}, response={5}, timestamp={6}]", client_id, confidence, conversation_id, dialogId, input, response, timestamp);
        }
    }

    [fsObject]
    public class SelfThingGoalParamQuestion
    {
        public SelfThing[] m_Children { get; set;}
        public double m_CreateTime { get; set;}
        public string m_GUID { get; set;}
        public string m_Pipeline { get; set;}
        public string m_State { get; set;}
        public string m_Text { get; set;}
        public bool m_bLocalDialog { get; set;}
        public float m_fImportance { get; set;}

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0;m_Children != null && i < m_Children.Length; i++)
            {
                stringBuilder.Append(m_Children[i].ToString());   
            }
            return string.Format("[SelfThingGoalParamQuestion: m_Children={0}, m_CreateTime={1}, m_GUID={2}, m_Pipeline={3}, m_State={4}, m_Text={5}, m_bLocalDialog={6}, m_fImportance={7}]", stringBuilder.ToString(), m_CreateTime, m_GUID, m_Pipeline, m_State, m_Text, m_bLocalDialog, m_fImportance);
        }
    }

    [fsObject]
    public class SelfThingHealth : SelfThing
    {
        public override string GetThingType() 
        {
            return "Health";
        }
        public override string GetText() 
        {
            return m_HealthName;
        }

        public SelfThingHealth()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }


        public string m_HealthName { get; set;}
        //TODO: m_bError is given in class declaration so why it is not visible in Json?
        //public bool m_bError { get; set; }
        public float m_fHealthValue { get; set;}

        public enum DataType
        {
            None,
            Service,
            Network,
            SystemVersion,
            TimeZone,
            Memory,
            Volume,
            AudioMute,
            RobotFallen,
            Temperature,
            Posture,
            Battery,
            BatteryCharging,
            Angle,
            Gyroscope,
            DiagnosisActiveTouch,
            DiagnosisActive,
            DiagnosisPassive,
            DiagnosisTemperature,
            ErrorLed,
            System,
            CpuUsage,
            DiskUsage,
            LastReboot,
            FreeMemory,
            /// <summary>
            /// FSR stands for Force Sensitive Resistors.
            /// </summary>
            ErrorFSR
        }

        private DataType m_TypeOfData = DataType.None;
        public DataType TypeOfData
        {
            get
            {
                return Constants.GetDataType(m_HealthName, m_State);
            }
        }

        public float Temperature
        {
            get
            {
                return m_fHealthValue;
            }
        }

        public float TemperatureInF
        {
            get
            {
                return ((9.0f / 5.0f) * m_fHealthValue) + 32;
            }
        }

        public float NormalizedTemperature
        {
            get
            {
                float normalizedTemp = 0.5f;
                float tempTemperature = Temperature;
                if (Temperature > 0)
                {
                    normalizedTemp = UnityEngine.Mathf.Clamp01( (tempTemperature - TEMPERATURE_LIMIT_LOW) / (TEMPERATURE_LIMIT_HIGH - TEMPERATURE_LIMIT_LOW));
                }
                return normalizedTemp;
            }
        }

        public Color WarningLevelColor
        {
            get{
                SensorWarningLevel warningLevel = WarningLevel;
                Color warningColor = Color.white;
                if (warningLevel == SensorWarningLevel.None)
                {
                    warningColor = COLOR_WARNING_NONE;
                }
                else if (warningLevel == SensorWarningLevel.Low)
                {
                    warningColor = COLOR_WARNING_LOW;
                }
                else if (warningLevel == SensorWarningLevel.Medium)
                {
                    warningColor = COLOR_WARNING_MEDIUM;
                }
                else if (warningLevel == SensorWarningLevel.High)
                {
                    warningColor = COLOR_WARNING_HIGH;
                }
                else if (warningLevel == SensorWarningLevel.Error)
                {
                    warningColor = COLOR_WARNING_ERROR;
                }
                else if (warningLevel == SensorWarningLevel.Critical)
                {
                    warningColor = COLOR_WARNING_CRITICAL;
                }
                else
                {
                    Log.Debug("SelfThingHealth", "Unknown Warning Level for WarningLevelColor: {0}", m_HealthName);
                }

                return warningColor;
            }
        }

        public SensorWarningLevel WarningLevel
        {
            get
            {
                //NEGLIGIBLE, SERIOUS or CRITICAL
                SensorWarningLevel warningLevel = SensorWarningLevel.NaN;

                if (TypeOfData == DataType.Temperature)
                {
                    if (Temperature < TEMPERATURE_LIMIT_LOW)
                    {
                        warningLevel = SensorWarningLevel.Low;
                    }
                    else if (Temperature < TEMPERATURE_LIMIT_HIGH)
                    {
                        warningLevel = SensorWarningLevel.None;
                    }
                    else
                    {
                        warningLevel = SensorWarningLevel.Medium;
                    }
                }
                else if (m_State == "NEGLIGIBLE")
                {
                    warningLevel = SensorWarningLevel.None;
                }
                else if (m_State == "SERIOUS")
                {
                    warningLevel = SensorWarningLevel.High;
                }
                else if (m_State == "CRITICAL")
                {
                    warningLevel = SensorWarningLevel.Critical;
                }
                else if (m_State == "UP")
                {
                    warningLevel = SensorWarningLevel.None;
                }
                else if (m_State == "DOWN")
                {
                    warningLevel = SensorWarningLevel.Error;
                }
                else if (TypeOfData == DataType.Angle 
                    || TypeOfData == DataType.Battery 
                    || TypeOfData == DataType.SystemVersion
                    || TypeOfData == DataType.TimeZone
                    || TypeOfData == DataType.AudioMute
                    || TypeOfData == DataType.Memory
                    || TypeOfData == DataType.Posture
                    || TypeOfData == DataType.Volume
                    || TypeOfData == DataType.RobotFallen
                    || TypeOfData == DataType.Gyroscope
                    || TypeOfData == DataType.BatteryCharging
                    || TypeOfData == DataType.Network)
                {
                    warningLevel = SensorWarningLevel.NaN;
                }
                else
                {
                    Log.Warning("SelfThingHealth", "Unknown Sensor Warning Level : {0}", m_HealthName);
                }

                return warningLevel;
            }
        }

        private string m_WarningLevelTitle = null;
        public string WarningLevelTitle
        {
            get
            {
                if (string.IsNullOrEmpty(m_WarningLevelTitle))
                {
                    string prefixWarning = "";
                    string postfixWarning = "";
                    string formatWarning = "{0} on {1}";

                    switch (WarningLevel)
                    {
                        case SensorWarningLevel.Critical:
                            prefixWarning = "Critical";
                            break;
                        case SensorWarningLevel.Error:
                            prefixWarning = "Error";
                            break;
                        case SensorWarningLevel.High:
                            prefixWarning = "High Warning";
                            break;
                        case SensorWarningLevel.Medium:
                            prefixWarning = "Medium Warning";
                            break;
                        case SensorWarningLevel.Low:
                            prefixWarning = "Low Warning";
                            break;
                        case SensorWarningLevel.None:
                            prefixWarning = "No Warning";
                            break;
                        default:
                            break;
                    }

                    switch (TypeOfData)
                    {
                        case DataType.Angle:
                            postfixWarning = "Joint";
                            break;
                        case DataType.Battery:
                            postfixWarning = "Battery";
                            break;
                        case DataType.Gyroscope:
                            postfixWarning = "Gyroscope";
                            break;
                        case DataType.SystemVersion:
                            postfixWarning = "System Version";
                            break;
                        case DataType.TimeZone:
                            postfixWarning = "Time Zone";
                            break;
                        case DataType.DiagnosisActiveTouch:
                            postfixWarning = "Active Diagnosis Touch";
                            break;
                        case DataType.DiagnosisActive:
                            postfixWarning = "Active Diagnosis";
                            break;
                        case DataType.DiagnosisPassive:
                            postfixWarning = "Passive Diagnosis";
                            break;
                        case DataType.DiagnosisTemperature:
                            postfixWarning = "Temperature Diagnosis";
                            break;
                        case DataType.ErrorFSR:
                            postfixWarning = "FSR";
                            break;
                        case DataType.ErrorLed:
                            postfixWarning = "LED";
                            break;
                        case DataType.Posture:
                            postfixWarning = "Posture";
                            break;
                        case DataType.Temperature:
                            postfixWarning = "Temperature";
                            break;
                        case DataType.None:
                            postfixWarning = " - ";
                            break;
                        default:
                            break;
                    }

                    m_WarningLevelTitle = string.Format(formatWarning, prefixWarning, postfixWarning);
                }

                return m_WarningLevelTitle;
            }
        }


        private SelfSensorType m_SensorType = SelfSensorType.None;
        [fsIgnore]
        public SelfSensorType SensorType
        { 
            get
            {
                //TODO: Add all possible healthname to Sensor Type conversion
                if (m_SensorType == SelfSensorType.None)
                {
                    
                    if (!string.IsNullOrEmpty(m_HealthName))
                    {
                        if (TypeOfData == DataType.Angle || TypeOfData == DataType.DiagnosisTemperature || TypeOfData == DataType.ErrorFSR || TypeOfData == DataType.DiagnosisActive || TypeOfData == DataType.DiagnosisPassive  || TypeOfData == DataType.Temperature)
                        {
                            string jointName = "";

                            if (Constants.SENSORNAME_NUMOFSLASHESBEFORENAME == 0)
                            {
                                int indexOfTrailingSlash = m_HealthName.IndexOf("/");

                                jointName = m_HealthName.Substring(0, indexOfTrailingSlash);
                            }
                            else
                            {
                                int[] indicies = Enumerable.Range(0, m_HealthName.Length).Where(x=>m_HealthName[x].ToString() == "/").ToArray();

                                int indexOfSlashBeforeName = indicies[Constants.SENSORNAME_NUMOFSLASHESBEFORENAME-1];
                                int indexOfTrailingSlash = indicies[Constants.SENSORNAME_NUMOFSLASHESBEFORENAME];

                                jointName = m_HealthName.Substring(indexOfSlashBeforeName + 1, (indexOfTrailingSlash - indexOfSlashBeforeName) - 1);
                            }

                            if (!string.IsNullOrEmpty(jointName))
                            {
                                m_SensorType = GetJointType(jointName);
                            }
                            else
                            {
                                m_SensorType = SelfSensorType.None;
                            }
                        }
                        else if (TypeOfData == DataType.Battery)
                        {
                            m_SensorType = SelfSensorType.Battery;
                        }
                        else
                        {
                            //do nothing - data type is non so it is not a joint type. No need to make a conversion
                        }
                    }
                    else
                    {
                        Log.Warning("SelfThingHealth", "m_HealthName is null or empty. JointType can't be retrieved");
                    }

                }

                return m_SensorType;
            }
        }

        public string SensorName
        {
            get
            {
                return SelfThing.GetSensorName(SensorType);
            }
        }

        public string SensorNamePretty
        {
            get
            {
                return SelfThing.GetPrettySensorName(SensorType);
            }
        }

        public bool CanPassFilter
        {
            get
            {
                bool canPassfilter = false;
                if (WarningFilterLevel.ContainsKey(WarningLevel))
                {
                    canPassfilter = WarningFilterLevel[WarningLevel];
                }
                return canPassfilter;
            }
        }

        public override string ToString()
        {
            return string.Format("[SelfThingHealth: " +
                "m_HealthName={0}, " +
                "m_fHealthValue={1}, " +
                "TypeOfData={2}, " +
                "WarningLevel={3}, " +
                "WarningLevelTitle={4}, " +
                "SensorType={5}, " +
                "SensorName={6}, " +
                "CanPassFilter={7}]", 
                m_HealthName, 
                m_fHealthValue, 
                TypeOfData, 
                WarningLevel, 
                WarningLevelTitle, 
                SensorType, 
                SensorName, 
                CanPassFilter);
        }

    }

    [fsObject]
    public class SelfThingEnvironment : SelfThing
    {
        public override string GetThingType() 
        {
            return "Environment";
        }
        public override string GetText() 
        {
            return GetThingType();
        }

        public SelfThingEnvironment()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public double m_CarbonDioxide { get; set;}
        public double m_Humidity { get; set;}
        public double m_Temperature {get;set;}
        public double m_Pressure { get; set;}

        public override string ToString()
        {
            return string.Format("[SelfThingEnvironment: Type_={0}, m_CreateTime={1}, m_GUID={2}, HasData={3}, m_CarbonDioxide={4}, m_Humidity={5}, m_Temperature={6}, m_Pressure={7}]", Type_, m_CreateTime, m_GUID, HasData, m_CarbonDioxide, m_Humidity, m_Temperature, m_Pressure);
        }
    }

    [fsObject]
    public class SelfThingEntity : SelfThing
    {
        public override string GetThingType() 
        {
            return "Entity";
        }
        public override string GetText() 
        {
            return GetThingType();
        }

        public SelfThingEntity()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public override string ToString()
        {
            return string.Format("[SelfThingEntity: Type_={0}, m_CreateTime={1}, m_GUID={2}, m_State={3}, HasData={4}]", Type_, m_CreateTime, m_GUID, m_State, HasData);
        }
    }

    [fsObject]
    public class SelfThingStatus : SelfThing
    {
        public override string GetThingType() 
        {
            return "Status";
        }
        public override string GetText() 
        {
            return GetThingType();
        }

        public SelfThingStatus()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        //TODO: There is m_State as int for Status object! 
        //public int m_State { get; set;}

        public override string ToString()
        {
            return string.Format("[SelfThingStatus: Type_={0}, m_CreateTime={1}, m_GUID={2}, m_State={3}, HasData={4}]", Type_, m_CreateTime, m_GUID, m_State, HasData);
        }
    }

    [fsObject]
    public class SelfThingProximity : SelfThing
    {
        public override string GetThingType() 
        {
            return "Proximity";
        }
        public override string GetText() 
        {
            return GetThingType();
        }

        public SelfThingProximity()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public bool m_Person { get; set; }
        public double m_Distance { get; set; }
        public bool m_IsClassified { get; set; }
        public double m_DistanceThreshold { get; set; }
        public string  m_SensorType { get; set; }

        public override string ToString()
        {
            return string.Format("[SelfThingProximity: Type_={0}, m_CreateTime={1}, m_GUID={2}, m_State={3}, HasData={4}, m_Person={5}, m_Distance={6}, m_IsClassified={7}, m_DistanceThreshold={8}, m_SensorType={9}]", Type_, m_CreateTime, m_GUID, m_State, HasData, m_Person, m_Distance, m_IsClassified, m_DistanceThreshold, m_SensorType);
        }
    }

    [fsObject]
    public class SelfThingPerson : SelfThing
    {
        public override string GetThingType() 
        {
            return "Person";
        }
        public override string GetText() 
        {
            return GetThingType();
        }

        public SelfThingPerson()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public string m_Name{ get; set; }
        public string m_Gender{ get; set; }
        public string m_AgeRange{ get; set; }
        public string m_PosX{ get; set; }
        public string m_PosY{ get; set; }
        public string m_Width{ get; set; }
        public string m_Height{ get; set; }
        public double m_Confidence{ get; set; }

        public override string ToString()
        {
            return string.Format("[SelfThingPerson: Type_={0}, m_CreateTime={1}, m_GUID={2}, m_State={3}, HasData={4}, m_Name={5}, m_Gender={6}, m_AgeRange={7}, m_PosX={8}, m_PosY={9}, m_Width={10}, m_Height={11}, m_Confidence={12}]", Type_, m_CreateTime, m_GUID, m_State, HasData, m_Name, m_Gender, m_AgeRange, m_PosX, m_PosY, m_Width, m_Height, m_Confidence);
        }

    }

    [fsObject]
    public class SelfThingObstacle : SelfThing
    {
        public override string GetThingType() 
        {
            return "Obstacle";
        }
        public override string GetText() 
        {
            return GetThingType();
        }

        public SelfThingObstacle()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public SelfVector3 a_Min { get; set; }
        public SelfVector3 a_Max { get; set; }

        public override string ToString()
        {
            return string.Format("[SelfThingObstacle: Type_={0}, m_CreateTime={1}, m_GUID={2}, m_State={3}, HasData={4}, a_Min={5}, a_Max={6}]", Type_, m_CreateTime, m_GUID, m_State, HasData, a_Min, a_Max);
        }
    }

    public class SelfVector3
    {
        public float   m_X { get; set; }
        public float   m_Y { get; set; }
        public float   m_Z { get; set; }

        public override string ToString()
        {
            return string.Format("[SelfVector3: X={0}, Y={1}, Z={2}]", m_X, m_Y, m_Z);
        }
    }

    [fsObject]
    public class SelfThingQuestionIntent : SelfThing
    {
        public override string GetThingType() 
        {
            return "QuestionIntent";
        }
        public override string GetText() 
        {
            return m_Text;
        }

        public SelfThingQuestionIntent()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public string m_Text { get; set; }
        public string m_Pipeline { get; set; }
        //TODO: Goal Params needs to deserialize into class
        public string m_GoalParams { get; set; }
        public bool m_bLocalDialog { get; set; }

        public override string ToString()
        {
            return string.Format("[SelfThingQuestionIntent: Type_={0}, m_CreateTime={1}, m_GUID={2}, m_State={3}, HasData={4}, m_Text={5}, m_Pipeline={6}, m_GoalParams={7}, m_bLocalDialog={8}]", Type_, m_CreateTime, m_GUID, m_State, HasData, m_Text, m_Pipeline, m_GoalParams, m_bLocalDialog);
        }
    }

    [fsObject]
    public class SelfThingImage : SelfThing
    {
        public override string GetThingType() 
        {
            return "Image";
        }
        public override string GetText() 
        {
            return GetThingType();
        }

        public SelfThingImage()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public string m_Content { get; set; }

        public override string ToString()
        {
            return string.Format("[SelfThingImage: Type_={0}, m_CreateTime={1}, m_GUID={2}, m_State={3}, HasData={4}, m_Content={5}]", Type_, m_CreateTime, m_GUID, m_State, HasData, m_Content);
        }
    }

    [fsObject]
    public class SelfThingObject : SelfThing
    {
        public override string GetThingType() 
        {
            return "Object";
        }
        public override string GetText() 
        {
            return GetThingType();
        }

        public SelfThingObject()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public string[] m_ObjectTypeList { get; set;}

        private string m_StringObjectList;
        private string StringObjectList
        {
            get
            {
                if (string.IsNullOrEmpty(m_StringObjectList))
                {
                    m_StringObjectList = (m_ObjectTypeList != null && m_ObjectTypeList.Length > 0)? string.Join(",", m_ObjectTypeList) : "NULL";
                }
                return m_StringObjectList;
            }
        }

        public override string ToString()
        {
            return string.Format("[SelfThingObject: Type_={0}, m_CreateTime={1}, m_GUID={2}, m_State={3}, HasData={4}, m_ObjectTypeList={5}}]", Type_, m_CreateTime, m_GUID, m_State, HasData, StringObjectList);
        }
    }

    [fsObject]
    public class SelfThingUnknown : SelfThing
    {
        public override string GetThingType() 
        {
            return string.IsNullOrEmpty(m_Type)? "Unknown" : m_Type;
        }
        public override string GetText() 
        {
            return m_Text;
        }

        public SelfThingUnknown()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public string m_Type { get; set;}
        public string m_Text { get; set;}

        public override string ToString()
        {
            return string.Format("[SelfThingUnknown: Type_={0}, m_CreateTime={1}, m_GUID={2}, m_State={3}, HasData={4}]", Type_, m_CreateTime, m_GUID, m_State, HasData);
        }
    }

    [fsObject]
    public class SelfThingEmotion : SelfThing
    {
        public override string GetThingType() 
        {
            return "Emotion";
        }
        public override string GetText() 
        {
            return GetThingType();
        }

        public SelfThingEmotion()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public string m_Type { get; set;}

        public override string ToString()
        {
            return string.Format("[SelfThingEmotion: Type_={0}, m_CreateTime={1}, m_GUID={2}, m_State={3}, HasData={4}]", Type_, m_CreateTime, m_GUID, m_State, HasData);
        }
    }

    [fsObject]
    public class SelfThingClarification : SelfThing
    {
        public override string GetThingType() 
        {
            return "Clarification";
        }
        public override string GetText() 
        {
            return m_Text;
        }

        public SelfThingClarification()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public string m_Text { get; set; }
        public string m_Info {get; set;}

        public override string ToString()
        {
            return string.Format("[SelfThingClarification: Type_={0}, m_CreateTime={1}, m_GUID={2}, m_State={3}, HasData={4}, m_Text={5}, m_Info={6}]", Type_, m_CreateTime, m_GUID, m_State, HasData, m_Text, m_Info);
        }
    }

    [fsObject]
    public class SelfThingConfirm : SelfThing
    {
        public override string GetThingType() 
        {
            return "Confirm";
        }
        public override string GetText() 
        {
            return GetThingType();
        }

        public SelfThingConfirm()
        {
            Type_ = GetThingType();
            m_CreateTime = Utility.GetEpochUTC();
            m_GUID = "";
            m_State = "";
        }

        public bool m_bConfirmed { get; set; }
        public string m_Info {get; set;}

        public override string ToString()
        {
            return string.Format("[SelfThingConfirm: Type_={0}, m_CreateTime={1}, m_GUID={2}, m_State={3}, HasData={4}, m_bConfirmed={5}, m_Info={6}]", Type_, m_CreateTime, m_GUID, m_State, HasData, m_bConfirmed, m_Info);
        }
    }



    #endregion
   
    #region Object Types to Send Message , Query and Subscribe
      

    [fsObject]
    public class SelfMessageToServer
    {
        public string[] targets { get; set;}
        public string origin { get; set;}
        public string msg { get; set;}
        public string data {get;set;}
        public bool persisted {get;set;}

        public SelfMessageToServer()
        {
            msg = "publish_at";
            origin = Utility.MacAddress + "/.";
            persisted = false;
        }
    }

    [fsObject]
    public class QueryMessage
    {
        public string[] targets { get; set;}
        public string origin { get; set;}
        public string msg { get; set;}
        public string request { get; set;}

        public QueryMessage(string[] targetsToQuery)
        {
            if (targetsToQuery == null)
                this.targets = new string[]{ "." };
            else
                this.targets = targetsToQuery;

            origin = Utility.MacAddress + "/.";
            msg = "query";
            request = Guid.NewGuid().ToString();
        }

    }


    [fsObject]
    public class SubscriptionMessage
    {
        public enum SubscriptionType
        {
            Subscribe,
            Unsubscribe
        }
        public string[] targets { get; set;}
        public string origin { get; set;}
        public string msg { get; set;}

        public SubscriptionMessage(SelfTopic[] topicListToSubscribe, string[] targets, SubscriptionType subscriptionType)
        {
            List<string> listStringTarget = new List<string>();

            for (int indexTopic = 0; topicListToSubscribe != null && indexTopic < topicListToSubscribe.Length; indexTopic++)
            {
                if (targets != null)
                {
                    for (int targetIndex = 0; targets != null && targetIndex < targets.Length; targetIndex++)
                    {
                        if (targets[targetIndex] == ".")
                        {
                            listStringTarget.Add(string.Format("{0}", topicListToSubscribe[indexTopic].topicId));
                        }
                        else
                        {
                            listStringTarget.Add(string.Format("{0}/{1}", targets[targetIndex].Replace("/.", ""), topicListToSubscribe[indexTopic].topicId));
                        }
                    }
                }
                else
                {
                    listStringTarget.Add(string.Format("{0}", topicListToSubscribe[indexTopic].topicId));
                }
            }

            this.targets = listStringTarget.ToArray();
            this.origin = Utility.MacAddress + "/.";

            if (subscriptionType == SubscriptionType.Subscribe)
            {
                this.msg = "subscribe";
            }
            else if (subscriptionType == SubscriptionType.Unsubscribe)
            {
                this.msg = "unsubscribe";
            }
            else
            {
                Log.Warning("SubscriptionMessage", "Invalid arguments: {0}", subscriptionType);
                this.msg = "subscribe";
            }

        }
    }


    #endregion

    #region Self Server - Parent Structure 

    public class SelfServer : SelfMessage
    {
        public List<SelfServer> childrenNodes { get; set;}
        public SelfServer parent{ get; set;}
        public bool isOnline = false;
        public SelfBody selfBody { get ; set; }

        //Constructor for child node
        public SelfServer(string selfId, SelfServer parent)
        {
            this.name = null;
            this.selfId = selfId;
            this.childrenNodes = null;
            this.parent = parent;
        }

        //Constructor for initial head node
        public SelfServer(SelfMessage robotMessage)
        {
            this.name = robotMessage.name;
            this.selfId = robotMessage.selfId;
            this.childrenNodes = new List<SelfServer>();
            this.parent = null;
            this.topics = robotMessage.topics;

            List<string> childrenTargets = new List<string>();

            for (int i = 0; robotMessage.children != null && i < robotMessage.children.Length; i++)
            {
                SelfServer foundSelfServer = this.Find(robotMessage.children[i]);

                if (foundSelfServer == null)
                {
                    SelfServer childServer = new SelfServer(robotMessage.children[i], this);
                    if (childServer.IsLocalhost)
                    {
                        childServer.name = "LocalHost"; // + UnityEngine.SystemInfo.operatingSystem;
                    }
                    this.childrenNodes.Add(childServer);
                    if (!childServer.IsLocalhost)
                        childrenTargets.Add(childServer.TargetPath);
                }
                else
                {
                    Log.Warning("SelfServer", "The same child Self Id has already added. {0}", foundSelfServer);
                }
            }

            if (SelfWebSocket.Instance.DiscoverAllChilderen && childrenTargets.Count > 0)
            {
                SelfWebSocket.Instance.QuerySelf(childrenTargets.ToArray());
            }
        }

        public void Clear()
        {
            for (int i = 0; childrenNodes != null && i < childrenNodes.Count; i++)
            {
                childrenNodes[i].Clear();
            }
            if(childrenNodes != null)
                childrenNodes.Clear();

            this.name = null;
            this.selfId = null;

        }

        public string TargetPath
        {
            get
            {
                StringBuilder targetPathBuilder = new StringBuilder();

                SelfServer tempCurrent = this;
                SelfServer tempParent = parent;
                while (tempParent != null)
                {
                    targetPathBuilder.Insert(0,  tempCurrent.selfId + "/");
                    tempCurrent = tempParent;
                    tempParent = tempParent.parent;
                }

                targetPathBuilder.Append(".");

                return targetPathBuilder.ToString();
            }
        }

        public bool NeedDiscovery
        {
            get
            {
                bool needDiscovery = (!IsLocalhost && !HasFullData);

                for (int i = 0; !needDiscovery && childrenNodes != null && i < childrenNodes.Count; i++)
                {
                    needDiscovery |= childrenNodes[i].NeedDiscovery;

                    if (needDiscovery)
                        break;
                }

                return needDiscovery;
            }
        }

        public bool IsLocalhost
        {
            get
            {
                return !string.IsNullOrEmpty(selfId) && selfId.ToUpper().Equals(Utility.MacAddress);
            }
        }

        public int GetHeightOfNode(SelfServer selfServerNode)
        {
            if (selfServerNode == null)
                return 0;

            int maxHeightOnChilderen = 0;
            for (int i = 0; selfServerNode.childrenNodes != null && i < selfServerNode.childrenNodes.Count; i++)
            {
                int tempHeightOfNode = GetHeightOfNode(selfServerNode.childrenNodes[i]);
                if (tempHeightOfNode > maxHeightOnChilderen)
                {
                    maxHeightOnChilderen = tempHeightOfNode;
                }
            }

            return maxHeightOnChilderen + 1;
        }

        public int NumberOfAllChildren
        {
            get
            {
                int numberOfChildren = (childrenNodes != null) ? childrenNodes.Count : 0;
                for (int i = 0; childrenNodes != null && i < childrenNodes.Count; i++)
                {
                    numberOfChildren += childrenNodes[i].NumberOfAllChildren;
                }
                return numberOfChildren;
            }
        }

        public int HeightOfTree
        {
            get
            {
                return GetHeightOfNode(RootNode);
            }
        }

        public SelfServer RootNode
        {
            get
            {
                SelfServer tempParent = this;
                while (tempParent.parent != null)
                {
                    tempParent = tempParent.parent;
                }
                return tempParent;
            }
        }

        int m_level = 0;
        public int Level
        {
            get
            {
                if (m_level == 0)
                {
                    m_level++;
                    SelfServer tempParent = parent;
                    while (tempParent != null)
                    {
                        m_level++;
                        tempParent = tempParent.parent;
                    }
                }
                return m_level;
            }
            set
            {
                m_level = value;
            }
        }

        public SelfServer Find(string selfId)
        {
            SelfServer foundSelfServer = null;

            if (selfId != null)
            {
                //TODO See how this will work with multi parents
                selfId = selfId.Replace("/.", "");

                if (this.selfId == selfId)
                {
                    foundSelfServer = this;
                }
                else
                {
                    for (int i = 0; this.childrenNodes != null && i < this.childrenNodes.Count; i++)
                    {
                        foundSelfServer = this.childrenNodes[i].Find(selfId);
                        if (foundSelfServer != null)
                            break;
                    }
                }
            }

            return foundSelfServer;
        }

        public SelfServer FindWithTargetPath(string targetPath)
        {
            SelfServer foundSelfServer = null;
            if (this.TargetPath == targetPath)
            {
                foundSelfServer = this;
            }
            else
            {
                for (int i = 0; this.childrenNodes != null && i < this.childrenNodes.Count; i++)
                {
                    foundSelfServer = this.childrenNodes[i].FindWithTargetPath(targetPath);
                    if (foundSelfServer != null)
                        break;
                }
            }
            return foundSelfServer;
        }

        public void PrintAllTree()
        {
            if (RootNode != null)
            {
                
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 1; i <= HeightOfTree; i++)
                {
                    PrintTraverserSameLevel(RootNode, i, stringBuilder);
                    stringBuilder.Append("\n");
                }

                Log.Debug("SelfServer", "---- Printing SelfServer Tree ----- \n {0}", stringBuilder.ToString());
            }
            else
            {
                Log.Debug("SelfServer", "There is no head parent node for Self to print");
            }
        }

        private void PrintTraverserSameLevel(SelfServer selfServerNode, int level, StringBuilder stringBuilder)
        {
            if (selfServerNode != null)
            {
                if (level == 1)
                {
                    stringBuilder.Append(selfServerNode.ToString());
                    stringBuilder.Append("\n");
                }
                else
                {
                    for (int i = 0; selfServerNode.childrenNodes != null && i < selfServerNode.childrenNodes.Count; i++)
                    {
                        PrintTraverserSameLevel(selfServerNode.childrenNodes[i], level - 1, stringBuilder);
                    }
                }
            }
            else
            {
                //do nothing
            }

        }

        public void AddChildren(SelfMessage robotMessage)
        {
            SelfServer foundSelfServer = null;
            List<string> childrenTargets = new List<string>();
            if (RootNode != null)
            {
                if (!string.IsNullOrEmpty(robotMessage.selfId))
                {
                    foundSelfServer = RootNode.Find(robotMessage.selfId);

                    if (foundSelfServer != null)
                    {
                        if (string.IsNullOrEmpty(foundSelfServer.name))
                        {
                            foundSelfServer.name = robotMessage.name;
                            foundSelfServer.topics = robotMessage.topics;

                            for (int i = 0; robotMessage.children != null && i < robotMessage.children.Length; i++)
                            {
                                SelfServer childServer = new SelfServer(robotMessage.children[i], foundSelfServer);
                                foundSelfServer.childrenNodes.Add(childServer);
                                if (!childServer.IsLocalhost)
                                    childrenTargets.Add(childServer.TargetPath);
                            }
                        }
                        else
                        {
                            Log.Warning("SelfServer", "Server Found with full data, so there is no need to update. SelfNode : {0}", foundSelfServer);
                        }
                    }
                    else
                    {
                        Log.Error("SelfServer", "Child Self Server couldn't find in the current tree : {0} - {1}", robotMessage.selfId, robotMessage.name);
                    }
                }
                else
                {
                    Log.Error("SelfServer", "Child without self id has appeared : {0}", robotMessage.ToString());
                }
            }
            else
            {
                Log.Error("SelfServer", "There is no head parent node for Self to add childeren");
            }

            if (SelfWebSocket.Instance.DiscoverAllChilderen && childrenTargets.Count > 0)
            {
                SelfWebSocket.Instance.QuerySelf(childrenTargets.ToArray());
            }
        }

        public bool UpdateNode(SelfMessage robotMessage)
        {
            //We are all set - but this is called for updating node

            return false;
        }

        public void ClearIfEmpty()
        {
            bool needDiscovery = (!IsLocalhost && !HasFullData);
            if (needDiscovery)
            {
                Clear();
                if (parent != null)
                {
                    //parent.childrenNodes.Remove(this);
                    parent = null;
                }
            }
            else
            {
                if (childrenNodes != null)
                {
                    for (int i = childrenNodes.Count - 1; i >= 0 ; i--)
                    {
                        childrenNodes[i].ClearIfEmpty();
                        if(childrenNodes[i].NeedDiscovery)
                            childrenNodes.RemoveAt(i);
                    }
                }

            }

        }

        public void SetAllDiscovered()
        {
            
            ClearIfEmpty();
            Log.Status("SelfServer", "SetAllDiscovered, result: {0}", NeedDiscovery);

        }

        public bool Equals(SelfServer obj)
        {
            return obj != null && this.selfId == obj.selfId && this.name == obj.name;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; this.childrenNodes != null && i < this.childrenNodes.Count; i++)
            {
                stringBuilder.Append(this.childrenNodes[i].selfId);
                stringBuilder.Append(" , ");
            }
            string parentSelfIf = "";
            if (parent != null)
                parentSelfIf = parent.selfId;

            int currentHeight = GetHeightOfNode(this);

            StringBuilder stringBuilderTopics = new StringBuilder();
            for (int i = 0; this.topics != null && i < this.topics.Length; i++)
            {
                stringBuilderTopics.Append(this.topics[i].topicId);
                stringBuilderTopics.Append(" , ");
            }

            return string.Format("[SelfServer: name={0}, selfId={1}, parent={3}, NeedDiscovery={4}, TargetPath={5}, children=[{2}], height={6}, level={7}, topics={8}]", name, selfId, stringBuilder.ToString(), parentSelfIf, NeedDiscovery, TargetPath, currentHeight, Level, stringBuilderTopics.ToString());
        }
    }

    #endregion
   
}
