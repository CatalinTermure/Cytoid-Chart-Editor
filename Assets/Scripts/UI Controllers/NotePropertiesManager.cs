using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotePropertiesManager : MonoBehaviour
{
    private bool ChangeNoteAR, ChangeNoteXPosition, ChangeNoteY;

    private static readonly List<int> notes = new List<int>();

    public GameObject NoteXLabel, NoteXInputField, NoteARLabel, NoteARInputField, NoteYLabel, NoteYInputField;

    private double NotesX = -1, NotesAR = -1, NotesY = -1;

    public bool IsEmpty
    {
        get => notes.Count == 0;
    }

    private void Awake()
    {
        NoteXLabel.SetActive(false);
        NoteXInputField.SetActive(false);
        NoteARLabel.SetActive(false);
        NoteARInputField.SetActive(false);
        NoteYInputField.SetActive(false);
        NoteYLabel.SetActive(false);

        NoteXInputField.GetComponent<InputField>().onEndEdit.AddListener((string s) =>
        {
            GameLogic.BlockInput = false;

            double x = double.Parse(s);

            for(int i = 0; i < notes.Count; i++)
            {
                GlobalState.CurrentChart.note_list[notes[i]].x = x;
                GameLogic.RefreshNote(notes[i]);
            }
            Clear();
        });

        NoteARInputField.GetComponent<InputField>().onEndEdit.AddListener((string s) =>
        {
            GameLogic.BlockInput = false;

            double approach_rate = double.Parse(s);

            for (int i = 0; i < notes.Count; i++)
            {
                GlobalState.CurrentChart.note_list[notes[i]].approach_rate = approach_rate;
                GameLogic.RefreshNote(notes[i]);
            }
            Clear();
        });

        NoteYInputField.GetComponent<InputField>().onEndEdit.AddListener((string s) =>
        {
            GameLogic.BlockInput = false;

            double y = double.Parse(s);

            for (int i = 0; i < notes.Count; i++)
            {
                GlobalState.CurrentChart.note_list[notes[i]].y = y;
                GlobalState.CurrentChart.note_list[notes[i]].tick = (int)Math.Round(GlobalState.CurrentChart.page_list[GlobalState.CurrentChart.note_list[notes[i]].page_index].start_tick +
                    GlobalState.CurrentChart.page_list[GlobalState.CurrentChart.note_list[notes[i]].page_index].PageSize * y);
                GameLogic.RefreshNote(notes[i]);
            }
            Clear();
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
        if(!ChangeNoteY)
        {
            ChangeNoteY = true;
            NoteYLabel.SetActive(true);
            NoteYInputField.SetActive(true);
        }
        if(notes.Count == 1)
        {
            NotesX = GlobalState.CurrentChart.note_list[notes[0]].x;
            NoteXInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.note_list[notes[0]].x.ToString("F3");

            NotesAR = GlobalState.CurrentChart.note_list[notes[0]].approach_rate;
            NoteARInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.note_list[notes[0]].approach_rate.ToString("F3");

            NotesY = GlobalState.CurrentChart.note_list[notes[0]].y;
            NoteYInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.note_list[notes[0]].y.ToString("F3");
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

            if(NotesY < -0.6)
            {
                NoteYInputField.GetComponent<InputField>().text = "";
            }
            else if(Math.Abs(GlobalState.CurrentChart.note_list[notes[notes.Count - 1]].y - NotesY) < 0.001)
            {
                NoteYInputField.GetComponent<InputField>().text = NotesY.ToString("F3");
            }
            else
            {
                NotesY = -1;
                NoteYInputField.GetComponent<InputField>().text = "";
            }
        }
    }

    public void Remove(Note note)
    {
        notes.Remove(note.id);
        if(notes.Count == 0)
        {
            ChangeNoteAR = ChangeNoteXPosition = ChangeNoteY = false;
            NoteXLabel.SetActive(false);
            NoteXInputField.SetActive(false);
            NoteARLabel.SetActive(false);
            NoteARInputField.SetActive(false);
            NoteYInputField.SetActive(false);
            NoteYLabel.SetActive(false);
            NotesX = NotesAR = NotesY = -1;
        }
        else if (notes.Count == 1)
        {
            NotesX = GlobalState.CurrentChart.note_list[notes[0]].x;
            NoteXInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.note_list[notes[0]].x.ToString("F3");

            NotesAR = GlobalState.CurrentChart.note_list[notes[0]].approach_rate;
            NoteARInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.note_list[notes[0]].approach_rate.ToString("F3");

            NotesY = GlobalState.CurrentChart.note_list[notes[0]].y;
            NoteYInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.note_list[notes[0]].y.ToString("F3");
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

            bool isNoteYSame = true;
            NotesY = GlobalState.CurrentChart.note_list[notes[0]].y;
            for(int i = 1; i < notes.Count; i++)
            {
                if (Math.Abs(GlobalState.CurrentChart.note_list[notes[notes.Count - 1]].y - NotesY) > 0.001)
                {
                    isNoteYSame = false;
                    NotesY = -1;
                }
            }

            if(NotesY < -0.6)
            {
                NoteARInputField.GetComponent<InputField>().text = "";
            }
            else if(isNoteYSame)
            {
                NoteYInputField.GetComponent<InputField>().text = NotesY.ToString("F3");
            }
        }
    }

    public void Clear()
    {
        ChangeNoteAR = ChangeNoteXPosition = ChangeNoteY = false;
        NotesX = NotesAR = NotesY = -1;
        NoteXLabel.SetActive(false);
        NoteXInputField.SetActive(false);
        NoteARLabel.SetActive(false);
        NoteARInputField.SetActive(false);
        NoteYInputField.SetActive(false);
        NoteYLabel.SetActive(false);
        notes.Clear();
    }

    public void BlockInput()
    {
        GameLogic.BlockInput = true;
    }
}
