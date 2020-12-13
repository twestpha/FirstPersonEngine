using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TiledBuilder))]
public class TiledBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TiledBuilder tiledBuilder = (TiledBuilder) target;
        if (GUILayout.Button("Generate Tiled Map"))
        {
            tiledBuilder.GenerateMap();
        }
    }

}
