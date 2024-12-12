using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class HUDSystem : MonoBehaviour
{
    public bool active = false;

    public Transform HUDPivot;
    [SerializeField] private GameObject HUDObjectPrefab;
    [SerializeField] private GameObject HUDObjectParent;
    [SerializeField] private float HUDDistance = 1.5f;
    public RectTransform combatPanel;
    public Image mainCrosshair;

    private Dictionary<int, HUDObject> instanceIDHUDPair = new Dictionary<int, HUDObject>();

    public HUDObject CreateObject(int ID, Vector3 position, Bounds bounds, string name, string details)
    {
        HUDObject newHUDObject = Instantiate(HUDObjectPrefab, HUDObjectParent.transform).GetComponent<HUDObject>();
        newHUDObject.Init(this, position, bounds, ID, name, details);
        return newHUDObject;
    }

    public HUDObject CreateObject(int ID, Transform target, string name, string details)
    {
        Bounds bounds = target.TryGetComponent<MeshRenderer>(out var colliderRenderer) ? colliderRenderer.bounds : new Bounds() { center = target.position, size = Vector3.zero };
        foreach (Transform child in target)
        {
            if (child.TryGetComponent<MeshRenderer>(out var childRenderer))
            {
                bounds.Encapsulate(childRenderer.bounds);
            }
        }
        Vector3 position = HUDPivot.position + (target.position - HUDPivot.position).normalized * HUDDistance;
        return CreateObject(ID, position, bounds, name, details);
    }

    public bool UpdateObject(int ID, Vector3 position, Bounds bounds, string name, string details)
    {
        if (instanceIDHUDPair.TryGetValue(ID, out HUDObject HUDObject))
        {
            HUDObject.UpdateObject(position, bounds, name, details);
            return true;
        }
        return false;
    }

    public bool UpdateObject(int ID, Transform target, string name, string details)
    {
        if (instanceIDHUDPair.TryGetValue(ID, out HUDObject HUDObject))
        {
            Bounds bounds = target.TryGetComponent<MeshRenderer>(out var colliderRenderer) ? colliderRenderer.bounds : new Bounds() { center = target.position, size = Vector3.zero };
            foreach (Transform child in target)
            {
                if (child.TryGetComponent<MeshRenderer>(out var childRenderer))
                {
                    bounds.Encapsulate(childRenderer.bounds);
                }
            }
            Vector3 position = HUDPivot.position + (target.position - HUDPivot.position).normalized * HUDDistance;
            HUDObject.UpdateObject(position, bounds, name, details);
            return true;
        }

        return false;
    }

    public bool TryGetValue(int ID, out HUDObject HUDObject)
    {
        return instanceIDHUDPair.TryGetValue(ID, out HUDObject);
    }

    public void Add(int ID, HUDObject HUDObject)
    {
        instanceIDHUDPair.Add(ID, HUDObject);
    }

    public void Remove(int ID)
    {
        instanceIDHUDPair.Remove(ID);
    }

    public void ToggleHUD(int state)
    {
        HUDPivot.gameObject.SetActive(state == 1);
    }
}
