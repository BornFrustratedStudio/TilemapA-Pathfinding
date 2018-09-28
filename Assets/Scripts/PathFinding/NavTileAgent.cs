using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BornFrustrated.Pathfinding
{
    public class NavTileAgent : MonoBehaviour
    {
        public enum PathStatus
        {
            Completed,
            Stopped,
            InProgress,
            Ready,
            Endless
        }

        #region Customize Stats Agent
        /// How much agent should stop before reach the target.
        [SerializeField]
        private float m_stoppingDistance;

        /// Max Speed reachable by the agent.
        [SerializeField]
        private float m_maxSpeed = 3.5f;

        /// Max Force
        [SerializeField]
        private float m_maxForce = 100;

        [SerializeField]
        private Vector2 m_offset;

        /// Distance start slowing before reaching target.
        [SerializeField]
        private float m_slowingDistance = 1;

        /// Acceleration Rate
        [SerializeField]
        private float m_accelerationRate = 2;

        /// Deceleration Rate
        [SerializeField]
        private float m_decelerationRate = 2;

        /// Max radius in which agent avoid other agents.
        [SerializeField]
        private float m_avoidRadius = 0.5f;        

        /// Max distance to search other agents to avoid.
        [SerializeField]
        private float m_lookAheadDistance = 1;
        #endregion

        #region Debug Only
        [SerializeField]
        private Vector2     m_velocity;
        
        [SerializeField]
        private Transform   m_target;

        [SerializeField]
        private PathStatus  m_pathStatus;

        [SerializeField]
        private NodeGrid map;
        #endregion

        private Vector2   m_currentWaypoint;
        private Path      m_path;
        private float     m_accelerationValue = 0;
        private IEnumerator m_coroutine;

        const float PATH_UPDATE_TIME = 0.2f;
        const float PATH_THRESHOLD   = 0.5f;
        
        private static List<NavTileAgent> m_listOfAgents = new List<NavTileAgent>();

        #region Public Properties
        public Transform  Target            { get { return m_target; } }
        public float      Speed             { get { return m_velocity.magnitude; } }
        public float      StoppingDistance  { get { return m_stoppingDistance; } }
        public bool       IsStopped         { get { return m_pathStatus == PathStatus.Stopped && !m_path.HasFinished; } }
        public PathStatus GetPathStatus     { get { return m_pathStatus; } }
        public bool       HasPath           { get { return (m_pathStatus == PathStatus.InProgress || m_pathStatus == PathStatus.Endless) && m_path != null; } }
        public Vector2    Position          
        { 
            get 
            { 
                return new Vector2(transform.position.x + m_offset.x, transform.position.y + m_offset.y); 
            }

            set
            {
                transform.position = new Vector3(value.x, value.y, transform.position.z) - new Vector3(m_offset.x, m_offset.y, 0);
            }
        }
        public float      RemainingDistance 
        {
            get 
            {
                if (!HasPath || !m_path.HasWaypoints)
                    return 0;

                float dist = Vector2.Distance(Position, m_currentWaypoint);

                for (int i = m_path.CurrentPoint; i < m_path.points.Length; i++)
                {
                    dist += Vector2.Distance(m_path.points[i], m_path.points[i == m_path.points.Length - 1 ? i : i + 1]);
                }

                return dist;
            }
        }        
        #endregion

        #region Private Method
        /// <summary>
        /// Update Path every time target is Moving.
        /// </summary>
        private IEnumerator UpdatePath()
        {
            /// Hard fix for unity editor.
            if (Time.timeSinceLevelLoad < .3f)
                yield return new WaitForSeconds(.3f);

            /// Request a path from player position and target position.
            PathRequestManager.RequestPath(new PathRequest(Position, m_target.position, OnPathFound));

            /// offset used to retrace the path if distance between target 
            /// and its previous position recorded is grater than this value.
            float   _thresholdToRetrace  = PATH_THRESHOLD;
            Vector3 _oldPositionRecorded = m_target.position;

            while (true)
            {
                yield return new WaitForSeconds(PATH_UPDATE_TIME);

                /// if target position change from the previously recorded
                /// over an offset, then retrace the path.
                if ((m_target.position - _oldPositionRecorded).sqrMagnitude > _thresholdToRetrace)
                {
                    PathRequestManager.RequestPath(new PathRequest(Position, m_target.position, OnPathFound));
                    _oldPositionRecorded = m_target.position;
                }
            }
        }

        /// <summary>
        /// Function used to find after it beign requested.
        /// </summary>
        /// <param name="waypoints">Waypoints founded</param>
        /// <param name="pathSuccessful">True if path is founded or false</param>
        private void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
        {
            StopCoroutine(FollowPath());

            if(waypoints.Length == 0)
            {
                m_pathStatus = PathStatus.Ready;
                m_path       = null;
                return;
            }

            /// Create new Path.
            m_path = new Path(waypoints, Position, 1.0f, m_stoppingDistance);

            /// Set the status of the path.           
            m_pathStatus = (pathSuccessful) ? PathStatus.InProgress : PathStatus.Endless;

            /// Restart Coroutine.
            StartCoroutine(FollowPath());
        }

        /// <summary>
        /// Start Following Path.
        /// </summary>
        /// <returns></returns>
        private IEnumerator FollowPath()
        {
            if (m_path.Length >  0)
            {
                    
                m_path.CurrentPoint = 0;
                m_currentWaypoint   = m_path.points[0];
                
                float proximity = 0.05f;

                while (true)
                {
                    if(m_path == null)
                        break;
                    /// if the point of the path is the last one
                    /// then change the stopping distance.
                    if (m_path.CurrentPoint == m_path.Length - 1)
                        proximity = m_stoppingDistance;
                    else
                        proximity = 0.05f;

                    /// If the distance between player and the 
                    /// waypoint is less than the stopping distance, 
                    /// then change waypoint.    
                    if ((Position - m_currentWaypoint).sqrMagnitude <= proximity)
                    {
                        m_path.CurrentPoint ++;

                        /// If the next node is over the path length, then 
                        /// stop follow because path is ended.
                        if (m_path.CurrentPoint >= m_path.Length)
                        {
                            yield break;
                        }

                        m_currentWaypoint = m_path.points[m_path.CurrentPoint];
                        m_currentWaypoint = new Vector3(m_currentWaypoint.x + .5f, m_currentWaypoint.y + .5f);
                    }
                    yield return null;
                }
            }

            yield return null;
        }
        #endregion

        #region Helper Function
        private Vector2 Truncate(Vector2 vec, float max)
        {
            if (vec.magnitude > max)
            {
                vec.Normalize();
                vec *= max;
            }
            return vec;
        }

        private Vector2 Seek(Vector2 pos)
        {
            Vector2 desiredVelocity = (pos - Position).normalized * m_maxSpeed;
            Vector2 steer = desiredVelocity - m_velocity;
            steer = Truncate(steer, m_maxForce);
            return steer;
        }

        /// <summary>
        /// Slow down target once arrive at position 
        /// </summary>
        /// <param name="pos">Position to reach</param>
        /// <returns>Resultant vector</returns>
        private Vector2 Arrive(Vector2 pos)
        {

            var desiredVelocity = (pos - Position);
            float dist = desiredVelocity.magnitude;

            if (dist > 0)
            {
                var reqSpeed = dist / (m_decelerationRate * 0.3f);
                reqSpeed = Mathf.Min(reqSpeed, m_maxSpeed);
                desiredVelocity *= reqSpeed / dist;
            }

            Vector2 steer = desiredVelocity - m_velocity;
            steer = Truncate(steer, m_maxForce);
            return steer;
        }

        private void LookAhead()
        {
            //if agent is outside dont LookAhead since that causes agent to constantely be slow.
            if (m_lookAheadDistance <= 0 || !map.NodeIsWalkable(Vector3Int.FloorToInt(Position)))
            {
                return;
            }

            var currentLookAheadDistance = Mathf.Lerp(0, m_lookAheadDistance, m_velocity.magnitude / Speed);
            var lookAheadPos = Position + m_velocity.normalized * currentLookAheadDistance;

            Debug.DrawLine(Position, lookAheadPos, Color.blue);
            Debug.Log(lookAheadPos);

          /*  if (!map.NodeIsWalkable(Vector3Int.FloorToInt(Position)))
            {
                m_velocity -= (lookAheadPos - Position);
            }*/

        /*    if (avoidRadius > 0)
            {
                for (int i = 0; i < m_listOfAgents.Count; i++)
                {
                    NavTileAgent otherAgent = m_listOfAgents[i];
                    if (otherAgent == this || otherAgent.avoidRadius <= 0)
                    {
                        continue;
                    }
                    
                    float mlt = otherAgent.avoidRadius + this.avoidRadius;
                    float dist = (lookAheadPos - otherAgent.Position).magnitude;
                    var str = (lookAheadPos - otherAgent.Position).normalized * mlt;
                    var steer = Vector3.Lerp((Vector3)str, Vector3.zero, dist / mlt);
                    m_velocity += (Vector2)steer;

                    Debug.DrawLine(otherAgent.Position, otherAgent.Position + str, new Color(1, 0, 0, 0.1f));
                }
            }*/
        }
        #endregion

        #region Unity Methods
        void OnEnable()
        {
            SetDestination(m_target);
            m_listOfAgents.Add(this);
        }

        void OnDisable()
        {
            Stop();
            m_listOfAgents.Remove(this);
        }

        private void LateUpdate()
        {
            if (m_maxSpeed <= 0 || !HasPath || IsStopped)
            {
                m_velocity = Vector2.zero;
                Debug.Log(this.gameObject.name + " - Restart");
                return;
            }

            if (RemainingDistance < m_slowingDistance)
            {
                m_accelerationValue = 0;
                m_velocity += Arrive(m_currentWaypoint);
            }
            else
            {
                m_velocity += Seek(m_currentWaypoint);
                m_accelerationValue += m_accelerationRate * Time.deltaTime;
                m_accelerationValue = Mathf.Clamp01(m_accelerationValue);
                m_velocity *= m_accelerationValue;
            }

            LookAhead();
            
            m_velocity = Truncate(m_velocity, m_maxSpeed);            
            
            Position +=  m_velocity * Time.deltaTime;

            if (RemainingDistance <= StoppingDistance)
            {
                if (m_path.CurrentPoint == m_path.points.Length && m_pathStatus == PathStatus.InProgress)
                {
                    m_pathStatus = PathStatus.Completed;
                }
            }

        }
        #endregion

        public void Stop()
        {
            if(HasPath && !m_path.HasFinished)
                m_pathStatus = PathStatus.Stopped;
            else
                m_pathStatus = PathStatus.Ready;

            m_path     = null;
            m_target   = null;
            m_velocity = new Vector2(0, 0);

            StopAllCoroutines();
            m_coroutine = null;
        }
        
        public void SetDestination(Transform _target)
        {
            if(!IsStopped || !HasPath)
                m_pathStatus = PathStatus.Ready;

            if(m_target != _target)
            {
                m_target   = _target;
                m_velocity = new Vector2(0, 0);
            }

            if(m_coroutine == null)
            {
                m_coroutine = UpdatePath();
                StartCoroutine(m_coroutine);
            }
        }

        /*
        public enum PathStatus
        {
            Completed,
            Stopped,
            InProgress,
            Ready,
            Endless
        }

        [SerializeField]
        private float m_speed;
        [SerializeField]
        private float m_stoppingDistance;

        private Path m_path;
        private bool m_isStopped;
        [SerializeField]
        private Vector2 m_velocity;
        private float m_maxSpeed = 3.5f;
        private float m_maxForce = 100;
        private Vector2 m_currentWaypoint;

        private float m_accelerationValue = 0;
        ///The distance to start slowing down
        public float slowingDistance = 1;
        ///The rate at which it will accelerate
        public float accelerationRate = 2;
        ///The rate at which it will slow down
        public float decelerationRate = 2;

        [SerializeField]
        private Transform m_target;
        private Action<Vector3[], bool> point;
        private IEnumerator m_coroutine;
        [SerializeField]
        private PathStatus m_pathStatus;

        public Transform Target { get { return m_target; } }
        public float Speed { get { return m_speed; } }
        public float StoppingDistance { get { return m_stoppingDistance; } }
        public bool IsStopped { get { return m_isStopped; } }
        public PathStatus GetPathStatus { get { return m_pathStatus; } }

        [SerializeField]
        private float avoidRadius = 0.5f;

        private static List<NavTileAgent> allAgents = new List<NavTileAgent>();
        public NodeGrid map;

        [SerializeField]
        private float lookAheadDistance = 1;
   
        ///The remaining distance of the active path. 0 if none
        public float remainingDistance {
            get {
                if (m_pathStatus != PathStatus.InProgress || m_path == null || m_path.points == null || m_path.points.Length == 0)
                    return 0;

                float dist = Vector2.Distance(Position,m_currentWaypoint);

                for (int i = m_path.CurrentPoint; i < m_path.points.Length; i++)
                {
                    dist += Vector2.Distance(m_path.points[i], m_path.points[i == m_path.points.Length - 1 ? i : i + 1]);
                }
                return dist;
            }
        }


        public Vector2 Position 
            {
            get { return new Vector2(transform.position.x, transform.position.y); }
        }

        const float PATH_UPDATE_TIME = 0.2f;

        const float PATH_THRESHOLD = .5f;

        #region Public Methods
        public void Stop()
        {
            m_isStopped = true;
            m_target = null;

            StopPath();

            m_pathStatus = PathStatus.Stopped;
        }

        public void StartPath(Transform _target)
        {
            if (m_isStopped)
                m_isStopped = false;

            m_target = _target;

            PlayPath();

            //m_pathStatus = (_target == null) ? PathStatus.Ready : PathStatus.InProgress;

        }
        #endregion

        #region UnityMethods
        void Start()
        {
            StartPath(m_target);
        }

        void OnEnable()
        {
            StartPath(m_target);
            allAgents.Add(this);
        }

        private void LateUpdate()
        {
            if (m_maxSpeed <= 0 || m_pathStatus != PathStatus.InProgress)
            {
                m_velocity = Vector2.zero;
                return;
            }

            if (remainingDistance < slowingDistance)
            {
                m_accelerationValue = 0;
                m_velocity += Arrive(m_currentWaypoint);
            }
            else
            {
                m_velocity += Seek(m_currentWaypoint);
                m_accelerationValue += accelerationRate * Time.deltaTime;
                m_accelerationValue = Mathf.Clamp01(m_accelerationValue);
                m_velocity *= m_accelerationValue;
            }

            m_velocity = Truncate(m_velocity, m_maxSpeed);

            LookAhead();
            transform.position += new Vector3(m_velocity.x * Time.deltaTime, m_velocity.y * Time.deltaTime, 0);

            if ((remainingDistance <= StoppingDistance))
            {
                if (m_path.CurrentPoint == m_path.points.Length && m_pathStatus == PathStatus.InProgress)
                {
                    m_pathStatus = PathStatus.Completed;
                }
            }

        }

        void OnDisable()
        {
            StopPath();
            allAgents.Remove(this);
        }
        #endregion

        #region Private Method
        private IEnumerator UpdatePath()
        {
            if (Time.timeSinceLevelLoad < .3f)
            {
                yield return new WaitForSeconds(.3f);
            }

            PathRequestManager.RequestPath(new PathRequest(transform.position + new Vector3(-0.5f, -0.5f), m_target.position, OnPathFound));

            float sqrMoveThreshold = PATH_THRESHOLD;
            Vector3 targetPosOld = m_target.position;

            while (!m_isStopped)
            {
                Debug.Log("asdadwa");
                yield return new WaitForSeconds(PATH_UPDATE_TIME);
                if ((m_target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
                {
                    Debug.Log("Change Path");
                    PathRequestManager.RequestPath(new PathRequest(transform.position + new Vector3(-0.5f, -0.5f), m_target.position, OnPathFound));
                    targetPosOld = m_target.position;
                }
            }
        }

        private void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
        {
            if (pathSuccessful)
            {
                //Debug.Log("Total Waypoints: " + waypoints.Length);
                m_path = new Path(waypoints, transform.position, 1.0f, m_stoppingDistance);

                StopCoroutine(FollowPath());

                m_pathStatus = PathStatus.InProgress;
                StartCoroutine(FollowPath());
            }
            else
            {
                m_pathStatus = PathStatus.Endless;
            }
        }

        private IEnumerator FollowPath()
        {
            if (m_path.Length > 0)
            {
                m_path.CurrentPoint = 0;
                m_currentWaypoint = m_path.points[0];

                float proximity = 0.05f;

                while (true)
                {
                    if ((m_path.points.Length - 1 == m_path.CurrentPoint))
                    {
                        //Debug.Log("Found last point");
                        proximity = m_stoppingDistance;
                    }
                    //Debug.Log("Proximity " + proximity);
                    if (((Vector2)transform.position - m_currentWaypoint).sqrMagnitude <= proximity)
                    {
                        //Debug.Log("Curret Waypoint" + m_path.CurrentPoint + " im arrivet to the current waypoint");
                        m_path.CurrentPoint ++;

                        //Debug.Log(" next one is = " + m_path.CurrentPoint + "/" + (m_path.points.Length-1));
                        if (m_path.CurrentPoint >= m_path.points.Length)
                        {
                            //Debug.Log("i finish the path");
                            yield break;
                        }

                        m_currentWaypoint = m_path.points[m_path.CurrentPoint];

                        m_currentWaypoint = new Vector3(m_currentWaypoint.x + .5f, m_currentWaypoint.y + .5f);
                    }

                    yield return null;
                }
            }
            yield return null;
        }


        private void PlayPath()
        {
            if (m_coroutine == null)
            {
                m_coroutine = UpdatePath();
                StartCoroutine(m_coroutine);
            }
        }

        private void StopPath()
        {
            if (m_coroutine != null)
            {
                StopCoroutine(m_coroutine);
            }
        }

        //limit the magnitude of a vector
        Vector2 Truncate(Vector2 vec, float max)
        {
            if (vec.magnitude > max)
            {
                vec.Normalize();
                vec *= max;
            }
            return vec;
        }

        Vector2 Seek(Vector2 pos)
        {
            Vector2 desiredVelocity = (pos - new Vector2(transform.position.x, transform.position.y)).normalized * m_maxSpeed;
            Vector2 steer = desiredVelocity - m_velocity;
            steer = Truncate(steer, m_maxForce);
            return steer;
        }

        Vector2 Arrive(Vector2 pos)
        {

            var desiredVelocity = (pos - (Vector2)transform.position);
            float dist = desiredVelocity.magnitude;

            if (dist > 0)
            {
                var reqSpeed = dist / (decelerationRate * 0.3f);
                reqSpeed = Mathf.Min(reqSpeed, m_maxSpeed);
                desiredVelocity *= reqSpeed / dist;
            }

            Vector2 steer = desiredVelocity - m_velocity;
            steer = Truncate(steer, m_maxForce);
            return steer;
        }
        #endregion

        void LookAhead()
        {
            //if agent is outside dont LookAhead since that causes agent to constantely be slow.
            if (lookAheadDistance <= 0 || !map.NodeIsWalkable(Vector3Int.FloorToInt(Position)))
            {
                return;
            }

            var currentLookAheadDistance = Mathf.Lerp(0, lookAheadDistance, m_velocity.magnitude / Speed);
            var lookAheadPos = Position + m_velocity.normalized * currentLookAheadDistance;

            Debug.DrawLine(Position, lookAheadPos, Color.blue);

            if (!map.NodeIsWalkable(Vector3Int.FloorToInt(Position)))
            {
                m_velocity -= (lookAheadPos - Position);
            }

            if (avoidRadius > 0)
            {
                for (int i = 0; i < allAgents.Count; i++)
                {
                    NavTileAgent otherAgent = allAgents[i];
                    if (otherAgent == this || otherAgent.avoidRadius <= 0)
                    {
                        continue;
                    }
                    Debug.Log("Slowing ai");
                    float mlt = otherAgent.avoidRadius + this.avoidRadius;
                    float dist = (lookAheadPos - otherAgent.Position).magnitude;
                    var str = (lookAheadPos - otherAgent.Position).normalized * mlt;
                    var steer = Vector3.Lerp((Vector3)str, Vector3.zero, dist / mlt);
                    m_velocity += (Vector2)steer;

                    Debug.DrawLine(otherAgent.Position, otherAgent.Position + str, new Color(1, 0, 0, 0.1f));
                }
            }
        }

*/
    }
}