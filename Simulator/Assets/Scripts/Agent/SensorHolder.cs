using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorHolder : MonoBehaviour {

    public float sensorOffset = 0.5f;

    public Transform sensorOrigin;

    public GameObject sensorPrefab;

    public List<Sensor> sensors;

    /*
     * Local reference to singleton class holding parameters.
     */
    private Parameters _params;

    void Start ()
    {
        _params = Parameters.instance;

        for (float sensorRot = 0; sensorRot <= 180.0f; sensorRot += 180.0f / (_params.NumSensors - 1))
          {
            Quaternion rot = Quaternion.AngleAxis(sensorRot - 90.0f, Vector3.forward);
            Sensor sensor = Instantiate (sensorPrefab, sensorOrigin.position, rot, transform).GetComponent<Sensor> ();
            sensors.Add (sensor);
            sensor.transform.localPosition += sensor.transform.up * sensorOffset;
          }
    }
}
