using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour {

    public float sensorRadius = 5f;
    public float value;

    public LayerMask layerMask;

    private LineRenderer lr;

    private Car car;
    
	void Start () {
        this.lr = this.GetComponent<LineRenderer> ();
        this.car = this.transform.GetComponentInParent<Car> ();
    }

    void Update ()
    {
        if (!this.car.isAlive)
          {
            lr.positionCount = 0;
          }
        else
          {
            lr.positionCount = 2;
          }
    }
	
	void FixedUpdate () {
        if (!this.car.isAlive) return;

        lr.SetPosition (0, transform.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, sensorRadius, layerMask);

        if (hit.collider != null)
          {
            lr.material.color = Color.red;
            lr.SetPosition (1, hit.point);
            value = hit.distance;
          }
        else
          {
            lr.material.color = Color.green;
            lr.SetPosition(1, transform.position + transform.up * sensorRadius);
            value = sensorRadius;
          }
    }
}
