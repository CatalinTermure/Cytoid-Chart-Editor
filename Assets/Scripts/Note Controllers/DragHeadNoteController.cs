using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class DragHeadNoteController : MonoBehaviour, IHighlightable, INote
{
    /// <summary>
    /// Time it takes from the note appearing on screen to it's start time.
    /// </summary>
    public float ApproachTime;

    /// <summary>
    /// The parts of the note.
    /// </summary>
    public GameObject NoteFill, NoteBorder;

    public GameObject DragConnector;

    public GameObject HighlightBorder;

    /// <summary>
    /// Stopwatch for keeping track of the animation time.
    /// </summary>
    private Stopwatch sw;

    /// <summary>
    /// Time delay from the time the note should first appear to when the stopwatch starts.
    /// </summary>
    private float Delay;

    public float StartTime;
    public int NextID;

    private bool started = false;

    struct PathPoint
    {
        public float x, y, time, connector_start_time;
    }
    private readonly List<PathPoint> Paths = new List<PathPoint>();
    private int CurrentPath = 0;

    private readonly List<GameObject> Connectors = new List<GameObject>();

    private float ApproachPercentage;

    public bool Highlighted { get; set; }
    public int NoteType { get; set; }
    public int NoteID { get; set; }

    public void SetDelay(float delay)
    {
        Delay = delay;
        if(started) UpdateComponentVisuals();
    }

    private void Awake()
    {
        Highlighted = false;
        HighlightBorder.SetActive(false);
    }

    void Start()
    {
        sw = Stopwatch.StartNew();
        NoteFill.transform.localScale = new Vector3(0.8f, 0.8f);
        NoteBorder.transform.localScale = new Vector3(0.8f, 0.8f);
        GeneratePath();
        started = true;
        UpdateComponentVisuals();
    }

    public void GeneratePath()
    {
        Paths.Add(new PathPoint
        {
            x = gameObject.transform.position.x,
            y = gameObject.transform.position.y,
            time = ApproachTime
        });
        while(NextID > 0)
        {
            Paths.Add(new PathPoint
            {
                x = (float)(GlobalState.CurrentChart.note_list[NextID].x - 0.5) * GlobalState.PlayAreaWidth,
                y = (float)(GlobalState.CurrentChart.note_list[NextID].y - 0.5) * GlobalState.PlayAreaHeight,
                time = (float)(GlobalState.CurrentChart.note_list[NextID].time - StartTime + ApproachTime),
                connector_start_time = (float)(GlobalState.CurrentChart.note_list[NextID].time - GlobalState.CurrentChart.note_list[NextID].approach_time - StartTime + ApproachTime)
            });

            float x1 = Paths[Paths.Count - 2].x, x2 = Paths[Paths.Count - 1].x, y1 = Paths[Paths.Count - 2].y, y2 = Paths[Paths.Count - 1].y;
            GameObject obj = Instantiate(DragConnector);
            obj.SetActive(!GlobalState.IsGameRunning);
            obj.transform.position = new Vector3(x2, y2);
            obj.GetComponent<SpriteRenderer>().size = new Vector2(0.25f, GlobalState.GetDistance(x1, y1, x2, y2));
            obj.transform.rotation = Quaternion.AngleAxis(-90 + (float)(Math.Atan2(y1 - y2, x1 - x2) * 180 / Math.PI), Vector3.forward);
            gameObject.transform.rotation = Quaternion.AngleAxis(-90 + (float)(Math.Atan2(y1 - y2, x1 - x2) * 180 / Math.PI), Vector3.forward);
            Connectors.Add(obj);
            NextID = GlobalState.CurrentChart.note_list[NextID].next_id;
        }
        if(Paths.Count > 1)
        {
            gameObject.transform.rotation = Quaternion.AngleAxis(90 + (float)(Math.Atan2(Paths[0].y - Paths[1].y, Paths[0].x - Paths[1].x) * 180 / Math.PI), Vector3.forward);
        }
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
                for (int i = 0; i < Connectors.Count; i++)
                {
                    if(Connectors[i] != null && Connectors[i].activeSelf)
                    {
                        Connectors[i].SetActive(false);
                    }
                }
                sw.Start();
            }
        }
        else if(sw.IsRunning)
        {
            sw.Stop();
            gameObject.transform.position = new Vector3(Paths[0].x, Paths[0].y);
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
            float time = Delay + sw.ElapsedMilliseconds / 1000f;
            ApproachPercentage = time / ApproachTime;

            for(int i = 1; i < Paths.Count; i++)
            {
                if(Paths[i].connector_start_time <= time && Connectors[i - 1] != null)
                {
                    Connectors[i - 1].SetActive(true);
                }
            }

            if (ApproachPercentage > 1)
            {
                NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(0.8f, 0.8f);
                if (CurrentPath < Paths.Count)
                {
                    float pathcompletion = (Delay + sw.ElapsedMilliseconds / 1000f - (CurrentPath > 0 ? Paths[CurrentPath - 1].time : 0)) / (Paths[CurrentPath].time - (CurrentPath > 0 ? Paths[CurrentPath - 1].time : 0));
                    while (pathcompletion > 1) // for when the note is jumped to during path movement using the timeline
                    {
                        if (CurrentPath > 0 && CurrentPath < Paths.Count)
                        {
                            Destroy(Connectors[CurrentPath - 1]);
                        }
                        if (CurrentPath + 1 < Paths.Count)
                        {
                            gameObject.transform.rotation = Quaternion.AngleAxis(90 + (float)(Math.Atan2(Paths[CurrentPath].y - Paths[CurrentPath + 1].y,
                                Paths[CurrentPath].x - Paths[CurrentPath + 1].x) * 180 / Math.PI), Vector3.forward);
                        }
                        CurrentPath++;
                        pathcompletion -= 1;
                    }
                    if (CurrentPath < Paths.Count)
                    {
                        gameObject.transform.position = new Vector3(Paths[CurrentPath - 1].x + pathcompletion * (Paths[CurrentPath].x - Paths[CurrentPath - 1].x),
                            Paths[CurrentPath - 1].y + pathcompletion * (Paths[CurrentPath].y - Paths[CurrentPath - 1].y));

                        if(CurrentPath + 1 < Paths.Count)
                        {
                            gameObject.transform.rotation = Quaternion.AngleAxis(90 + (float)(Math.Atan2(Paths[CurrentPath].y - Paths[CurrentPath + 1].y,
                                Paths[CurrentPath].x - Paths[CurrentPath + 1].x) * 180 / Math.PI), Vector3.forward);
                        }

                        if (CurrentPath > 0)
                        {
                            Connectors[CurrentPath - 1].GetComponent<SpriteRenderer>().size = new Vector2(0.25f, (1.0f - pathcompletion) *
                                GlobalState.GetDistance(Paths[CurrentPath - 1].x, Paths[CurrentPath - 1].y, Paths[CurrentPath].x, Paths[CurrentPath].y));
                        }
                    }
                }

                if (CurrentPath >= Paths.Count) // no, this CANNOT be an else
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                NoteFill.transform.localScale = NoteBorder.transform.localScale = new Vector3(0.4f + ApproachPercentage * 0.4f, 0.4f + ApproachPercentage * 0.4f);
            }
        }
    }

    public void Highlight()
    {
        Highlighted = !Highlighted;
        HighlightBorder.SetActive(Highlighted);
    }
}
