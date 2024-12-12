using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Android;
using static UnityEditor.FilePathAttribute;
using UnityEngine.UIElements;

public class RadarIcon : MonoBehaviour
{
    private Radar parentRadar;
    private int ID;

    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int maxLinePositions = 1000;
    [SerializeField] private float newLinePosThreshold;
    private float sqrNewLinePosThreshold;
    private List<Vector3> linePositions = new List<Vector3>();

    public Transform model;
    private MeshRenderer modelMeshRenderer;
    [SerializeField] private TextMeshPro text3D;

    [SerializeField] private float killTime = 0.5f;
    private float killTimer;

    private Color baseColor;

    void LateUpdate()
    {
        killTimer -= Time.deltaTime;
        if (killTimer <= 0)
        {
            parentRadar.RemoveIcon(ID);
            Destroy(gameObject);
        }
        text3D.transform.rotation = Quaternion.LookRotation(text3D.transform.position - FlatCamera.instance.transform.position, FlatCamera.instance.transform.up);
    }

    public void Init(Radar radar, int instanceID, Vector3 position, Quaternion rotation, Color baseColor, Color emission, string text)
    {
        this.baseColor = baseColor;
        modelMeshRenderer = model.GetComponent<MeshRenderer>();
        Material clonedMaterial = Instantiate(modelMeshRenderer.sharedMaterial);
        clonedMaterial.color = baseColor;
        clonedMaterial.SetColor("_EmissionColor", emission);
        modelMeshRenderer.sharedMaterial = clonedMaterial;
        if (lineRenderer != null)
        {
            lineRenderer.sharedMaterial = clonedMaterial;
        }

        killTimer = killTime;
        sqrNewLinePosThreshold = newLinePosThreshold * newLinePosThreshold;
        parentRadar = radar;
        ID = instanceID;
        UpdateIcon(position, rotation, text);
    }

    public void UpdateIcon(Vector3 position, Quaternion rotation, string text)
    {
        killTimer = killTime;

        transform.position = position;
        model.rotation = rotation;
        if (lineRenderer != null)
        {
            float sqrMagnitude = linePositions.Count > 0 ? (linePositions[^1] - transform.localPosition).sqrMagnitude : 999f;
            if (sqrMagnitude > sqrNewLinePosThreshold)
            {
                linePositions.Add(transform.localPosition);
                if (linePositions.Count > maxLinePositions)
                {
                    linePositions.RemoveAt(0);
                }
                lineRenderer.positionCount = linePositions.Count + 1;
            }
            for (int i = 0; i < linePositions.Count; i++)
            {
                lineRenderer.SetPosition(i, linePositions[i] - transform.localPosition);
            }
            lineRenderer.SetPosition(linePositions.Count, Vector3.zero);
        }

        if (text3D != null)
        {
            text3D.text = text;
            text3D.color = baseColor;
        }
    }

    public void UpdateIcon(Vector3 position, Vector3 displacement, string text)
    {
        killTimer = killTime;

        transform.position = position;
        if (lineRenderer != null)
        {
            // Move vertices back by displacement
            for (int i = 0; i < linePositions.Count; i++)
            {
                linePositions[i] += displacement;
            }
            float sqrMagnitude = linePositions.Count > 0 ? (linePositions[^1] - transform.localPosition).sqrMagnitude : 999f;
            if (sqrMagnitude > sqrNewLinePosThreshold)
            {
                linePositions.Add(transform.localPosition);
                if (linePositions.Count > maxLinePositions)
                {
                    linePositions.RemoveAt(0);
                }
                lineRenderer.positionCount = linePositions.Count + 1;
            }
            for (int i = 0; i < linePositions.Count; i++)
            {
                lineRenderer.SetPosition(i, linePositions[i] - transform.localPosition);
            }
            lineRenderer.SetPosition(linePositions.Count, Vector3.zero);
        }

        if (text3D != null)
        {
            text3D.text = text;
            text3D.color = baseColor;
        }
    }

    public Color GetColor()
    {
        return modelMeshRenderer.sharedMaterial.color;
    }

    public Color GetEmission()
    {
        return modelMeshRenderer.sharedMaterial.GetColor("_EmissionColor");
    }

    public void SetColor(Color mainColor, Color emissionColor)
    {
        modelMeshRenderer.sharedMaterial.color = mainColor;
        modelMeshRenderer.sharedMaterial.SetColor("_EmissionColor", emissionColor);
        if (lineRenderer != null)
        {
            lineRenderer.sharedMaterial.color = mainColor;
            lineRenderer.sharedMaterial.SetColor("_EmissionColor", emissionColor);
        }
    }
}
