using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;

public abstract class Turret : MonoBehaviour
{
    public StatSystem statSystem;
    protected TurretSystem turretSystem;

    [Header("Turret")] public bool active = true;
    [SerializeField] protected LayerMask ignoreLayers;
    [SerializeField] protected Transform origin;
    [SerializeField] public Transform platform;
    [SerializeField] public Transform rotatingObject;
    public Transform firePoint;
    [SerializeField] private Transform casingPoint;
    [SerializeField] private float casingRandomness = 0.1f;

    [SerializeField] private Vector3 minAngles;
    [SerializeField] private Vector3 maxAngles;
    [SerializeField] private float rotateSpeed = 180f;

    [SerializeField] private float shootAngle = 1;
    [SerializeField, Tooltip("One bullet per fireRate seconds.")] private float fireRate = 0.15f;
    protected float fireTime = 0;
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] private GameObject shootParticles;
    [SerializeField] protected float projectileSpeed = 50;
    [SerializeField, Tooltip("0 for no tracer. Otherwise 1 tracer every tracerInterval shots.")] protected int tracerInterval = 3;
    protected int tracerCounter = 0;

    [SerializeField] private GameObject casingPrefab;
    [SerializeField] private float casingSpeed = 5;

    public Rigidbody target;
    [HideInInspector] public Vector3 aimDirection;
    [HideInInspector] public bool fire = false;
    [SerializeField] protected bool showLines;

    public GameObject UIModel;

    public bool destroyed = false;
    [SerializeField, Tooltip("Percent of health lost before enabling damaged particles.")] private float damagedThreshold = 0.5f;
    [SerializeField] private ParticleSystem damagedParticles;
    [SerializeField] private GameObject aliveGameObject;
    [SerializeField] private GameObject destroyedGameObject;

    protected virtual void Start()
    {
        statSystem = GetComponent<StatSystem>();
        turretSystem = GetComponentInParent<TurretSystem>();
    }

    protected virtual void Update()
    {
        if (active)
        {
            if (fire)
            {
                fireTime += Time.deltaTime;
                if (fireTime >= fireRate)
                {
                    if (Vector3.Angle(firePoint.forward, aimDirection) < shootAngle)
                    {
                        Fire();
                    }
                }
            }
            if (Mathf.Abs(aimDirection.x) > 0.01f || Mathf.Abs(aimDirection.y) > 0.01f || Mathf.Abs(aimDirection.z) > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(aimDirection, platform.up);
                platform.rotation = rotatingObject.rotation = Quaternion.RotateTowards(rotatingObject.rotation, targetRotation, rotateSpeed * Time.deltaTime);
                platform.localEulerAngles = new Vector3(0, platform.localEulerAngles.y, 0);
                rotatingObject.localEulerAngles = CustomMethods.Clamp(rotatingObject.localEulerAngles.FixEulers(), minAngles, maxAngles);
            }
        }
    }

    protected virtual void Fire()
    {
        tracerCounter++;
        fireTime = 0;
        Rigidbody projectileRB = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation).GetComponent<Rigidbody>();
        projectileRB.velocity = turretSystem.ship.physicsHandler.velocity.ToVector3() + firePoint.forward * projectileSpeed;
        if (tracerCounter >= tracerInterval)
        {
            projectileRB.GetComponent<TrailRenderer>().enabled = true;
            tracerCounter = 0;
        }
        if (shootParticles != null)
            Instantiate(shootParticles, firePoint.position, firePoint.rotation);

        if (casingPoint != null && casingPrefab != null)
        {
            Rigidbody casingRigidbody = Instantiate(casingPrefab, casingPoint.position, casingPoint.rotation).GetComponent<Rigidbody>();
            casingRigidbody.angularVelocity = Random.insideUnitSphere * casingRandomness;
            casingRigidbody.velocity = turretSystem.ship.physicsHandler.velocity.ToVector3() + (casingPoint.up + Random.insideUnitSphere * casingRandomness) * casingSpeed;
        }
    }

    public bool GetObstruction(out RaycastHit hit)
    {
        return Physics.Raycast(firePoint.position, firePoint.forward, out hit, Mathf.Infinity, ~ignoreLayers);
    }

    public void TryPlayDamagedParticles()
    {
        if (damagedParticles != null && !damagedParticles.isPlaying && statSystem.health / statSystem.maxHealth < damagedThreshold)
        {
            damagedParticles.Play();
        }
    }

    public void OnDeath()
    {
        aliveGameObject.SetActive(false);
        destroyedGameObject.SetActive(true);
        destroyed = true;
    }

    public void Repair(float healAmount)
    {
        statSystem.Heal(healAmount);
        aliveGameObject.SetActive(true);
        destroyedGameObject.SetActive(false);
        TryPlayDamagedParticles();
        destroyed = false;
    }
}
