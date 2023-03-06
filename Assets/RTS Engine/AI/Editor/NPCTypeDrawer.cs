using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

/* NPCTypeDrawer script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    [CustomPropertyDrawer(typeof(NPCTypeInfo))]
    public class NPCTypeDrawer : PropertyDrawer
    {
        private int index = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                Dictionary<string, NPCTypeInfo> npcTypeDic = RTSEditorHelper.GetNPCTypes();

                index = npcTypeDic.Values.ToList().IndexOf(property.objectReferenceValue as NPCTypeInfo);

                if (index < 0) //make sure the index value is always valid
                    index = 0;

                index = EditorGUI.Popup(position, label.text, index, npcTypeDic.Keys.ToArray());

                property.objectReferenceValue = npcTypeDic[npcTypeDic.Keys.ToArray()[index]] as Object;
            }
            else
                EditorGUI.LabelField(position, label.text, "Use [NPCType] with object reference fields.");
        }
    }
}
