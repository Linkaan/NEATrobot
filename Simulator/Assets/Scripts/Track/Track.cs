using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour {

    // the finish line should be element 0
    public Transform[] gates;

    public float totDistance;

    public bool doReverse;

    void Awake ()
    {
        if (doReverse)
          {
            System.Array.Reverse (gates);
          }

        totDistance = 0;
        for (int i = 1; i < gates.Length; i++)
          {
            Transform gate = gates[i];
            Transform lastGate = gates[i - 1];
            totDistance += Vector2.Distance(gate.position, lastGate.position);
          }

        totDistance += Vector2.Distance(gates[gates.Length - 1].position, gates[0].position);
    }

    public Transform NextGate (Transform gate)
    {
        int index = System.Array.IndexOf (gates, gate);
        if (index < 0)
          {
            Debug.LogError ("gate is not in list of gates");
            return null;
          }

        if (index + 1 >= gates.Length)
          {
            return gates[0];
          }

        return gates[index + 1];
    }

}
