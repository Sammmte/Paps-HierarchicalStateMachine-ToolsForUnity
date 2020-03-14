using UnityEngine;
using System;
using Paps.StateMachines;
using Paps.StateMachines.Extensions;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Paps.HierarchicalStateMachine_ToolsForUnity.Editor")]
namespace Paps.HierarchicalStateMachine_ToolsForUnity
{
    [CreateAssetMenu(menuName = "Paps/State Machine Builders/Hierarchical State Machine Builder")]
    public sealed class HierarchicalStateMachineBuilder : ScriptableObject
    {
        internal Type StateIdType
        {
            get
            {
                if (_stateIdType == null)
                    _stateIdType = HierarchicalStateMachineBuilderHelper.GetTypeOf(_stateIdTypeFullName);

                return _stateIdType;
            }

            set
            {
                _stateIdType = value;
                _stateIdTypeFullName = _stateIdType.FullName;
                RemoveAllTransitions();
                RemoveAllStates();
            }
        }

        internal Type TriggerType
        {
            get
            {
                if (_triggerType == null)
                    _triggerType = HierarchicalStateMachineBuilderHelper.GetTypeOf(_triggerTypeFullName);

                return _triggerType;
            }

            set
            {
                _triggerType = value;
                _triggerTypeFullName = _triggerType.FullName;
                RemoveAllTransitions();
            }
        }
        
        [SerializeField]
        private List<StateInfo> _states;

        [SerializeField] 
        private List<TransitionInfo> _transitions;

        [SerializeField]
        private string _stateIdTypeFullName, _triggerTypeFullName;

        [SerializeField]
        private List<Metadata> _metadata;

        private Type _stateIdType;
        private Type _triggerType;

        [SerializeField]
        private string _serializedInitialStateId;

        private object InitialStateId
        {
            get
            {
                if (string.IsNullOrEmpty(_serializedInitialStateId) == false)
                    return GenericTypeSerializer.Deserialize(_serializedInitialStateId, StateIdType);
                else
                    return null;
            }
        }

        private void Awake()
        {
            if(_states == null)
                _states = new List<StateInfo>();
            
            if(_transitions == null)
                _transitions = new List<TransitionInfo>();

            if (_metadata == null)
                _metadata = new List<Metadata>();
        }

        internal void AddState(object stateId, ScriptableState stateObject)
        {
            if(stateId == null)
                return;
            
            if (StateIdType != stateId.GetType())
                return;

            if (ContainsState(stateId))
                return;

            _states.Add(new StateInfo(stateId, stateObject));

            if (_states.Count == 1)
                SetInitialState(stateId);
        }

        internal StateInfo StateInfoOf(object stateId)
        {
            for(int i = 0; i < _states.Count; i++)
            {
                var current = _states[i];

                if (HierarchicalStateMachineBuilderHelper.AreEquals(stateId, current.StateId))
                    return current;
            }

            return null;
        }

        internal void SetInitialState(object stateId)
        {
            if(ContainsState(stateId))
            {
                _serializedInitialStateId = GenericTypeSerializer.Serialize(stateId);
            }
        }

        internal object GetInitialStateId()
        {
            return InitialStateId;
        }

        internal void RemoveState(object stateId)
        {
            if(ContainsState(stateId))
            {
                for(int i = 0; i < _states.Count; i++)
                {
                    var current = _states[i];

                    if(HierarchicalStateMachineBuilderHelper.AreEquals(stateId, current.StateId))
                    {
                        _states.RemoveAt(i);

                        if (HierarchicalStateMachineBuilderHelper.AreEquals(current.StateId, InitialStateId))
                            SetInitialDefaultStateIfThereIsAny();
                    }
                }
            }
        }

        private void SetInitialDefaultStateIfThereIsAny()
        {
            if (_states.Count > 0)
                SetInitialState(_states[0].StateId);
            else
                _serializedInitialStateId = "";
        }

        internal bool ContainsState(object stateId)
        {
            if(stateId != null)
            {
                for (int i = 0; i < _states.Count; i++)
                {
                    var current = _states[i];

                    if (HierarchicalStateMachineBuilderHelper.AreEquals(stateId, current.StateId))
                        return true;
                }
            }

            return false;
        }

        internal StateInfo[] GetStates()
        {
            if (_states.Count > 0)
                return _states.ToArray();
            else
                return null;
        }

        internal void RemoveAllStates()
        {
            _states.Clear();
            SetInitialDefaultStateIfThereIsAny();
        }

        internal void AddTransition(object StateFrom, object Trigger, object StateTo, ScriptableGuardCondition[] guardConditions)
        {
            if(StateFrom == null || Trigger == null || StateTo == null)
                return;

            if (StateIdType != StateFrom.GetType() ||
                StateIdType != StateTo.GetType() ||
                TriggerType != Trigger.GetType())
                return;
            
            if(ContainsState(StateFrom) == false || ContainsState(StateTo) == false)
                return;
            
            if(ContainsTransition(StateFrom, Trigger, StateTo))
                return;
            
            var newTransition = new TransitionInfo(StateFrom, Trigger, StateTo, guardConditions);
            
            _transitions.Add(newTransition);
        }

        internal void RemoveTransition(TransitionInfo transitionInfo)
        {
            _transitions.Remove(transitionInfo);
        }

        internal TransitionInfo[] GetTransitions()
        {
            if (_transitions.Count > 0)
                return _transitions.ToArray();
            else
                return null;
        }

        internal bool ContainsTransition(TransitionInfo transition)
        {
            for (int i = 0; i < _transitions.Count; i++)
            {
                var current = _transitions[i];

                if (HierarchicalStateMachineBuilderHelper.AreEquals(current.StateFrom, transition.StateFrom) &&
                    HierarchicalStateMachineBuilderHelper.AreEquals(current.Trigger, transition.Trigger) &&
                    HierarchicalStateMachineBuilderHelper.AreEquals(current.StateTo, transition.StateTo))
                {
                    return true;
                }
            }

            return false;
        }

        internal bool ContainsTransition(object StateFrom, object Trigger, object StateTo)
        {
            for (int i = 0; i < _transitions.Count; i++)
            {
                var current = _transitions[i];

                if (HierarchicalStateMachineBuilderHelper.AreEquals(current.StateFrom, StateFrom) &&
                    HierarchicalStateMachineBuilderHelper.AreEquals(current.Trigger, Trigger) &&
                    HierarchicalStateMachineBuilderHelper.AreEquals(current.StateTo, StateTo))
                {
                    return true;
                }
            }

            return false;
        }

        internal void RemoveAllTransitions()
        {
            _transitions.Clear();
        }

        private PlainStateMachine<TState, TTrigger> Build<TState, TTrigger>()
        {
            var stateMachine = new PlainStateMachine<TState, TTrigger>();

            AddStates(stateMachine);
            AddTransitionsAndGuardConditions(stateMachine);

            return stateMachine;
        }

        private void AddStates<TState, TTrigger>(PlainStateMachine<TState, TTrigger> stateMachine)
        {
            if (_states.Count > 0)
            {
                for (int i = 0; i < _states.Count; i++)
                {
                    var current = _states[i];

                    TState stateId = (TState) current.StateId;
                    IState stateObject = (current.StateObject as IState) ?? new EmptyState();

                    stateMachine.AddState(stateId, stateObject);

                    if (stateObject is IStateEventHandler eventHandler)
                        stateMachine.SubscribeEventHandlerTo(stateId, eventHandler);
                }

                stateMachine.InitialState = (TState) InitialStateId;
            }
        }

        private void AddTransitionsAndGuardConditions<TState, TTrigger>(PlainStateMachine<TState, TTrigger> stateMachine)
        {
            if (_transitions.Count > 0)
            {
                for (int i = 0; i < _transitions.Count; i++)
                {
                    var current = _transitions[i];

                    TState stateFrom = (TState) current.StateFrom;
                    TState stateTo = (TState) current.StateTo;
                    TTrigger trigger = (TTrigger) current.Trigger;

                    var transition = new Transition<TState, TTrigger>(stateFrom, trigger, stateTo);
                    
                    stateMachine.AddTransition(transition);

                    for (int j = 0; j < current.GuardConditions.Length; j++)
                    {
                        stateMachine.AddGuardConditionTo(transition, ScriptableObject.Instantiate(current.GuardConditions[j]));
                    }
                }
            }
        }

        public object Build()
        {
            return GetType()
                .GetMethod("Build", BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(StateIdType, TriggerType)
                .Invoke(this, null);
        }

        internal void SaveMetadata(string key, object value)
        {
            if (ContainsMetadataKey(key) == false)
                _metadata.Add(new Metadata() { Key = key, Value = JsonUtility.ToJson(value) });
            else
            {
                for (int i = 0; i < _metadata.Count; i++)
                {
                    if (_metadata[i].Key == key)
                        _metadata[i].Value = JsonUtility.ToJson(value);
                }
            }
        }

        internal void RemoveMetadata(string key)
        {
            if(ContainsMetadataKey(key))
            {
                for (int i = 0; i < _metadata.Count; i++)
                {
                    if (_metadata[i].Key == key)
                        _metadata.RemoveAt(i);
                }
            }
        }

        internal T GetMetadata<T>(string key)
        {
            for(int i = 0; i < _metadata.Count; i++)
            {
                if (_metadata[i].Key == key)
                    return JsonUtility.FromJson<T>(_metadata[i].Value);
            }

            return default;
        }

        internal bool ContainsMetadataKey(string key)
        {
            for(int i = 0; i < _metadata.Count; i++)
            {
                if (_metadata[i].Key == key)
                    return true;
            }

            return false;
        }

        [Serializable]
        private class Metadata
        {
            [SerializeField]
            public string Key, Value;
        }
    }
}