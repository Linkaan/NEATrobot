using System.Collections.Generic;
using GA;

namespace GA
{
    /*
     * This datatype contains information about the two neurons it is
     * connected to. It holds the weight as a number to describe the
     * "importance" of the connection that links together the two
     * neurons.
     * 
     * Some connections between neurons will be evolved to be disabled
     * so a flag is set to indicate whether this link is active.
     * 
     * Finally a flag is used to indicate if this link is recurrent and
     * this datatype also stores an innovation number. See explanations
     * below for more in-depth information.
     */
    public class SLinkGene
    {

  		public
  		SLinkGene (int inID, int outID,
  				  bool enable, int tag, float w,
  				  bool rec = false)
  		{
  			Enabled = enable;
  			InnovationID = tag;
  			FromNeuron = inID;
  			ToNeuron = outID;
  			Weight = w;
  			IsRecurrent = rec;
  		}

        public
        SLinkGene (SLinkGene original)
        {
            Enabled = original.Enabled;
            InnovationID = original.InnovationID;
            FromNeuron = original.FromNeuron;
            ToNeuron = original.ToNeuron;
            Weight = original.Weight;
            IsRecurrent = original.IsRecurrent;
        }

        /*
         * This is a link gene so we store the IDs of the two neurons
         * that make up the connection.
         */
        public int FromNeuron,
                   ToNeuron;

        /*
  		 * This is the number that represents the strength of this
  		 * connection. This is used in the calculation of activations
  		 * of neurons.
  		 */
        public float Weight;

        /* Whether or not this link is currently active */
  		public bool Enabled;

  		/*
  		 * Whether or not this link is recurrent (i.e loops back to
  		 * itself).
  		 */
  		public bool IsRecurrent;

        /*
         * The id of the innovation for this connection.
         * An innovation occurs each time the network structure is changed
         * by adding a neuron or connection. To keep track of previous
         * changes a database of all innovations is maintained.
         */
  		public int InnovationID;

        /* accessor methods */

        public void
        Disable()
        {
            this.Enabled = false;
        }
    }
}