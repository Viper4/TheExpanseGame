using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Switch : MonoBehaviour
{
    MeshRenderer highlight;
    AudioSource audioSource;
    [SerializeField] InputActionReference leftToggleAction;
    [SerializeField] InputActionReference rightToggleAction;

    [SerializeField] Vector3[] eulerStates;
    [SerializeField] MeshRenderer indicator;
    [SerializeField] Color[] indicatorColors;
    [SerializeField] Color[] indicatorEmissionColors;
    public int currentState;

    public UnityEvent<int> OnSwitchToggle;
    private bool leftHandInTrigger;
    private bool rightHandInTrigger;

    [SerializeField] private bool nonVRToggle = false;

    private void Start()
    {
        highlight = transform.GetChild(0).GetComponent<MeshRenderer>();
        audioSource = GetComponent<AudioSource>();
        transform.eulerAngles = eulerStates[currentState];
        if (indicator != null)
        {
            indicator.material = Instantiate(indicator.sharedMaterial);
            indicator.material.SetColor("_MainColor", indicatorColors[currentState]);
            indicator.material.SetColor("_EmissionColor", indicatorEmissionColors[currentState]);
        }
    }

    private void Update()
    {
        if (nonVRToggle)
        {
            Toggle();
            nonVRToggle = false;
        }
        if (leftHandInTrigger)
        {
            if (leftToggleAction.action.WasPressedThisFrame())
            {
                Toggle();
            }
        }
        if (rightHandInTrigger)
        {
            if (rightToggleAction.action.WasPressedThisFrame())
            {
                Toggle();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.HasTag("Hand"))
        {
            string groupName = other.GetComponent<XRInteractionGroup>().groupName;
            if(groupName == "Left")
            {
                leftHandInTrigger = true;
            }
            else
            {
                rightHandInTrigger = true;
            }
            Hover(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.HasTag("Hand"))
        {
            string groupName = other.GetComponent<XRInteractionGroup>().groupName;
            if(groupName == "Left")
            {
                leftHandInTrigger = false;
            }
            else
            {
                rightHandInTrigger = false;
            }
            if(!leftHandInTrigger && !rightHandInTrigger)
                Hover(false);
        }
    }

    public void Hover(bool value)
    {
        highlight.enabled = value;
    }

    public void Toggle()
    {
        currentState++;
        if (currentState > eulerStates.Length - 1)
            currentState = 0;
        transform.localEulerAngles = eulerStates[currentState];
        if(indicator != null)
        {
            indicator.material.SetColor("_MainColor", indicatorColors[currentState]);
            indicator.material.SetColor("_EmissionColor", indicatorEmissionColors[currentState]);
        }

        OnSwitchToggle?.Invoke(currentState);
        if (audioSource != null)
            audioSource.Play();
        highlight.enabled = false;
    }
}
