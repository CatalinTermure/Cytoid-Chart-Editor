using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotePropertiesManager : MonoBehaviour
{
    private bool ChangeNoteAR, ChangeNoteXPosition;

    private static readonly List<int> notes = new List<int>();

    public GameObject NoteXLabel, NoteXInputField, NoteARLabel, NoteARInputField;

    private double NotesX = -1, NotesAR = -1;

    private void Awake()
    {
        NoteXLabel.SetActive(false);
        NoteXInputField.SetActive(false);
        NoteARLabel.SetActive(false);
        NoteARInputField.SetActive(false);

        NoteXInputField.GetComponent<InputField>().onEndEdit.AddListener((string s) =>
        {
            double x = double.Parse(s);

            for(int i = 0; i < notes.Count; i++)
            {
                GlobalState.CurrentChart.note_list[notes[i]].x = x;
                GameLogic.RefreshNote(notes[i]);
            }
            notes.Clear();

            NoteXLabel.SetActive(false);
            NoteXInputField.SetActive(false);
            ChangeNoteXPosition = false;

            NoteARLabel.SetActive(false);
            NoteARInputField.SetActive(false);
            ChangeNoteAR = false;

            NotesX = NotesAR = -1;
        });

        NoteARInputField.GetComponent<InputField>().onEndEdit.AddListener((string s) =>
        {
            double approach_rate = double.Parse(s);

            for (int i = 0; i < notes.Count; i++)
            {
                GlobalState.CurrentChart.note_list[notes[i]].approach_rate = approach_rate;
                GameLogic.RefreshNote(notes[i]);
            }
            notes.Clear();

            NoteXLabel.SetActive(false);
            NoteXInputField.SetActive(false);
            ChangeNoteXPosition = false;

            NoteARLabel.SetActive(false);
            NoteARInputField.SetActive(false);
            ChangeNoteAR = false;

            NotesX = NotesAR = -1;
        });
    }

    public void Add(Note note)
    {
        notes.Add(note.id);
        if(!ChangeNoteAR)
        {
            ChangeNoteAR = true;
            NoteARLabel.SetActive(true);
            NoteARInputField.SetActive(true);
        }
        if(!ChangeNoteXPosition)
        {
            ChangeNoteXPosition = true;
            NoteXLabel.SetActive(true);
            NoteXInputField.SetActive(true);
        }
        if(notes.Count == 1)
        {
            NotesX = GlobalState.CurrentChart.note_list[notes[0]].x;
            NoteXInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.note_list[notes[0]].x.ToString();

            NotesAR = GlobalState.CurrentChart.note_list[notes[0]].approach_rate;
            NoteARInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.note_list[notes[0]].approach_rate.ToString();
        }
        else
        {
            if (NotesX < -0.6)
            {
                NoteXInputField.GetComponent<InputField>().text = "";
            }
            else if (Math.Abs(GlobalState.CurrentChart.note_list[notes[notes.Count - 1]].x - NotesX) < 0.001)
            {
                NoteXInputField.GetComponent<InputField>().text = NotesX.ToString("F3");
            }
            else
            {
                NotesX = -1;
                NoteXInputField.GetComponent<InputField>().text = "";
            }

            if (NotesAR < -0.6)
            {
                NoteARInputField.GetComponent<InputField>().text = "";
            }
            else if (Math.Abs(GlobalState.CurrentChart.note_list[notes[notes.Count - 1]].approach_rate - NotesAR) < 0.001)
            {
                NoteARInputField.GetComponent<InputField>().text = NotesAR.ToString("F3");
            }
            else
            {
                NotesAR = -1;
                NoteARInputField.GetComponent<InputField>().text = "";
            }
        }
    }

    public void Remove(Note note)
    {
        notes.Remove(note.id);
        if(notes.Count == 0)
        {
            ChangeNoteAR = ChangeNoteXPosition = false;
            NoteXLabel.SetActive(false);
            NoteXInputField.SetActive(false);
            NoteARLabel.SetActive(false);
            NoteARInputField.SetActive(false);
            NotesX = NotesAR = -1;
        }
        else if (notes.Count == 1)
        {
            NotesX = GlobalState.CurrentChart.note_list[notes[0]].x;
            NoteXInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.note_list[notes[0]].x.ToString();

            NotesAR = GlobalState.CurrentChart.note_list[notes[0]].approach_rate;
            NoteARInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.note_list[notes[0]].approach_rate.ToString();
        }
        else
        {
            bool isNoteXSame = true;
            NotesX = GlobalState.CurrentChart.note_list[notes[0]].x;
            for(int i = 1; i < notes.Count; i++)
            {
                if (Math.Abs(GlobalState.CurrentChart.note_list[notes[notes.Count - 1]].x - NotesX) > 0.001)
                {
                    isNoteXSame = false;
                    NotesX = -1;
                }
            }

            if (NotesX < -0.6)
            {
                NoteXInputField.GetComponent<InputField>().text = "";
            }
            else if(isNoteXSame)
            {
                NoteXInputField.GetComponent<InputField>().text = NotesX.ToString("F3");
            }

            bool isNoteARSame = true;
            NotesAR = GlobalState.CurrentChart.note_list[notes[0]].approach_rate;
            for (int i = 1; i < notes.Count; i++)
            {
                if (Math.Abs(GlobalState.CurrentChart.note_list[notes[notes.Count - 1]].approach_rate - NotesAR) > 0.001)
                {
                    isNoteARSame = false;
                    NotesAR = -1;
                }
            }

            if (NotesAR < -0.6)
            {
                NoteARInputField.GetComponent<InputField>().text = "";
            }
            else if(isNoteARSame)
            {
                NoteARInputField.GetComponent<InputField>().text = NotesAR.ToString("F3");
            }
        }
    }

    public void Clear()
    {
        ChangeNoteAR = ChangeNoteXPosition = false;
        NotesX = NotesAR = -1;
        NoteXLabel.SetActive(false);
        NoteXInputField.SetActive(false);
        NoteARLabel.SetActive(false);
        NoteARInputField.SetActive(false);
        notes.Clear();
    }
}
