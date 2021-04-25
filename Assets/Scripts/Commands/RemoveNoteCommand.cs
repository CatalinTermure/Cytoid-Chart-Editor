using System.Collections.Generic;
using CCE.Core;
using CCE.Data;
using CCE.Game;

namespace CCE.Commands
{
    public class RemoveNotesCommand : NoteCommand
    {
        private readonly List<Note> _removedNotes = new List<Note>();

        public RemoveNotesCommand(int[] noteIDs)
        {
            AffectedNoteIDs = noteIDs;
        }
    
        public override void Execute()
        {
            foreach (var noteID in AffectedNoteIDs)
            {
                var noteClone = new Note(GlobalState.CurrentChart.NoteList[noteID]);
                _removedNotes.Add(noteClone);
                GameLogic.Instance.RemoveNoteInternal(noteID);
            }
        }

        public override void Undo()
        {
            foreach (var note in _removedNotes)
            {
                GameLogic.Instance.AddNoteInternal(note);
            }
        }
    }
}