using System;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;

namespace BornFrustrated.Pathfinding
{
    public class PathFinding : MonoBehaviour
    {
        public NodeGrid grid;

        private void Awake()
        {
            grid = FindObjectOfType<NodeGrid>();
        }

        public void FindPath(PathRequest _request, Action<PathResult> _callback)
        {
            /// For Performance Only
            Stopwatch sw = new Stopwatch();
            sw.Start();

            /// Result Array of Waypoints
            Vector3[] waypoints = new Vector3[0];
            /// Control Variable.
            bool pathSuccess = false;

            /// Get Start Node From Position and set root as
            /// himself.
            Node startNode = grid.NodeFromWorldPoint(_request.pathStart);
            if (startNode == null)
                startNode = grid.FindTheMostNearNode(_request.pathStart);

            startNode.parent = startNode;

            /// Get End Node From Position.
            Node targetNode = grid.NodeFromWorldPoint(_request.pathEnd);

            if (targetNode == null)
            {
                targetNode = grid.FindTheMostNearNode(_request.pathEnd);
            }


            /// Analyze Nodes with Heap Data Structure and store 
            /// the ones not analyzed yet.
            Heap<Node> openSet = new Heap<Node>(grid.tiles.Count);
            openSet.Add(startNode);

            /// Store Already Analyzed Nodes.
            HashSet<Node> closedSet = new HashSet<Node>();

            /// While there are nodes to analyze. . .
            while (openSet.Count > 0)
            {
                /// Get The first node and analyze.
                Node currentNode = openSet.RemoveFirst();
                /// Add current node into already analyzed stack.
                closedSet.Add(currentNode);

                /// If Current node is our target, close path and 
                /// break the loop.
                if (currentNode == targetNode)
                {
                    sw.Stop();
                    pathSuccess = true;

                    UnityEngine.Debug.Log("Path found: " + sw.ElapsedMilliseconds + " ms");
                    break;
                }

                /// Otherwhise, analyze every neighbour of the selected node
                /// and check here its values.
                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    /// Only if neighbour is not walkable or if it was already 
                    /// analyzed, then continue and check another one.    
                    if (!neighbour.Walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }


                    int newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour) + neighbour.MovePenalty;
                    if (newMovementCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour))
                    {
                        neighbour.GCost = newMovementCostToNeighbour;
                        neighbour.HCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }

            }
            if (!grid.Reachable(targetNode))
                pathSuccess = false;


            /// If a path was found, then stop searching 
            /// and try to trace a path in order to get waypoints 
            /// correctly.
            if (pathSuccess)
            {
                waypoints = RetracePath(startNode, targetNode);
                pathSuccess = waypoints.Length > 0;
            }

            _callback(new PathResult(waypoints, pathSuccess, _request.callback));

        }

        /// <summary>
        /// Return the distance between two node
        /// </summary>
        /// <param name="nodeA"></param>
        /// <param name="nodeB"></param>
        /// <returns></returns>
        int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.LocalPlace.x - nodeB.LocalPlace.x);
            int dstY = Mathf.Abs(nodeA.LocalPlace.y - nodeB.LocalPlace.y);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }

        /// <summary>
        /// Retrace parent starting from EndNode and use the parent
        /// property to find every relationship between nodes to 
        /// StartNode
        /// </summary>
        /// <param name="startNode">Node To Start</param>
        /// <param name="endNode">Node To End</param>
        /// <returns>Return Path</returns>
        Vector3[] RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            Vector3[] waypoints = SimplifyPath(path);
            Array.Reverse(waypoints);
            return waypoints;

        }

        /// <summary>
        /// Get a list of position from a path of Nodes.
        /// </summary>
        /// <param name="path">Path to follow</param>
        /// <returns>Get a positions List.</returns>
        Vector3[] SimplifyPath(List<Node> path)
        {
            List<Vector3> waypoints = new List<Vector3>();

            Vector2 oldDirection = new Vector2(0, 0);

            for (int i = 0; i < path.Count; i++)
            {
                waypoints.Add(new Vector3(path[i].WorldLocation.x, path[i].WorldLocation.y, 0));
            }

            return waypoints.ToArray();
        }

    }
}