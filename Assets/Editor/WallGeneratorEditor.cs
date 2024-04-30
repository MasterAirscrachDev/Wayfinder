using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WallGenerator))]
public class WallGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WallGenerator myScript = (WallGenerator)target;
        if(GUILayout.Button("Create Next Point"))
        {
            myScript.CreateNextPoint();
        }
    }
}