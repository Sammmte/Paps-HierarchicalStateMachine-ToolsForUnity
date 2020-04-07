﻿using Paps.HierarchicalStateMachine_ToolsForUnity;
using Paps.StateMachines;
using SomeNamespace;
using System.Reflection;
using UnityEngine;

namespace Tests
{
    public class TestScript : MonoBehaviour
    {
        [SerializeField]
        private HierarchicalStateMachineBuilder _stateMachineBuilder;

        public void Start()
        {
            var stateMachine = _stateMachineBuilder.Build();

            GetType()
                .GetMethod(nameof(ShowMachineData), BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(_stateMachineBuilder.StateIdType, _stateMachineBuilder.TriggerType)
                .Invoke(this, new object[] { stateMachine });
        }

        private void ShowMachineData<TState, TTrigger>(HierarchicalStateMachine<TState, TTrigger> stateMachine)
        {
            var states = stateMachine.GetStates();

            Debug.Log("State count: " + stateMachine.StateCount);

            Debug.Log("Initial State: " + stateMachine.InitialState);

            if (states != null)
            {
                foreach (var state in states)
                {
                    Debug.Log(state);

                    var eventHandlers = stateMachine.GetEventHandlersOf(state);

                    if (eventHandlers != null)
                        Debug.Log("State " + state + " contains " + eventHandlers.Length + " event handlers");

                    var childs = stateMachine.GetImmediateChildsOf(state);

                    if (childs != null)
                    {
                        foreach (var child in childs)
                            Debug.Log("Child state: " + child);

                        Debug.Log("Initial Child: " + stateMachine.GetInitialStateOf(state));
                    }
                }

                var transitions = stateMachine.GetTransitions();

                if (transitions != null)
                {
                    foreach (var transition in transitions)
                    {
                        Debug.Log("Transition: " + transition.StateFrom + " -> " + transition.Trigger + " -> " + transition.StateTo);

                        var guardConditions = stateMachine.GetGuardConditionsOf(transition);

                        if (guardConditions != null)
                            Debug.Log("Guard conditions count: " + guardConditions.Length);
                        else
                            Debug.Log("Guard conditions count: " + 0);
                    }
                }
            }
        }
    }

}

