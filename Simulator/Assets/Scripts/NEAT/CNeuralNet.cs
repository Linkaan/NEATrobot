using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using GA;

namespace EANN {

    /*
     * It is not legal to directly modify List<T> elements
     * when T is a struct hence these datastructures are
     * classes instead so list stores the references and
     * doesn't create copies.
     */

    /*
     * This datastructure describes the links between neurons. It has two
     * references to the two neurons that is linked together and a connection
     * weight is used to store the "strength" of the link.
     * 
     * TODO: explain difference between SLinkGene and SLink
     */
    [Serializable]
    public class SLink
    {

        public
        SLink (float w, SNeuron from, SNeuron to, bool rec = false)
        {
            weight = w;
            fromNeuron = from.neuronID;
            toNeuron = to.neuronID;
            isRecurrent = rec;
        }
        

        public int fromNeuron;
        public int toNeuron;

        public float weight;

        /*
         * Whether or not this link is recurrent (i.e loops back to
         * itself).
         */
        public bool isRecurrent;
    }

    /*
     * This datastructure is used by the phenotype to hold information about
     * each node in the network. The main difference between SNeuron and
     * SNeuronGene is that the former holds more information about the neuron
     * including:
     * 
     * - the sum of all inputs multiplied by the weights.
     * - the output of the neuron after the activation function
     * - two lists with the links into the neuron and the links originating
     *   from the neuron.
     */
    [Serializable]
    public class SNeuron
    {

        public
        SNeuron (NeuronType neuron_type, int id,
            float y, float x, float response)
        {
            type = neuron_type;
            neuronID = id;
            sumActivation = 0;
            output = 0;
            splitX = x;
            splitY = y;
            activationResponse = response;
            linksFrom = new List<SLink> ();
            linksTo = new List<SLink> ();
        }

        /*
         * The links that refer to this neuron.
         */
        public List<SLink> linksTo;

        /*
         * The links that has this neuron as the origin.
         */
        public List<SLink> linksFrom;

        public float sumActivation;

        public float output;

        /*
         * What function this neuron has in the network.
         */
        public NeuronType type;

        /*
         * Identifaction number for this neuron.
         */
        public int neuronID;

        /*
         * The curvature of the activation function (sigmoid function).
         * A higher value means more prone to changes.TODO: change this sentence
         */
        public float activationResponse;

        /*
         * For an explanation on how this works, see SNeuronGene.
         * 
         * This is used in visualization of the neural network.
         */
        public float splitX, splitY;
    }

    [Serializable]
    public enum UpdateType
    {
       Snapshot,
       Active
    }

    /*
     * This class is the phenotype for a genome and holds all the information
     * about the neurons and the connections between the neurons and the
     * topology of the neural network. It is used by the genetic algorithm.
     */
    [Serializable]
    public class CNeuralNet
    {

        public
        CNeuralNet (List<SNeuron> neurons, int depth)
        {
            this.depth = depth;
            this.neurons = neurons;
        }

        [SerializeField]
        private List<SNeuron> neurons;

        private Dictionary<int, SNeuron> neuronsLookup;

        /*
         * TODO: explain this 
         */
        [SerializeField]
        private int depth;

        /*
         * The activation function used to create a non-linear output from
         * the neurons.
         * 
         * TODO: explain more
         */
        float Sigmoid (float netinput, float response)
        {
            return (1 / (1 + Mathf.Exp(-netinput / response)));
        }

        /*
         * The NEAT update method runs the inputs through the network
         * and returns the outputs. However with NEAT, the network can
         * have any topology with neurons going forward, backward or
         * recurrent. Therefore, the layer-based update method used in
         * traditional feed forward neural networks is not practical.
         * 
         * The network can be updated in two modes:
         * 
         * active: Each neuron adds up all the activations, calculated from
         * all the incoming neurons, during the previous time-step.
         * Essentialy the conceptual difference is that in a layer-based
         * approach the activations are summed per layer and in this mode,
         * the activations travel from one neuron to the next.
         * This mode is useful for unsupervised learning.
         * 
         * snapshot: To completely flush the network and achieve the same
         * result as the layer-based method, this mode of update flushes the
         * activations all the way through from the input neurons to the
         * output neurons. To do this the update needs to iterate through all
         * the neurons "depth" times. This mode is useful for supervised
         * learning.
         */
        public List<float> Update (List<float> inputs, UpdateType type)
        {
            List<float> outputs = new List<float> ();

            int flushCount = 1;

            if (type == UpdateType.Snapshot)
              {
                flushCount = depth;
              }

            for (int i = 0; i < flushCount; i++)
              {
                // clear the output from the last iteration
                outputs.Clear ();

                int neuronIndex = 0;

                // update the input neurons to the 'inputs' parameter
                for (; neurons[neuronIndex].type == NeuronType.Input; neuronIndex++)
                  {
                    neurons[neuronIndex].output = inputs[neuronIndex];
                  }

                Debug.Assert(neurons[neuronIndex].type == NeuronType.Bias);

                // set bias output to 1
                neurons[neuronIndex++].output = 1;

                // iterate through all the neurons through the network
                for (; neuronIndex < neurons.Count; neuronIndex++)
                  {
                    /*
                     * The result from adding up all the activations from the
                     * neurons linking to the current neuron.
                     */
                    float sum = 0;

                    SNeuron neuron = neurons[neuronIndex];

                    for (int j = 0; j < neuron.linksTo.Count; j++)
                      {
                        SLink link = neuron.linksTo[j];
                        sum += link.weight * FindNeuronById (link.fromNeuron).output;
                      }                    

                    // apply the activation function
                    neuron.output = Sigmoid (sum, neuron.activationResponse);

                    if (neuron.type == NeuronType.Output)
                      {
                        outputs.Add (neuron.output);
                      }
                  }                
              }

            /*
             * The implementation difference in the snapshot mode is that
             * instead of dynamically updating the network and storing
             * the results from previous updates, only a "snapshot"
             * is taken from the network and this can be ensured by
             * resetting the network each update.
             */
            if (type == UpdateType.Snapshot)
              {
                for (int i = 0; i < neurons.Count; i++)
                  {
                    SNeuron neuron = neurons[i];
                    neuron.output = 0;
                  }
              }

            return outputs;
        }

        public SNeuron
        FindNeuronById (int neuronID)
        {
            if (neuronsLookup == null)
              {
                this.neuronsLookup = new Dictionary<int, SNeuron>();
                for (int i = 0; i < neurons.Count; i++)
                  {
                    if (this.neuronsLookup.ContainsKey (neurons[i].neuronID))
                      {
                        Debug.LogWarning ("a neuron with id " + neurons[i].neuronID + " already exists in the lookup table!");
                        continue;
                      }
                    this.neuronsLookup.Add(neurons[i].neuronID, neurons[i]);
                  }
              }

            return neuronsLookup[neuronID];
        }

        /* accessor methods */

        public List<SNeuron>
        Neurons ()
        {
            return neurons;
        }
            
	}
}
