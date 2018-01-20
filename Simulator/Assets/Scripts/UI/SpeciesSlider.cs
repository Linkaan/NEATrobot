using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeciesSlider : MonoBehaviour {

    public Image fill;
    public Text speciesID;

    public void SetFillColour (Color colour)
    {
        fill.color = colour;
    }

    public void SetSpeciesID (int id)
    {
        speciesID.text = "" + id;
    }
}
