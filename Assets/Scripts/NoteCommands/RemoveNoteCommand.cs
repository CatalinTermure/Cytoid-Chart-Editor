using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RemoveNotesCommand : NoteCommand
{
    public List<Note> removedNotes = new List<Note>();

    public RemoveNotesCommand(int[] noteIDs)
    {
        affectedNoteIDs = noteIDs;
    }
    public override void Execute()
    {
        foreach (var noteID in affectedNoteIDs)
        {
            var noteClone = new Note(GlobalState.CurrentChart.note_list[noteID]);
            removedNotes.Add(noteClone);
            GameLogic.Instance.RemoveNoteInternal(noteID);
        }
    }

    public override void Undo()
    {
        foreach (var note in removedNotes)
        {
            GameLogic.Instance.AddNoteInternal(note);
        }
    }
}