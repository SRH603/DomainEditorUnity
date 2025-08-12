namespace RuntimeCurveEditor
{
    public interface Operation
    {
        void Undo();

        void Redo();
    }
}
