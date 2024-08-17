using System;
using System.Collections.Generic;
using CCE.Core;
using CCE.Data;
using CCE.Game;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.UI
{
    public class NotePropertiesManager : MonoBehaviour
    {
        private readonly List<int> _notes = new();

        public GameObject NoteXLabel, NoteXInputField, NoteARLabel, NoteARInputField, NoteYLabel, NoteYInputField;
        private bool _changeNoteAR, _changeNoteXPosition, _changeNoteY;

        private double _notesX = -1, _notesAR = -1, _notesY = -1;

        public bool IsEmpty => _notes.Count == 0;

        private void Awake()
        {
            NoteXLabel.SetActive(false);
            NoteXInputField.SetActive(false);
            NoteARLabel.SetActive(false);
            NoteARInputField.SetActive(false);
            NoteYInputField.SetActive(false);
            NoteYLabel.SetActive(false);

            NoteXInputField.GetComponent<InputField>().onEndEdit.AddListener(s =>
            {
                GameLogic.BlockInput = false;

                double x;

                try
                {
                    if (s.Contains("/"))
                    {
                        var numbers = s.Split('/');
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

                for (var i = 0; i < _notes.Count; i++)
                {
                    GlobalState.CurrentChart.NoteList[_notes[i]].X = x;
                    GameLogic.RefreshNote(_notes[i]);
                }

                Clear();
            });

            NoteARInputField.GetComponent<InputField>().onEndEdit.AddListener(s =>
            {
                GameLogic.BlockInput = false;

                double approachRate;

                try
                {
                    if (s.Contains("/"))
                    {
                        var numbers = s.Split('/');
                        approachRate = (double)int.Parse(numbers[0]) / int.Parse(numbers[1]);
                    }
                    else
                    {
                        approachRate = double.Parse(s);
                    }
                }
                catch (FormatException)
                {
                    return;
                }


                foreach (var noteID in _notes)
                {
                    GlobalState.CurrentChart.NoteList[noteID].ApproachRate = approachRate;
                    GameLogic.RefreshNote(noteID);
                }

                GameLogic.ForceUpdate();
                Clear();
            });

            NoteYInputField.GetComponent<InputField>().onEndEdit.AddListener(s =>
            {
                GameLogic.BlockInput = false;

                double y;

                try
                {
                    if (s.Contains("/"))
                    {
                        var numbers = s.Split('/');
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

                foreach (var noteID in _notes)
                {
                    var tick = (int)Math.Round(
                        GlobalState.CurrentChart.PageList[GlobalState.CurrentChart.NoteList[noteID].PageIndex]
                            .StartTick +
                        GlobalState.CurrentChart.PageList[GlobalState.CurrentChart.NoteList[noteID].PageIndex]
                            .PageSize * y);

                    if (GlobalState.CurrentChart.NoteList[noteID].Type == (int)NoteType.CDragHead ||
                        GlobalState.CurrentChart.NoteList[noteID].Type == (int)NoteType.DragHead)
                    {
                        tick = Math.Min(tick,
                            GlobalState.CurrentChart.NoteList[noteID].NextID >= 0
                                ? GlobalState.CurrentChart.NoteList[GlobalState.CurrentChart.NoteList[noteID].NextID]
                                    .Tick
                                : 0);
                    }
                    else if (GlobalState.CurrentChart.NoteList[noteID].Type == (int)NoteType.CDragChild ||
                             GlobalState.CurrentChart.NoteList[noteID].Type == (int)NoteType.DragChild)
                    {
                        tick = GlobalState.Clamp(tick,
                            GlobalState.CurrentChart.NoteList[GameLogic.GetDragParent(noteID)].Tick,
                            GlobalState.CurrentChart.NoteList[noteID].NextID >= 0
                                ? GlobalState.CurrentChart.NoteList[GlobalState.CurrentChart.NoteList[noteID].NextID]
                                    .Tick
                                : 0);
                    }

                    GlobalState.CurrentChart.NoteList[noteID].Tick = tick;

                    var id = noteID;
                    while (id + 1 < GlobalState.CurrentChart.NoteList.Count &&
                           GlobalState.CurrentChart.NoteList[id].Tick > GlobalState.CurrentChart.NoteList[id + 1].Tick)
                    {
                        var dragParent = GameLogic.GetDragParent(id);
                        if (dragParent > -1)
                        {
                            GlobalState.CurrentChart.NoteList[dragParent].NextID++;
                        }

                        var aux = GlobalState.CurrentChart.NoteList[id];
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
            _notes.Add(note.ID);
            if (!_changeNoteAR)
            {
                _changeNoteAR = true;
                NoteARLabel.SetActive(true);
                NoteARInputField.SetActive(true);
            }

            if (!_changeNoteXPosition)
            {
                _changeNoteXPosition = true;
                NoteXLabel.SetActive(true);
                NoteXInputField.SetActive(true);
            }

            if (!_changeNoteY)
            {
                _changeNoteY = true;
                NoteYLabel.SetActive(true);
                NoteYInputField.SetActive(true);
            }

            if (_notes.Count == 1)
            {
                _notesX = GlobalState.CurrentChart.NoteList[_notes[0]].X;
                NoteXInputField.GetComponent<InputField>().text =
                    GlobalState.CurrentChart.NoteList[_notes[0]].X.ToString("F3");

                _notesAR = GlobalState.CurrentChart.NoteList[_notes[0]].ApproachRate;
                NoteARInputField.GetComponent<InputField>().text =
                    GlobalState.CurrentChart.NoteList[_notes[0]].ApproachRate.ToString("F3");

                _notesY = GlobalState.CurrentChart.NoteList[_notes[0]].Y;
                NoteYInputField.GetComponent<InputField>().text =
                    GlobalState.CurrentChart.NoteList[_notes[0]].Y.ToString("F3");
            }
            else
            {
                if (_notesX < -0.6)
                {
                    NoteXInputField.GetComponent<InputField>().text = "";
                }
                else if (Math.Abs(GlobalState.CurrentChart.NoteList[_notes[^1]].X - _notesX) < 0.001)
                {
                    NoteXInputField.GetComponent<InputField>().text = _notesX.ToString("F3");
                }
                else
                {
                    _notesX = -1;
                    NoteXInputField.GetComponent<InputField>().text = "";
                }

                if (_notesAR < -0.6)
                {
                    NoteARInputField.GetComponent<InputField>().text = "";
                }
                else if (Math.Abs(GlobalState.CurrentChart.NoteList[_notes[^1]].ApproachRate - _notesAR) <
                         0.001)
                {
                    NoteARInputField.GetComponent<InputField>().text = _notesAR.ToString("F3");
                }
                else
                {
                    _notesAR = -1;
                    NoteARInputField.GetComponent<InputField>().text = "";
                }

                if (_notesY < -0.6)
                {
                    NoteYInputField.GetComponent<InputField>().text = "";
                }
                else if (Math.Abs(GlobalState.CurrentChart.NoteList[_notes[^1]].Y - _notesY) < 0.001)
                {
                    NoteYInputField.GetComponent<InputField>().text = _notesY.ToString("F3");
                }
                else
                {
                    _notesY = -1;
                    NoteYInputField.GetComponent<InputField>().text = "";
                }
            }
        }

        public void Remove(Note note)
        {
            _notes.Remove(note.ID);
            if (_notes.Count == 0)
            {
                _changeNoteAR = _changeNoteXPosition = _changeNoteY = false;
                NoteXLabel.SetActive(false);
                NoteXInputField.SetActive(false);
                NoteARLabel.SetActive(false);
                NoteARInputField.SetActive(false);
                NoteYInputField.SetActive(false);
                NoteYLabel.SetActive(false);
                _notesX = _notesAR = _notesY = -1;
            }
            else if (_notes.Count == 1)
            {
                _notesX = GlobalState.CurrentChart.NoteList[_notes[0]].X;
                NoteXInputField.GetComponent<InputField>().text =
                    GlobalState.CurrentChart.NoteList[_notes[0]].X.ToString("F3");

                _notesAR = GlobalState.CurrentChart.NoteList[_notes[0]].ApproachRate;
                NoteARInputField.GetComponent<InputField>().text =
                    GlobalState.CurrentChart.NoteList[_notes[0]].ApproachRate.ToString("F3");

                _notesY = GlobalState.CurrentChart.NoteList[_notes[0]].Y;
                NoteYInputField.GetComponent<InputField>().text =
                    GlobalState.CurrentChart.NoteList[_notes[0]].Y.ToString("F3");
            }
            else
            {
                var isNoteXSame = true;
                _notesX = GlobalState.CurrentChart.NoteList[_notes[0]].X;
                for (var i = 1; i < _notes.Count; i++)
                {
                    if (Math.Abs(GlobalState.CurrentChart.NoteList[_notes[^1]].X - _notesX) > 0.001)
                    {
                        isNoteXSame = false;
                        _notesX = -1;
                    }
                }

                if (_notesX < -0.6)
                {
                    NoteXInputField.GetComponent<InputField>().text = "";
                }
                else if (isNoteXSame)
                {
                    NoteXInputField.GetComponent<InputField>().text = _notesX.ToString("F3");
                }

                var isNoteARSame = true;
                _notesAR = GlobalState.CurrentChart.NoteList[_notes[0]].ApproachRate;
                for (var i = 1; i < _notes.Count; i++)
                {
                    if (Math.Abs(GlobalState.CurrentChart.NoteList[_notes[^1]].ApproachRate - _notesAR) >
                        0.001)
                    {
                        isNoteARSame = false;
                        _notesAR = -1;
                    }
                }

                if (_notesAR < -0.6)
                {
                    NoteARInputField.GetComponent<InputField>().text = "";
                }
                else if (isNoteARSame)
                {
                    NoteARInputField.GetComponent<InputField>().text = _notesAR.ToString("F3");
                }

                var isNoteYSame = true;
                _notesY = GlobalState.CurrentChart.NoteList[_notes[0]].Y;
                for (var i = 1; i < _notes.Count; i++)
                {
                    if (Math.Abs(GlobalState.CurrentChart.NoteList[_notes[^1]].Y - _notesY) > 0.001)
                    {
                        isNoteYSame = false;
                        _notesY = -1;
                    }
                }

                if (_notesY < -0.6)
                {
                    NoteARInputField.GetComponent<InputField>().text = "";
                }
                else if (isNoteYSame)
                {
                    NoteYInputField.GetComponent<InputField>().text = _notesY.ToString("F3");
                }
            }
        }

        public void Clear()
        {
            _changeNoteAR = _changeNoteXPosition = _changeNoteY = false;
            _notesX = _notesAR = _notesY = -1;
            NoteXLabel.SetActive(false);
            NoteXInputField.SetActive(false);
            NoteARLabel.SetActive(false);
            NoteARInputField.SetActive(false);
            NoteYInputField.SetActive(false);
            NoteYLabel.SetActive(false);
            _notes.Clear();
        }

        public void BlockInput()
        {
            GameLogic.BlockInput = true;
        }
    }
}