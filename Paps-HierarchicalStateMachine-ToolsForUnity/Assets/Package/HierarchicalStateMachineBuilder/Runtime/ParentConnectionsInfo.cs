using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

[assembly: InternalsVisibleTo("Paps.HierarchicalStateMachine_ToolsForUnity.Editor")]
namespace Paps.HierarchicalStateMachine_ToolsForUnity
{
    [Serializable]
    internal class ParentConnectionsInfo
    {
        [SerializeField]
        private string _serializedParentStateId, _serializedInitialChildId;

        [SerializeField]
        private List<string> _serializedChildStateIds;

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

        private object _parentStateId;

        public object ParentStateId
        {
            get
            {
                if (_parentStateId == null)
                    _parentStateId = GenericTypeSerializer.Deserialize(_serializedParentStateId, StateIdType);

                return _parentStateId;
            }
        }

        private object _initialChildId;

        public object InitialChildId
        {
            get
            {
                if (_initialChildId == null)
                    _initialChildId = GenericTypeSerializer.Deserialize(_serializedInitialChildId, StateIdType);

                return _initialChildId;
            }
        }

        public string SerializedParentStateId => _serializedParentStateId;
        public string SerializedInitialChildId => _serializedInitialChildId;

        private ParentConnectionsInfo()
        {
            if (_serializedChildStateIds == null)
                _serializedChildStateIds = new List<string>();
        }

        public ParentConnectionsInfo(object parentStateId, object firstChildStateId) : this()
        {
            _stateIdTypeFullName = parentStateId.GetType().FullName;

            _serializedParentStateId = GenericTypeSerializer.Serialize(parentStateId);

            AddChild(firstChildStateId);
        }

        public void AddChild(object childStateId)
        {
            if(ContainsChild(childStateId) == false)
            {
                _serializedChildStateIds.Add(GenericTypeSerializer.Serialize(childStateId));

                if (_serializedChildStateIds.Count == 1)
                    SetInitialChild(childStateId);
            }
        }

        public bool ContainsChild(object childStateId)
        {
            for(int i = 0; i < _serializedChildStateIds.Count; i++)
            {
                var current = _serializedChildStateIds[i];

                var deserializedId = GenericTypeSerializer.Deserialize(current, StateIdType);

                if (HierarchicalStateMachineBuilderHelper.AreEquals(deserializedId, childStateId))
                    return true;
            }

            return false;
        }

        public void SetInitialChild(object childStateId)
        {
            if (ContainsChild(childStateId))
                _serializedInitialChildId = GenericTypeSerializer.Serialize(childStateId);
        }

        public object[] GetChilds()
        {
            object[] childs = new object[_serializedChildStateIds.Count];

            for(int i = 0; i < childs.Length; i++)
            {
                childs[i] = GenericTypeSerializer.Deserialize(_serializedChildStateIds[i], StateIdType);
            }

            return childs;
        }
    }
}