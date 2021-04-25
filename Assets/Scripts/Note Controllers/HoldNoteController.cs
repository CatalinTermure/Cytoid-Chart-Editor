using System.Diagnostics;
using CCE;
using CCE.Core;
using UnityEngine;
using CCE.Data;

public class HoldNoteController : NoteController
{
    /// <summary>
    /// The parts of the hold note object.
    /// </summary>
    public GameObject NoteFill, NoteBorder, InnerNoteBorder, HollowNoteBody, FillNoteBodyMask, FillNoteBody, NoteHead, UpArrow, DownArrow;

    public BoxCollider2D UpArrowCollider, DownArrowCollider;

    private float CompletionPercentage;

    /// <summary>
    /// The time the note needs to be held for.
    /// </summary>
    [HideInInspector]
    public float HoldTime;

    private float Height = 3;
    private float Size = 1;

    public override void ChangeNoteColor(Color color)
    {
        NoteFill.GetComponent<SpriteRenderer>().color = color;
        FillNoteBody.GetComponent<SpriteRenderer>().color = color;
        HollowNoteBody.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, color.a);
        InnerNoteBorder.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, color.a);
        NoteBorder.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, color.a);
    }

    public override void Initialize(Note note)
    {
        NoteStopwatch = Stopwatch.StartNew();

        gameObject.transform.position = new Vector3((float)((note.X - 0.5) * GlobalState.PlayAreaWidth), (float)((note.Y - 0.5) * GlobalState.PlayAreaHeight));

        Size = GlobalState.Config.DefaultNoteSize * (float)note.ActualSize;
        NoteHead.transform.localScale = new Vector2(Size, Size);
        NoteHead.transform.localPosition = new Vector3(0, Size);

        ApproachTime = (float)note.ApproachTime;

        Height = (float)(GlobalState.PlayAreaHeight * note.HoldTick / GlobalState.CurrentChart.PageList[note.PageIndex].PageSize);
        FillNoteBody.GetComponent<SpriteRenderer>().size = new Vector2(Size, Height);
        HollowNoteBody.GetComponent<SpriteRenderer>().size = new Vector2(Size, Height);

        HoldTime = (float)note.HoldTime;

        if (GlobalState.CurrentChart.PageList[note.PageIndex].ScanLineDirection == -1)
        {
            transform.rotation = new Quaternion(0, 0, 1, 0);
        }
        else
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }

        CompletionPercentage = 0;
        FillNoteBodyMask.transform.localScale = new Vector3(Size * 50, 0);

        Highlighted = true;
        Highlight();

        Notetype = note.Type;
        NoteID = note.ID;

        if (GlobalState.IsGameRunning)
        {
            NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(0.6f, 0.6f);
            InnerNoteBorder.transform.localScale = new Vector3(0.4f, 0.4f);
            HollowNoteBody.transform.localScale = new Vector3(0.2f, 1);
        }
        else
        {
            ChangeToPausedVisuals();
        }
    }

    protected override void UpdateVisuals()
    {
        ApproachPercentage = (Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f) / ApproachTime;
        if (ApproachPercentage < 1)
        {
            HollowNoteBody.transform.localScale = new Vector3(0.2f + 0.3f * ApproachPercentage, 1);
            NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(0.6f + 0.4f * ApproachPercentage, 0.6f + 0.4f * ApproachPercentage);
            InnerNoteBorder.transform.localScale = new Vector3(0.4f + 0.35f * ApproachPercentage, 0.4f + 0.35f * ApproachPercentage);
        }
        else
        {
            CompletionPercentage = (Delay + NoteStopwatch.ElapsedMilliseconds * PlaybackSpeed / 1000f - ApproachTime) / HoldTime;

            FillNoteBodyMask.transform.localScale = new Vector3(Size * 50, Height * CompletionPercentage * 100);

            if (CompletionPercentage > 1)
            {
                NoteStopwatch.Stop();
                ParentPool.ReturnToPool(gameObject, Notetype);
            }
        }
    }

    protected override void ChangeToPausedVisuals()
    {
        HollowNoteBody.transform.localScale = new Vector3(0.5f, 1);
        NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(1, 1);
        InnerNoteBorder.transform.localScale = new Vector3(0.75f, 0.75f);

        FillNoteBodyMask.transform.localScale = new Vector3(Size * 50, 0);
    }

    public override void Highlight()
    {
        Highlighted = !Highlighted;
        HighlightBorder.SetActive(Highlighted);
        UpArrow.SetActive(Highlighted);
        DownArrow.SetActive(Highlighted);
    }
}
