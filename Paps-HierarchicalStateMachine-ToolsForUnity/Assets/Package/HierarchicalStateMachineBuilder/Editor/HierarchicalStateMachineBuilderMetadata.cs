using System;
using System.Collections.Generic;
using UnityEngine;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    [Serializable]
    internal class HierarchicalStateMachineBuilderMetadata
    {
        [SerializeField]
        public List<StateNodeMetadata> StateNodesMetadata;

        public HierarchicalStateMachineBuilderMetadata()
        {
            StateNodesMetadata = new List<StateNodeMetadata>();
        }
    }
}