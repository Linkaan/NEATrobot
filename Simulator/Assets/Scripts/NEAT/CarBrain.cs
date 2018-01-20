using System.Collections;
using System.Collections.Generic;
using EANN;
using UnityEngine;

public class CarBrain : MonoBehaviour {

    public bool isAlive;

    /*
     * When run only mode is enabled, the car is run until it crashes.
     */
    public bool runOnlyMode;

    /*
     * The genome ID that this car represents. This is used to match the
     * phenotypes to the genotypes in the genetic algorithm.
     */
    public int genomeID;

    /*
     * Reference to the car object that the neural network controls.
     */
    public Car car;

    /*
     * The phenotype is the brain that has been assigned to this particular
     * car.
     */
    private CNeuralNet phenotype;    

    /*
     * The fitness for this genome.
     */
    private float fitness;

    /*
     * The best ever fitness for this genome this generation.
     */
    private float bestFitness;

    /*
     * The time that the best fitness was achieved.
     */
    private float lastImprovementTime;

    /*
     * Initial state of the gameobject.
     */
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    /*
     * Local reference to singleton class holding parameters.
     */
    private Parameters _params;

    void
    Awake ()
    {
        _params = Parameters.instance;
        car = this.GetComponent<Car> ();
    }

    void
    Start ()
    {
        isAlive = true;
        fitness = 0;        

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        lastImprovementTime = Time.time;
    }

    public void
    AssignPhenotype (CNeuralNet phenotype)
    {
        this.phenotype = phenotype;
    }

    public void
    Reset ()
    {
        transform.SetPositionAndRotation (initialPosition, initialRotation);
        fitness = 0;
        isAlive = true;
        bestFitness = 0;
        lastImprovementTime = Time.time;

        car.trackMgr.Reset ();
    }

    // Update the neural network assigned to this genome with sensor input.
    void Update ()
    {
        car.isAlive = isAlive;
        if (!isAlive) return;

        List<float> inputs = car.GetSensorReadings ();

        /*
         * Two outputs for left and right thrust.
         */
        List<float> outputs = phenotype.Update (inputs, UpdateType.Active);

        if (outputs.Count != 2)
          {
            Debug.LogError ("Error in neural network, output count should be two.");
            return;
          }

        car.leftThrust = Mathf.Clamp01 (outputs[0]);
        car.rightThrust = Mathf.Clamp01 (outputs[1]);

        if (runOnlyMode && Time.time - lastImprovementTime > 15.0f && car.trackMgr.avgSpeed <= 0.33f)
          {
            isAlive = false;
          }

        fitness = car.trackMgr.progressDistance * car.trackMgr.avgSpeed;
        if (fitness < 0) fitness = 0;

        // Only kill car if it crashes when doing runOnlyMode
        if (runOnlyMode) return;      

        // NOTICE: kill of the car after one lap so speed is evolved.
        if (car.progressValue >= 1.0f)
          {
            isAlive = false;
            return;
          }

        float compensation = 1.0f;

        if (car.trackMgr.progressDistance >= 10)
          {
            compensation = 2.0f;
          }

        // TODO: clean up this mess
        if (fitness > bestFitness + _params.ImprovementEpsilon / _params.ImprovementTimeScale)
          {
            if (fitness > bestFitness + _params.ImprovementEpsilon)
              {
                bestFitness = fitness;
                lastImprovementTime = Time.time;
              }
            else if (Time.time - lastImprovementTime >
                    (compensation * _params.MaxTimeWithoutImprovement *
                    _params.ImprovementTimeScale))
              {
                isAlive = false;
              }
          }
        else if (Time.time - lastImprovementTime > _params.MaxTimeWithoutImprovement)
          {
            isAlive = false;
          }
    }

    void OnCollisionEnter2D (Collision2D coll)
    {
        isAlive = false;
    }

    /* accessor methods */
    public float
    Fitness ()
    {
        return fitness;
    }

    public CNeuralNet
    Phenotype ()
    {
        return phenotype;
    }
}
