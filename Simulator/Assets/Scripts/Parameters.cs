using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Singleton class holding all parameters for NEAT and controllers.
 */
public class Parameters : MonoBehaviour {

    /* ------ CONTROLLER ------ */

    /*
     * The minimum amount of improvement a genome must improve with to not be
     * killed after MaxTimeWithoutImprovement.
     */
    public float ImprovementEpsilon = 0.01f;

    /*
     * The maximum amount of time in seconds that can pass without improving
     * before the genome is killed.
     */
    public float MaxTimeWithoutImprovement = 4f;

    /*
     * The scale that the max time without improvement will be multiplied with
     * if the genome is improving and hasn't stagnated.
     */
    public float ImprovementTimeScale = 3f;

    /* ------ NEURAL NETWORK ------ */

    /*
     * The number of sensors (i.e inputs) for the neural networks.
     */
    public int NumSensors = 5;

    /* ------ GA ------ */

    /*
     * The total number of genomes to add to each population.
     */
    public int NumGenomesToSpawn = 50;

    /*
     * The rate at which crossovers will happen.
     */
    public float CrossoverRate = 0.7f;

    /*
     * The upper limit for the amount of neurons one genome can have.
     * This is used to prevent a genome to take over the world!
     * 
     * TODO: nope
     */
    public int MaxPermittedNeurons = 100;

    /*
     * TODO: explain these
     */
    public int NumAddLinkAttempts = 5;
    public int NumTriesToFindLoopedLink = 5;
    public int NumTriesToFindOldLink = 5;
    public int NumGensAllowedNoImprovement = 15;  

    public float ChanceAddLink = 0.07f;
    public float ChanceAddNode = 0.03f;
    public float ChanceAddRecurrentLink = 0.05f;

    public float MutationRate = 0.8f;
    public float ChanceWeightReplaced = 0.1f;
    public float MaxWeightPerturbation = 0.5f;

    public float ActivationMutationRate = 0.1f;
    public float MaxActivationPerturbation = 0.1f;    

    /* ------ GENOME ------ */

    /*
     * If the genome contains less than MinHiddenNeurons hidden neurons
     * it is too risky to select a link at random. This is used to avoid
     * the chaining effect. See the implementation of AddNeuron.
     */
    public int MinHiddenNeurons = 5;

    /* ------ SPECIES ------ */

    /*
     * If the species is younger than this a fitness boost will be added.
     */
    public int YoungAgeThreshold = 10;

    /*
     * The fitness bonus factor for young species.
     */
    public float YoungFitnessBonus = 1.3f;

    /*
     * If the species is older than this a fitness penalty will be applied.
     */
    public int OldAgeThreshold = 50;

    /*
     * If the species is old, this will be its penalty factor when adjusting fitness.
     */
    public float OldAgePenalty = 0.7f;

    /*
     * This value is used to determine whether a new species shall be created
     * or if a genome can be added to an already existing one.
     */
    public float CompatibilityThreshold = 0.26f;

    /*
     * The percentage to consider when selecting the best members of
     * a species.
     */
    public float SurvivalRate = 0.2f;

    /*
     * s_Instance is used to cache the instance found in the scene so we
     * don't have to look it up every time.
     */
    public static Parameters s_Instance = null;

    /*
     * This defines a static instance property that attempts to find the
     * manager object in the scene and returns it to the caller.
     */
    public static Parameters instance
    {
        get
          {
            if (s_Instance == null)
              {
                Debug.LogError ("Parameters object doesn't exist.");
              }

            return s_Instance;
          }
    }

    void
    Awake ()
    {
        s_Instance = FindObjectOfType(typeof(Parameters)) as Parameters;

        if (s_Instance == null)
        {
            GameObject obj = new GameObject("_PARAMETERS");
            s_Instance = obj.AddComponent(typeof(Parameters)) as Parameters;
            Debug.Log("Could not locate a Parameters object. \n" +
                      "Parameters was generated automatically.");
        }
    }
}
