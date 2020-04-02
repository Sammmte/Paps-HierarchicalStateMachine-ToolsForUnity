using Paps.StateMachines;
using Paps.StateMachines.Extensions.BehaviouralStates;
using UnityEngine;

namespace Paps.HierarchicalStateMachine_ToolsForUnity
{
    public class ScriptableStateBehaviour : ScriptableObject, IStateBehaviour, IStateEventHandler
    {
        [SerializeField]
        private string _debugName;
        [SerializeField]
        private bool _instantiateThis = true;

        public string DebugName => _debugName;
        public bool InstantiateThis => _instantiateThis;

        public virtual bool HandleEvent(IEvent ev)
        {
            return false;
        }

        public virtual void OnEnter()
        {
            
        }

        public virtual void OnExit()
        {
            
        }

        public virtual void OnUpdate()
        {
            
        }
    }
}