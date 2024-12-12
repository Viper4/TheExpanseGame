using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;

[RequireComponent(typeof(Collider))]
public class GunGrabPoint : MonoBehaviour
{
    [SerializeField] MeshRenderer highlight;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.HasTag("Hand"))
        {
            highlight.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.HasTag("Hand"))
        {
            highlight.enabled = false;
        }
    }

    public void Attach()
    {
        highlight.enabled = false;
    }

    public void Detach()
    {
        highlight.enabled = false;
    }
}
