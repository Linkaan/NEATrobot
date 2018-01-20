using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EANN;

public class NeuronRenderer : MonoBehaviour {

    public GameObject linkPrefab;

    public SNeuron neuron;

    public float splitX;
    public float splitY;

    void
    Start ()
    {
        Color colour;

        for (int i = 0; i < neuron.linksFrom.Count; i++)
          {
            LinkRenderer link = Instantiate (linkPrefab, transform).GetComponent<LinkRenderer> ();
            link.link = neuron.linksFrom[i];
          }        

        switch (neuron.type)
          {
            case GA.NeuronType.Bias:
                colour = Color.green;
                break;
            default:
            case GA.NeuronType.Hidden:
                colour = Color.white;
                break;
            case GA.NeuronType.Input:
                colour = Color.blue;
                break;
            case GA.NeuronType.Output:
                colour = Color.red;
                break;
          }

        GetComponent<Image> ().color = colour;

        splitX = neuron.splitX;
        splitY = neuron.splitY;
	}
}
