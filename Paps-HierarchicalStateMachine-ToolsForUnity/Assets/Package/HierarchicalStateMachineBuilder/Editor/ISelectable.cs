namespace Paps.HierarchicalStateMachine_ToolsForUnity.Editor
{
    public interface ISelectable
    {
        void DrawControls();
        void Select();
        void Deselect();
    }
}