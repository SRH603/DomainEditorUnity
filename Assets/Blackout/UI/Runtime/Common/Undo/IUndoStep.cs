namespace Blackout.UI
{
    /// <summary>
    /// The bare requirements for an undo step
    /// </summary>
    public interface IUndoStep
    {
        void PerformUndo();

        void PerformRedo();

        void Free();
    }
}