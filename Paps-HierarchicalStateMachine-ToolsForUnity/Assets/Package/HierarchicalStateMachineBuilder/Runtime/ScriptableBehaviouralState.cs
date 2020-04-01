using Paps.StateMachines.Extensions.BehaviouralStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paps.HierarchicalStateMachine_ToolsForUnity
{
    public class ScriptableBehaviouralState : ScriptableState, IBehaviouralState
    {
        [SerializeField]
        private ScriptableStateBehaviour[] behaviours;

        private BehaviouralState _innerBehaviouralState = new BehaviouralState();

        public int BehaviourCount => _innerBehaviouralState.BehaviourCount;

        private bool _initialized;

        protected override void OnEnter()
        {
            if(_initialized == false)
            {
                AddInitialBehaviours();
                _initialized = true;
            }

            _innerBehaviouralState.Enter();
        }

        private void AddInitialBehaviours()
        {
            for (int i = 0; i < behaviours.Length; i++)
                _innerBehaviouralState.AddBehaviour(behaviours[i]);
        }

        protected override void OnUpdate()
        {
            _innerBehaviouralState.Update();
        }

        protected override void OnExit()
        {
            _innerBehaviouralState.Exit();
        }

        public void AddBehaviour(IStateBehaviour stateBehaviour) => _innerBehaviouralState.AddBehaviour(stateBehaviour);

        public bool ContainsBehaviour(IStateBehaviour stateBehaviour) => _innerBehaviouralState.ContainsBehaviour(stateBehaviour);

        public T GetBehaviour<T>() => _innerBehaviouralState.GetBehaviour<T>();

        public T[] GetBehaviours<T>() => _innerBehaviouralState.GetBehaviours<T>();

        public IEnumerator<IStateBehaviour> GetEnumerator() => _innerBehaviouralState.GetEnumerator();

        public bool RemoveBehaviour(IStateBehaviour stateBehaviour) => _innerBehaviouralState.RemoveBehaviour(stateBehaviour);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerBehaviouralState.GetEnumerator();
        }
    }
}