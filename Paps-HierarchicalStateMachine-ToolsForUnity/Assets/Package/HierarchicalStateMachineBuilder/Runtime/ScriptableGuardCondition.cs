using Paps.StateMachines;
using UnityEngine;

namespace Paps.HierarchicalStateMachine_ToolsForUnity
{
    public abstract class ScriptableGuardCondition : ScriptableObject, IGuardCondition
    {
        [SerializeField]
        private string _debugName;
        [SerializeField]
        private bool _instantiateThis;

        public string DebugName => _debugName;
        public bool InstantiateThis => _instantiateThis;

        public bool IsValid()
        {
            return Validate();
        }

        protected abstract bool Validate();
    }
}