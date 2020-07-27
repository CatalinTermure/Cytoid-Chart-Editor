using System.Diagnostics;
using UnityEngine;

public class ClickNoteController : MonoBehaviour, IHighlightable, INote
{
    /// <summary>
    /// Time it takes from the note appearing on screen to it's start time.
    /// </summary>
    public float ApproachTime;

    /// <summary>
    /// The parts of the click note.
    /// </summary>
    public GameObject NoteFill, NoteBorder, HighlightBorder;

    /// <summary>
    /// Stopwatch for keeping track of the animation time.
    /// </summary>
    private Stopwatch sw;

    /// <summary>
    /// Time delay from the time the note should first appear to when the stopwatch starts.
    /// </summary>
    private float Delay;

    private float ApproachPercentage;

    private bool started = false;

    public bool Highlighted { get; set; }
    public int NoteType { get; set; }
    public int NoteID { get; set; }

    public void SetDelay(float delay)
    {
        Delay = delay;
        if (started) UpdateComponentVisuals();
    }

    private void Awake()
    {
        Highlighted = false;
        HighlightBorder.SetActive(false);
    }

    void Start()
    {
        sw = Stopwatch.StartNew();
        NoteFill.transform.localScale = new Vector3(0, 0);
        NoteBorder.transform.localScale = new Vector3(0.6f, 0.6f);
        UpdateComponentVisuals();
        started = true;
    }

    public void ChangeNoteColor(Color color)
    {
        NoteFill.GetComponent<SpriteRenderer>().color = color;
        Color tmp = NoteBorder.GetComponent<SpriteRenderer>().color;
        NoteBorder.GetComponent<SpriteRenderer>().color = new Color(tmp.r, tmp.g, tmp.b, color.a);
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

    void Update()
    {
        UpdateComponentVisuals();
    }

    private void UpdateComponentVisuals()
    {
        if (GlobalState.IsGameRunning)
        {
            ApproachPercentage = (Delay + sw.ElapsedMilliseconds / 1000f) / ApproachTime;

            NoteFill.transform.localScale = new Vector3(ApproachPercentage, ApproachPercentage);
            NoteBorder.transform.localScale = new Vector3(0.6f + ApproachPercentage * 0.4f, 0.6f + ApproachPercentage * 0.4f);

            if (ApproachPercentage > 1)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            NoteFill.transform.localScale = new Vector3(1, 1);
            NoteBorder.transform.localScale = new Vector3(1, 1);
        }
    }

    public void Highlight()
    {
        Highlighted = !Highlighted;
        HighlightBorder.SetActive(Highlighted);
    }
}
