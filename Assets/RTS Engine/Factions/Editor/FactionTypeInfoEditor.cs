using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RTSEngine
{
    [CustomEditor(typeof(FactionTypeInfo))]
    public class FactionTypeInfoEditor : Editor
    {
        SerializedObject target_SO;

        public void OnEnable()
        {
            target_SO = new SerializedObject(target as FactionTypeInfo);
            RTSEditorHelper.GetFactionTypes(true, target as FactionTypeInfo);
        }

        public override void OnInspectorGUI()
        {
            target_SO.Update(); //Always update the Serialized Object.

            EditorGUILayout.PropertyField(target_SO.FindProperty("_name"));
            EditorGUILayout.PropertyField(target_SO.FindProperty("code"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(target_SO.FindProperty("capitalBuilding"));
            EditorGUILayout.PropertyField(target_SO.FindProperty("centerBuilding"));
            EditorGUILayout.PropertyField(target_SO.FindProperty("populationBuilding"));
            EditorGUILayout.PropertyField(target_SO.FindProperty("extraBuildings"), true);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(target_SO.FindProperty("limits"), true);

            target_SO.ApplyModifiedProperties(); //Apply all modified properties always at the end of this method.
        }
    }
}
