using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EANN;
using GA;

[Serializable]
class SerializablePhenotypes
{
    public List<CNeuralNet> phenotypes;
}

/*
 * This script handles the genetic algorithm that trains the brains
 * (i.e neural networks) for all the cars (genomes).
 */
public class CarController : MonoBehaviour {

    public GameObject carPrefab;

    public FollowTarget follower;
    public Slider sliderLeft;
    public Slider sliderRight;
    public TextMeshProUGUI progress;
    public TextMeshProUGUI avgSpeed;
    public TextMeshProUGUI slowDown;
    public Track activeTrack;

    public TextMeshProUGUI bestFitness;
    public TextMeshProUGUI generation;
    public TextMeshProUGUI numSpecies;
    public TextMeshProUGUI bestSpecies;
    public TextMeshProUGUI numMembers;
    public TextMeshProUGUI speciesAge;
    public TextMeshProUGUI gensWithoutImproving;
    public SpeciesDistributionBar bar;
    public NeuralNetRenderer annRenderer;

    /*
     * The population of genomes and storage for all chromosones.
     */
    private Cga population;

    /*
     * This list holds all the brains for the genomes that make up this
     * population.
     */
    private List<CarBrain> brains;

    /*
     * List of all phenotypes for the current generation used for export.
     */
    private List<CNeuralNet> phenotypes;

    /*
     * The best performing car.
     */
    private CarBrain best;

    /*
     * All time best phenotype
     */
    private CNeuralNet bestPhenotype;

    /*
     * Run the simulation as long as there is atleast one genome alive.
     */
    private bool runningSimulation;

    /*
     * When importing phenotypes, we set the mode to runonlymode which means
     * the genetic algorithm will not be run but only the neural networks.
     * No learning will be simulated.
     */
    private bool runOnlyMode;

    /*
     * The number of generations that has been simulated.
     */
    private int generationCount;

    /*
     * Local reference to singleton class holding parameters.
     */
    private Parameters _params;

    private bool initalized;

    void
    Start ()
    {
        Time.timeScale = 7.0f;
        _params = Parameters.instance;

        population = new Cga (_params.NumSensors, 2);

        brains = new List<CarBrain> ();

        phenotypes = population.CreatePhenotypes ();

        // Create the cars (genomes) making up the population.
        InstantiatePopulation ();              

        SetBestCar (brains[0]);

        runningSimulation = true;
        generationCount = 0;
        initalized = true;
    }

    void
    InstantiatePopulation ()
    {
        for (int i = 0; i < _params.NumGenomesToSpawn; i++)
          {
            GameObject car = Instantiate (carPrefab);
            CarBrain brain = car.GetComponent<CarBrain> ();
            brain.genomeID = population.Genomes()[i].ID ();
            brain.AssignPhenotype (population.Genomes ()[i].Phenotype ());
            brains.Add (brain);

            Car carController = car.GetComponent<Car> ();
            carController.sliderLeft = sliderLeft;
            carController.sliderRight = sliderRight;
            carController.slowDown = slowDown;
            carController.progress = progress;
            carController.avgSpeed = avgSpeed;
            carController.id.text = "" + brain.genomeID;

            TrackManager trackMgr = car.GetComponent<TrackManager> ();
            trackMgr.activeTrack = activeTrack;
          }
    }

    void
    SetBestCar (CarBrain newBest)
    {
        if (best != null)
          {
            best.car.doUpdateUI = false;
          }

        annRenderer.UpdateNeural (newBest.Phenotype ());
        best = newBest;
        best.car.doUpdateUI = true;
        follower.target = newBest.transform;
    }

    public void
    ResetRunBest ()
    {
        foreach (CarBrain brain in brains)
          {
            if (brain != best)
              {
                brain.gameObject.SetActive (false);                
              }
          }
        best.Reset ();
    }

    public void
    HideOthers ()
    {
        foreach (CarBrain brain in brains)
          {
            if (brain != best)
              {
                brain.GetComponent<SpriteRenderer>().enabled = false;
                foreach (LineRenderer lr in brain.GetComponentsInChildren<LineRenderer>())
                    lr.enabled = false;
                brain.GetComponent<Car> ().id.gameObject.SetActive(false);
              }
            else
              {
                brain.GetComponent<SpriteRenderer>().enabled = true;
                foreach (LineRenderer lr in brain.GetComponentsInChildren<LineRenderer>())
                    lr.enabled = true;
                brain.GetComponent<Car>().id.gameObject.SetActive(true);
            }
          }
        //best.Reset();
    }

    public void
    Export ()
    {
        if (phenotypes == null) return;

        Debug.Log ("exporting phenotypes...");
        SerializablePhenotypes serializablePhenotypes = new SerializablePhenotypes
        {
            phenotypes = this.phenotypes
        };

        string json = JsonUtility.ToJson (serializablePhenotypes);

        File.WriteAllText ("phenotypes.neat", json);

        json = JsonUtility.ToJson (bestPhenotype);

        File.WriteAllText ("best.neat", json);
    }

    public void
    Import ()
    {
        Debug.Log ("importing phenotypes...");

        if (File.Exists ("phenotypes.neat"))
          {
            string json = File.ReadAllText ("phenotypes.neat");

            SerializablePhenotypes serializablePhenotypes = JsonUtility.FromJson<SerializablePhenotypes> (json);

            if (serializablePhenotypes.phenotypes.Count != brains.Count)
              {
                Debug.LogError("phenotypes and genomes mismatch error. (" + serializablePhenotypes.phenotypes.Count + " / " + brains.Count + ")");
                return;
              }

            this.phenotypes = serializablePhenotypes.phenotypes;

            for (int i = 0; i < brains.Count; i++)
              {
                CarBrain brain = brains[i];
                brain.AssignPhenotype (phenotypes[i]);
                brain.runOnlyMode = true;
                brain.Reset ();
              }

            runOnlyMode = true;
          }
    }

    void
    Update ()
    {
        if (!initalized) return;

        if (runningSimulation || runOnlyMode)
          {
            bool keepRunning = false;
            for (int i = 0; i < brains.Count; i++)
              {
                CarBrain brain = brains[i];
                if (brain.isAlive)
                  {
                    keepRunning = true;
                    if (brain.Fitness() > best.Fitness() || !best.isAlive)
                      {
                        SetBestCar (brain);
                      }
                  }
              }

            if (!keepRunning)
              {
                if (runOnlyMode)
                  {
                    for (int i = 0; i < brains.Count; i++)
                      {
                        CarBrain brain = brains[i];
                        brain.Reset ();
                      }
                  }
                else
                  {
                    // If all genomes are dead we stop the simulation.
                    runningSimulation = false;
                  } 
              }            
          }

        if (!runningSimulation && !runOnlyMode)
          {
            generationCount++;

            // Keep track of the all time best phenotype
            if (best.Fitness() > population.BestFitness ())
              {
                bestPhenotype = best.Phenotype ();
              }

            /*
             * Using the genetic algorithm try to improve the brains for each
             * genome in the hope that it will perform better.
             */
            phenotypes = population.Epoch (GetFitnessScores ());

            // TODO: clean up the mess below
            Dictionary<int, CGenome> genomeIDs = new Dictionary<int, CGenome>();
            foreach (CGenome genome in population.Genomes())
              {
                genomeIDs.Add(genome.ID(), genome);
              }

            List<CarBrain> brainsToBeAssigned = new List<CarBrain> (brains);

            for (int i = brainsToBeAssigned.Count - 1; i >= 0; i--)
              {
                int oldGenomeID = brainsToBeAssigned[i].genomeID;
                if (genomeIDs.ContainsKey (oldGenomeID) && genomeIDs[oldGenomeID].Phenotype () != null)
                  {
                    brainsToBeAssigned[i].AssignPhenotype (genomeIDs[oldGenomeID].Phenotype ());
                    genomeIDs.Remove (oldGenomeID);
                    brainsToBeAssigned[i].GetComponent<Car>().id.text = "" + brains[i].genomeID;
                    brainsToBeAssigned[i].Reset();
                    brainsToBeAssigned.RemoveAt (i);
                  }
              }

            for (int i = 0; i < brainsToBeAssigned.Count; i++)
              {
                brainsToBeAssigned[i].genomeID = genomeIDs.ElementAt(0).Key;
                brainsToBeAssigned[i].AssignPhenotype(genomeIDs.ElementAt(0).Value.Phenotype());
                genomeIDs.Remove(genomeIDs.ElementAt(0).Key);

                brainsToBeAssigned[i].GetComponent<Car>().id.text = "" + brains[i].genomeID;
                brainsToBeAssigned[i].Reset();
              }

            generation.text = "Generation:\t" + generationCount;
            bestFitness.text = string.Format("Best fitness:\t{0:#.0000}", population.BestFitness ());         

            Dictionary<int, int> allSpecies = new Dictionary<int, int> ();
            CSpecies theBestSpecies = null;
            float bestSpeciesFitness = -1;

            for (int i = 0; i < population.Species().Count; i++)
              {
                CSpecies species = population.Species()[i];
                allSpecies.Add (species.ID (), species.NumMembers ());
                if (species.BestFitness () > bestSpeciesFitness)
                  {
                    bestSpeciesFitness = species.BestFitness ();
                    theBestSpecies = species;
                  }

                if (species.Leader () == null) continue;
            }

            numSpecies.text = "Num species:\t" + allSpecies.Count;
            bestSpecies.text = "Best species:\t" + theBestSpecies.ID ();
            numMembers.text = "Num members:\t" + theBestSpecies.NumMembers ();
            speciesAge.text = "Species age:\t" + theBestSpecies.Age ();
            gensWithoutImproving.text = "Gens no impr:\t" + theBestSpecies.GensNoImprovement ();

            bar.UpdateBar (allSpecies);

            runningSimulation = true;
          }
    }

    private Dictionary<int, float>
    GetFitnessScores ()
    {
        Dictionary<int, float> scores = new Dictionary<int, float> ();
        /*
        brains.Sort(delegate (CarBrain a, CarBrain b) {
            return b.Fitness().CompareTo(a.Fitness());
        });*/

        for (int i = 0; i < brains.Count; i++)
          {
            CarBrain brain = brains[i];
            scores.Add (brain.genomeID, brain.Fitness ());
          }

        return scores;
    }
}
