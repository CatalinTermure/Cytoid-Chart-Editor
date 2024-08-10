namespace CCE.Commands
{
    public abstract class NoteCommand
    {
        public int[] AffectedNoteIDs;

        public abstract void Undo();
        public abstract void Execute();
    }
}