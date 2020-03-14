using UnityEditor;
using UnityEngine;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class StateNodeEventHandler
    {
        private HierarchicalStateMachineBuilderEditorWindow _window;

        public StateNodeEventHandler(HierarchicalStateMachineBuilderEditorWindow window)
        {
            _window = window;
        }

        public void HandleEventFor(StateNode node, Event nodeEvent)
        {
            switch (nodeEvent.type)
            {
                case EventType.MouseDown:

                    if (IsLeftMouseClick(nodeEvent.button))
                    {
                        if (node.IsPointOverNode(nodeEvent.mousePosition))
                        {
                            if (_window.HasTransitionPreview())
                            {
                                _window.AddTransition(_window.GetSourceNodeFromTransitionPreview(), node);
                                _window.EndTransitionPreview();
                            }
                            else if(_window.HasParentConnectionPreview())
                            {
                                _window.AddChildTo(_window.GetParentNodeFromParentConnectionPreview(), node);
                                _window.EndParentConnectionPreview();
                            }

                            _window.Select(node);
                            nodeEvent.Use();
                        }
                            
                    }
                    else if (IsRightMouseClick(nodeEvent.button) && _window.IsSelected(node))
                    {
                        DisplayNodeOptionsAtPosition(node);
                        nodeEvent.Use();
                    }

                    break;
                
                case EventType.MouseDrag:

                    if (IsLeftMouseClick(nodeEvent.button) && _window.IsSelected(node))
                    {
                        node.Drag(nodeEvent.delta);
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

        private void DisplayNodeOptionsAtPosition(StateNode node)
        {
            GenericMenu genericMenu = new GenericMenu();

            if(_window.HasParent(node))
                genericMenu.AddItem(new GUIContent("Set as initial child"), false, () => _window.SetInitialChildNodeOf(_window.GetParentOf(node), node));
            else
                genericMenu.AddItem(new GUIContent("Set as initial state"), false, () => _window.SetInitialStateNode(node));
                
            genericMenu.AddItem(new GUIContent("Add transition"), false, () => _window.BeginTransitionPreviewFrom(node));
            genericMenu.AddItem(new GUIContent("Add child state"), false, () => _window.BeginParentConnectionPreviewFrom(node));
            genericMenu.AddItem(new GUIContent("Remove node"), false, () => _window.RemoveNode(node));
            genericMenu.ShowAsContext();
        }
    }
}


