using UnityEditor;
using UnityEngine;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class TransitionConnectionEventHandler
    {
        private HierarchicalStateMachineBuilderEditorWindow _window;

        public TransitionConnectionEventHandler(HierarchicalStateMachineBuilderEditorWindow window)
        {
            _window = window;
        }

        public void HandleEventFor(TransitionConnection transition, Event nodeEvent)
        {
            switch (nodeEvent.type)
            {
                case EventType.MouseDown:

                    if (IsLeftMouseClick(nodeEvent.button))
                    {
                        if (transition.IsPointOverConnection(nodeEvent.mousePosition))
                        {
                            _window.SelectTransition(transition);
                            nodeEvent.Use();
                        }
                    }
                    else if (IsRightMouseClick(nodeEvent.button) && _window.IsSelected(transition))
                    {
                        DisplayNodeOptionsAtPosition(transition);
                        nodeEvent.Use();
                    }

                    break;
            }
        }

        private bool IsLeftMouseClick(int button)
        {
            return button == 0;
        }

        private bool IsRightMouseClick(int button)
        {
            return button == 1;
        }

        private void DisplayNodeOptionsAtPosition(TransitionConnection transition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove transition"), false, () => _window.RemoveTransition(transition));
            genericMenu.ShowAsContext();
        }
    }

}