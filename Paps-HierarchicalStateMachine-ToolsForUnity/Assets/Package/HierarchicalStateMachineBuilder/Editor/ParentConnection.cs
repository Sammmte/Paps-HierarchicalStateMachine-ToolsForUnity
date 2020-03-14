using UnityEditor;
using UnityEngine;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class ParentConnection
    {
        private const float Width = 4f, ClickableExtraRange = 8f, ArrowWidthExtent = 8, ArrowHeightExtent = 8;

        private const int ControlPaddingLeft = 20, ControlPaddingRight = 20, ControlPaddingTop = 20, ControlPaddingBottom = 20;

        private static readonly Color SelectedColor = new Color(44f / 255f, 130f / 255f, 201f / 255f);
        private static readonly Color NormalColor = Color.red;

        private Color _currentColor;

        private GUIStyle _controlsAreaStyle;
        private GUIStyle _simpleLabelStyle;

        public Vector2 StartPoint => Parent.Center;
        public Vector2 EndPoint => Child.Center;

        public StateNode Parent { get; private set; }
        public StateNode Child { get; private set; }

        public ParentConnection(StateNode parent, StateNode child)
        {
            Parent = parent;
            Child = child;

            _controlsAreaStyle = new GUIStyle();
            _controlsAreaStyle.padding = new RectOffset(ControlPaddingLeft, ControlPaddingRight, ControlPaddingTop, ControlPaddingBottom);

            _simpleLabelStyle = new GUIStyle();
            _simpleLabelStyle.wordWrap = true;

            _currentColor = NormalColor;
        }

        public void Draw()
        {
            var previousColor = Handles.color;
            Handles.color = _currentColor;

            Handles.DrawAAPolyLine(Width, StartPoint, EndPoint);
            DrawArrow(Vector2.Lerp(StartPoint, EndPoint, 0.5f), EndPoint - StartPoint);

            Handles.color = previousColor;
        }

        private void DrawArrow(Vector2 center, Vector2 direction)
        {
            Vector2 normalizedDirection = direction.normalized;

            var perpendicular = Vector2.Perpendicular(direction) * -1;

            var inferiorLeftPoint =
                center + (perpendicular.normalized * ArrowWidthExtent) - (normalizedDirection * ArrowHeightExtent);

            var inferiorRightPoint =
                center - (perpendicular.normalized * ArrowWidthExtent) - (normalizedDirection * ArrowHeightExtent);

            var superiorPoint =
                center + (normalizedDirection * ArrowHeightExtent);

            Handles.DrawAAConvexPolygon(superiorPoint, inferiorLeftPoint, inferiorRightPoint, superiorPoint);
        }

        public void DrawControls()
        {
            EditorGUILayout.BeginVertical(_controlsAreaStyle);

            DrawTransitionParts();

            EditorGUILayout.EndVertical();
        }

        private void DrawTransitionParts()
        {
            if (Parent.StateId != null && Child.StateId != null)
                GUILayout.Label("Parent State: " + Parent.StateId + " -> Child State: " + Child.StateId, _simpleLabelStyle);
        }

        public bool IsPointOverConnection(Vector2 point)
        {
            return HandleUtility.DistancePointLine(point, StartPoint, EndPoint) <= ClickableExtraRange;
        }

        public void Select()
        {
            _currentColor = SelectedColor;
        }

        public void Deselect()
        {
            _currentColor = NormalColor;
        }
    }
}