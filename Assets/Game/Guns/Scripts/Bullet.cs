using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private LayerMask ignoreLayers;

    [SerializeField] float destroyDelay = 5;
    [SerializeField] float damageAmount = 10;
    [SerializeField] float armorPiercing = 0.25f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, destroyDelay);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent<StatSystem>(out var statSystem))
        {
            statSystem.Damage(damageAmount, armorPiercing);
        }
        else
        {
            StatSystem parentStatSystem = collision.transform.GetComponentInParent<StatSystem>();
            if (parentStatSystem != null)
            {
                parentStatSystem.Damage(damageAmount, armorPiercing);
            }
        }
        if (collision.transform.TryGetComponent<Torpedo>(out var torpedo))
        {
            torpedo.Detonate();
        }
        Destroy(gameObject);
    }
}
