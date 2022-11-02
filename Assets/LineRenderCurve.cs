using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRenderCurve : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public LineRenderer value;
    public AnimationCurve curve;
    public int resolution = 20;
    

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer.positionCount = resolution;
        for (int i = 0; i < resolution; i++){
            float x = (1f / resolution) * i;
            float y = curve.Evaluate(x);
            lineRenderer.SetPosition(i,new Vector3(x,y,0f));
        }
    }

    public void SetValue(float newValue){
        value.positionCount = 2;
        float x = newValue;
        float y = curve.Evaluate(x);
        value.SetPosition(0, new Vector3(x, 0f, 0f));
        value.SetPosition(1, new Vector3(x,y,0f));
    }
}
