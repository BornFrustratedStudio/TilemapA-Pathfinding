using UnityEngine;
using System.Collections;

public class MovingTargetFollowPoint : MonoBehaviour
{
    public float lerpTime = 3;
    IEnumerator moving;

    public int index;

    public Transform[] points;

    private void Start()
    {
        moving = MovingPoint(NextPoint());
        StartCoroutine(moving);
    }

    IEnumerator MovingPoint(Vector2 position)
    {
        yield return new WaitForSeconds(0.5f);
        float time = 0;
        while (time < lerpTime)
        {
            transform.position = Vector3.Lerp(transform.position, position, time);
            time += Time.deltaTime;
            yield return null;
        }
        moving = null;
    }

    private void Update()
    {
        if (moving == null)
        {
            moving = MovingPoint(NextPoint());
            StartCoroutine(moving);
        }
    }

    private Vector2 NextPoint()
    {
        index++;
        if (index > points.Length - 1)
            index = 0;

        return points[index].position;
    }
}
