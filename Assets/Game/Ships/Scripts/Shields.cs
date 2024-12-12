using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Shields : MonoBehaviour
{
    public StatSystem shieldsStatSystem;
    public StatSystem shipStatSystem;

    [SerializeField] private GameObject shieldObject;
    [SerializeField] private float damageTime = 0.5f;
    [SerializeField] private float maxAlpha = 1.0f;
    [SerializeField] private float minAlpha = 0.1f;
    [SerializeField] private float maxRadius = 1.0f;
    [SerializeField] private float minRadius = 0.1f;
    [SerializeField] private GameObject shieldRippleEffect;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ToggleShields(int state)
    {
        shieldObject.SetActive(state == 1);
    }

    private IEnumerator DamageRoutine(GameObject ripple, Vector3 damagePoint, float magnitude)
    {
        Material rippleMaterial = ripple.GetComponent<MeshRenderer>().material;
        rippleMaterial.SetVector("_Center", damagePoint);
        float timer = 0f;
        float halfDamageTime = damageTime * 0.5f;
        while (timer < halfDamageTime)
        {
            float t = timer / damageTime;
            rippleMaterial.SetFloat("_Alpha", Mathf.Lerp(minAlpha, maxAlpha, t));
            rippleMaterial.SetFloat("_Radius", Mathf.Lerp(minRadius, maxRadius, t) * magnitude);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        timer = 0;
        while (timer < halfDamageTime)
        {
            float t = timer / damageTime;
            rippleMaterial.SetFloat("_Alpha", Mathf.Lerp(maxAlpha, minAlpha, t));
            rippleMaterial.SetFloat("_Radius", Mathf.Lerp(maxRadius, minRadius, t) * magnitude);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Destroy(ripple);
    }

    public void Damage(Vector3 damagePoint, float magnitude)
    {
        if (shieldObject.activeSelf)
        {
            GameObject ripple = Instantiate(shieldRippleEffect, damagePoint, Quaternion.identity, shieldObject.transform);
            StartCoroutine(DamageRoutine(ripple, damagePoint, magnitude));
        }
    }
}
