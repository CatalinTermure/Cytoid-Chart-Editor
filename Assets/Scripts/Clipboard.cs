using System.Collections.Generic;
using CCE.Data;

public static class Clipboard
{
    private static readonly List<Note> _notes = new List<Note>();
    private static readonly List<Tempo> _tempos = new List<Tempo>();

    public static int ReferenceTick;
    public static int ReferencePageIndex;

    public static void Add(Note note)
    {
        _notes.Add(new Note(note));
    }

    public static void Add(Tempo tempo)
    {
        _tempos.Add(tempo);
    }

    public static List<Note> GetNotes()
    {
        List<Note> aux = new List<Note>(_notes.Count);
        for (int i = 0; i < _notes.Count; i++)
        {
            aux.Add(new Note(_notes[i]));
        }
        return aux;
    }

    public static List<Tempo> GetTempos()
    {
        return new List<Tempo>(_tempos);
    }

    public static void Clear()
    {
        _notes.Clear();
        _tempos.Clear();
    }
}
