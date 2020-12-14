using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class DragChildNoteController : NoteController
{
    public GameObject NoteFill, DragConnector;

    private int NextID;

    public override void ChangeNoteColor(Color color)
    {
        NoteFill.GetComponent<SpriteRenderer>().color = color;
    }

    public override void Initialize(Note note)
    {
        NoteStopwatch = Stopwatch.StartNew();

        gameObject.transform.position = new Vector3((float)(GlobalState.PlayAreaWidth * (note.x - 0.5)), (float)(GlobalState.PlayAreaHeight * (note.y - 0.5)));
        gameObject.transform.localScale = new Vector3(GlobalState.Config.DefaultNoteSize * (float)note.actual_size, GlobalState.Config.DefaultNoteSize * (float)note.actual_size);

        ApproachTime = (float)note.approach_time;

        Highlighted = true;
        Highlight();

        Notetype = note.type;
        NoteID = note.id;

        NextID = GlobalState.CurrentChart.note_list[NoteID].next_id;
        if(NextID > 0)
        {
            float x1 = (float)(GlobalState.CurrentChart.note_list[NoteID].x - 0.5) * GlobalState.PlayAreaWidth, y1 = (float)(GlobalState.CurrentChart.note_list[NoteID].y - 0.5) * GlobalState.PlayAreaHeight;
            float x2 = (float)(GlobalState.CurrentChart.note_list[NextID].x - 0.5) * GlobalState.PlayAreaWidth, y2 = (float)(GlobalState.CurrentChart.note_list[NextID].y - 0.5) * GlobalState.PlayAreaHeight;
            DragConnector.transform.rotation = Quaternion.AngleAxis(90 + (float)(System.Math.Atan2(y1 - y2, x1 - x2) * 180 / System.Math.PI), Vector3.forward);
            DragConnector.GetComponent<SpriteRenderer>().size = new Vector2(0.175f, GlobalState.GetDistance(x2, y2, x1, y1) / gameObject.transform.localScale.x);
            DragConnector.SetActive(true);
        }
        else
        {
            DragConnector.SetActive(false);
        }

        if (GlobalState.IsGameRunning)
        {
            NoteFill.transform.localScale = new Vector3(0.2f, 0.2f);
        }
        else
        {
            ChangeToPausedVisuals();
        }
    }

    protected override void UpdateVisuals()
    {
        if(!DragConnector.activeSelf && NextID > 0)
        {
            DragConnector.SetActive(true);
        }
        ApproachPercentage = (Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f) / ApproachTime;

        NoteFill.transform.localScale = new Vector3(0.2f + ApproachPercentage * 0.2f, 0.2f + ApproachPercentage * 0.2f);

        if (ApproachPercentage > 1)
        {
            NoteStopwatch.Stop();
            ParentPool.ReturnToPool(gameObject, Notetype);
        }
    }

    protected override void ChangeToPausedVisuals()
    {
        if(GlobalState.CurrentChart.note_list[NoteID].page_index != GameObject.Find("UICanvas").GetComponent<GameLogic>().CurrentPageIndex && NextID > 0)
        {
            DragConnector.SetActive(false);
        }

        NoteFill.transform.localScale = new Vector3(0.4f, 0.4f);
    }

    public override void Highlight()
    {
        Highlighted = !Highlighted;
        HighlightBorder.SetActive(Highlighted);
    }
}
