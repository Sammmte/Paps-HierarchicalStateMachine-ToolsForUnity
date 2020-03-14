using System;

namespace Paps.HierarchicalStateMachine_ToolsForUnity
{
    internal static class GenericTypeSerializer
    {
        public static string Serialize(object value)
        {
            if (value is int || value is float || value is string || value.GetType().IsEnum)
            {
                return value.ToString();
            }
            
            throw new ArgumentException("argument serialization not supported");
        }

        public static object Deserialize(string serialized, Type type)
        {
            if (type == typeof(int))
            {
                return int.Parse(serialized);
            }
            else if (type == typeof(float))
            {
                return float.Parse(serialized);
            }
            else if (type == typeof(string))
            {
                return serialized;
            }
            else if (type.IsEnum)
            {
                return Enum.Parse(type, serialized);
            }
            
            throw new ArgumentException("argument serialization not supported");
        }
    }
}


