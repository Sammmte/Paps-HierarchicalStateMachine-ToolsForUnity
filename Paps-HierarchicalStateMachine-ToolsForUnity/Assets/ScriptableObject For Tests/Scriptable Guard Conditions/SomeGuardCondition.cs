using Paps.HierarchicalStateMachine_ToolsForUnity;
using UnityEngine;

namespace Tests
{
    [CreateAssetMenu(menuName = "Paps/Guard Conditions/Some Guard Condition")]
    public class SomeGuardCondition : ScriptableGuardCondition
    {
        protected override bool Validate()
        {
            return true;
        }
    }
}

