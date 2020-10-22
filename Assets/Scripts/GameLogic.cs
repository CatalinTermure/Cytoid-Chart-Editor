using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.Audio;
using System.Text;

using static GlobalState;

public class GameLogic : MonoBehaviour
{
    /// <summary>
    /// The currently selected note adder/tool.
    /// </summary>
    public static NoteType CurrentTool = NoteType.NONE;

    public static bool BlockInput = false;

    private GameObject PlayPauseButton;

    public GameObject ScanlineNote;
    public GameObject DivisorLine;

    public NotePropertiesManager NotePropsManager;

    private ChartObjectPool ObjectPool;

    public Text PageText, TimeText;

    public GameObject Scanline;

    public Slider Timeline;

    public AudioSource MusicSource;

    private GameObject SelectionBox;

    public AudioSource[] HitsoundSources;

    private readonly List<double> HitsoundTimings = new List<double>();

    private struct NoteSpawnTime
    {
        public double time;
        public int id;
    }
    private readonly List<NoteSpawnTime> NoteSpawns = new List<NoteSpawnTime>();

    public static Camera MainCamera;

    public Slider BeatDivisor;

    public AudioMixerGroup HalfSpeedMixer, ThreeQuarterSpeedMixer;

    public static int CurrentPageIndexOverride = -1;

    private int CurrentHitsoundIndex = 0;
    private int CurrentTempoIndex = 0;
    private int CurrentNoteIndex = 0;

    [HideInInspector]
    public int CurrentPageIndex = 0;
    private Page CurrentPage
    {
        get => CurrentChart.page_list[CurrentPageIndex];
    }

    private double ScheduledTime { get; set; }
    private bool IsStartScheduled = false;

    private static int PlaybackSpeedIndex = 2;
    private readonly float[] PlaybackSpeeds = new float[3] { 0.5f, 0.75f, 1.0f };

    private readonly StringBuilder TimeTextBuilder = new StringBuilder(32);

    private static string LogPath;

    private bool LockX = false;
    private float LockedX = 0;

    /// <summary>
    /// The value of the beat divisor slider.
    /// </summary>
    public static int DivisorValue = 8;
    private readonly int[] AllowedDivisor = { 1, 2, 3, 4, 6, 8, 12, 16 };

    public void Awake()
    {
        SelectionBox = GameObject.Find("SelectionBox");
        SelectionBox.SetActive(false);
        PlayPauseButton = GameObject.Find("PlayPauseButton");
        MainCamera = Camera.main;
        if (CurrentChart != null)
        {
            if (Config.DebugMode)
            {
                LogPath = Path.Combine(Application.persistentDataPath, "GameLogicLog.txt");
                Logging.CreateLog(LogPath, "Starting loading the chart...\n");
            }

            CalculateTimings();

            CalculateDragIDs();

            Logging.AddToLog(LogPath, "Calculated timings...\n");

            MusicManager.SetSource(MusicSource);
            for (int i = 0; i < HitsoundSources.Length; i++)
            {
                HitsoundSources[i].volume = Config.HitsoundVolume;
            }

            if (File.Exists(Path.Combine(Application.persistentDataPath, "Hitsound.wav")))
            {
                Logging.AddToLog(LogPath, "Trying to load custom hitsound\n");
                LoadCustomHitsounds(Path.Combine(Application.persistentDataPath, "Hitsound.wav"));
                for (int i = 0; i < HitsoundSources.Length; i++)
                {
                    HitsoundSources[i].clip = Hitsound;
                }
                Logging.AddToLog(LogPath, "Custom hitsound loaded\n");
            }

            Logging.AddToLog(LogPath, "Audio sources loaded\n");

            ObjectPool = new ChartObjectPool();

            Logging.AddToLog(LogPath, "Pool created\n");

            if (CurrentPageIndexOverride != -1)
            {
                CurrentPageIndex = CurrentPageIndexOverride;
                CurrentPageIndexOverride = -1;
            }
            else
            {
                CurrentPageIndex = 0;
            }

            UpdateTime(CurrentPage.actual_start_time);

            Logging.AddToLog(LogPath, $"Moved to page {CurrentPageIndex}\n");

            UpdatePlaybackSpeed();

            GameObject.Find("NoteCountText").GetComponent<Text>().text = $"Note count: {CurrentChart.note_list.Count}";
        }
    }

    private void Start()
    {
        // Adjust for different aspect ratios
        GameObject.Find("PlayAreaBorder").GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaWidth, PlayAreaHeight);
        Scanline.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaWidth, 0.1f);
        GameObject LevelOptionsButton = GameObject.Find("LevelOptionsButton"), EditorSettingsButton = GameObject.Find("EditorSettingsButton"), SaveButton = GameObject.Find("SaveButton");
        GameObject.Find("ChartSelectButton").GetComponent<RectTransform>().sizeDelta = new Vector3(200 + 1.5f * ((200 * AspectRatio / NormalAspectRatio) - 200), 70);
        LevelOptionsButton.GetComponent<RectTransform>().sizeDelta = new Vector3(200 + 1.5f * ((200 * AspectRatio / NormalAspectRatio) - 200), 70);
        EditorSettingsButton.GetComponent<RectTransform>().sizeDelta = new Vector3(200 + 1.5f * ((200 * AspectRatio / NormalAspectRatio) - 200), 70);
        SaveButton.GetComponent<RectTransform>().sizeDelta = new Vector3(200 + 1.5f * ((200 * AspectRatio / NormalAspectRatio) - 200), 70);
        LevelOptionsButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(LevelOptionsButton.GetComponent<RectTransform>().anchoredPosition.x * AspectRatio / NormalAspectRatio, LevelOptionsButton.GetComponent<RectTransform>().anchoredPosition.y);
        EditorSettingsButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(EditorSettingsButton.GetComponent<RectTransform>().anchoredPosition.x * AspectRatio / NormalAspectRatio, EditorSettingsButton.GetComponent<RectTransform>().anchoredPosition.y);
        SaveButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(SaveButton.GetComponent<RectTransform>().anchoredPosition.x * AspectRatio / NormalAspectRatio, SaveButton.GetComponent<RectTransform>().anchoredPosition.y);
        
        if(Config.NotchOverlapFix)
        {
            if(Screen.orientation == ScreenOrientation.LandscapeLeft)
            {
                GameObject.Find("AddClickNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.safeArea.x - 15, GameObject.Find("AddClickNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                GameObject.Find("AddHoldNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.safeArea.x - 15, GameObject.Find("AddHoldNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                GameObject.Find("AddLongHoldNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.safeArea.x - 15, GameObject.Find("AddLongHoldNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                GameObject.Find("AddDragNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.safeArea.x - 15, GameObject.Find("AddDragNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                GameObject.Find("AddCDragNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.safeArea.x - 15, GameObject.Find("AddCDragNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
            }
            else if(Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                GameObject.Find("AddFlickNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(-Screen.safeArea.x + 15, GameObject.Find("AddFlickNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                GameObject.Find("MoveNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(-Screen.safeArea.x + 15, GameObject.Find("MoveNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                GameObject.Find("BPMButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(-Screen.safeArea.x + 15, GameObject.Find("BPMButton").GetComponent<RectTransform>().anchoredPosition.y);
                GameObject.Find("OtherOptionsScrollView").GetComponent<RectTransform>().anchoredPosition = new Vector2(-Screen.safeArea.x + 15, GameObject.Find("OtherOptionsScrollView").GetComponent<RectTransform>().anchoredPosition.y);
            }
        }

        if (CurrentChart != null)
        {
            Logging.AddToLog(LogPath, "Adjusted to resolution...\n");
        }

        // Keep current page index after navigating to editor/level/chart options
        LevelOptionsButton.GetComponent<Button>().onClick.AddListener(() => CurrentPageIndexOverride = CurrentPageIndex);
        EditorSettingsButton.GetComponent<Button>().onClick.AddListener(() => CurrentPageIndexOverride = CurrentPageIndex);

        // Create vertical divisor lines
        if(Config.VerticalLineAccent)
        {
            int mid = (Config.VerticalDivisors - Config.VerticalDivisors % 2) / 2;

            GameObject obj;

            for (int i = 1; i < mid; i++)
            {
                obj = Instantiate(DivisorLine);
                obj.transform.position = new Vector3(PlayAreaWidth / Config.VerticalDivisors * i - PlayAreaWidth / 2, 0);
                obj.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
                obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaHeight, 0.05f);
                obj.tag = "GridLine";
            }

            for (int i = mid + 1 + (Config.VerticalDivisors % 2); i < Config.VerticalDivisors; i++)
            {
                obj = Instantiate(DivisorLine);
                obj.transform.position = new Vector3(PlayAreaWidth / Config.VerticalDivisors * i - PlayAreaWidth / 2, 0);
                obj.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
                obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaHeight, 0.05f);
                obj.tag = "GridLine";
            }


            if (Config.VerticalDivisors % 2 == 1)
            {
                obj = Instantiate(DivisorLine);
                obj.transform.position = new Vector3(PlayAreaWidth / Config.VerticalDivisors * (mid + 1) - PlayAreaWidth / 2, 0);
                obj.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
                obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaHeight, 0.15f);
                obj.tag = "GridLine";
                obj.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.59f);
            }

            obj = Instantiate(DivisorLine);
            obj.transform.position = new Vector3(PlayAreaWidth / Config.VerticalDivisors * mid - PlayAreaWidth / 2, 0);
            obj.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
            obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaHeight, 0.15f);
            obj.tag = "GridLine";
            obj.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.59f);
        }
        else
        {
            for (int i = 0; i <= Config.VerticalDivisors; i++)
            {
                GameObject obj = Instantiate(DivisorLine);
                obj.transform.position = new Vector3(PlayAreaWidth / Config.VerticalDivisors * i - PlayAreaWidth / 2, 0);
                obj.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
                obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaHeight, 0.1f);
                obj.tag = "GridLine";
            }
        }

        // Create horizontal divisor lines
        BeatDivisor.value = DivisorValue;

        RenderDivisorLines();

        UpdateOffsetText();

        GameObject.Find("BeatDivisorInputField").GetComponent<InputField>().onEndEdit.AddListener((string s) => SetBeatDivisorValueUnsafe(int.Parse(s)));

        if(CurrentChart != null)
        {
            Logging.AddToLog(LogPath, "PlayArea loaded...\n");
        }
    }

    /// <summary>
    /// Calculates the start_time and end_time of pages, the time of tempos and the time, y, approach_time and hold_time of notes. 
    /// </summary>
    private void CalculateTimings()
    {
        int timebase = CurrentChart.time_base;
        List<Note> notes = CurrentChart.note_list;
        List<Tempo> tempos = CurrentChart.tempo_list;
        List<Page> pages = CurrentChart.page_list;

        HitsoundTimings.Clear();
        NoteSpawns.Clear();

        int ni = 0, pi = 0, ti = 0, n = notes.Count, p = pages.Count, t = tempos.Count;
        double temposum = 0; // Time from tick 0 to the start of tempo ti

        // Calculate page times and tempo times
        while(ti < t && pi < p)
        {
            if(ti + 1 < t && pages[pi].start_tick >= tempos[ti + 1].tick) // If page is not on this tempo, go to next tempo
            {
                tempos[ti].time = temposum;
                temposum += (double)tempos[ti].value * (tempos[ti + 1].tick - tempos[ti].tick) / timebase / 1000000;
                ti++;
            }
            else
            {
                // If page starts at tempo ti and ends at tempo tj
                if(ti + 1 < t && pages[pi].end_tick > tempos[ti + 1].tick)
                {
                    int tj = ti + 1;
                    double auxtemposum = 0; // Time from start of tempo (t1 + 1) to start of tempo t2

                    while(tj + 1 < t && pages[pi].end_tick >= tempos[tj + 1].tick)
                    {
                        auxtemposum += (double)tempos[tj].value * (tempos[tj + 1].tick - tempos[tj].tick) / timebase / 1000000;
                        tj++;
                    }

                    pages[pi].start_time = temposum + (double)tempos[ti].value * (pages[pi].start_tick - tempos[ti].tick) / timebase / 1000000;
                    pages[pi].end_time = pages[pi].start_time + (double)tempos[ti].value * (tempos[ti + 1].tick - pages[pi].start_tick) / timebase / 1000000
                        + auxtemposum + (double)tempos[tj].value * (pages[pi].end_tick - tempos[tj].tick) / timebase / 1000000;
                }
                // If page starts and ends at tempo t1
                else
                {
                    pages[pi].start_time = temposum + (double)tempos[ti].value * (pages[pi].start_tick - tempos[ti].tick) / timebase / 1000000;
                    pages[pi].end_time = temposum + (double)tempos[ti].value * (pages[pi].end_tick - tempos[ti].tick) / timebase / 1000000;
                }


                if (pi != 0)
                {
                    pages[pi].actual_start_tick = pages[pi - 1].end_tick;
                    pages[pi].actual_start_time = pages[pi - 1].end_time;
                }
                else
                {
                    pages[pi].actual_start_tick = 0;
                    pages[pi].actual_start_time = 0;
                }

                pi++;
            }
        }
        while(ti + 1 < t)
        {
            tempos[ti].time = temposum;
            temposum += (double)tempos[ti].value * (tempos[ti + 1].tick - tempos[ti].tick) / timebase / 1000000;
            ti++;
        }
        if(ti < t)
        {
            tempos[ti].time = temposum;
        }
        double realmaxtime = (double)MusicManager.MaxTime / PlaybackSpeeds[PlaybackSpeedIndex] + CurrentChart.music_offset;
        if(pages[p - 1].end_time < realmaxtime) // Add pages in case the page_list ends before the music
        {
            double lasttempotime = CurrentChart.tempo_list.Last().value / 1000000.0; // Calculate time of the pages in respect to the last tempo
            while(pages[p - 1].end_time < realmaxtime)
            {
                pages.Add(new Page()
                {
                    start_tick = pages[p - 1].end_tick,
                    end_tick = pages[p - 1].end_tick + timebase,
                    start_time = pages[p - 1].end_time,
                    end_time = pages[p - 1].end_time + lasttempotime,
                    scan_line_direction = -pages[p - 1].scan_line_direction
                });
                p++;
            }
            CalculateTimings();
            return;
        }
        if(pages[p - 1].actual_start_time > realmaxtime && n > 0 && notes[n - 1].page_index + 1 < p)
        {
            while (pages[p - 1].actual_start_time > realmaxtime && notes[n - 1].page_index + 1 < p)
            {
                pages.RemoveAt(p - 1);
                p--;
            }
            CalculateTimings();
            return;
        }

        tempos[ti].time = temposum;

        temposum = 0;
        ti = 0;

        Page prevPage, currPage;
        double page_ratio;

        // Calculate note time, hold time, AR and others
        while(ti < t && ni < n)
        {
            if(ti + 1 < t && notes[ni].tick >= tempos[ti + 1].tick)
            {
                temposum += (double)tempos[ti].value * (tempos[ti + 1].tick - tempos[ti].tick) / timebase / 1000000;
                ti++;
            }
            else
            {
                notes[ni].actual_opacity = notes[ni].opacity < 0 ? CurrentChart.opacity : notes[ni].opacity;
                notes[ni].actual_size = notes[ni].size < 0 ? CurrentChart.size : notes[ni].size;

                notes[ni].time = temposum + (double)tempos[ti].value * (notes[ni].tick - tempos[ti].tick) / timebase / 1000000;
                HitsoundTimings.Add(notes[ni].time);

                notes[ni].y = pages[notes[ni].page_index].scan_line_direction == 1 ?
                    (double)(notes[ni].tick - pages[notes[ni].page_index].actual_start_tick) / (pages[notes[ni].page_index].end_tick - pages[notes[ni].page_index].actual_start_tick) :
                    1.0 - (double)(notes[ni].tick - pages[notes[ni].page_index].actual_start_tick) / (pages[notes[ni].page_index].end_tick - pages[notes[ni].page_index].actual_start_tick);

                if(notes[ni].page_index == 0)
                {
                    notes[ni].approach_time = 1.367 / notes[ni].approach_rate;
                }
                else
                {
                    currPage = pages[notes[ni].page_index];
                    prevPage = pages[notes[ni].page_index - 1];
                    page_ratio = (double)(notes[ni].tick - currPage.actual_start_tick) / (currPage.end_tick - currPage.actual_start_tick);

                    notes[ni].approach_time = 1.367 / (notes[ni].approach_rate * Math.Max(1.0, 1.367 / ((currPage.end_time - currPage.actual_start_time) * page_ratio + (prevPage.end_time - prevPage.start_time) * (1.367 - page_ratio))));
                }

                if(notes[ni].type == 1 || notes[ni].type == 2)
                {
                    int holdendtick = notes[ni].tick + notes[ni].hold_tick;
                    // Calculate hold time in the same way as page end time
                    if (ti + 1 < t && holdendtick > tempos[ti + 1].tick)
                    {
                        int tj = ti + 1;
                        double auxtemposum = 0;
                        while (tj + 1 < t && holdendtick >= tempos[tj + 1].tick)
                        {
                            auxtemposum += (double)tempos[tj].value * (tempos[tj + 1].tick - tempos[tj].tick) / timebase / 1000000;
                            tj++;
                        }

                        notes[ni].hold_time = auxtemposum + (double)tempos[ti].value * (tempos[ti + 1].tick - notes[ni].tick) / timebase / 1000000
                            + (double)tempos[tj].value * (holdendtick - tempos[tj].tick) / timebase / 1000000;
                    }
                    else
                    {
                        notes[ni].hold_time = (double)tempos[ti].value * notes[ni].hold_tick / timebase / 1000000;
                    }

                    if(Config.PlayHitsoundsOnHoldEnd)
                    {
                        HitsoundTimings.Add(notes[ni].time + notes[ni].hold_time);
                    }
                }
                else
                {
                    notes[ni].hold_time = 0;
                }

                NoteSpawns.Add(new NoteSpawnTime { time = notes[ni].time - notes[ni].approach_time, id = notes[ni].id });

                ni++;
            }
        }
        NoteSpawns.Sort((NoteSpawnTime a, NoteSpawnTime b) =>
        {
            if(Math.Abs(a.time - b.time) < 0.0001)
            {
                return CurrentChart.note_list[a.id].page_index.CompareTo(CurrentChart.note_list[b.id].page_index);
            }
            return a.time.CompareTo(b.time);
        }); // keeping it like this because inserting *could* be slower and notecount is quite low
        HitsoundTimings.Sort(); // keeping it like this because NlogN is comparable(or higher than) to N*holdcount for inserting
    }

    private void CalculateDragIDs()
    {
        int dragid = -1;
        for(int i = 0; i < CurrentChart.note_list.Count; i++)
        {
            if(CurrentChart.note_list[i].type == (int)NoteType.CDRAG_HEAD || CurrentChart.note_list[i].type == (int)NoteType.DRAG_HEAD)
            {
                dragid++;
                int id = CurrentChart.note_list[i].id;
                while(CurrentChart.note_list[id].next_id >= 0)
                {
                    CurrentChart.note_list[id].drag_id = dragid;
                    id = CurrentChart.note_list[id].next_id;
                }
                CurrentChart.note_list[id].drag_id = dragid;
            }
        }
    }

    public static int GetDragParent(int id)
    {
        int i = id - 1;
        while(i >= 0)
        {
            if(CurrentChart.note_list[i].next_id == id)
            {
                return i;
            }
            i--;
        }
        return -1;
    }

    /// <summary>
    /// Adds the note passed as parameter to the <see cref="CurrentChart"/>'s note_list so that the list remains sorted by tick.
    /// </summary>
    /// <param name="note"> The note to be added, its id will be modified. </param>
    /// <returns> Returns the position it was added to. </returns>
    private int AddNote(Note note)
    {
        int poz = 0; // Determine the position to be inserted in
        // Currently using sequential search and not binary because we already have necessary O(N) complexity following so it does not make much of a difference, to be changed if performance is hit because of this.
        while(poz < CurrentChart.note_list.Count && CurrentChart.note_list[poz].tick < note.tick)
        {
            poz++;
        }
        note.id = poz;
        for(int i = 0; i < CurrentChart.note_list.Count; i++)
        {
            if(CurrentChart.note_list[i].type == (int)NoteType.DRAG_HEAD || CurrentChart.note_list[i].type == (int)NoteType.DRAG_CHILD || CurrentChart.note_list[i].type == (int)NoteType.CDRAG_CHILD || CurrentChart.note_list[i].type == (int)NoteType.CDRAG_HEAD)
            {
                if (CurrentChart.note_list[i].next_id >= poz)
                {
                    CurrentChart.note_list[i].next_id++;
                }
            }
            else
            {
                CurrentChart.note_list[i].next_id = 0;
            }
        }
        if(CurrentChart.note_list.Count == 0)
        {
            CurrentChart.note_list.Add(note);
            return 0;
        }
        // Use a classic insertion algorithm while modifying ids accordingly
        CurrentChart.note_list.Add(new Note());
        CurrentChart.note_list.Last().id = CurrentChart.note_list.Count - 1;
        for(int i = CurrentChart.note_list.Count - 1; i > poz; i--)
        {
            CurrentChart.note_list[i] = CurrentChart.note_list[i - 1];
            CurrentChart.note_list[i].id = i;
        }
        CurrentChart.note_list[poz] = note;
        GameObject.Find("NoteCountText").GetComponent<Text>().text = $"Note count: {CurrentChart.note_list.Count}";
        return poz;
    }

    /// <summary>
    /// Removes the note with the specified id.
    /// </summary>
    /// <param name="noteID"> The id of the note to be removed. </param>
    private void RemoveNote(int noteID)
    {
        for(int i = 0; i < NoteSpawns.Count; i++)
        {
            if(NoteSpawns[i].id == noteID)
            {
                NoteSpawns.RemoveAt(i);
                for(int j = i; j < NoteSpawns.Count; j++)
                {
                    NoteSpawns[j] = new NoteSpawnTime { id = NoteSpawns[j].id - 1, time = NoteSpawns[j].time };
                }
                break;
            }
        }
        if (CurrentChart.note_list[noteID].type == (int)NoteType.CDRAG_HEAD || CurrentChart.note_list[noteID].type == (int)NoteType.DRAG_HEAD)
            // If the deleted note is a (c)drag head, then make the next note the head instead
        {
            int nxt = CurrentChart.note_list[noteID].next_id;
            if(nxt > 0)
            {
                CurrentChart.note_list[nxt].type = CurrentChart.note_list[noteID].type;
            }
        }
        for(int i = 0; i < CurrentChart.note_list.Count; i++)
        {
            if(CurrentChart.note_list[i].next_id > noteID)
            {
                CurrentChart.note_list[i].next_id--;
            }
            else if(CurrentChart.note_list[i].next_id == noteID) // If the deleted note is part of a (c)drag chain, then remove it from the chain while keeping the chain valid
            {
                CurrentChart.note_list[i].next_id = CurrentChart.note_list[noteID].next_id - (CurrentChart.note_list[noteID].next_id > noteID ? 1 : 0);
            }
        }
        for(int i = 0; i < HitsoundTimings.Count; i++)
        {
            if(Math.Abs(CurrentChart.note_list[noteID].time - HitsoundTimings[i]) < 0.001)
            {
                HitsoundTimings.RemoveAt(i);
                break;
            }
        }
        if(Config.PlayHitsoundsOnHoldEnd && (CurrentChart.note_list[noteID].type == (int)NoteType.HOLD || CurrentChart.note_list[noteID].type == (int)NoteType.LONG_HOLD))
        {
            for (int i = 0; i < HitsoundTimings.Count; i++)
            {
                if (Math.Abs(CurrentChart.note_list[noteID].time + CurrentChart.note_list[noteID].hold_time - HitsoundTimings[i]) < 0.001)
                {
                    HitsoundTimings.RemoveAt(i);
                    break;
                }
            }
        }
        // Use a classic deletion algorithm while modifying ids accordingly
        for(int i = noteID; i + 1 < CurrentChart.note_list.Count; i++)
        {
            CurrentChart.note_list[i] = CurrentChart.note_list[i + 1];
            CurrentChart.note_list[i].id = i;
        }
        CurrentChart.note_list.RemoveAt(CurrentChart.note_list.Count - 1);
        GameObject.Find("NoteCountText").GetComponent<Text>().text = $"Note count: {CurrentChart.note_list.Count}";
    }

    /// <summary>
    /// Spawn a note in world space.
    /// </summary>
    /// <param name="note"> The note data that contains the necessary note properties. </param>
    /// <param name="delay"> The delay, in seconds, from when the note is spawned to when it should have been spawned. </param>
    /// <param name="loweropacity"> If the note should appear with lower(1/3rd) opacity, reserved for notes on the previous page. </param>
    private void SpawnNote(Note note, double delay, bool loweropacity = false)
    {
        GameObject obj = ObjectPool.GetNote((NoteType)note.type);

        NoteController notecontroller = obj.GetComponent<NoteController>();

        notecontroller.ParentPool = ObjectPool;
        notecontroller.Initialize(note);

        obj.SetActive(true);

        int indx = ColorIndexes[note.type];

        ColorUtility.TryParseHtmlString(note.fill_color ??
            CurrentChart.fill_colors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? indx : indx + 1] ??
            DefaultFillColors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? indx : indx + 1], out Color notecolor);
        notecolor.a = (float)note.actual_opacity / (loweropacity ? 3 : 1);
        notecontroller.ChangeNoteColor(notecolor);

        notecontroller.SetDelay((float)delay);

        notecontroller.PlaybackSpeed = PlaybackSpeeds[PlaybackSpeedIndex];

        if(notecontroller.NoteType == (int)NoteType.LONG_HOLD)
        {
            if(note.tick + note.hold_tick >= CurrentPage.start_tick)
            {
                obj.GetComponent<LongHoldNoteController>().FinishIndicator.transform.position = new Vector3(obj.transform.position.x, CurrentPage.scan_line_direction *
                (PlayAreaHeight * (note.tick + note.hold_tick - CurrentPage.actual_start_tick) / (int)CurrentPage.ActualPageSize - PlayAreaHeight / 2));
            }
            else
            {
                obj.GetComponent<LongHoldNoteController>().FinishIndicator.transform.position = new Vector3(obj.transform.position.x, 10000);
            }
        }
    }

    /// <summary>
    /// Spawn a scanline/tempo note from the <see cref="Tempo"/> with the specified id.
    /// </summary>
    /// <param name="id"> The id of the <see cref="Tempo"/> this note represents. </param>
    private void SpawnScanlineNote(int id)
    {
        GameObject obj = Instantiate(ScanlineNote);

        obj.GetComponent<ScanlineNoteController>().TempoID = id;

        obj.GetComponent<ScanlineNoteController>().SetPosition(new Vector3(-PlayAreaWidth / 2 - 1, -PlayAreaHeight / 2 + PlayAreaHeight * (float)(CurrentPage.scan_line_direction == 1 ?
            (CurrentChart.tempo_list[id].tick - CurrentPage.actual_start_tick) / CurrentPage.ActualPageSize :
            1.0 - (CurrentChart.tempo_list[id].tick - CurrentPage.actual_start_tick) / CurrentPage.ActualPageSize)));

        obj.GetComponent<ScanlineNoteController>().TimeInputField.text = (CurrentChart.tempo_list[id].time - CurrentChart.music_offset).ToString();
        
        obj.GetComponent<ScanlineNoteController>().BPMInputField.text = Math.Round(120000000.0 / CurrentChart.tempo_list[id].value, 2).ToString();
    }


    /// <summary>
    /// Adds a tempo to the <see cref="CurrentChart"/>'s tempo_list while keeping the list sorted by tick.
    /// </summary>
    /// <param name="tempo"> The tempo to be added. </param>
    private void AddTempo(Tempo tempo)
    {
        int poz = 0;
        while(poz < CurrentChart.tempo_list.Count && tempo.tick > CurrentChart.tempo_list[poz].tick)
        {
            poz++;
        }
        if(poz < CurrentChart.tempo_list.Count && CurrentChart.tempo_list[poz].tick == tempo.tick)
        {
            poz++;
            tempo.tick++;
        }
        tempo.value = CurrentChart.tempo_list[poz > 0 ? poz - 1 : 0].value;
        CurrentChart.tempo_list.Insert(poz, tempo);
    }

    /// <summary>
    /// Removes the tempo with the specified id.
    /// </summary>
    /// <param name="id"> The id of the tempo to be removed. </param>
    private void RemoveTempo(int id)
    {
        CurrentChart.tempo_list.RemoveAt(id);
        CalculateTimings();
    }

    /// <summary>
    /// Schedules the start or pauses the current chart.
    /// </summary>
    public void PlayPause()
    {
        if (CurrentChart == null || IsStartScheduled)
        {
            return;
        }
        if (IsGameRunning)
        {
            MusicManager.Pause();
            IsGameRunning = false;
            IsStartScheduled = false;

            if (CurrentPageIndex < CurrentChart.page_list.Count)
            {
                UpdateTime(CurrentPage.actual_start_time);
            }
            else
            {
                CurrentPageIndex = CurrentChart.page_list.Count - 1;
                UpdateTime(CurrentPage.actual_start_time);
            }
        }
        else
        {
            PlayPauseButton.SetActive(false);
            ScheduledTime = MusicManager.Play();
            IsStartScheduled = true;
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("ScanlineNote"))
            {
                Destroy(obj);
            }
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
            {
                if (obj.GetComponent<IHighlightable>().Highlighted)
                {
                    HighlightObject(obj);
                }
            }

            AddMissingDrags();
        }
    }

    private void AddMissingDrags()
    {
        for(int i = 0; i < CurrentChart.note_list.Count; i++)
        {
            Note note = CurrentChart.note_list[i];
            if((note.type == (int)NoteType.CDRAG_HEAD || note.type == (int)NoteType.DRAG_HEAD) && note.page_index < CurrentPageIndex)
            {
                int id = note.id;
                while(CurrentChart.note_list[id].next_id > 0)
                {
                    if(CurrentChart.note_list[id].page_index == CurrentPageIndex)
                    {
                        SpawnNote(note, CurrentPage.actual_start_time - note.time + note.approach_time);
                        break;
                    }
                    id = CurrentChart.note_list[id].next_id;
                }
            }
        }
    }

    public void BeatDivisorValueChanged()
    {
        for(int i = 0; i < 8; i++)
        {
            if(AllowedDivisor[i] <= (int)BeatDivisor.value)
            {
                DivisorValue = AllowedDivisor[i];
            }
        }
        RenderDivisorLines();
    }

    public void SetBeatDivisorValueUnsafe(int val)
    {
        val = Clamp(val, 1, 32);
        BeatDivisor.SetValueWithoutNotify(val);
        DivisorValue = val;
        RenderDivisorLines();
    }

    private void RenderDivisorLines()
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("DivisorLine"))
        {
            Destroy(obj);
        }
        int interval = 1000;
        if(DivisorValue % 3 == 0)
        {
            interval = 3;
        }
        else if(DivisorValue % 4 == 0)
        {
            interval = 4;
        }
        for (int i = 1; i < DivisorValue; i++)
        {
            GameObject obj = Instantiate(DivisorLine);
            obj.transform.position = new Vector3(0, PlayAreaHeight / DivisorValue * i - PlayAreaHeight / 2);
            obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaWidth, (i % interval == 0 && Config.HorizontalLineAccents) ? 0.175f : 0.1f);
        }

        GameObject.Find("BeatDivisorInputField").GetComponent<InputField>().text = DivisorValue.ToString();
    }

    public void TimelineValueChange()
    {
        if(CurrentChart == null)
        {
            Timeline.SetValueWithoutNotify(0);
            return;
        }
        if (!IsGameRunning)
        {
            double time = Timeline.value * MusicManager.MaxTime * 1 / PlaybackSpeeds[PlaybackSpeedIndex];

            CurrentPageIndex = SnapTimeToPage(time);

            time = CurrentPage.actual_start_time;

            UpdateTime(time);
        }
    }

    public static void RefreshNote(int noteID)
    {
        bool needupdate = false;
        foreach (var obj in GameObject.FindGameObjectsWithTag("Note"))
        {
            if(obj.GetComponent<NoteController>().NoteID == noteID)
            {
                int type = obj.GetComponent<NoteController>().NoteType;
                if(type == (int)NoteType.DRAG_HEAD || type == (int)NoteType.DRAG_CHILD || type == (int)NoteType.CDRAG_HEAD || type == (int)NoteType.CDRAG_CHILD)
                {
                    needupdate = true;
                }
                obj.GetComponent<NoteController>().Initialize(CurrentChart.note_list[noteID]);
            }
        }
        if(needupdate)
        {
            GameLogic g = GameObject.Find("UICanvas").GetComponent<GameLogic>();
            g.UpdateTime(g.CurrentPage.start_time);
        }
    }

    /// <summary>
    /// Updates the current time to the <paramref name="time"/> specified.
    /// </summary>
    /// <param name="time"> The specified time the chart should be at. </param>
    private void UpdateTime(double time)
    {
        NotePropsManager.Clear();
        MakeButtonsInteractable();

        foreach (var obj in GameObject.FindGameObjectsWithTag("Note"))
        {
            ObjectPool.ReturnToPool(obj, obj.GetComponent<NoteController>().NoteType);
        }

        foreach (var obj in GameObject.FindGameObjectsWithTag("Connector"))
        {
            ObjectPool.ReturnToPool(obj, 8);
        }

        foreach (var obj in GameObject.FindGameObjectsWithTag("ScanlineNote"))
        {
            Destroy(obj);
        }

        CurrentHitsoundIndex = 0;

        CurrentNoteIndex = 0;

        for(int i = 0; i < NoteSpawns.Count; i++)
        {
            if(NoteSpawns[i].time <= time)
            {
                if(CurrentChart.note_list[NoteSpawns[i].id].page_index == CurrentPageIndex ||
                    (Config.ShowApproachingNotesWhilePaused && CurrentChart.note_list[NoteSpawns[i].id].page_index > CurrentPageIndex))
                {
                    SpawnNote(CurrentChart.note_list[NoteSpawns[i].id], time - NoteSpawns[i].time);
                    CurrentNoteIndex = i + 1;
                }
                else if((CurrentChart.note_list[NoteSpawns[i].id].type == (int)NoteType.HOLD || CurrentChart.note_list[NoteSpawns[i].id].type == (int)NoteType.LONG_HOLD)
                    && CurrentChart.note_list[NoteSpawns[i].id].time + CurrentChart.note_list[NoteSpawns[i].id].hold_time >= time &&
                    (CurrentChart.note_list[NoteSpawns[i].id].page_index <= CurrentPageIndex || Config.ShowApproachingNotesWhilePaused))
                {
                    SpawnNote(CurrentChart.note_list[NoteSpawns[i].id], time - NoteSpawns[i].time);
                    CurrentNoteIndex = i + 1;
                }
                else if(CurrentChart.note_list[NoteSpawns[i].id].page_index + 1 == CurrentPageIndex)
                {
                    SpawnNote(CurrentChart.note_list[NoteSpawns[i].id], 10000, true);
                    CurrentNoteIndex = i + 1;
                }
                else if(CurrentChart.note_list[NoteSpawns[i].id].page_index < CurrentPageIndex)
                {
                    CurrentNoteIndex = i + 1;
                }
            }
            else if(CurrentChart.note_list[NoteSpawns[i].id].page_index == CurrentPageIndex)
            {
                SpawnNote(CurrentChart.note_list[NoteSpawns[i].id], 10000);
            }
        }

        // Optimize if necessary
        for(int i = 0; i < HitsoundTimings.Count; i++)
        {
            if(HitsoundTimings[i] < time)
            {
                CurrentHitsoundIndex = i + 1;
            }
        }
        
        for(int i = 0; i < CurrentChart.tempo_list.Count; i++)
        {
            if(CurrentChart.tempo_list[i].tick <= CurrentPage.end_tick && CurrentChart.tempo_list[i].tick >= CurrentPage.actual_start_tick)
            {
                SpawnScanlineNote(i);
            }
            if (CurrentChart.tempo_list[i].time <= time)
            {
                CurrentTempoIndex = i;
            }
        }

        GameObject.Find("CurrentBPMText").GetComponentInChildren<Text>().text = $"BPM: {Math.Round(120000000.0 / CurrentChart.tempo_list[CurrentTempoIndex].value, 2)}";

        GameObject.Find("SweepChangeButton").GetComponentInChildren<Text>().text = CurrentPage.scan_line_direction == 1 ? "Up" : "Down";

        GameObject.Find("PageText").GetComponent<Text>().text = CurrentPageIndex.ToString();
        int milliseconds = (int)((time - CurrentChart.music_offset) * 1000 - Math.Floor(time - CurrentChart.music_offset) * 1000);
        if(time < CurrentChart.music_offset && milliseconds != 0)
        {
            milliseconds = 1000 - milliseconds;
        }
        GameObject.Find("TimeText").GetComponent<Text>().text = ((int)((time - CurrentChart.music_offset) / 60)).ToString() + ":" + ((int)(time - CurrentChart.music_offset) % 60).ToString("D2") + "." + milliseconds.ToString("D3");

        GameObject.Find("NoteCountText").GetComponent<Text>().text = $"Note count: {CurrentChart.note_list.Count}";

        Scanline.transform.position = new Vector3(0, CurrentPage.scan_line_direction == 1
            ? PlayAreaHeight * (float)((time - CurrentPage.actual_start_time) /
            (CurrentPage.end_time - CurrentPage.actual_start_time) - 0.5f)
            : PlayAreaHeight * (0.5f - (float)((time - CurrentPage.actual_start_time) /
            (CurrentPage.end_time - CurrentPage.actual_start_time))));

        MusicManager.SetTime(time - Offset);

        Timeline.SetValueWithoutNotify((float)(MusicManager.Time / MusicManager.MaxTime) * PlaybackSpeeds[PlaybackSpeedIndex]);
    }

    private bool isTouchHeld = false;
    private GameObject currentlymoving;

    private Rect LastSafeArea = new Rect(0, 0, Screen.width, Screen.height);

    private int HitsoundSourceIndex = 0;

    private void Update()
    {
#if UNITY_STANDALONE
        if (WasPressed(HotkeyManager.PlayPause))
        {
            PlayPause();
        }
#endif
        if (SaveOffsetScheduledTime > 0 && Time.time > SaveOffsetScheduledTime)
        {
            SaveConfig();
            SaveOffsetScheduledTime = -1;
        }
        if(IsStartScheduled)
        {
            if(AudioSettings.dspTime > ScheduledTime)
            {
                IsGameRunning = true;
                PlayPauseButton.SetActive(true);
                IsStartScheduled = false;
            }
        }
        if(IsGameRunning)
        {
            if(!MusicManager.IsPlaying) // If the music ended
            {
                IsGameRunning = false;
                CurrentPageIndex = 0;
                UpdateTime(0);
                return;
            }

            if(Config.UpdateTimelineWhileRunning)
            {
                Timeline.SetValueWithoutNotify((float)(MusicManager.Time / MusicManager.MaxTime) * PlaybackSpeeds[PlaybackSpeedIndex]);
            }

            double time = MusicManager.Time + Offset;

            while(CurrentHitsoundIndex < HitsoundTimings.Count && HitsoundTimings[CurrentHitsoundIndex] - Config.HitsoundPrepTime <= time)
            {
                HitsoundSources[HitsoundSourceIndex].PlayScheduled(HitsoundTimings[CurrentHitsoundIndex] - time + AudioSettings.dspTime);
                HitsoundSourceIndex++;
                if(HitsoundSourceIndex > 3)
                {
                    HitsoundSourceIndex = 0;
                }
                CurrentHitsoundIndex++;
            }

            while(CurrentNoteIndex < NoteSpawns.Count && NoteSpawns[CurrentNoteIndex].time <= time)
            {
                SpawnNote(CurrentChart.note_list[NoteSpawns[CurrentNoteIndex].id], time - NoteSpawns[CurrentNoteIndex].time);
                CurrentNoteIndex++;
            }
            while (CurrentPageIndex < CurrentChart.page_list.Count && CurrentPage.end_time < time)
            {
                CurrentPageIndex++;
            }
            while(CurrentTempoIndex + 1 < CurrentChart.tempo_list.Count && CurrentChart.tempo_list[CurrentTempoIndex + 1].time < time)
            {
                CurrentTempoIndex++;
                GameObject.Find("CurrentBPMText").GetComponentInChildren<Text>().text = $"BPM: {Math.Round(120000000.0 / CurrentChart.tempo_list[CurrentTempoIndex].value, 2)}";
            }

            TimeTextBuilder.Clear();
            TimeTextBuilder.Append((int)((time - CurrentChart.music_offset) / 60));
            TimeTextBuilder.Append(':');
            TimeTextBuilder.Append(((int)(time - CurrentChart.music_offset) % 60).ToString("D2"));
            TimeTextBuilder.Append('.');
            TimeTextBuilder.Append(((int)((time - CurrentChart.music_offset) * 1000 - Math.Floor(time - CurrentChart.music_offset) * 1000)).ToString("D3"));

            GameObject.Find("PageText").GetComponent<Text>().text = CurrentPageIndex.ToString();
            GameObject.Find("TimeText").GetComponent<Text>().text = TimeTextBuilder.ToString();

            if (CurrentPageIndex < CurrentChart.page_list.Count) // in case the pages don't go to the end of the chart
            {
                double CurrentTick = CurrentChart.tempo_list[CurrentTempoIndex].tick + (time - CurrentChart.tempo_list[CurrentTempoIndex].time) * 1000000 / CurrentChart.tempo_list[CurrentTempoIndex].value * CurrentChart.time_base;

                Scanline.transform.position = new Vector3(0, CurrentPage.scan_line_direction == 1
                    ? PlayAreaHeight * (float)((CurrentTick - CurrentPage.start_tick) / CurrentPage.PageSize - 0.5)
                    : PlayAreaHeight * (float)(0.5 - (CurrentTick - CurrentPage.start_tick) / CurrentPage.PageSize));
            }
        }
        else
        {
#if UNITY_STANDALONE
            HandlePCInput();
#endif
            HandleInput();
        }
        if(Config.NotchOverlapFix && Screen.safeArea != LastSafeArea)
        {
            LastSafeArea = Screen.safeArea;
            if (Screen.orientation == ScreenOrientation.LandscapeLeft)
            {
                GameObject.Find("AddClickNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.safeArea.x - 15, GameObject.Find("AddClickNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                GameObject.Find("AddHoldNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.safeArea.x - 15, GameObject.Find("AddHoldNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                GameObject.Find("AddLongHoldNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.safeArea.x - 15, GameObject.Find("AddLongHoldNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                GameObject.Find("AddDragNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.safeArea.x - 15, GameObject.Find("AddDragNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                GameObject.Find("AddCDragNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.safeArea.x - 15, GameObject.Find("AddCDragNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
            }
            else if (Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                GameObject.Find("AddFlickNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(-Screen.safeArea.x + 15, GameObject.Find("AddFlickNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                GameObject.Find("MoveNoteButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(-Screen.safeArea.x + 15, GameObject.Find("MoveNoteButton").GetComponent<RectTransform>().anchoredPosition.y);
                GameObject.Find("BPMButton").GetComponent<RectTransform>().anchoredPosition = new Vector2(-Screen.safeArea.x + 15, GameObject.Find("BPMButton").GetComponent<RectTransform>().anchoredPosition.y);
                GameObject.Find("OtherOptionsScrollView").GetComponent<RectTransform>().anchoredPosition = new Vector2(-Screen.safeArea.x + 15, GameObject.Find("OtherOptionsScrollView").GetComponent<RectTransform>().anchoredPosition.y);
            }
        }
    }

#if UNITY_STANDALONE
    private const double NUDGE_DISTANCE = 0.02;

    private bool mousedragstarted = false;
    private Vector2 mousestartpos = new Vector2(0, 0);

    private void HandlePCInput()
    {
        if(BlockInput)
        {
            return;
        }
        if(Input.GetMouseButtonDown(0) && CurrentTool == NoteType.NONE)
        {
            mousestartpos = MainCamera.ScreenToWorldPoint(Input.mousePosition);
            if(Math.Abs(mousestartpos.x) < PlayAreaWidth / 2 + 3 && Math.Abs(mousestartpos.y) < PlayAreaHeight / 2 + 2)
            {
                SelectionBox.SetActive(true);
                SelectionBox.GetComponent<SpriteRenderer>().size = new Vector2(0, 0);
                mousedragstarted = true;
            }
        }
        if(Input.GetMouseButtonUp(0) && mousedragstarted)
        {
            SelectionBox.SetActive(false);

            Vector2 pos = MainCamera.ScreenToWorldPoint(Input.mousePosition);

            if(CurrentChart != null && GetDistance(pos.x, pos.y, mousestartpos.x, mousestartpos.y) > 0.1)
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                    {
                        if (obj.GetComponent<IHighlightable>().Highlighted)
                        {
                            HighlightObject(obj);
                        }
                    }
                }
                bool deselectall = true;
                float lx = Math.Min(pos.x, mousestartpos.x), rx = Math.Max(pos.x, mousestartpos.x), uy = Math.Max(pos.y, mousestartpos.y), dy = Math.Min(pos.y, mousestartpos.y);
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (obj.transform.position.x >= lx && obj.transform.position.x <= rx && obj.transform.position.y >= dy && obj.transform.position.y <= uy &&
                        !obj.GetComponent<IHighlightable>().Highlighted)
                    {
                        deselectall = false;
                        HighlightObject(obj);
                    }
                }

                if (deselectall)
                {
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                    {
                        if (obj.GetComponent<IHighlightable>().Highlighted)
                        {
                            HighlightObject(obj);
                        }
                    }
                }
            }
            mousedragstarted = false;
        }
        if(Input.GetMouseButton(0) && mousedragstarted)
        {
            Vector2 pos = MainCamera.ScreenToWorldPoint(Input.mousePosition);
            SelectionBox.transform.position = new Vector2((pos.x + mousestartpos.x) / 2, (pos.y + mousestartpos.y) / 2);
            SelectionBox.GetComponent<SpriteRenderer>().size = new Vector2(Math.Abs(pos.x - mousestartpos.x), Math.Abs(pos.y - mousestartpos.y));
        }

        List<Note> tohighlight = new List<Note>();

        if(WasPressed(HotkeyManager.MoveTool))
        {
            gameObject.GetComponent<SideButtonController>().HighlightButton(GameObject.Find("MoveNoteButton"));
        }
        else if(WasPressed(HotkeyManager.ClickNote))
        {
            gameObject.GetComponent<SideButtonController>().ChangeTool(NoteType.CLICK);
        }
        else if (WasPressed(HotkeyManager.HoldNote))
        {
            gameObject.GetComponent<SideButtonController>().ChangeTool(NoteType.HOLD);
        }
        else if (WasPressed(HotkeyManager.LongHoldNote))
        {
            gameObject.GetComponent<SideButtonController>().ChangeTool(NoteType.LONG_HOLD);
        }
        else if (WasPressed(HotkeyManager.DragNote))
        {
            gameObject.GetComponent<SideButtonController>().ChangeTool(NoteType.DRAG_HEAD);
        }
        else if (WasPressed(HotkeyManager.CDragNote))
        {
            gameObject.GetComponent<SideButtonController>().ChangeTool(NoteType.CDRAG_HEAD);
        }
        else if (WasPressed(HotkeyManager.FlickNote))
        {
            gameObject.GetComponent<SideButtonController>().ChangeTool(NoteType.FLICK);
        }
        else if (WasPressed(HotkeyManager.ScanlineNote))
        {
            gameObject.GetComponent<SideButtonController>().ChangeTool(NoteType.SCANLINE);
        }

        if (CurrentChart == null)
        {
            return;
        }

        if(WasPressed(HotkeyManager.Copy))
        {
            CopySelection();
        }

        else if(WasPressed(HotkeyManager.Paste))
        {
            Paste();
        }

        else if(WasPressed(HotkeyManager.SelectAll))
        {
            bool allNotesOnCurrentPageSelected = true, noNotesSelected = true, allNotesSelected = true;
            GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");
            foreach(GameObject note in notes)
            {
                bool highlighted = note.GetComponent<IHighlightable>().Highlighted;
                if(!highlighted && CurrentChart.note_list[note.GetComponent<NoteController>().NoteID].page_index == CurrentPageIndex)
                {
                    allNotesOnCurrentPageSelected = false;
                }
                else if(highlighted)
                {
                    noNotesSelected = false;
                }
                if(!highlighted)
                {
                    allNotesSelected = false;
                }
            }
            if(allNotesOnCurrentPageSelected && !allNotesSelected)
            {
                foreach(GameObject note in notes)
                {
                    if(!note.GetComponent<IHighlightable>().Highlighted)
                    {
                        HighlightObject(note);
                    }
                }
            }
            else if(noNotesSelected)
            {
                foreach (GameObject note in notes)
                {
                    if (!note.GetComponent<IHighlightable>().Highlighted && CurrentChart.note_list[note.GetComponent<NoteController>().NoteID].page_index == CurrentPageIndex)
                    {
                        HighlightObject(note);
                    }
                }
            }
            else if(!noNotesSelected)
            {
                foreach (GameObject note in notes)
                {
                    if (note.GetComponent<IHighlightable>().Highlighted)
                    {
                        HighlightObject(note);
                    }
                }
            }
        }

        else if(WasPressed(HotkeyManager.Mirror))
        {
            MirrorSelection();
        }
        
        else if(WasPressed(HotkeyManager.Flip))
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
            {
                if (obj.GetComponent<IHighlightable>().Highlighted)
                {
                    int id = obj.GetComponent<NoteController>().NoteID;
                    tohighlight.Add(CurrentChart.note_list[id]);
                    CurrentChart.note_list[id].tick = (int)Math.Round(CurrentChart.page_list[CurrentChart.note_list[id].page_index].actual_start_tick +
                        CurrentChart.page_list[CurrentChart.note_list[id].page_index].ActualPageSize * (1.0 - 
                        (CurrentChart.note_list[id].tick - CurrentChart.page_list[CurrentChart.note_list[id].page_index].actual_start_tick)
                        / CurrentChart.page_list[CurrentChart.note_list[id].page_index].ActualPageSize));
                }
            }
            FixIDs();
            CalculateTimings();
            UpdateTime(CurrentPage.start_time);
        }

        else if(WasPressed(HotkeyManager.NudgeLeft))
        {
            foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
            {
                if(obj.GetComponent<IHighlightable>().Highlighted)
                {
                    tohighlight.Add(CurrentChart.note_list[obj.GetComponent<NoteController>().NoteID]);
                    CurrentChart.note_list[obj.GetComponent<NoteController>().NoteID].x -= NUDGE_DISTANCE;
                }
            }
            UpdateTime(CurrentPage.start_time);
        }
        
        else if(WasPressed(HotkeyManager.NudgeRight))
        {
            foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
            {
                if(obj.GetComponent<IHighlightable>().Highlighted)
                {
                    tohighlight.Add(CurrentChart.note_list[obj.GetComponent<NoteController>().NoteID]);
                    CurrentChart.note_list[obj.GetComponent<NoteController>().NoteID].x += NUDGE_DISTANCE;
                }
            }
            UpdateTime(CurrentPage.start_time);
        }

        else if(WasPressed(HotkeyManager.NudgeUp))
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
            {
                if(obj.GetComponent<IHighlightable>().Highlighted)
                {
                    int id = obj.GetComponent<NoteController>().NoteID;
                    tohighlight.Add(CurrentChart.note_list[id]);
                    if (CurrentChart.page_list[CurrentChart.note_list[id].page_index].scan_line_direction > 0)
                    {
                        IncreaseTick(id);
                    }
                    else
                    {
                        DecreaseTick(id);
                    }
                }
            }
            FixIDs();
            CalculateTimings();
            UpdateTime(CurrentPage.start_time);
        }

        else if(WasPressed(HotkeyManager.NudgeDown))
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
            {
                if (obj.GetComponent<IHighlightable>().Highlighted)
                {
                    int id = obj.GetComponent<NoteController>().NoteID;
                    tohighlight.Add(CurrentChart.note_list[id]);
                    if (CurrentChart.page_list[CurrentChart.note_list[id].page_index].scan_line_direction > 0)
                    {
                        DecreaseTick(id);
                    }
                    else
                    {
                        IncreaseTick(id);
                    }
                }
            }
            FixIDs();
            CalculateTimings();
            UpdateTime(CurrentPage.start_time);
        }
        
        else if(WasPressed(HotkeyManager.PreviousPage))
        {
            GoToPreviousPage();
        }

        else if(WasPressed(HotkeyManager.NextPage))
        {
            GoToNextPage();
        }

        else if(WasPressed(HotkeyManager.BackToStart))
        {
            CurrentPageIndex = 0;
            UpdateTime(CurrentPage.actual_start_time);
        }

        else if (WasPressed(HotkeyManager.Save))
        {
            SaveChart();
        }

        else if (WasPressed(HotkeyManager.Delete))
        {
            List<int> toremove = new List<int>();
            foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
            {
                if(obj.GetComponent<IHighlightable>().Highlighted)
                {
                    toremove.Add(obj.GetComponent<NoteController>().NoteID);
                }
            }
            toremove.Sort();
            for(int i = 0; i < toremove.Count; i++)
            {
                RemoveNote(toremove[i] - i);
            }
            UpdateTime(CurrentPage.actual_start_time);
        }

        for (int i = 0; i < tohighlight.Count; i++)
        {
            HighlightNoteWithID(tohighlight[i].id);
        }
    }


    private void FixIDs()
    {
        for(int i = 0; i < CurrentChart.note_list.Count; i++)
        {
            int id = i;
            while (id < CurrentChart.note_list.Count - 1 && CurrentChart.note_list[id].tick > CurrentChart.note_list[id + 1].tick)
            {
                int pid1 = GetDragParent(id), pid2 = GetDragParent(id + 1);
                if (pid1 >= 0)
                {
                    CurrentChart.note_list[pid1].next_id++;
                }
                if (pid2 >= 0)
                {
                    CurrentChart.note_list[pid2].next_id--;
                }
                Note aux = CurrentChart.note_list[id];
                CurrentChart.note_list[id] = CurrentChart.note_list[id + 1];
                CurrentChart.note_list[id + 1] = aux;
                CurrentChart.note_list[id].id = id;
                CurrentChart.note_list[id + 1].id = id + 1;
                id++;
            }
            while (id > 0 && CurrentChart.note_list[id].tick < CurrentChart.note_list[id - 1].tick)
            {
                int pid1 = GetDragParent(id), pid2 = GetDragParent(id - 1);
                if (pid1 >= 0)
                {
                    CurrentChart.note_list[pid1].next_id--;
                }
                if (pid2 >= 0)
                {
                    CurrentChart.note_list[pid2].next_id++;
                }
                Note aux = CurrentChart.note_list[id];
                CurrentChart.note_list[id] = CurrentChart.note_list[id - 1];
                CurrentChart.note_list[id - 1] = aux;
                CurrentChart.note_list[id].id = id;
                CurrentChart.note_list[id - 1].id = id - 1;
                id--;
            }
        }
    }

    private void IncreaseTick(int id)
    {
        Note note = CurrentChart.note_list[id];
        Page p = CurrentChart.page_list[note.page_index];
        int deltatick = (int)p.ActualPageSize / DivisorValue;
        if (note.type == (int)NoteType.HOLD)
        {
            if (note.tick + note.hold_tick + deltatick <= p.end_tick)
            {
                note.tick += deltatick;
            }
            else
            {
                note.tick += deltatick;
                note.hold_tick = p.end_tick - note.tick;
            }
        }
        else if (note.type == (int)NoteType.LONG_HOLD)
        {
            note.tick += deltatick;
        }
        else if ((note.type == (int)NoteType.DRAG_HEAD || note.type == (int)NoteType.DRAG_CHILD || note.type == (int)NoteType.CDRAG_CHILD || note.type == (int)NoteType.CDRAG_HEAD)
            && (note.next_id > 0))
        {
            if (note.tick + deltatick <= Math.Min(p.end_tick, CurrentChart.note_list[note.next_id].tick))
            {
                note.tick += deltatick;
            }
            else
            {
                note.tick = Math.Min(p.end_tick, CurrentChart.note_list[note.next_id].tick);
            }
        }
        else
        {
            if (note.tick + deltatick <= p.end_tick)
            {
                note.tick += deltatick;
            }
            else
            {
                note.tick = p.end_tick;
            }
        }
        
    }

    private void DecreaseTick(int id)
    {
        Note note = CurrentChart.note_list[id];
        Page p = CurrentChart.page_list[note.page_index];
        int deltatick = (int)p.ActualPageSize / DivisorValue;
        if ((note.type == (int)NoteType.DRAG_HEAD || note.type == (int)NoteType.DRAG_CHILD || note.type == (int)NoteType.CDRAG_CHILD || note.type == (int)NoteType.CDRAG_HEAD)
            && (GetDragParent(note.id) > -1))
        {
            int parent = GetDragParent(note.id);
            if (note.tick - deltatick >= Math.Max(p.actual_start_tick, CurrentChart.note_list[parent].tick))
            {
                note.tick -= deltatick;
            }
            else
            {
                note.tick = Math.Max(p.actual_start_tick, CurrentChart.note_list[parent].tick);
            }
        }
        else
        {
            if (note.tick - deltatick >= p.actual_start_tick)
            {
                note.tick -= deltatick;
            }
            else
            {
                note.tick = p.actual_start_tick;
            }
        }
    }
#endif

    private void HandleInput()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector2 touchpos = MainCamera.ScreenToWorldPoint(Input.mousePosition);

            isTouchHeld = true;

            if (CurrentTool == NoteType.NONE)
            {
                List<GameObject> tohighlight = new List<GameObject>();

                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (obj.GetComponent<IHighlightable>().Highlighted && obj.GetComponent<NoteController>().NoteType == (int)NoteType.HOLD) // Modifying hold_time for short holds
                    {
                        var holdcontroller = obj.GetComponent<HoldNoteController>();
                        int id = holdcontroller.NoteID;
                        if (holdcontroller.UpArrowCollider.OverlapPoint(touchpos))
                        {
                            CurrentChart.note_list[id].hold_tick += CurrentChart.time_base / DivisorValue;
                            if (CurrentChart.note_list[id].hold_tick + CurrentChart.note_list[id].tick > CurrentChart.page_list[CurrentChart.note_list[id].page_index].end_tick)
                            {
                                CurrentChart.note_list[id].hold_tick = CurrentChart.page_list[CurrentChart.note_list[id].page_index].end_tick - CurrentChart.note_list[id].tick;
                            }

                            CalculateTimings();
                            UpdateTime(CurrentPage.actual_start_time);
                            HighlightNoteWithID(id);
                        }
                        else if (holdcontroller.DownArrowCollider.OverlapPoint(touchpos))
                        {
                            CurrentChart.note_list[id].hold_tick -= CurrentChart.time_base / DivisorValue;
                            if (CurrentChart.note_list[id].hold_tick < 0)
                            {
                                CurrentChart.note_list[id].hold_tick = 1;
                            }

                            CalculateTimings();
                            UpdateTime(CurrentPage.actual_start_time);
                            HighlightNoteWithID(id);
                        }
                        
                    }
                    else if (obj.GetComponent<IHighlightable>().Highlighted && obj.GetComponent<NoteController>().NoteType == (int)NoteType.LONG_HOLD) // Modifying hold_time for long holds
                    {
                        var holdcontroller = obj.GetComponent<LongHoldNoteController>();
                        int id = holdcontroller.NoteID;
                        if (holdcontroller.UpArrowCollider.OverlapPoint(touchpos))
                        {
                            CurrentChart.note_list[id].hold_tick += CurrentChart.time_base / DivisorValue;

                            CalculateTimings();
                            UpdateTime(CurrentPage.actual_start_time);
                            HighlightNoteWithID(id);
                        }
                        else if (holdcontroller.DownArrowCollider.OverlapPoint(touchpos))
                        {
                            CurrentChart.note_list[id].hold_tick -= CurrentChart.time_base / DivisorValue;

                            if (CurrentChart.note_list[id].hold_tick < 0)
                            {
                                CurrentChart.note_list[id].hold_tick = 1;
                            }

                            CalculateTimings();
                            UpdateTime(CurrentPage.actual_start_time);
                            HighlightNoteWithID(id);
                        }
                    }
                    if (obj.GetComponentInChildren<CircleCollider2D>().OverlapPoint(touchpos)) // Deciding which note to highlight
                    {
#if UNITY_STANDALONE
                        if(Input.GetKey(KeyCode.LeftShift))
                        {
                            tohighlight.Add(obj);
                        }
                        else
                        {
                            foreach (GameObject obj2 in GameObject.FindGameObjectsWithTag("Note"))
                            {
                                if (obj2.GetComponent<IHighlightable>().Highlighted)
                                {
                                    HighlightObject(obj2);
                                }
                            }
                            tohighlight.Add(obj);
                        }
#endif
                        if(Application.isMobilePlatform)
                        {
                            tohighlight.Add(obj);
                        }
                    }
                }

                int idtohighlight = -1;
                bool onlycurrentpage = false;
                for(int i = 0; i < tohighlight.Count; i++)
                {
                    int id = tohighlight[i].GetComponent<NoteController>().NoteID;
                    if(CurrentChart.note_list[id].page_index == CurrentPageIndex)
                    {
                        if(!onlycurrentpage)
                        {
                            idtohighlight = id;
                        }
                        onlycurrentpage = true;
                        idtohighlight = Math.Max(idtohighlight, id);
                    }
                    else if(!onlycurrentpage)
                    {
                        idtohighlight = Math.Max(idtohighlight, id);
                    }
                }
                if(idtohighlight >= 0)
                {
                    HighlightNoteWithID(idtohighlight);
                }
            }
            else if (CurrentTool == NoteType.MOVE) // Starting the move of notes
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if ((Config.InteractWithNotesOnOtherPages || CurrentChart.note_list[obj.GetComponent<NoteController>().NoteID].page_index == CurrentPageIndex)
                        && obj.GetComponentInChildren<Collider2D>().OverlapPoint(touchpos))
                    {
                        currentlymoving = obj;
                        LockedX = currentlymoving.transform.position.x;
                    }
                }
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("ScanlineNote"))
                {
                    if (obj.GetComponent<ITempo>().TempoID != 0 && obj.GetComponentInChildren<Collider2D>().OverlapPoint(touchpos))
                    {
                        currentlymoving = obj;
                        break;
                    }
                }
            }
            else if (CurrentChart != null && touchpos.x < PlayAreaWidth / 2 && touchpos.x > -PlayAreaWidth / 2 && touchpos.y < PlayAreaHeight / 2 && touchpos.y > -PlayAreaHeight / 2)
            // Adding notes
            {
                foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (obj.GetComponent<NoteController>().NoteType == (int)CurrentTool && Math.Abs(touchpos.y - obj.transform.position.y) < (PlayAreaHeight / DivisorValue) / 2
                        && Math.Abs(touchpos.x - obj.transform.position.x) < (PlayAreaHeight / DivisorValue) / 2)
                    {
                        return;
                    }
                }
                double notex = (touchpos.x + PlayAreaWidth / 2) / PlayAreaWidth;
                if(Config.HorizontalSnap)
                {
                    notex = Math.Round((touchpos.x + PlayAreaWidth / 2) / (PlayAreaWidth / Config.VerticalDivisors)) / Config.VerticalDivisors;
                }
                if (CurrentTool == NoteType.CLICK) // Add click note
                {
                    AddNote(new Note
                    {
                        x = notex,
                        page_index = CurrentPageIndex,
                        type = (int)NoteType.CLICK,
                        id = -1,
                        hold_tick = 0,
                        next_id = 0,
                        tick = (int)(CurrentPage.actual_start_tick + CurrentPage.ActualPageSize *
                            (CurrentPage.scan_line_direction == 1 ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                            : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue))
                    });

                    CalculateTimings();
                    UpdateTime(CurrentPage.actual_start_time);
                }
                else if (CurrentTool == NoteType.HOLD) // Add hold note
                {
                    int tick = (int)(CurrentPage.actual_start_tick + CurrentPage.ActualPageSize * (CurrentPage.scan_line_direction == 1
                        ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                        : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue));

                    AddNote(new Note
                    {
                        x = notex,
                        page_index = tick == CurrentPage.end_tick ? CurrentPageIndex + 1 : CurrentPageIndex,
                        type = (int)NoteType.HOLD,
                        id = -1,
                        hold_tick = CurrentChart.time_base / DivisorValue,
                        next_id = 0,
                        tick = tick
                    });

                    CalculateTimings();
                    UpdateTime(CurrentPage.actual_start_time);
                }
                else if (CurrentTool == NoteType.LONG_HOLD) // Add long hold note
                {
                    int tick = (int)(CurrentPage.actual_start_tick + CurrentPage.ActualPageSize * (CurrentPage.scan_line_direction == 1
                        ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                        : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue));

                    AddNote(new Note
                    {
                        x = notex,
                        page_index = tick == CurrentPage.end_tick ? CurrentPageIndex + 1 : CurrentPageIndex,
                        type = (int)NoteType.LONG_HOLD,
                        id = -1,
                        hold_tick = CurrentChart.time_base / DivisorValue,
                        next_id = 0,
                        tick = tick
                    });

                    CalculateTimings();
                    UpdateTime(CurrentPage.actual_start_time);
                }
                else if (CurrentTool == NoteType.FLICK) // Add flick note
                {
                    AddNote(new Note
                    {
                        x = notex,
                        page_index = CurrentPageIndex,
                        type = (int)NoteType.FLICK,
                        id = -1,
                        hold_tick = 0,
                        next_id = 0,
                        tick = (int)(CurrentPage.actual_start_tick + CurrentPage.ActualPageSize *
                            (CurrentPage.scan_line_direction == 1 ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                            : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue))
                    });

                    CalculateTimings();
                    UpdateTime(CurrentPage.actual_start_time);
                }
                else if (CurrentTool == NoteType.DRAG_HEAD) // Add drag head and child
                {
                    bool existsHighlightedDragHead = false;
                    int IDtoHighlight = -1;
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                    {
                        if (obj.GetComponent<IHighlightable>().Highlighted && CurrentChart.note_list[obj.GetComponent<NoteController>().NoteID].next_id == -1 && (obj.GetComponent<DragHeadNoteController>() != null || obj.GetComponent<DragChildNoteController>() != null))
                        // Add drag child
                        {
                            int tick = (int)(CurrentPage.actual_start_tick + CurrentPage.ActualPageSize *
                                    (CurrentPage.scan_line_direction == 1 ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                                    : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue));

                            if (CurrentChart.note_list[obj.GetComponent<NoteController>().NoteID].tick < tick)
                            {
                                int id = AddNote(new Note
                                {
                                    x = notex,
                                    page_index = CurrentPageIndex,
                                    type = (int)NoteType.DRAG_CHILD,
                                    id = -1,
                                    hold_tick = 0,
                                    next_id = -1,
                                    tick = tick
                                });
                                IDtoHighlight = id;
                                CurrentChart.note_list[obj.GetComponent<NoteController>().NoteID].next_id = id;
                            }
                            existsHighlightedDragHead = true;
                            break;
                        }
                    }

                    if (!existsHighlightedDragHead) // Add drag head
                    {
                        IDtoHighlight = AddNote(new Note
                        {
                            x = notex,
                            page_index = CurrentPageIndex,
                            type = (int)NoteType.DRAG_HEAD,
                            id = -1,
                            hold_tick = 0,
                            next_id = -1,
                            tick = (int)(CurrentPage.actual_start_tick + CurrentPage.ActualPageSize *
                            (CurrentPage.scan_line_direction == 1 ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                            : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue))
                        });
                    }

                    CalculateTimings();
                    UpdateTime(CurrentPage.actual_start_time);
                    HighlightNoteWithID(IDtoHighlight);
                }
                else if (CurrentTool == NoteType.CDRAG_HEAD) // Add cdrag head and child
                {
                    bool existsHighlightedDragHead = false;
                    int IDtoHighlight = -1;
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                    {
                        if (obj.GetComponent<IHighlightable>().Highlighted && CurrentChart.note_list[obj.GetComponent<NoteController>().NoteID].next_id == -1 && (obj.GetComponent<DragHeadNoteController>() != null || obj.GetComponent<DragChildNoteController>() != null))
                        // Add drag child
                        {
                            int tick = (int)(CurrentPage.actual_start_tick + CurrentPage.ActualPageSize *
                                    (CurrentPage.scan_line_direction == 1 ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                                    : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue));

                            if (CurrentChart.note_list[obj.GetComponent<NoteController>().NoteID].tick < tick)
                            {
                                int id = AddNote(new Note
                                {
                                    x = notex,
                                    page_index = CurrentPageIndex,
                                    type = (int)NoteType.CDRAG_CHILD,
                                    id = -1,
                                    hold_tick = 0,
                                    next_id = -1,
                                    tick = tick
                                });
                                IDtoHighlight = id;
                                CurrentChart.note_list[obj.GetComponent<NoteController>().NoteID].next_id = id;
                            }
                            existsHighlightedDragHead = true;
                            break;
                        }
                    }

                    if (!existsHighlightedDragHead) // Add drag head
                    {
                        IDtoHighlight = AddNote(new Note
                        {
                            x = notex,
                            page_index = CurrentPageIndex,
                            type = (int)NoteType.CDRAG_HEAD,
                            id = -1,
                            hold_tick = 0,
                            next_id = -1,
                            tick = (int)(CurrentPage.actual_start_tick + CurrentPage.ActualPageSize *
                            (CurrentPage.scan_line_direction == 1 ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                            : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue))
                        });
                    }

                    CalculateTimings();
                    UpdateTime(CurrentPage.actual_start_time);
                    HighlightNoteWithID(IDtoHighlight);
                }
                else if (CurrentTool == NoteType.SCANLINE) // Add scanline/tempo note
                {
                    AddTempo(new Tempo
                    {
                        tick = (int)(CurrentPage.actual_start_tick + CurrentPage.ActualPageSize *
                            (CurrentPage.scan_line_direction == 1 ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                            : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue)),
                        value = 1000000,
                    });

                    CalculateTimings();
                    UpdateTime(CurrentPage.actual_start_time);
                }
                GameObject.Find("NoteCountText").GetComponent<Text>().text = $"Note count: {CurrentChart.note_list.Count}";
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (currentlymoving != null && CurrentTool == NoteType.MOVE) // Finish the move and update the chart
            {
                if (currentlymoving.transform.position.x > PlayAreaWidth / 2 + 2 || currentlymoving.transform.position.x < -PlayAreaWidth / 2 - 2)
                {
                    if (currentlymoving.CompareTag("ScanlineNote"))
                    {
                        RemoveTempo(currentlymoving.GetComponent<ITempo>().TempoID);
                        Destroy(currentlymoving);
                    }
                    else
                    {
                        RemoveNote(currentlymoving.GetComponent<NoteController>().NoteID);
                        ObjectPool.ReturnToPool(currentlymoving, currentlymoving.GetComponent<NoteController>().NoteType);
                    }
                    CalculateTimings();
                    UpdateTime(CurrentPage.actual_start_time);
                }
                else
                {
                    currentlymoving.transform.position = new Vector3(LockX ? LockedX : Clamp(currentlymoving.transform.position.x, -PlayAreaWidth / 2, PlayAreaWidth / 2),
                        Clamp(currentlymoving.transform.position.y, -PlayAreaHeight / 2, PlayAreaHeight / 2));

                    if (currentlymoving.CompareTag("Note"))
                    {
                        float newX = currentlymoving.transform.position.x;
                        if (Config.HorizontalSnap)
                        {
                            newX = (float)Math.Round((currentlymoving.transform.position.x + PlayAreaWidth / 2) / (PlayAreaWidth / Config.VerticalDivisors)) * (PlayAreaWidth / Config.VerticalDivisors) - PlayAreaWidth / 2;
                        }
                        else
                        {
                            newX = (float)Math.Round(newX, 2);
                        }
                        currentlymoving.transform.position = new Vector3(newX,
                            (float)Math.Round((currentlymoving.transform.position.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) * (PlayAreaHeight / DivisorValue) - PlayAreaHeight / 2);

                        int id = currentlymoving.GetComponent<NoteController>().NoteID;
                        Note note = CurrentChart.note_list[id];
                        note.x = (currentlymoving.transform.position.x + PlayAreaWidth / 2) / PlayAreaWidth;
                        int tick = (int)Math.Round(CurrentChart.page_list[note.page_index].actual_start_tick + CurrentChart.page_list[note.page_index].ActualPageSize *
                            (CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? (currentlymoving.transform.position.y + PlayAreaHeight / 2) / PlayAreaHeight
                            : 1.0f - (currentlymoving.transform.position.y + PlayAreaHeight / 2) / PlayAreaHeight));

                        int dragparent = GetDragParent(id), dragchild = note.next_id;

                        if(dragparent >= 0 && tick == CurrentChart.note_list[dragparent].tick)
                        {
                            tick++;
                        }

                        if ((dragparent == -1 || tick > CurrentChart.note_list[dragparent].tick) && (dragchild <= 0 || tick < CurrentChart.note_list[dragchild].tick))
                        {
                            note.tick = tick;

                            RemoveNote(id);
                            int newid = AddNote(note);
                            if (dragparent > -1)
                            {
                                CurrentChart.note_list[dragparent].next_id = newid;
                            }
                            CurrentChart.note_list[newid].next_id = dragchild;
                            if (dragchild > 0 && CurrentChart.note_list[dragchild].type == (int)NoteType.DRAG_HEAD)
                            {
                                CurrentChart.note_list[dragchild].type = (int)NoteType.DRAG_CHILD;
                            }
                            else if (dragchild > 0 && CurrentChart.note_list[dragchild].type == (int)NoteType.CDRAG_HEAD)
                            {
                                CurrentChart.note_list[dragchild].type = (int)NoteType.CDRAG_CHILD;
                            }
                        }

                        if(note.type == (int)NoteType.HOLD && tick + note.hold_tick > CurrentChart.page_list[note.page_index].end_tick)
                        {
                            note.hold_tick = CurrentChart.page_list[note.page_index].end_tick - tick;
                        }
                    }
                    else if (currentlymoving.CompareTag("ScanlineNote"))
                    {
                        currentlymoving.transform.position = new Vector3(-PlayAreaWidth / 2 - 1,
                            (float)Math.Round((currentlymoving.transform.position.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) * (PlayAreaHeight / DivisorValue) - PlayAreaHeight / 2);

                        int id = currentlymoving.GetComponent<ITempo>().TempoID;
                        Tempo tempo = CurrentChart.tempo_list[id];
                        tempo.tick = (int)Math.Round((CurrentPage.scan_line_direction == 1 ? (currentlymoving.transform.position.y + PlayAreaHeight / 2) / PlayAreaHeight :
                            1.0 - (currentlymoving.transform.position.y + PlayAreaHeight / 2) / PlayAreaHeight) * CurrentPage.ActualPageSize) + CurrentPage.actual_start_tick;

                        RemoveTempo(id);
                        AddTempo(tempo);
                    }

                    CalculateTimings();

                    UpdateTime(CurrentPage.actual_start_time);
                }
                currentlymoving = null;
            }
            isTouchHeld = false;
        }

        if (isTouchHeld && CurrentTool == NoteType.MOVE && currentlymoving != null) // Handle moving notes
        {
            Vector2 touchpos = MainCamera.ScreenToWorldPoint(Input.mousePosition);

            currentlymoving.transform.position = touchpos;
        }
    }

    private void HighlightNoteWithID(int id)
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("Note"))
        {
            if (obj.GetComponent<NoteController>().NoteID == id)
            {
                HighlightObject(obj);
            }
        }
    }

    public void GoToPreviousPage()
    {
        if(!IsGameRunning && CurrentChart != null && CurrentPageIndex > 0)
        {
            CurrentPageIndex--;
            UpdateTime(CurrentPage.actual_start_time);
        }
    }

    public void GoToNextPage()
    {
        if (!IsGameRunning && CurrentChart != null && CurrentPageIndex < CurrentChart.page_list.Count)
        {
            CurrentPageIndex++;
            UpdateTime(CurrentPage.actual_start_time);
        }
    }

    public void ChangeTempo(GameObject scanlineNote, bool updateoffset = false)
    {
        string bpminput = scanlineNote.GetComponent<ScanlineNoteController>().BPMInputField.text, timeinput = scanlineNote.GetComponent<ScanlineNoteController>().TimeInputField.text;
        int id = scanlineNote.GetComponent<ITempo>().TempoID;
        if(double.TryParse(bpminput, out double bpm))
        {
            CurrentChart.tempo_list[id].value = (long)Math.Round(120000000 / bpm);
        }
        if(updateoffset && double.TryParse(timeinput, out double time) && id == 0)
        {
            CurrentChart.music_offset = -time;
        }
        CalculateTimings();
        UpdateTime(CurrentPage.actual_start_time);
    }

    public void IncreasePlaybackSpeed()
    {
        if(CurrentChart != null && PlaybackSpeedIndex != 2 && !IsGameRunning && !IsStartScheduled)
        {
            PlaybackSpeedIndex++;
            UpdatePlaybackSpeed();
        }
    }

    public void DecreasePlaybackSpeed()
    {
        if(CurrentChart != null && PlaybackSpeedIndex != 0 && !IsGameRunning && !IsStartScheduled)
        {
            PlaybackSpeedIndex--;
            UpdatePlaybackSpeed();
        }
    }

    private void UpdatePlaybackSpeed()
    {
        if (PlaybackSpeedIndex == 0)
        {
            MusicSource.outputAudioMixerGroup = HalfSpeedMixer;
        }
        else if (PlaybackSpeedIndex == 1)
        {
            MusicSource.outputAudioMixerGroup = ThreeQuarterSpeedMixer;
        }
        else if (PlaybackSpeedIndex == 2)
        {
            MusicSource.outputAudioMixerGroup = null;
        }
        MusicManager.PlaybackSpeed = PlaybackSpeeds[PlaybackSpeedIndex];
        UpdateTime(CurrentPage.actual_start_time);
        GameObject.Find("PlaybackSpeedText").GetComponent<Text>().text = $"{(int)(PlaybackSpeeds[PlaybackSpeedIndex] * 100)}%";
    }

    private double SaveOffsetScheduledTime = -1;

    public void IncreaseOffset()
    {
        SaveOffsetScheduledTime = Time.time + 5;
        Config.UserOffset += Config.PreciseOffsetDelta ? 1 : 5;
        UpdateOffsetText();
    }

    public void DecreaseOffset()
    {
        SaveOffsetScheduledTime = Time.time + 5;
        Config.UserOffset -= Config.PreciseOffsetDelta ? 1 : 5;
        UpdateOffsetText();
    }

    private void UpdateOffsetText()
    {
        GameObject.Find("OffsetText").GetComponent<Text>().text = $"{Config.UserOffset}ms";
    }

    public void ChangeSweepDirection()
    {
        if(CurrentChart != null)
        {
            for(int i = CurrentPageIndex; i < CurrentChart.page_list.Count; i++)
            {
                CurrentChart.page_list[i].scan_line_direction = -CurrentChart.page_list[i].scan_line_direction;
            }
            GameObject.Find("SweepChangeButton").GetComponentInChildren<Text>().text = CurrentPage.scan_line_direction == 1 ? "Up" : "Down";
            CalculateTimings();
            UpdateTime(CurrentPage.actual_start_time);
        }
    }

    public void HighlightObject(GameObject obj)
    {
        IHighlightable HighlightInfo = obj.GetComponent<IHighlightable>();
        HighlightInfo.Highlight();
        if(HighlightInfo.Highlighted)
        {
            if(obj.CompareTag("Note"))
            {
                Note note = CurrentChart.note_list[obj.GetComponent<NoteController>().NoteID];
                NotePropsManager.Add(note);

                // Fix hold arrows overlapping with buttons/timeline
                if(obj.GetComponent<NoteController>().NoteType == (int)NoteType.HOLD || obj.GetComponent<NoteController>().NoteType == (int)NoteType.LONG_HOLD)
                {
                    Bounds UpBounds, DownBounds;
                    if(obj.GetComponent<NoteController>().NoteType == (int)NoteType.LONG_HOLD)
                    {
                        UpBounds = obj.GetComponent<LongHoldNoteController>().UpArrowCollider.bounds;
                        DownBounds = obj.GetComponent<LongHoldNoteController>().DownArrowCollider.bounds;
                    }
                    else
                    {
                        UpBounds = obj.GetComponent<HoldNoteController>().UpArrowCollider.bounds;
                        DownBounds = obj.GetComponent<HoldNoteController>().DownArrowCollider.bounds;
                    }
                    Vector3[] bounds = new Vector3[4];

                    GameObject.Find("LevelOptionsButton").GetComponent<RectTransform>().GetWorldCorners(bounds);
                    Bounds b = new Bounds(new Vector3((bounds[0].x + bounds[1].x + bounds[2].x + bounds[3].x) / 4, (bounds[0].y + bounds[1].y + bounds[2].y + bounds[3].y) / 4, UpBounds.center.z), 
                        new Vector3((bounds[3].x - bounds[0].x) / 2, (bounds[1].y - bounds[0].y) / 2, 3));
                    if(b.Intersects(UpBounds) || b.Intersects(DownBounds))
                    {
                        GameObject.Find("LevelOptionsButton").GetComponent<Button>().interactable = false;
                    }
                    
                    GameObject.Find("EditorSettingsButton").GetComponent<RectTransform>().GetWorldCorners(bounds);
                    b = new Bounds(new Vector3((bounds[0].x + bounds[1].x + bounds[2].x + bounds[3].x) / 4, (bounds[0].y + bounds[1].y + bounds[2].y + bounds[3].y) / 4, UpBounds.center.z),
                        new Vector3((bounds[3].x - bounds[0].x) / 2, (bounds[1].y - bounds[0].y) / 2, 3));
                    if (b.Intersects(UpBounds) || b.Intersects(DownBounds))
                    {
                        GameObject.Find("EditorSettingsButton").GetComponent<Button>().interactable = false;
                    }

                    GameObject.Find("SaveButton").GetComponent<RectTransform>().GetWorldCorners(bounds);
                    b = new Bounds(new Vector3((bounds[0].x + bounds[1].x + bounds[2].x + bounds[3].x) / 4, (bounds[0].y + bounds[1].y + bounds[2].y + bounds[3].y) / 4, UpBounds.center.z),
                        new Vector3((bounds[3].x - bounds[0].x) / 2, (bounds[1].y - bounds[0].y) / 2, 3));
                    if (b.Intersects(UpBounds) || b.Intersects(DownBounds))
                    {
                        GameObject.Find("SaveButton").GetComponent<Button>().interactable = false;
                    }

                    GameObject.Find("Timeline").GetComponent<RectTransform>().GetWorldCorners(bounds);
                    b = new Bounds(new Vector3((bounds[0].x + bounds[1].x + bounds[2].x + bounds[3].x) / 4, (bounds[0].y + bounds[1].y + bounds[2].y + bounds[3].y) / 4, UpBounds.center.z),
                        new Vector3((bounds[3].x - bounds[0].x) / 2, (bounds[1].y - bounds[0].y) / 2, 3));
                    if (b.Intersects(UpBounds) || b.Intersects(DownBounds))
                    {
                        GameObject.Find("Timeline").GetComponent<Slider>().interactable = false;
                    }
                }
            }
        }
        else
        {
            if (obj.CompareTag("Note"))
            {
                Note note = CurrentChart.note_list[obj.GetComponent<NoteController>().NoteID];
                NotePropsManager.Remove(note);
            }
        }
        if(NotePropsManager.IsEmpty)
        {
            MakeButtonsInteractable();
        }
    }

    private static void MakeButtonsInteractable()
    {
        GameObject.Find("LevelOptionsButton").GetComponent<Button>().interactable = true;
        GameObject.Find("EditorSettingsButton").GetComponent<Button>().interactable = true;
        GameObject.Find("SaveButton").GetComponent<Button>().interactable = true;
        GameObject.Find("Timeline").GetComponent<Slider>().interactable = true;
    }

    public void SaveChart()
    {
        if(CurrentChart != null)
        {
            CurrentChart.event_order_list.Clear();

            for(int i = 1; i < CurrentChart.tempo_list.Count; i++)
            {
                CurrentChart.event_order_list.Add(new EventOrder
                {
                    tick = CurrentChart.tempo_list[i].tick,
                    event_list = new List<EventOrder.Event>(1)
                });
                CurrentChart.event_order_list[i - 1].event_list.Add(new EventOrder.Event()
                {
                    type = CurrentChart.tempo_list[i].value > CurrentChart.tempo_list[i - 1].value ? 1 : 0,
                    args = CurrentChart.tempo_list[i].value > CurrentChart.tempo_list[i - 1].value ? "G" : "R"
                });
            }

            while(CurrentChart.note_list.Count > 0 && CurrentChart.note_list[CurrentChart.note_list.Count - 1].page_index >= CurrentChart.page_list.Count)
            {
                CurrentChart.note_list.RemoveAt(CurrentChart.note_list.Count - 1);
            }

            File.WriteAllText(Path.Combine(CurrentLevelPath, CurrentChart.Data.path), JsonConvert.SerializeObject(CurrentChart, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            }));

            LevelDataChanger.SaveLevel();

            GameObject.Find("ToastText").GetComponent<ToastMessageManager>().CreateToast("Saved chart!");
        }
    }

    public void CopySelection()
    {
        Clipboard.Clear();
        Clipboard.ReferencePageIndex = CurrentPageIndex;
        Clipboard.ReferenceTick = CurrentPage.actual_start_tick;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
        {
            if (obj.GetComponent<IHighlightable>().Highlighted)
            {
                int id = obj.GetComponent<NoteController>().NoteID;
                if(CurrentChart.note_list[id].type != (int)NoteType.CDRAG_HEAD && CurrentChart.note_list[id].type != (int)NoteType.DRAG_HEAD &&
                    CurrentChart.note_list[id].type != (int)NoteType.CDRAG_CHILD && CurrentChart.note_list[id].type != (int)NoteType.DRAG_CHILD)
                {
                    Clipboard.Add(CurrentChart.note_list[id]);
                }
            }
        }

        GameObject.Find("ToastText").GetComponent<ToastMessageManager>().CreateToast("Copied to clipboard!");
    }

    public void Paste()
    {
        if (IsGameRunning)
        {
            return;
        }
        List<Note> notes = Clipboard.GetNotes();
        if(notes.Count > 0)
        {
            for(int i = 0; i < notes.Count; i++)
            {
                notes[i].tick += CurrentPage.start_tick - Clipboard.ReferenceTick;
                notes[i].page_index += CurrentPageIndex - Clipboard.ReferencePageIndex;
                notes[i].id = -1;
                AddNote(notes[i]);
            }
        }
        CalculateTimings();
        UpdateTime(CurrentPage.actual_start_time);
        for(int i = 0; i < notes.Count; i++)
        {
            HighlightNoteWithID(notes[i].id);
        }
    }

    public void MirrorSelection()
    {
        if(IsGameRunning)
        {
            return;
        }
        List<int> notes = new List<int>();
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
        {
            if(obj.GetComponent<IHighlightable>().Highlighted)
            {
                int id = obj.GetComponent<NoteController>().NoteID;
                notes.Add(id);
                CurrentChart.note_list[id].x = 1 - CurrentChart.note_list[id].x;
            }
        }
        UpdateTime(CurrentPage.actual_start_time);
        for (int i = 0; i < notes.Count; i++)
        {
            HighlightNoteWithID(notes[i]);
        }
    }
}
