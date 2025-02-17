using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.InputSystem;

[RequireComponent(typeof(XRGrabInteractable))]
public class Gun : MonoBehaviour
{
    XRGrabInteractable interactable;

    private int handIndex = -1; // 0 is left, 1 is right
    [SerializeField] InputActionReference[] fireAction;

    [SerializeField] Transform trigger;
    [SerializeField] GameObject shootParticles;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] AudioSource triggerAudio;
    [SerializeField] float bulletSpeed = 60;

    [SerializeField] Animation slideAnimation;
    [SerializeField] GameObject casing;
    [SerializeField] Transform casingSpawn;
    [SerializeField] float casingSpeed = 5;

    [SerializeField] GameObject[] gunColliderObjects;

    [SerializeField] Switch fireModeSwitch;

    [SerializeField] Ammo ammoManager;
    [SerializeField] int tracerInterval = 3;
    [SerializeField] float fireRate = 0.077f;
    float holdTime = 0;
    [SerializeField] float burstDelay = 1;
    [SerializeField] int burstAmount = 3;
    int burstCount;

    private void Start()
    {
        interactable = GetComponent<XRGrabInteractable>();
    }

    void Update()
    {
        if (handIndex != -1)
        {
            float squeezeAxis = fireAction[handIndex].action.ReadValue<float>();
            trigger.localEulerAngles = new Vector3(squeezeAxis * 40, 0, 0);
            if (ammoManager.attachedToGun && ammoManager.currentAmmo > 0)
            {
                switch (fireModeSwitch.currentState)
                {
                    case 1: // Semi
                        if (fireAction[handIndex].action.WasPressedThisFrame())
                        {
                            Fire();
                        }
                        break;
                    case 2: // Auto
                        if (fireAction[handIndex].action.IsPressed())
                        {
                            holdTime += Time.deltaTime;
                            if (holdTime >= fireRate)
                            {
                                Fire();
                                holdTime = 0;
                            }
                        }
                        break;
                    case 3: // Burst
                        if (fireAction[handIndex].action.IsPressed())
                        {
                            holdTime += Time.deltaTime;
                            if (burstCount < burstAmount)
                            {
                                if (holdTime >= fireRate)
                                {
                                    Fire();
                                    holdTime = 0;
                                    burstCount++;
                                }
                            }
                            else
                            {
                                if (holdTime >= burstDelay)
                                {
                                    holdTime = 0;
                                    burstCount = 0;
                                }
                            }
                        }
                        break;
                    default:
                        if (fireAction[handIndex].action.WasPressedThisFrame())
                        {
                            triggerAudio.Play();
                        }
                        break;
                }
            }
            else
            {
                if (fireAction[handIndex].action.WasPressedThisFrame())
                {
                    triggerAudio.Play();
                }
            }
        }
    }

    public void Attach()
    {
        foreach (GameObject GO in gunColliderObjects)
        {
            GO.layer = 3;
        }
    }

    public void Detach()
    {
        foreach (GameObject GO in gunColliderObjects)
        {
            GO.layer = 0;
        }
    }

    public void Fire()
    {
        if(ammoManager.attachedToGun && ammoManager.currentAmmo > 0)
        {
            slideAnimation.Play();
            Rigidbody bulletRB = Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation).GetComponent<Rigidbody>();
            bulletRB.velocity = bulletSpawn.forward * bulletSpeed;
            if (ammoManager.currentAmmo % tracerInterval == 0)
            {
                bulletRB.GetComponent<TrailRenderer>().enabled = true;
            }
            Instantiate(shootParticles, bulletSpawn.position, bulletSpawn.rotation);
            ammoManager.RemoveBullet();

            Rigidbody casingRB = Instantiate(casing, casingSpawn.position, casingSpawn.rotation).GetComponent<Rigidbody>();
            casingRB.velocity = casingSpawn.right * casingSpeed;
        }
    }

    public int AmmoCount()
    {
        return ammoManager.currentAmmo;
    }

    public int MaxAmmo()
    {
        return ammoManager.maxAmmo;
    }

    public void OnAttach()
    {
        handIndex = 0;
    }
}
