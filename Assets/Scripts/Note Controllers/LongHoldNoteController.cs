using System.Diagnostics;
using UnityEngine;

public class LongHoldNoteController : MonoBehaviour, IHighlightable, INote
{
    /// <summary>
    /// Stopwatch for keeping track of the animation time.
    /// </summary>
    private Stopwatch sw;

    /// <summary>
    /// The parts of the hold note object.
    /// </summary>
    public GameObject NoteFill, NoteBorder, InnerNoteBorder, NoteHead;
    /// <summary>
    /// The parts of the hold note object.
    /// </summary>
    public GameObject TopHollowNoteBody, TopFillNoteBodyMask, TopFillNoteBody, BottomHollowNoteBody, BottomFillNoteBodyMask, BottomFillNoteBody, UpArrow, DownArrow, FinishIndicator;

    public Collider2D UpArrowCollider, DownArrowCollider;

    public GameObject HighlightBorder;

    /// <summary>
    /// Time it takes from the note appearing on screen to it's start time.
    /// </summary>
    public float ApproachTime;

    /// <summary>
    /// Time delay from the time the note should first appear to when the stopwatch starts.
    /// </summary>
    private float Delay;

    public float PlaybackSpeed;

    private float ApproachPercentage, CompletionPercentage;

    /// <summary>
    /// The time the note needs to be held for.
    /// </summary>
    public float HoldTime;

    private float Size = 1;
    private float TopHeight, BottomHeight;

    private bool started = false;

    public bool Highlighted { get; set; }
    public int NoteType { get; set; }
    public int NoteID { get; set; }


    private void Awake()
    {
        Highlighted = false;
        HighlightBorder.SetActive(false);
        UpArrow.SetActive(false);
        DownArrow.SetActive(false);
        FinishIndicator.SetActive(false);
    }

    void Start()
    {
        sw = Stopwatch.StartNew();
        UpdateComponentVisuals();
        started = true;
    }

    public void ChangeNoteColor(Color color)
    {
        NoteFill.GetComponent<SpriteRenderer>().color = color;
        BottomFillNoteBody.GetComponent<SpriteRenderer>().color = color;
        TopFillNoteBody.GetComponent<SpriteRenderer>().color = color;
        TopHollowNoteBody.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, color.a);
        BottomHollowNoteBody.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, color.a);
    }

    public void SetTopHeight(float height)
    {
        TopHeight = height;
        TopFillNoteBody.GetComponent<SpriteRenderer>().size = new Vector2(Size, height);
        TopHollowNoteBody.GetComponent<SpriteRenderer>().size = new Vector2(Size, height);
    }

    public void SetBottomHeight(float height)
    {
        BottomHeight = height;
        BottomFillNoteBody.GetComponent<SpriteRenderer>().size = new Vector2(Size, height);
        BottomHollowNoteBody.GetComponent<SpriteRenderer>().size = new Vector2(Size, height);
    }

    public void SetSize(float size)
    {
        NoteHead.transform.localScale = new Vector2(size, size);
        Size = size;
    }

    public void SetDelay(float delay)
    {
        Delay = delay;
        if(started) UpdateComponentVisuals();
    }

    private void FixedUpdate()
    {
        if (GlobalState.IsGameRunning)
        {
            if (!sw.IsRunning)
            {
                sw.Start();
            }
        }
        else
        {
            sw.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateComponentVisuals();
    }

    private void UpdateComponentVisuals()
    {
        if(GlobalState.IsGameRunning)
        {
            ApproachPercentage = (Delay + sw.ElapsedMilliseconds * PlaybackSpeed / 1000f) / ApproachTime;
            if (ApproachPercentage < 1)
            {
                TopHollowNoteBody.transform.localScale = new Vector3(0.2f + 0.3f * ApproachPercentage, 1);
                BottomHollowNoteBody.transform.localScale = new Vector3(0.2f + 0.3f * ApproachPercentage, 1);
                NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(0.6f + 0.4f * ApproachPercentage, 0.6f + 0.4f * ApproachPercentage);
                InnerNoteBorder.transform.localScale = new Vector3(0.4f + 0.35f * ApproachPercentage, 0.4f + 0.35f * ApproachPercentage);
            }
            else
            {
                CompletionPercentage = (Delay + sw.ElapsedMilliseconds * PlaybackSpeed / 1000f - ApproachTime) / HoldTime;

                TopFillNoteBodyMask.transform.localScale = new Vector3(Size * 50, TopHeight * CompletionPercentage * 100);
                BottomFillNoteBodyMask.transform.localScale = new Vector3(Size * 50, BottomHeight * CompletionPercentage * 100);

                if (CompletionPercentage > 1)
                {
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            TopHollowNoteBody.transform.localScale = new Vector3(0.5f, 1);
            BottomHollowNoteBody.transform.localScale = new Vector3(0.5f, 1);
            NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(1, 1);
            InnerNoteBorder.transform.localScale = new Vector3(0.75f, 0.75f);
            TopFillNoteBodyMask.transform.localScale = BottomFillNoteBodyMask.transform.localScale = new Vector3(Size * 50, 0);
        }
    }

    public void Highlight()
    {
        Highlighted = !Highlighted;
        HighlightBorder.SetActive(Highlighted);
        UpArrow.SetActive(Highlighted);
        DownArrow.SetActive(Highlighted);
        FinishIndicator.SetActive(Highlighted);
    }
}
