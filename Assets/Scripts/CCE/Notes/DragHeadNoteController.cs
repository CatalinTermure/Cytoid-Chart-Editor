using System;
using System.Collections.Generic;
using System.Diagnostics;
using CCE.Core;
using CCE.Data;
using CCE.Game;
using CCE.Utils;
using UnityEngine;

namespace CCE.Notes
{
    public class DragHeadNoteController : NoteController
    {
        /// <summary>
        ///     The parts of the note.
        /// </summary>
        public GameObject NoteFill, NoteBorder, DragConnector;

        public float StartTime;
        public int NextID;
        private readonly List<PathPoint> _paths = new();
        private int _currentPath;

        public override void Initialize(Note note)
        {
            NoteStopwatch = Stopwatch.StartNew();

            gameObject.transform.position = new Vector3((float)(GlobalState.PlayAreaWidth * (note.X - 0.5)),
                (float)(GlobalState.PlayAreaHeight * (note.Y - 0.5)));
            gameObject.transform.localScale = new Vector3(GlobalState.Config.DefaultNoteSize * (float)note.ActualSize,
                GlobalState.Config.DefaultNoteSize * (float)note.ActualSize);

            ApproachTime = (float)note.ApproachTime;

            NextID = note.NextID;
            StartTime = (float)note.Time;

            NoteType = note.Type;
            NoteID = note.ID;

            Highlighted = true;
            Highlight();

            _paths.Clear();

            _currentPath = 0;

            GeneratePath();

            if (GlobalState.IsGameRunning)
            {
                NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(0.4f, 0.4f);
            }
            else
            {
                ChangeToPausedVisuals();
            }
        }

        private void GeneratePath()
        {
            _paths.Add(new PathPoint
            {
                X = gameObject.transform.position.x,
                Y = gameObject.transform.position.y,
                Time = ApproachTime
            });
            while (NextID > 0)
            {
                _paths.Add(new PathPoint
                {
                    X = (float)(GlobalState.CurrentChart.NoteList[NextID].X - 0.5) * GlobalState.PlayAreaWidth,
                    Y = (float)(GlobalState.CurrentChart.NoteList[NextID].Y - 0.5) * GlobalState.PlayAreaHeight,
                    Time = (float)(GlobalState.CurrentChart.NoteList[NextID].Time - StartTime + ApproachTime)
                });

                NextID = GlobalState.CurrentChart.NoteList[NextID].NextID;
            }

            if (_paths.Count > 1)
            {
                DragConnector.GetComponent<SpriteRenderer>().size = new Vector2(0.175f,
                    MiscUtils.GetDistance(_paths[1].X, _paths[1].Y, _paths[0].X, _paths[0].Y) /
                    gameObject.transform.localScale.x);
                if (NoteType == (int)Data.NoteType.CDragHead)
                {
                    gameObject.transform.rotation = Quaternion.AngleAxis(
                        90 + (float)(Math.Atan2(_paths[0].Y - _paths[1].Y, _paths[0].X - _paths[1].X) * 180 / Math.PI),
                        Vector3.forward);
                }
                else
                {
                    DragConnector.transform.rotation = Quaternion.AngleAxis(
                        90 + (float)(Math.Atan2(_paths[0].Y - _paths[1].Y, _paths[0].X - _paths[1].X) * 180 / Math.PI),
                        Vector3.forward);
                }

                DragConnector.SetActive(true);
            }
            else
            {
                DragConnector.SetActive(false);
            }
        }

        public override void ChangeNoteColor(Color color)
        {
            NoteFill.GetComponent<SpriteRenderer>().color = color;
            var tmp = NoteBorder.GetComponent<SpriteRenderer>().color;
            NoteBorder.GetComponent<SpriteRenderer>().color = new Color(tmp.r, tmp.g, tmp.b, color.a);
        }

        protected override void UpdateVisuals()
        {
            var time = Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f;
            ApproachPercentage = time / ApproachTime;

            if (!DragConnector.activeSelf && NextID > 0)
            {
                DragConnector.SetActive(true);
            }

            if (ApproachPercentage > 1)
            {
                NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(0.8f, 0.8f);

                if (_currentPath < _paths.Count)
                {
                    var pathCompletion = (Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f -
                                          (_currentPath > 0 ? _paths[_currentPath - 1].Time : 0)) /
                                         (_paths[_currentPath].Time -
                                          (_currentPath > 0 ? _paths[_currentPath - 1].Time : 0));

                    while (float.IsInfinity(pathCompletion))
                    {
                        _currentPath++;
                        if (_currentPath < _paths.Count)
                        {
                            pathCompletion = (Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f -
                                              (_currentPath > 0 ? _paths[_currentPath - 1].Time : 0)) /
                                             (_paths[_currentPath].Time -
                                              (_currentPath > 0 ? _paths[_currentPath - 1].Time : 0));

                            gameObject.transform.rotation = Quaternion.AngleAxis(90 + (float)(Math.Atan2(
                                _paths[_currentPath - 1].Y - _paths[_currentPath].Y,
                                _paths[_currentPath - 1].X - _paths[_currentPath].X) * 180 / Math.PI), Vector3.forward);
                        }
                        else
                        {
                            pathCompletion = 0;
                        }
                    }

                    while (pathCompletion > 1)
                    {
                        if (_currentPath > 0 && _currentPath + 1 < _paths.Count &&
                            NoteType == (int)Data.NoteType.CDragHead)
                        {
                            gameObject.transform.rotation = Quaternion.AngleAxis(90 + (float)(Math.Atan2(
                                _paths[_currentPath].Y - _paths[_currentPath + 1].Y,
                                _paths[_currentPath].X - _paths[_currentPath + 1].X) * 180 / Math.PI), Vector3.forward);
                        }

                        _currentPath++;
                        if (_currentPath < _paths.Count)
                        {
                            pathCompletion = (Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f -
                                              (_currentPath > 0 ? _paths[_currentPath - 1].Time : 0)) /
                                             (_paths[_currentPath].Time -
                                              (_currentPath > 0 ? _paths[_currentPath - 1].Time : 0));
                        }
                        else
                        {
                            pathCompletion = 0;
                        }
                    }

                    if (_currentPath < _paths.Count)
                    {
                        gameObject.transform.position = new Vector3(
                            _paths[_currentPath - 1].X +
                            pathCompletion * (_paths[_currentPath].X - _paths[_currentPath - 1].X),
                            _paths[_currentPath - 1].Y +
                            pathCompletion * (_paths[_currentPath].Y - _paths[_currentPath - 1].Y));

                        if (_currentPath > 0)
                        {
                            DragConnector.GetComponent<SpriteRenderer>().size = new Vector2(0.175f,
                                (1.0f - pathCompletion) *
                                MiscUtils.GetDistance(_paths[_currentPath - 1].X, _paths[_currentPath - 1].Y,
                                    _paths[_currentPath].X,
                                    _paths[_currentPath].Y) / gameObject.transform.localScale.x);

                            DragConnector.transform.rotation = Quaternion.AngleAxis(
                                90 + (float)(Math.Atan2(_paths[_currentPath - 1].Y - _paths[_currentPath].Y,
                                    _paths[_currentPath - 1].X - _paths[_currentPath].X) * 180 / Math.PI),
                                Vector3.forward);
                        }
                    }
                }

                if (_currentPath >= _paths.Count)
                {
                    ParentPool.ReturnToPool(gameObject, NoteType);
                }
            }
            else
            {
                NoteFill.transform.localScale = NoteBorder.transform.localScale =
                    new Vector3(0.4f + ApproachPercentage * 0.4f, 0.4f + ApproachPercentage * 0.4f);
            }
        }

        protected override void ChangeToPausedVisuals()
        {
            if (GlobalState.CurrentChart.NoteList[NoteID].PageIndex !=
                GameObject.Find("UICanvas").GetComponent<GameLogic>().CurrentPageIndex && NextID > 0)
            {
                DragConnector.SetActive(false);
            }

            NoteBorder.transform.localScale = NoteFill.transform.localScale = new Vector3(0.8f, 0.8f);
        }

        public override void Highlight()
        {
            Highlighted = !Highlighted;
            HighlightBorder.SetActive(Highlighted);
        }

        private struct PathPoint
        {
            public float X, Y, Time;
        }
    }
}