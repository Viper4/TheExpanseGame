using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using SpaceStuff;

public class Torpedo : MonoBehaviour
{
    [SerializeField] private Transform target;
    private Rigidbody targetRB;
    [SerializeField] private float propulsionForce = 1000f;
    [SerializeField] private float rotateSpeed = 20f;
    [SerializeField] private float moveAngleThreshold = 5f; 
    private Rigidbody attachedRB;
    private bool active;
    [SerializeField] private ParticleSystem rocketParticles;

    [SerializeField] private LayerMask ignoreLayers;
    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] private float explosionRadius = 15f;
    [SerializeField] private float explosionForce = 100f;
    [SerializeField] private float maxDamage = 75f;
    [SerializeField] private float minDamage = 25f;
    [SerializeField] private float armorPiercing = 0.5f;

    [SerializeField] private bool nonVRActivate;

    private Vector3 lastTargetVelocity;
    private Vector3 targetAcceleration;

    private Vector3 lastVelocity;
    private Vector3 acceleration;

    private Vector3 futureTargetPosition;

    private void Start()
    {
        attachedRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (nonVRActivate)
        {
            nonVRActivate = false;
            if(target != null)
                StartCoroutine(ActivateRoutine(0));
        }
        if (active)
        {
            if (target != null)
            {
                /*Vector3 targetVelocity = Vector3.zero;
                if(targetRB != null)
                    targetVelocity = targetRB.velocity;
                targetAcceleration = (targetVelocity - lastTargetVelocity) / Time.fixedDeltaTime;
                acceleration = (attachedRB.velocity - lastVelocity) / Time.fixedDeltaTime;
                Vector3 relativePosition = target.position - transform.position;
                Vector3 relativeVelocity = targetVelocity - attachedRB.velocity;
                Vector3 relativeAcceleration = targetAcceleration - acceleration;

                float speed = attachedRB.velocity.magnitude;
                float predictionTime = speed > 25f ? relativePosition.magnitude / speed : 5f;

                futureTargetPosition = target.position + (relativeVelocity * predictionTime) + (0.5f * predictionTime * predictionTime * relativeAcceleration);

                Vector3 targetDirection = (futureTargetPosition - transform.position).normalized;*/
                Vector3 targetDirection = (target.position - transform.position).normalized;

                //Debug.DrawLine(transform.position, futureTargetPosition, Color.green, 0.1f);
                Debug.DrawLine(transform.position, transform.position + targetDirection * 100, Color.green, 0.1f);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetDirection), Time.fixedDeltaTime * rotateSpeed);

                if (Vector3.Angle(transform.forward, targetDirection) < moveAngleThreshold)
                {
                    attachedRB.AddForce(transform.forward * propulsionForce, ForceMode.Acceleration);
                    if (!rocketParticles.isPlaying)
                        rocketParticles.Play();
                }
                else
                {
                    if (rocketParticles.isPlaying)
                    {
                        rocketParticles.Stop();
                        rocketParticles.Clear();
                    }
                }

                //lastTargetVelocity = targetVelocity;
                lastVelocity = attachedRB.velocity;
            }
            else
            {
                attachedRB.AddForce(transform.forward * propulsionForce, ForceMode.Acceleration);
            }
        }
        else
        {
            if (rocketParticles.isPlaying)
            {
                rocketParticles.Stop();
                rocketParticles.Clear();
            }
        }
    }

    public void Activate(Transform target, float delay)
    {
        this.target = target;
        target.TryGetComponent(out targetRB);
        StartCoroutine(ActivateRoutine(delay));
    }

    private IEnumerator ActivateRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        rocketParticles.Play();
        active = true;
        GetComponent<Collider>().enabled = true;
    }

    public void Detonate()
    {
        Instantiate(explosionParticles, transform.position, transform.rotation);
        Collider[] colliders = new Collider[64];
        int hits = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, colliders, ~ignoreLayers, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits; i++)
        {
            if (colliders[i].TryGetComponent<Rigidbody>(out var otherRB))
            {
                otherRB.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0, ForceMode.Impulse);
            }
            if (colliders[i].TryGetComponent<StatSystem>(out var statSystem))
            {
                float distance = Vector3.Distance(transform.position, statSystem.transform.position);
                // y = -(maxHeight / intercept^2) * x^2 + maxHeight + minHeight;
                float damage = -(maxDamage / (explosionRadius * explosionRadius)) * (distance * distance) + maxDamage + minDamage;
                statSystem.Damage(damage, armorPiercing);
            }
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Detonate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            if ((other.transform == target || (targetRB != null && other.attachedRigidbody == targetRB)))
            {
                Detonate();
            }
        }
        else if (other.transform.HasTag("Shields"))
        {
            Detonate();
        }
    }
}
