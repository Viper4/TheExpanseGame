using SpaceStuff;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Ship))]
public class TurretSystem : MonoBehaviour
{
    public struct TargetInfo
    {
        public int turretsTargeting;
        public float distance;
        public Vector3 lastVelocity;
        public Vector3 acceleration;
        public Vector3 relativeVelocity;
        public Vector3 relativeAcceleration;
        public float closingSpeed;
        public float closingAcceleration;
        public Color originalHUDColor;
        public Color originalRadarColor;
        public Color originalRadarEmission;
    }

    [HideInInspector] public Ship ship;
    [SerializeField] private HUDSystem _HUDSystem;
    [SerializeField] private Radar radar;

    [SerializeField] private Transform[] turretPoints;
    [HideInInspector] public Turret[] turrets;
    [SerializeField] private GameObject turretCrosshair;

    public bool manualControl = false;

    [SerializeField] private Color crosshairNormalColor;
    [SerializeField] private Color crosshairHoverColor;
    [SerializeField] private Color crosshairTriggerColor;
    private bool triggerHeld = false;
    [SerializeField] private Color turretNormalColor;
    [SerializeField] private Color turretBlockedColor;

    public Dictionary<Rigidbody, TargetInfo> targets = new Dictionary<Rigidbody, TargetInfo>();
    [SerializeField] private Color targetingHUDColor;

    private void Start()
    {
        ship = GetComponent<Ship>();

        turrets = new Turret[turretPoints.Length];
        for (int i = 0; i < turretPoints.Length; i++)
        {
            turrets[i] = turretPoints[i].GetChild(0).GetComponent<Turret>();
            Instantiate(turretCrosshair, _HUDSystem.combatPanel);
        }
    }

    private void FixedUpdate()
    {
        if (manualControl)
        {
            //mainCrosshair.transform.localPosition = new Vector2(-joystick.direction.z * ship.combatPanel.rect.width * 0.5f, -joystick.direction.x * ship.combatPanel.rect.height * 0.5f);
            Vector3 crosshairDirection = _HUDSystem.mainCrosshair.transform.position - ship.pilotCamera.transform.position;

            if (Physics.Raycast(_HUDSystem.mainCrosshair.transform.position, crosshairDirection, out RaycastHit hit, Mathf.Infinity, ~ship.ignoreLayers))
            {
                if (!triggerHeld)
                    _HUDSystem.mainCrosshair.color = crosshairHoverColor;
            }
            else
            {
                if (!triggerHeld)
                    _HUDSystem.mainCrosshair.color = crosshairNormalColor;
            }

            for (int i = 0; i < turrets.Length; i++)
            {
                Turret turret = turrets[i];
                turret.aimDirection = hit.point - turret.firePoint.position;
                Transform turretCrosshair = _HUDSystem.combatPanel.GetChild(i + 1);
                Vector3 turretHitPoint;
                if (turret.GetObstruction(out RaycastHit turretHit))
                {
                    if (turretHit.transform != transform)
                        turretCrosshair.GetComponent<Image>().color = turretNormalColor;
                    else
                        turretCrosshair.GetComponent<Image>().color = turretBlockedColor;
                    turretHitPoint = turretHit.point;
                }
                else
                {
                    turretCrosshair.GetComponent<Image>().color = turretNormalColor;
                    turretHitPoint = turret.firePoint.position + turret.firePoint.forward * 1000;
                }

                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_HUDSystem.combatPanel, ship.pilotCamera.WorldToScreenPoint(turretHitPoint), ship.pilotCamera, out Vector3 turretCrosshairPos))
                {
                    turretCrosshair.position = turretCrosshairPos;
                }
            }
        }
        else
        {
            foreach(Rigidbody target in targets.Keys.ToList())
            {
                TargetInfo targetInfo = targets[target];
                if (target == null || targetInfo.turretsTargeting <= 0)
                {
                    targets.Remove(target);
                    if(target != null)
                    {
                        int targetID = target.transform.GetInstanceID();
                        if (_HUDSystem.TryGetValue(targetID, out HUDObject targetHUDObject))
                        {
                            targetHUDObject.SetTargetText("");
                            targetHUDObject.SetColor(targetInfo.originalHUDColor);
                        }
                        if (radar != null && radar.instanceIDIconPair.TryGetValue(targetID, out RadarIcon targetIcon))
                        {
                            targetIcon.SetColor(targetInfo.originalRadarColor, targetInfo.originalRadarEmission);
                        }
                    }
                    continue;
                }

                int instanceID = target.transform.GetInstanceID();

                targetInfo.acceleration = target.velocity - targetInfo.lastVelocity;
                targetInfo.lastVelocity = target.velocity;
                targetInfo.distance = (target.transform.position - transform.position).magnitude;
                float arrivalTime = CustomMethods.CalculateArrivalTime(targetInfo.closingAcceleration, targetInfo.closingSpeed, targetInfo.distance);
                string ETA = arrivalTime < 0 ? "Never" : CustomMethods.SecondsToFormattedString(arrivalTime, 2);
                string details = "Distance: " + CustomMethods.DistanceToFormattedString(targetInfo.distance, 2) +
                    "\nSpeed: " + CustomMethods.SpeedToFormattedString(target.velocity.magnitude, 2) +
                    "\nClosing Speed: " + CustomMethods.SpeedToFormattedString(targetInfo.closingSpeed, 2) +
                    "\nETA: " + ETA;

                if (_HUDSystem.TryGetValue(instanceID, out HUDObject _targetHUDObject))
                {
                    _HUDSystem.UpdateObject(instanceID, target.transform, target.name, details);
                    _targetHUDObject.SetTargetText(targetInfo.turretsTargeting.ToString());
                    if(targetInfo.originalHUDColor == Color.clear)
                        targetInfo.originalHUDColor = _targetHUDObject.GetColor();

                    _targetHUDObject.SetColor(targetingHUDColor);
                }
                else
                {
                    HUDObject newHUDObject = _HUDSystem.CreateObject(instanceID, target.transform, target.name, details);
                    newHUDObject.SetTargetText(targetInfo.turretsTargeting.ToString());
                    if (targetInfo.originalHUDColor == Color.clear)
                        targetInfo.originalHUDColor = newHUDObject.GetColor();
                    newHUDObject.SetColor(targetingHUDColor);
                }

                if(radar != null && radar.instanceIDIconPair.TryGetValue(instanceID, out RadarIcon icon))
                {
                    if (targetInfo.originalRadarColor == Color.clear)
                    {
                        targetInfo.originalRadarColor = icon.GetColor();
                        targetInfo.originalRadarEmission = icon.GetEmission();
                    }
                    icon.SetColor(targetingHUDColor, targetingHUDColor);
                }

                targets[target] = targetInfo;
            }
        }
    }

    public void TriggerStart()
    {
        if (manualControl)
        {
            triggerHeld = true;
            _HUDSystem.mainCrosshair.color = crosshairTriggerColor;
            for (int i = 0; i < turrets.Length; i++)
            {
                Turret turret = turrets[i];
                if (!turret.GetObstruction(out RaycastHit turretHit) || turretHit.transform != transform)
                {
                    turret.fire = true;
                }
            }
        }
    }

    public void TriggerEnd()
    {
        if (manualControl)
        {
            triggerHeld = false;
            for (int i = 0; i < turrets.Length; i++)
            {
                turrets[i].fire = false;
            }
        }
    }

    public void ToggleManualControl()
    {
        manualControl = !manualControl;
    }
}
