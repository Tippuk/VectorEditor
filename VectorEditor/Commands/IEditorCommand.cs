namespace VectorEditor.Commands
{
    public interface IEditorCommand
    {
        void Execute();
        void Unexecute();
    }
}
