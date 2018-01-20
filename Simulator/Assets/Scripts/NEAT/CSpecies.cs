using System.Collections.Generic;
using UnityEngine;
using EANN;
using GA;

namespace GA
{
    /*
	 * This class is used to hold information about a group of genomes that have
     * similar topology and can be said to belong to the same "species".
     * 
     * The information is used in the fitness adjustment calculations and
     * is used to "protect" newer innovations within the population. The
     * technique that is used in NEAT is called explicit fitness sharing
     * so fitness score will be identical between the groups within the
     * same species.
     */
    public class CSpecies
    {

        public
        CSpecies (CGenome leader, int speciesID)
        {
            m_vecMembers = new List<CGenome> ();

            m_iSpeciesID = speciesID;
            m_dBestFitness = leader.Fitness ();
            m_Leader = leader;
            m_vecMembers.Add (leader);
        }     

        /*
         * The leader of a species is simply the first member.
         */
        private CGenome m_Leader;

        /*
         * This list references all the genomes that are part of this species.
         */
        private List<CGenome> m_vecMembers;

        /*
         * Identifaction number for this species.
         */
        private int m_iSpeciesID;

        /*
         * The best fitness of this species.
         */
        private float m_dBestFitness;

        /*
         * The average fitness of this species.
         */
        private float m_dAvgFitness;

        /*
         * Store the number of generations that has passed since
         * there was an improvement to this species.
         * 
         * This is used to elimnate species with no improvements.
         */
        private int m_iGensWithoutImproving;

        /*
         * The total age in generations.
         */
        private int m_iAge;

        /*
         * This variable keeps track of how many offspring should be spawned
         * in the next population.
         * 
         * It's impossible for an organism to spawn a fractional part of
         * itslef but a float is used so that when all spawn amounts are
         * summed together, an overall spawn amount can be calculated with
         * more accuracy.
         */
        private float m_dAmountToSpawn;

        /*
         * Local reference to singleton class holding parameters.
         */
        private Parameters _params = Parameters.instance;

        /*
         * Calculation of adjusted fitness depending on age of this species.
         * The young species will get a boost of fitness and the
         * old will be punished.
         */
        public void
        AdjustFitnesses ()
        {
            float total = 0;

            for (int i = 0; i < m_vecMembers.Count; i++)
              {
                CGenome member = m_vecMembers[i];
                float fitness = member.Fitness ();

                if (m_iAge < _params.YoungAgeThreshold)
                  {
                    fitness *= _params.YoungFitnessBonus;
                  }
                else if (m_iAge > _params.OldAgeThreshold)
                  {
                    fitness *= _params.OldAgePenalty;
                  }

                total += fitness;

                // Calculation of fitness sharing
                float adjustedFitness = fitness / m_vecMembers.Count;

                member.SetAdjustedFitness (adjustedFitness);
              }
        }

        public void
        AddMember (CGenome genome)
        {
            if (genome.Fitness () > m_dBestFitness)
              {
                m_dBestFitness = genome.Fitness ();
                m_iGensWithoutImproving = 0;
                m_Leader = genome;
              }

            m_vecMembers.Add (genome);
            /*
            m_vecMembers.Sort(delegate (CGenome a, CGenome b) {
                return b.Fitness().CompareTo(a.Fitness());
            });*/
        }

        /*
         * Empty the species and update age and generations without
         * improvements.
         */
        public void
        Purge ()
        {
            m_vecMembers.Clear ();

            m_iAge++;
            m_iGensWithoutImproving++;

            m_dAmountToSpawn = 0;
        }

        /*
         * This method calculates the amount of offspring this species should
         * spawn in the next population. This is equal to the sum of the
         * expected spawn amount for each member in the species.
         */
        public void
        CalculateSpawnAmount ()
        {
            for (int i = 0; i < m_vecMembers.Count; i++)
              {
                CGenome member = m_vecMembers[i];
                m_dAmountToSpawn += member.AmountToSpawn ();
              }
        }

        /*
         * This method returns a random indivudal selected from the best
         * among the members of this species.
         */
        public CGenome
        Spawn ()
        {
            CGenome baby;

            if (m_vecMembers.Count == 1)
              {
                baby = m_vecMembers[0];
              }
            else
              {                
                int maxIndex = (int)(_params.SurvivalRate * m_vecMembers.Count) + 1;

                baby = m_vecMembers[Random.Range (0, maxIndex)];
              }

            return baby;
        }

        /* accessor methods */

        public int
        ID ()
        {
            return m_iSpeciesID;
        }

        public float
        NumToSpawn ()
        {
            return m_dAmountToSpawn;
        }

        public CGenome
        Leader()
        {
            return m_Leader;
        }

        public int
        Age ()
        {
            return m_iAge;
        }

        public int
        NumMembers ()
        {
            return m_vecMembers.Count;
        }

        public int
        GensNoImprovement ()
        {
            return m_iGensWithoutImproving;
        }

        public float
        BestFitness ()
        {
            return m_dBestFitness;
        }

    }

}