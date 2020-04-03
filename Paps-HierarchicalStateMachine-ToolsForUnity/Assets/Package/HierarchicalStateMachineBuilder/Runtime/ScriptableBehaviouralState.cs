﻿using System.Linq;
using Paps.StateMachines.Extensions.BehaviouralStates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paps.HierarchicalStateMachine_ToolsForUnity
{
    [CreateAssetMenu(menuName = "Paps/Scriptable States/Behavioural State")]
    public class ScriptableBehaviouralState : ScriptableState, IBehaviouralState
    {
        [SerializeField]
        private ScriptableStateBehaviour[] behaviours;

        [NonSerialized]
        private BehaviouralState _innerBehaviouralState = new BehaviouralState();

        public int BehaviourCount => _innerBehaviouralState.BehaviourCount;

        public ScriptableStateBehaviour[] GetSerializedBehaviours()
        {
            return behaviours.ToArray();
        }

        protected override sealed void OnEnter()
        {
            _innerBehaviouralState.Enter();
        }

        protected override sealed void OnUpdate()
        {
            _innerBehaviouralState.Update();
        }

        protected override sealed void OnExit()
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