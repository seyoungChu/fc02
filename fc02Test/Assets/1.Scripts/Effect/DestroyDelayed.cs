using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyDelayed : MonoBehaviour
{
    public float DelayedTime = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, DelayedTime);
    }

}
