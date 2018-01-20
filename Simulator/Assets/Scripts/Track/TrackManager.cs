using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour {

    public Track activeTrack;

    public float progressRatio;
    public float avgSpeed;
    
    public float passedDistance;
    public float progressDistance;

    private List<Transform> passedGates;

    private Rigidbody2D rb;

    private Vector3 lastGate;
    private Vector3 nextGate;

    private float totalVel;
    private float ticksAlive;

    private bool newLap;

    void Awake ()
    {
        rb = GetComponent<Rigidbody2D> ();
        passedGates = new List<Transform>();
        passedDistance = 0;
        lastGate = transform.position; 
    }

    public void Reset ()
    {
        passedGates.Clear ();
        passedDistance = 0;
        ticksAlive = 0;
        totalVel = 0;
        lastGate = transform.position;
        nextGate = activeTrack.gates[1].position;
    }

    void Start ()
    {
        nextGate = activeTrack.gates[1].position;
    }

    void Update ()
    {
        totalVel += rb.velocity.magnitude;
        ticksAlive++;

        avgSpeed = totalVel / ticksAlive;

        Vector3 pos = transform.position;
        float distanceToNext = Vector2.Distance (pos, nextGate);
        float distanceFromLast = Vector2.Distance(pos, lastGate);
        if (distanceToNext > Vector2.Distance (lastGate, nextGate))
        {
            distanceFromLast *= -1;
        }        
        progressDistance = passedDistance + distanceFromLast;
        progressRatio = progressDistance / activeTrack.totDistance;
    }
	
	void OnTriggerEnter2D (Collider2D other)
    {
        if (other.CompareTag("gate") &&
            !passedGates.Contains (other.transform) &&
            nextGate == other.transform.position)
          {
            if (passedGates.Count > 0)
              {
                lastGate = passedGates[passedGates.Count - 1].position;
                passedDistance = progressDistance;
              }

            lastGate = transform.position;
            nextGate = activeTrack.NextGate(other.transform).position;

            if (newLap && other.transform == activeTrack.gates[0])
              {
                newLap = false;
                passedGates.Clear();
              }

            // Reset every lap
            if (nextGate == activeTrack.gates[0].position)
              {
                passedGates.Clear();
                newLap = true;
              }            

            passedGates.Add(other.transform);            
          }
    }
}
