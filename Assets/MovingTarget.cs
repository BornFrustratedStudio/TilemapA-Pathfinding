using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTarget : MonoBehaviour
{
    private Vector2 nextPosition;
    public float lerpTime = 3;
    IEnumerator moving;

    private void Start()
    {
        moving = MovingPoint(FindPoint());
        StartCoroutine(moving);
    }

    IEnumerator MovingPoint(Vector2 position)
    {
        yield return new WaitForSeconds(2f);
        float time = 0;
        while(time <= lerpTime)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x + position.x, transform.position.y + position.y), time);
            time += Time.deltaTime;
            yield return null;
        }
        moving = null;
    }

    private void Update()
    {
        if(moving == null)
        {
            moving = MovingPoint(FindPoint());
            StartCoroutine(moving);
        }
    }

    private Vector2 FindPoint()
    {
        return Random.insideUnitSphere * 0.01f;
    }

}
