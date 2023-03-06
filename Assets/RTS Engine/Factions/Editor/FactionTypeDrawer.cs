using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

/* FactionTypeDrawer script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    [CustomPropertyDrawer(typeof(FactionTypeInfo))]
    public class FactionTypeDrawer : PropertyDrawer
    {
        private int index = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                Dictionary<string, FactionTypeInfo> factionTypeDic = RTSEditorHelper.GetFactionTypes();

                index = factionTypeDic.Values.ToList().IndexOf(property.objectReferenceValue as FactionTypeInfo);

                if (index < 0) //make sure the index value is always valid
                    index = 0;

                index = EditorGUI.Popup(position, label.text, index, factionTypeDic.Keys.ToArray());

                property.objectReferenceValue = factionTypeDic[factionTypeDic.Keys.ToArray()[index]] as Object;
            }
            else
                EditorGUI.LabelField(position, label.text, "Use [FactionType] with object reference fields.");
        }
    }
}
