using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRControl : MonoBehaviour
{
    public static VRControl instance;

    // General use
    private bool paused;
    public bool Paused
    {
        get
        {
            return paused;
        }
        set
        {
            paused = value;
            if (movementEnabled)
            {
                movement.SetActive(!paused);
                teleporting.SetActive(!paused);
            }
            if (snapTurnEnabled)
                snapTurn.SetActive(!paused);
        }
    }
    public PlayerInputActions inputActions;

    bool movementEnabled = false;
    public bool enableMovement
    {
        get
        {
            return movementEnabled;
        }
        set
        {
            movementEnabled = value;

            movement.SetActive(value);
            teleporting.SetActive(value);
        }
    }

    bool snapTurnEnabled = false;
    public bool enableSnapTurn
    {
        get
        {
            return snapTurnEnabled;
        }
        set
        {
            snapTurnEnabled = value;

            snapTurn.SetActive(value);
        }
    }

    [SerializeField] private GameObject movement;
    [SerializeField] private GameObject teleporting;
    [SerializeField] private GameObject snapTurn;

    private void OnEnable()
    {
        if(instance == null)
        {
            instance = this;
            inputActions = new PlayerInputActions();
            inputActions.Enable();
        }
        else
        {
            Debug.LogWarning("There is more than one VRControl in the scene. Destroying.");
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
}
