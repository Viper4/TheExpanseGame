using SpaceStuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GrabTrigger : MonoBehaviour
{
    [SerializeField] private MeshRenderer highlight;
    protected Transform grabbingHand;
    private Transform handInTrigger;
    protected int handIndex = -1; // 0 = left, 1 = right
    [SerializeField] private InputActionReference[] grabAction;
    [SerializeField] private bool toggleGrab = true;

    void Update()
    {
        if (toggleGrab)
        {
            if (grabbingHand == null)
            {
                if (handIndex != -1 && grabAction[handIndex].action.WasPressedThisFrame())
                {
                    grabbingHand = handInTrigger;
                }
            }
            else
            {
                if (grabAction[handIndex].action.WasPressedThisFrame())
                {
                    grabbingHand = null;
                    handInTrigger = null;
                    handIndex = -1;
                }
            }
        }
        else
        {
            if(handIndex != -1 && grabAction[handIndex].action.IsPressed())
            {
                grabbingHand = handInTrigger;
            }
            else
            {
                grabbingHand = null;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (handIndex == -1 && grabbingHand == null && other.transform.HasTag("Hand"))
        {
            handIndex = other.GetComponent<XRInteractionGroup>().groupName == "Left" ? handIndex = 0 : handIndex = 1;
            highlight.enabled = true;
            handInTrigger = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.HasTag("Hand"))
        {
            if (handInTrigger == other.transform)
            {
                handIndex = -1;
                highlight.enabled = false;
                handInTrigger = null;
            }
        }
    }
}
