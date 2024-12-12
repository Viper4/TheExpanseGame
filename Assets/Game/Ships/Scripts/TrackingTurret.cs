using SpaceStuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class TrackingTurret : Turret
{
    [SerializeField, Header("Tracking Turret")] private string[] targetTags;
    [SerializeField] private string torpedoTag;
    private HashSet<string> targetTagsSet = new HashSet<string>();

    private List<Rigidbody> targetsInRange = new List<Rigidbody>();

    public float torpedoRange = 20f;

    private Rigidbody previousTarget = null;

    private Vector3 futureTargetPosition;

    protected override void Start()
    {
        base.Start();
        foreach(string tag in targetTags)
        {
            targetTagsSet.Add(tag);
        }
    }

    private void SelectTarget()
    {
        Rigidbody bestTarget = previousTarget;
        float fastestClosingSpeed = float.MaxValue;
        for (int i = 0; i < targetsInRange.Count; i++)
        {
            if (targetsInRange[i] == null)
            {
                targetsInRange.RemoveAt(i);
                i--;
                continue;
            }

            Vector3 direction = (targetsInRange[i].position - rotatingObject.position).normalized;
            if (Physics.Raycast(rotatingObject.position, direction, out RaycastHit hit, 10f, ~ignoreLayers) && hit.rigidbody != targetsInRange[i])
                continue; // Obstructed view of target

            Vector3 relativeVelocity = targetsInRange[i].velocity - turretSystem.ship.physicsHandler.velocity.ToVector3();

            float closingSpeed = Vector3.Dot(relativeVelocity, direction); // Negative value means target is moving towards us
            if (turretSystem.targets.TryGetValue(targetsInRange[i], out TurretSystem.TargetInfo targetInfo))
            {
                targetInfo.relativeVelocity = relativeVelocity;
                targetInfo.relativeAcceleration = turretSystem.targets[targetsInRange[i]].acceleration - turretSystem.ship.acceleration;
                targetInfo.closingSpeed = closingSpeed;
                targetInfo.closingAcceleration = Vector3.Dot(targetInfo.relativeAcceleration, direction);
                turretSystem.targets[targetsInRange[i]] = targetInfo;
            }
            if (closingSpeed > projectileSpeed)
                continue; // Target is moving away too fast

            float sqrDst = (targetsInRange[i].position - rotatingObject.position).sqrMagnitude;
            if (closingSpeed < fastestClosingSpeed && (!targetsInRange[i].transform.HasTag(torpedoTag) || sqrDst > torpedoRange * torpedoRange))
            {
                bestTarget = targetsInRange[i];
                fastestClosingSpeed = closingSpeed;
            }
        }
        target = bestTarget;
        if (target != null)
        {
            if (previousTarget != null && previousTarget != target)
            {
                TurretSystem.TargetInfo prevTargetInfo = turretSystem.targets[previousTarget];
                prevTargetInfo.turretsTargeting--;
                turretSystem.targets[previousTarget] = prevTargetInfo;
            }
            if (turretSystem.targets.TryGetValue(target, out TurretSystem.TargetInfo targetInfo))
            {
                if (previousTarget != target)
                    targetInfo.turretsTargeting++;
                targetInfo.distance = Vector3.Distance(target.position, rotatingObject.position);
                turretSystem.targets[target] = targetInfo;
            }
            else
            {
                turretSystem.targets.Add(target, new TurretSystem.TargetInfo() { turretsTargeting = 1, distance = Vector3.Distance(target.position, rotatingObject.position) });
            }

            previousTarget = target;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (active)
        {
            if (!turretSystem.manualControl)
            {
                fire = false;
                if (targetsInRange.Count > 0)
                {
                    SelectTarget();

                    if (target == null)
                        return;

                    // Calculate future position of target
                    Vector3 relativePosition = target.position - origin.position;
                    float predictionTime = turretSystem.targets[target].distance / projectileSpeed;
                    for (int i = 0; i < 5; i++)
                    {
                        Vector3 predictedRelativePosition = relativePosition + (turretSystem.targets[target].relativeVelocity * predictionTime) + (0.5f * predictionTime * predictionTime * turretSystem.targets[target].relativeAcceleration);
                        predictionTime = predictedRelativePosition.magnitude / projectileSpeed;
                    }

                    futureTargetPosition = target.position + (turretSystem.targets[target].relativeVelocity * predictionTime) + (0.5f * predictionTime * predictionTime * turretSystem.targets[target].relativeAcceleration);

                    aimDirection = (futureTargetPosition - origin.position).normalized;

                    if (!Physics.Raycast(firePoint.position, aimDirection, out RaycastHit hit, turretSystem.targets[target].distance - 0.1f, ~ignoreLayers) || hit.rigidbody == target)
                        fire = true;

                    if (showLines)
                    {
                        Debug.DrawLine(firePoint.position, futureTargetPosition, Color.green, Time.fixedDeltaTime);
                    }
                }                
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && other.attachedRigidbody != turretSystem.ship.physicsHandler.attachedRigidbody)
        {
            if (targetTagsSet.Contains(other.transform.tag))
            {
                targetsInRange.Add(other.attachedRigidbody);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger && targetTagsSet.Contains(other.transform.tag))
        {
            targetsInRange.Remove(other.attachedRigidbody);
            if(other.attachedRigidbody == target)
            {
                target = null;
                if (turretSystem.targets.TryGetValue(other.attachedRigidbody, out TurretSystem.TargetInfo targetInfo))
                {
                    targetInfo.turretsTargeting--;
                    turretSystem.targets[other.attachedRigidbody] = targetInfo;
                }
            }
        }
    }
}
