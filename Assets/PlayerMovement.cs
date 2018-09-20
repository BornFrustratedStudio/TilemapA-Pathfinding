using UnityEngine;
using System;
using BornFrustrated.Pathfinding;

public class PlayerMovement : MonoBehaviour
{
    Action<Vector3[], bool> point; 
    public Transform target;

    private void Start()
    {
        PathRequestManager.RequestPath(transform.position, target.position, point);
    }

    private void OnEnable()
    {
        point += Move; 
    }
    private void OnDisable()
    {
        point -= Move;
    }

    private void Update()
    {
    }

    void Move(Vector3[]point, bool asd)
    {
        Debug.Log(point.Length);
    }


}
