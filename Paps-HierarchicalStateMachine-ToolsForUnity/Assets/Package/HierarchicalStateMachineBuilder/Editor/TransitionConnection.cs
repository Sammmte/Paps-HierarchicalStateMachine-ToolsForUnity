using UnityEditor;
using UnityEngine;
using System;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using System.Collections.Generic;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class TriggerWithGuardConditions
    {
        private GenericTypeDrawer _triggerDrawer;
        public object Trigger => _triggerDrawer.Value;
        public ScriptableGuardCondition[] GuardConditions { get; private set; }
        public Action<TriggerWithGuardConditions> OnTriggerChanged;
        public Action<TriggerWithGuardConditions> OnGuardConditionsChanged;
        private bool _guardConditionsArrayOpened;

        public TriggerWithGuardConditions(Type triggerType, object trigger = null, ScriptableGuardCondition[] guardConditions = null)
        {
            if(trigger == null)
                _triggerDrawer = GenericTypeDrawerFactory.Create(triggerType);
            else
                _triggerDrawer = GenericTypeDrawerFactory.Create(triggerType, trigger);

            if (guardConditions == null)
                GuardConditions = new ScriptableGuardCondition[0];
            else
            {
                GuardConditions = new ScriptableGuardCondition[guardConditions.Length];

                guardConditions.CopyTo(GuardConditions, 0);
            }
        }

        public void Draw()
        {
            var previousTrigger = Trigger;
            DrawTriggerField();
            if (previousTrigger != Trigger) OnTriggerChanged?.Invoke(this);

            var previousGuardConditions = GuardConditions;
            DrawGuardConditionsField();
            if (previousGuardConditions != GuardConditions) OnGuardConditionsChanged?.Invoke(this);
        }

        public void SetNewTriggerType(Type triggerType)
        {
            _triggerDrawer = GenericTypeDrawerFactory.Create(triggerType);
        }

        private void DrawTriggerField()
        {
            _triggerDrawer.Draw("Trigger");
        }

        private void DrawGuardConditionsField()
        {
            GuardConditions = DrawGuardConditionArrayField("Guard Conditions", ref _guardConditionsArrayOpened, GuardConditions);
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
                    if (previousValue != array[i]) OnGuardConditionsChanged?.Invoke(this);
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
    }

    internal class TransitionConnection : ISelectable
    {
        private const float Width = 4f, ClickableExtraRange = 8f, ArrowWidthExtent = 8, ArrowHeightExtent = 8, LineOffset = 7;

        private const int ControlPaddingLeft = 20, ControlPaddingRight = 20, ControlPaddingTop = 20, ControlPaddingBottom = 20;

        private static readonly Color SelectedColor = new Color(44f / 255f, 130f / 255f, 201f / 255f);
        private static readonly Color NormalColor = Color.white;
        
        private bool _triggersListOpened;
        private Vector2 _scrollPosition;
        private GUIStyle _controlsAreaStyle;
        private GUIStyle _simpleLabelStyle;

        private Color _currentColor;

        public Vector2 StartPoint => GetStartPoint();
        public Vector2 EndPoint => GetEndPoint();
        private List<TriggerWithGuardConditions> _triggersWithGuardConditionsList = new List<TriggerWithGuardConditions>();
        public TriggerWithGuardConditions[] TriggersWithGuardConditions => _triggersWithGuardConditionsList.ToArray();
        public Action<TransitionConnection> OnTriggersWithGuardConditionsChanged;

        public StateNode Source { get; private set; }
        public StateNode Target { get; private set; }

        public object StateFrom => Source.StateId;
        public object StateTo => Target.StateId;

        private Type _triggerType;

        public TransitionConnection(StateNode source, StateNode target, Type triggerType, Dictionary<object, ScriptableGuardCondition[]> triggersWithGuardConditions = null)
        {
            Source = source;
            Target = target;

            _triggerType = triggerType;

            if (triggersWithGuardConditions != null)
            {
                foreach (var triggerWithGuardConditions in triggersWithGuardConditions)
                {
                    var t = new TriggerWithGuardConditions(_triggerType, triggerWithGuardConditions.Key, triggerWithGuardConditions.Value);
                    t.OnTriggerChanged += CallOnTriggersWithGuardConditionsChangedEvent;
                    t.OnGuardConditionsChanged += CallOnTriggersWithGuardConditionsChangedEvent;

                    _triggersWithGuardConditionsList.Add(t);
                }
                    
            }
            
            _controlsAreaStyle = new GUIStyle();
            _controlsAreaStyle.padding = new RectOffset(ControlPaddingLeft, ControlPaddingRight, ControlPaddingTop, ControlPaddingBottom);

            _simpleLabelStyle = new GUIStyle();
            _simpleLabelStyle.wordWrap = true;

            _currentColor = NormalColor;
        }

        private void CallOnTriggersWithGuardConditionsChangedEvent(TriggerWithGuardConditions triggerWithGuardConditions)
        {
            OnTriggersWithGuardConditionsChanged?.Invoke(this);
        }

        public void SetNewTriggerType(Type triggerType)
        {
            _triggerType = triggerType;

            foreach (var triggerWithGuardCondtions in _triggersWithGuardConditionsList)
                triggerWithGuardCondtions.SetNewTriggerType(triggerType);
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

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            _triggersWithGuardConditionsList = DrawTriggerWithGuardConditionsArrayField("Triggers", ref _triggersListOpened, _triggersWithGuardConditionsList);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        public bool IsPointOverConnection(Vector2 point)
        {
            if (IsReentrant())
            {
                var points = GetReentrantLinePoints();

                for(int i = 1; i < points.Length; i++)
                {
                    if (HandleUtility.DistancePointLine(point, points[i - 1], points[i]) <= ClickableExtraRange)
                        return true;
                }

                return false;
            }
            else
                return HandleUtility.DistancePointLine(point, StartPoint, EndPoint) <= ClickableExtraRange;
        }

        private List<TriggerWithGuardConditions> DrawTriggerWithGuardConditionsArrayField(string label, ref bool open, List<TriggerWithGuardConditions> list)
        {
            open = EditorGUILayout.Foldout(open, label);
            int newSize = list.Count;

            if(open)
            {
                newSize = EditorGUILayout.IntField("Size", newSize);
                newSize = newSize < 0 ? 0 : newSize;

                if(newSize != list.Count)
                {
                    list = ResizeList(list, newSize);
                }

                for(int i = 0; i < newSize; i++)
                {
                    list[i].Draw();
                }
            }

            return list;
        }

        private List<TriggerWithGuardConditions> ResizeList(List<TriggerWithGuardConditions> list, int size)
        {
            List<TriggerWithGuardConditions> newList = new List<TriggerWithGuardConditions>();

            for (var i = 0; i < size; i++)
            {
                if (i < list.Count)
                {
                    newList.Add(list[i]);
                }
                else
                {
                    var t = new TriggerWithGuardConditions(_triggerType);
                    t.OnTriggerChanged += CallOnTriggersWithGuardConditionsChangedEvent;
                    t.OnGuardConditionsChanged += CallOnTriggersWithGuardConditionsChangedEvent;

                    newList.Add(t);
                }
            }

            return newList;
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

