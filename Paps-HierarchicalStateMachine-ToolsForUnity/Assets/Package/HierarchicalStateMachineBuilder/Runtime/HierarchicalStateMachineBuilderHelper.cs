using System;
using System.Reflection;

namespace Paps.HierarchicalStateMachine_ToolsForUnity
{
    internal static class HierarchicalStateMachineBuilderHelper
    {
        public static bool AreEquals(object stateId1, object stateId2)
        {
            return object.Equals(stateId1, stateId2);
        }

        public static Type GetTypeOf(string typeFullName)
        {
            if(string.IsNullOrEmpty(typeFullName) == false)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = assembly.GetType(typeFullName);

                    if (type != null)
                        return type;
                }
            }

            return null;
        }
    }
}