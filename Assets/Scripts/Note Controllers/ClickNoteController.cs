using System.Diagnostics;
using UnityEngine;

public class ClickNoteController : NoteController
{
    /// <summary>
    /// The parts of the click note.
    /// </summary>
    public GameObject NoteFill, NoteBorder;

    public override void ChangeNoteColor(Color color)
    {
        NoteFill.GetComponent<SpriteRenderer>().color = color;
        Color tmp = NoteBorder.GetComponent<SpriteRenderer>().color;
        NoteBorder.GetComponent<SpriteRenderer>().color = new Color(tmp.r, tmp.g, tmp.b, color.a);
    }

    public override void Initialize(Note note)
    {
        NoteStopwatch = Stopwatch.StartNew();

        gameObject.transform.position = new Vector3((float)(GlobalState.PlayAreaWidth * (note.x - 0.5)), (float)(GlobalState.PlayAreaHeight * (note.y - 0.5)));
        gameObject.transform.localScale = new Vector3(GlobalState.Config.DefaultNoteSize * (float)note.actual_size * (note.type == 0 ? 1 : 0.8f), GlobalState.Config.DefaultNoteSize * (float)note.actual_size * (note.type == 0 ? 1 : 0.8f));

        ApproachTime = (float)note.approach_time;

        Highlighted = true;
        Highlight();

        Notetype = note.type;
        NoteID = note.id;

        if(GlobalState.IsGameRunning)
        {
            NoteFill.transform.localScale = new Vector3(0, 0);
            NoteBorder.transform.localScale = new Vector3(0.6f, 0.6f);
        }
        else
        {
            ChangeToPausedVisuals();
        }
    }

    protected override void UpdateVisuals()
    {
        ApproachPercentage = (Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f) / ApproachTime;

        NoteFill.transform.localScale = new Vector3(ApproachPercentage, ApproachPercentage);
        NoteBorder.transform.localScale = new Vector3(0.6f + ApproachPercentage * 0.4f, 0.6f + ApproachPercentage * 0.4f);

        if (ApproachPercentage > 1)
        {
            NoteStopwatch.Stop();
            ParentPool.ReturnToPool(gameObject, Notetype);
        }
    }

    protected override void ChangeToPausedVisuals()
    {
        NoteFill.transform.localScale = new Vector3(1, 1);
        NoteBorder.transform.localScale = new Vector3(1, 1);
    }

    public override void Highlight()
    {
        Highlighted = !Highlighted;
        HighlightBorder.SetActive(Highlighted);
    }
}
