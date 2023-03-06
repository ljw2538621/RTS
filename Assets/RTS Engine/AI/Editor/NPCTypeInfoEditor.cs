using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace RTSEngine
{
    /// <summary>
    /// Custom editor for the NPCTypeInfo scriptable object.
    /// </summary>
    [CustomEditor(typeof(NPCTypeInfo))]
    public class NPCTypeInfoEditor : Editor
    {
        private SerializedObject target_SO;
        private int index;

        public void OnEnable()
        {
            target_SO = new SerializedObject(target as NPCTypeInfo);
            RTSEditorHelper.GetNPCTypes(true, target as NPCTypeInfo);
        }

        public override void OnInspectorGUI()
        {
            target_SO.Update(); //Always update the Serialized Object.

            EditorGUILayout.PropertyField(target_SO.FindProperty("_name"));
            EditorGUILayout.PropertyField(target_SO.FindProperty("code"));
            EditorGUILayout.PropertyField(target_SO.FindProperty("npcManagers"), true);

            target_SO.ApplyModifiedProperties(); //Apply all modified properties always at the end of this method.
        }
    }
}
