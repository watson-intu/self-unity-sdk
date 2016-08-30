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

    #region Self Body Class Definition

    [fsObject(Converter = typeof(SelfBodyDataConverter))]
    public class SelfBody
    {
        #region Public members of self body config

        public string Type_;
        [fsIgnore]
        public IAgent[] m_Agents;
        [fsIgnore]
        public IClassifier[] m_Classifiers;
        [fsIgnore]
        public IExtractor[] m_Extractors;
        public string[] m_GestureFiles;
        public string[] m_Libs;
        public string m_LogoUrl;
        public string m_MacId;
        public string[] m_PlanFiles;
        public string m_RobotKey;
        public string m_RobotName;
        public string m_RobotType;
        public string m_RobotUrl;
        public string m_SelfVersion;
        [fsIgnore]
        public ISensor[] m_Sensors;
        public ServiceConfig[] m_ServiceConfigs;
        [fsIgnore]
        public IService[] m_Services;
        public string[] m_SkillFiles;

        #endregion

        #region Functions for SelfBody

        public override string ToString()
        {
            StringBuilder agents = new StringBuilder();
            agents.Append("\n");
            for (int i = 0; m_Agents != null && i < m_Agents.Length; i++)
            {
                agents.Append(m_Agents[i].Type_);
                agents.Append("\n");
            }

            StringBuilder classifiers = new StringBuilder();
            classifiers.Append("\n");
            for (int i = 0; m_Classifiers != null && i < m_Classifiers.Length; i++)
            {
                classifiers.Append(m_Classifiers[i].Type_);
                classifiers.Append("\n");
            }

            StringBuilder extractors = new StringBuilder();
            extractors.Append("\n");
            for (int i = 0; m_Extractors != null && i < m_Extractors.Length; i++)
            {
                extractors.Append(m_Extractors[i].Type_);
                extractors.Append("\n");
            }

            StringBuilder sensors = new StringBuilder();
            sensors.Append("\n");
            for (int i = 0; m_Sensors != null && i < m_Sensors.Length; i++)
            {
                sensors.Append(m_Sensors[i].Type_);
                sensors.Append("\n");
            }

            StringBuilder services = new StringBuilder();
            services.Append("\n");
            for (int i = 0; m_Services != null && i < m_Services.Length; i++)
            {
                services.Append(m_Services[i].Type_);
                services.Append("\n");
            }

            StringBuilder serviceConfigs = new StringBuilder();
            serviceConfigs.Append("\n");
            for (int i = 0; m_ServiceConfigs != null && i < m_ServiceConfigs.Length; i++)
            {
                serviceConfigs.Append(m_ServiceConfigs[i].m_ServiceId);
                serviceConfigs.Append("\n");
            }

            StringBuilder gestureFiles = new StringBuilder();
            gestureFiles.Append("\n");
            for (int i = 0; m_GestureFiles != null && i < m_GestureFiles.Length; i++)
            {
               gestureFiles.Append(gestureFiles[i]);
               gestureFiles.Append("\n");
            }

            StringBuilder libs = new StringBuilder();
            libs.Append("\n");
            for (int i = 0; m_Libs != null && i < m_Libs.Length; i++)
            {
                libs.Append(m_Libs[i]);
                libs.Append("\n");
            }

            StringBuilder planFiles = new StringBuilder();
            planFiles.Append("\n");
            for (int i = 0; m_PlanFiles != null && i < m_PlanFiles.Length; i++)
            {
                planFiles.Append(m_PlanFiles[i]);
                planFiles.Append("\n");
            }

            StringBuilder skillFiles = new StringBuilder();
            skillFiles.Append("\n");
            for (int i = 0; m_SkillFiles != null && i < m_SkillFiles.Length; i++)
            {
                skillFiles.Append(m_SkillFiles[i]);
                skillFiles.Append("\n");
            }

            return string.Format("[SelfBody: Type_: {0} \n m_Agents:{1} \n m_Classifiers: {2} \n m_Extractors: {3} \n m_GestureFiles: {4} \n m_Libs: {5} \n m_LogoUrl: {6} \n m_MacId: {7} \n m_PlanFiles: {8} \n m_RobotKey: {9} \n m_RobotName: {10} \n " +
                "m_RobotType: {11} \n m_RobotUrl: {12} \n m_SelfVersion: {13} \n m_Sensors: {14} \n m_ServiceConfigs: {15} \n m_Services: {16} \n m_SkillFiles: {17}]", Type_, agents.ToString(), classifiers.ToString(), extractors.ToString(), gestureFiles.ToString(), libs.ToString(),
                m_LogoUrl, m_MacId, planFiles.ToString(), m_RobotKey, m_RobotName, m_RobotType, m_RobotUrl, m_SelfVersion, sensors.ToString(), serviceConfigs.ToString(), services.ToString(), skillFiles.ToString());
        }

        #endregion
    }

    #endregion

    #region Serialization Functions for body json

    public class SelfBodyDataConverter : fsConverter {
        private static fsSerializer sm_Serializer = new fsSerializer();

        public override bool CanProcess(Type type) {
            return type == typeof(SelfBody);
        }

        public override fsResult TrySerialize(object instance,
            out fsData serialized, Type storageType) {

            serialized = null;

//            SelfBody myType = (SelfBody)instance;
//
//            Dictionary<string, fsData> currentSerialization = new Dictionary<string, fsData>();
//
//            currentSerialization.Add("event", new fsData(myType.@event)) ;
//            //currentSerialization.Add("parent", new fsData(myType.parent));
//
//            if (myType.thing != null)
//            {
//                string thingType = myType.thing.GetThingType();
//                fsData tempData = null;
//
//                Type[] types = Utilities.Utility.FindAllDerivedTypes(typeof(SelfThing));
//                foreach (Type currentType in types)
//                {
//                    try
//                    {
//                        System.Object selfThingObject = Activator.CreateInstance(currentType);
//                        SelfThing selfThing = selfThingObject as SelfThing;
//                        if(selfThing.GetThingType() == thingType)
//                        {
//                            sm_Serializer.TrySerialize(currentType, myType.thing, out tempData);
//                            break;
//                        }
//                    }
//                    catch (Exception)
//                    { 
//                    }
//                }
//
//                if (tempData != null)
//                {
//                    currentSerialization.Add("thing", tempData);
//                }
//
//            }
//
//            serialized = new fsData(currentSerialization);
//
//            //serialized = new fsData(myType.Value);
            return fsResult.Success;
        }

        public override fsResult TryDeserialize(fsData storage,
            ref object instance, Type storageType) {

            if (storage.Type != fsDataType.Object) {
                return fsResult.Fail("Expected object fsData type but got " + storage.Type);
            }

            SelfBody selfBody = (SelfBody)instance;

            Dictionary<string, fsData> deserializedObject = storage.AsDictionary;

            #region Convert basic string attributes
            if(deserializedObject.ContainsKey("Type_"))
                selfBody.Type_ = deserializedObject["Type_"].AsString;

            if(deserializedObject.ContainsKey("m_LogoUrl"))
                selfBody.m_LogoUrl = deserializedObject["m_LogoUrl"].AsString;

            if(deserializedObject.ContainsKey("m_MacId"))
                selfBody.m_MacId = deserializedObject["m_MacId"].AsString;

            if(deserializedObject.ContainsKey("m_RobotKey"))
                selfBody.m_RobotKey = deserializedObject["m_RobotKey"].AsString;

            if(deserializedObject.ContainsKey("m_RobotType"))
                selfBody.m_RobotType = deserializedObject["m_RobotType"].AsString;

            if(deserializedObject.ContainsKey("m_RobotUrl"))
                selfBody.m_RobotUrl = deserializedObject["m_RobotUrl"].AsString;

            if(deserializedObject.ContainsKey("m_SelfVersion"))
                selfBody.m_SelfVersion = deserializedObject["m_SelfVersion"].AsString;

            #endregion

            #region Convert Basic String Arrays

            if (deserializedObject.ContainsKey("m_GestureFiles"))
            {
                List<fsData> gestureFiles = deserializedObject["m_GestureFiles"].AsList;
                List<string> gestureFilesConverted = new List<string>();

                foreach(fsData file in gestureFiles)
                {
                    gestureFilesConverted.Add(file.AsString);
                }

                selfBody.m_GestureFiles = gestureFilesConverted.ToArray();
            }

            if (deserializedObject.ContainsKey("m_Libs"))
            {
                List<fsData> libsFiles = deserializedObject["m_Libs"].AsList;
                List<string> libsFilesConverted = new List<string>();

                foreach(fsData file in libsFiles)
                {
                    libsFilesConverted.Add(file.AsString);
                }

                selfBody.m_Libs = libsFilesConverted.ToArray();
            }

            if (deserializedObject.ContainsKey("m_PlanFiles"))
            {
                List<fsData> planFiles = deserializedObject["m_PlanFiles"].AsList;
                List<string> planFilesConverted = new List<string>();

                foreach(fsData file in planFiles)
                {
                    planFilesConverted.Add(file.AsString);
                }

                selfBody.m_PlanFiles = planFilesConverted.ToArray();
            }

            if (deserializedObject.ContainsKey("m_SkillFiles"))
            {
                List<fsData> skillFiles = deserializedObject["m_SkillFiles"].AsList;
                List<string> skillFilesConverted = new List<string>();

                foreach(fsData file in skillFiles)
                {
                    skillFilesConverted.Add(file.AsString);
                }

                selfBody.m_SkillFiles = skillFilesConverted.ToArray();
            }

            #endregion

            #region Deserialize custom class lists, find the correct derived class and parse it out
            if (deserializedObject.ContainsKey("m_Agents"))
            {
                List<fsData> jsonListOfAgents = deserializedObject["m_Agents"].AsList;
                List<IAgent> deserializedBodyAgents = null;

                AttemptDeserializationOfJSONArray(jsonListOfAgents, out deserializedBodyAgents);
                selfBody.m_Agents = deserializedBodyAgents.ToArray();
            }

            if (deserializedObject.ContainsKey("m_Classifiers"))
            {
                List<fsData> jsonListOfClassifiers = deserializedObject["m_Classifiers"].AsList;
                List<IClassifier> deserializedBodyClassifiers = null;

                AttemptDeserializationOfJSONArray(jsonListOfClassifiers, out deserializedBodyClassifiers);
                selfBody.m_Classifiers = deserializedBodyClassifiers.ToArray();
            }
           
            if (deserializedObject.ContainsKey("m_Extractors"))
            {
                List<fsData> jsonListOfExtractors = deserializedObject["m_Extractors"].AsList;
                List<IExtractor> deserializedBodyExtractors = null;

                AttemptDeserializationOfJSONArray(jsonListOfExtractors, out deserializedBodyExtractors);
                selfBody.m_Extractors = deserializedBodyExtractors.ToArray();
            }

            if (deserializedObject.ContainsKey("m_Services"))
            {
                List<fsData> jsonListOfServices = deserializedObject["m_Services"].AsList;
                List<IService> deserializedBodyServices = null;

                AttemptDeserializationOfJSONArray(jsonListOfServices, out deserializedBodyServices);
                selfBody.m_Services = deserializedBodyServices.ToArray();
                SelfThing.m_ServiceNames = selfBody.m_Services;
            }

            if (deserializedObject.ContainsKey("m_Sensors") && !deserializedObject["m_Sensors"].IsNull)
            {
                List<fsData> jsonListOfSensors = deserializedObject["m_Sensors"].AsList;
                List<ISensor> deserializedBodySensors = null;

                AttemptDeserializationOfJSONArray(jsonListOfSensors, out deserializedBodySensors);
                selfBody.m_Sensors = deserializedBodySensors.ToArray();
            }

            #endregion

            return fsResult.Success;
        }

        #region Serialization Helper Functions

        public override object CreateInstance(fsData data, Type storageType) {
            return new SelfBody();
        }

        void AttemptDeserializationOfJSONArray<T>(List<fsData> listToDeserialize , out List<T> listToReturn) where T : class
        {
            listToReturn = new List<T>();

            if (listToDeserialize == null)
            {
                return;
            }

            Type[] types = Utilities.Utility.FindAllDerivedTypes(typeof(T));
            foreach (fsData data in listToDeserialize)
            {
                if (data == null || data.IsNull)
                {
                    continue;
                }

                try
                {
                    Dictionary<string, fsData> dataDictionary = data.AsDictionary;

                    String objectType = dataDictionary["Type_"].AsString;

                    foreach (Type currentType in types)
                    {
                        System.Object systemObject = Activator.CreateInstance(currentType);
                        T newObject = systemObject as T;

                        Type t = newObject.GetType();
                        System.Reflection.PropertyInfo prop = t.GetProperty("Type_");
                        string newObjectType = (string) prop.GetValue(newObject, null);

                        if (newObjectType == objectType)
                        {
                            sm_Serializer.TryDeserialize(data, currentType, ref systemObject);

                            listToReturn.Add(newObject);

                            break;
                        }

                    }
                }
                catch (Exception ex)
                {
                    Log.Error("SelfBodyConverter", "AttemptDeserializationOfJSONArray - Exception: {0} - {1} - {2}", ex.Message, typeof(T), ex.StackTrace);
                }

            }

        }

        #endregion
    }

    #endregion
        
    #region Self Body interface classes

    public interface IAgent
    {
        string Type_ { get ; set; }
    }
        
    public interface IClassifier
    {
        string Type_ { get; set; }
    }
        
    public interface IExtractor
    {
        string Type_ { get; set; }
    }
        
    public interface ISensor
    {
        string Type_ { get; set; } 
    }
        
    public interface IService
    {
        string Type_ { get; set; }
        string m_ServiceId { get; set; }
    }

    #endregion

    #region Self body custom classes as per JSON information

    [fsObject]
    public class ServiceConfig
    {
        public string m_Password { get; set; }
        public string m_ServiceId { get; set; }
        public string m_URL { get; set; }
        public string m_User { get; set; }
    }

    [fsObject]
    public class Dialog
    {
        public bool m_AppendDialogClass { get; set; }
        public string m_ClassifierFile { get; set; }
        public string m_ClassifierId { get; set; }
        public string m_DialogFile { get; set; }
        public string m_DialogId { get; set; }
        public bool m_DialogUsesIntent { get; set; }
        public string m_Language { get; set; }
        public float m_UpdateInterval { get; set; }
    }

    [fsObject]
    public class Greeting
    {
        public string m_GenderFilter { get; set; }
        public string m_Greeting { get; set; }
    }

    [fsObject]
    public class VolumeTuner 
    {
        public string m_Sensor { get; set; }
        public string m_Skill { get; set; }
    }

    [fsObject]
    public class TextFilter
    {
        public string Type_ { get; set; }
        public float m_MinIntentWindow { get; set; }
    }

    [fsObject] 
    public class IntentClass
    {
        public string m_Class { get; set; }
        public string m_Intent { get; set; }
    }

    [fsObject]
    public class Header
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    [fsObject]
    public class RestCall
    {
        public string m_Body { get; set; }
        public Header[] m_Headers { get; set; }
        public string m_Params { get; set; }
        public string m_Type { get; set; }
        public string m_URL { get; set; }
    }

    #region Agent Classes

    [fsObject]
    public class SelfUpdateAgent : IAgent
    {
        public string Type_ { get ; set; }
        public string m_FoundUpdateResponse { get; set; }
        public string m_InstallationCompleteResponse { get; set; }
        public string m_LastVersionConfirmed { get; set; }
        public string m_SelfPackageName { get; set; }
        public bool m_bAllowRecommendedDownload { get; set; }
        public string m_fUpdateCheckDelay { get; set; }

        public SelfUpdateAgent()
        {
            Type_ = "SelfUpdateAgent";
        }
    }

    [fsObject]
    public class GoalAgent : IAgent
    {
        public string Type_ { get; set; }

        public GoalAgent()
        {
            Type_ = "GoalAgent";
        }
    }

    [fsObject]
    public class TopicAgent : IAgent
    {
        public string Type_ { get; set; }
        public string m_CertFile { get; set; }
        public string m_Interface { get; set; }
        public string m_KeyFile { get; set; }
        public string m_ParentHost { get; set; }
        public string m_VerifyFile { get; set; }
        public float m_fReconnectInterval { get; set; }
        public float m_fRequestTimeout { get; set; }
        public int m_nPort { get; set; }
        public int m_nThreads { get; set; }

        public TopicAgent() 
        {
            Type_ = "TopicAgent";
        }
    }

    [fsObject]
    public class LearningAgent : IAgent
    {
        public string Type_ { get; set; }
        public string[] m_ConfirmRetrain { get; set; }
        public string[] m_ForgetResponses { get; set; }
        public string[] m_LearningResponses { get; set; }
        public string[] m_RetrainResponses { get; set; }

        public LearningAgent() 
        {
            Type_ = "LearningAgent";
        }
    }

    [fsObject]
    public class URLAgent : IAgent
    {
        public string Type_ { get; set; }
        public float m_HeartBeatServerInterval { get; set; }

        public URLAgent()
        {
            Type_ = "URLAgent";
        }
    }

    [fsObject]
    public class AttentionAgent : IAgent
    {
        public string Type_ { get; set; }
        public float m_ElevatedThresh { get; set; }
        public float m_StandardThresh { get; set; }
        public float m_LoweredThresh { get; set; }
        public float m_TimeHoldOn { get; set; }
        public float m_TimeProximityWait { get; set; }
        public float m_TimeGazeWait { get; set; }
        
        public AttentionAgent()
        {
            Type_ = "AttentionAgent";
        }
    }

    [fsObject]
    public class QuestionAgent : IAgent
    {
        public string Type_ { get; set; }
        public string m_ClarificationTag { get; set; }
        public string m_ConfirmationTag { get; set; }
        public int m_DeleteDelay { get; set; }
        public string m_DialogMissIntent { get; set; }
        public Dialog[] m_Dialogs {get; set;}
        public string[] m_HangOnResponses { get; set; }
        public int m_HangOnTime { get; set; }
        public int m_MinAnswerConfidence { get; set; }
        public int m_MinDialogConfidence { get; set; }
        public string[] m_PipelineDownResponses { get; set; }
        public int m_UseDialogConfidence { get; set; }
        public int m_nQuestionLimit { get; set; }

        public QuestionAgent() 
        {
            Type_ = "QuestionAgent";
        }
    }

    [fsObject]
    public class RandomInteractionAgent : IAgent
    {
        public string Type_ { get; set; }
        public string[] m_TextList { get; set; }
        public float m_fMaxSpeakDelay { get; set; }
        public float m_fMinSpeakDelay { get; set; }

        public RandomInteractionAgent() 
        {
            Type_ = "RandomInteractionAgent";
        }
    }

    [fsObject]
    public class RequestAgent : IAgent
    {
        public string Type_ { get; set; }
        public string[] m_RequestFailedText { get; set; }

        public RequestAgent() 
        {
            Type_ = "RequestAgent";
        }
    }

    [fsObject]
    public class FeedbackAgent : IAgent
    {
        public string Type_ { get; set; }
        public string[] m_NegativeResponses { get; set; }
        public string[] m_PositiveResponses { get; set; }

        public FeedbackAgent() 
        {
            Type_ = "FeedbackAgent";
        }
    }

    [fsObject]
    public class ExcessiveProcessingAgent : IAgent
    {
        public string Type_ { get; set; }
        public string[] m_PleaseWaitText { get; set; }
        public float m_fProcessingTime { get; set; }

        public ExcessiveProcessingAgent() 
        {
            Type_ = "ExcessiveProcessingAgent";
        }
    }

    [fsObject]
    public class GreeterAgent : IAgent
    {
        public string Type_ { get; set; }
        public Greeting[] m_Greetings { get; set; }
        public int m_WaitTime { get; set; }

        public GreeterAgent() 
        {
            Type_ = "GreeterAgent";
        }
    }

    [fsObject]
    public class DialogAgent : IAgent
    {
        public string Type_ { get; set; }
        public string[] m_InterruptionResponses { get; set; }
        public string[] m_InterruptionSensors { get; set; }
        public string[] m_Interruptions { get; set; }
        public int m_MinInterruptionSensors { get; set; }
        public string m_SpeechSkill { get; set; }
        public string m_Voice { get; set; }
        public VolumeTuner[] m_VolumeTunings { get; set; }
        public float m_fInterruptionSensorInterval { get; set; }

        public DialogAgent() 
        {
            Type_ = "DialogAgent";
        }
    }

    [fsObject]
    public class HealthAgent : IAgent
    {
        public string Type_ { get; set; }
        public float m_HealthCheckInterval { get; set; }
        public string[] m_HealthCheckServices { get; set; }

        public HealthAgent() 
        {
            Type_ = "HealthAgent";
        }

        public override string ToString()
        {
            return string.Format("[HealthAgent: Type_={0}, m_HealthCheckInterval={1}, m_HealthCheckServices={2}]", Type_, m_HealthCheckInterval, m_HealthCheckServices);
        }
    }

    [fsObject]
    public class EmotionAgent : IAgent
    {
        public string Type_ { get; set; }
        public float m_WaitTime { get; set; }

        public EmotionAgent() 
        {
            Type_ = "EmotionAgent";
        }
    }

    [fsObject]
    public class StatusChangeAgent : IAgent
    {
        public string Type_ { get; set; }

        public StatusChangeAgent() 
        {
            Type_ = "StatusChangeAgent";
        }
    }

    [fsObject]
    public class EnvironmentAgent : IAgent
    {
        public string Type_ { get; set; }

        public EnvironmentAgent() 
        {
            Type_ = "EnvironmentAgent";
        }
    }

    [fsObject]
    public class ReminderAgent : IAgent
    {
        public string Type_ { get; set; }
        public float m_Delay { get; set; }
        public string[] m_Sayings { get; set; }

        public ReminderAgent() 
        {
            Type_ = "ReminderAgent";
        }
    }

    [fsObject]
    public class SleepAgent : IAgent
    {
        public string Type_ { get; set; }
        public string[] m_HealthSensorMasks { get; set; }
        public float m_SleepTime { get; set; }
        public float m_WakeTime { get; set; }

        public SleepAgent() 
        {
            Type_ = "SleepAgent";
        }
    }

    #endregion

    #region Classifiers Classes

    [fsObject]
    public class TextClassifier : IClassifier
    {
        public string Type_ { get; set; }
        public string m_ClassifierFile { get; set; }
        public string m_ClassifierId { get; set; }
        public string[] m_FailureResponses { get; set; }
        public TextFilter[] m_Filters { get; set; }
        public IntentClass[] m_IntentClasses { get; set; }
        public string m_Language { get; set; }
        public string[] m_LowConfidenceResponses { get; set; }
        public float m_MinFailureResponseInterval { get; set; } 
        public float m_MinIntentConfidence { get; set; } 
        public float m_MinMissNodeConfidence { get; set; } 

        public TextClassifier() 
        {
            Type_ = "TextClassifier";
        }
    }

    [fsObject]
    public class ImageClassifier : IClassifier
    {
        public string Type_ { get; set; }

        public ImageClassifier() 
        {
            Type_ = "ImageClassifier";
        }
    }

    [fsObject]
    public class PersonClassifier : IClassifier
    {
        public string Type_ { get; set; }

        public PersonClassifier() 
        {
            Type_ = "PersonClassifier";
        }
    }

    [fsObject]
    public class EnvironmentClassifier : IClassifier
    {
        public string Type_ { get; set; }
        public float m_EnvironmentAnomalyThreshold { get; set; }

        public EnvironmentClassifier() 
        {
            Type_ = "EnvironmentClassifier";
        }
    }

    [fsObject]
    public class FaceClassifier : IClassifier
    {
        public string Type_ { get; set; }

        public FaceClassifier() 
        {
            Type_ = "FaceClassifier";
        }
    }

    #endregion

    #region Extractors Classes

    [fsObject]
    public class ImageExtractor : IExtractor
    {
        public string Type_ { get; set; }

        public ImageExtractor() 
        {
            Type_ = "ImageExtractor";
        }
    }

    [fsObject]
    public class ProximityExtractor : IExtractor
    {
        public string Type_ { get; set; }

        public ProximityExtractor()
        {
            Type_ = "ProximityExtractor";
        }
    }

    [fsObject]
    public class RemoteDeviceExtractor : IExtractor
    {
        public string Type_ { get; set; }

        public RemoteDeviceExtractor() 
        {
            Type_ = "RemoteDeviceExtractor";
        }
    }

    [fsObject]
    public class TextExtractor : IExtractor
    {
        public string Type_ { get; set; }
        public float m_ConfidenceThreshold { get; set; }
        public float m_ConfidenceThresholdLocal { get; set; }
        public float m_EnergyAverageSampleCount { get; set; }
        public float m_EnergyTimeInterval { get; set; }
        public string[] m_FailureResponses { get; set; }
        public float m_MaxConfidence { get; set; }
        public float m_MinConfidence { get; set; }
        public float m_MinFailureResponseInterval { get; set; }
        public float m_MaxFailureResponsesCount { get; set; }
        public float m_NormalizedEnergyAvg { get; set; }
        public float m_StdDevThreshold { get; set; }

        public TextExtractor() 
        {
            Type_ = "TextExtractor";
        }
    }

    #endregion

    #region Sensor Classes

    [fsObject]
    public class Microphone : ISensor
    {
        public string Type_ { get; set; }
        public float m_RecordingBits { get; set; }
        public float m_RecordingChannels { get; set; }
        public float m_RecordingHZ { get; set; }

        public Microphone() 
        {
            Type_ = "Microphone";
        }
    }

    [fsObject]
    public class Sonar : ISensor
    {
        public string Type_ { get; set; }

        public Sonar() 
        {
            Type_ = "Sonar";
        }
    }

    [fsObject]
    public class NaoGaze : ISensor
    {
        public string Type_ { get; set; }
        public float m_Tolerance { get; set; }

        public NaoGaze() 
        {
            Type_ = "NaoGaze";
        }
    }

    [fsObject]
    public class LocalSpeechToText : ISensor
    {
        public string Type_ { get; set; }
        public float m_MinConfidence { get; set; }
        public string[] m_VocabularyList { get; set; }

        public LocalSpeechToText() 
        {
            Type_ = "LocalSpeechToText";
        }
    }

    [fsObject]
    public class Network : ISensor
    {
        public string Type_ { get; set; }
        public string[] m_Addresses { get; set; }
        public float m_NetworkCheckInterval { get; set; }

        public Network() 
        {
            Type_ = "Network";
        }
    }

    [fsObject]
    public class RemoteDevice : ISensor
    {
        public string Type_ { get; set; }
        public RestCall[] m_Rests { get; set; }
        public float m_fPollInterval { get; set; }

        public RemoteDevice() 
        {
            Type_ = "RemoteDevice";
        }
    }

    [fsObject]
    public class HealthSensor : ISensor
    {
        public string Type_ { get; set; }
        public string[] m_ErrorDiagnosis { get; set; }
        public float m_HealthSensorCheckInterval { get; set; }
        public float m_fLowBatteryThreshold { get; set; }
        public string[] m_StablePostures { get; set; }
        public string[] m_SensorReadings { get; set; }

        public HealthSensor() 
        {
            Type_ = "HealthSensor";
        }
    }

    #endregion

    #region Services Classes

    [fsObject]
    public class ServiceRobotGateway : IService
    {
        public string Type_ { get; set; }
        public int m_MaxCacheAge { get; set; }
        public int m_MaxCacheSize { get; set; }
        public string[] m_PersistLogFilter { get; set; }
        public int m_PersistLogInterval { get; set; }
        public int m_PersistLogLevel { get; set; }
        public int m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceRobotGateway() 
        {
            Type_ = "RobotGateway";
        }
    }

    [fsObject]
    public class ServicePackageStore : IService
    {
        public string Type_ { get; set; }
        public int m_MaxCacheAge { get; set; }
        public int m_MaxCacheSize { get; set; }
        public int m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServicePackageStore() 
        {
            Type_ = "PackageStore";
        }
    }

    [fsObject]
    public class ServiceXRAY : IService
    {
        public string Type_ { get; set; }
        public int m_MaxCacheAge { get; set; }
        public int m_MaxCacheSize { get; set; }
        public int m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceXRAY() 
        {
            Type_ = "XRAY";
        }
    }

    [fsObject]
    public class ServiceFacialRecognition : IService
    {
        public string Type_ { get; set; }
        public string m_Group { get; set; }
        public string m_Key { get; set; }
        public float m_MaxCacheAge{ get; set; }
        public float m_MaxCacheSize { get; set; }
        public float m_RequestTimeout { get; set; }
        public string m_Secret { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceFacialRecognition() 
        {
            Type_ = "FacialRecognition";
        }
    }
        
    [fsObject]
    public class ServiceSMS : IService
    {
        public string Type_ { get; set; }
        public string m_FromNumber { get; set; }
        public string m_Key { get; set; }
        public float m_MaxCacheAge { get; set; }
        public float m_MaxCacheSize { get; set; }
        public float m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceSMS() 
        {
            Type_ = "SMS";
        }
    }

    [fsObject]
    public class ServiceDialog : IService
    {
        public string Type_ { get; set; }
        public float m_MaxCacheAge { get; set; }
        public float m_MaxCacheSize { get; set; }
        public float m_MaxDialogAge { get; set; }
        public float m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceDialog() 
        {
            Type_ = "Dialog";
        }
    }

    [fsObject]
    public class ServiceSpeechToText : IService
    {
        public string Type_ { get; set; }
        public bool m_Continous { get; set; }
        public bool m_DetectSilence { get; set; }
        public bool m_Interium { get; set; }
        public float m_MaxAlternatives { get; set; }
        public float m_MaxAudioQueueSize { get; set; }
        public float m_MaxCacheAge { get; set; }
        public float m_MaxCacheSize { get; set; }
        public string m_RecognizeModel { get; set; }
        public float m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public float m_SilenceThreshold { get; set; }
        public bool m_Timestamps { get; set; }
        public bool m_WordConfidence { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceSpeechToText() 
        {
            Type_ = "SpeechToText";
        }
    }

    [fsObject]
    public class ServiceTextToSpeech : IService
    {
        public string Type_ { get; set; }
        public float m_MaxCacheAge { get; set; }
        public float m_MaxCacheSize { get; set; }
        public float m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public string m_Voice { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceTextToSpeech() 
        {
            Type_ = "TextToSpeech";
        }
    }

    [fsObject]
    public class ServiceNLC : IService
    {
        public string Type_ { get; set; }
        public float m_MaxCacheAge { get; set; }
        public float m_MaxCacheSize { get; set; }
        public float m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceNLC() 
        {
            Type_ = "NaturalLanguageClassifier";
        }
    }

    [fsObject]
    public class ServiceRelationshipExtraction : IService
    {
        public string Type_ { get; set; }
        public float m_MaxCacheAge { get; set; }
        public float m_MaxCacheSize { get; set; }
        public float m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceRelationshipExtraction() 
        {
            Type_ = "RelationshipExtraction";
        }
    }

    [fsObject]
    public class ServiceAlchemy : IService
    {
        public string Type_ { get; set; }
        public float m_MaxCacheAge { get; set; }
        public float m_MaxCacheSize { get; set; }
        public float m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceAlchemy() 
        {
            Type_ = "Alchemy";
        }
    }

    [fsObject]
    public class ServiceURL : IService
    {
        public string Type_ { get; set; }
        public string m_AvailabilitySuffix { get; set; }
        public string m_FunctionalSuffix { get; set; }
        public float m_MaxCacheAge { get; set; }
        public float m_MaxCacheSize { get; set; }
        public float m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceURL()
        {
            Type_ = "URLService";
        }
    }

    [fsObject]
    public class ServiceLanguageTranslation : IService
    {
        public string Type_ { get; set; }
        public float m_MaxCacheAge { get; set; }
        public float m_MaxCacheSize { get; set; }
        public float m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceLanguageTranslation()
        {
            Type_ = "LanguageTranslation";
        }
    }

    [fsObject]
    public class ServiceWeatherInsights : IService
    {
        public string Type_ { get; set; }
        public float m_Latitude { get; set; }
        public float m_Longitude { get; set; }
        public string m_Units { get; set; }
        public string m_Language { get; set; }
        public float m_MaxCacheAge { get; set; }
        public float m_MaxCacheSize { get; set; }
        public float m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceWeatherInsights()
        {
            Type_ = "WeatherInsights";
        }
    }

    [fsObject]
    public class ServiceDeepQA : IService
    {
        public string Type_ { get; set; }
        public float m_MaxCacheAge { get; set; }
        public float m_MaxCacheSize { get; set; }
        public float m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceDeepQA()
        {
            Type_ = "DeepQA";   
        }
    }

    [fsObject]
    public class ServiceVisualRecognition : IService
    {
        public string Type_ { get; set; }
        public float m_MaxCacheAge { get; set; }
        public float m_MaxCacheSize { get; set; }
        public float m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceVisualRecognition()
        {
            Type_ = "VisualRecognition";   
        } 
    }

    [fsObject]
    public class ServiceTennisService : IService
    {
        public string Type_ { get; set; }
        public float m_MaxCacheAge { get; set; }
        public float m_MaxCacheSize { get; set; }
        public float m_RequestTimeout { get; set; }
        public string m_ServiceId { get; set; }
        public bool m_bCacheEnabled { get; set; }

        public ServiceTennisService()
        {
            Type_ = "Tennis";
        }
    }
        
    #endregion

    #endregion
}