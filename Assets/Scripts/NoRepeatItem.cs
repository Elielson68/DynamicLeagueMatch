#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class NoDuplicateAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(NoDuplicateAttribute))]
public class NoDuplicateDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Só aplica a validação se for um array/lista
        if (property.isArray && property.propertyType == SerializedPropertyType.Generic)
        {
            CheckForDuplicates(property);
        }
        EditorGUI.PropertyField(position, property, label, true);
    }

    private void CheckForDuplicates(SerializedProperty listProperty)
    {
        List<string> values = new List<string>();
        for (int i = 0; i < listProperty.arraySize; i++)
        {
            SerializedProperty element = listProperty.GetArrayElementAtIndex(i);

            // Adapte conforme o tipo do elemento (ex: int, string, Vector3, etc.)
            string value = element.propertyType switch
            {
                SerializedPropertyType.String => element.stringValue,
                SerializedPropertyType.Integer => element.intValue.ToString(),
                _ => element.objectReferenceValue?.ToString() ?? "null"
            };

            if (values.Contains(value))
            {
                // Destaca elementos repetidos
                GUIStyle errorStyle = new GUIStyle(EditorStyles.label);
                errorStyle.normal.textColor = Color.red;
                EditorGUILayout.LabelField($"Elemento {i} duplicado: {value}", errorStyle);
            }
            values.Add(value);
        }
    }
}
#endif