using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Clipboard
{
    private static readonly List<Note> notes = new List<Note>();
    private static readonly List<Tempo> tempos = new List<Tempo>();

    public static int ReferenceTick;
    public static int ReferencePageIndex;

    public static void Add(Note note)
    {
        notes.Add(new Note(note));
    }

    public static void Add(Tempo tempo)
    {
        tempos.Add(tempo);
    }

    public static List<Note> GetNotes()
    {
        List<Note> aux = new List<Note>(notes.Count);
        for(int i = 0; i < notes.Count; i++)
        {
            aux.Add(new Note(notes[i]));
        }
        return aux;
    }

    public static List<Tempo> GetTempos()
    {
        return new List<Tempo>(tempos);
    }

    public static void Clear()
    {
        notes.Clear();
        tempos.Clear();
    }
}
