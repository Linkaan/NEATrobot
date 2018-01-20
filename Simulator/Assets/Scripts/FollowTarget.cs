using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour {

    public Transform target;
    public float smoothTime = 0.3f;
    public float maxSpeed = 10f;

    private Vector2 velocity = Vector2.zero;

    void Update()
    {
        if (target != null)
          {
            Vector2 goalPos = target.position;
            //goalPos.y = transform.position.y;
            Vector3 newPos = Vector2.SmoothDamp (transform.position, goalPos, ref velocity, smoothTime, maxSpeed, Time.deltaTime);
            newPos.z = transform.position.z;
            transform.position = newPos;
          }        
    }

}
