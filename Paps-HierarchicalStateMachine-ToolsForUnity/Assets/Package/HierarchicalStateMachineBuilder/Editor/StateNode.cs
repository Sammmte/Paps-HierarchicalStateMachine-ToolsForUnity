using System;
using UnityEditor;
using UnityEngine;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class StateNode : ISelectable
    {
        private const float Width = 200;
        private const float Height = 150;

        private const int ControlPaddingLeft = 20, ControlPaddingRight = 20, ControlPaddingTop = 20, ControlPaddingBottom = 20;
        private const float SelectedExtraPixels = 5;
        private static readonly float SelectedHalfExtraPixels = SelectedExtraPixels / 2;

        public object StateId
        {
            get
            {
                if (_stateIdDrawer == null)
                    return null;
                else
                    return _stateIdDrawer.Value;
            }
        }

        public ScriptableState StateObject { get; private set; }

        public Action<StateNode, object, object> OnStateIdChanged;
        public Action<StateNode, ScriptableState, ScriptableState> OnStateObjectChanged;
        public Action<StateNode, Vector2> OnPositionChanged;

        private Rect _nodeRect;
        private GUIStyle _controlsAreaStyle;
        private GUIStyle _identityTitleStyle;

        private GenericTypeDrawer _stateIdDrawer;

        public int NodeId { get; private set; }

        private static readonly Color NormalColor = Color.gray;
        private static readonly Color InitialColor = new Color(235f / 255f, 149f / 255f, 50f / 255f);
        private static readonly Color SelectedColor = new Color(135f / 255f, 206f / 255f, 250f / 255f);
        private static readonly Color InitialChildColor = Color.red;

        private Color _currentColor;

        private bool _isSelected;

        public Vector2 Position => _nodeRect.position;
        public Vector2 Center => _nodeRect.center;

        public bool IsInitial { get; private set; }
        public bool IsInitialChild { get; private set; }

        public StateNode(Vector2 position, Type stateIdType, int nodeId, ScriptableState stateAsset = null, object stateId = null)
        {
            _nodeRect = new Rect(position.x, position.y, Width, Height);
            NodeId = nodeId;

            _controlsAreaStyle = new GUIStyle();
            _controlsAreaStyle.padding = new RectOffset(ControlPaddingLeft, ControlPaddingRight, ControlPaddingTop, ControlPaddingBottom);

            _identityTitleStyle = new GUIStyle();
            _identityTitleStyle.padding = new RectOffset(20, 20, 20, 20);
            _identityTitleStyle.alignment = TextAnchor.MiddleCenter;
            _identityTitleStyle.fontSize = 20;
            _identityTitleStyle.wordWrap = true;

            _stateIdDrawer = GenericTypeDrawerFactory.Create(stateIdType, stateId);

            AsNormal();
        }

        public void SetNewStateIdType(Type stateIdType)
        {
            if (stateIdType == null)
                _stateIdDrawer = null;
            else
                _stateIdDrawer = GenericTypeDrawerFactory.Create(stateIdType);
        }

        public void Drag(Vector2 delta)
        {
            _nodeRect.position += delta;
            OnPositionChanged?.Invoke(this, _nodeRect.position);
        }

        public void Draw()
        {
            if(_isSelected)
                DrawSelectedOutline();

            var previousColor = GUI.color;
            GUI.color = _currentColor;
            _nodeRect = GUI.Window(NodeId, _nodeRect, id => { DrawWindow(); }, StateId != null ? "State Id: " + StateId.ToString() : "No Id");
            GUI.color = previousColor;
        }

        private void DrawSelectedOutline()
        {
            var selectedNodeRect = new Rect(
                new Vector2(_nodeRect.position.x - SelectedHalfExtraPixels, _nodeRect.position.y - SelectedHalfExtraPixels),
                new Vector2(_nodeRect.size.x + SelectedExtraPixels, _nodeRect.size.y + SelectedExtraPixels)
                );

            var previousColor = GUI.color;
            GUI.color = SelectedColor;
            GUILayout.BeginArea(selectedNodeRect, GUI.skin.window);
            GUILayout.EndArea();
            GUI.color = previousColor;
        }

        private void DrawWindow()
        {
            if (StateObject != null)
            {
                if(string.IsNullOrEmpty(StateObject.DebugName))
                    GUILayout.Label("Nameless Object", _identityTitleStyle);
                else
                    GUILayout.Label(StateObject.DebugName, _identityTitleStyle);
            }
            else
                GUILayout.Label("Empty State", _identityTitleStyle);
        }

        public void DrawControls()
        {
            EditorGUILayout.BeginVertical(_controlsAreaStyle);

            var previousStateId = StateId;
            DrawStateIdDrawer();
            if (previousStateId != StateId) OnStateIdChanged?.Invoke(this, previousStateId, StateId);

            EditorGUI.BeginChangeCheck();
            var previousStateObject = StateObject;
            DrawStateAssetField();
            if (EditorGUI.EndChangeCheck()) OnStateObjectChanged?.Invoke(this, previousStateObject, StateObject);

            EditorGUILayout.EndVertical();
        }

        private void DrawStateAssetField()
        {
            GUILayout.Label("State Asset");
            StateObject = (ScriptableState)EditorGUILayout.ObjectField(StateObject, typeof(ScriptableState), false);
        }

        private void DrawStateIdDrawer()
        {
            _stateIdDrawer.Draw("State Id");
        }

        public void Select()
        {
            _isSelected = true;
        }

        public void Deselect()
        {
            _isSelected = false;
        }

        public void AsNormal()
        {
            IsInitial = false;
            IsInitialChild = false;
            _currentColor = NormalColor;
        }

        public void AsInitial()
        {
            IsInitial = true;
            IsInitialChild = false;
            _currentColor = InitialColor;
        }

        public void AsInitialChild()
        {
            IsInitial = false;
            IsInitialChild = true;
            _currentColor = InitialChildColor;
        }

        public Rect GetRect()
        {
            return _nodeRect;
        }

        public bool IsPointOverNode(Vector2 point)
        {
            return _nodeRect.Contains(point);
        }
    }
}