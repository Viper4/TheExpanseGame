using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;
using UnityEngine.InputSystem;

public class Throttle : GrabTrigger
{
    [SerializeField] InputActionReference[] lockThrottleAction;
    bool lockThrottle = false;

    [SerializeField] Transform throttle;
    [SerializeField] float[] angleRange;
    [SerializeField] float[] valueRange;

    [SerializeField] Transform lockButton;
    float lockStartPos;
    [SerializeField] float lockEndPos;

    public float value { get; private set; }

    void Start()
    {
        lockStartPos = lockButton.localPosition.x;
    }

    void LateUpdate()
    {
        if (grabbingHand != null)
        {
            throttle.rotation = grabbingHand.rotation;
            throttle.localEulerAngles = new Vector3(throttle.localEulerAngles.x, 0, 0);
            Vector3 fixedEulers = throttle.localEulerAngles.FixEulers();
            throttle.localEulerAngles = CustomMethods.Clamp(fixedEulers, angleRange[0], angleRange[1]);
            Vector3 fixedClampedEulers = throttle.localEulerAngles.FixEulers();

            value = CustomMethods.normalize(fixedClampedEulers.x, angleRange[0], angleRange[1]);

            if (lockThrottleAction[handIndex].action.WasPressedThisFrame())
            {
                if (lockThrottle)
                {
                    lockThrottle = false;
                    lockButton.localPosition = new Vector3(lockStartPos, lockButton.localPosition.y, lockButton.localPosition.z);
                }
                else
                {
                    lockThrottle = true;
                    lockButton.localPosition = new Vector3(lockEndPos, lockButton.localPosition.y, lockButton.localPosition.z);
                }
            }
        }
        else
        {
            if (!lockThrottle)
            {
                throttle.localEulerAngles = Vector3.RotateTowards(throttle.localEulerAngles.FixEulers(), new Vector3(angleRange[0], 0, 0), 2, 1);
                value = valueRange[0];
            }
        }
    }
}
