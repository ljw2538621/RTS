using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

/* ResourceTypeDrawer script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    [CustomPropertyDrawer(typeof(ResourceTypeAttribute)), CustomPropertyDrawer(typeof(ResourceTypeInfo))]
    public class ResourceTypeDrawer : PropertyDrawer
    {
        private int index = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            Dictionary<string, ResourceTypeInfo> resourceTypeDic = RTSEditorHelper.GetResourceTypes();

            if (property.propertyType == SerializedPropertyType.ObjectReference)
                index = resourceTypeDic.Values.ToList().IndexOf(property.objectReferenceValue as ResourceTypeInfo);
            else if (property.propertyType == SerializedPropertyType.String)
                index = resourceTypeDic.Keys.ToList().IndexOf(property.stringValue);
            else
            {
                EditorGUI.LabelField(position, label.text, "Use [ResourceType] with object reference or string fields.");
                return;
            }

            if (index < 0) //make sure the index value is always valid
                index = 0;

            index = EditorGUI.Popup(position, label.text, index, resourceTypeDic.Keys.ToArray());

            if (property.propertyType == SerializedPropertyType.ObjectReference)
                property.objectReferenceValue = resourceTypeDic[resourceTypeDic.Keys.ToArray()[index]] as Object;
            else if (property.propertyType == SerializedPropertyType.String)
                property.stringValue = resourceTypeDic.Keys.ToArray()[index];
        }
    }
}
