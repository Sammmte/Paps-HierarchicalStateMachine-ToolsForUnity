using UnityEngine;
using UnityEditor;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class IntDrawer : GenericTypeDrawer
    {
        public IntDrawer(object value) : base(value)
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
                if (int.TryParse(value, out int result))
                    Value = result;
                else
                {
                    if(string.IsNullOrEmpty(value) == false)
                    {
                        Debug.LogWarning("Value " + value + " is not an Integer");
                    }
                    
                    Value = null;
                }
            }
        }

        protected override bool IsValidType(object value)
        {
            return value.GetType() == typeof(int);
        }
    }
}