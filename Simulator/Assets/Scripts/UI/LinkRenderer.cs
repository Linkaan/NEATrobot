using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using EANN;

public class LinkRenderer : MonoBehaviour {

    public UILineRenderer lineRenderer;
    public float radiusRecurrent = 1.0f;
    public float recurrentOffset = -1.5f;
    public int recurrentSegments = 20;

    public float thicknessCoefficient = 1.0f;
    public float maxThickness = 5.0f;
    public float minThickness = 0.5f;

    public SLink link;

    public bool isRecurrent;
    public float weight;
   
    void
    Update ()
    {
        if (link == null) return;

        Color colour;
        float thickness = 1.0f;

        thickness = Mathf.Clamp(link.weight * thicknessCoefficient, minThickness, maxThickness);

        isRecurrent = link.isRecurrent;
        weight = link.weight;

        if (link.isRecurrent)
          {
            if (link.weight < 0)
              {
                colour = Color.blue;
              }
            else if (link.weight == 0)
              {
                colour = Color.black;
              }
            else
              {
                colour = Color.red;
              }

            if (link.fromNeuron == link.toNeuron)
              {
                float x;
                float y;
                float angle = 20f;

                List<Vector2> newPoints = new List<Vector2> ();

                for (int i = 0; i < (recurrentSegments + 1); i++)
                {
                    x = Mathf.Sin(Mathf.Deg2Rad * angle) * radiusRecurrent;
                    y = Mathf.Cos(Mathf.Deg2Rad * angle) * radiusRecurrent - recurrentOffset;

                    newPoints.Add (new Vector2 (x, y));

                    angle += (360f / recurrentSegments);
                }

                lineRenderer.color = colour;
                lineRenderer.LineThickness = thickness;
                lineRenderer.Points = newPoints.ToArray ();
                return;
              }            
          }
        else
          {            
            if (link.weight <= 0)
              {
                colour = Color.yellow;
              }
            else if (link.weight == 0)
              {
                colour = Color.black;
              }
            else
              {
                colour = Color.grey;
              }
          }

        // draw bias links in green
        NeuronRenderer neuronRenderer = GetComponentInParent<NeuralNetRenderer>().GetNeuronById(link.fromNeuron);
        if (neuronRenderer == null)
          {
            Debug.LogWarning("neuron with neuron id " + link.fromNeuron + " does not exist!");
            Destroy(this.gameObject);
            return;
          }

        if (neuronRenderer.neuron.type == GA.NeuronType.Bias)
          {
            colour = Color.green;
          }

        lineRenderer.color = colour;
        lineRenderer.LineThickness = thickness;
        Vector2 endPoint = NeuronPositionById (link.toNeuron);
        lineRenderer.Points = new Vector2[] { Vector2.zero, endPoint };
    }

    private Vector2
    NeuronPositionById (int id)
    {
        NeuronRenderer neuron = GetComponentInParent<NeuralNetRenderer> ().GetNeuronById (id);
        if (neuron == null)
          {
            Debug.LogWarning ("neuron with neuron id " + id + " does not exist!");
            Destroy (this.gameObject);
            return Vector2.zero;
          }
        Vector3 originWorldPosition = GetComponentInParent<NeuronRenderer> ().transform.position;
        Vector3 worldPosition = neuron.transform.position;


        RectTransform canvasRect = GetComponentInParent<Canvas> ().GetComponent<RectTransform>();

        Vector2 viewportPosition = Camera.main.WorldToViewportPoint(originWorldPosition);
        Vector2 originScreenPos = new Vector2(
            ((viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
            ((viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)));

        viewportPosition = Camera.main.WorldToViewportPoint(worldPosition);
        Vector2 screenPos = new Vector2(
            ((viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
            ((viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)));

        return screenPos - originScreenPos;
    }


}
