using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SpaceStuff;
using TMPro;

public class Gunship : Ship
{
    [SerializeField] private Joystick joystick;
    [SerializeField] private Throttle throttle;
    [SerializeField] private float rotationForce = 1000;
    [SerializeField] private float cruiseForce = 1000;
    [SerializeField] private float launchForce = 10000;
    [SerializeField] private float translationForce = 750;
    private bool launchMode = false;

    private bool autoStabilizeRot = false;
    private bool autoStabilizePos = false;

    private TurretSystem turretSystem;
    private TorpedoSystem torpedoSystem;

    private int translationMode = 1;

    [SerializeField, Range(-1000, 1000)] private float P, I, D;
    private PIDController rotationPID;
    private PIDController translationPID;

    [SerializeField] private Material modelMaterial;
    [SerializeField] private Transform targetModelParent;
    [SerializeField] private TextMeshProUGUI targetName;
    [SerializeField] private Transform targetDirectionPivot;
    private Transform targetModel;
    private Transform lockedTarget;

    private void Start()
    {
        turretSystem = GetComponent<TurretSystem>();
        torpedoSystem = GetComponent<TorpedoSystem>();

        rotationPID = new PIDController(P, I, D);
        translationPID = new PIDController(P, I, D);
    }

    private void FixedUpdate()
    {
        if (turretSystem.manualControl || torpedoSystem.torpedoBayDoorOpen)
        {
            if (!_HUDSystem.combatPanel.gameObject.activeSelf)
            {
                _HUDSystem.combatPanel.gameObject.SetActive(true);
            }
        }
        else
        {
            if (_HUDSystem.combatPanel.gameObject.activeSelf)
            {
                _HUDSystem.combatPanel.gameObject.SetActive(false);
            }
        }

        Vector3 joystickDirection = Vector3.zero;
        if (isPilot)
        {
            joystickDirection = joystick.direction;
            if (translationMode != 1)
            {
                if (joystickDirection.x != 0 && joystickDirection.z != 0)
                {
                    switch (translationMode)
                    {
                        case 0: // Vertical
                            physicsHandler.AddRelativeForce(new Vector3d(-joystickDirection.z, -joystickDirection.x, 0) * translationForce, ForceMode.Acceleration);
                            break;
                        case 2: // Horizontal
                            physicsHandler.AddRelativeForce(new Vector3d(-joystickDirection.z, 0, joystickDirection.x) * translationForce, ForceMode.Acceleration);
                            break;
                    }
                }
            }
            else
            {
                physicsHandler.AddRelativeTorque(joystickDirection.ToVector3d() * rotationForce, ForceMode.Acceleration);
            }

            if (lockedTarget != null)
            {
                targetName.text = lockedTarget.name;
                targetModel.rotation = lockedTarget.rotation;
                if (TryGetComponent<PhysicsHandler>(out var targetPhysicsHandler))
                {
                    if (targetPhysicsHandler.velocity != Vector3d.zero)
                        targetDirectionPivot.rotation = Quaternion.LookRotation(targetPhysicsHandler.velocity.ToVector3(), lockedTarget.up);
                }
                else if (TryGetComponent<Rigidbody>(out var targetRigidbody))
                {
                    if (targetRigidbody.velocity != Vector3.zero)
                        targetDirectionPivot.rotation = Quaternion.LookRotation(targetRigidbody.velocity, lockedTarget.up);
                }

                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_HUDSystem.combatPanel, pilotCamera.WorldToScreenPoint(lockedTarget.position), pilotCamera, out Vector3 crosshairPosition))
                {
                    _HUDSystem.mainCrosshair.transform.position = crosshairPosition;
                }
                _HUDSystem.mainCrosshair.transform.rotation = Quaternion.LookRotation(pilotCamera.transform.position - _HUDSystem.mainCrosshair.transform.position, FlatCamera.instance.transform.up);
            }
            else if (targetModel != null)
            {
                Destroy(targetModel.gameObject);
                targetName.text = "NO TARGET";
                targetModelParent.gameObject.SetActive(false);
            }
        }

        if (throttle.value > 0)
        {
            if (!rocketTrail.isPlaying)
            {
                rocketTrail.Play();
            }
        }
        else
        {
            if (rocketTrail.isPlaying)
            {
                rocketTrail.Stop();
            }
        }
        Vector3d force = launchMode ? launchForce * throttle.value * Vector3d.forward : cruiseForce * throttle.value * Vector3d.forward;
        physicsHandler.AddRelativeForce(force, ForceMode.Acceleration);

        if (autoStabilizeRot && (turretSystem.manualControl || joystickDirection == Vector3.zero))
        {
            float torqueCorrectionX = Mathf.Clamp(-rotationPID.GetOutput(physicsHandler.attachedRigidbody.angularVelocity.x, Time.fixedDeltaTime), -rotationForce, rotationForce);
            float torqueCorrectionY = Mathf.Clamp(-rotationPID.GetOutput(physicsHandler.attachedRigidbody.angularVelocity.y, Time.fixedDeltaTime), -rotationForce, rotationForce);
            float torqueCorrectionZ = Mathf.Clamp(-rotationPID.GetOutput(physicsHandler.attachedRigidbody.angularVelocity.z, Time.fixedDeltaTime), -rotationForce, rotationForce);
            physicsHandler.AddTorque(new Vector3d(torqueCorrectionX, torqueCorrectionY, torqueCorrectionZ) * rotationForce, ForceMode.Acceleration);
        }
        if (autoStabilizePos && throttle.value == 0 && (translationMode == 1 || joystickDirection == Vector3.zero))
        {
            float forceCorrectionX = Mathf.Clamp(-translationPID.GetOutput((float)physicsHandler.velocity.x, Time.fixedDeltaTime), -translationForce, translationForce);
            float forceCorrectionY = Mathf.Clamp(-translationPID.GetOutput((float)physicsHandler.velocity.y, Time.fixedDeltaTime), -translationForce, translationForce);
            float forceCorrectionZ = Mathf.Clamp(-translationPID.GetOutput((float)physicsHandler.velocity.z, Time.fixedDeltaTime), -translationForce, translationForce);
            physicsHandler.AddForce(new Vector3d(forceCorrectionX, forceCorrectionY, forceCorrectionZ) * translationForce, ForceMode.Acceleration);
        }
    }

    public void LockTarget()
    {
        if (targetModel != null)
            Destroy(targetModel.gameObject);
        targetName.text = "NO TARGET";
        if (_HUDSystem.combatPanel.gameObject.activeSelf)
        {
            Vector3 crosshairDirection = _HUDSystem.mainCrosshair.transform.position - pilotCamera.transform.position;
            if (Physics.Raycast(_HUDSystem.mainCrosshair.transform.position, crosshairDirection, out RaycastHit hit, Mathf.Infinity, ~ignoreLayers))
            {
                if (hit.transform.HasTag("RadarTarget"))
                {
                    lockedTarget = hit.transform;
                }
                Debug.DrawLine(_HUDSystem.mainCrosshair.transform.position, hit.point, Color.green, 0.1f);
            }
            targetModelParent.gameObject.SetActive(lockedTarget != null);
            if (lockedTarget != null)
            {
                _HUDSystem.mainCrosshair.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            if (lockedTarget == null)
                return;
            GameObject newTargetModel;
            if (lockedTarget.HasTag("Ship"))
            {
                newTargetModel = Instantiate(lockedTarget.GetComponent<Ship>().radarModel, targetModelParent, false);
            }
            else
            {
                newTargetModel = CustomMethods.GenerateModel(lockedTarget.gameObject, 2, modelMaterial, 1); // 2 is Ignore Raycast layer
                newTargetModel.transform.localScale = new Vector3(5, 5, 5);
            }

            newTargetModel.name = "Target Model";
            targetModel = newTargetModel.transform;
            targetModel.SetParent(targetModelParent);
            targetModel.localPosition = Vector3.zero;
        }
        else
        {
            lockedTarget = null;
            targetModelParent.gameObject.SetActive(false);
        }
    }

    public void LaunchSwitch(int state)
    {
        launchMode = state == 1;
    }

    public void StabilizeRotSwitch(int state)
    {
        autoStabilizeRot = state == 1;
    }

    public void StabilizePosSwitch(int state)
    {
        autoStabilizePos = state == 1;
    }

    public void TranslationControlSwitch(int state)
    {
        translationMode = state;
    }

    public void LaunchTorpedo()
    {
        if (lockedTarget != null)
            torpedoSystem.FireTorpedo(lockedTarget);
    }
}
