using TMPro;
using UnityEngine;

public class LineConnector : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    public Renderer pointARenderer;
    public Renderer pointBRenderer;

    public float thickness;
    public float strength;

    public Color pointAColor;
    public Color pointBColor;

    public GameObject value;

    private LineRenderer lineRenderer;
    private Material lineMaterial;

    public bool showValue;

    void Start()
    {
        lineRenderer = transform.GetComponentInChildren<LineRenderer>();
        lineMaterial = lineRenderer.material;

        lineRenderer.SetPosition(0, pointA.position);
        lineRenderer.SetPosition(1, pointB.position);

        pointARenderer = pointA.gameObject.GetComponentInChildren<Renderer>();
        pointBRenderer = pointB.gameObject.GetComponentInChildren<Renderer>();

        lineRenderer.widthCurve = new AnimationCurve(new Keyframe(0, thickness/100), new Keyframe(1, thickness / 100));
        lineMaterial.SetColor("_ColorA", pointAColor);
        lineMaterial.SetColor("_ColorB", pointBColor);

        if (showValue)
        {
            value.GetComponent<TextMeshPro>().text = thickness.ToString();
            value.transform.position = (pointA.position + pointB.position) / 2;
        }
        else
        {
            Destroy(value.GetComponent<TextMeshPro>());
        }
    }

    private void Update()
    {
        bool aEnabled = pointARenderer.enabled;
        bool bEnabled = pointBRenderer.enabled;

        lineRenderer.SetPosition(0, pointA.position);
        lineRenderer.SetPosition(1, pointB.position);

        lineRenderer.enabled = aEnabled || bEnabled;
        value.SetActive(aEnabled || bEnabled);

        if (!aEnabled && !bEnabled) return;
        lineMaterial.SetInt("_isPartial", aEnabled && bEnabled ? 0 : 1);
        lineMaterial.SetInt("_isStart", aEnabled && !bEnabled ? 1 : 0);
    }
}