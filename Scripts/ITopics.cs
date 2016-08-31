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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBM.Watson.Self
{
    public abstract class ITopics
    {
	    public struct SubInfo 
	    {
		    public bool			m_Subscribed;       // true if subscribing, false if un-subscribing
		    public string		m_Origin;	        // who is the subscriber
		    public string		m_Topic;	        // topic they are subscribing too
	    };
        public delegate void OnSubscriber( SubInfo a_Info );

	    public struct Payload
	    {
		    string	    		m_Topic;		// the topic of this payload
		    string	    		m_Origin;		// who sent this payload
		    string	    		m_Data;			// the payload data
		    string	    		m_Type;			// the type of data
		    bool	    		m_Persisted;	// true if this was a persisted payload
		    string	    		m_RemoteOrigin;	// this is set to the origin that published this payload 
	    };
        public delegate void OnPayload( Payload a_Payload );

        public struct TopicInfo 
	    {
		    string		    	m_TopicId;		// the ID of this topic
		    string		    	m_Type;			// type of topic
	    };
	    typedef std::vector<TopicInfo>			TopicVector;
	    typedef std::vector<std::string>		ChildVector;

	    struct QueryInfo
	    {
		    QueryInfo() : m_bSuccess(false)
		    {}

		    bool				m_bSuccess;
		    std::string			m_Path;
		    std::string			m_GroupId;
		    std::string			m_SelfId;
		    std::string			m_ParentId;
		    std::string			m_Name;
		    std::string			m_Type;
		    ChildVector			m_Children;
		    TopicVector			m_Topics;
	    };
	    typedef Delegate<const QueryInfo &>		QueryCallback;

	    //! This returns a list of registered topics.
	    virtual void GetTopics( TopicVector & a_Topics ) = 0;
	    //! Local systems should register what topics they will be publishing, this allows other
	    //! clients to enumerate available topics since some topics are not published unless they have
	    //! subscribers.
	    virtual void RegisterTopic(
		    const std::string & a_TopicId,
		    const std::string & a_Type,
		    SubCallback a_SubscriberCallback = SubCallback() ) = 0;
	    //! unregister a topic
	    virtual void UnregisterTopic( const std::string & a_TopicId) = 0;

	    //! This will return true if anyone is subscribed to the given topic.
	    virtual bool IsSubscribed(const std::string & a_TopicId) = 0;
	    //! This returns a list of subscribers for the given topic, the strings returned will be 
	    //! relative paths to the given subscribers. 
	    virtual size_t GetSubscriberCount(const std::string & a_TopicId) = 0;
	    //! Publish data for a given topic to all subscribers.
	    virtual bool Publish( 
		    const std::string & a_TopicId, 
		    const std::string & a_Data, 
		    bool a_bPersisted = false,
		    bool a_bBinary = false ) = 0;
	    //! Send data for a given topic to a specific subscriber
	    virtual bool Send(
		    const std::string & a_Targets,
		    const std::string & a_TopicId,
		    const std::string & a_Data,
		    bool a_bBinary = false ) = 0;
	    //! Publish data for a remote target specified by the provided path.
	    virtual bool PublishAt(
		    const std::string & a_Path,
		    const std::string & a_Data,
		    bool a_bPersisted = false,
		    bool a_bBinary = false ) = 0;

	    //! This queries a node specified by the given path.
	    virtual void Query(
		    const std::string & a_Path,				//! the path to the node, we will invoke the callback with a QueryInfo structure
		    QueryCallback a_Callback ) = 0;
	    //! Subscribe to the given topic specified by the provided path.
	    virtual bool Subscribe(
		    const std::string & a_Path,		//! The topic to subscribe, ".." moves up to a parent self
		    PayloadCallback a_Callback) = 0;
	    //! Unsubscribe from the given topic
	    virtual bool Unsubscribe( 
		    const std::string & a_Path,
		    void * a_pObject = NULL ) = 0;

	    //! Helper function for appending a topic onto a origin
	    static std::string GetPath(const std::string & a_Origin, const std::string & a_Topic);
    }
}
