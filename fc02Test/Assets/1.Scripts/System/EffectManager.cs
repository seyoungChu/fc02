using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : SingletonMonobehaviour<EffectManager>
{
    private Transform effctPoolRoot = null;

    private void Start()
    {
        if (effctPoolRoot == null)
        {
            effctPoolRoot = new GameObject("EffectRoot").transform;
            effctPoolRoot.SetParent(transform);
        }
        
    }

    public GameObject EffectOneShot(int index, Vector3 position)
    {
        EffectClip clip = DataManager.EffectData().GetClip(index);
        GameObject effectInstance = clip.Instantiate(position);
        effectInstance.SetActive(true);
        return effectInstance;
    }
}
