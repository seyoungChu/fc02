using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneHandle))]
public class SceneHandleEditor : Editor
{
    private void OnSceneGUI()
    {
        SceneHandle handle = target as SceneHandle;
        Handles.color = Color.cyan;
        Transform handleTR = handle.transform;
        Handles.DrawLine(handleTR.position, handleTR.position + handleTR.forward * handle.Size);
        Handles.color = Color.magenta;
        Handles.DrawLine(handleTR.position, handleTR.position + handleTR.right * handle.Size);
        Handles.color = Color.green;
        Handles.DrawLine(handleTR.position, handleTR.position + handleTR.up * handle.Size);
        Handles.color = Color.white;
        Handles.DrawWireArc(handleTR.position,-handleTR.up,handleTR.right, 90f,handle.Size);
        Handles.color = Color.yellow;
        Handles.DrawWireArc(handleTR.position, handleTR.up , -handleTR.right,90f, handle.Size);

    }
}
