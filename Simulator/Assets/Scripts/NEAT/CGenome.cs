using System.Collections.Generic;
using UnityEngine;
using EANN;
using GA;

namespace GA
{
	/*
	 * This is a datatype that holds all information about a specific
	 * configuration for this network. This is how a network structure
	 * is encoded.
	 * 
	 * Its function is to store a certain phenotype and set of neurons
	 * that are linked in different ways evolved by the genetic algorithm.
	 */
	public class CGenome
    {

		/*
		 * Minimal configuration of a genome with all input neurons
		 * connected linearly to output layers.
		 */
		public
		CGenome (int id, int inputs, int outputs)
		{
            m_GenomeID = id;
            m_iNumInputs = inputs;
            m_iNumOutputs = outputs;

            m_vecNeurons = new List<SNeuronGene> ();
            m_vecLinks = new List<SLinkGene> ();

            //create the input neurons
            float InputRowSlice = 1 / (float)(inputs + 2);

            for (int i = 0; i < inputs; i++)
              {
                m_vecNeurons.Add (new SNeuronGene (NeuronType.Input, i, 0, (i + 2) * InputRowSlice));
              }

            //create the bias
            m_vecNeurons.Add (new SNeuronGene (NeuronType.Bias, inputs, 0, InputRowSlice));

            //create the output neurons
            float OutputRowSlice = 1 / (float)(outputs + 1);

            for (int i = 0; i < outputs; i++)
              {
                m_vecNeurons.Add (new SNeuronGene (NeuronType.Output, i + inputs + 1, 1, (i + 1) * OutputRowSlice));
              }

            /*
             * Create the link genes and connect the inputs and outputs linearily and 
             * assign a random weight between -1 and 1.
             */
            for (int i = 0; i < inputs + 1; i++)
              {
                for (int j = 0; j < outputs; j++)
                  {
                    m_vecLinks.Add (new SLinkGene (m_vecNeurons[i].ID,
                                                   m_vecNeurons[inputs + j + 1].ID,
                                                   true,
                                                   inputs + outputs + 1 + NumGenes(),
                                                   Random.Range (-1.0f, 1.0f)));
                  }
              }
        }

		/*
		 * Takes a list of neurons and list of connections and a unique
		 * id that describes this genome
		 */
		public
		CGenome (int id, List<SNeuronGene> neurons,
		        List<SLinkGene> genes, int inputs, int outputs)
		{
            m_vecLinks = genes;
            m_vecNeurons = neurons;
            m_iNumInputs = inputs;
            m_iNumOutputs = outputs;
            m_GenomeID = id;
        }

        public
        CGenome (CGenome original)
        {
            m_vecLinks = new List<SLinkGene> ();
            m_vecNeurons = new List<SNeuronGene>();
            for (int i = 0; i < original.m_vecLinks.Count; i++)
              {
                m_vecLinks.Add (new SLinkGene (original.m_vecLinks[i]));
              }
            for (int i = 0; i < original.m_vecNeurons.Count; i++)
              {
                m_vecNeurons.Add (new SNeuronGene (original.m_vecNeurons[i]));
              }            
            m_iNumInputs = original.m_iNumInputs;
            m_iNumOutputs = original.m_iNumOutputs;
            m_GenomeID = original.m_GenomeID;
        }

        /* The identification number of this genome */
        private int m_GenomeID;

        /* 
		 * This list contains all the neurons that this configuration
		 * of the network consist of.
		 */
        private List<SNeuronGene> m_vecNeurons;

        /* All the connections defined between the neurons */
        private List<SLinkGene> m_vecLinks;

        /* 
		 * This is a reference to the phenotype (that describes the
		 * specific behaviour for this genome).
		 */
        private CNeuralNet m_Phenotype;

        /* The actual fitness score computed by the GA */
        private float m_dFitness;

        /* 
		 * This is the fitness score that is adjusted according to
		 * the species that it is part of.
		 */
        private float m_dAdjustedFitness;

        /* 
		 * The number of offspring this genome is set to spawn for the
		 * next generation/epoch.
		 */
        private float m_dAmountToSpawn;

        /* Store the number of inputs and outputs */
        private int m_iNumInputs,
			        m_iNumOutputs;

        /*
		 * Each genome is assigned what species it is part of. This is
		 * essentially used to group together similar genomes.
		 */
        private int m_iSpecies;

        /*
         * Local reference to singleton class holding parameters.
         */
        private Parameters _params = Parameters.instance;

        /*
		 * Retrieves the position of the neuron with the specified id from
		 * m_vecNeurons.
		 */
        private int
		GetNeuronIndexById (int id)
		{
            for (int i = 0; i < m_vecNeurons.Count; i++)
			  {
                SNeuronGene neuron = m_vecNeurons[i];

				if (neuron.ID == id)
					  return i;
			  }
            return -1;
		}

        /*
         * This method checks if the two neurons specified by id already
         * are linked in the genome.
         */
        private bool
        IsDuplicateLink(int neuronIn, int neuronOut)
        {
            for (int i = 0; i < m_vecLinks.Count; i++)
              {
                if ((m_vecLinks[i].FromNeuron == neuronIn) &&
                    (m_vecLinks[i].ToNeuron == neuronOut))
                  {
                    //we already have this link
                    return true;
                  }
              }

            return false;
        }

		/*
		 * This method creates a neural network that is assigned to
		 * this genome. The depth describes TODO: what does this describe?
		 */
		public CNeuralNet
		CreatePhenotype (int depth)
		{
            // Delete previous phenotype assigned to this genome.
            DeletePhenotype ();

            List<SNeuron> neurons = new List<SNeuron> ();

            // Add the neuron genes to the phenotype neurons.
            for (int i = 0; i < m_vecNeurons.Count; i++)
              {
                SNeuronGene neuron = m_vecNeurons[i];
                neurons.Add (new SNeuron (neuron.Type,
                                          neuron.ID,
                                          neuron.SplitY,
                                          neuron.SplitX,
                                          neuron.ActivationResponse));
              }

            // Create the links for the phenotype.
            for (int i = 0; i < m_vecLinks.Count; i++)
              {
                SLinkGene link = m_vecLinks[i];
                // Ignore the disabled links
                if (link.Enabled)
                  {
                    SNeuron fromNeuron = neurons[GetNeuronIndexById (link.FromNeuron)];
                    SNeuron toNeuron = neurons[GetNeuronIndexById (link.ToNeuron)];

                    SLink newLink = new SLink (link.Weight, fromNeuron, toNeuron, link.IsRecurrent);

                    fromNeuron.linksFrom.Add (newLink);
                    toNeuron.linksTo.Add (newLink);
                  }
              }

            m_Phenotype = new CNeuralNet (neurons, depth);

            return m_Phenotype;
		}

        public void
        DeletePhenotype ()
        {
            m_Phenotype = null;
        }

        /*
         * Tries to select a random neuron that is either a hidden or output
         * neuron and that is not recurrent.
         * 
         * Return true if successful, false otherwise.
         */
        private bool
        AddLoopedRecurrentLink (out int neuron1_ID, out int neuron2_ID,
            int numTriesToFindLoop)
        {
            // Attempt numTriesToFindLoop times to find a neuron that has no
            // loopback connection and is either a hidden or output neuron.
            while (numTriesToFindLoop-- > 0)
              {
                int index = Random.Range(0, m_vecNeurons.Count);
                SNeuronGene randNeuron = m_vecNeurons[index];

                if (!randNeuron.IsRecurrent &&
                    (randNeuron.Type == NeuronType.Hidden ||
                    randNeuron.Type == NeuronType.Output))
                  {
                    neuron1_ID = neuron2_ID = randNeuron.ID;

                    randNeuron.IsRecurrent = true;

                    return true;
                  }
              }

            neuron1_ID = neuron2_ID = -1;
            return false;
        }

        /*
         * Tries to add a non-recurrent link between two random neurons.
         */
        private void
        AddNonReccurentLink(out int neuron1_ID, out int neuron2_ID,
            int numTriesToAddLink)
        {
            // We only attempt numTriesToAddLink times to prevent
            // entering an infinite loop if all available neurons has a
            // connection.
            while (numTriesToAddLink-- > 0)
              {
                // The first neuron can be any type of neuron.
                int index = Random.Range(0, m_vecNeurons.Count);
                neuron1_ID = m_vecNeurons[index].ID;

                // The second neuron has to be a output or hidden neuron so
                // we select a random index from index of bias neuron + 1 to
                // index of last neuron in the list.
                index = Random.Range(m_iNumInputs + 1, m_vecNeurons.Count);
                neuron2_ID = m_vecNeurons[index].ID;

                if (!IsDuplicateLink(neuron1_ID, neuron2_ID) &&
                    neuron1_ID != neuron2_ID)
                  {
                    // If we got here, we found a valid pair of neurons.
                    return;  
                  }
              }
            neuron1_ID = neuron2_ID = -1;
        }

        /*
		 * This method tries to add a link between two random neurons, if
         * a mutation occurs dependent on the mutation rate, with one of
         * the following types of links:
         * - forward link
         * - recurrent link
         * - looped recurrent link
		 */
        public void
        AddLink (float mutationRate, float chanceOfLooped,
				CInnovation innovations, int numTriesToFindLoop,
				int numTriesToAddLink)
		{
            if (Random.value > mutationRate) return;

            int neuron1_ID = -1;
            int neuron2_ID = -1;

            // If this flag is true a recurrent link is added.
            bool setRecurrent = false;

            // There is a chance that this will be a looped recurrent link
            // (i.e a neuron that has a link directly to itself).
            if (Random.value < chanceOfLooped)
              {
                setRecurrent = AddLoopedRecurrentLink (out neuron1_ID,
                    out neuron2_ID, numTriesToFindLoop);
              }
            else
              {
                AddNonReccurentLink (out neuron1_ID, out neuron2_ID,
                    numTriesToAddLink);
              }

            if (neuron1_ID < 0 || neuron2_ID < 0) return;

            // Does the database already hold a similar innovation?
            int innovationID = innovations.CheckInnovation (neuron1_ID,
                neuron2_ID, InnovationType.NewLink);

            // Check if this link is a recurrent link (non-looped).
            // If the link feeds backward it is a recurrent link.
            //
            // A backward link is characterized with the fact that the from
            // neuron has a greater SplitY value than the to neuron.
            if (m_vecNeurons[GetNeuronIndexById(neuron1_ID)].SplitY >
                m_vecNeurons[GetNeuronIndexById(neuron2_ID)].SplitY)
              {
                setRecurrent = true;
              }

            if (innovationID < 0)
              {
                // This innovation does not exist so we add a new innovation
                // to the database with a id so we can later refer to it by
                // its new id. The new link gene will be tagged with this id.
                innovations.CreateNewInnovation (neuron1_ID, neuron2_ID,
                    InnovationType.NewLink);
                innovationID = innovations.NextInnovationID() - 1;
              }

            // Assign the innovation id to a new link gene.
            SLinkGene newGene = new SLinkGene(neuron1_ID, neuron2_ID,
                true /* enable */,
                innovationID,
                Random.Range(-1.0f, 1.0f) /* initial weight */,
                setRecurrent);

            m_vecLinks.Add(newGene);
        }

        /*
         * This method determines if a link is acceptable to be split. It
         * satisfies the following conditions:
         * - the link is enabled
         * - the link is not recurrent
         * - the From neuron of the link is not a bias input
         */
        private bool
        CanLinkBeSplit(int linkIndex)
        {
            int fromNeuron = m_vecLinks[linkIndex].FromNeuron;

            return m_vecLinks[linkIndex].Enabled &&
                  !m_vecLinks[linkIndex].IsRecurrent &&
                  m_vecNeurons[GetNeuronIndexById(fromNeuron)].Type !=
                  NeuronType.Bias;
        }

        /*
         * Used in AddNeuron to select a random link biased towards older ones
         * in order to prevent a chaining effect. This happens when the same
         * link is selected and split repeatedly. This mainly happens in the
         * early stage of a network.
         * 
         * The link has to be enabled and feed forward (i.e non-recurrent) and
         * the From neuron shall not be connected to a bias neuron
         * TODO: explain why
         */
        private bool
        FindNeuronLinkBiased (out int linkIndex, int numTriesToFindOldLink)
        {
            while(numTriesToFindOldLink-- > 0)
              {
                // Randomly select a link where older links are more likely to
                // be chosen.
                linkIndex = Random.Range(0, NumGenes() - (int)Mathf.Sqrt(NumGenes()));

                if (CanLinkBeSplit (linkIndex))
                  {
                    return true;
                  }
              }          
            
            // Unable to find a valid link.
            linkIndex = -1;
            return false;
        }

        /*
         * This method checks if the genome is already using the specified
         * neuron id and is used in the AddNewNeuronLinks method.
         */
        private bool
        UsingNeuronID (int neuronID)
        {
            return false;
        }

        // TODO: rename the helper methods to more accurately describe
        // what they do and maybe consider splitting up this logic
        // to a helper class.
        // http://wiki.c2.com/?MethodObject
        // the following is also interesting:
        // https://www.refactoring.com/catalog/introduceParameterObject.html

        /*
         * Creates and adds new links and neurons to this genome.
         * This method is used by AddNeuronExistingInnovation and
         * AddNeuronNewInnovation.
         */
        private void
        CreateNewNeuronLinks (int from, int to, int newNeuronID, int idLink1,
            int idLink2, float newWidth, float newDepth,
            float initialWeight)
        {
            // The first link will have a initial weight of 1 and
            // the other neuron will have the original weight.
            m_vecLinks.Add (new SLinkGene (from, newNeuronID,
                true /* enable */,
                idLink1,
                1.0f));

            m_vecLinks.Add (new SLinkGene (newNeuronID, to,
                true,
                idLink2,
                initialWeight));

            m_vecNeurons.Add (new SNeuronGene (NeuronType.Hidden,
                newNeuronID, newWidth, newDepth));
        }

        /*
         * Creates two new link genes using the already existing neuron. This
         * is a helper method used in AddNewNeuronLinks.
         */
        private void
        AddNeuronExistingInnovation (CInnovation innovations, int newNeuronID,
            int from, int to, float newWidth, float newDepth,
            float initialWeight)
        {
            int idLink1 = innovations.CheckInnovation (from, newNeuronID,
                InnovationType.NewLink);
            int idLink2 = innovations.CheckInnovation (newNeuronID, to,
                InnovationType.NewLink);

            /*
             * This should not happen because if we got here the
             * links should logically already have a innovation
             */
            if (idLink1 < 0 || idLink2 < 0)
              {
                Debug.LogError ("Error: no link genes found for neuron");
                return;
              }

            CreateNewNeuronLinks (from, to, newNeuronID, idLink1, idLink2,
                newWidth, newDepth, initialWeight);            
        }

        /*
         * Creates new neuron and two new links with new innovations. This is
         * a helper method used in AddNewNeuronLinks.
         */
        private void
        AddNeuronNewInnovation (CInnovation innovations, int from, int to,
            float newWidth, float newDepth, float initialWeight)
        {
            int newNeuronID = innovations.CreateNewInnovation(from, to,
                    InnovationType.NewNeuron, NeuronType.Hidden,
                    newWidth, newDepth);

            // In addition to adding the new neuron two new links
            // are created and stored in the innovations database
            int idLink1 = innovations.NextInnovationID();

            innovations.CreateNewInnovation(from, newNeuronID,
                InnovationType.NewLink);            

            int idLink2 = innovations.NextInnovationID();

            innovations.CreateNewInnovation(newNeuronID, to,
                InnovationType.NewLink);

            CreateNewNeuronLinks (from, to, newNeuronID, idLink1, idLink2,
                newWidth, newDepth, initialWeight);            
        }

        /*
         * Used in AddNeuron to add two new links in place of the old selected
         * link. The initial weight is stored and used in one of the new links
         * in order to create as little disruption of any existing learnt
         * behaviour thus far.
         * 
         * The method also create a new innovation for the same neuron in
         * order to prevent NEAT from entering a loop where the following
         * process is repeated over and over:
         * 1. Find a link in the genome.
         * 2. Disable the link
         * 3. Add a new neuron and two new links.
         * 4. When a crossover operator is applied to this genome and a genome
         *   with the link found in step 1 it is then re-enabled and the
         *   process restarts from step 1.
         */
        private void
        AddNewNeuronLinks (int linkIndex, CInnovation innovations)
        {
            float initialWeight = m_vecLinks[linkIndex].Weight;

            int from = m_vecLinks[linkIndex].FromNeuron;
            int to = m_vecLinks[linkIndex].ToNeuron;

            SNeuronGene fromNeuron = m_vecNeurons[GetNeuronIndexById (from)];
            SNeuronGene toNeuron = m_vecNeurons[GetNeuronIndexById (from)];

            // The depth of the neuron is used to determine if the new link
            // feeds backwards (i.e it is a recurrent link) or forwards.
            float newDepth = (fromNeuron.SplitY + toNeuron.SplitY) / 2;
            float newWidth = (fromNeuron.SplitX + toNeuron.SplitX) / 2;

            int innovationID = innovations.CheckInnovation (from, to,
                InnovationType.NewNeuron);
            bool alreadyUsed = innovationID >= 0 &&
                UsingNeuronID (innovations.GetNeuronID (innovationID));
            if (innovationID < 0 || alreadyUsed)
              {
                AddNeuronNewInnovation (innovations, from, to, newWidth,
                    newDepth, initialWeight);
              }
            else
              {
                int newNeuronID = innovations.GetNeuronID (innovationID);
                AddNeuronExistingInnovation (innovations, newNeuronID, from, to,
                    newWidth, newDepth, initialWeight);
              }
        }

        /*
		 * This method attempts to add a new neuron to the network, if
         * a mutation occurs dependent on the mutation rate, in the middle of
         * an already established link between two neurons.
         * 
         * First we disable the randomly chosen link and create a neuron with
         * two new links with one that links to the initial From neuron and
         * one that links to the initial To neuron.
		 */
        public void
        AddNeuron (float mutationRate, CInnovation innovations,
				int numTriesToFindOldLink)
		{
            if (Random.value > mutationRate) return;

            // If this flag is true a valid link for the new neuron has been
            // found.
            bool foundLink = false;

            // Store the index of the link that will be used.
            int linkIndex = 0;

            if (m_vecLinks.Count < (m_iNumInputs + m_iNumOutputs + _params.MinHiddenNeurons))
              {
                foundLink = FindNeuronLinkBiased (out linkIndex, numTriesToFindOldLink);

                if (!foundLink) return;
              }
            else /* The genome is complex enough to select any link */
              {
                do
                  {
                    linkIndex = Random.Range(0, NumGenes());
                  }
                while (!CanLinkBeSplit (linkIndex));
              }

            // Disable this link first so we can replace it with two new.
            m_vecLinks[linkIndex].Disable ();

            AddNewNeuronLinks (linkIndex, innovations);
        }

        /*
		 * This method mutates the weights or strengths of the
		 * connections between the neurons based on a mutation rate.
		 */
        public void
        MutateWeights (float mutationRate, float chanceOfReplacement,
					  float maxPertubation)
		{
			for (int i = 0; i < m_vecLinks.Count; i++)
              {
                SLinkGene connection = m_vecLinks[i];
                if (Random.value < mutationRate)
                  {
                    if (Random.value < chanceOfReplacement)
                      {
                        connection.Weight = Random.Range(-1.0f, 1.0f);
                      }
                    else
                      {
                        connection.Weight += Random.Range(-1.0f, 1.0f) * maxPertubation;
                      }
                  }
			  }
		}

        /*
		 * This method mutates the activation responses of the neurons
		 */
        public void
        MutateActivationResponse (float mutationRate,
                                  float maxPertubation)
        {
            for (int i = 0; i < m_vecNeurons.Count; i++)
              {
                SNeuronGene neuron = m_vecNeurons[i];
                if (Random.value < mutationRate)
                  {
                    neuron.ActivationResponse += Random.Range (-1.0f, 1.0f) * maxPertubation;
                  }
              }
        }

        /*
		 * This method computes how compatible this genome is with
		 * the the other gneome and is used in speciation to determine
         * which genomes goes together in a population and can be grouped
         * together in a species.
         * 
         * This strategy is used to avoid premature extinction.
         * See the species class for further information.
		 */
        public float
        GetCompatibilityScore (CGenome other)
		{
            /*
             * Keep track of these numbers because genomes with different
             * topologies are unlikely to group together well. Therefore
             * the compatability score is based on how different the
             * topologies of the genomes are.
             */
            float numDisjointGenes = 0;
            float numExcessGenes = 0;
            float numMatchedGenes = 0;

            /*
             * The combined error/difference between the weights of the genomes.
             * A lower score means the behaviour is more probable to be
             * similar.
             */
            float weightDifference = 0;

            CalculateTopologyDifference (out numDisjointGenes, out numExcessGenes,
                                         out numMatchedGenes, out weightDifference,
                                         other);

            float longest = Mathf.Max (NumGenes(), other.NumGenes());

            /*
             * Coeffecients that are tweaked to influence the score
             * roughly the same amount.
             */
            const float coefDisjoint = 1.0f;
            const float coefExcess   = 1.0f;
            const float coefMatched  = 0.4f;

            return (coefExcess * numExcessGenes / longest) +
                   (coefDisjoint * numDisjointGenes / longest) +
                   (coefMatched * weightDifference / numMatchedGenes);
		}

        private void
        CalculateTopologyDifference (out float numDisjoint, out float numExcess,
                                     out float numMatched, out float weightDiff,
                                     CGenome other)
        {
            numDisjoint = 0;
            numExcess = 0;
            numMatched = 0;
            weightDiff = 0;

            /*
             * Current position of the gene in both genomes. We increment the
             * position as we traverse the topology of the genomes.
             */
            int g1 = 0;
            int g2 = 0;
            while (g1 < m_vecLinks.Count - 1 || g2 < other.m_vecLinks.Count - 1)
              {
                // more genes in genome1 than genome2
                if (g1 == m_vecLinks.Count - 1)
                  {
                    g2++;
                    numExcess++;

                    continue;
                  }

                // more gens in genome2 than genome1
                if (g2 == other.m_vecLinks.Count - 1)
                  {
                    g1++;
                    numExcess++;

                    continue;
                  }

                int id1 = m_vecLinks[g1].InnovationID;
                int id2 = other.m_vecLinks[g2].InnovationID;

                // compare innovation numbers
                if (id1 == id2)
                  {
                    g1++;
                    g2++;
                    numMatched++;

                    weightDiff += Mathf.Abs((float)(m_vecLinks[g1].Weight - other.m_vecLinks[g2].Weight));
                  }

                if (id1 < id2)
                  {
                    g1++;
                    numDisjoint++;
                  }

                if (id1 > id2)
                  {
                    g2++;
                    numDisjoint++;
                  }
              }
        }

        /*
		 * Sorts the neurons by innovation ID.
		 */
        public void
		SortGenes ()
		{
            m_vecLinks.Sort (delegate (SLinkGene a, SLinkGene b) {
                return a.InnovationID.CompareTo(b.InnovationID);
            });
        }

        /* accessor methods */
        public int
        ID ()
        {
            return m_GenomeID;
        }

        public void
        SetID (int id)
        {
            m_GenomeID = id;
        }

        public int
        NumNeurons ()
        {
            return m_vecNeurons.Count;
        }

        public int
        NumGenes ()
        {
            return m_vecLinks.Count;
        }

        public int
        NumInputs ()
        {
            return m_iNumInputs;
        }

        public int
        NumOutputs()
        {
            return m_iNumOutputs;
        }

        public float
        Fitness ()
        {
            return m_dFitness;
        }

        public float
        GetAdjustedFitness()
        {
            return this.m_dAdjustedFitness;
        }

        public float
        SplitY (int neuronIndex)
        {
            return m_vecNeurons[neuronIndex].SplitY;
        }

        public CNeuralNet
        Phenotype ()
        {
            return m_Phenotype;
        }

        public List<SLinkGene>
        GetLinks ()
        {
            return m_vecLinks;
        }

        public List<SNeuronGene>
        GetNeurons ()
        {
            return m_vecNeurons;
        }
        
        public float
        AmountToSpawn ()
        {
            return m_dAmountToSpawn;
        }

        public void
        SetFitness (float fitness)
        {
            this.m_dFitness = fitness;
        }

        public void
        SetAdjustedFitness (float newFitness)
        {
            this.m_dAdjustedFitness = newFitness;
        }

        public void
        SetAmountToSpawn (float toSpawn)
        {
            this.m_dAmountToSpawn = toSpawn;
        }

        public void
        SetSpecies (int speciesID)
        {
            this.m_iSpecies = speciesID;
        }
    }
}