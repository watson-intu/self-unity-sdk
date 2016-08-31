///**
//* Copyright 2015 IBM Corp. All Rights Reserved.
//*
//* Licensed under the Apache License, Version 2.0 (the "License");
//* you may not use this file except in compliance with the License.
//* You may obtain a copy of the License at
//*
//*      http://www.apache.org/licenses/LICENSE-2.0
//*
//* Unless required by applicable law or agreed to in writing, software
//* distributed under the License is distributed on an "AS IS" BASIS,
//* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//* See the License for the specific language governing permissions and
//* limitations under the License.
//*
//*/
//#define ENABLE_DEBUGGING
////#define USE_BEST_HTTP
//#define USE_WEBSOCKET_SHARP

//using IBM.Watson.DeveloperCloud.Logging;
//using IBM.Watson.DeveloperCloud.Connection;
//using IBM.Watson.DeveloperCloud.Utilities;
//using MiniJSON;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using FullSerializer;
//using System.Collections;
//using System.IO;
//using UnityEngine;
//using UnityEngine.SocialPlatforms;

//#if USE_BEST_HTTP
//using BestHTTP.WebSocket;
//#elif USE_WEBSOCKET_SHARP
//using WebSocketSharp;
//#endif

//namespace IBM.Watson.DeveloperCloud.Services.Self.v1
//{
//    public class SelfWebSocket : IWatsonService
//    {

//        #region Public Data
//        public enum ConnectionState
//        {
//            NotConnected,
//            Connecting,
//            Connected,
//            Disconnecting,

//        }

//        public enum SubscriptionState
//        {
//            NotSubscribed,
//            Subscribing,
//            Subscribed,
//        }

//        public enum SelfParentChildrenStatus
//        {
//            Adding,
//            Finished,
//            Updating,
//            UpdatingFinished
//        }


//        public static SelfWebSocket Instance
//        {
//            get
//            {
//                return Singleton<SelfWebSocket>.Instance;
//            }
//        }

//        public ConnectionState RobotConnectionState
//        {
//            get
//            {
//                return m_RobotState;
//            }
//        }

//        public bool DiscoverAllChilderen
//        {
//            get
//            {
//                return m_DiscoverAllChilderen;
//            }
//        }

//        public SelfServer HeadNodeSelfServer
//        {
//            get
//            {
//                return m_HeadNodeSelfServer;
//            }
//        }

//        public SelfServer HeadNodeSelfServerToCompare
//        {
//            get
//            {
//                return m_HeadNodeSelfServerTemp;
//            }
//        }

//        public SelfServer[] SubscribedSelfServers
//        {
//            get
//            {
//                SelfServer[] subscribedList = null;

//                if (m_HeadNodeSelfServer != null)
//                {
//                    subscribedList = new SelfServer[SubscribedTargets.Length];

//                    for (int i = 0; i < subscribedList.Length; i++)
//                    {
//                        subscribedList[i] = m_HeadNodeSelfServer.Find(SubscribedTargets[i]);
//                    }
//                }
//                else if (m_HeadNodeSelfServerTemp != null)
//                {
//                    subscribedList = new SelfServer[SubscribedTargets.Length];

//                    for (int i = 0; i < subscribedList.Length; i++)
//                    {
//                        subscribedList[i] = m_HeadNodeSelfServerTemp.Find(SubscribedTargets[i]);
//                    }
//                }
//                else
//                {

//                }

//                return subscribedList;
//            }
//        }

//        public string[] SubscribedTargets
//        {
//            get
//            {
//                return m_SubscribedTargets;
//            }
//        }

//        public delegate void OnGetHealthSubscriptionData(SelfThingHealth selfThingHealth);
//        public delegate void OnGetSubscriptionData(SelfSubscriptionData subscriptionMessage);
//        public delegate void OnGetRobotMessage(SelfMessage robotMessage);
//        public delegate void OnDiscovery(SelfParentChildrenStatus selfParentChildrenStatus);
//        public delegate void OnConnectionStateChangeDelegate(ConnectionState connectionState);
//        public delegate void OnSubscriptionStateChangeDelegate(SubscriptionState subscriptionState);
//        public delegate void OnCloseConnection(bool intentionally);
//        public delegate void OnGetSubscriptionDataTimeOut();
//        public OnGetRobotMessage m_OnGetQueryMessage;
//        public OnGetRobotMessage m_OnGetPublishMessage;
//        public OnGetHealthSubscriptionData m_OnGetHealthData, m_OnTimeOutHealthData;
//        public OnDiscovery m_OnDiscoveryStateChange;
//        public OnConnectionStateChangeDelegate m_OnConnectionStateChanged;
//        public OnSubscriptionStateChangeDelegate m_OnSubscriptionStateChanged;
//        public OnCloseConnection m_OnCloseConnection;
//        #endregion

//        #region Private Data
//        private const string SERVICE_ID = "SelfWebSocket";
//        public const string m_RobotWebSocketConnectionFormat = "ws://{0}:{1}/stream";

//        private static fsSerializer sm_Serializer = new fsSerializer();
//        private bool m_DiscoverAllChilderen = true;
//        private WebSocket m_WebSocketRobot = null;
//        private ConnectionState m_RobotState = ConnectionState.NotConnected;
//        private Queue<SelfMessage> m_QueueQueryMessagesFromSelf = new Queue<SelfMessage>();
//        private Queue<SelfMessage> m_QueuePublishMessagesFromSelf = new Queue<SelfMessage>();
//        private Queue<SelfParentChildrenStatus> m_QueueDiscoveryOnSelf = new Queue<SelfParentChildrenStatus>();
//        private Queue<ConnectionState> m_QueueConnectionStateChange = new Queue<ConnectionState>();
//        private Queue<SubscriptionState> m_QueueSubscriptionStateChange = new Queue<SubscriptionState>();
//        private Queue<bool> m_QueueConnectionClosed = new Queue<bool>();
//        private SelfServer m_HeadNodeSelfServer = null;
//        private SelfServer m_HeadNodeSelfServerTemp = null;
//        private int m_AsyncMessageCheckId = -1;
//        private bool m_SelfSubscriptionTimeOutEnable = false;
//        private string[] m_SubscribedTargets = null;
//        private string[] m_PreviouslySubscribedTargets = null;
//        private SelfTopic[] m_SubscribedTopics = null;
//        private SelfTopic[] m_PreviouslySubscribedTopics = null;

//        private bool m_SelfParentPingEnable = false;
//        private int m_SelfParentPingID = -1;

//        private bool m_SelfSubscriptionPingEnable = false;
//        private int m_SelfSubscriptionPingID = -1;

//        private DateTime m_LastTimeInitializedSelfParentServer = default(DateTime);
//        private DateTime m_LastTimeInitializedSelfParentServerTemp = default(DateTime);
//        private DateTime m_LastTimeGotPublishMessage = default(DateTime);
//        private SubscriptionState m_SubscriptionState = SubscriptionState.NotSubscribed;

//        private const float TIME_TO_CHECK_FOR_CHILDREN = 10.0f;
//        private const float RESPONSE_FROM_CHILD_TIMEOUT = 8.0f;
//        #endregion

//        #region Starting Web Socket Connection

//        public void ConnectRobot(string robotIP, int robotPort, string localMachineMAC, string localMachineGroupKey)
//        {

//            Log.Status("SelfWebSocket", "Starting Robot Connection to {0} : {1}", robotIP, robotPort.ToString());

//            if (m_WebSocketRobot != null)
//            {
//#if USE_BEST_HTTP
//                if (m_WebSocketRobot.IsOpen)
//                {
//                    Log.Error("SelfWebSocket", "There is already a websocket connection.");
//                    return;
//                }
//                else
//                {
//                    Log.Warning("SelfWebSocket", "WebSocketRobot is not null but it is not connected.");
//                }
//#elif USE_WEBSOCKET_SHARP
//                if (m_WebSocketRobot.IsConnected)
//                {
//                    Log.Error("SelfWebSocket", "There is already a websocket connection.");
//                    return;
//                }
//                else
//                {
//                    Log.Warning("SelfWebSocket", "WebSocketRobot is not null but it is not connected.");
//                }
//#endif


//            }
//            if (string.IsNullOrEmpty(localMachineMAC))
//                localMachineMAC = Utility.MacAddress;
//            if (string.IsNullOrEmpty(localMachineGroupKey))
//                localMachineGroupKey = Utility.MacAddress;
//            if (m_AsyncMessageCheckId > 0)
//                Runnable.Stop(m_AsyncMessageCheckId);
//            if (m_HeadNodeSelfServer != null)
//                m_HeadNodeSelfServer.Clear();
//            if (!m_SelfSubscriptionTimeOutEnable)
//                Runnable.Run(AsyncCheckForSelfMessageTimeouts());
//            if (!m_SelfParentPingEnable)
//                m_SelfParentPingID = Runnable.Run(AsyncCheckForConnectedChildren());
//            if (!m_SelfSubscriptionPingEnable)
//                m_SelfSubscriptionPingID = Runnable.Run(AsyncCheckForSubscription());

//            m_QueuePublishMessagesFromSelf.Clear();
//            SetCurrentRobotConnectState(ConnectionState.Connecting);
//            //string webSocketHost = string.Format(m_RobotWebSocketConnectionFormat, robotIP, robotPort); 

//            string webSocketHost = robotIP + "/stream";

//            if (!(webSocketHost.Contains("ws")))
//            {
//                webSocketHost = string.Format(m_RobotWebSocketConnectionFormat, robotIP, robotPort);
//            }

//#if USE_BEST_HTTP
//            Dictionary<string, string> customHeaders = new Dictionary<string, string>();
//            customHeaders.Add("groupId", localMachineGroupKey); //TODO: Fix 
//            customHeaders.Add("selfId", localMachineMAC);
//            m_WebSocketRobot = new WebSocket(new Uri(webSocketHost), customHeaders);
//#elif USE_WEBSOCKET_SHARP
//            m_WebSocketRobot = new WebSocket(webSocketHost);
//            m_WebSocketRobot.Headers = new Dictionary<string, string>();
//            m_WebSocketRobot.Headers.Add("groupId", localMachineGroupKey); //TODO: Fix 
//            m_WebSocketRobot.Headers.Add("selfId", localMachineMAC);
//#endif


//            m_WebSocketRobot.OnMessage += OnMessageWebSocket;
//            m_WebSocketRobot.OnOpen += OnOpenWebSocket;
//            m_WebSocketRobot.OnError += OnErrorWebSocket;

//#if USE_BEST_HTTP
//            m_WebSocketRobot.OnClosed += OnCloseWebSocket;
//            m_WebSocketRobot.OnBinary += HandleOnWebSocketBinaryDelegate;
//            m_WebSocketRobot.OnErrorDesc += HandleOnWebSocketErrorDescriptionDelegate;
//            m_WebSocketRobot.OnIncompleteFrame += HandleOnWebSocketIncompleteFrameDelegate;
//            m_AsyncMessageCheckId = Runnable.Run(AsyncCheckForSelfMessage());
//            m_WebSocketRobot.Open();
//#elif USE_WEBSOCKET_SHARP
//            m_WebSocketRobot.OnClose += OnCloseWebSocket;
//            m_AsyncMessageCheckId = Runnable.Run(AsyncCheckForSelfMessage());
//            m_WebSocketRobot.ConnectAsync();
//#endif


//        }

//#if USE_BEST_HTTP
//        void HandleOnWebSocketIncompleteFrameDelegate (WebSocket webSocket, BestHTTP.WebSocket.Frames.WebSocketFrameReader frame)
//        {
//#if ENABLE_DEBUGGING
//            Log.Status("SelfWebSocket", "HandleOnWebSocketIncompleteFrameDelegate - {0}", frame);
//#endif
//        }
//#endif

//        void HandleOnWebSocketErrorDescriptionDelegate(WebSocket webSocket, string reason)
//        {
//#if ENABLE_DEBUGGING
//            Log.Status("SelfWebSocket", "HandleOnWebSocketErrorDescriptionDelegate - {0}", reason);
//#endif
//        }

//        void HandleOnWebSocketBinaryDelegate(WebSocket webSocket, byte[] data)
//        {
//#if ENABLE_DEBUGGING
//            Log.Status("SelfWebSocket", "HandleOnWebSocketBinaryDelegate - {0}", ((data != null) ? Encoding.UTF8.GetString(data) : ""));
//#endif
//        }

//        public void CloseConnectionRobot()
//        {
//            if (m_WebSocketRobot != null)
//            {
//                //First unscribe from the topics we subscribed already
//                if (m_SubscribedTopics != null && m_SubscribedTargets != null)
//                {
//                    SubscribeTopics(m_SubscribedTopics, m_SubscribedTargets, SubscriptionMessage.SubscriptionType.Unsubscribe);
//                }

//                SetCurrentRobotConnectState(ConnectionState.Disconnecting);
//#if ENABLE_DEBUGGING
//                Log.Debug("SelfWebSocket", "WebSocketRobot is closing");
//#endif

//                if (m_HeadNodeSelfServer != null)
//                    m_HeadNodeSelfServer.Clear();

//#if USE_BEST_HTTP
//                m_WebSocketRobot.Close();
//                OnCloseWebSocket(m_WebSocketRobot, 0, "");  //We are forcing to call close method
//#elif USE_WEBSOCKET_SHARP
//                m_WebSocketRobot.CloseAsync();
//#endif
//            }
//            else
//                Log.Warning("SelfWebSocket", "WebSocketRobot is null, can't be closed.");
//        }

//#if USE_BEST_HTTP
//        public void OnMessageWebSocket(WebSocket ws, string message)
//        {
//#if ENABLE_DEBUGGING
//            Log.Debug("SelfWebSocket", "<- From Server. OnMessageWebSocket: {0}" , message);
//#endif

//            SelfMessage response = Utility.DeserializeResponse<SelfMessage>(message, null);

//#elif USE_WEBSOCKET_SHARP
//        public void OnMessageWebSocket(object sender, MessageEventArgs messageEventArgs)
//        {
//#if ENABLE_DEBUGGING
//            Log.Debug("SelfWebSocket", "<- From Server. OnMessageWebSocket: {0}", messageEventArgs.Data);
//#endif

//            SelfMessage response = Utility.DeserializeResponse<SelfMessage>(messageEventArgs.RawData, null);
//#endif

//            if (response != null)
//            {
//                switch (response.MessageType)
//                {
//                    case SelfMessage.RobotMessageType.Authentication:
//                        //SetCurrentRobotConnectState(ConnectionState.Connected);
//                        break;
//                    case SelfMessage.RobotMessageType.Query:
//                        OnQueryMessageFromSelf(response);
//                        break;
//                    case SelfMessage.RobotMessageType.Publish:
//                        OnPublishMessageFromSelf(response);
//                        break;
//                    default:
//                        break;
//                }
//            }

//        }

//#if USE_BEST_HTTP
//        public void OnOpenWebSocket(WebSocket ws)
//#elif USE_WEBSOCKET_SHARP
//        public void OnOpenWebSocket(object sender, EventArgs e)
//#endif
//        {
//            Log.Status("SelfWebSocket", "OnOpenWebSocket - Connected to Robot");
//            SetCurrentRobotConnectState(ConnectionState.Connected);
//        }

//#if USE_BEST_HTTP
//        public void OnErrorWebSocket(WebSocket ws, Exception ex)
//        {
//            Log.Status("SelfWebSocket", "OnErrorWebSocket: {0} - {1} " , ex.Message , ex.ToString());
//        }
//#elif USE_WEBSOCKET_SHARP
//        public void OnErrorWebSocket(object sender, WebSocketSharp.ErrorEventArgs e)
//        {
//            Log.Status("SelfWebSocket", "OnErrorWebSocket: {0} - {1} ", e.Message, e.Exception.ToString());
//        }
//#endif

//#if USE_BEST_HTTP
//            public void OnCloseWebSocket(WebSocket ws, UInt16 code, string message)
//            {
//                Log.Status("SelfWebSocket", "OnCloseWebSocket. {0} {1}", code.ToString(), message);
//#elif USE_WEBSOCKET_SHARP
//        public void OnCloseWebSocket(object sender, CloseEventArgs e)
//        {
//            Log.Status("SelfWebSocket", "OnCloseWebSocket. {0} {1}", e.Reason, e.ToString());
//#endif
//            m_WebSocketRobot.OnMessage -= OnMessageWebSocket;
//            m_WebSocketRobot.OnOpen -= OnOpenWebSocket;
//            m_WebSocketRobot.OnError -= OnErrorWebSocket;
//#if USE_BEST_HTTP
//            m_WebSocketRobot.OnClosed -= OnCloseWebSocket;
//            m_WebSocketRobot.OnBinary -= HandleOnWebSocketBinaryDelegate;
//            m_WebSocketRobot.OnErrorDesc -= HandleOnWebSocketErrorDescriptionDelegate;
//            m_WebSocketRobot.OnIncompleteFrame -= HandleOnWebSocketIncompleteFrameDelegate;
//#elif USE_WEBSOCKET_SHARP
//            m_WebSocketRobot.OnClose -= OnCloseWebSocket;
//#endif


//            bool closeConnectionIntentional = (m_RobotState == ConnectionState.Disconnecting);
//            SetCurrentRobotConnectState(ConnectionState.NotConnected);


//            m_WebSocketRobot = null;

//            if (m_HeadNodeSelfServer != null)
//                m_HeadNodeSelfServer.Clear();

//            if (!closeConnectionIntentional)
//            {
//                m_PreviouslySubscribedTargets = m_SubscribedTargets;
//                m_PreviouslySubscribedTopics = m_SubscribedTopics;
//            }
//            else
//            {
//                m_PreviouslySubscribedTargets = null;
//                m_PreviouslySubscribedTopics = null;
//            }

//            //Stop Pinging - because we close connection. It will begin one more time after connection established
//            if (m_SelfParentPingEnable)
//            {
//                m_SelfParentPingEnable = false;
//                Runnable.Stop(m_SelfParentPingID);
//            }

//            //Stop Pinging for Subscription
//            if (m_SelfSubscriptionPingEnable)
//            {
//                m_SelfSubscriptionPingEnable = false;
//                Runnable.Stop(m_SelfSubscriptionPingID);
//            }

//            m_SubscribedTargets = null;
//            m_SubscribedTopics = null;
//        }


//        public void SetCurrentRobotConnectState(ConnectionState newConnectionState)
//        {
//            if (m_RobotState != newConnectionState)
//            {
//                if (newConnectionState == ConnectionState.Connected)
//                {
//                    //Start sending query message
//                    QueryMessage message = new QueryMessage(null);

//                    fsData data;
//                    if (sm_Serializer.TrySerialize<QueryMessage>(message, out data).Succeeded)
//                    {
//                        string jsonString = fsJsonPrinter.CompressedJson(data);
//#if ENABLE_DEBUGGING
//                        Log.Debug("SelfWebSocket", "-> To Server. Initial Query of Topics. Localhost MAC: {0} - Sending JSON : {1}", Utility.MacAddress, jsonString);
//#endif
//                        m_WebSocketRobot.Send(jsonString);
//                    }
//                    else
//                    {
//                        Log.Error("SelfWebSocket", "Error parsing to JSON : {0}", message.ToString());
//                    }
//                }

//#if ENABLE_DEBUGGING
//                Log.Debug("SelfWebSocket", "Connection State Change from {0} to {1}", m_RobotState, newConnectionState);
//#endif

//                if (newConnectionState == ConnectionState.NotConnected)
//                {
//                    ChangeSubscriptionState(SubscriptionState.NotSubscribed);
//                    //Determining on connection close whether it is intentional or unintentional 
//                    m_QueueConnectionClosed.Enqueue(m_RobotState == ConnectionState.Disconnecting);
//                }

//                m_RobotState = newConnectionState;
//                m_QueueConnectionStateChange.Enqueue(newConnectionState);


//            }
//            else
//            {
//                Log.Warning("SelfWebSocket", "Robot state doesn't change. Current State: {0}", newConnectionState.ToString());
//            }
//        }

//        public void QuerySelf(string[] targets)
//        {
//            if (m_RobotState == ConnectionState.Connected)
//            {
//                //Start sending query message
//                QueryMessage message = new QueryMessage(targets); //selfRouteMACToQuery

//                fsData data;
//                if (sm_Serializer.TrySerialize<QueryMessage>(message, out data).Succeeded)
//                {
//                    string jsonString = fsJsonPrinter.CompressedJson(data);
//#if ENABLE_DEBUGGING
//                    Log.Debug("SelfWebSocket", "-> To Server. Query of Topics. Sending JSON : {0}", jsonString);
//#endif
//                    m_WebSocketRobot.Send(jsonString);
//                }
//                else
//                {
//                    Log.Error("SelfWebSocket", "Error parsing to JSON : {0}", message.ToString());
//                }
//            }
//            else
//            {
//                Log.Warning("SelfWebSocket", "Robot is not connected, can't send query message. Current State: {0}", m_RobotState.ToString());
//            }
//        }

//        public void SubscribeTopics(SelfTopic[] topicsToSubscribe, string[] targets, SubscriptionMessage.SubscriptionType subscriptionType)
//        {
//            if (m_RobotState == ConnectionState.Connected)
//            {
//                //Start sending query message
//                SubscriptionMessage message = new SubscriptionMessage(topicsToSubscribe, targets, subscriptionType);

//                if (subscriptionType == SubscriptionMessage.SubscriptionType.Subscribe)
//                {
//                    m_SubscribedTargets = targets;
//                    m_SubscribedTopics = topicsToSubscribe;
//                    ChangeSubscriptionState(SubscriptionState.Subscribing);
//                }
//                else
//                {
//                    ChangeSubscriptionState(SubscriptionState.NotSubscribed);
//                }

//                fsData data;
//                if (sm_Serializer.TrySerialize<SubscriptionMessage>(message, out data).Succeeded)
//                {
//                    string jsonString = fsJsonPrinter.CompressedJson(data);
//#if ENABLE_DEBUGGING
//                    Log.Debug("SelfWebSocket", "-> To Server. SubscribeTopics. Sending JSON : {0}", jsonString);
//#endif
//                    m_WebSocketRobot.Send(jsonString);
//                }
//                else
//                {
//                    Log.Error("SelfWebSocket", "Error parsing to JSON : {0}", message.ToString());
//                }
//            }
//            else
//            {
//                Log.Warning("SelfWebSocket", "Robot is not connected, can't send subscription message. Current State: {0}", m_RobotState.ToString());
//            }
//        }


//        private void ChangeSubscriptionState(SubscriptionState newState)
//        {
//            if (m_SubscriptionState != newState)
//            {
//                Log.Warning("ChangeSubscriptionState", "New SubscriptionState: {0}", newState);

//                m_SubscriptionState = newState;
//                m_QueueSubscriptionStateChange.Enqueue(newState);
//            }
//        }


//        #endregion

//        #region Get Query Message From Server
//        public void OnQueryMessageFromSelf(SelfMessage robotMessage)
//        {
//#if ENABLE_DEBUGGING
//            Log.Status("SelfWebSocket", "OnQueryMessageFromSelf: {0} - HeadNode: {1}", robotMessage.ToString(), m_HeadNodeSelfServer);
//#endif

//            if (m_HeadNodeSelfServer == null || !m_HeadNodeSelfServer.HasFullData)
//            {
//#if ENABLE_DEBUGGING
//                Log.Debug("SelfWebSocket", "OnQueryMessageFromSelf: Creating Head Node");
//#endif
//                //Head Node
//                m_HeadNodeSelfServer = new SelfServer(robotMessage);
//                m_LastTimeInitializedSelfParentServer = DateTime.Now;

//                m_QueueDiscoveryOnSelf.Enqueue(m_HeadNodeSelfServer.NeedDiscovery ? SelfParentChildrenStatus.Adding : SelfParentChildrenStatus.Finished);
//            }
//            else if (m_HeadNodeSelfServer.NeedDiscovery)
//            {
//#if ENABLE_DEBUGGING
//                Log.Debug("SelfWebSocket", "OnQueryMessageFromSelf: Adding Children");
//#endif
//                m_HeadNodeSelfServer.AddChildren(robotMessage);
//                m_QueueDiscoveryOnSelf.Enqueue(m_HeadNodeSelfServer.NeedDiscovery ? SelfParentChildrenStatus.Adding : SelfParentChildrenStatus.Finished);
//            }
//            else
//            {
//                //We got the all information for the main Self Parent
//                //Now we are creating a temp tree to see the changes on the main tree
//                if (m_HeadNodeSelfServerTemp == null || !m_HeadNodeSelfServerTemp.HasFullData)
//                {
//#if ENABLE_DEBUGGING
//                    Log.Debug("SelfWebSocket", "OnQueryMessageFromSelf: Updating New Tree Node. ");
//#endif

//                    m_HeadNodeSelfServerTemp = new SelfServer(robotMessage);
//                    m_LastTimeInitializedSelfParentServerTemp = DateTime.Now;

//                    m_QueueDiscoveryOnSelf.Enqueue(m_HeadNodeSelfServerTemp.NeedDiscovery ? SelfParentChildrenStatus.Updating : SelfParentChildrenStatus.UpdatingFinished);
//                }
//                else
//                {
//#if ENABLE_DEBUGGING
//                    Log.Debug("SelfWebSocket", "OnQueryMessageFromSelf: Updating new Tree childeren nodes ");
//#endif

//                    m_HeadNodeSelfServerTemp.AddChildren(robotMessage);
//                    m_QueueDiscoveryOnSelf.Enqueue(m_HeadNodeSelfServerTemp.NeedDiscovery ? SelfParentChildrenStatus.Updating : SelfParentChildrenStatus.UpdatingFinished);
//                }

//            }


//            m_QueueQueryMessagesFromSelf.Enqueue(robotMessage);
//        }


//        #endregion

//        #region Get Publish Message From Server

//        public void OnPublishMessageFromSelf(SelfMessage robotMessage)
//        {
//#if ENABLE_DEBUGGING
//            if (robotMessage.HasSubscriptionData)
//                Log.Status("SelfWebSocket", "OnPublishMessageFromSelf: {0}", robotMessage.SubscriptionData.ToString());
//            if (robotMessage.HasBodyData)
//                Log.Status("SelfWebSocket", "OnPublishMessageFromSelf Body: {0}", robotMessage.SelfBodyData.ToString());
//#endif

//            if (robotMessage.HasBodyData)
//            {
//                if (m_HeadNodeSelfServer != null)
//                {
//                    if (robotMessage.selfId != null)
//                    {
//                        SelfServer selfServer = m_HeadNodeSelfServer.Find(robotMessage.selfId);
//                        if (selfServer != null)
//                        {
//                            selfServer.selfBody = robotMessage.SelfBodyData;
//                        }
//                        else
//                        {
//                            Log.Error("SelfWebSocket", "selfServer is NULL. SelfID: {0}", robotMessage.selfId);
//                        }
//                    }
//                    else
//                    {
//                        Log.Warning("SelfWebSocket", "robotMessage selfId is null: {0}", robotMessage);
//                    }
//                }
//                else
//                {
//                    Log.Error("SelfWebSocket", "m_HeadNodeSelfServer is NULL");
//                }
//            }

//            ChangeSubscriptionState(SubscriptionState.Subscribed);
//            m_LastTimeGotPublishMessage = System.DateTime.Now;
//            m_QueuePublishMessagesFromSelf.Enqueue(robotMessage);
//        }

//        #endregion


//        #region Asyc Checks For WebSocket

//        public IEnumerator AsyncCheckForSubscription()
//        {
//            m_SelfSubscriptionPingEnable = true;
//            while (m_SelfSubscriptionPingEnable)
//            {
//                yield return new WaitForSeconds(5.0f);

//                if (m_RobotState == ConnectionState.Connected)
//                {

//                    //We may reconnected due to some error - so checking previous subscriptions
//                    if (m_PreviouslySubscribedTopics != null &&
//                        m_SubscribedTopics == null &&
//                        m_PreviouslySubscribedTargets != null &&
//                        m_SubscribedTargets == null)
//                    {
//                        Log.Warning("AsyncCheckForSubscription", "After reconnect to parent. Subscribe previous robot with previous topic.");
//                        //Try subscribing the robots with targets one more time
//                        SubscribeTopics(m_PreviouslySubscribedTopics, m_PreviouslySubscribedTargets, SubscriptionMessage.SubscriptionType.Subscribe);
//                    }
//                    else if (m_SubscribedTopics != null &&
//                        m_SubscribedTargets != null)
//                    {
//                        Log.Warning("AsyncCheckForSubscription", "Check if need to reconnect. Last Time Push Message: {0} , diff from now: {1}", m_LastTimeGotPublishMessage, (DateTime.Now - m_LastTimeGotPublishMessage).TotalSeconds);
//                        //It is already subscribed to robot - checking timeout
//                        if (m_LastTimeGotPublishMessage != default(DateTime) && (DateTime.Now - m_LastTimeGotPublishMessage).TotalSeconds > 60)
//                        {
//                            ChangeSubscriptionState(SubscriptionState.NotSubscribed);

//                            Log.Warning("AsyncCheckForSubscription", "Trying to subscribe the same target with same topics");
//                            //we didn't receive any subscription message. Go and subscribe one more time.
//                            SubscribeTopics(m_SubscribedTopics, m_SubscribedTargets, SubscriptionMessage.SubscriptionType.Subscribe);
//                        }
//                    }
//                    else if (m_PreviouslySubscribedTopics == null &&
//                        m_SubscribedTopics == null &&
//                        m_PreviouslySubscribedTargets == null &&
//                        m_SubscribedTargets == null)
//                    {
//                        //Initial state - so wait to connect one of the robot
//                    }
//                    else
//                    {
//                        Log.Warning("AsyncCheckForSubscription", "Invalid state.");
//                    }
//                }
//                else
//                {
//                    //wait for connection established
//                }
//            }

//            yield break;
//        }



//        public IEnumerator AsyncCheckForConnectedChildren()
//        {
//            m_SelfParentPingEnable = true;
//            while (m_SelfParentPingEnable)
//            {

//                yield return new WaitForSeconds(TIME_TO_CHECK_FOR_CHILDREN);

//                //Check the updates are finished on the Temp tree 
//                if (m_RobotState == ConnectionState.Connected & m_HeadNodeSelfServerTemp != null)
//                {
//                    if ((m_HeadNodeSelfServerTemp.NeedDiscovery && (DateTime.Now - m_LastTimeInitializedSelfParentServerTemp).TotalSeconds > RESPONSE_FROM_CHILD_TIMEOUT) || !m_HeadNodeSelfServerTemp.NeedDiscovery)
//                    {
//                        m_HeadNodeSelfServerTemp.SetAllDiscovered();
//                        m_HeadNodeSelfServer.Clear();
//                        m_HeadNodeSelfServer = m_HeadNodeSelfServerTemp;
//                        m_HeadNodeSelfServerTemp = null;

//                        m_QueueDiscoveryOnSelf.Enqueue(m_HeadNodeSelfServer.NeedDiscovery ? SelfParentChildrenStatus.Updating : SelfParentChildrenStatus.UpdatingFinished);
//                    }
//                }

//                yield return new WaitForSeconds(1.0f);

//                //If there is no need to discover we should check the new children
//                if (m_RobotState == ConnectionState.Connected && m_HeadNodeSelfServer != null && !m_HeadNodeSelfServer.NeedDiscovery)
//                {
//                    Log.Warning("AsyncCheckForConnectedChildren", "Pinging Parent to check new childeren");
//                    if (m_HeadNodeSelfServerTemp != null)
//                        m_HeadNodeSelfServerTemp.Clear();
//                    QuerySelf(null);
//                }
//                //If server need discovery and it passed timeout; then we are discarding all child objects without full data
//                else if (m_RobotState == ConnectionState.Connected & m_HeadNodeSelfServer != null && m_HeadNodeSelfServer.NeedDiscovery && (DateTime.Now - m_LastTimeInitializedSelfParentServer).TotalSeconds > RESPONSE_FROM_CHILD_TIMEOUT)
//                {
//                    m_HeadNodeSelfServer.SetAllDiscovered();
//                    m_QueueDiscoveryOnSelf.Enqueue(m_HeadNodeSelfServer.NeedDiscovery ? SelfParentChildrenStatus.Adding : SelfParentChildrenStatus.Finished);
//                }
//                else
//                {
//                    //wait for connection established
//                }


//            }

//            yield break;
//        }

//        public IEnumerator AsyncCheckForSelfMessageTimeouts()
//        {
//            m_SelfSubscriptionTimeOutEnable = true;
//            while (m_SelfSubscriptionTimeOutEnable)
//            {
//                //TODO: find a better way to check the timeouts!
//                foreach (KeyValuePair<string, List<SubscriptionDataCallback>> entry in m_HealthSubscriptionCallbacks)
//                {
//                    foreach (SubscriptionDataCallback subscriptionDataCallback in entry.Value)
//                    {
//                        if (subscriptionDataCallback.callbackTimeOut != null
//                            && subscriptionDataCallback.lastTimeInvoke != default(DateTime)
//                            && !subscriptionDataCallback.HasTimeOutAlready
//                            && subscriptionDataCallback.IsTimeOut)
//                        {
//                            Log.Warning("AsyncCheckForSelfMessageTimeouts", "Timeout happened for given subscription data: {0}", subscriptionDataCallback.callback);
//                            subscriptionDataCallback.callbackTimeOut.Invoke();
//                            subscriptionDataCallback.HasTimeOutAlready = true;
//                        }
//                    }
//                }

//                yield return new WaitForSeconds(1.0f);
//            }

//            yield break;
//        }

//        public IEnumerator AsyncCheckForSelfMessage()
//        {
//            while (m_RobotState != ConnectionState.NotConnected)
//            {
//                while (m_QueueQueryMessagesFromSelf.Count > 0)
//                {
//                    SelfMessage robotMessage = m_QueueQueryMessagesFromSelf.Dequeue();
//                    OnQueryMessage(robotMessage);
//                }

//                while (m_QueuePublishMessagesFromSelf.Count > 0)
//                {
//                    SelfMessage robotMessage = m_QueuePublishMessagesFromSelf.Dequeue();
//                    OnSubscriptionMessage(robotMessage);
//                }

//                while (m_QueueDiscoveryOnSelf.Count > 0)
//                {
//                    SelfParentChildrenStatus discoveryState = m_QueueDiscoveryOnSelf.Dequeue();
//                    OnDiscoveryMessage(discoveryState);
//                }

//                while (m_QueueConnectionStateChange.Count > 0)
//                {
//                    ConnectionState connectionState = m_QueueConnectionStateChange.Dequeue();
//                    OnConnectionStateChange(connectionState);
//                }

//                while (m_QueueSubscriptionStateChange.Count > 0)
//                {
//                    SubscriptionState subscriptionState = m_QueueSubscriptionStateChange.Dequeue();
//                    OnSubscriptionStateChange(subscriptionState);
//                }

//                while (m_QueueConnectionClosed.Count > 0)
//                {
//                    bool connectionCloseIntentionally = m_QueueConnectionClosed.Dequeue();
//                    OnCloseConnectionIntentionally(connectionCloseIntentionally);
//                }

//                yield return null;
//            }

//            if (m_RobotState == ConnectionState.NotConnected)
//            {
//                while (m_QueueConnectionStateChange.Count > 0)
//                {
//                    ConnectionState connectionState = m_QueueConnectionStateChange.Dequeue();
//                    OnConnectionStateChange(connectionState);
//                }

//                while (m_QueueSubscriptionStateChange.Count > 0)
//                {
//                    SubscriptionState subscriptionState = m_QueueSubscriptionStateChange.Dequeue();
//                    OnSubscriptionStateChange(subscriptionState);
//                }

//                while (m_QueueConnectionClosed.Count > 0)
//                {
//                    bool connectionCloseIntentionally = m_QueueConnectionClosed.Dequeue();
//                    OnCloseConnectionIntentionally(connectionCloseIntentionally);
//                }
//            }

//            m_AsyncMessageCheckId = -1; //finished running this asyn call
//            yield break;
//        }
//        #endregion

//        #region Event Handler For Query Messages

//        private void OnQueryMessage(SelfMessage robotMessage)
//        {
//            if (m_OnGetQueryMessage != null)
//            {
//                m_OnGetQueryMessage.Invoke(robotMessage);
//            }
//        }

//        #endregion

//        #region Event Handler For Connection State / Subscription State change

//        private void OnConnectionStateChange(ConnectionState connectionState)
//        {
//            if (m_OnConnectionStateChanged != null)
//            {
//                m_OnConnectionStateChanged.Invoke(connectionState);
//            }
//        }

//        private void OnSubscriptionStateChange(SubscriptionState subscriptoinState)
//        {
//            if (m_OnSubscriptionStateChanged != null)
//            {
//                m_OnSubscriptionStateChanged.Invoke(subscriptoinState);
//            }
//        }


//        #endregion

//        #region Event Handler For Discovery Messages

//        private void OnDiscoveryMessage(SelfParentChildrenStatus discoveryStatus)
//        {
//            if (m_OnDiscoveryStateChange != null)
//                m_OnDiscoveryStateChange.Invoke(discoveryStatus);
//        }

//        #endregion

//        #region Event Handler on Close Connections

//        private void OnCloseConnectionIntentionally(bool intentionally)
//        {
//            if (m_OnCloseConnection != null)
//            {
//                m_OnCloseConnection.Invoke(intentionally);
//            }
//        }

//        #endregion

//        #region Register / UnRegister Subscription Messages

//        public class SubscriptionDataCallback
//        {
//            public OnGetSubscriptionData callback { get; set; }
//            public OnGetSubscriptionDataTimeOut callbackTimeOut { get; set; }

//            public DateTime lastTimeInvoke { get; set; }
//            public float TimeOut { get; set; }
//            public bool HasTimeOutAlready { get; set; }

//            public bool IsTimeOut
//            {
//                get
//                {
//                    return ((DateTime.Now - lastTimeInvoke).TotalSeconds > TimeOut);
//                }
//            }

//            public SubscriptionDataCallback(OnGetSubscriptionData callback, OnGetSubscriptionDataTimeOut callbackTimeOut = null, float TimeOut = Mathf.Infinity)
//            {
//                this.callback = callback;
//                this.callbackTimeOut = callbackTimeOut;
//                this.lastTimeInvoke = default(DateTime);
//                this.TimeOut = TimeOut;
//                this.HasTimeOutAlready = false;
//            }
//        }


//        private Dictionary<string, List<SubscriptionDataCallback>> m_HealthSubscriptionCallbacks = new Dictionary<string, List<SubscriptionDataCallback>>();

//        private void OnSubscriptionMessage(SelfMessage robotMessage)
//        {
//            if (robotMessage != null)
//            {
//                if (m_OnGetPublishMessage != null)
//                {
//                    m_OnGetPublishMessage.Invoke(robotMessage);
//                }

//                if (robotMessage.HasSubscriptionData && robotMessage.SubscriptionData != null && robotMessage.SubscriptionData.HasData && robotMessage.SubscriptionData.IsAdded && robotMessage.SubscriptionData.thing is SelfThingHealth)
//                {
//                    SelfThingHealth selfThingHealth = robotMessage.SubscriptionData.thing as SelfThingHealth;

//#if ENABLE_DEBUGGING
//                    Log.Debug("SelfWebSocket", "OnSubscriptionMessage Health: {0}", selfThingHealth.ToString());
//#endif

//                    if (m_OnGetHealthData != null)
//                    {
//                        m_OnGetHealthData.Invoke(selfThingHealth);
//                    }

//                    if (m_HealthSubscriptionCallbacks.ContainsKey(selfThingHealth.m_HealthName))
//                    {
//                        foreach (SubscriptionDataCallback subscriptionDataCallback in m_HealthSubscriptionCallbacks[selfThingHealth.m_HealthName])
//                        {
//                            subscriptionDataCallback.HasTimeOutAlready = false;
//                            subscriptionDataCallback.lastTimeInvoke = DateTime.Now;
//                            subscriptionDataCallback.callback.Invoke(robotMessage.SubscriptionData);
//                        }
//                    }
//                    else if (m_HealthSubscriptionCallbacks.ContainsKey(selfThingHealth.SensorName))
//                    {
//                        foreach (SubscriptionDataCallback subscriptionDataCallback in m_HealthSubscriptionCallbacks[selfThingHealth.SensorName])
//                        {
//                            subscriptionDataCallback.HasTimeOutAlready = false;
//                            subscriptionDataCallback.lastTimeInvoke = DateTime.Now;
//                            subscriptionDataCallback.callback.Invoke(robotMessage.SubscriptionData);
//                        }
//                    }
//                    else
//                    {
//                        //do nothing - there is no subscribed listener for this data
//                    }
//                }
//                else
//                {
//                    //do nothing - it doesn't have any subscription data
//                }
//            }
//            else
//            {
//                Log.Warning("SelfWebSocket", "OnSubscriptionMessage - RobotMessage is invalid: {0}", robotMessage);
//            }
//        }

//        //TODO: It is not really subscription data registrationg. In reality; it is only selfThingHealth Data we are getting and subscrbing. 
//        public void RegisterHealthSubscriptionData(SelfSensorType sensorType, OnGetSubscriptionData callback, OnGetSubscriptionDataTimeOut callbackTimeOut, float timeOut)
//        {
//            string sensorName = SelfThing.GetSensorName(sensorType);

//            if (!m_HealthSubscriptionCallbacks.ContainsKey(sensorName))
//            {
//                m_HealthSubscriptionCallbacks[sensorName] = new List<SubscriptionDataCallback>();
//            }

//            SubscriptionDataCallback subscriptionDataCallBack = m_HealthSubscriptionCallbacks[sensorName].Find(e => e.callback == callback);

//            if (subscriptionDataCallBack == null)
//            {
//                m_HealthSubscriptionCallbacks[sensorName].Add(new SubscriptionDataCallback(callback, callbackTimeOut, timeOut));
//            }
//            else
//            {
//                Log.Warning("SelfWebSocket", "RegisterSubscriptionMessage. Can't register callback, already exist. {0} , {1}", sensorName, callback.ToString());
//            }
//        }

//        public void UnregisterHealthSubscriptionData(SelfSensorType sensorType, OnGetSubscriptionData callback, OnGetSubscriptionDataTimeOut callbackTimeOut, float timeOut)
//        {
//            string sensorName = SelfThing.GetSensorName(sensorType);

//            if (m_HealthSubscriptionCallbacks.ContainsKey(sensorName))
//            {
//                SubscriptionDataCallback subscriptionDataCallBack = m_HealthSubscriptionCallbacks[sensorName].Find(e => e.callback == callback);
//                if (subscriptionDataCallBack != null)
//                {
//                    m_HealthSubscriptionCallbacks[sensorName].Remove(subscriptionDataCallBack);
//                }
//            }
//            else
//            {
//                Log.Warning("SelfWebSocket", "UnregisterSubscriptionMessage. There is no callback in the system: {0}", callback.ToString());
//            }
//        }

//        public void RegisterHealthSubscriptionData(string healthName, OnGetSubscriptionData callback, OnGetSubscriptionDataTimeOut callbackTimeOut, float timeOut)
//        {
//            if (!m_HealthSubscriptionCallbacks.ContainsKey(healthName))
//            {
//                m_HealthSubscriptionCallbacks[healthName] = new List<SubscriptionDataCallback>();
//            }

//            SubscriptionDataCallback subscriptionDataCallBack = m_HealthSubscriptionCallbacks[healthName].Find(e => e.callback == callback);

//            if (subscriptionDataCallBack == null)
//            {
//                m_HealthSubscriptionCallbacks[healthName].Add(new SubscriptionDataCallback(callback, callbackTimeOut, timeOut));
//            }
//            else
//            {
//                Log.Warning("SelfWebSocket", "RegisterSubscriptionMessage. Can't register callback, already exist. {0}", callback.ToString());
//            }
//        }

//        public void UnregisterHealthSubscriptionData(string healthName, OnGetSubscriptionData callback, OnGetSubscriptionDataTimeOut callbackTimeOut, float timeOut)
//        {
//            if (m_HealthSubscriptionCallbacks.ContainsKey(healthName))
//            {
//                SubscriptionDataCallback subscriptionDataCallBack = m_HealthSubscriptionCallbacks[healthName].Find(e => e.callback == callback);
//                if (subscriptionDataCallBack != null)
//                {
//                    m_HealthSubscriptionCallbacks[healthName].Remove(subscriptionDataCallBack);
//                }
//            }
//            else
//            {
//                Log.Warning("SelfWebSocket", "UnregisterSubscriptionMessage. There is no callback in the system: {0} , {1}", healthName, callback.ToString());
//            }
//        }

//        #endregion

//        #region Send Message to Connected Self Server

//        public bool isValidJSON(string json)
//        {
//            bool isValidJSONString = false;
//            try
//            {
//                if (!string.IsNullOrEmpty(json))
//                {
//                    System.Object deserializedObject = Json.Deserialize(json);
//                    if (deserializedObject != null)
//                    {
//                        isValidJSONString = true;
//                    }
//                }
//            }
//            catch (Exception)
//            {
//                isValidJSONString = false;
//            }

//            return isValidJSONString;
//        }

//        public void SendQuestionToSelfServer(string message)
//        {
//            SelfMessageToServer messageToSend = new SelfMessageToServer();

//            List<string> targets = new List<string>();

//            targets.Add("conversation");

//            messageToSend.targets = targets.ToArray();

//            SelfSubscriptionData subscriptionData = new SelfSubscriptionData();

//            subscriptionData.@event = "ADDED";

//            messageToSend.data = message;
//            messageToSend.msg = "publish";

//            fsData tempData = null;
//            sm_Serializer.TrySerialize<SelfMessageToServer>(messageToSend, out tempData);
//            string jsonMessage = tempData.ToString();


//            m_WebSocketRobot.Send(jsonMessage);
//        }

//        public bool SendMessageToSelfServer(string message = null, SelfThing selfThingObject = null)
//        {
//            bool success = false;
//            if (m_WebSocketRobot != null)
//            {
//#if USE_BEST_HTTP
//                if (m_WebSocketRobot.IsOpen && m_RobotState == ConnectionState.Connected)
//                {
//#elif USE_WEBSOCKET_SHARP
//                if (m_WebSocketRobot.IsConnected && m_RobotState == ConnectionState.Connected)
//                {
//#endif

//                    if (!string.IsNullOrEmpty(message) || selfThingObject != null)
//                    {
//                        bool isValidJson = isValidJSON(message);

//                        if (!isValidJson)
//                        {
//                            if (m_SubscribedTargets != null && m_SubscribedTargets.Length > 0)
//                            {
//                                string jsonMessage = "";
//                                SelfMessageToServer messageToSend = new SelfMessageToServer();

//                                List<string> targetBlackBoards = new List<string>();

//                                for (int i = 0; i < m_SubscribedTargets.Length; i++)
//                                {
//                                    if (m_SubscribedTargets[i] == ".")
//                                    {
//                                        targetBlackBoards.Add("blackboard");
//                                    }
//                                    else
//                                    {
//                                        targetBlackBoards.Add(m_SubscribedTargets[i].Replace("/.", "") + "/blackboard");
//                                    }

//                                }

//                                messageToSend.targets = targetBlackBoards.ToArray();

//                                SelfSubscriptionData subscriptionData = new SelfSubscriptionData();
//                                subscriptionData.@event = "ADDED";
//                                subscriptionData.thing = selfThingObject;
//                                if (selfThingObject == null)
//                                {
//                                    subscriptionData.thing = new SelfThingText();
//                                    ((SelfThingText)subscriptionData.thing).m_Text = message;
//                                }


//                                fsData tempSubscriptionData = null;
//                                sm_Serializer.TrySerialize<SelfSubscriptionData>(subscriptionData, out tempSubscriptionData);
//                                string jsonMessageSubscription = tempSubscriptionData.ToString().Replace(",\"m_GUID\":\"\"", "").Replace(",\"m_State\":\"\"", "");

//                                messageToSend.data = jsonMessageSubscription;

//                                fsData tempData = null;
//                                sm_Serializer.TrySerialize<SelfMessageToServer>(messageToSend, out tempData);
//                                jsonMessage = tempData.ToString();

//                                if (!string.IsNullOrEmpty(jsonMessage))
//                                {
//                                    Log.Status("SelfWebSocket", "-> To Server. Sending Message as JSON: {0}", jsonMessage);
//                                    m_WebSocketRobot.Send(jsonMessage);
//                                }
//                                else
//                                {
//                                    Log.Warning("SelfWebSocket", "SendMessageToSelfServer: can't send message because Json string is null or empty");
//                                }
//                            }
//                            else
//                            {
//                                Log.Warning("SelfWebSocket", "SendMessageToSelfServer: can't send message because there is target to send");
//                            }
//                        }
//                        else
//                        {
//                            Log.Status("SelfWebSocket", "-> To Server. Sending Message as Plain: {0}", message);
//                            m_WebSocketRobot.Send(message);

//                        }
//                    }
//                    else
//                    {
//                        Log.Warning("SelfWebSocket", "SendMessageToSelfServer: can't send message because there is no message to send");
//                    }
//                }
//                else
//                {
//#if USE_BEST_HTTP
//                    Log.Warning("SelfWebSocket", "SendMessageToSelfServer: can't send message becuase connection state is {0}, is open: {1}", m_RobotState, m_WebSocketRobot.IsOpen);
//#elif USE_WEBSOCKET_SHARP
//                    Log.Warning("SelfWebSocket", "SendMessageToSelfServer: can't send message becuase connection state is {0}, is open: {1}", m_RobotState, m_WebSocketRobot.IsConnected);
//#endif

//                }
//            }
//            else
//            {
//                Log.Warning("SelfWebSocket", "SendMessageToSelfServer: can't send message becuase there is no connection");
//            }

//            return success;
//        }

//        #endregion

//        #region IWatsonService interface
//        /// <exclude />
//        public string GetServiceID()
//        {
//            return SERVICE_ID;
//        }

//        /// <exclude />
//        public void GetServiceStatus(ServiceStatus callback)
//        {
//            if (Config.Instance.FindCredentials(SERVICE_ID) != null)
//                new CheckServiceStatus(this, callback);
//            else
//            {
//                if (callback != null && callback.Target != null)
//                {
//                    callback(SERVICE_ID, false);
//                }
//            }
//        }

//        private class CheckServiceStatus
//        {
//            private ServiceStatus m_Callback = null;

//            public CheckServiceStatus(SelfWebSocket service, ServiceStatus callback)
//            {
//                m_Callback(SERVICE_ID, true);
//            }

//        };
//        #endregion
//    }
//}
