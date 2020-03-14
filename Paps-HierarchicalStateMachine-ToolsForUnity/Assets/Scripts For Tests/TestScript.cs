using Paps.HierarchicalStateMachine_ToolsForUnity;
using Paps.StateMachines;
using UnityEngine;

namespace Tests
{
    public class TestScript : MonoBehaviour
    {
        [SerializeField]
        private HierarchicalStateMachineBuilder _stateMachineBuilder;

        public void Start()
        {
            var stateMachine = (PlainStateMachine<string, string>)_stateMachineBuilder.Build();

            var states = stateMachine.GetStates();

            Debug.Log("State count: " + stateMachine.StateCount);

            Debug.Log("Initial State: " + stateMachine.InitialState);

            foreach (var state in states)
            {
                Debug.Log(state);

                var eventHandlers = stateMachine.GetEventHandlersOf(state);
                
                if(eventHandlers != null)
                    Debug.Log("State " + state + " contains " + eventHandlers.Length + " event handlers");
            }

            var transitions = stateMachine.GetTransitions();

            foreach (var transition in transitions)
            {
                Debug.Log("Transition: " + transition.StateFrom + " -> " + transition.Trigger + " -> " + transition.StateTo);

                var guardConditions = stateMachine.GetGuardConditionsOf(transition);

                if(guardConditions != null)
                    Debug.Log("Guard conditions count: " + guardConditions.Length);
                else
                    Debug.Log("Guard conditions count: " + 0);
            }
        }
    }

}

