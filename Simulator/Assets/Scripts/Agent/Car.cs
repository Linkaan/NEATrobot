using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Car : MonoBehaviour {

    public Slider sliderLeft;
    public Slider sliderRight;

    public TextMeshProUGUI progress;
    public TextMeshProUGUI avgSpeed;
    public TextMeshProUGUI slowDown;

    public TextMeshPro id;

    public TrackManager trackMgr;

    public bool doUpdateUI;
    public bool isAlive;

    public Transform BLwheel;
    public Transform FLwheel;

    public Transform BRwheel;
    public Transform FRwheel;

    public float powerBoost = 5f;
    public float maxAngularVelocity = 5f;

    public int changeTolerance = 3;
    public float slowDownFactor = 0.5f;

    public float leftThrust;
    public float rightThrust;

    public float progressValue;

    private Rigidbody2D rb;    
    private SensorHolder sensorHolder;
    private SpriteRenderer renderer;

    private List<float> sensorReadings;

    private float[] lastSign;
    private int sameSign;

    private bool slowingDown;
    private float slowingDownStart;

    /*
     * Local reference to singleton class holding parameters.
     */
    private Parameters _params;

    void Start ()
    {
        _params = Parameters.instance;

        this.rb = this.GetComponent<Rigidbody2D> ();
        this.trackMgr = this.GetComponent<TrackManager> ();
        this.sensorHolder = this.GetComponent<SensorHolder> ();
        this.renderer = this.GetComponent<SpriteRenderer> ();

        sensorReadings = new List<float> (new float[_params.NumSensors]);

        leftThrust = 0.5f;
        rightThrust = 0.5f;

        isAlive = true;

        lastSign = new float[] { 1.0f, 1.0f };
    }

    public List<float>
    GetSensorReadings ()
    {
        for (int i = 0; i < _params.NumSensors; i++)
          {
            Sensor sensor = sensorHolder.sensors[i];
            sensorReadings[i] = sensor.value;
          }

        return sensorReadings;
    }

    public void Update ()
    {
        if (!isAlive)
        {
            Color colour = Color.red;
            colour.a = 0.5f;
            renderer.color = colour;
            return;
        }

        progressValue = this.trackMgr.progressRatio;
        if (doUpdateUI)
          {
            Color colour = Color.white;
            colour.a = 1;
            renderer.color = colour;
            progress.text = string.Format("Progress:\t{0:#.0000}", progressValue);
            sliderLeft.value = leftThrust;
            sliderRight.value = rightThrust;
            avgSpeed.text = string.Format("Avg speed:\t{0:#.00}", trackMgr.avgSpeed);
            slowDown.gameObject.SetActive (slowingDown);
          }
        else
          {
            Color colour = Color.white;
            colour.a = 0.5f;
            renderer.color = colour;
          }
    }

    public void FixedUpdate ()
    {
        if (!isAlive)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        float adjustedPowerBoost = powerBoost;

        float adjustedLeftThrust = leftThrust - 0.5f;
        float adjustedRightThrust = rightThrust - 0.5f;

        if (Time.time - slowingDownStart > 1.0f)
          {
            slowingDown = false;
          }            

        // Discourage behaviour that causes rapid change of direction.
        float[] signNow = { Mathf.Sign(adjustedLeftThrust), Mathf.Sign(adjustedRightThrust)};
        if (lastSign.SequenceEqual (signNow))
          {
            sameSign += 1;
          }
        else
          {
            if (sameSign < changeTolerance || signNow.Reverse().SequenceEqual (lastSign))
              {
                adjustedPowerBoost *= slowDownFactor;
                slowingDown = true;
                slowingDownStart = Time.time;
              }
            sameSign = 0;
          }
        lastSign = signNow;

        float leftPower = powerBoost * adjustedLeftThrust;
        float rightPower = powerBoost * adjustedRightThrust;
        Vector2 leftForce = transform.up * leftPower;
        Vector2 rightForce = transform.up * rightPower;

        rb.AddForceAtPosition (leftForce, BLwheel.position, ForceMode2D.Force);
        rb.AddForceAtPosition (leftForce, FLwheel.position, ForceMode2D.Force);

        rb.AddForceAtPosition (rightForce, BRwheel.position, ForceMode2D.Force);
        rb.AddForceAtPosition (rightForce, FRwheel.position, ForceMode2D.Force);

        if (rb.angularVelocity > maxAngularVelocity)
          {
            rb.angularVelocity = maxAngularVelocity;
          }
        else if (rb.angularVelocity < -maxAngularVelocity)
          {
            rb.angularVelocity = -maxAngularVelocity;
          }
    }
}
