using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(AutoVerticalSize))]
public class AutoVertSizeEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //DrawDefaultInspector();
        if(GUILayout.Button("Recalc Size"))
        {
            AutoVerticalSize myScript = ((AutoVerticalSize)target);
            myScript.AdjustSize();
        }
    }
}
