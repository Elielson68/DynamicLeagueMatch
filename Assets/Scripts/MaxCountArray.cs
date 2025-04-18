#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

public class MaxItemsAttribute : PropertyAttribute
{
    public int MaxCount { get; }

    public MaxItemsAttribute(int maxCount)
    {
        MaxCount = maxCount;
    }
}




[CustomPropertyDrawer(typeof(MaxItemsAttribute))]
public class MaxItemsDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MaxItemsAttribute maxItemsAttr = (MaxItemsAttribute)attribute;

        // Verifica se é uma lista (SerializedPropertyType.Generic) OU um array (SerializedPropertyType.ArraySize)
        if (property.propertyType == SerializedPropertyType.Generic ||
            property.propertyType == SerializedPropertyType.ArraySize)
        {
            // Obtém o tamanho real da lista/array
            SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
            if (arraySizeProp != null)
            {
                int currentSize = arraySizeProp.intValue;
                if (currentSize > maxItemsAttr.MaxCount)
                {
                    // Aviso e correção
                    EditorGUILayout.HelpBox($"Máximo de {maxItemsAttr.MaxCount} itens permitidos!", MessageType.Error);
                    arraySizeProp.intValue = maxItemsAttr.MaxCount;
                    property.serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI(); // Força a atualização imediata
                }
            }
        }

        EditorGUI.PropertyField(position, property, label, true);
    }
}
#endif

