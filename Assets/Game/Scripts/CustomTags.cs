using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTags : MonoBehaviour
{
    [SerializeField] List<string> tags = new List<string>();
    HashSet<string> hashTags = new HashSet<string>();

    private void Start()
    {
        foreach (string tag in tags)
        {
            hashTags.Add(tag);
        }
    }

    public bool HasTag(string tag)
    {
        return hashTags.Contains(tag);
    }
}
