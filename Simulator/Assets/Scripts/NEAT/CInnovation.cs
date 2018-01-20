using System.Collections.Generic;
using GA;

namespace EANN
{

    public enum InnovationType
    {
        NewNeuron,
        NewLink
    }

    /*
     * This datastructure exists to store changes in the network over time
     * and is used to check for already existing connections in the network.
     * 
     * This is mainly used for the crossover operator implementation in NEAT.
     */
    public struct SInnovation
    {

        /* TODO: add descriptions of each different constructor */
        public
        SInnovation (int inID, int outID, InnovationType type,
            int innovationID)
        {
            NeuronIn = inID;
            NeuronOut = outID;
            Type = type;
            ID = innovationID;
            NeuronID = 0;
            SplitX = 0;
            SplitY = 0;
            neuronType = GA.NeuronType.None;
        }

        public
        SInnovation (SNeuronGene neuron, int innovationID, int neuronID)
        {
            ID = innovationID;
            NeuronID = neuronID;
            Type = InnovationType.NewNeuron;
            SplitX = neuron.SplitX;
            SplitY = neuron.SplitY;
            neuronType = neuron.Type;
            NeuronIn = -1;
            NeuronOut = -1;            
        }

        public
        SInnovation (int inID, int outID, InnovationType type,
            int innovationID, NeuronType neuron_type, float x, float y)
        {
            NeuronIn = inID;
            NeuronOut = outID;
            Type = type;
            ID = innovationID;
            NeuronID = 0;
            SplitX = x;
            SplitY = y;
            neuronType = neuron_type;
        }

        /*
         * There are two fundamental changes that can be made to the network:
         * - a new neuron was added
         * - a new connection/link was established
         */
        public InnovationType Type;

        /* Each innovation has its own unique id in the database. */
        public int ID;

        /* TODO: explain the meaning of these and add constructor */
        public int NeuronIn;
        public int NeuronOut;

        public int NeuronID;

        public NeuronType neuronType;

        public float SplitY;
        public float SplitX;
    }

    /*
     * This class is responsible for maintaining a database of all changes to
     * the network during the evolution of the active population.
     */
    public class CInnovation
    {

        /* The database of all changes in the network. */
        List<SInnovation> m_vecInnovations;

        /*
         * These variables are useful to keep track of the next ID that will
         * be assigned to the next innovation.
         */
        int m_NextNeuronID;
        int m_NextInnovationID;

        /*
         * NEAT grows the network structure from a population with a
         * basic and minimalistic topology and random connection weights.
         * 
         * The initial population of genomes will generate a set of initial
         * innovations that is stored in the database.
         */
        public
        CInnovation (List<SLinkGene> startGenes,
            List<SNeuronGene> startNeurons)
        {
            m_vecInnovations = new List<SInnovation> ();

            for (int i = 0; i < startNeurons.Count; i++)
              {
                SNeuronGene neuron = startNeurons[i];
                m_vecInnovations.Add (new SInnovation (neuron,
                                                       m_NextInnovationID++,
                                                       m_NextNeuronID++));
              }

            for (int i = 0; i < startGenes.Count; i++)
              {
                SLinkGene gene = startGenes[i];
                m_vecInnovations.Add(new SInnovation(gene.FromNeuron,
                                                     gene.ToNeuron,
                                                     InnovationType.NewLink,
                                                     m_NextInnovationID++));
              }
        }

        /*
         * This method is used to he assign newly created genes an innovation
         * number depending on if the innovation created for that gene has
         * already occured. This is used in the crossover function of the
         * genetic algorithm.
         * 
         * Returns a negative value if the innovation is not in the database
         * or the innovation ID if it was found.
         */
        public int
        CheckInnovation (int inID, int outID, InnovationType type)
        {
            for (int i = 0; i < m_vecInnovations.Count; i++)
              {
                SInnovation innovation = m_vecInnovations[i];
                if (innovation.NeuronIn == inID &&
                    innovation.NeuronOut == outID &&
                    innovation.Type == type)
                  {
                    return innovation.ID;
                  }
              }

            return -1;
        }

        /*
         * These methods creates a new innovation and puts it in the database.
         */
        public int
        CreateNewInnovation (int inID, int outID, InnovationType type)
        {
            SInnovation innovation = new SInnovation (inID, outID, type,
                                                      m_NextInnovationID++);

            if (type == InnovationType.NewNeuron)
              {
                innovation.NeuronID = m_NextNeuronID++;
              }

            m_vecInnovations.Add (innovation);

            return m_NextNeuronID - 1;
        }

        /*
         * Same construction of innovation as the method above but with 
         * the positon of the neuron included.
         */
        public int
        CreateNewInnovation(int from, int to, InnovationType type,
            NeuronType neuron_type, float x, float y)
        {
            SInnovation innovation = new SInnovation (from, to, type,
                                                      m_NextInnovationID++,
                                                      neuron_type, x, y);

            if (type == InnovationType.NewNeuron)
            {
                innovation.NeuronID = m_NextNeuronID++;
            }

            m_vecInnovations.Add(innovation);

            return m_NextNeuronID - 1;
        }

        /*
         * This method is used in the genetic algorithm to initialize the
         * neurons.
         * 
         * Clones an already existing neuron with the given id.
         */
        public SNeuronGene
        CreateNeuronByID (int id)
        {
            SNeuronGene clone = new SNeuronGene (NeuronType.Hidden, 0, 0, 0);

            for (int i = 0; i < m_vecInnovations.Count; i++)
              {
                SInnovation innovation = m_vecInnovations[i];
                if (innovation.NeuronID == id)
                  {
                    clone.Type = innovation.neuronType;
                    clone.ID = innovation.NeuronID;
                    clone.SplitY = innovation.SplitY;
                    clone.SplitX = innovation.SplitX;

                    return clone;
                  }
              }

            return clone;
        }

        /* accessor methods */

        public int
        GetNeuronID (int innovationID)
        {
            return m_vecInnovations[innovationID].NeuronID;
        }

        public int
        NextInnovationID()
        {
            return m_NextInnovationID;
        }
    }
}
