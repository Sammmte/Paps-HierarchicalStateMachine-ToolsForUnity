using UnityEngine;
using UnityEditor;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class StringDrawer : GenericTypeDrawer
    {
        public StringDrawer(object value) : base(value)
        {

        }

        protected override void DrawValueControl()
        {
            EditorGUI.BeginChangeCheck();

            string value = Value != null ? Value.ToString() : "";

            if (Value == null)
                value = EditorGUILayout.TextField(value);
            else
                value = EditorGUILayout.TextField(value);

            if (EditorGUI.EndChangeCheck())
            {
                if (string.IsNullOrEmpty(value) == false)
                    Value = value;
                else
                {
                    Debug.LogWarning("String field cannot be empty");
                    Value = null;
                }
            }
        }

        protected override bool IsValidType(object value)
        {
            return value.GetType() == typeof(string);
        }
    }
}