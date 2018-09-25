using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BornFrustrated.Pathfinding
{
    public class PathRequestManager : MonoBehaviour
    {
        Queue<PathResult> results = new Queue<PathResult>();
        private static PathRequestManager instance;
        private PathFinding pathfinding;

        private void Awake()
        {
            instance = this;
            pathfinding = GetComponent<PathFinding>();
        }

        private void Update() 
        {
            if(results.Count > 0)
            {
                int itemsInQueue = results.Count;
                lock(results)
                {
                    for (int i = 0; i < itemsInQueue; i++)
                    {
                        PathResult result = results.Dequeue();
                        result.callback(result.path, result.success);
                    }
                }
            }    
        }
        public static void RequestPath(PathRequest _pathRequest)
        {
           ThreadStart _thStart = delegate
           {
               instance.pathfinding.FindPath(_pathRequest, instance.FinishedProcessingPath);
           };

            _thStart.Invoke();  
        }

        public void FinishedProcessingPath(PathResult _pathResult)
        {
            lock(results)
            {
                results.Enqueue(_pathResult);
            }
        }

    }

}

public struct PathRequest
{
    public Vector3 pathStart;
    public Vector3 pathEnd;
    public Action<Vector3[], bool> callback;

    public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback)
    {
        pathStart = _start;
        pathEnd = _end;
        callback = _callback;
    }

}

public struct PathResult
{
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool> callback;

    public PathResult(Vector3[] _path, bool _success, Action<Vector3[], bool> _callback)
    {
        path = _path;
        success = _success;
        callback = _callback;
    }
}
