using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private Transform[] cameraPoints;
    private Camera[] cameras;
    private AxisTransform[] axisTransforms;

    [SerializeField] private Transform buttonParent;
    [SerializeField] private GameObject cameraFeedPanel;

    [SerializeField] private GameObject buttonPrefab;

    private int selectedCamera;

    private void AddButtonListener(Button button, int index)
    {
        button.onClick.AddListener(() => SelectCamera(index));
    }

    void Start()
    {
        cameras = new Camera[cameraPoints.Length];
        axisTransforms = new AxisTransform[cameraPoints.Length];
        for (int i = 0; i < cameraPoints.Length; i++)
        {
            cameras[i] = cameraPoints[i].GetChild(0).GetComponent<Camera>();
            axisTransforms[i] = cameras[i].GetComponent<AxisTransform>();
            GameObject cameraButton = Instantiate(buttonPrefab, buttonParent);
            cameraButton.name = "Camera Button " + i;
            AddButtonListener(cameraButton.GetComponent<Button>(), i); // Do this so the event doesn't just reference int i and instead creates a new integer
            cameraButton.transform.Find("Button Front").Find("Text").GetComponent<TextMeshProUGUI>().text = "CAM" + (i + 1);
            cameras[i].gameObject.SetActive(false);
        }
    }

    public void ToggleCameraRotation(int axis)
    {
        if(selectedCamera >= 0)
        {
            if (axisTransforms[selectedCamera].rotationAxisIndex == axis)
            {
                axisTransforms[selectedCamera].StopRotating();
            }
            else
            {
                axisTransforms[selectedCamera].StartRotating(axis);
            }
        }
    }

    public void SelectCamera(int i)
    {
        DisableCameras();
        selectedCamera = i;
        buttonParent.parent.gameObject.SetActive(false);
        cameras[i].gameObject.SetActive(true);
        cameraFeedPanel.gameObject.SetActive(true);
    }

    public void DisableCameras()
    {
        selectedCamera = -1;
        for (int j = 0; j < cameras.Length; j++)
        {
            cameras[j].gameObject.SetActive(false);
        }
    }
}
