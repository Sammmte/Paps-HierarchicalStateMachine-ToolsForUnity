using Paps.StateMachines;
using UnityEngine;

namespace Paps.HierarchicalStateMachine_ToolsForUnity
{
    public abstract class ScriptableState : ScriptableObject, IState, IStateEventHandler
    {
        [SerializeField]
        private string _debugName;
        [SerializeField]
        private bool _instantiateThis = true;

        public string DebugName => _debugName;
        public bool InstantiateThis => _instantiateThis;

        public void Enter()
        {
            OnEnter();
        }

        protected virtual void OnEnter()
        {

        }

        public void Exit()
        {
            OnExit();
        }

        protected virtual void OnExit()
        {

        }

        public void Update()
        {
            OnUpdate();
        }

        protected virtual void OnUpdate()
        {

        }

        public virtual bool HandleEvent(IEvent ev)
        {
            return false;
        }
    }
}