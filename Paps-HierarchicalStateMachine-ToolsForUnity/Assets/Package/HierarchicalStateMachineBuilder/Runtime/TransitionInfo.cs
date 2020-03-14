using System;
using UnityEngine;

namespace Paps.HierarchicalStateMachine_ToolsForUnity
{
    [Serializable]
    internal class TransitionInfo
    {
        public Type StateIdType
        {
            get
            {
                if(_stateIdType == null)
                    _stateIdType = HierarchicalStateMachineBuilderHelper.GetTypeOf(_stateIdTypeFullName);

                return _stateIdType;
            }
        }

        public Type TriggerType
        {
            get
            {
                if (_triggerType == null)
                    _triggerType = HierarchicalStateMachineBuilderHelper.GetTypeOf(_triggerTypeFullName);

                return _triggerType;
            }
        }

        public object StateFrom
        {
            get
            {
                if (_stateFrom == null)
                    _stateFrom = GenericTypeSerializer.Deserialize(_serializedStateFrom, StateIdType);

                return _stateFrom;
            }
        }
        
        public object StateTo
        {
            get
            {
                if (_stateTo == null)
                    _stateTo = GenericTypeSerializer.Deserialize(_serializedStateTo, StateIdType);

                return _stateTo;
            }
        }
        
        public object Trigger
        {
            get
            {
                if (_trigger == null)
                    _trigger = GenericTypeSerializer.Deserialize(_serializedTrigger, TriggerType);

                return _trigger;
            }
        }

        public string SerializedStateFrom => _serializedStateFrom;
        public string SerializedStateTo => _serializedStateTo;
        public string SerializedTrigger => _serializedTrigger;
        
        [SerializeField]
        private string _serializedStateFrom, _serializedTrigger, _serializedStateTo;

        [SerializeField]
        private string _stateIdTypeFullName, _triggerTypeFullName;

        private Type _stateIdType, _triggerType;
        
        private object _stateFrom, _trigger, _stateTo;

        [SerializeField]
        private ScriptableGuardCondition[] _guardConditions;

        public ScriptableGuardCondition[] GuardConditions => _guardConditions;

        public TransitionInfo(object StateFrom, object Trigger, object StateTo, ScriptableGuardCondition[] guardConditions)
        {
            _serializedStateFrom = GenericTypeSerializer.Serialize(StateFrom);
            _serializedStateTo = GenericTypeSerializer.Serialize(StateTo);
            _serializedTrigger = GenericTypeSerializer.Serialize(Trigger);

            _stateIdTypeFullName = StateFrom.GetType().FullName;
            _triggerTypeFullName = Trigger.GetType().FullName;

            if (guardConditions != null)
            {
                _guardConditions = new ScriptableGuardCondition[guardConditions.Length];
                guardConditions.CopyTo(_guardConditions, 0);
            }
        }
    }
}