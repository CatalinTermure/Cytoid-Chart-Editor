using System.Collections.Generic;
using System.Linq;
using CCE.Commands;
using CCE.Data;

namespace CCE.Game.Commands
{
    public class PlaceNotesCommand : NoteCommand
    {
        private readonly Note[] _addedNotes;

        public PlaceNotesCommand(IEnumerable<Note> notes)
        {
            _addedNotes = notes.ToArray();
        }

        public override void Execute()
        {
            AffectedNoteIDs = new int[_addedNotes.Length];
            for (var i = 0; i < _addedNotes.Length; i++)
            {
                var note = _addedNotes[i];
                var id = GameLogic.AddNote(note);
                AffectedNoteIDs[i] = id;
            }
        }

        public override void Undo()
        {
            foreach (var id in AffectedNoteIDs)
            {
                GameLogic.Instance.RemoveNote(id);
            }
        }
    }
}