using System;
using System.Linq;
using CCE.Data;

namespace CCE.Commands
{
    public abstract class NoteCommand
    {
        public abstract void Undo();
        public abstract void Execute();

        public int[] AffectedNoteIDs;

        public string DisplayName = String.Empty;

        public Note[] GetAffectedNotes(Chart chart) =>
            chart.NoteList
                .Where(x => AffectedNoteIDs
                    .Any(y => x.ID == y))
                .ToArray();
    }
}