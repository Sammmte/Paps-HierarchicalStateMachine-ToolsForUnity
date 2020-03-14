using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class BuilderSettingsDrawer
    {
        private const BuilderGenericType DefaultStateIdType = BuilderGenericType.Int;
        private const BuilderGenericType DefaultTriggerType = BuilderGenericType.Int;

        private const float Width = 300, Height = 400, RightPadding = 10, TopPadding = 10;

        private BuilderGenericType _stateIdRepresentation { get; set; }
        private BuilderGenericType _triggerRepresentation { get; set; }

        public Type StateIdType { get; private set; }
        public Type TriggerType { get; private set; }

        public event Action<Type> OnStateIdTypeChanged;
        public event Action<Type> OnTriggerTypeChanged;

        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _controlsAreaStyle;

        private string _stateIdEnumTypeFullName = "";
        private string _triggerEnumTypeFullName = "";

        private HierarchicalStateMachineBuilder PlainStateMachineBuilder { get; set; }

        public BuilderSettingsDrawer(HierarchicalStateMachineBuilder builder = null)
        {
            PlainStateMachineBuilder = builder;

            _titleStyle = new GUIStyle();
            _titleStyle.padding = new RectOffset(20, 20, 20, 20);
            _titleStyle.alignment = TextAnchor.MiddleCenter;
            _titleStyle.fontSize = 20;

            _labelStyle = new GUIStyle();
            _labelStyle.wordWrap = true;

            _controlsAreaStyle = new GUIStyle();
            _controlsAreaStyle.padding = new RectOffset(20, 20, 20, 20);

            LoadStateIdTypeDataFrom(builder.StateIdType);
            LoadTriggerTypeDataFrom(builder.TriggerType);

            SetStateIdTypeByRepresentation();
            SetTriggerTypeByRepresentation();
        }

        private void LoadStateIdTypeDataFrom(Type type)
        {
            if (type == null)
                _stateIdRepresentation = DefaultStateIdType;

            if (type == typeof(int))
                _stateIdRepresentation = BuilderGenericType.Int;
            else if (type == typeof(float))
                _stateIdRepresentation = BuilderGenericType.Float;
            else if (type == typeof(string))
                _stateIdRepresentation = BuilderGenericType.String;
            else if (type.IsEnum)
            {
                _stateIdRepresentation = BuilderGenericType.Enum;
                _stateIdEnumTypeFullName = type.FullName;
            }
            else
            {
                _stateIdRepresentation = DefaultStateIdType;
            }
        }

        private void LoadTriggerTypeDataFrom(Type type)
        {
            if (type == null)
                _triggerRepresentation = DefaultTriggerType;

            if (type == typeof(int))
                _triggerRepresentation = BuilderGenericType.Int;
            else if (type == typeof(float))
                _triggerRepresentation = BuilderGenericType.Float;
            else if (type == typeof(string))
                _triggerRepresentation = BuilderGenericType.String;
            else if (type.IsEnum)
            {
                _triggerRepresentation = BuilderGenericType.Enum;
                _triggerEnumTypeFullName = type.FullName;
            }
            else
            {
                _triggerRepresentation = DefaultTriggerType;
            }
        }

        public void Draw(Rect windowRect)
        {
            var position = new Vector2(windowRect.size.x - RightPadding - Width, TopPadding);
            var size = new Vector2(Width, Height);

            var boxRect = new Rect(position, size);

            var previousColor = GUI.color;
            GUI.color = Color.gray;
            GUILayout.BeginArea(boxRect, GUI.skin.window);
            GUI.color = previousColor;

            DrawTitle();

            EditorGUI.BeginChangeCheck();
            DrawControls();

            if (EditorGUI.EndChangeCheck())
            {
                SetStateIdTypeByRepresentation();
                SetTriggerTypeByRepresentation();
            }

            GUILayout.EndArea();
        }

        private void SetStateIdTypeByRepresentation()
        {
            var previousType = StateIdType;

            if (_stateIdRepresentation == Editor.BuilderGenericType.Int)
                StateIdType = typeof(int);
            else if (_stateIdRepresentation == Editor.BuilderGenericType.Float)
                StateIdType = typeof(float);
            else if (_stateIdRepresentation == Editor.BuilderGenericType.String)
                StateIdType = typeof(string);
            else if (_stateIdRepresentation == Editor.BuilderGenericType.Enum)
            {
                if (string.IsNullOrEmpty(_stateIdEnumTypeFullName) == false)
                {
                    var enumType = GetTypeOf(_stateIdEnumTypeFullName);

                    if(enumType != null)
                    {
                        StateIdType = enumType;
                    }
                }
            }

            if(previousType != StateIdType)
            {
                OnStateIdTypeChanged?.Invoke(StateIdType);
            }
        }

        private void SetTriggerTypeByRepresentation()
        {
            var previousType = TriggerType;

            if (_triggerRepresentation == Editor.BuilderGenericType.Int)
                TriggerType = typeof(int);
            else if (_triggerRepresentation == Editor.BuilderGenericType.Float)
                TriggerType = typeof(float);
            else if (_triggerRepresentation == Editor.BuilderGenericType.String)
                TriggerType = typeof(string);
            else if (_triggerRepresentation == Editor.BuilderGenericType.Enum)
            {
                if (string.IsNullOrEmpty(_triggerEnumTypeFullName) == false)
                {
                    var enumType = GetTypeOf(_triggerEnumTypeFullName);

                    if (enumType != null)
                    {
                        TriggerType = enumType;
                    }
                }
            }

            if (previousType != TriggerType)
            {
                OnTriggerTypeChanged?.Invoke(TriggerType);
            }
        }

        private Type GetTypeOf(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(typeName);

                if (type != null)
                    return type;
            }

            return null;
        }

        private void DrawBuilderField()
        {
            GUILayout.Label("Hierarchical State Machine Builder", _labelStyle);
            GUI.enabled = false;
            EditorGUILayout.ObjectField(PlainStateMachineBuilder, typeof(HierarchicalStateMachineBuilder), false);
            GUI.enabled = true;
        }

        private void DrawControls()
        {
            EditorGUILayout.BeginVertical(_controlsAreaStyle);

            DrawBuilderField();

            GUILayout.Space(20);

            if (PlainStateMachineBuilder != null)
            {
                DrawStateIdRepresentationField();
                if (_stateIdRepresentation == BuilderGenericType.Enum)
                {
                    DrawEnumTypeFieldFor("State Id Enum Type Full Name", ref _stateIdEnumTypeFullName);
                }

                DrawTriggerRepresentationField();
                if(_triggerRepresentation == BuilderGenericType.Enum)
                {
                    DrawEnumTypeFieldFor("Trigger Enum Type Full Name", ref _triggerEnumTypeFullName);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawStateIdRepresentationField()
        {
            GUILayout.Label("State Id Type", _labelStyle);
            _stateIdRepresentation = (BuilderGenericType)EditorGUILayout.EnumPopup(_stateIdRepresentation);
        }

        private void DrawTriggerRepresentationField()
        {
            GUILayout.Label("Trigger Type", _labelStyle);
            _triggerRepresentation = (BuilderGenericType)EditorGUILayout.EnumPopup(_triggerRepresentation);
        }

        private void DrawTitle()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Builder Settings", _titleStyle);
            GUILayout.EndVertical();
        }

        private void DrawEnumTypeFieldFor(string title, ref string fullNameVariable)
        {
            GUILayout.Label(title, _labelStyle);

            fullNameVariable = EditorGUILayout.TextField(fullNameVariable);

            EditorGUILayout.HelpBox("If the enum type is a nested type, the name would be like this:\nTheNamespace.TheClass+NestedEnum", MessageType.Info);
        }
    }
}