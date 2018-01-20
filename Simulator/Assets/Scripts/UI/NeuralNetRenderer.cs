using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EANN;

public class NeuralNetRenderer : MonoBehaviour {

    /*
     * A layer is not a neural network layer per se but might simply represent
     * a set of neurons with the same SplitX value.
     */
    public GameObject layerPrefab;

    /*
     * The neuron gameobject is responsible for rendering all the links to
     * other neurons.
     */
    public GameObject neuronPrefab;

    /*
     * The neural net to draw on screen.
     */
    private CNeuralNet neuralNet;

    private Dictionary<float, GameObject> layers;

    private Dictionary<int, NeuronRenderer> neurons;

    void
    Awake ()
    {
        layers = new Dictionary<float, GameObject> ();
        neurons = new Dictionary<int, NeuronRenderer> ();
    }

    public NeuronRenderer
    GetNeuronById (int neuronID)
    {
        if (!neurons.ContainsKey(neuronID)) return null;
        return neurons[neuronID];
    }
	
	public void        
    UpdateNeural (CNeuralNet neuralNet)
    {
        this.neuralNet = neuralNet;
        neurons.Clear ();

		AddLayers ();

        List<SNeuron> neuronsToAdd = new List<SNeuron>();

        for (int i = 0; i < layers.Count; i++)
          {
            float splitY = layers.ElementAt (i).Key;
            Transform parentLayer = layers[splitY].transform;            
            neuronsToAdd.Clear ();

            // select all neurons with the same splitY value
            for (int j = 0; j < neuralNet.Neurons ().Count; j++)
              {
                SNeuron neuron = neuralNet.Neurons()[j];
                if (neuron.splitY != splitY) continue;

                neuronsToAdd.Add (neuron);
              }

            // sort by splitX values
            neuronsToAdd.Sort(delegate (SNeuron a, SNeuron b) {
                return a.splitX.CompareTo(b.splitX);
            });

            for (int j = 0; j < neuronsToAdd.Count; j++)
              {
                SNeuron neuron = neuronsToAdd[j];

                if (neurons.ContainsKey(neuron.neuronID))
                  {
                    Debug.LogWarning("a neuron with id " + neuron.neuronID + " already exists!");
                    continue;
                  }

                NeuronRenderer neuronRenderer = Instantiate(neuronPrefab, parentLayer).GetComponent<NeuronRenderer>();
                neuronRenderer.neuron = neuron;
                
                neurons.Add(neuron.neuronID, neuronRenderer);
            }
        }
    }

    // TODO: explain me
    private void
    AddLayers()
    {
        foreach (GameObject layer in layers.Values)
          {
            Destroy (layer);
          }
        layers.Clear ();

        List<float> splitYs = new List<float> ();

        for (int i = 0; i < neuralNet.Neurons ().Count; i++)
          {
            float splitY = neuralNet.Neurons ()[i].splitY;
            if (!splitYs.Contains (splitY))
              {                
                splitYs.Add (splitY);
              }
          }

        splitYs.Sort ();

        for (int i = 0; i < splitYs.Count; i++)
          {
            layers.Add (splitYs[i], Instantiate (layerPrefab, transform));
          }
    }
}
