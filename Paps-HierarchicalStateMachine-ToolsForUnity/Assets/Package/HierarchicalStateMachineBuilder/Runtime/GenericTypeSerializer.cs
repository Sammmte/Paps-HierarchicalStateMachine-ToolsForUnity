using System;

namespace Paps.HierarchicalStateMachine_ToolsForUnity
{
    internal static class GenericTypeSerializer
    {
        public static string Serialize(object value)
        {
            if (value is int || value is float || value is string)
            {
                return value.ToString();
            }
            else if(value.GetType().IsEnum)
            {
                var rawValue = Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));

                return rawValue.ToString();
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
                var enumValue = Enum.Parse(type, serialized);

                if (Enum.IsDefined(type, enumValue))
                    return enumValue;
                else
                    return 0;
            }
            
            throw new ArgumentException("argument serialization not supported");
        }
    }
}


