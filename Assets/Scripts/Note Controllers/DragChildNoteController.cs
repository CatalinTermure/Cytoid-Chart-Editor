using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class DragChildNoteController : NoteController
{
    public GameObject NoteFill;

    public override void ChangeNoteColor(Color color)
    {
        NoteFill.GetComponent<SpriteRenderer>().color = color;
    }

    public override void Initialize(Note note)
    {
        NoteStopwatch = Stopwatch.StartNew();

        gameObject.transform.position = new Vector3((float)(GlobalState.PlayAreaWidth * (note.x - 0.5)), (float)(GlobalState.PlayAreaHeight * (note.y - 0.5)));
        gameObject.transform.localScale = new Vector3(GlobalState.Config.DefaultNoteSize * (float)note.size, GlobalState.Config.DefaultNoteSize * (float)note.size);

        ApproachTime = (float)note.approach_time;

        Highlighted = true;
        Highlight();

        NoteType = note.type;
        NoteID = note.id;

        if(GlobalState.IsGameRunning)
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
        ApproachPercentage = (Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f) / ApproachTime;

        NoteFill.transform.localScale = new Vector3(0.2f + ApproachPercentage * 0.2f, 0.2f + ApproachPercentage * 0.2f);

        if (ApproachPercentage > 1)
        {
            NoteStopwatch.Stop();
            ParentPool.ReturnToPool(gameObject, NoteType);
        }
    }

    protected override void ChangeToPausedVisuals()
    {
        NoteFill.transform.localScale = new Vector3(0.4f, 0.4f);
    }

    public override void Highlight()
    {
        Highlighted = !Highlighted;
        HighlightBorder.SetActive(Highlighted);
    }
}
