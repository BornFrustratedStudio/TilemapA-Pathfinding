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

		[SerializeField]
		private float     m_speed;
		[SerializeField]
		private float 	  m_stoppingDistance;

		private Path	  m_path;
		private bool 	  m_isStopped;
		[SerializeField]
		private Transform m_target;
		private Action<Vector3[], bool> point;
		private IEnumerator m_coroutine;
		[SerializeField]
		private PathStatus m_pathStatus;

		public Transform Target { get{ return m_target; } }
		public float     Speed  { get{ return m_speed;  } }
		public float 	 StoppingDistance { get { return m_stoppingDistance; } }
		public bool      IsStopped { get { return m_isStopped; } }
		public PathStatus GetPathStatus { get { return m_pathStatus; } }

		const float PATH_UPDATE_TIME = 0.2f;
		
    	const float PATH_THRESHOLD = .5f;

		#region Public Methods
		public void Stop()
		{
			m_isStopped = true;
			m_target    = null;

			StopPath();

			m_pathStatus = PathStatus.Stopped;
		}

		public void StartPath(Transform _target)
		{
			if(m_isStopped)
				m_isStopped = false;
			
			m_target = _target;

			PlayPath();		

			m_pathStatus = (_target == null) ? PathStatus.Ready : PathStatus.InProgress;	
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
		}

		void OnDisable() 
		{
			StopPath();
		}
		#endregion

		private IEnumerator UpdatePath()
    	{
			if (Time.timeSinceLevelLoad < .3f)
			{
				yield return new WaitForSeconds(.3f);
			}
			
			PathRequestManager.RequestPath(new PathRequest(transform.position, m_target.position, OnPathFound));

			float sqrMoveThreshold = PATH_THRESHOLD;
			Vector3 targetPosOld = m_target.position;

			while (!m_isStopped)
			{
				yield return new WaitForSeconds(PATH_UPDATE_TIME);
				if ((m_target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
				{
					PathRequestManager.RequestPath(new PathRequest(transform.position, m_target.position, OnPathFound));
					targetPosOld = m_target.position;
				}
			}
    	}

		private void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    	{
			if (pathSuccessful)
			{
				Debug.Log("Total Waypoints: " + waypoints.Length);
				m_path = new Path(waypoints, transform.position, 1.0f, m_stoppingDistance);

				StopCoroutine("FollowPath");

				m_pathStatus = PathStatus.InProgress;
				StartCoroutine("FollowPath");
			}
    	}

		private IEnumerator FollowPath()
		{
			if (m_path.Length > 0) 
			{
				int _targetIndex = 0;
				Vector2 currentWaypoint = m_path.points[0];

				while (true) 
				{
					if ((Vector2)transform.position == currentWaypoint) 
					{
						_targetIndex++;

						if (_targetIndex >= m_path.Length) 
						{
							yield break;
						}

						currentWaypoint = m_path.points[_targetIndex];
						currentWaypoint = new Vector3(currentWaypoint.x + .5f, currentWaypoint.y + .5f);
					}	

					transform.position = Vector2.MoveTowards (transform.position, currentWaypoint, m_speed * Time.deltaTime);
				yield return null;

				}
			}
		}

		private void PlayPath()
		{
			if(m_coroutine == null)
			{
				m_coroutine = UpdatePath();
				StartCoroutine(m_coroutine);
			}
		}

		private void StopPath()
		{
			if(m_coroutine != null)
			{
				StopCoroutine(m_coroutine);
			}
		}
	}
}