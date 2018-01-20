using System.Collections.Generic;
using GA;

namespace GA
{
	public enum NeuronType {
		Input,
		Hidden,
		Bias,
		Output,
		None
	}

    /*
     * This datatype is used in the genetic algorithm to store this
     * neuron's function within the network.
     * 
     * There are four different types of neurons in this implementation
     * of NEAT:
     * - input neuron
     * - output neuron
     * - hidden neuron
     * - bias neuron
     * 
     * Refer to the implementation of the EANN for more details about
     * what the different types of neurons do.
     * 
     * To differantiate between neurons and make connections between
     * neurons, each neuron has its own unique identifier. 
     */
	public class SNeuronGene
	{

		public
		SNeuronGene (NeuronType neuron_type, int id,
					float y, float x, bool rec = false)
		{
			ID = id;
			Type = neuron_type;
			IsRecurrent = rec;
			SplitX = x;
			SplitY = y;
			ActivationResponse = 1;
		}

        public
        SNeuronGene (SNeuronGene original)
        {
            ID = original.ID;
            Type = original.Type;
            IsRecurrent = original.IsRecurrent;
            SplitX = original.SplitX;
            SplitY = original.SplitY;
            ActivationResponse = original.ActivationResponse;
        }

        /*
         * This is the ID that represents this gene
         */
        public int ID;

		/*
         * What function this neuron has in the network
         */
		public NeuronType Type;

		/*
         * Whether or not this neuron is recurrent. A recurrent neuron
		 * is defined as a neuron with a connection that loops back
		 * on itself.
		 */
		public bool IsRecurrent;

		/* 
		 * This describes how steep the activation of the sigmoid
		 * function is. For a description of the sigmoid function
		 * see the implementation of the EANN.
		 */
        public float ActivationResponse { get; set; }

        /*
		 * In this implementation of NEAT each neuron has a position
		 * in the network "grid". This will be useful for rendering
		 * the topology of the network for the user. Also useful for
		 * knowing if a link is recurrent.
		 * 
		 * For all input neurons 0 will be assigned to SplitY and for
		 * all output nerons a 1 will be assigned to SplitY and all
		 * neurons that are added will be assigned a value inbetween
		 * the two neurons that made up that connection.
		 */
        public float SplitX;
        public float SplitY;
    }
}