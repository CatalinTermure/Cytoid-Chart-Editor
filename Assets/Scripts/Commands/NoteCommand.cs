namespace CCE.Commands
{
    public abstract class NoteCommand : ICommand
    {
        public int[] AffectedNoteIDs;

        public abstract void Undo();
        public abstract void Execute();
    }
}