using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextMeshOrder : MonoBehaviour
{
    public int order = 0;
    // Start is called before the first frame update
    void Start()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingOrder = order;
            return;
        }
        SkinnedMeshRenderer skinned = GetComponent<SkinnedMeshRenderer>();
        if (skinned != null)
        {
            skinned.sortingOrder = order;
            return;
        }
    }
}
