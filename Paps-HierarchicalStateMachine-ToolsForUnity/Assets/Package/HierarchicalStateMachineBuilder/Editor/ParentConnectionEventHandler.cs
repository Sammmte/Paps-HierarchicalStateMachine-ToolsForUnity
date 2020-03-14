using UnityEditor;
using UnityEngine;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class ParentConnectionEventHandler
    {
        private HierarchicalStateMachineBuilderEditorWindow _window;

        public ParentConnectionEventHandler(HierarchicalStateMachineBuilderEditorWindow window)
        {
            _window = window;
        }

        public void HandleEventFor(ParentConnection parentConnection, Event nodeEvent)
        {
            switch (nodeEvent.type)
            {
                case EventType.MouseDown:

                    if (IsLeftMouseClick(nodeEvent.button))
                    {
                        if (parentConnection.IsPointOverConnection(nodeEvent.mousePosition))
                        {
                            _window.Select(parentConnection);
                            nodeEvent.Use();
                        }
                    }
                    else if (IsRightMouseClick(nodeEvent.button) && _window.IsSelected(parentConnection))
                    {
                        DisplayNodeOptionsAtPosition(parentConnection);
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

        private void DisplayNodeOptionsAtPosition(ParentConnection parentConnection)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove child from parent"), false, () => _window.RemoveChildFromParent(parentConnection.Child));
            genericMenu.ShowAsContext();
        }
    }
}