using UnityEditor;
using UnityEditor.Callbacks;

namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    internal class HierarchicalStateMachineAssetDoubleClickHandler
    {
        [OnOpenAsset(1)]
        public static bool OpenEditorWindow(int instanceID, int line)
        {
            HierarchicalStateMachineBuilder builderAsset = EditorUtility.InstanceIDToObject(instanceID) as HierarchicalStateMachineBuilder;

            if(builderAsset != null)
            {
                HierarchicalStateMachineBuilderEditorWindow.OpenWindow(builderAsset);

                return true;
            }

            return false;
        }
    }
}