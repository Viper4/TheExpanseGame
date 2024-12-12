using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;

public class AxisTransform : MonoBehaviour
{
    [SerializeField] private Vector3 maxLocalPosition;
    [SerializeField] private Vector3 minLocalPosition;
    [SerializeField] private Vector3[] movementAxes;
    [SerializeField] private float[] movementSpeeds;
    private int movementAxisIndex = -1;

    [SerializeField] private Vector3 maxLocalEulers;
    [SerializeField] private Vector3 minLocalEulers;
    [SerializeField] private Vector3[] rotationAxes;
    [SerializeField] private float[] rotationSpeeds;
    public int rotationAxisIndex = -1;

    private void Update()
    {
        if (movementAxisIndex >= 0)
        {
            transform.localPosition += movementSpeeds[movementAxisIndex] * Time.deltaTime * movementAxes[movementAxisIndex];
            transform.localPosition = CustomMethods.Clamp(transform.localPosition, minLocalEulers, maxLocalEulers);
        }

        if (rotationAxisIndex >= 0)
        {
            transform.localRotation *= Quaternion.AngleAxis(rotationSpeeds[rotationAxisIndex] * Time.deltaTime, rotationAxes[rotationAxisIndex]);
            transform.localEulerAngles = CustomMethods.Clamp(transform.localEulerAngles.FixEulers(), minLocalEulers, maxLocalEulers);
        }
    }

    public void StartMoving(int axisIndex)
    {
        if (axisIndex >= 0 && axisIndex < movementAxes.Length)
        {
            movementAxisIndex = axisIndex;
        }
        else
        {
            Debug.LogWarning($"{transform.name} SmoothMovement: {axisIndex} out of range of {movementAxes.Length} movementAxes.");
        }
    }

    public void StopMoving()
    {
        movementAxisIndex = -1;
    }

    public void StartRotating(int axisIndex)
    {
        if (axisIndex >= 0 && axisIndex < rotationAxes.Length)
        {
            rotationAxisIndex = axisIndex;
        }
        else
        {
            Debug.LogWarning($"{transform.name} SmoothMovement: {axisIndex} out of range of {rotationAxes.Length} rotationAxes.");
        }
    }

    public void StopRotating()
    {
        rotationAxisIndex = -1;
    }
}
