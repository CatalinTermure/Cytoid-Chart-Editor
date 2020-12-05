using System.Diagnostics;
using UnityEngine;

public class LongHoldNoteController : NoteController
{
    /// <summary>
    /// The parts of the hold note object.
    /// </summary>
    public GameObject NoteFill, NoteBorder, InnerNoteBorder, NoteHead;
    /// <summary>
    /// The parts of the hold note object.
    /// </summary>
    public GameObject TopHollowNoteBody, TopFillNoteBodyMask, TopFillNoteBody, BottomHollowNoteBody, BottomFillNoteBodyMask, BottomFillNoteBody, UpArrow, DownArrow, FinishIndicator;

    public Collider2D UpArrowCollider, DownArrowCollider;

    private float CompletionPercentage;

    /// <summary>
    /// The time the note needs to be held for.
    /// </summary>
    public float HoldTime;

    private float Size = 1;
    private float TopHeight, BottomHeight;

    public override void ChangeNoteColor(Color color)
    {
        NoteFill.GetComponent<SpriteRenderer>().color = color;
        BottomFillNoteBody.GetComponent<SpriteRenderer>().color = color;
        TopFillNoteBody.GetComponent<SpriteRenderer>().color = color;
        TopHollowNoteBody.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, color.a);
        BottomHollowNoteBody.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, color.a);
    }

    public override void Initialize(Note note)
    {
        NoteStopwatch = Stopwatch.StartNew();

        gameObject.transform.position = new Vector3((float)((note.x - 0.5) * GlobalState.PlayAreaWidth), (float)((note.y - 0.5) * GlobalState.PlayAreaHeight));

        Size = GlobalState.Config.DefaultNoteSize * (float)note.actual_size;
        NoteHead.transform.localScale = new Vector2(Size, Size);

        ApproachTime = (float)note.approach_time;

        TopHeight = (float)(1.0 - note.y) * GlobalState.PlayAreaHeight;
        TopFillNoteBody.GetComponent<SpriteRenderer>().size = new Vector2(Size, TopHeight);
        TopHollowNoteBody.GetComponent<SpriteRenderer>().size = new Vector2(Size, TopHeight);

        BottomHeight = (float)note.y * GlobalState.PlayAreaHeight;
        BottomFillNoteBody.GetComponent<SpriteRenderer>().size = new Vector2(Size, BottomHeight);
        BottomHollowNoteBody.GetComponent<SpriteRenderer>().size = new Vector2(Size, BottomHeight);

        CompletionPercentage = 0;
        TopFillNoteBodyMask.transform.localScale = BottomFillNoteBodyMask.transform.localScale = new Vector3(Size * 50, 0);

        HoldTime = (float)note.hold_time;

        Highlighted = true;
        Highlight();

        Notetype = note.type;
        NoteID = note.id;

        FinishIndicator.SetActive(!GlobalState.IsGameRunning);

        if(GlobalState.IsGameRunning)
        {
            TopHollowNoteBody.transform.localScale = new Vector3(0.2f, 1);
            BottomHollowNoteBody.transform.localScale = new Vector3(0.2f, 1);
            NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(0.6f, 0.6f);
            InnerNoteBorder.transform.localScale = new Vector3(0.4f, 0.4f);
        }
        else
        {
            ChangeToPausedVisuals();
        }
    }

    protected override void UpdateVisuals()
    {
        FinishIndicator.SetActive(false);
        ApproachPercentage = (Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f) / ApproachTime;
        if (ApproachPercentage < 1)
        {
            TopHollowNoteBody.transform.localScale = new Vector3(0.2f + 0.3f * ApproachPercentage, 1);
            BottomHollowNoteBody.transform.localScale = new Vector3(0.2f + 0.3f * ApproachPercentage, 1);
            NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(0.6f + 0.4f * ApproachPercentage, 0.6f + 0.4f * ApproachPercentage);
            InnerNoteBorder.transform.localScale = new Vector3(0.4f + 0.35f * ApproachPercentage, 0.4f + 0.35f * ApproachPercentage);
        }
        else
        {
            CompletionPercentage = (Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f - ApproachTime) / HoldTime;

            TopFillNoteBodyMask.transform.localScale = new Vector3(Size * 50, TopHeight * CompletionPercentage * 100);
            BottomFillNoteBodyMask.transform.localScale = new Vector3(Size * 50, BottomHeight * CompletionPercentage * 100);

            if (CompletionPercentage > 1)
            {
                NoteStopwatch.Stop();
                ParentPool.ReturnToPool(gameObject, Notetype);
            }
        }
    }

    protected override void ChangeToPausedVisuals()
    {
        TopHollowNoteBody.transform.localScale = new Vector3(0.5f, 1);
        BottomHollowNoteBody.transform.localScale = new Vector3(0.5f, 1);
        NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(1, 1);
        InnerNoteBorder.transform.localScale = new Vector3(0.75f, 0.75f);
        TopFillNoteBodyMask.transform.localScale = BottomFillNoteBodyMask.transform.localScale = new Vector3(Size * 50, 0);
        BottomFillNoteBodyMask.transform.localScale = BottomFillNoteBodyMask.transform.localScale = new Vector3(Size * 50, 0);
        FinishIndicator.SetActive(true);
    }

    public override void Highlight()
    {
        Highlighted = !Highlighted;
        HighlightBorder.SetActive(Highlighted);
        UpArrow.SetActive(Highlighted);
        DownArrow.SetActive(Highlighted);
    }
}
