using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using CCE;
using CCE.Core;
using UnityEngine;
using UnityEngine.UI;
using CCE.Data;
using CCE.Game;

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

            double x;

            try
            {
                if (s.Contains("/"))
                {
                    string[] numbers = s.Split('/');
                    x = (double)int.Parse(numbers[0]) / int.Parse(numbers[1]);
                }
                else
                {
                    x = double.Parse(s);
                }
            }
            catch (FormatException)
            {
                return;
            }


            x = GlobalState.Clamp(x, 0.0, 1.0);

            for (int i = 0; i < notes.Count; i++)
            {
                GlobalState.CurrentChart.NoteList[notes[i]].X = x;
                GameLogic.RefreshNote(notes[i]);
            }
            Clear();
        });

        NoteARInputField.GetComponent<InputField>().onEndEdit.AddListener((string s) =>
        {
            GameLogic.BlockInput = false;

            double approach_rate;

            try
            {
                if (s.Contains("/"))
                {
                    string[] numbers = s.Split('/');
                    approach_rate = (double)int.Parse(numbers[0]) / int.Parse(numbers[1]);
                }
                else
                {
                    approach_rate = double.Parse(s);
                }
            }
            catch (FormatException)
            {
                return;
            }


            for (int i = 0; i < notes.Count; i++)
            {
                GlobalState.CurrentChart.NoteList[notes[i]].ApproachRate = approach_rate;
                GameLogic.RefreshNote(notes[i]);
            }
            GameLogic.ForceUpdate();
            Clear();
        });

        NoteYInputField.GetComponent<InputField>().onEndEdit.AddListener((string s) =>
        {
            GameLogic.BlockInput = false;

            double y;

            try
            {
                if (s.Contains("/"))
                {
                    string[] numbers = s.Split('/');
                    y = (double)int.Parse(numbers[0]) / int.Parse(numbers[1]);
                }
                else
                {
                    y = double.Parse(s);
                }
            }
            catch (FormatException)
            {
                return;
            }


            y = GlobalState.Clamp(y, 0.0, 1.0);

            for (int i = 0; i < notes.Count; i++)
            {
                int tick = (int)Math.Round(GlobalState.CurrentChart.PageList[GlobalState.CurrentChart.NoteList[notes[i]].PageIndex].StartTick +
                    GlobalState.CurrentChart.PageList[GlobalState.CurrentChart.NoteList[notes[i]].PageIndex].PageSize * y);

                if (GlobalState.CurrentChart.NoteList[notes[i]].Type == (int)NoteType.CDragHead || GlobalState.CurrentChart.NoteList[notes[i]].Type == (int)NoteType.DragHead)
                {
                    tick = Math.Min(tick, GlobalState.CurrentChart.NoteList[notes[i]].NextID >= 0 ? GlobalState.CurrentChart.NoteList[GlobalState.CurrentChart.NoteList[notes[i]].NextID].Tick : 0);
                }
                else if (GlobalState.CurrentChart.NoteList[notes[i]].Type == (int)NoteType.CDragChild || GlobalState.CurrentChart.NoteList[notes[i]].Type == (int)NoteType.DragChild)
                {
                    tick = GlobalState.Clamp(tick, GlobalState.CurrentChart.NoteList[GameLogic.GetDragParent(notes[i])].Tick,
                        GlobalState.CurrentChart.NoteList[notes[i]].NextID >= 0 ? GlobalState.CurrentChart.NoteList[GlobalState.CurrentChart.NoteList[notes[i]].NextID].Tick : 0);
                }

                GlobalState.CurrentChart.NoteList[notes[i]].Tick = tick;

                int id = notes[i];
                while (id + 1 < GlobalState.CurrentChart.NoteList.Count && GlobalState.CurrentChart.NoteList[id].Tick > GlobalState.CurrentChart.NoteList[id + 1].Tick)
                {
                    int dragparent = GameLogic.GetDragParent(id);
                    if (dragparent > -1)
                    {
                        GlobalState.CurrentChart.NoteList[dragparent].NextID++;
                    }
                    Note aux = GlobalState.CurrentChart.NoteList[id];
                    GlobalState.CurrentChart.NoteList[id] = GlobalState.CurrentChart.NoteList[id + 1];
                    GlobalState.CurrentChart.NoteList[id + 1] = aux;
                    GlobalState.CurrentChart.NoteList[id + 1].ID = id + 1;
                    GlobalState.CurrentChart.NoteList[id].ID = id;
                    id++;
                }
            }
            GameLogic.ForceUpdate();
            Clear();
        });
    }

    public void Add(Note note)
    {
        notes.Add(note.ID);
        if (!ChangeNoteAR)
        {
            ChangeNoteAR = true;
            NoteARLabel.SetActive(true);
            NoteARInputField.SetActive(true);
        }
        if (!ChangeNoteXPosition)
        {
            ChangeNoteXPosition = true;
            NoteXLabel.SetActive(true);
            NoteXInputField.SetActive(true);
        }
        if (!ChangeNoteY)
        {
            ChangeNoteY = true;
            NoteYLabel.SetActive(true);
            NoteYInputField.SetActive(true);
        }
        if (notes.Count == 1)
        {
            NotesX = GlobalState.CurrentChart.NoteList[notes[0]].X;
            NoteXInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.NoteList[notes[0]].X.ToString("F3");

            NotesAR = GlobalState.CurrentChart.NoteList[notes[0]].ApproachRate;
            NoteARInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.NoteList[notes[0]].ApproachRate.ToString("F3");

            NotesY = GlobalState.CurrentChart.NoteList[notes[0]].Y;
            NoteYInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.NoteList[notes[0]].Y.ToString("F3");
        }
        else
        {
            if (NotesX < -0.6)
            {
                NoteXInputField.GetComponent<InputField>().text = "";
            }
            else if (Math.Abs(GlobalState.CurrentChart.NoteList[notes[notes.Count - 1]].X - NotesX) < 0.001)
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
            else if (Math.Abs(GlobalState.CurrentChart.NoteList[notes[notes.Count - 1]].ApproachRate - NotesAR) < 0.001)
            {
                NoteARInputField.GetComponent<InputField>().text = NotesAR.ToString("F3");
            }
            else
            {
                NotesAR = -1;
                NoteARInputField.GetComponent<InputField>().text = "";
            }

            if (NotesY < -0.6)
            {
                NoteYInputField.GetComponent<InputField>().text = "";
            }
            else if (Math.Abs(GlobalState.CurrentChart.NoteList[notes[notes.Count - 1]].Y - NotesY) < 0.001)
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
        notes.Remove(note.ID);
        if (notes.Count == 0)
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
            NotesX = GlobalState.CurrentChart.NoteList[notes[0]].X;
            NoteXInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.NoteList[notes[0]].X.ToString("F3");

            NotesAR = GlobalState.CurrentChart.NoteList[notes[0]].ApproachRate;
            NoteARInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.NoteList[notes[0]].ApproachRate.ToString("F3");

            NotesY = GlobalState.CurrentChart.NoteList[notes[0]].Y;
            NoteYInputField.GetComponent<InputField>().text = GlobalState.CurrentChart.NoteList[notes[0]].Y.ToString("F3");
        }
        else
        {
            bool isNoteXSame = true;
            NotesX = GlobalState.CurrentChart.NoteList[notes[0]].X;
            for (int i = 1; i < notes.Count; i++)
            {
                if (Math.Abs(GlobalState.CurrentChart.NoteList[notes[notes.Count - 1]].X - NotesX) > 0.001)
                {
                    isNoteXSame = false;
                    NotesX = -1;
                }
            }

            if (NotesX < -0.6)
            {
                NoteXInputField.GetComponent<InputField>().text = "";
            }
            else if (isNoteXSame)
            {
                NoteXInputField.GetComponent<InputField>().text = NotesX.ToString("F3");
            }

            bool isNoteARSame = true;
            NotesAR = GlobalState.CurrentChart.NoteList[notes[0]].ApproachRate;
            for (int i = 1; i < notes.Count; i++)
            {
                if (Math.Abs(GlobalState.CurrentChart.NoteList[notes[notes.Count - 1]].ApproachRate - NotesAR) > 0.001)
                {
                    isNoteARSame = false;
                    NotesAR = -1;
                }
            }

            if (NotesAR < -0.6)
            {
                NoteARInputField.GetComponent<InputField>().text = "";
            }
            else if (isNoteARSame)
            {
                NoteARInputField.GetComponent<InputField>().text = NotesAR.ToString("F3");
            }

            bool isNoteYSame = true;
            NotesY = GlobalState.CurrentChart.NoteList[notes[0]].Y;
            for (int i = 1; i < notes.Count; i++)
            {
                if (Math.Abs(GlobalState.CurrentChart.NoteList[notes[notes.Count - 1]].Y - NotesY) > 0.001)
                {
                    isNoteYSame = false;
                    NotesY = -1;
                }
            }

            if (NotesY < -0.6)
            {
                NoteARInputField.GetComponent<InputField>().text = "";
            }
            else if (isNoteYSame)
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
