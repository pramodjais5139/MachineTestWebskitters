using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryArea : MonoBehaviour
{
    public PlayerCharacterController character;
    public MeshCollider mineMeshCollider;

    private void Awake()
    {
        mineMeshCollider = gameObject.AddComponent<MeshCollider>();
        mineMeshCollider.convex = true;
        mineMeshCollider.isTrigger = true;
    }
}
