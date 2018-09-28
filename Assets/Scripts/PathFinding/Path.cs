using System.Collections.Generic;
using UnityEngine;

public class Path
{
    
    public readonly Vector3[] points;
    public readonly int finishLineIndex;
    public readonly int slowDownIndex;

    public int Length          { get { return points.Length; } }
    public int CurrentPoint    { get; set; }
    public bool HasFinished    { get { return CurrentPoint >= Length; }}
    public bool HasWaypoints   { get { return (points != null && points.Length > 0); } }

    public Path(Vector3[] waypoints, Vector3 startPos, float turnDst, float stoppingDst)
    {
        points = waypoints;

        Vector2 previousPoint = V3ToV2(startPos);
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 currentPoint = V3ToV2(points[i]);
            Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
            Vector2 turnBoundaryPoint = (i == finishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDst;
            previousPoint = turnBoundaryPoint;
        }

        float dstFromEndPoint = 0;
        for (int i = points.Length - 1; i > 0; i--)
        {
            dstFromEndPoint += Vector3.Distance(points[i], points[i - 1]);
            if (dstFromEndPoint > stoppingDst)
            {
                slowDownIndex = i;
                break;
            }
        }
    }

    Vector2 V3ToV2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.y);
    }
}