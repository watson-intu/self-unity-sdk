using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.Self.Topics;
using System.Collections.Generic;

namespace IBM.Watson.Self.Utils
{
    /// <summary>
    /// This object is created to explore the graph of self instances through the TopicClient object. 
    /// </summary>
    public class SelfExplorer
    {
        #region Private Data
        private int m_PendingRequests = 0;
        #endregion

        #region Public Types
        public class Node
        {
            #region Private Data
            private SelfExplorer m_Explorer = null;
            private Node m_Parent = null;
            private List<Node> m_Children = new List<Node>();
            private string m_Path = null;
            private TopicClient.QueryInfo m_Info = null;
            private string m_Source = null;       // selfId of who created this node, so we don't backtrack 
            private bool m_bError = false;
            #endregion

            #region Public Properties
            public bool IsReady { get { return m_Info != null; } }
            public bool IsError { get { return m_bError; } }
            public Node Parent { get { return m_Parent; } set { m_Parent = value; } }
            public List<Node> Children { get { return m_Children; } }
            public string Path { get { return m_Path; } }
            public TopicClient.QueryInfo Info { get { return m_Info; } }
            public string GroupId { get { return m_Info != null ? m_Info.GroupId : ""; } }
            public string SelfId { get { return m_Info != null ? m_Info.SelfId : ""; } }
            public string ParentId { get { return m_Info != null ? m_Info.ParentId : ""; } }
            #endregion

            /// <summary>
            /// Refresh this node and all connected nodes.
            /// </summary>
            public void Refresh( string a_Path, string a_Source )
            {
                m_Path = a_Path;
                m_Source = a_Source;
                m_bError = false;

                m_Explorer.m_PendingRequests += 1;
                Log.Debug( "SelfExplorer", "Query: {0}", m_Path );
                TopicClient.Instance.Query( m_Path, OnQueryResponse );
            }

            public Node( SelfExplorer a_Explorer )
            {
                m_Explorer = a_Explorer;
            }

            private bool IsCircular( string a_Id )
            {
                if ( a_Id == SelfId )
                    return true;
                if ( Parent != null )
                    return Parent.IsCircular( a_Id );
                return false;
            }

            private void OnQueryResponse( TopicClient.QueryInfo a_Info )
            {
                m_Explorer.m_PendingRequests -= 1;
                m_Info = a_Info;

                if ( m_Info != null )
                {
                    if (! string.IsNullOrEmpty( m_Info.ParentId ) )
                    {
                        if ( m_Info.ParentId != m_Source )
                        {
                            if (! IsCircular( m_Info.ParentId ) )
                            {
                                Parent = new Node(m_Explorer);
                                Parent.Children.Add( this );
                        
                                if ( m_Explorer.OnNodeAdded != null )
                                    m_Explorer.OnNodeAdded( Parent );

                                Parent.Refresh( m_Path + "../", SelfId );
                            }
                            else
                                Log.Warning( "SelfExplorer", "Circular parent detected {0}", m_Info.ParentId );
                        }
                    }
                    else if ( Parent != null )
                    {
                        //Log.Debug( "SelfExplorer", "Parent {0} is gone.", Parent.SelfId );
                        if ( m_Explorer.OnNodeRemoved != null )
                            m_Explorer.OnNodeRemoved( Parent );

                        Parent = null;
                    }

                    foreach( string childId in m_Info.Children )
                    {
                        if ( childId == m_Source || childId == SelfId )
                            continue;           // skip our source

                        // look for an existing node first..
                        Node child = null;
                        foreach( Node node in m_Children )
                            if ( node.SelfId == childId )
                            {
                                child = node;
                                break;
                            }

                        if ( child == null )
                        {
                            // no existing node found, create one..
                            child = new Node(m_Explorer);
                            m_Children.Add( child );

                            if ( m_Explorer.OnNodeAdded != null )
                                m_Explorer.OnNodeAdded( child );
                        }

                        // refresh the child node..
                        child.Refresh( m_Path + childId + "/", SelfId );
                    }

                    // remove children..
                    List<Node> remove = new List<Node>();
                    foreach( Node child in m_Children )
                    {
                        bool bFoundChild = false;
                        foreach( string childId in m_Info.Children )
                            if ( childId == child.SelfId )
                            {
                                bFoundChild = true;
                                break;
                            }

                        if (! bFoundChild )
                        {
                            //Log.Debug( "SelfExplorer", "Removing child {0}", child.SelfId );
                            if ( m_Explorer.OnNodeRemoved != null )
                                m_Explorer.OnNodeRemoved( child );
                            remove.Add( child );
                        }
                    }

                    foreach( Node purge in remove )
                        m_Children.Remove( purge );
                }
                else
                {
                    Log.Error( "SelfExplorer", "Failed to query {0}", m_Path );
                    m_bError = true;
                }

                if ( m_Explorer.OnNodeReady != null )
                    m_Explorer.OnNodeReady( this );
                if ( m_Explorer.m_PendingRequests == 0 && m_Explorer.OnExplorerDone != null )
                    m_Explorer.OnExplorerDone( m_Explorer );
            }
        }
        public delegate void OnNode( Node a_Node );
        public delegate void OnDone( SelfExplorer a_Explorer );
        #endregion

        #region Public Properties
        public int PendingRequests { get { return m_PendingRequests; } }
        public Node Root { get; set; }
        public OnNode OnNodeReady { get; set; }
        public OnNode OnNodeAdded { get; set; }
        public OnNode OnNodeRemoved { get; set; }
        public OnDone OnExplorerDone { get; set; }
        #endregion


        #region Public Functions
        public void Explore( string a_StartTarget = "" )
        {
            Root = new Node(this);
            Root.Refresh( a_StartTarget, TopicClient.Instance.SelfId );
        }
        #endregion
    }
}
