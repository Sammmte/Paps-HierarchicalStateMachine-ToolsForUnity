using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class HierarchicalStateMachineBuilderEditorWindow : EditorWindow
    {
        private const string MetadataKey = "PLAIN_STATE_MACHINE_BUILDER_METADATA";

        private static readonly Type DefaultStateIdType = typeof(int);
        private static readonly Type DefaultTriggerType = typeof(int);

        private List<StateNode> _nodes;
        private List<TransitionConnection> _transitions;
        private BackgroundGridDrawer _gridDrawer;
        private BuilderSettingsDrawer _builderSettingsDrawer;
        private WindowEventHandler _windowEventHandler;
        private StateNodeEventHandler _nodeEventHandler;
        private TransitionConnectionEventHandler _transitionConnectionEventHandler;
        private HierarchicalStateMachineBuilderMetadata _metadata;
        private HierarchicalStateMachineBuilder _builder;
        private InspectorDrawer _inspectorDrawer;

        private IInspectable _selectedObject;

        private StateNode _initialNode;

        private TransitionConnectionPreview _transitionPreview;
        
        private static readonly Color backgroundColor = new Color(95f / 255f, 95f / 255f, 95f / 255f);
        private Texture2D backgroundTexture;

        public static void OpenWindow(HierarchicalStateMachineBuilder builder)
        {
            var window = GetWindow<HierarchicalStateMachineBuilderEditorWindow>();
            window.Initialize(builder);
            window.Show();
        }

        private void Initialize(HierarchicalStateMachineBuilder builder)
        {
            if(builder == null)
                return;

            _builder = builder;

            backgroundTexture = CreateBackgroundTexture(backgroundColor);

            titleContent = new GUIContent("Hierarchical State Machine Builder Window");

            _nodes = new List<StateNode>();
            _transitions = new List<TransitionConnection>();

            _gridDrawer = new BackgroundGridDrawer();
            _windowEventHandler = new WindowEventHandler(this);
            _nodeEventHandler = new StateNodeEventHandler(this);
            _transitionConnectionEventHandler = new TransitionConnectionEventHandler(this);
            _metadata = new HierarchicalStateMachineBuilderMetadata();
            _inspectorDrawer = new InspectorDrawer();

            LoadBuilder();

            _builderSettingsDrawer = new BuilderSettingsDrawer(_builder);
            _builderSettingsDrawer.OnStateIdTypeChanged += OnStateIdTypeChanged;
            _builderSettingsDrawer.OnTriggerTypeChanged += OnTriggerTypeChanged;

            Undo.undoRedoPerformed += Reload;
        }
        
        private static Texture2D CreateBackgroundTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();

            return texture;
        }

        private void LoadBuilder()
        {
            if (TryLoadFromBuilderData() == false)
            {
                SetBuilderDefaults();
            }
        }

        private bool TryLoadFromBuilderData()
        {
            if (_builder.StateIdType != null)
            {
                LoadStates();
                LoadTransitions();

                return true;
            }

            return false;
        }

        private void LoadStates()
        {
            var states = _builder.GetStates();

            if (states != null)
            {
                _metadata = _builder.GetMetadata<HierarchicalStateMachineBuilderMetadata>(MetadataKey);

                var initialStateId = _builder.GetInitialStateId();

                for (int i = 0; i < states.Length; i++)
                {
                    var current = states[i];

                    for (int j = 0; j < _metadata.StateNodesMetadata.Count; j++)
                    {
                        if (HierarchicalStateMachineBuilderHelper.AreEquals(_metadata.StateNodesMetadata[j].StateId, current.StateId))
                        {
                            AddNodeFrom(states[i], _metadata.StateNodesMetadata[j]);
                            break;
                        }
                    }
                }

                var initialNode = StateNodeOf(initialStateId);

                if (initialNode != null)
                    SetInitialStateNode(initialNode);
            }
        }

        private void LoadTransitions()
        {
            var transitions = _builder.GetTransitions();

            if (transitions != null)
            {
                for (int i = 0; i < transitions.Length; i++)
                {
                    AddTransitionFrom(transitions[i]);
                }
            }
        }

        private void SetBuilderDefaults()
        {
            _builder.StateIdType = DefaultStateIdType;
            _builder.TriggerType = DefaultTriggerType;

            EditorUtility.SetDirty(_builder);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void OnGUI()
        {
            if (_builder == null)
                return;

            DrawBackground();
            DrawTransitions();
            DrawNodes();
            DrawTransitionPreview();
            DrawBuilderSettings();
            DrawInspector();

            ProcessNodeEvents(Event.current);
            ProcessTransitionEvents(Event.current);
            ProcessWindowEvents(Event.current);

            if (GUI.changed) Repaint();
        }

        private void DrawTransitionPreview()
        {
            if (_transitionPreview != null)
            {
                _transitionPreview.Draw(Event.current.mousePosition);
                GUI.changed = true;
            }
        }

        private void DrawInspector()
        {
            if(HasSomethingSelected())
                _inspectorDrawer.Draw(position, _selectedObject);
        }

        private void DrawBackground()
        {
            GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), backgroundTexture);
            _gridDrawer.Draw(position);
        }

        private void DrawBuilderSettings()
        {
            _builderSettingsDrawer.Draw(position);
        }

        private void DrawNodes()
        {
            BeginWindows();
            if (_nodes.Count > 0)
            {
                for (int i = _nodes.Count - 1; i >= 0; i--)
                {
                    var current = _nodes[i];

                    current.Draw();
                }
            }
            EndWindows();
        }

        private void DrawTransitions()
        {
            if (_transitions.Count > 0)
            {
                for (int i = 0; i < _transitions.Count; i++)
                {
                    var current = _transitions[i];

                    current.Draw();
                }
            }
        }

        public void Drag(Vector2 delta)
        {
            if (_nodes != null)
            {
                for (int i = 0; i < _nodes.Count; i++)
                {
                    _nodes[i].Drag(delta);
                }
            }

            GUI.changed = true;
        }

        private StateNode StateNodeOf(object stateId)
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                var current = _nodes[i];

                if (HierarchicalStateMachineBuilderHelper.AreEquals(current.StateId, stateId))
                    return current;
            }

            return null;
        }

        private void AddNodeFrom(StateInfo stateInfo, StateNodeMetadata metadata)
        {
            var newNode = new StateNode(metadata.Position, _builder.StateIdType, GenerateStateNodeId(), stateInfo.StateObject, stateInfo.StateId);
            InternalAddNode(newNode);
        }

        public void AddNode(Vector2 mousePosition, ScriptableState stateObject = null)
        {
            var newNode = new StateNode(mousePosition, _builder.StateIdType, GenerateStateNodeId(), stateObject);
            InternalAddNode(newNode);
        }

        private int GenerateStateNodeId()
        {
            int id;

            do
            {
                id = Guid.NewGuid().GetHashCode();
            }
            while (ContainsStateNodeWithNodeId(id));

            return id;
        }

        private bool ContainsStateNodeWithNodeId(int nodeId)
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i].NodeId == nodeId)
                    return true;
            }

            return false;
        }

        private void InternalAddNode(StateNode node)
        {
            DoWithUndoAndDirtyFlag(() =>
            {
                node.OnStateIdChanged += (changedNode, previousId, currentId) => RecordAndRebuild();
                node.OnStateObjectChanged += (changedNode, previousStateObj, currentStateObj) => RecordAndRebuild();
                node.OnPositionChanged += UpdateNodePositionMetadata;
            
                _nodes.Add(node);

                if (_nodes.Count == 1)
                    SetInitialStateNode(node);
            });
        }

        public void AddTransition(StateNode source, StateNode target)
        {
            DoWithUndoAndDirtyFlag(() =>
            {
                if(ContainsTransitionWithSourceAndTarget(source, target))
                    return;
            
                var newTransition = new TransitionConnection(source, target, _builder.TriggerType);

                newTransition.OnTriggerChanged += (connection, previousTrigger, currentTrigger) => RecordAndRebuild();
                newTransition.OnGuardConditionsChanged += (connection, currentGuardConditions) => RecordAndRebuild();

                _transitions.Add(newTransition);
            });
        }

        public void AddTransitionFrom(TransitionInfo transitionInfo)
        {
            var source = StateNodeOf(transitionInfo.StateFrom);
            var target = StateNodeOf(transitionInfo.StateTo);
            
            var newTransition = new TransitionConnection(source, target, _builder.TriggerType, transitionInfo.Trigger, transitionInfo.GuardConditions);

            newTransition.OnTriggerChanged += (connection, previousTrigger, currentTrigger) => RecordAndRebuild();
            newTransition.OnGuardConditionsChanged += (connection, currentGuardConditions) => RecordAndRebuild();
            
            _transitions.Add(newTransition);
        }

        private bool ContainsTransitionWithSourceAndTarget(StateNode source, StateNode target)
        {
            for (int i = 0; i < _transitions.Count; i++)
            {
                if (_transitions[i].Source == source && _transitions[i].Target == target)
                    return true;
            }

            return false;
        }

        private void RecordAndRebuild()
        {
            DoWithUndoAndDirtyFlag(() =>
            {
                Rebuild();
            });
        }

        public void RemoveNode(StateNode node)
        {
            if (_nodes.Remove(node))
            {
                if(IsSelected(node))
                    DeselectAll();
                
                RecordAndRebuild();
            }
        }

        public void RemoveTransition(TransitionConnection transition)
        {
            if (_transitions.Remove(transition))
            {
                if (IsSelected(transition))
                    DeselectAll();
                
                RecordAndRebuild();
            }
        }

        private void UpdateNodePositionMetadata(StateNode node, Vector2 position)
        {
            DoWithUndoAndDirtyFlag(() =>
            {
                RebuildMetadata();
            });
        }

        private void RebuildMetadata()
        {
            _metadata.StateNodesMetadata.Clear();

            _builder.RemoveMetadata(MetadataKey);

            var states = _builder.GetStates();

            if (states == null) return;

            for (int i = 0; i < states.Length; i++)
            {
                StateInfo currentState = states[i];
                StateNode currentStateNode = GetNodeOf(currentState);

                _metadata.StateNodesMetadata.Add(new StateNodeMetadata(currentState.StateId, currentStateNode.Position));
            }

            _builder.SaveMetadata(MetadataKey, _metadata);
        }

        private StateNode GetNodeOf(StateInfo stateInfo)
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                var current = _nodes[i];

                if (HierarchicalStateMachineBuilderHelper.AreEquals(current.StateId, stateInfo.StateId))
                    return current;
            }

            return null;
        }

        private void OnStateIdTypeChanged(Type newType)
        {
            DoWithUndoAndDirtyFlag(() =>
            {
                _builder.StateIdType = newType;

                for (int i = 0; i < _nodes.Count; i++)
                {
                    _nodes[i].SetNewStateIdType(newType);
                }

                RebuildMetadata();
            });
        }

        private void OnTriggerTypeChanged(Type newType)
        {
            DoWithUndoAndDirtyFlag(() =>
            {
                _builder.TriggerType = newType;

                for(int i = 0; i < _transitions.Count; i++)
                {
                    _transitions[i].SetNewTriggerType(newType);
                }

                RebuildMetadata();
            });
        }

        private void ProcessWindowEvents(Event ev)
        {
            _windowEventHandler.HandleEvent(ev);
        }

        private void ProcessNodeEvents(Event e)
        {
            if (_nodes.Count > 0)
            {
                for (int i = _nodes.Count - 1; i >= 0; i--)
                {
                    var currentNode = _nodes[i];

                    _nodeEventHandler.HandleEventFor(currentNode, e);
                }
            }
        }

        private void ProcessTransitionEvents(Event e)
        {
            if(_transitions.Count > 0)
            {
                for(int i = 0; i < _transitions.Count; i++)
                {
                    var current = _transitions[i];

                    _transitionConnectionEventHandler.HandleEventFor(current, e);
                }
            }
        }

        public void SelectNode(StateNode node)
        {
            DeselectAll();
            
            for (int i = 0; i < _nodes.Count; i++)
            {
                var currentNode = _nodes[i];

                if (currentNode == node)
                {
                    GUI.FocusControl(null);
                    _selectedObject = currentNode;
                    currentNode.Select();
                }

            }

            Repaint();
        }

        public void DeselectAll()
        {
            if (_selectedObject is StateNode node)
                node.Deselect();
            else if (_selectedObject is TransitionConnection transition)
                transition.Deselect();

            _selectedObject = null;

            Repaint();
        }

        public void SelectTransition(TransitionConnection transition)
        {
            DeselectAll();
            
            for (int i = 0; i < _transitions.Count; i++)
            {
                var currentTransition = _transitions[i];

                if (currentTransition == transition)
                {
                    GUI.FocusControl(null);
                    _selectedObject = currentTransition;
                    currentTransition.Select();
                }
            }

            Repaint();
        }

        public void SetInitialStateNode(StateNode node)
        {
            _nodes.ForEach(n => n.AsNormal());

            _initialNode = node;
            _initialNode.AsInitial();

            _builder.SetInitialState(_initialNode.StateId);
        }

        private void Reload()
        {
            Undo.undoRedoPerformed -= Reload;

            Initialize(_builder);

            EditorUtility.SetDirty(_builder);
            Repaint();
        }

        private void DoWithUndoAndDirtyFlag(Action action)
        {
            Undo.RecordObject(_builder, _builder.name);
            action();
            EditorUtility.SetDirty(_builder);
        }

        public bool IsSelected(StateNode node)
        {
            return object.ReferenceEquals(_selectedObject, node);
        }

        public bool IsSelected(TransitionConnection transition)
        {
            return object.ReferenceEquals(_selectedObject, transition);
        }

        public bool IsInitial(StateNode node)
        {
            return _initialNode == node;
        }

        public bool HasSelectedNode()
        {
            return _selectedObject != null && _selectedObject is StateNode;
        }

        public bool HasSelectedTransition()
        {
            return _selectedObject != null && _selectedObject is TransitionConnection;
        }

        public bool HasSomethingSelected()
        {
            return _selectedObject != null;
        }

        public void BeginTransitionPreviewFrom(StateNode source)
        {
            _transitionPreview = new TransitionConnectionPreview(source);
        }

        public void EndTransitionPreview()
        {
            _transitionPreview = null;
        }

        public bool HasTransitionPreview()
        {
            return _transitionPreview != null;
        }

        public StateNode GetSourceNodeFromTransitionPreview()
        {
            if (HasTransitionPreview())
                return _transitionPreview.Source;
            else 
                return null;
        }

        private void Rebuild()
        {
            ClearBuilder();
            RebuildStates();
            RebuildTransitions();
            RebuildMetadata();
            RebuildInitialState();
        }

        private void RebuildInitialState()
        {
            var initialState = _builder.GetInitialStateId();

            if (initialState != null)
                SetInitialStateNode(StateNodeOf(_builder.GetInitialStateId()));
            else if (_nodes.Count > 0)
                SetInitialStateNode(_nodes[0]);
        }

        private void ClearBuilder()
        {
            _builder.RemoveAllTransitions();
            _builder.RemoveAllStates();
        }

        private void RebuildStates()
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                _builder.AddState(_nodes[i].StateId, _nodes[i].StateObject);
                
                if(IsInitial(_nodes[i]))
                    SetInitialStateNode(_nodes[i]);
            }
        }

        private void RebuildTransitions()
        {
            for (int i = 0; i < _transitions.Count; i++)
            {
                _builder.AddTransition(_transitions[i].StateFrom, _transitions[i].Trigger, _transitions[i].StateTo,
                    _transitions[i].GuardConditions);
            }
        }
    }
}