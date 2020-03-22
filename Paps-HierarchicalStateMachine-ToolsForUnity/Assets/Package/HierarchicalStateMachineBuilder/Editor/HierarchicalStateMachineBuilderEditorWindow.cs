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
        private List<ParentConnection> _parentConnections;
        private BackgroundGridDrawer _gridDrawer;
        private BuilderSettingsDrawer _builderSettingsDrawer;
        private WindowEventHandler _windowEventHandler;
        private StateNodeEventHandler _nodeEventHandler;
        private TransitionConnectionEventHandler _transitionConnectionEventHandler;
        private ParentConnectionEventHandler _parentConnectionEventHandler;
        private HierarchicalStateMachineBuilderMetadata _metadata;
        private HierarchicalStateMachineBuilder _builder;
        private InspectorDrawer _inspectorDrawer;

        private ISelectable _selectedObject;

        private TransitionConnectionPreview _transitionPreview;
        private ParentConnectionPreview _parentConnectionPreview;
        
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
            LoadBackground();

            if(builder == null)
                return;

            _builder = builder;

            Load();
        }

        private void OnEnable()
        {
            LoadBackground();

            if (_builder == null)
                return;

            Load();
        }

        private void Load()
        {
            titleContent = new GUIContent("Hierarchical State Machine Builder Window");

            _nodes = new List<StateNode>();
            _transitions = new List<TransitionConnection>();
            _parentConnections = new List<ParentConnection>();

            _windowEventHandler = new WindowEventHandler(this);
            _nodeEventHandler = new StateNodeEventHandler(this);
            _transitionConnectionEventHandler = new TransitionConnectionEventHandler(this);
            _parentConnectionEventHandler = new ParentConnectionEventHandler(this);
            _metadata = new HierarchicalStateMachineBuilderMetadata();
            _inspectorDrawer = new InspectorDrawer();

            LoadBuilder();

            _builderSettingsDrawer = new BuilderSettingsDrawer(_builder);
            _builderSettingsDrawer.OnStateIdTypeChanged += OnStateIdTypeChanged;
            _builderSettingsDrawer.OnTriggerTypeChanged += OnTriggerTypeChanged;

            Undo.undoRedoPerformed += Reload;
        }

        private void LoadBackground()
        {
            _gridDrawer = new BackgroundGridDrawer();
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
                LoadParentConnections();

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

        private void LoadParentConnections()
        {
            for(int i = 0; i < _nodes.Count; i++)
            {
                var childs = _builder.GetChildsOf(_nodes[i].StateId);
                var initialChildId = _builder.GetInitialChildOf(_nodes[i].StateId);

                if(childs != null)
                {
                    for (int j = 0; j < childs.Length; j++)
                    {
                        var childNode = StateNodeOf(childs[j]);

                        if (HierarchicalStateMachineBuilderHelper.AreEquals(childs[j], initialChildId))
                            childNode.AsInitialChild();

                        AddParentConnectionFrom(_nodes[i], childNode);
                    }
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
            DrawBackground();

            if (_builder == null)
                return;
            
            DrawParentConnections();
            DrawTransitions();
            DrawNodes();
            DrawTransitionPreview();
            DrawParentConnectionPreview();
            DrawBuilderSettings();
            DrawInspector();

            ProcessNodeEvents(Event.current);
            ProcessTransitionEvents(Event.current);
            ProcessParentConnectionEvents(Event.current);
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

        private void DrawParentConnectionPreview()
        {
            if(HasParentConnectionPreview())
            {
                _parentConnectionPreview.Draw(Event.current.mousePosition);
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
            if(backgroundTexture == null)
                backgroundTexture = CreateBackgroundTexture(backgroundColor);

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

        private void DrawParentConnections()
        {
            if(_parentConnections.Count > 0)
            {
                for(int i = 0; i < _parentConnections.Count; i++)
                {
                    var current = _parentConnections[i];

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
                RemoveTransitionsRelatedTo(node);
                RemoveParentConnectionsRelatedTo(node);

                if (node.IsInitial)
                    SetInitialStateNode(GetRoots()[0]);

                if(IsSelected(node))
                    DeselectAll();
                
                RecordAndRebuild();
            }
        }

        private void RemoveTransitionsRelatedTo(StateNode node)
        {
            for(int i = 0; i < _transitions.Count; i++)
            {
                if(_transitions[i].Source == node || _transitions[i].Target == node)
                {
                    RemoveTransition(_transitions[i]);
                    i--;
                }
            }
        }

        private void RemoveParentConnectionsRelatedTo(StateNode node)
        {
            RemoveChildFromParent(node);

            for(int i = 0; i < _parentConnections.Count; i++)
            {
                if(node == _parentConnections[i].Parent)
                {
                    RemoveChildFromParent(_parentConnections[i].Child);
                    i--;
                }
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

        private void ProcessParentConnectionEvents(Event e)
        {
            if(_parentConnections.Count > 0)
            {
                for(int i = 0; i < _parentConnections.Count; i++)
                {
                    var current = _parentConnections[i];

                    _parentConnectionEventHandler.HandleEventFor(current, e);
                }
            }
        }

        public void Select(ISelectable selectable)
        {
            DeselectAll();

            GUI.FocusControl(null);
            _selectedObject = selectable;
            selectable.Select();

            Repaint();
        }

        public void DeselectAll()
        {
            if(_selectedObject != null)
                _selectedObject.Deselect();

            _selectedObject = null;

            Repaint();
        }

        public void SetInitialStateNode(StateNode node)
        {
            _nodes.ForEach(n =>
            {
                if(n.IsInitial)
                    n.AsNormal();
            });

            node.AsInitial();
        }

        private void Reload()
        {
            Undo.undoRedoPerformed -= Reload;

            Load();

            EditorUtility.SetDirty(_builder);
            Repaint();
        }

        private void DoWithUndoAndDirtyFlag(Action action)
        {
            Undo.RecordObject(_builder, _builder.name);
            action();
            EditorUtility.SetDirty(_builder);
        }

        public bool IsSelected(ISelectable selectable)
        {
            return _selectedObject == selectable;
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

        public void BeginParentConnectionPreviewFrom(StateNode parent)
        {
            _parentConnectionPreview = new ParentConnectionPreview(parent);
        }

        public void EndParentConnectionPreview()
        {
            _parentConnectionPreview = null;
        }

        public bool HasParentConnectionPreview()
        {
            return _parentConnectionPreview != null;
        }

        public StateNode GetParentNodeFromParentConnectionPreview()
        {
            if (HasParentConnectionPreview())
                return _parentConnectionPreview.Parent;
            else
                return null;
        }

        private void Rebuild()
        {
            ClearBuilder();
            RebuildStates();
            RebuildTransitions();
            RebuildParentConnections();
            RebuildMetadata();
        }

        private void ClearBuilder()
        {
            _builder.RemoveAllParentConnections();
            _builder.RemoveAllTransitions();
            _builder.RemoveAllStates();
        }

        private void RebuildStates()
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                _builder.AddState(_nodes[i].StateId, _nodes[i].StateObject);

                if (_nodes[i].IsInitial)
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

        private void RebuildParentConnections()
        {
            for(int i = 0; i < _parentConnections.Count; i++)
            {
                var current = _parentConnections[i];

                _builder.AddChildTo(current.Parent.StateId, current.Child.StateId);

                if (current.Child.IsInitialChild)
                    SetInitialChildNodeOf(current.Parent, current.Child);
            }
        }

        public void SetInitialChildNodeOf(StateNode parent, StateNode initialChild)
        {
            var parentConnections = _parentConnections.Where(connection => connection.Parent == parent);

            foreach(var connection in parentConnections)
            {
                connection.Child.AsNormal();
            }

            initialChild.AsInitialChild();

            _builder.SetInitialChildOf(parent.StateId, initialChild.StateId);
        }

        public void AddChildTo(StateNode parent, StateNode child)
        {
            if (parent == child || HasParent(child) || GetParentOf(parent) == child)
                return;

            _parentConnections.Add(new ParentConnection(parent, child));

            if (child.IsInitial)
                SetInitialStateNode(GetRoots()[0]);

            if (_parentConnections.Where(connection => connection.Parent == parent).Count() == 1)
                SetInitialChildNodeOf(parent, child);

            RecordAndRebuild();
        }

        private List<StateNode> GetRoots()
        {
            List<StateNode> roots = new List<StateNode>();

            for(int i = 0; i < _nodes.Count; i++)
            {
                if (HasParent(_nodes[i]) == false)
                    roots.Add(_nodes[i]);
            }

            if (roots.Count == 0)
                return null;
            else
                return roots;
        }

        public void AddParentConnectionFrom(StateNode parent, StateNode child)
        {
            _parentConnections.Add(new ParentConnection(parent, child));
        }

        public void RemoveChildFromParent(StateNode child)
        {
            for(int i = 0; i < _parentConnections.Count; i++)
            {
                if (_parentConnections[i].Child == child)
                {
                    child.AsNormal();

                    if (IsSelected(_parentConnections[i]))
                        DeselectAll();

                    _parentConnections.RemoveAt(i);

                    RecordAndRebuild();
                    break;
                }
            }
        }

        public bool HasParent(StateNode node)
        {
            for(int i = 0; i < _parentConnections.Count; i++)
            {
                var current = _parentConnections[i];

                if (current.Child == node)
                    return true;
            }

            return false;
        }

        public StateNode GetParentOf(StateNode node)
        {
            for (int i = 0; i < _parentConnections.Count; i++)
            {
                var current = _parentConnections[i];

                if (current.Child == node)
                    return current.Parent;
            }

            return null;
        }
    }
}