using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;
using System;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.UI;

public class Radar : MonoBehaviour
{
    private Rigidbody attachedRB;
    private PhysicsHandler attachedPhysicsHandler;
    [SerializeField] private HUDSystem _HUDSystem;

    public bool active = true;
    [SerializeField] private float[] radarRanges;
    [SerializeField] private int rangeIndex = 0;
    private List<Collider> collidersInRange = new List<Collider>();
    [SerializeField] private SphereCollider triggerCollider;

    [SerializeField] private GameObject iconParent;
    [SerializeField] private Transform hologram;

    [SerializeField] private GameObject shipIcon;
    [SerializeField] private Color friendlyShipColor;
    [SerializeField] private Color friendlyShipEmission;
    [SerializeField] private Color hostileShipColor;
    [SerializeField] private Color hostileShipEmission;

    [SerializeField] private GameObject pointIcon;
    [SerializeField] private Color projectileColor;
    [SerializeField] private Color projectileEmission;
    [SerializeField] private Color celestialBodyColor;
    [SerializeField] private Color celestialBodyEmission;

    [SerializeField] private Transform displayPulse;
    [SerializeField] private float pulseSpeed = 0.5f;

    private bool hologramActive = false;

    public Dictionary<int, RadarIcon> instanceIDIconPair = new Dictionary<int, RadarIcon>();

    private Dictionary<int, Vector3> lastVelocities = new Dictionary<int, Vector3>();

    private void Start()
    {
        attachedRB = GetComponentInParent<Rigidbody>();
        attachedPhysicsHandler = GetComponentInParent<PhysicsHandler>();
        if(attachedRB != null)
        {
            collidersInRange.Add(attachedRB.GetComponent<Collider>());
        }
        else if(attachedPhysicsHandler != null)
        {
            collidersInRange.Add(attachedPhysicsHandler.GetComponent<Collider>());
        }
    }

    private void Update()
    {
        if(active && hologramActive)
        {
            float pulseAddition = Time.deltaTime * pulseSpeed;
            displayPulse.localScale = CustomMethods.WrapClamp(new Vector3(displayPulse.localScale.x + pulseAddition, displayPulse.localScale.y + pulseAddition, displayPulse.localScale.z + pulseAddition), 0, 1);
        }
    }

    private void FixedUpdate()
    {
        if(active)
        {
            for (int i = collidersInRange.Count - 1; i >= 0; i--)
            {
                Collider collider = collidersInRange[i];
                if(collider == null)
                {
                    collidersInRange.RemoveAt(i);
                    continue;
                }
                if(collider.attachedRigidbody != attachedRB)
                {
                    if(collider.transform.HasTag("RadarTarget"))
                    {
                        Vector3 direction;
                        float distance;
                        int instanceID = collider.transform.GetInstanceID();
                        if (collider.TryGetComponent<ScaledTransform>(out var scaledTransform))
                        {
                            distance = Vector3.Distance(transform.position, scaledTransform.position.ToVector3());
                            direction = (scaledTransform.position.ToVector3() - transform.position).normalized;
                        }
                        else
                        {
                            distance = Vector3.Distance(transform.position, collider.transform.position);
                            direction = (collider.transform.position - transform.position).normalized;
                        }

                        Vector3 colliderVelocity = Vector3.zero;
                        Vector3 colliderAcceleration = Vector3.zero;
                        if (collider.TryGetComponent<PhysicsHandler>(out var physicsHandler))
                        {
                            if (!attachedPhysicsHandler.isKinematic)
                            {
                                colliderVelocity = physicsHandler.velocity.ToVector3();
                            }
                        }
                        else if (collider.TryGetComponent<Rigidbody>(out var colliderRigidbody) && !colliderRigidbody.isKinematic)
                        {
                            colliderVelocity = colliderRigidbody.velocity;
                        }
                        if(lastVelocities.TryGetValue(instanceID, out var lastVelocity))
                        {
                            colliderAcceleration = colliderVelocity - lastVelocity;
                            lastVelocities[instanceID] = colliderVelocity;
                        }
                        else
                        {
                            lastVelocities.Add(instanceID, colliderVelocity);
                        }

                        Vector3 relativeVelocity = colliderVelocity;
                        Vector3 relativeAcceleration = colliderAcceleration;

                        int transformInstanceID = transform.GetInstanceID();
                        Vector3 velocity = Vector3.zero;
                        Vector3 acceleration = Vector3.zero;
                        if (attachedPhysicsHandler != null)
                        {
                            if (!attachedPhysicsHandler.isKinematic)
                            {
                                velocity = attachedPhysicsHandler.velocity.ToVector3();
                            }
                        }
                        else if (attachedRB != null)
                        {
                            velocity = attachedRB.velocity;
                        }
                        if (lastVelocities.TryGetValue(transformInstanceID, out var myLastVelocity))
                        {
                            acceleration = velocity - myLastVelocity;
                            lastVelocities[transformInstanceID] = velocity;
                        }
                        else
                        {
                            lastVelocities.Add(transformInstanceID, velocity);
                        }

                        relativeVelocity -= velocity;
                        relativeAcceleration -= acceleration;
                        float closingSpeed = Vector3.Dot(relativeVelocity, direction);
                        float closingAcceleration = Vector3.Dot(relativeAcceleration, direction);

                        float arrivalTime = CustomMethods.CalculateArrivalTime(closingAcceleration, closingSpeed, distance);

                        string ETA = arrivalTime < 0 ? "Never" : CustomMethods.SecondsToFormattedString(arrivalTime, 2);
                        string details = "Distance: " + CustomMethods.DistanceToFormattedString(distance, 2) +
                            "\nSpeed: " + CustomMethods.SpeedToFormattedString(colliderVelocity.magnitude, 2) +
                            "\nClosing Speed: " + CustomMethods.SpeedToFormattedString(closingSpeed, 2) +
                            "\nETA: " + ETA;

                        if (!_HUDSystem.UpdateObject(instanceID, collider.transform, collider.transform.name, details))
                        {
                            HUDObject newHUDObject = _HUDSystem.CreateObject(instanceID, collider.transform, collider.transform.name, details);
                            switch (collider.transform.tag)
                            {
                                case "Ship":
                                    newHUDObject.SetColor(friendlyShipColor);
                                    break;
                                case "Projectile":
                                    newHUDObject.SetColor(projectileColor);
                                    break;
                                case "CelestialBody":
                                    newHUDObject.SetColor(celestialBodyColor);
                                    break;
                            }
                        }
                            
                        if(hologramActive)
                        {
                            // Display on radar hologram
                            Vector3 radarPosition = hologram.position + (direction * (float)(distance / radarRanges[rangeIndex] * 0.5f));
                            if (instanceIDIconPair.TryGetValue(instanceID, out RadarIcon icon))
                            {
                                icon.UpdateIcon(radarPosition, collider.transform.rotation, collider.transform.name + "\n" + CustomMethods.DistanceToFormattedString(distance, 2));
                            }
                            else
                            {
                                RadarIcon newIcon;
                                Color iconColor = friendlyShipColor;
                                Color iconEmission = friendlyShipEmission;
                                switch (collider.transform.tag)
                                {
                                    case "Ship":
                                        newIcon = Instantiate(shipIcon, iconParent.transform).GetComponent<RadarIcon>();
                                        iconColor = friendlyShipColor;
                                        iconEmission = friendlyShipEmission;
                                        break;
                                    case "Projectile":
                                        newIcon = Instantiate(pointIcon, iconParent.transform).GetComponent<RadarIcon>();
                                        iconColor = projectileColor;
                                        iconEmission = projectileEmission;
                                        break;
                                    case "CelestialBody":
                                        newIcon = Instantiate(pointIcon, iconParent.transform).GetComponent<RadarIcon>();
                                        float iconRadius = (float)scaledTransform.scale.x / radarRanges[rangeIndex];
                                        if (iconRadius < 0.01f)
                                            iconRadius = 0.01f;
                                        newIcon.model.localScale = new Vector3(iconRadius, iconRadius, iconRadius);
                                        iconColor = celestialBodyColor;
                                        iconEmission = celestialBodyEmission;
                                        break;
                                    default:
                                        newIcon = Instantiate(pointIcon, iconParent.transform).GetComponent<RadarIcon>();
                                        break;
                                }

                                newIcon.Init(this, instanceID, radarPosition, collider.transform.rotation, iconColor, iconEmission, collider.transform.name + "\n" + CustomMethods.DistanceToFormattedString(distance, 2));
                                instanceIDIconPair.Add(instanceID, newIcon);
                            }
                        }
                        
                    }
                }
                else if (hologramActive)
                {
                    int originID = transform.GetInstanceID();
                    if (instanceIDIconPair.TryGetValue(originID, out RadarIcon icon))
                    {
                        icon.UpdateIcon(hologram.position, ((attachedPhysicsHandler.velocity.ToVector3() / radarRanges[rangeIndex]) * Time.fixedDeltaTime), "You");
                    }
                    else
                    {
                        RadarIcon newIcon = Instantiate(shipIcon, iconParent.transform).GetComponent<RadarIcon>();
                        newIcon.Init(this, originID, hologram.position, transform.rotation, friendlyShipColor, friendlyShipEmission, "You");
                        instanceIDIconPair.Add(originID, newIcon);
                    }
                }
            }
        }
    }

    public void ToggleHologram(int state)
    {
        hologramActive = state == 1;
        if (active)
        {
            iconParent.SetActive(hologramActive);
        }
    }

    public void ToggleScale(int state)
    {
        rangeIndex = state;
        switch (state)
        {
            case 0:
                iconParent.SetActive(false);
                active = false;
                break;
            case 1:
                iconParent.SetActive(hologramActive);
                active = true;
                break;
        }
        triggerCollider.radius = radarRanges[rangeIndex];
    }

    public void RemoveIcon(int ID)
    {
        instanceIDIconPair.Remove(ID);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && other.transform.HasTag("RadarTarget"))
        {
            collidersInRange.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger && other.transform.HasTag("RadarTarget"))
        {
            collidersInRange.Remove(other);
        }
    }
}
