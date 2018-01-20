using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeciesDistributionBar : MonoBehaviour {

    public GameObject sliderPrefab;

    /*
     * This list contains the number of members for each species and is sorted
     * by the species ID. From lowest to highest.
     */
    private List<int> distributions;

    private List<SpeciesSlider> sliders;
	
    void
    Start ()
    {
        sliders = new List<SpeciesSlider> ();
    }

	public void
    UpdateBar (Dictionary<int, int> distributions)
    {
        if (distributions == null || distributions.Count == 0) return;

        this.distributions = distributions.Values.ToList();

        AddSliders ();

        /*
         * The sum of all species members should be equal to the total amount
         * of genomes spawned.
         */
        int total = CalculateTotal ();
        float numColours = 1.0f / distributions.Count;
        float portion = 1.0f;

        for (int i = 0; i < distributions.Count; i++)
          {
            SpeciesSlider speciesSlider = sliders[i];
            int id = distributions.Keys.ElementAt(i);
            int memberCount = distributions.Values.ElementAt(i);
            float ratio = (float) memberCount / (float) total;

            UpdateSlider (speciesSlider, id, portion, new Color(numColours * i, 1.0f, 255 - numColours * i));

            portion -= ratio;            
          }
	}

    private void
    UpdateSlider (SpeciesSlider speciesSlider, int id, float portion, Color colour)
    {
        Slider slider = speciesSlider.GetComponent<Slider> ();
        slider.value = portion;

        speciesSlider.SetFillColour (colour);
        
        if (portion == 0.0f)
        {
            ColorBlock colours = slider.colors;
            Color transparent = Color.white;
            transparent.a = 0;
            colours.disabledColor = transparent;
            slider.colors = colours;
            speciesSlider.speciesID.enabled = false;
        }
        else
        {
            ColorBlock colours = slider.colors;
            colours.disabledColor = Color.white;
            slider.colors = colours;
            speciesSlider.SetSpeciesID(id);
            speciesSlider.speciesID.enabled = true;
        }
    }

    private int
    CalculateTotal ()
    {
        int sum = 0;

        foreach (int i in distributions)
          {
            sum += i;
          }

        return sum;
    }

    private void
    AddSliders ()
    {
        while (distributions.Count != sliders.Count)
          {
            if (sliders.Count > distributions.Count)
              {
                Destroy (sliders[sliders.Count - 1].gameObject);
                sliders.RemoveAt (sliders.Count - 1);
              }
            else
              {
                SpeciesSlider newSlider = Instantiate (sliderPrefab, transform).GetComponent<SpeciesSlider> ();
                sliders.Add (newSlider);
              }
          }
    }
}
