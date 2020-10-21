using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UI;

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

    private GameObject InfoText;

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
                Destroy(InfoText);
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
            InfoText.transform.position = gameObject.transform.position;
            UpdateInfoText();
        }
    }

    private void OnEnable()
    {
        InfoText = Instantiate(GameObject.Find("IDText"), GameObject.Find("OverlayCanvas").transform);
        InfoText.transform.position = gameObject.transform.position;
    }

    private void OnDisable()
    {
        Destroy(InfoText);
    }

    public void UpdateInfoText()
    {
        switch(GlobalState.ShownNoteInfo)
        {
            case GlobalState.NoteInfo.NoteID:
                InfoText.GetComponent<Text>().text = NoteID.ToString();
                break;
            case GlobalState.NoteInfo.NoteX:
                InfoText.GetComponent<Text>().text = GlobalState.CurrentChart.note_list[NoteID].x.ToString("F2");
                break;
            case GlobalState.NoteInfo.NoteY:
                InfoText.GetComponent<Text>().text = GlobalState.CurrentChart.note_list[NoteID].y.ToString("F2");
                break;
        }
    }

    protected abstract void UpdateVisuals();
    protected abstract void ChangeToPausedVisuals();

    public abstract void ChangeNoteColor(Color color);

    public abstract void Highlight();

    public abstract void Initialize(Note note);
}
