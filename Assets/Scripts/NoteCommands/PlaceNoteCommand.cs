using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlaceNotesCommand : NoteCommand
{
    Note[] addedNotes;
    public PlaceNotesCommand(IEnumerable<Note> notes)
    {
        addedNotes = notes.ToArray();
    }
    public override void Execute()
    {
        affectedNoteIDs = new int[addedNotes.Length];
        for (int i = 0; i < addedNotes.Length; i++)
        {
            Note note = addedNotes[i];
            int id = GameLogic.Instance.AddNoteInternal(note);
            affectedNoteIDs[i] = id;
        }
    }

    public override void Undo()
    {
        foreach (var id in affectedNoteIDs)
        {
            GameLogic.Instance.RemoveNoteInternal(id);
        }
    }
}
