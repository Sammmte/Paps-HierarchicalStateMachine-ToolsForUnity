using UnityEngine;
using System;
using UnityEditor;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class EnumDrawer : GenericTypeDrawer
    {
        private Type _enumType;

        private string[] _possibleValues;

        private int _currentSelected;

        public EnumDrawer(Type enumType, object value) : base(value)
        {
            _enumType = enumType;

            SavePossibleValues();
            SetCurrentSelectedIfInitialValueIsNotNull(value);
        }

        private void SavePossibleValues()
        {
            var values = Enum.GetValues(_enumType);

            _possibleValues = new string[values.Length + 1];
            _possibleValues[0] = "No Value";

            int index = 1;

            foreach (var enumValue in values)
            {
                _possibleValues[index] = enumValue.ToString();
                index++;
            }
        }

        private void SetCurrentSelectedIfInitialValueIsNotNull(object value)
        {
            if(value != null)
            {
                var values = Enum.GetValues(_enumType);

                int index = 1;
                foreach(var enumValue in values)
                {
                    if (object.Equals(enumValue, value))
                    {
                        _currentSelected = index;
                        return;
                    }

                    index++;
                }
            }
        }

        protected override void DrawValueControl()
        {
            EditorGUI.BeginChangeCheck();

            _currentSelected = EditorGUILayout.Popup(_currentSelected, _possibleValues);

            if (EditorGUI.EndChangeCheck())
            {
                if (_currentSelected != 0)
                    Value = Enum.Parse(_enumType, _possibleValues[_currentSelected]);
                else
                {
                    Value = null;
                }
            }
        }

        protected override bool IsValidType(object value)
        {
            return value.GetType().IsEnum;
        }
    }
}