using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRButton : XRSimpleInteractable
{
    [SerializeField] private Vector3 startLocalPosition;
    [SerializeField] private Vector3 endLocalPosition;
    [SerializeField] private float moveSpeed = 5f;
    private Coroutine moveCoroutine;

    [SerializeField] private Vector3 startLocalEulers;
    [SerializeField] private Vector3 endLocalEulers;
    [SerializeField] private float rotateSpeed = 5f;
    private Coroutine rotateCoroutine;

    private bool pressed;

    private IEnumerator MoveTowards(Vector3 targetPosition)
    {
        while ((transform.localPosition - targetPosition).sqrMagnitude > 0.1f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = targetPosition;
    }

    private IEnumerator RotateTowards(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotateSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        transform.localRotation = targetRotation;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if (!pressed)
        {
            pressed = true;
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            if (rotateCoroutine != null)
                StopCoroutine(rotateCoroutine);
            moveCoroutine = StartCoroutine(MoveTowards(startLocalPosition));
            rotateCoroutine = StartCoroutine(RotateTowards(Quaternion.Euler(startLocalEulers)));
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        if (pressed)
        {
            pressed = false;
            if(moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            if (rotateCoroutine != null)
                StopCoroutine(rotateCoroutine);
            moveCoroutine = StartCoroutine(MoveTowards(endLocalPosition));
            rotateCoroutine = StartCoroutine(RotateTowards(Quaternion.Euler(endLocalEulers)));
        }
    }
}
