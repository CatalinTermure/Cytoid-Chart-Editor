using System.Diagnostics;
using UnityEngine;

public class HoldNoteController : MonoBehaviour, IHighlightable, INote
{
    /// <summary>
    /// Stopwatch for keeping track of the animation time.
    /// </summary>
    private Stopwatch sw;

    /// <summary>
    /// The parts of the hold note object.
    /// </summary>
    public GameObject NoteFill, NoteBorder, InnerNoteBorder, HollowNoteBody, FillNoteBodyMask, FillNoteBody, NoteHead, HighlightBorder, UpArrow, DownArrow;

    public Collider2D UpArrowCollider, DownArrowCollider;

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

    private float Height = 3;
    private float Size = 1;

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
    }

    void Start()
    {
        sw = Stopwatch.StartNew();
        started = true;
        UpdateComponentVisuals();
    }

    public void ChangeNoteColor(Color color)
    {
        NoteFill.GetComponent<SpriteRenderer>().color = color;
        FillNoteBody.GetComponent<SpriteRenderer>().color = color;
        HollowNoteBody.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, color.a);
        InnerNoteBorder.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, color.a);
        NoteBorder.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, color.a);
    }

    public void SetHeight(float height)
    {
        Height = height;
        FillNoteBody.GetComponent<SpriteRenderer>().size = new Vector2(Size, height);
        HollowNoteBody.GetComponent<SpriteRenderer>().size = new Vector2(Size, height);
    }

    public void SetSize(float size)
    {
        NoteHead.transform.localScale = new Vector2(size, size);
        NoteHead.transform.localPosition = new Vector3(0, size);
        Size = size;
    }

    public void SetDelay(float delay)
    {
        Delay = delay;
        if(started) UpdateComponentVisuals();
    }

    public void Flip()
    {
        transform.rotation = new Quaternion(0, 0, 1, 0);
    }

    private void FixedUpdate()
    {
        if(GlobalState.IsGameRunning)
        {
            if(!sw.IsRunning)
            {
                sw.Start();
            }
        }
        else
        {
            sw.Stop();
        }
    }

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
                HollowNoteBody.transform.localScale = new Vector3(0.2f + 0.3f * ApproachPercentage, 1);
                NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(0.6f + 0.4f * ApproachPercentage, 0.6f + 0.4f * ApproachPercentage);
                InnerNoteBorder.transform.localScale = new Vector3(0.4f + 0.35f * ApproachPercentage, 0.4f + 0.35f * ApproachPercentage);
            }
            else
            {
                CompletionPercentage = (Delay + sw.ElapsedMilliseconds * PlaybackSpeed / 1000f - ApproachTime) / HoldTime;

                FillNoteBodyMask.transform.localScale = new Vector3(Size * 50, Height * CompletionPercentage * 100);

                if (CompletionPercentage > 1)
                {
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            HollowNoteBody.transform.localScale = new Vector3(0.5f, 1);
            NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(1, 1);
            InnerNoteBorder.transform.localScale = new Vector3(0.75f, 0.75f);

            FillNoteBodyMask.transform.localScale = new Vector3(Size * 50, 0);
        }
    }

    public void Highlight()
    {
        Highlighted = !Highlighted;
        HighlightBorder.SetActive(Highlighted);
        UpArrow.SetActive(Highlighted);
        DownArrow.SetActive(Highlighted);
    }
}
