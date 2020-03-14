using System;
using UnityEngine;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    [Serializable]
    internal class StateNodeMetadata
    {
        [SerializeField]
        private Vector2 _position;

        public Vector2 Position => _position;

        [SerializeField]
        private string _serializedStateId;

        [SerializeField]
        private string _stateIdTypeFullName;

        private Type _stateIdType;

        public Type StateIdType
        {
            get
            {
                if (_stateIdType == null)
                    _stateIdType = HierarchicalStateMachineBuilderHelper.GetTypeOf(_stateIdTypeFullName);

                return _stateIdType;
            }
        }

        private object _stateId;

        public object StateId
        {
            get
            {
                if (_stateId == null)
                    _stateId = GenericTypeSerializer.Deserialize(_serializedStateId, StateIdType);

                return _stateId;
            }
        }

        public StateNodeMetadata(object stateId, Vector2 position)
        {
            _position = position;

            _serializedStateId = GenericTypeSerializer.Serialize(stateId);

            _stateIdTypeFullName = stateId.GetType().FullName;
        }
    }

}