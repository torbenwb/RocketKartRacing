using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ForceGizmo : MonoBehaviour
{
    public CarController carController;
    public Material lineMaterial;
    public List<LineRenderer> lineRenderers;
    public bool x = true, y = true, z = true;
    public float directionMagnitude = 0.2f;

    private void Start()
    {
        lineRenderers = new List<LineRenderer>();
        foreach(CarController.WheelForces w in carController.wheelForces){
            for (int i = 0; i < 3; i++){
                GameObject newLineRenderer = new GameObject("Line Renderer");
                newLineRenderer.transform.SetParent(transform);
                lineRenderers.Add(newLineRenderer.AddComponent<LineRenderer>());
                newLineRenderer.GetComponent<LineRenderer>().material = lineMaterial;
                newLineRenderer.GetComponent<LineRenderer>().startWidth = 0.1f;
                newLineRenderer.GetComponent<LineRenderer>().endWidth = 0.1f;
            }
           
            
        }
    }

    private void Update()
    {
        int i = 0;
        foreach(CarController.WheelForces w in carController.wheelForces){
            lineRenderers[i].gameObject.SetActive(x);
            lineRenderers[i].positionCount = 2;
            lineRenderers[i].startColor = Color.red;
            lineRenderers[i].endColor = Color.red;
            lineRenderers[i].SetPosition(0, w.position);
            lineRenderers[i].SetPosition(1,w.position + w.xAxis * directionMagnitude);
            i++;
            lineRenderers[i].gameObject.SetActive(y);
            lineRenderers[i].positionCount = 2;
            lineRenderers[i].startColor = Color.green;
            lineRenderers[i].endColor = Color.green;
            lineRenderers[i].SetPosition(0, w.position);
            lineRenderers[i].SetPosition(1,w.position + w.yAxis * directionMagnitude);
            i++;
            lineRenderers[i].gameObject.SetActive(z);
            lineRenderers[i].positionCount = 2;
            lineRenderers[i].startColor = Color.blue;
            lineRenderers[i].endColor = Color.blue;
            lineRenderers[i].SetPosition(0, w.position);
            lineRenderers[i].SetPosition(1,w.position + w.zAxis * directionMagnitude);
            i++;
        }
    }
}
