using UnityEditor;
using UnityEngine;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class TransitionConnectionPreview
    {
        private const float Width = 4f, ArrowWidthExtent = 8, ArrowHeightExtent = 8;

        public StateNode Source { get; private set; }

        private Vector2 StartPoint => Source.Center;

        public TransitionConnectionPreview(StateNode source)
        {
            Source = source;
        }

        public void Draw(Vector2 currentEndPoint)
        {
            var previousColor = Handles.color;
            Handles.color = Color.white;
            Handles.DrawAAPolyLine(Width, StartPoint, currentEndPoint);
            DrawArrow(Vector2.Lerp(StartPoint, currentEndPoint, 0.5f), currentEndPoint - StartPoint);
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

            Handles.DrawAAConvexPolygon( superiorPoint, inferiorLeftPoint, inferiorRightPoint, superiorPoint);
        }
    }
}