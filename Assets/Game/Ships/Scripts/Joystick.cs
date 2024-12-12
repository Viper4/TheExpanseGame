using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class Joystick : GrabTrigger
{
    [SerializeField] InputActionReference[] triggerAction;
    [SerializeField] InputActionReference[] selectionPosAction;
    [SerializeField] InputActionReference[] selectionClickAction;

    [SerializeField] Transform joystick;
    [SerializeField] float maxAngle = 35;
    [SerializeField] Transform trigger;
    bool triggerDown;
    [SerializeField] float triggerDistance = 0.015f;
    float triggerStartPos;
    [SerializeField] float triggerAngle = 8;
    [SerializeField] Transform releaseButton;
    Animation releaseAnimation;
    [SerializeField] Transform lockButton;
    Animation lockAnimation;
    [SerializeField] Transform toggleSwitch;
    bool toggle;
    Vector3 toggleStartEulers;
    [SerializeField] Vector3 toggleEndEulers;

    public Vector3 direction { get; private set; }

    public UnityEvent OnTriggerDown;
    public UnityEvent OnTriggerUp;
    public UnityEvent OnToggleSwitch;
    public UnityEvent OnReleaseButton;
    public UnityEvent OnLockButton;

    void Start()
    {
        triggerStartPos = trigger.localPosition.z;
        toggleStartEulers = toggleSwitch.localEulerAngles;
        releaseAnimation = releaseButton.GetComponent<Animation>();
        lockAnimation = lockButton.GetComponent<Animation>();
    }

    void Update()
    {
        if (grabbingHand == null)
        {
            joystick.rotation = Quaternion.RotateTowards(joystick.rotation, transform.rotation, 5);
            if (triggerDown)
            {
                triggerDown = false;
                TriggerUp();
            }
            trigger.localPosition = new Vector3(trigger.localPosition.x, trigger.localPosition.y, triggerStartPos);
            trigger.localEulerAngles = new Vector3(0, trigger.localEulerAngles.y, trigger.localEulerAngles.z);
            direction = Vector3.zero;
        }
        else
        {
            joystick.rotation = grabbingHand.rotation;
            Vector3 fixedEulers = joystick.localEulerAngles.FixEulers();
            joystick.localEulerAngles = CustomMethods.Clamp(fixedEulers, -maxAngle, maxAngle);
            Vector3 fixedClampedEulers = joystick.localEulerAngles.FixEulers();
            direction = new Vector3(fixedClampedEulers.x / maxAngle, fixedClampedEulers.y / maxAngle, fixedClampedEulers.z / maxAngle);

            float triggerAxis = triggerAction[handIndex].action.ReadValue<float>();
            trigger.localPosition = new Vector3(trigger.localPosition.x, trigger.localPosition.y, triggerStartPos - triggerDistance * triggerAxis);
            trigger.localEulerAngles = new Vector3(triggerAngle * triggerAxis, trigger.localEulerAngles.y, trigger.localEulerAngles.z);
            if (triggerAction[handIndex].action.WasPressedThisFrame())
            {
                TriggerDown();
            }
            else if (triggerAction[handIndex].action.WasReleasedThisFrame())
            {
                TriggerUp();
            }

            Vector2 touchpadPosition = selectionPosAction[handIndex].action.ReadValue<Vector2>();
            if (selectionClickAction[handIndex].action.WasPressedThisFrame())
            {
                if(touchpadPosition.x < 0) // Left side
                {
                    if (touchpadPosition.y < 0) // Bottom
                    {
                        ReleaseButton();
                    }
                    else if (touchpadPosition.y > 0) // Top
                    {
                        ToggleSwitch();
                    }
                }
                else if(touchpadPosition.x > 0) // Right side
                {
                    if (touchpadPosition.y < 0) // Bottom
                    {
                        LockButton();
                    }
                    else if (touchpadPosition.y > 0) // Top
                    {
                        
                    }
                }
            }
        }
    }

    public void TriggerDown()
    {
        triggerDown = true;
        OnTriggerDown?.Invoke();
    }

    public void TriggerUp()
    {
        triggerDown = false;
        OnTriggerUp?.Invoke();
    }

    public void ReleaseButton()
    {
        releaseAnimation.Play();
        OnReleaseButton?.Invoke();
    }

    public void LockButton()
    {
        lockAnimation.Play();
        OnLockButton?.Invoke();
    }

    public void ToggleSwitch()
    {
        if (toggle)
        {
            toggle = false;
            toggleSwitch.localEulerAngles = toggleStartEulers;
        }
        else
        {
            toggle = true;
            toggleSwitch.localEulerAngles = toggleEndEulers;
        }
        OnToggleSwitch?.Invoke();
    }
}
