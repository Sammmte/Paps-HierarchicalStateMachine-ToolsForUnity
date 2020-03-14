using UnityEditor;
using UnityEngine;
using System;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class TransitionConnection : ISelectable
    {
        private const float Width = 4f, ClickableExtraRange = 8f, ArrowWidthExtent = 8, ArrowHeightExtent = 8, LineOffset = 7;

        private const int ControlPaddingLeft = 20, ControlPaddingRight = 20, ControlPaddingTop = 20, ControlPaddingBottom = 20;

        private static readonly Color SelectedColor = new Color(44f / 255f, 130f / 255f, 201f / 255f);
        private static readonly Color NormalColor = Color.white;

        private readonly StateNode _source;
        private readonly StateNode _target;
        
        private bool _guardConditionsArrayOpened;
        private GUIStyle _controlsAreaStyle;
        private GUIStyle _simpleLabelStyle;
        private GenericTypeDrawer _triggerDrawer;

        private Color _currentColor;

        public Vector2 StartPoint => GetStartPoint();
        public Vector2 EndPoint => GetEndPoint();
        public object Trigger => _triggerDrawer.Value;
        public ScriptableGuardCondition[] GuardConditions { get; private set; }
        public Action<TransitionConnection, object, object> OnTriggerChanged;
        public Action<TransitionConnection, ScriptableGuardCondition[]> OnGuardConditionsChanged;

        public StateNode Source => _source;
        public StateNode Target => _target;

        public object StateFrom => Source.StateId;
        public object StateTo => Target.StateId;

        public TransitionConnection(StateNode source, StateNode target, Type triggerType, object trigger = null, ScriptableGuardCondition[] guardConditions = null)
        {
            _source = source;
            _target = target;

            _triggerDrawer = GenericTypeDrawerFactory.Create(triggerType, trigger);
            GuardConditions = guardConditions;

            _controlsAreaStyle = new GUIStyle();
            _controlsAreaStyle.padding = new RectOffset(ControlPaddingLeft, ControlPaddingRight, ControlPaddingTop, ControlPaddingBottom);

            _simpleLabelStyle = new GUIStyle();
            _simpleLabelStyle.wordWrap = true;

            _currentColor = NormalColor;
        }

        public void SetNewTriggerType(Type triggerType)
        {
            if (triggerType == null)
                _triggerDrawer = null;
            else
                _triggerDrawer = GenericTypeDrawerFactory.Create(triggerType);
        }

        public void Draw()
        {
            var previousColor = Handles.color;
            Handles.color = _currentColor;

            if (IsReentrant())
            {
                var points = GetReentrantLinePoints();
                Handles.DrawAAPolyLine(Width, points);
                DrawArrow(Vector2.Lerp(points[2], points[3], 0.5f), points[3] - points[2]);
            }
            else
            {
                Handles.DrawAAPolyLine(Width,StartPoint, EndPoint);
                DrawArrow(Vector2.Lerp(StartPoint, EndPoint, 0.5f), EndPoint - StartPoint);
            }
                
            
            Handles.color = previousColor;
        }

        private Vector3[] GetReentrantLinePoints()
        {
            int offset = 130;

            return new Vector3[]
            {
                StartPoint,
                new Vector2(StartPoint.x - offset, StartPoint.y),
                new Vector2(StartPoint.x - offset, StartPoint.y - offset),
                new Vector2(StartPoint.x, StartPoint.y - offset),
                StartPoint
            };
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

            Handles.DrawAAConvexPolygon( superiorPoint, inferiorLeftPoint, inferiorRightPoint, superiorPoint);
        }

        public void DrawControls()
        {
            EditorGUILayout.BeginVertical(_controlsAreaStyle);
            
            DrawTransitionParts();
            
            GUILayout.Space(10);

            var previousTrigger = Trigger;
            DrawTriggerField();
            if (previousTrigger != Trigger) OnTriggerChanged?.Invoke(this, previousTrigger, Trigger);

            var previousGuardConditions = GuardConditions;
            DrawGuardConditionsField();
            if (previousGuardConditions != GuardConditions) OnGuardConditionsChanged?.Invoke(this, GuardConditions);

            EditorGUILayout.EndVertical();
        }

        private void DrawTransitionParts()
        {
            if(Source.StateId != null && Trigger != null && Target.StateId != null)
                GUILayout.Label("From State: " + Source.StateId + " -> When Triggered: " + Trigger + " -> To State: " + Target.StateId, _simpleLabelStyle);
        }

        private void DrawTriggerField()
        {
            _triggerDrawer.Draw("Trigger");
        }

        private void DrawGuardConditionsField()
        {
            GuardConditions = DrawGuardConditionArrayField("Guard Conditions", ref _guardConditionsArrayOpened, GuardConditions);
        }

        public bool IsPointOverConnection(Vector2 point)
        {
            return HandleUtility.DistancePointLine(point, StartPoint, EndPoint) <= ClickableExtraRange;
        }

        public ScriptableGuardCondition[] DrawGuardConditionArrayField(string label, ref bool open, ScriptableGuardCondition[] array)
        {
            if (array == null)
                array = new ScriptableGuardCondition[0];

            open = EditorGUILayout.Foldout(open, label);
            int newSize = array.Length;

            if (open)
            {
                newSize = EditorGUILayout.IntField("Size", newSize);
                newSize = newSize < 0 ? 0 : newSize;

                if (newSize != array.Length)
                {
                    array = ResizeArray(array, newSize);
                }

                for (var i = 0; i < newSize; i++)
                {
                    var previousValue = array[i];
                    array[i] = (ScriptableGuardCondition)EditorGUILayout.ObjectField("Value " + i, array[i], typeof(ScriptableGuardCondition), false);
                    if (previousValue != array[i]) OnGuardConditionsChanged?.Invoke(this, GuardConditions);
                }
            }
            return array;
        }

        private T[] ResizeArray<T>(T[] array, int size)
        {
            T[] newArray = new T[size];

            for (var i = 0; i < size; i++)
            {
                if (i < array.Length)
                {
                    newArray[i] = array[i];
                }
            }

            return newArray;
        }

        private bool IsReentrant()
        {
            return Source == Target;
        }

        private Vector2 GetStartPoint()
        {
            Vector2 direction = Target.Center - Source.Center;
            Vector2 normalizedDirection = direction.normalized;

            Vector2 perpendicular = Vector2.Perpendicular(normalizedDirection);

            return Source.Center + (perpendicular * LineOffset);
        }

        private Vector2 GetEndPoint()
        {
            Vector2 direction = Target.Center - Source.Center;
            Vector2 normalizedDirection = direction.normalized;

            Vector2 perpendicular = Vector2.Perpendicular(normalizedDirection);

            return Target.Center + (perpendicular * LineOffset);
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

