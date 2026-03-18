using UnityEditor;
using UnityEngine;

namespace MVsToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnly_PropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Pr�serve la hauteur normale (y compris les enfants/arrays)
            return EditorGUI.GetPropertyHeight(property, label, includeChildren: true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledScope(true)) // grise et bloque l��dition
            {
                EditorGUI.PropertyField(position, property, label, includeChildren: true);
            }
        }
    }

}