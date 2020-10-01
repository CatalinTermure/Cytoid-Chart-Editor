using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public abstract class NoteController : MonoBehaviour, IHighlightable
{
    /// <summary>
    /// Time it takes from the note appearing on screen to it's start time.
    /// </summary>
    [HideInInspector]
    public float ApproachTime;
    protected float ApproachPercentage;

    public ChartObjectPool ParentPool;

    [HideInInspector]
    public float PlaybackSpeed;

    /// <summary>
    /// Stopwatch for keeping track of the animation time.
    /// </summary>
    protected Stopwatch NoteStopwatch;

    /// <summary>
    /// Time delay from the time the note should first appear to when the stopwatch starts.
    /// </summary>
    protected float Delay;

    public GameObject HighlightBorder;

    public bool Highlighted { get; set; }

    [HideInInspector]
    public int NoteType, NoteID;

    private GameObject IDText;

    public void SetDelay(float delay)
    {
        Delay = delay;
    }

    private void Awake()
    {
        Highlighted = true;
        Highlight();
    }

    void Start()
    {
        if(GlobalState.IsGameRunning)
        {
            UpdateVisuals();
        }
        else
        {
            ChangeToPausedVisuals();
        }
    }

    void Update()
    {
        if(GlobalState.IsGameRunning)
        {
            if(!NoteStopwatch.IsRunning)
            {
                NoteStopwatch.Start();
            }
            UpdateVisuals();
        }
        else
        {
            if(NoteStopwatch.IsRunning)
            {
                NoteStopwatch.Stop();
                ChangeToPausedVisuals();
            }
#if UNITY_EDITOR
            IDText.transform.position = gameObject.transform.position;
            IDText.GetComponent<UnityEngine.UI.Text>().text = NoteID.ToString();
#endif
        }
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        IDText = Instantiate(GameObject.Find("IDText"), GameObject.Find("OverlayCanvas").transform);
        IDText.transform.position = new Vector3(0, 0, 0);
    }

    private void OnDisable()
    {
        Destroy(IDText);
    }
#endif

    protected abstract void UpdateVisuals();
    protected abstract void ChangeToPausedVisuals();

    public abstract void ChangeNoteColor(Color color);

    public abstract void Highlight();

    public abstract void Initialize(Note note);
}
