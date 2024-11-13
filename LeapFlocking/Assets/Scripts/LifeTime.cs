using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeTime : MonoBehaviour
{
    public float lifetime = 5.0f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        
    }
}
