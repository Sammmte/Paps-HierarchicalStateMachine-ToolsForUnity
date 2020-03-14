using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class InspectorDrawer
    {
        private const float Width = 300, Height = 400, LeftPadding = 10, TopPadding = 10;

        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _controlsAreaStyle;

        public InspectorDrawer()
        {
            _titleStyle = new GUIStyle();
            _titleStyle.padding = new RectOffset(20, 20, 20, 20);
            _titleStyle.alignment = TextAnchor.MiddleCenter;
            _titleStyle.fontSize = 20;
            _titleStyle.wordWrap = true;

            _labelStyle = new GUIStyle();
            _labelStyle.wordWrap = true;

            _controlsAreaStyle = new GUIStyle();
            _controlsAreaStyle.padding = new RectOffset(20, 20, 20, 20);
        }

        public void Draw(Rect windowRect, ISelectable inspectable)
        {
            var position = new Vector2(LeftPadding, TopPadding);
            var size = new Vector2(Width, Height);

            var boxRect = new Rect(position, size);

            var previousColor = GUI.color;
            GUI.color = Color.gray;
            GUILayout.BeginArea(boxRect, GUI.skin.window);
            GUI.color = previousColor;

            DrawTitle();
            inspectable?.DrawControls();
            GUILayout.EndArea();
        }

        private void DrawTitle()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Element Inspector", _titleStyle);
            GUILayout.EndVertical();
        }
    }
}