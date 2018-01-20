using System.Collections.Generic;
using UnityEngine;
using EANN;
using GA;

namespace GA
{

    /*
     * Helper struct used in calculating the neural network depth. See the
     * Split method in the genetic algorithm class for more details.
     */
    struct SplitDepth
    {
        public double value;
        public int depth;

        public
        SplitDepth (double v, int d)
        {
            value = v;
            depth = d;
        }
    };

    /*
     * This is the class that is responsible for handling the evolutionary
     * algorithm based on Kenneth Owen Stanley's and Risto Miikkulainen's NEAT idea.
     * It manipulates all the genomes, species and innovations.
     * 
     * The epoch method for each generation is defined here and the crossover
     * algorithm is also implemented in this class.
     */
    public class Cga
    {
        /*
         * This enum is used in the crossover method to easily distinguish the
         * type of genome that is fittest.
         */
        private enum ParentType
        {
            Mum,
            Dad
        }

        public
        Cga (int inputs, int outputs)
        {
            m_Population = new List<CGenome> ();

            for (int i = 0; i < _params.NumGenomesToSpawn; i++)
              {
                m_Population.Add (new CGenome (nextGenomeID++, inputs, outputs));
              }
            m_PopSize = m_Population.Count;

            // Create simple genome used for innovations database
            CGenome genome = new CGenome (1, inputs, outputs);

            m_vecSpecies = new List<CSpecies> ();

            innovation = new CInnovation (genome.GetLinks (), genome.GetNeurons ());

            vecSplits = new List<SplitDepth> ();

            // Create the network depth lookup table.
            Split (0, 1, 0);
        }

        /*
         * Reference to the innovation database used in the crossover method.
         */
        private CInnovation innovation;

        /*
         * The list of genomes that makes up the current population.
         */
        private List<CGenome> m_Population;

        /*
         * The list of species in the current populaton.
         */
        private List<CSpecies> m_vecSpecies;

        /*
         * TODO: explain me
         */
        private List<SplitDepth> vecSplits;

        private int m_PopSize;

        /*
         * Each genome that is created gets a ID and this variable keeps track
         * of the last assigned id.
         */
        private int nextGenomeID;

        /*
         * Every species also gets a ID, keep track of the last assigned id.
         */
        private int nextSpeciesID;

        /* TODO: explain these */
        private float m_dTotFitAdj = 0;
        private float m_dAvFitAdj = 0;
        private float m_dBestEverFitness = 0;

        private int m_iGeneration;

        /*
         * Local reference to singleton class holding parameters.
         */
        private Parameters _params = Parameters.instance;

        private void
        AddNeuronID (int neuronID, List<int> neuronIDs)
        {
            for (int i = 0; i < neuronIDs.Count; i++)
              {
                if (neuronIDs[i] == neuronID)
                  {
                    //already added
                    return;
                  }
              }

            neuronIDs.Add (neuronID);
        }

        private ParentType
        DetermineBestParent (CGenome mum, CGenome dad)
        {
            if (mum.Fitness() == dad.Fitness())
              {
                if (mum.NumGenes() == dad.NumGenes())
                  {
                    if (Random.value < 0.5)
                        return ParentType.Mum;
                    else
                        return ParentType.Dad;
                  }
                else
                  {
                    /*
                     * Choose the parent with the fewest genes because
                     * the fitness is the same.
                     */
                    if (mum.NumGenes() < dad.NumGenes()) return ParentType.Mum;
                    else return ParentType.Dad;
                  }
              }
            else
              {
                if (mum.Fitness() > dad.Fitness()) return ParentType.Mum;
                else return ParentType.Dad;
              }
        }

        private void
        SelectGenesToBreed (CGenome mum, CGenome dad, ParentType best,
                            List<SLinkGene> babyGenes,
                            List<int> neuronIDs)
        {
            List<SLinkGene> mumLinks = mum.GetLinks ();
            List<SLinkGene> dadLinks = dad.GetLinks ();
            using (IEnumerator<SLinkGene> mumEnumerator = mumLinks.GetEnumerator())
            using (IEnumerator<SLinkGene> dadEnumerator = dadLinks.GetEnumerator())
              {
                bool hasMumMore = mumEnumerator.MoveNext ();
                bool hasDadMore = dadEnumerator.MoveNext ();
                SLinkGene selectedGene = mumEnumerator.Current;

                while (hasMumMore || hasDadMore)
                  {
                    if (!hasMumMore && hasDadMore)
                      {
                        if (best == ParentType.Dad)
                          {
                            selectedGene = dadEnumerator.Current;
                          }
                        hasDadMore = dadEnumerator.MoveNext();
                      }
                    else if (!hasDadMore && hasMumMore)
                      {
                        if (best == ParentType.Mum)
                          {
                            selectedGene = mumEnumerator.Current;
                          }
                        hasMumMore = mumEnumerator.MoveNext();
                      }
                    else if (mumEnumerator.Current.InnovationID < dadEnumerator.Current.InnovationID)
                      {
                        if (best == ParentType.Mum)
                          {
                            selectedGene = mumEnumerator.Current;
                          }
                        hasMumMore = mumEnumerator.MoveNext();
                      }
                    else if (dadEnumerator.Current.InnovationID < mumEnumerator.Current.InnovationID)
                      {
                        if (best == ParentType.Dad)
                          {
                            selectedGene = dadEnumerator.Current;
                          }
                        hasDadMore = dadEnumerator.MoveNext();
                      }
                    else if (dadEnumerator.Current.InnovationID == mumEnumerator.Current.InnovationID)
                      {
                        if (Random.value < 0.5f)
                            selectedGene = mumEnumerator.Current;
                        else
                            selectedGene = dadEnumerator.Current;

                        hasMumMore = mumEnumerator.MoveNext();
                        hasDadMore = dadEnumerator.MoveNext();
                      }
                    
                    if (babyGenes.Count == 0)
                        babyGenes.Add (new SLinkGene (selectedGene));
                    else
                      {
                        if (babyGenes[babyGenes.Count - 1].InnovationID !=
                            selectedGene.InnovationID)
                          {
                            babyGenes.Add (new SLinkGene (selectedGene));
                          }                        
                      }

                    AddNeuronID (selectedGene.FromNeuron, neuronIDs);
                    AddNeuronID (selectedGene.ToNeuron, neuronIDs);
                  }
              }
        }

        /*
         * TODO: talk about the difference between disjoint/excess genes
         */
        private CGenome
        Crossover (CGenome mum, CGenome dad)
        {
            ParentType best = DetermineBestParent (mum, dad);

            // The resulting offspring produced are stored in these lists.
            List<SNeuronGene> babyNeurons = new List<SNeuronGene>();
            List<SLinkGene> babyGenes = new List<SLinkGene>();
            List<int> neuronIDs = new List<int>();

            SelectGenesToBreed (mum, dad, best, babyGenes, neuronIDs);

            neuronIDs.Sort();

            // Create the new neurons using the neuron ids.
            for (int i = 0; i < neuronIDs.Count; i++)
              {
                int neuronID = neuronIDs[i];
                babyNeurons.Add (this.innovation.CreateNeuronByID (neuronID));
              }

            // Create the baby genome using the newly created neurons.
            return new CGenome (nextGenomeID++, babyNeurons, babyGenes, mum.NumInputs(), mum.NumOutputs());
        }

        /*TODO: double check me
         * This stage of the epoch deletes all the previous phenotypes.
         * It also deletes all members of each species except the best
         * performing ones. This is a helper method used in Epoch.
         */
        private void
        ResetAndKill ()
        {
            m_dTotFitAdj = 0;
            m_dAvFitAdj = 0;

            // Purge the species that doesn't improve
            for (int i = m_vecSpecies.Count - 1; i >= 0; i--)
              {
                CSpecies species = m_vecSpecies[i];
                species.Purge ();

                if (species.GensNoImprovement () >
                    _params.NumGensAllowedNoImprovement &&
                    species.BestFitness () < m_dBestEverFitness)
                  {
                    m_vecSpecies.RemoveAt (i);
                    Debug.Log ("killed species " + species.ID ());
                  }
              }

            for (int i = 0; i < m_Population.Count; i++)
              {
                CGenome genome = m_Population[i];
                genome.DeletePhenotype ();
              }
        }

        /*
         * This is a helper method used in Epoch.
         */
        private void
        SortAndRecord ()
        {
            // sort the genomes without taking into account fitness sharing
            m_Population.Sort(delegate (CGenome a, CGenome b) {
                return b.Fitness ().CompareTo(a.Fitness ());
            });

            if (m_Population[0].Fitness () > m_dBestEverFitness)
              {
                m_dBestEverFitness = m_Population[0].Fitness ();
              }            
        }

        /*
         * This method calculates the compatibility distance of each genome
         * against the leader of each species in the population and assigns
         * the genome to the species with the least compatibility distance.
         * If no match is found within a set tolerance, a new species is
         * created with the genome added to that as the leader. This is a
         * helper method used in Epoch.
         */
        public void
        SpeciateGenomes ()
        {
            bool addedToSpecies = false;

            for (int i = 0; i < m_Population.Count; i++)
            {
                CGenome genome = m_Population[i];

                float bestCompatability = 1000;
                CSpecies bestSpecies = null;

                for (int j = 0; j < m_vecSpecies.Count; j++)
                  {
                    CSpecies species = m_vecSpecies[j];
                    float compatibility = genome.GetCompatibilityScore (species.Leader ());

                    // if this individual is similar to this species leader add to species
                    if (compatibility < bestCompatability)
                      {
                        bestCompatability = compatibility;
                        bestSpecies = species;                        
                      }
                  }
                
                if (bestCompatability <= _params.CompatibilityThreshold)
                  {
                    bestSpecies.AddMember(genome);

                    genome.SetSpecies(bestSpecies.ID());

                    addedToSpecies = true;
                  }
                  
                if (!addedToSpecies)
                  {
                    // we have not found a compatible species so a new one will be created
                    m_vecSpecies.Add (new CSpecies (genome, nextSpeciesID++));
                  }

                addedToSpecies = false;
              }

            /*
             * Adjust the fitness for all members of the every species to take
             * into account fitness sharing and age of species.
             */
            for (int i = 0; i < m_vecSpecies.Count; i++)
              {
                CSpecies species = m_vecSpecies[i];
                species.AdjustFitnesses ();
              }

            /*
             * Calculate new adjusted total and average fitness for the population.
             */
            for (int i = 0; i < m_Population.Count; i++)
              {
                CGenome genome = m_Population[i];
                m_dTotFitAdj += genome.GetAdjustedFitness ();
              }

            m_dAvFitAdj = m_dTotFitAdj / m_Population.Count;
        }

        /*
         * This method calculates how many offspring is predicted to be
         * spawned by each individual and added to the new generation.
         * 
         * The offspring is derived from dividing each genome's adjusted
         * fitness by the average adjusted fitness for the entire population.
         * 
         * This is a helper method used in Epoch.
         */
        public void
        CalculateSpawnLevels ()
        {
            for (int i = 0; i < m_Population.Count; i++)
              {
                CGenome genome = m_Population[i];
                float toSpawn = 0;

                if (m_dAvFitAdj > 0)
                    toSpawn = genome.GetAdjustedFitness() / m_dAvFitAdj;

                genome.SetAmountToSpawn (toSpawn);
              }

            for (int i = 0; i < m_vecSpecies.Count; i++)
              {
                CSpecies species = m_vecSpecies[i];
                species.CalculateSpawnAmount ();
              }
        }

        public void
        MutateBaby (CGenome baby)
        {
            if (baby.NumNeurons () < _params.MaxPermittedNeurons)
              {
                baby.AddNeuron (_params.ChanceAddNode, innovation,
                                _params.NumTriesToFindOldLink);
              }

            baby.AddLink (_params.ChanceAddLink,
                          _params.ChanceAddRecurrentLink, innovation,
                          _params.NumTriesToFindLoopedLink,
                          _params.NumAddLinkAttempts);
                          
            baby.MutateWeights (_params.MutationRate,
                                _params.ChanceWeightReplaced,
                                _params.MaxWeightPerturbation);
                                
            baby.MutateActivationResponse (_params.ActivationMutationRate,
                                           _params.MaxActivationPerturbation);
        }

        /*
         * Implementation of per species elitism by transfering the
         * best performing genome to the new population without any
         * mutations.
         */
        public int
        SpawnLeaders (List<CGenome> newPopulation)
        {
            for (int i = 0; i < m_vecSpecies.Count; i++)
              {
                CSpecies species = m_vecSpecies[i];
                CGenome baby = species.Leader ();

                Debug.Log("spawning leader (" + baby.Fitness() + "): " + baby.ID() + " for " + species.ID());
                newPopulation.Add (baby);
            }

            return newPopulation.Count;
        }

        /*
         * TODO: explain me
         * 
         * This is a helper method used in Epoch.
         */
        public int
        SpawnOffspring (CSpecies species, int numSpawned,
                        List<CGenome> newPopulation)
        {
            CGenome baby = null;

            /*
             * Prevent overflowing the total number of genomes spawned per population.
             */
            if (numSpawned < _params.NumGenomesToSpawn)
              {
                // Exclude the leader from numToSpawn.
                int numToSpawn = Mathf.RoundToInt (species.NumToSpawn ()) - 1;

                numToSpawn = Mathf.Min(numToSpawn, _params.NumGenomesToSpawn - numSpawned);

                Debug.Log ("spawning " + numToSpawn + " num indivudals for species " + species.ID () + ", best fitness: " + species.BestFitness ());

                while (numToSpawn-- > 0)
                  {
                    /*
                     * Unless we have >2 members in the species crossover
                     * can't be performed.
                     */
                    if (species.NumMembers () == 1)
                      {
                        baby = new CGenome (species.Spawn ());
                      }
                    else
                      {
                        CGenome mum = species.Spawn ();

                        if (Random.value < _params.CrossoverRate)
                          {
                            CGenome dad = species.Spawn ();

                            int numAttempts = 5;
                                
                            // try to select a genome which is not the same as mum
                            while (mum.ID () == dad.ID () && numAttempts-- > 0)
                              {
                                dad = species.Spawn ();
                              }

                            if (mum.ID () != dad.ID ())
                              {
                                baby = Crossover (mum, dad);
                              }
                            else
                              {
                                if (Random.value < 0.5f)
                                    baby = new CGenome (dad);
                                else
                                    baby = new CGenome (mum);
                            }
                          }
                        else
                          {
                            baby = new CGenome (mum);
                          }
                      }

                    if (baby == null) continue;

                    baby.SetID (++nextGenomeID);

                    MutateBaby (baby); //EDIT me

                    baby.SortGenes ();                    

                    newPopulation.Add (baby);

                    if (numSpawned++ >= _params.NumGenomesToSpawn)
                      {
                        numToSpawn = 0;
                        break;
                      }
                  }
              }

            return numSpawned;
        }

        /*
         * TODO: add explanation
         * 
         * This is a helper method used in Epoch.
         */
        public CGenome
        TournamentSelection (int numSelections)
        {
            double bestFitness = 0;
            CGenome best = m_Population[0];

            for (int i = 0; i < numSelections; i++)
              {
                CGenome genome = m_Population[Random.Range(0, m_Population.Count)];

                if (genome.Fitness() > bestFitness)
                  {
                    best = genome;

                    bestFitness = genome.Fitness();
                  }
              }

            return best;
        }

        /*
         * TODO: explain how this works
         * 
         * This is a helper method used in CreatePhenotypes.
         */
        public int
        CalculateNetDepth (CGenome genome)
        {
            int maxDepth = 0;

            for (int i = 0; i < genome.NumNeurons(); i++)
              {
                for (int j = 0; j < vecSplits.Count; j++)
                  {
                    SplitDepth split = vecSplits[i];
                    if (genome.SplitY (i) == split.value &&
                        split.depth > maxDepth)
                      {
                        maxDepth = split.depth;
                      }
                  }
              }

            return maxDepth + 2;
        }

        /*
         * TODO: explain me
         */
        void Split (double low, double high, int depth)
        {
            double span = high - low;

            vecSplits.Add (new SplitDepth (low + span / 2, depth + 1));

            if (depth <= 6) // TODO: MAGIC NUMBER???
              {
                Split (low, low + span / 2, depth + 1);
                Split (low + span / 2, high, depth + 1);
              }
        }

        /*
         * This method converts all the genomes into phenotypes
         * (i.e neural networks). For details on how this is done
         * see the CreatePhenotype method in CGenome. This is a helper
         * method used in Epoch.
         */
        public List<CNeuralNet>
        CreatePhenotypes ()
        {
            List<CNeuralNet> newPhenotypes = new List<CNeuralNet> ();

            for (int i = 0; i < m_Population.Count; i++)
              {
                CGenome genome = m_Population[i];

                int depth = CalculateNetDepth (genome);

                newPhenotypes.Add (genome.CreatePhenotype (depth));
              }

            return newPhenotypes;
        }

        /*
         * The epoch method is the workhorse of the genetic algorithm and its job
         * is to create a new set of phenotypes which will be evolved to gain the
         * most fitness score.
         * 
         * One epoch means one generation. Every epoch a number of stages are
         * executed. In summary TODO: write summary of the epoch method
         */
        public List<CNeuralNet>
        Epoch (Dictionary<int, float> fitnessScores)
        {
            if (fitnessScores.Count != m_Population.Count)
              {
                Debug.LogError ("scores and genomes mismatch error. (" + fitnessScores.Count + " / " + m_Population.Count + ")");
                return null;
              }

            ResetAndKill ();

            for (int i = 0; i < m_Population.Count; i++)
              {
                CGenome genome = m_Population[i];
                genome.SetFitness (fitnessScores[genome.ID ()]);
              }

            SortAndRecord ();
            SpeciateGenomes ();
            CalculateSpawnLevels ();

            Debug.Log("best fitness last gen: " + m_Population[0].Fitness ());

            List<CGenome> newPopulation = new List<CGenome> ();

            int numSpawned = SpawnLeaders (newPopulation);
            
            for (int i = 0; i < m_vecSpecies.Count; i++)
              {
                CSpecies species = m_vecSpecies[i];
                numSpawned = SpawnOffspring (species, numSpawned, newPopulation);
              }                 

            /*
             * Tournament selection is used over the entire population if due
             * to a underflow of the species amount to spawn the offspring
             * doesn't fill the entire population additional children needs to
             * created.
             */
            if (numSpawned < _params.NumGenomesToSpawn)
              {
                int numMoreToSpawn = _params.NumGenomesToSpawn - numSpawned;

                while (numMoreToSpawn-- > 0)
                  {
                    CGenome baby = new CGenome (TournamentSelection (m_PopSize / 5));
                    baby.SetID (++nextGenomeID);
                    newPopulation.Add (baby);
                  }
              }

            m_Population = newPopulation;

            m_iGeneration++;

            return CreatePhenotypes ();
        }

        /* accessor methods */

        public List<CSpecies>
        Species ()
        {
            return m_vecSpecies;
        }

        public List<CGenome>
        Genomes ()
        {
            return m_Population;
        }

        public float
        BestFitness ()
        {
            return m_dBestEverFitness;
        }

        public void
        SetPopulation (List<CGenome> newPopulation)
        {
            this.m_Population = newPopulation;
        }
    }    
}
