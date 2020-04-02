using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    /// <summary>
    /// Load the specified path.
    /// </summary>
    public static Object Load(string path)
    {
        return Resources.Load(path);
    }

    /// <summary>
    /// Loadands the instantiate.
    /// </summary>
    public static GameObject LoadandInstantiate(string path)
    {
        Object source = Load(path);
        if (source != null)
        {
            return GameObject.Instantiate(source) as GameObject;
        }
        else
        {
            return null;
        }
    }
}