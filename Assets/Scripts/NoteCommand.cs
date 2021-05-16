using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class NoteCommand
{
    public abstract void Undo();
    public abstract void Execute();

    public int[] affectedNoteIDs;

    public string displayName = string.Empty;

    public Note[] GetAffectedNotes(Chart chart) =>
        chart.note_list
             .Where(x => affectedNoteIDs
             .Any(y => x.id == y))
             .ToArray();
}

