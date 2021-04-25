using System.Collections.Generic;
using System.Linq;
using CCE.Data;
using CCE.Game;

namespace CCE.Commands
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
            for (int i = 0; i < _addedNotes.Length; i++)
            {
                Note note = _addedNotes[i];
                int id = GameLogic.Instance.AddNoteInternal(note);
                AffectedNoteIDs[i] = id;
            }
        }

        public override void Undo()
        {
            foreach (var id in AffectedNoteIDs)
            {
                GameLogic.Instance.RemoveNoteInternal(id);
            }
        }
    }
}