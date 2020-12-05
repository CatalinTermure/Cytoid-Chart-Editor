using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class DragHeadNoteController : NoteController
{
    /// <summary>
    /// The parts of the note.
    /// </summary>
    public GameObject NoteFill, NoteBorder, DragConnector;

    public float StartTime;
    public int NextID;

    struct PathPoint
    {
        public float x, y, time, connector_start_time;
    }
    private readonly List<PathPoint> Paths = new List<PathPoint>();
    private int CurrentPath = 0;

    public override void Initialize(Note note)
    {
        NoteStopwatch = Stopwatch.StartNew();

        gameObject.transform.position = new Vector3((float)(GlobalState.PlayAreaWidth * (note.x - 0.5)), (float)(GlobalState.PlayAreaHeight * (note.y - 0.5)));
        gameObject.transform.localScale = new Vector3(GlobalState.Config.DefaultNoteSize * (float)note.actual_size, GlobalState.Config.DefaultNoteSize * (float)note.actual_size);

        ApproachTime = (float)note.approach_time;

        NextID = note.next_id;
        StartTime = (float)note.time;

        Notetype = note.type;
        NoteID = note.id;

        Highlighted = true;
        Highlight();

        Paths.Clear();

        CurrentPath = 0;

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

    public void GeneratePath()
    {
        Paths.Add(new PathPoint
        {
            x = gameObject.transform.position.x,
            y = gameObject.transform.position.y,
            time = ApproachTime
        });
        while(NextID > 0)
        {
            Paths.Add(new PathPoint
            {
                x = (float)(GlobalState.CurrentChart.note_list[NextID].x - 0.5) * GlobalState.PlayAreaWidth,
                y = (float)(GlobalState.CurrentChart.note_list[NextID].y - 0.5) * GlobalState.PlayAreaHeight,
                time = (float)(GlobalState.CurrentChart.note_list[NextID].time - StartTime + ApproachTime),
                connector_start_time = (float)(GlobalState.CurrentChart.note_list[NextID].time - GlobalState.CurrentChart.note_list[NextID].approach_time - StartTime + ApproachTime),
            });

            NextID = GlobalState.CurrentChart.note_list[NextID].next_id;
        }
        if(Paths.Count > 1)
        {
            DragConnector.GetComponent<SpriteRenderer>().size = new Vector2(0.175f, GlobalState.GetDistance(Paths[1].x, Paths[1].y, Paths[0].x, Paths[0].y) / 2);
            if(Notetype == (int)NoteType.CDRAG_HEAD)
            {
                gameObject.transform.rotation = Quaternion.AngleAxis(90 + (float)(Math.Atan2(Paths[0].y - Paths[1].y, Paths[0].x - Paths[1].x) * 180 / Math.PI), Vector3.forward);
            }
            else
            {
                DragConnector.transform.rotation = Quaternion.AngleAxis(90 + (float)(Math.Atan2(Paths[0].y - Paths[1].y, Paths[0].x - Paths[1].x) * 180 / Math.PI), Vector3.forward);
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
        Color tmp = NoteBorder.GetComponent<SpriteRenderer>().color;
        NoteBorder.GetComponent<SpriteRenderer>().color = new Color(tmp.r, tmp.g, tmp.b, color.a);
    }

    protected override void UpdateVisuals()
    {
        float time = Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f;
        ApproachPercentage = time / ApproachTime;

        if (!DragConnector.activeSelf && NextID > 0)
        {
            DragConnector.SetActive(true);
        }

        if (ApproachPercentage > 1)
        {
            NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(0.8f, 0.8f);

            if (CurrentPath < Paths.Count)
            {
                float pathcompletion = (Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f - (CurrentPath > 0 ? Paths[CurrentPath - 1].time : 0)) /
                    (Paths[CurrentPath].time - (CurrentPath > 0 ? Paths[CurrentPath - 1].time : 0));
                
                while(float.IsInfinity(pathcompletion))
                {
                    CurrentPath++;
                    if(CurrentPath < Paths.Count)
                    {
                        pathcompletion = (Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f - (CurrentPath > 0 ? Paths[CurrentPath - 1].time : 0)) /
                        (Paths[CurrentPath].time - (CurrentPath > 0 ? Paths[CurrentPath - 1].time : 0));

                        gameObject.transform.rotation = Quaternion.AngleAxis(90 + (float)(Math.Atan2(Paths[CurrentPath - 1].y - Paths[CurrentPath].y,
                                Paths[CurrentPath - 1].x - Paths[CurrentPath].x) * 180 / Math.PI), Vector3.forward);
                    }
                    else
                    {
                        pathcompletion = 0;
                    }
                }

                while (pathcompletion > 1)
                {
                    if (CurrentPath > 0 && CurrentPath + 1 < Paths.Count && Notetype == (int)NoteType.CDRAG_HEAD)
                    {
                        gameObject.transform.rotation = Quaternion.AngleAxis(90 + (float)(Math.Atan2(Paths[CurrentPath].y - Paths[CurrentPath + 1].y,
                            Paths[CurrentPath].x - Paths[CurrentPath + 1].x) * 180 / Math.PI), Vector3.forward);
                    }

                    CurrentPath++;
                    if(CurrentPath < Paths.Count)
                    {
                        pathcompletion = (Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f - (CurrentPath > 0 ? Paths[CurrentPath - 1].time : 0)) /
                                (Paths[CurrentPath].time - (CurrentPath > 0 ? Paths[CurrentPath - 1].time : 0));
                    }
                    else
                    {
                        pathcompletion = 0;
                    }
                }

                if(CurrentPath < Paths.Count)
                {
                    gameObject.transform.position = new Vector3(Paths[CurrentPath - 1].x + pathcompletion * (Paths[CurrentPath].x - Paths[CurrentPath - 1].x),
                        Paths[CurrentPath - 1].y + pathcompletion * (Paths[CurrentPath].y - Paths[CurrentPath - 1].y));

                    if (CurrentPath > 0)
                    {
                        DragConnector.GetComponent<SpriteRenderer>().size = new Vector2(0.175f, (1.0f - pathcompletion) *
                            GlobalState.GetDistance(Paths[CurrentPath - 1].x, Paths[CurrentPath - 1].y, Paths[CurrentPath].x, Paths[CurrentPath].y) / 2);

                        DragConnector.transform.rotation = Quaternion.AngleAxis(90 + (float)(Math.Atan2(Paths[CurrentPath - 1].y - Paths[CurrentPath].y, Paths[CurrentPath - 1].x - Paths[CurrentPath].x) * 180 / Math.PI), Vector3.forward);
                    }
                }
            }

            if (CurrentPath >= Paths.Count)
            {
                ParentPool.ReturnToPool(gameObject, Notetype);
            }
        }
        else
        {
            NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(0.4f + ApproachPercentage * 0.4f, 0.4f + ApproachPercentage * 0.4f);
        }
    }

    protected override void ChangeToPausedVisuals()
    {
        if (GlobalState.CurrentChart.note_list[NoteID].page_index != GameObject.Find("UICanvas").GetComponent<GameLogic>().CurrentPageIndex && NextID > 0)
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
}
