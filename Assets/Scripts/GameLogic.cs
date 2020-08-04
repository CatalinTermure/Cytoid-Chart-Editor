using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.Audio;

using static GlobalState;
using System.Xml.Schema;

public class GameLogic : MonoBehaviour
{
    /// <summary>
    /// The currently selected note adder/tool.
    /// </summary>
    public static NoteType CurrentTool = NoteType.NONE;

    public GameObject ClickNote;
    public GameObject ScanlineNote;
    public GameObject HoldNote;
    public GameObject LongHoldNote;
    public GameObject FlickNote;
    public GameObject DragHeadNote;
    public GameObject DragChildNote;
    public GameObject CDragHeadNote;
    public GameObject CDragChildNote;
    public GameObject DivisorLine;

    public GameObject Scanline;

    public Slider Timeline;

    public AudioSource MusicSource;

    public AudioSource[] HitsoundSources;
    private readonly double[] HitsoundScheduledTimes = new double[4];

    public Camera MainCamera;

    public Slider BeatDivisor;

    public AudioMixerGroup HalfSpeedMixer, ThreeQuarterSpeedMixer;

    public static int CurrentPageIndexOverride = -1;

    private int CurrentHoldHitsoundIndex { get; set; }
    private int CurrentHitsoundIndex { get; set; }
    private int CurrentTempoIndex { get; set; }
    private int CurrentNoteIndex { get; set; }
    private int CurrentPageIndex { get; set; }
    private Page CurrentPage
    {
        get => CurrentChart.page_list[CurrentPageIndex];
    }

    private double ScheduledTime { get; set; }
    private bool IsStartScheduled = false;

    private int PlaybackSpeedIndex = 2;
    private readonly float[] PlaybackSpeeds = new float[3] { 0.5f, 0.75f, 1.0f };

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

        #if CCE_DEBUG
        if(CurrentChart != null)
        {
            File.AppendAllText(LogPath, "Adjusted to resolution...\n");
        }
        #endif

        // Keep current page index after navigating to editor/level/chart options
        LevelOptionsButton.GetComponent<Button>().onClick.AddListener(() => CurrentPageIndexOverride = CurrentPageIndex);
        EditorSettingsButton.GetComponent<Button>().onClick.AddListener(() => CurrentPageIndexOverride = CurrentPageIndex);

        // Create vertical divisor lines
        for (int i = 0; i <= Config.VerticalDivisors; i++)
        {
            GameObject obj = Instantiate(DivisorLine);
            obj.transform.position = new Vector3(PlayAreaWidth / Config.VerticalDivisors * i - PlayAreaWidth / 2, 0);
            obj.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
            obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaHeight, 0.05f);
            obj.tag = "GridLine";
        }

        // Create horizontal divisor lines
        BeatDivisorValueChanged();

        UpdateOffsetText();

        GameObject.Find("BeatDivisorInputField").GetComponent<InputField>().onEndEdit.AddListener((string s) => SetBeatDivisorValueUnsafe(int.Parse(s)));

        #if CCE_DEBUG
        if(CurrentChart != null)
        {
            File.AppendAllText(LogPath, "PlayArea loaded...\n");
        }
        #endif
    }

    #if CCE_DEBUG
    private static string LogPath;
    #endif

    public void Awake()
    {
        if(CurrentChart != null)
        {
            #if CCE_DEBUG
            LogPath = Path.Combine(Application.persistentDataPath, "GameLogicLog.txt");
            File.WriteAllText(LogPath, "Starting loading the chart...\n");
            #endif

            CalculateTimings();

            #if CCE_DEBUG
            File.AppendAllText(LogPath, "Calculated timings...\n");
            #endif

            MusicManager.SetSource(MusicSource);
            for (int i = 0; i < HitsoundSources.Length; i++)
            {
                HitsoundSources[i].volume = Config.HitsoundVolume;
            }

            #if CCE_DEBUG
            File.AppendAllText(LogPath, "Audio sources loaded\n");
            #endif

            if (CurrentPageIndexOverride != -1)
            {
                CurrentPageIndex = CurrentPageIndexOverride;
                CurrentPageIndexOverride = -1;
            }
            else
            {
                CurrentPageIndex = 0;
            }

            UpdateTime(CurrentPage.start_time);

            #if CCE_DEBUG
            File.AppendAllText(LogPath, $"Moved to page {CurrentPageIndex}\n");
            #endif

            GameObject.Find("NoteCountText").GetComponent<Text>().text = $"Note count: {CurrentChart.note_list.Count}";
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
                    int tj = ti + 1; // Find the tempo the page ends at
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
                // The trick that makes the scanline start at the top/bottom breaks with this, but it's more important to make sure the page is editable properly
                if (pages[pi].start_tick < 0)
                {
                    pages[pi].start_tick = 0;
                    pages[pi].start_time = 0;
                }
                pi++;
            }
        }
        if(pages[p - 1].end_time < MusicManager.MaxTime) // Add pages in case the page_list ends before the music
        {
            double lasttempotime = CurrentChart.tempo_list.Last().value / 1000000.0; // Calculate time of the pages in respect to the last tempo
            while(pages[p - 1].end_time < MusicManager.MaxTime)
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
        if(pages[p - 1].start_time > MusicManager.MaxTime)
        {
            while (pages[p - 1].start_time > MusicManager.MaxTime)
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

        // Calculate note time, hold time and AR
        while(ti < t && ni < n)
        {
            if(ti + 1 < t && notes[ni].tick >= tempos[ti + 1].tick)
            {
                temposum += (double)tempos[ti].value * (tempos[ti + 1].tick - tempos[ti].tick) / timebase / 1000000;
                ti++;
            }
            else
            {
                notes[ni].opacity = notes[ni].opacity < 0 ? CurrentChart.opacity : notes[ni].opacity;

                notes[ni].time = temposum + (double)tempos[ti].value * (notes[ni].tick - tempos[ti].tick) / timebase / 1000000;

                notes[ni].y = pages[notes[ni].page_index].scan_line_direction == 1 ?
                    (double)(notes[ni].tick - pages[notes[ni].page_index].start_tick) / (pages[notes[ni].page_index].end_tick - pages[notes[ni].page_index].start_tick) :
                    1.0 - (double)(notes[ni].tick - pages[notes[ni].page_index].start_tick) / (pages[notes[ni].page_index].end_tick - pages[notes[ni].page_index].start_tick);

                notes[ni].approach_time = 1.0 / notes[ni].approach_rate;
                if(notes[ni].page_index == 0)
                {
                    notes[ni].approach_time *= 1.367;
                }
                else
                {
                    currPage = pages[notes[ni].page_index];
                    prevPage = pages[notes[ni].page_index - 1];
                    page_ratio = (double)(notes[ni].tick - currPage.start_tick) / (currPage.end_tick - currPage.start_tick);

                    notes[ni].approach_time *= 1.367 / Math.Max(1.0, 1.367 / ((currPage.end_time - currPage.start_time) * page_ratio + (prevPage.end_time - prevPage.start_time) * (1.367 - page_ratio)));
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
                }
                else
                {
                    notes[ni].hold_time = 0;
                }

                ni++;
            }
        }
    }

    private int GetDragParent(int id)
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
        int poz = 0; // determine the position to be inserted in
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
        GameObject obj;
        Color notecolor;
        switch(note.type)
        {
            case 0:
                obj = Instantiate(ClickNote);

                obj.transform.position = new Vector3((float)((note.x - 0.5) * PlayAreaWidth), (float)((note.y - 0.5) * PlayAreaHeight), (float)note.y);
                obj.transform.localScale = new Vector3(Config.DefaultNoteSize * (float)note.size, Config.DefaultNoteSize * (float)note.size);

                obj.GetComponent<ClickNoteController>().ApproachTime = (float)note.approach_time;

                ColorUtility.TryParseHtmlString(note.fill_color ??
                    CurrentChart.fill_colors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 0 : 1] ??
                    DefaultFillColors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 0 : 1], out notecolor);
                notecolor.a = (float)note.opacity / (loweropacity ? 3 : 1);
                obj.GetComponent<ClickNoteController>().ChangeNoteColor(notecolor);

                obj.GetComponent<ClickNoteController>().SetDelay((float)delay);

                obj.GetComponent<ClickNoteController>().PlaybackSpeed = PlaybackSpeeds[PlaybackSpeedIndex];

                obj.GetComponent<INote>().NoteType = (int)NoteType.CLICK;
                obj.GetComponent<INote>().NoteID = note.id;
                break;

            case 1:
                obj = Instantiate(HoldNote);

                obj.transform.position = new Vector3((float)((note.x - 0.5) * PlayAreaWidth), (float)((note.y - 0.5) * PlayAreaHeight), (float)note.y);
                obj.GetComponent<HoldNoteController>().SetSize(Config.DefaultNoteSize * (float)note.size);

                obj.GetComponent<HoldNoteController>().ApproachTime = (float)note.approach_time;

                ColorUtility.TryParseHtmlString(note.fill_color ??
                    CurrentChart.fill_colors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 0 : 1] ??
                    DefaultFillColors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 4 : 5], out notecolor);
                notecolor.a = (float)note.opacity / (loweropacity ? 3 : 1);
                obj.GetComponent<HoldNoteController>().ChangeNoteColor(notecolor);

                obj.GetComponent<HoldNoteController>().SetHeight((float)(PlayAreaHeight * note.hold_tick / CurrentChart.page_list[note.page_index].PageSize));
                obj.GetComponent<HoldNoteController>().HoldTime = (float)note.hold_time;

                if(CurrentChart.page_list[note.page_index].scan_line_direction == -1)
                {
                    obj.GetComponent<HoldNoteController>().Flip();
                }

                obj.GetComponent<HoldNoteController>().SetDelay((float)delay);

                obj.GetComponent<HoldNoteController>().PlaybackSpeed = PlaybackSpeeds[PlaybackSpeedIndex];

                obj.GetComponent<INote>().NoteType = (int)NoteType.HOLD;
                obj.GetComponent<INote>().NoteID = note.id;
                break;

            case 2:
                obj = Instantiate(LongHoldNote);

                obj.transform.position = new Vector3((float)((note.x - 0.5) * PlayAreaWidth), (float)((note.y - 0.5) * PlayAreaHeight), (float)note.y);
                obj.GetComponent<LongHoldNoteController>().SetSize(Config.DefaultNoteSize * (float)note.size);

                obj.GetComponent<LongHoldNoteController>().ApproachTime = (float)note.approach_time;

                ColorUtility.TryParseHtmlString(note.fill_color ??
                    CurrentChart.fill_colors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 0 : 1] ??
                    DefaultFillColors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 6 : 7], out notecolor);
                notecolor.a = (float)note.opacity / (loweropacity ? 3 : 1);
                obj.GetComponent<LongHoldNoteController>().ChangeNoteColor(notecolor);

                obj.GetComponent<LongHoldNoteController>().SetTopHeight((float)(1.0 - note.y) * PlayAreaHeight);
                obj.GetComponent<LongHoldNoteController>().SetBottomHeight((float)note.y * PlayAreaHeight);
                obj.GetComponent<LongHoldNoteController>().HoldTime = (float)note.hold_time;

                obj.GetComponent<LongHoldNoteController>().SetDelay((float)delay);

                obj.GetComponent<LongHoldNoteController>().PlaybackSpeed = PlaybackSpeeds[PlaybackSpeedIndex];

                obj.GetComponent<LongHoldNoteController>().FinishIndicator.transform.position = new Vector3(obj.transform.position.x, CurrentPage.scan_line_direction *
                    (PlayAreaHeight * (note.tick + note.hold_tick - CurrentPage.start_tick) / (int)CurrentPage.PageSize - PlayAreaHeight / 2));
                
                obj.GetComponent<INote>().NoteType = (int)NoteType.LONG_HOLD;
                obj.GetComponent<INote>().NoteID = note.id;
                break;

            case 3:
                obj = Instantiate(DragHeadNote);

                obj.transform.position = new Vector3((float)((note.x - 0.5) * PlayAreaWidth), (float)((note.y - 0.5) * PlayAreaHeight), (float)note.y);
                obj.transform.localScale = new Vector3(Config.DefaultNoteSize * (float)note.size, Config.DefaultNoteSize * (float)note.size);

                obj.GetComponent<DragHeadNoteController>().ApproachTime = (float)note.approach_time;

                ColorUtility.TryParseHtmlString(note.fill_color ??
                    CurrentChart.fill_colors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 0 : 1] ??
                    DefaultFillColors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 2 : 3], out notecolor);
                notecolor.a = (float)note.opacity / (loweropacity ? 3 : 1);
                obj.GetComponent<DragHeadNoteController>().ChangeNoteColor(notecolor);

                obj.GetComponent<DragHeadNoteController>().NextID = note.next_id;
                obj.GetComponent<DragHeadNoteController>().StartTime = (float)note.time;

                obj.GetComponent<DragHeadNoteController>().SetDelay((float)delay);

                obj.GetComponent<DragHeadNoteController>().PlaybackSpeed = PlaybackSpeeds[PlaybackSpeedIndex];

                obj.GetComponent<INote>().NoteType = (int)NoteType.DRAG_HEAD;
                obj.GetComponent<INote>().NoteID = note.id;
                break;

            case 4:
                obj = Instantiate(DragChildNote);

                obj.transform.position = new Vector3((float)((note.x - 0.5) * PlayAreaWidth), (float)((note.y - 0.5) * PlayAreaHeight), (float)note.y);
                obj.transform.localScale = new Vector3(Config.DefaultNoteSize * (float)note.size, Config.DefaultNoteSize * (float)note.size);

                obj.GetComponent<DragChildNoteController>().ApproachTime = (float)note.approach_time;

                ColorUtility.TryParseHtmlString(note.fill_color ??
                    CurrentChart.fill_colors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 0 : 1] ??
                    DefaultFillColors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 2 : 3], out notecolor);
                notecolor.a = (float)note.opacity / (loweropacity ? 3 : 1);
                obj.GetComponent<DragChildNoteController>().ChangeNoteColor(notecolor);

                obj.GetComponent<DragChildNoteController>().SetDelay((float)delay);

                obj.GetComponent<DragChildNoteController>().PlaybackSpeed = PlaybackSpeeds[PlaybackSpeedIndex];

                obj.GetComponent<INote>().NoteType = (int)NoteType.DRAG_CHILD;
                obj.GetComponent<INote>().NoteID = note.id;
                break;

            case 5:
                obj = Instantiate(FlickNote);

                obj.transform.position = new Vector3((float)((note.x - 0.5) * PlayAreaWidth), (float)((note.y - 0.5) * PlayAreaHeight), (float)note.y);
                obj.transform.localScale = new Vector3(Config.DefaultNoteSize * (float)note.size * 0.8f, Config.DefaultNoteSize * (float)note.size * 0.8f);

                obj.GetComponent<ClickNoteController>().ApproachTime = (float)note.approach_time;

                ColorUtility.TryParseHtmlString(note.fill_color ??
                    CurrentChart.fill_colors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 0 : 1] ??
                    DefaultFillColors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 10 : 11], out notecolor);
                notecolor.a = (float)note.opacity / (loweropacity ? 3 : 1);
                obj.GetComponent<ClickNoteController>().ChangeNoteColor(notecolor);

                obj.GetComponent<ClickNoteController>().SetDelay((float)delay);

                obj.GetComponent<ClickNoteController>().PlaybackSpeed = PlaybackSpeeds[PlaybackSpeedIndex];

                obj.GetComponent<INote>().NoteType = (int)NoteType.FLICK;
                obj.GetComponent<INote>().NoteID = note.id;
                break;

            case 6:
                obj = Instantiate(CDragHeadNote);

                obj.transform.position = new Vector3((float)((note.x - 0.5) * PlayAreaWidth), (float)((note.y - 0.5) * PlayAreaHeight), (float)note.y);
                obj.transform.localScale = new Vector3(Config.DefaultNoteSize * (float)note.size, Config.DefaultNoteSize * (float)note.size);

                obj.GetComponent<DragHeadNoteController>().ApproachTime = (float)note.approach_time;

                ColorUtility.TryParseHtmlString(note.fill_color ??
                    CurrentChart.fill_colors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 0 : 1] ??
                    DefaultFillColors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 10 : 11], out notecolor);
                notecolor.a = (float)note.opacity / (loweropacity ? 3 : 1);
                obj.GetComponent<DragHeadNoteController>().ChangeNoteColor(notecolor);

                obj.GetComponent<DragHeadNoteController>().NextID = note.next_id;
                obj.GetComponent<DragHeadNoteController>().StartTime = (float)note.time;

                obj.GetComponent<DragHeadNoteController>().SetDelay((float)delay);

                obj.GetComponent<DragHeadNoteController>().PlaybackSpeed = PlaybackSpeeds[PlaybackSpeedIndex];

                obj.GetComponent<INote>().NoteType = (int)NoteType.CDRAG_HEAD;
                obj.GetComponent<INote>().NoteID = note.id;
                break;

            case 7:
                obj = Instantiate(DragChildNote);

                obj.transform.position = new Vector3((float)((note.x - 0.5) * PlayAreaWidth), (float)((note.y - 0.5) * PlayAreaHeight), (float)note.y);
                obj.transform.localScale = new Vector3(Config.DefaultNoteSize * (float)note.size, Config.DefaultNoteSize * (float)note.size);

                obj.GetComponent<DragChildNoteController>().ApproachTime = (float)note.approach_time;

                ColorUtility.TryParseHtmlString(note.fill_color ??
                    CurrentChart.fill_colors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 0 : 1] ??
                    DefaultFillColors[CurrentChart.page_list[note.page_index].scan_line_direction == 1 ? 10 : 11], out notecolor);
                notecolor.a = (float)note.opacity / (loweropacity ? 3 : 1);
                obj.GetComponent<DragChildNoteController>().ChangeNoteColor(notecolor);

                obj.GetComponent<DragChildNoteController>().SetDelay((float)delay);

                obj.GetComponent<DragChildNoteController>().PlaybackSpeed = PlaybackSpeeds[PlaybackSpeedIndex];

                obj.GetComponent<INote>().NoteType = (int)NoteType.CDRAG_CHILD;
                obj.GetComponent<INote>().NoteID = note.id;
                break;
        };
    }

    /// <summary>
    /// Spawn a scanline/tempo note from the <see cref="Tempo"/> with the specified id.
    /// </summary>
    /// <param name="id"> The id of the <see cref="Tempo"/> this note represents. </param>
    private void SpawnScanlineNote(int id)
    {
        GameObject obj = Instantiate(ScanlineNote);

        obj.GetComponent<ScanlineNoteController>().NoteID = id;

        obj.GetComponent<ScanlineNoteController>().SetPosition(new Vector3(-PlayAreaWidth / 2 - 1, -PlayAreaHeight / 2 + PlayAreaHeight * (float)(CurrentPage.scan_line_direction == 1 ?
            (CurrentChart.tempo_list[id].tick - CurrentPage.start_tick) / CurrentPage.PageSize :
            1.0 - (CurrentChart.tempo_list[id].tick - CurrentPage.start_tick) / CurrentPage.PageSize)));

        obj.GetComponent<ScanlineNoteController>().TimeInputField.text = (CurrentChart.tempo_list[id].time - CurrentChart.music_offset).ToString();
        obj.GetComponent<ScanlineNoteController>().BPMInputField.text = (Math.Round(120000000.0 / CurrentChart.tempo_list[id].value, 2)).ToString();
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
        CurrentChart.tempo_list.Insert(poz, tempo);
    }

    /// <summary>
    /// Removes the tempo with the specified id.
    /// </summary>
    /// <param name="id"> The id of the tempo to be removed. </param>
    private void RemoveTempo(int id)
    {
        CurrentChart.tempo_list.RemoveAt(id);
    }

    /// <summary>
    /// Schedules the start or pauses the current chart.
    /// </summary>
    public void PlayPause()
    {
        if (CurrentChart == null)
        {
            return;
        }
        if (IsGameRunning || IsStartScheduled)
        {
            MusicManager.Pause();
            IsGameRunning = false;
            IsStartScheduled = false;

            if (CurrentPageIndex < CurrentChart.page_list.Count)
            {
                UpdateTime(CurrentPage.start_time);
            }
            else
            {
                CurrentPageIndex = CurrentChart.page_list.Count - 1;
                UpdateTime(CurrentPage.start_time);
            }
        }
        else
        {
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
                    obj.GetComponent<IHighlightable>().Highlight();
                }
            }
        }
    }

    /// <summary>
    /// The value of the divisor slider.
    /// </summary>
    public static int DivisorValue = 8;
    private readonly int[] AllowedDivisor = {1, 2, 3, 4, 6, 8, 12, 16 };

    public void BeatDivisorValueChanged()
    {
        for(int i = 0; i < 8; i++)
        {
            if(AllowedDivisor[i] <= (int)BeatDivisor.value)
            {
                DivisorValue = AllowedDivisor[i];
            }
        }
        foreach(var obj in GameObject.FindGameObjectsWithTag("DivisorLine"))
        {
            Destroy(obj);
        }
        for(int i = 1; i < DivisorValue; i++)
        {
            GameObject obj = Instantiate(DivisorLine);
            obj.transform.position = new Vector3(0, PlayAreaHeight / DivisorValue * i - PlayAreaHeight / 2);
            obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaWidth, 0.1f);
        }

        GameObject.Find("BeatDivisorInputField").GetComponent<InputField>().text = DivisorValue.ToString();
    }

    public void SetBeatDivisorValueUnsafe(int val)
    {
        val = Clamp<int>(val, 1, 16);
        BeatDivisor.SetValueWithoutNotify(val);
        DivisorValue = val;
        foreach (var obj in GameObject.FindGameObjectsWithTag("DivisorLine"))
        {
            Destroy(obj);
        }
        for (int i = 1; i < DivisorValue; i++)
        {
            GameObject obj = Instantiate(DivisorLine);
            obj.transform.position = new Vector3(0, PlayAreaHeight / DivisorValue * i - PlayAreaHeight / 2);
            obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaWidth, 0.1f);
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

            time = CurrentPage.start_time;

            UpdateTime(time);
        }
    }

    /// <summary>
    /// Updates the current time to the <paramref name="time"/> specified.
    /// </summary>
    /// <param name="time"> The specified time the chart should be at. </param>
    private void UpdateTime(double time)
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("Note"))
        {
            Destroy(obj);
        }

        foreach (var obj in GameObject.FindGameObjectsWithTag("Connector"))
        {
            Destroy(obj);
        }

        foreach (var obj in GameObject.FindGameObjectsWithTag("ScanlineNote"))
        {
            Destroy(obj);
        }

        CurrentHitsoundIndex = 0;
        CurrentHoldHitsoundIndex = 0;

        CurrentNoteIndex = 0;

        // Can be optimized if necessary with binary search algorithms and a segment tree for the holds, albeit very tricky to implement properly. The last resort if optimization is necessary.
        for (int i = 0; i < CurrentChart.note_list.Count; i++)
        {
            if (CurrentChart.note_list[i].time - (Config.ShowApproachingNotesWhilePaused || CurrentChart.note_list[i].page_index == CurrentPageIndex ? CurrentChart.note_list[i].approach_time : 0) <= time && CurrentChart.note_list[i].time + CurrentChart.note_list[i].hold_time >= time
                || ((CurrentChart.note_list[i].type == (int)NoteType.DRAG_HEAD || CurrentChart.note_list[i].type == (int)NoteType.CDRAG_HEAD) && GetLastChild(i).time >= time && CurrentChart.note_list[i].time <= time))
            {
                SpawnNote(CurrentChart.note_list[i], time - CurrentChart.note_list[i].time + CurrentChart.note_list[i].approach_time);
            }
            else if (CurrentChart.note_list[i].page_index == CurrentPageIndex)
            {
                SpawnNote(CurrentChart.note_list[i], 10000);
            }
            else if (CurrentChart.note_list[i].page_index + 1 == CurrentPageIndex)
            {
                SpawnNote(CurrentChart.note_list[i], 10000, true);
            }
            else if(CurrentChart.note_list[i].time - (Config.ShowApproachingNotesWhilePaused || CurrentChart.note_list[i].page_index == CurrentPageIndex ? CurrentChart.note_list[i].approach_time : 0) <= time)
            {
                CurrentNoteIndex = i + 1;
            }

            if(CurrentChart.note_list[i].time < time)
            {
                CurrentHitsoundIndex = i + 1;
            }

            if((CurrentChart.note_list[i].type == (int)NoteType.HOLD || CurrentChart.note_list[i].type == (int)NoteType.LONG_HOLD) && CurrentChart.note_list[i].time + CurrentChart.note_list[i].hold_time < time)
            {
                CurrentHoldHitsoundIndex = i + 1;
            }
        }

        for(int i = 0; i < CurrentChart.tempo_list.Count; i++)
        {
            if(CurrentChart.tempo_list[i].tick <= CurrentPage.end_tick && CurrentChart.tempo_list[i].tick >= CurrentPage.start_tick)
            {
                SpawnScanlineNote(i);
            }
            if (CurrentChart.tempo_list[i].time <= time)
            {
                CurrentTempoIndex = i;
            }
        }

        GameObject.Find("SweepChangeButton").GetComponentInChildren<Text>().text = CurrentPage.scan_line_direction == 1 ? "Up" : "Down";

        GameObject.Find("TimeText").GetComponent<Text>().text = "Page: " + CurrentPageIndex.ToString() + "\nTime: " + ((int)((time - CurrentChart.music_offset) / 60)).ToString() + ":" + ((int)(time - CurrentChart.music_offset) % 60).ToString("D2") + "." + ((int)((time - CurrentChart.music_offset) * 1000 - Math.Floor(time - CurrentChart.music_offset) * 1000)).ToString("D3");

        Scanline.transform.position = new Vector3(0, CurrentPage.scan_line_direction == 1
            ? PlayAreaHeight * (float)((time - CurrentPage.start_time) /
            (CurrentPage.end_time - CurrentPage.start_time) - 0.5f)
            : PlayAreaHeight * (0.5f - (float)((time - CurrentPage.start_time) /
            (CurrentPage.end_time - CurrentPage.start_time))));

        MusicManager.SetTime(time - Offset);

        Timeline.SetValueWithoutNotify((float)(MusicManager.Time / MusicManager.MaxTime) * PlaybackSpeeds[PlaybackSpeedIndex]);
    }

    private Note GetLastChild(int parent)
    {
        while(CurrentChart.note_list[parent].next_id > 0)
        {
            parent = CurrentChart.note_list[parent].next_id;
        }
        return CurrentChart.note_list[parent];
    }

    private bool isTouchHeld = false;
    private GameObject currentlymoving;

    private void Update()
    {
        if (IsStartScheduled)
        {
            if(AudioSettings.dspTime > ScheduledTime)
            {
                IsGameRunning = true;
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

            Timeline.SetValueWithoutNotify((float)(MusicManager.Time / MusicManager.MaxTime) * PlaybackSpeeds[PlaybackSpeedIndex]);
            double time = MusicManager.Time + Offset;

            while(CurrentHitsoundIndex < CurrentChart.note_list.Count && CurrentChart.note_list[CurrentHitsoundIndex].time - 0.05 <= time)
            {
                for(int i = 0; i < HitsoundSources.Length; i++)
                {
                    if(HitsoundScheduledTimes[i] < AudioSettings.dspTime)
                    {
                        HitsoundSources[i].PlayScheduled(CurrentChart.note_list[CurrentHitsoundIndex].time - time + AudioSettings.dspTime);
                        HitsoundScheduledTimes[i] = CurrentChart.note_list[CurrentHitsoundIndex].time - time + AudioSettings.dspTime + HitsoundSources[i].clip.length;
                        break;
                    }
                }
                CurrentHitsoundIndex++;
            }

            if(Config.PlayHitsoundsOnHoldEnd)
            {
                while (CurrentHoldHitsoundIndex < CurrentChart.note_list.Count && CurrentChart.note_list[CurrentHoldHitsoundIndex].time + CurrentChart.note_list[CurrentHoldHitsoundIndex].hold_time - 0.05 <= time)
                {
                    if (CurrentChart.note_list[CurrentHoldHitsoundIndex].type == (int)NoteType.HOLD || CurrentChart.note_list[CurrentHoldHitsoundIndex].type == (int)NoteType.LONG_HOLD)
                    {
                        for (int i = 0; i < HitsoundSources.Length; i++)
                        {
                            if (HitsoundScheduledTimes[i] < AudioSettings.dspTime)
                            {
                                HitsoundSources[i].PlayScheduled((CurrentChart.note_list[CurrentHoldHitsoundIndex].time + CurrentChart.note_list[CurrentHoldHitsoundIndex].hold_time - time) * 1.0 / PlaybackSpeeds[PlaybackSpeedIndex] + AudioSettings.dspTime);
                                HitsoundScheduledTimes[i] = (CurrentChart.note_list[CurrentHoldHitsoundIndex].time + CurrentChart.note_list[CurrentHoldHitsoundIndex].hold_time - time) * 1.0 / PlaybackSpeeds[PlaybackSpeedIndex] + AudioSettings.dspTime + HitsoundSources[i].clip.length;
                                break;
                            }
                        }
                    }
                    CurrentHoldHitsoundIndex++;
                }
            }

            while(CurrentNoteIndex < CurrentChart.note_list.Count &&  CurrentChart.note_list[CurrentNoteIndex].time - CurrentChart.note_list[CurrentNoteIndex].approach_time <= time)
            {
                SpawnNote(CurrentChart.note_list[CurrentNoteIndex], time - CurrentChart.note_list[CurrentNoteIndex].time + CurrentChart.note_list[CurrentNoteIndex].approach_time);
                CurrentNoteIndex++;
            }
            while(CurrentPageIndex < CurrentChart.page_list.Count && CurrentPage.end_time < time)
            {
                CurrentPageIndex++;
            }
            while(CurrentTempoIndex + 1 < CurrentChart.tempo_list.Count && CurrentChart.tempo_list[CurrentTempoIndex + 1].time < time)
            {
                CurrentTempoIndex++;
            }

            GameObject.Find("TimeText").GetComponent<Text>().text = "Page: " + CurrentPageIndex.ToString() + "\nTime: " + ((int)((time - CurrentChart.music_offset) / 60)).ToString() + ":" + ((int)(time - CurrentChart.music_offset) % 60).ToString("D2") + "." + ((int)((time - CurrentChart.music_offset) * 1000 - Math.Floor(time - CurrentChart.music_offset) * 1000)).ToString("D3");

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
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0)) // Handle input
        {
            Vector2 touchpos = MainCamera.ScreenToWorldPoint(Input.mousePosition);

            isTouchHeld = true;

            if (CurrentTool == NoteType.NONE)
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (obj.GetComponentInChildren<CircleCollider2D>().OverlapPoint(touchpos)) // Highlighting notes
                    {
                        obj.GetComponent<IHighlightable>().Highlight();
                        if (obj.GetComponent<LongHoldNoteController>() != null)
                        {
                            Note note = CurrentChart.note_list[obj.GetComponent<INote>().NoteID];
                            obj.GetComponent<LongHoldNoteController>().FinishIndicator.transform.position = new Vector3(obj.transform.position.x, CurrentPage.scan_line_direction *
                                (PlayAreaHeight * (note.tick + note.hold_tick - CurrentPage.start_tick) / (int)CurrentPage.PageSize - PlayAreaHeight / 2));
                        }
                    }
                    if (obj.GetComponent<HoldNoteController>() != null) // Modifying hold_time for short holds
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
                        }
                        else if (holdcontroller.DownArrowCollider.OverlapPoint(touchpos))
                        {
                            CurrentChart.note_list[id].hold_tick -= CurrentChart.time_base / DivisorValue;
                            if (CurrentChart.note_list[id].hold_tick < 0)
                            {
                                CurrentChart.note_list[id].hold_tick = 1;
                            }
                        }
                        else
                        {
                            continue;
                        }
                        CalculateTimings();
                        UpdateTime(CurrentPage.start_time);
                        HighlightNoteWithID(id);
                    }
                    else if (obj.GetComponent<LongHoldNoteController>() != null) // Modifying hold_time for long holds
                    {
                        var holdcontroller = obj.GetComponent<LongHoldNoteController>();
                        int id = holdcontroller.NoteID;
                        if (holdcontroller.UpArrowCollider.OverlapPoint(touchpos))
                        {
                            CurrentChart.note_list[id].hold_tick += CurrentChart.time_base / DivisorValue;
                        }
                        else if (holdcontroller.DownArrowCollider.OverlapPoint(touchpos))
                        {
                            CurrentChart.note_list[id].hold_tick -= CurrentChart.time_base / DivisorValue;

                            if (CurrentChart.note_list[id].hold_tick < 0)
                            {
                                CurrentChart.note_list[id].hold_tick = 1;
                            }
                        }
                        else
                        {
                            continue;
                        }
                        CalculateTimings();
                        UpdateTime(CurrentPage.start_time);
                        foreach (var obj2 in GameObject.FindGameObjectsWithTag("Note"))
                        {
                            if (obj2.GetComponent<INote>().NoteID == id)
                            {
                                obj2.GetComponent<IHighlightable>().Highlight();
                            }
                        }
                    }
                }
            }
            else if (CurrentTool == NoteType.MOVE) // Starting the move of notes
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (obj.GetComponentInChildren<Collider2D>().OverlapPoint(touchpos))
                    {
                        currentlymoving = obj;
                    }
                }
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("ScanlineNote"))
                {
                    if (obj.GetComponent<INote>().NoteID != 0 && obj.GetComponentInChildren<Collider2D>().OverlapPoint(touchpos))
                    {
                        currentlymoving = obj;
                        break;
                    }
                }
            }
            else if (CurrentChart != null && touchpos.x < PlayAreaWidth / 2 && touchpos.x > -PlayAreaWidth / 2 && touchpos.y < PlayAreaHeight / 2 && touchpos.y > -PlayAreaHeight / 2)
            // Adding notes
            {
                if (CurrentTool == NoteType.CLICK) // Add click note
                {
                    AddNote(new Note
                    {
                        x = Math.Round((touchpos.x + PlayAreaWidth / 2) / (PlayAreaWidth / Config.VerticalDivisors)) / Config.VerticalDivisors,
                        page_index = CurrentPageIndex,
                        type = (int)NoteType.CLICK,
                        id = -1,
                        hold_tick = 0,
                        next_id = 0,
                        tick = (int)(CurrentPage.start_tick + CurrentPage.PageSize *
                            (CurrentPage.scan_line_direction == 1 ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                            : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue))
                    });

                    CalculateTimings();
                    UpdateTime(CurrentPage.start_time);
                }
                else if (CurrentTool == NoteType.HOLD) // Add hold note
                {
                    int tick = (int)(CurrentPage.start_tick + CurrentPage.PageSize * (CurrentPage.scan_line_direction == 1
                        ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                        : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue));

                    AddNote(new Note
                    {
                        x = Math.Round((touchpos.x + PlayAreaWidth / 2) / (PlayAreaWidth / Config.VerticalDivisors)) / Config.VerticalDivisors,
                        page_index = tick == CurrentPage.end_tick ? CurrentPageIndex + 1 : CurrentPageIndex,
                        type = (int)NoteType.HOLD,
                        id = -1,
                        hold_tick = CurrentChart.time_base / DivisorValue,
                        next_id = 0,
                        tick = tick
                    });

                    CalculateTimings();
                    UpdateTime(CurrentPage.start_time);
                }
                else if (CurrentTool == NoteType.LONG_HOLD) // Add long hold note
                {
                    int tick = (int)(CurrentPage.start_tick + CurrentPage.PageSize * (CurrentPage.scan_line_direction == 1
                        ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                        : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue));

                    AddNote(new Note
                    {
                        x = Math.Round((touchpos.x + PlayAreaWidth / 2) / (PlayAreaWidth / Config.VerticalDivisors)) / Config.VerticalDivisors,
                        page_index = tick == CurrentPage.end_tick ? CurrentPageIndex + 1 : CurrentPageIndex,
                        type = (int)NoteType.LONG_HOLD,
                        id = -1,
                        hold_tick = CurrentChart.time_base / DivisorValue,
                        next_id = 0,
                        tick = tick
                    });

                    CalculateTimings();
                    UpdateTime(CurrentPage.start_time);
                }
                else if (CurrentTool == NoteType.FLICK) // Add flick note
                {
                    AddNote(new Note
                    {
                        x = Math.Round((touchpos.x + PlayAreaWidth / 2) / (PlayAreaWidth / Config.VerticalDivisors)) / Config.VerticalDivisors,
                        page_index = CurrentPageIndex,
                        type = (int)NoteType.FLICK,
                        id = -1,
                        hold_tick = 0,
                        next_id = 0,
                        tick = (int)(CurrentPage.start_tick + CurrentPage.PageSize *
                            (CurrentPage.scan_line_direction == 1 ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                            : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue))
                    });

                    CalculateTimings();
                    UpdateTime(CurrentPage.start_time);
                }
                else if (CurrentTool == NoteType.DRAG_HEAD) // Add drag head and child
                {
                    bool existsHighlightedDragHead = false;
                    int IDtoHighlight = -1;
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                    {
                        if (obj.GetComponent<IHighlightable>().Highlighted && CurrentChart.note_list[obj.GetComponent<INote>().NoteID].next_id == -1 && (obj.GetComponent<DragHeadNoteController>() != null || obj.GetComponent<DragChildNoteController>() != null))
                        // Add drag child
                        {
                            int tick = (int)(CurrentPage.start_tick + CurrentPage.PageSize *
                                    (CurrentPage.scan_line_direction == 1 ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                                    : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue));

                            if (CurrentChart.note_list[obj.GetComponent<INote>().NoteID].tick < tick)
                            {
                                int id = AddNote(new Note
                                {
                                    x = Math.Round((touchpos.x + PlayAreaWidth / 2) / (PlayAreaWidth / Config.VerticalDivisors)) / Config.VerticalDivisors,
                                    page_index = CurrentPageIndex,
                                    type = (int)NoteType.DRAG_CHILD,
                                    id = -1,
                                    hold_tick = 0,
                                    next_id = -1,
                                    tick = tick
                                });
                                IDtoHighlight = id;
                                CurrentChart.note_list[obj.GetComponent<INote>().NoteID].next_id = id;
                            }
                            existsHighlightedDragHead = true;
                            break;
                        }
                    }

                    if (!existsHighlightedDragHead) // Add drag head
                    {
                        IDtoHighlight = AddNote(new Note
                        {
                            x = Math.Round((touchpos.x + PlayAreaWidth / 2) / (PlayAreaWidth / Config.VerticalDivisors)) / Config.VerticalDivisors,
                            page_index = CurrentPageIndex,
                            type = (int)NoteType.DRAG_HEAD,
                            id = -1,
                            hold_tick = 0,
                            next_id = -1,
                            tick = (int)(CurrentPage.start_tick + CurrentPage.PageSize *
                            (CurrentPage.scan_line_direction == 1 ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                            : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue))
                        });
                    }

                    CalculateTimings();
                    UpdateTime(CurrentPage.start_time);
                    HighlightNoteWithID(IDtoHighlight);
                }
                else if (CurrentTool == NoteType.CDRAG_HEAD) // Add cdrag head and child
                {
                    bool existsHighlightedDragHead = false;
                    int IDtoHighlight = -1;
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                    {
                        if (obj.GetComponent<IHighlightable>().Highlighted && CurrentChart.note_list[obj.GetComponent<INote>().NoteID].next_id == -1 && (obj.GetComponent<DragHeadNoteController>() != null || obj.GetComponent<DragChildNoteController>() != null))
                        // Add drag child
                        {
                            int tick = (int)(CurrentPage.start_tick + CurrentPage.PageSize *
                                    (CurrentPage.scan_line_direction == 1 ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                                    : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue));

                            if (CurrentChart.note_list[obj.GetComponent<INote>().NoteID].tick < tick)
                            {
                                int id = AddNote(new Note
                                {
                                    x = Math.Round((touchpos.x + PlayAreaWidth / 2) / (PlayAreaWidth / Config.VerticalDivisors)) / Config.VerticalDivisors,
                                    page_index = CurrentPageIndex,
                                    type = (int)NoteType.CDRAG_CHILD,
                                    id = -1,
                                    hold_tick = 0,
                                    next_id = -1,
                                    tick = tick
                                });
                                IDtoHighlight = id;
                                CurrentChart.note_list[obj.GetComponent<INote>().NoteID].next_id = id;
                            }
                            existsHighlightedDragHead = true;
                            break;
                        }
                    }

                    if (!existsHighlightedDragHead) // Add drag head
                    {
                        IDtoHighlight = AddNote(new Note
                        {
                            x = Math.Round((touchpos.x + PlayAreaWidth / 2) / (PlayAreaWidth / Config.VerticalDivisors)) / Config.VerticalDivisors,
                            page_index = CurrentPageIndex,
                            type = (int)NoteType.CDRAG_HEAD,
                            id = -1,
                            hold_tick = 0,
                            next_id = -1,
                            tick = (int)(CurrentPage.start_tick + CurrentPage.PageSize *
                            (CurrentPage.scan_line_direction == 1 ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                            : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue))
                        });
                    }

                    CalculateTimings();
                    UpdateTime(CurrentPage.start_time);
                    HighlightNoteWithID(IDtoHighlight);
                }
                else if (CurrentTool == NoteType.SETTINGS) // Add scanline/tempo note
                {
                    AddTempo(new Tempo
                    {
                        tick = (int)(CurrentPage.start_tick + CurrentPage.PageSize *
                            (CurrentPage.scan_line_direction == 1 ? Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue
                            : 1.0f - Math.Round((touchpos.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) / DivisorValue)),
                        value = 1000000,
                    });

                    CalculateTimings();
                    UpdateTime(CurrentPage.start_time);
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
                        RemoveTempo(currentlymoving.GetComponent<INote>().NoteID);
                    }
                    else
                    {
                        RemoveNote(currentlymoving.GetComponent<INote>().NoteID);
                    }
                    Destroy(currentlymoving);
                    UpdateTime(CurrentPage.start_time);
                }
                else
                {
                    currentlymoving.transform.position = new Vector3(Clamp(currentlymoving.transform.position.x, -PlayAreaWidth / 2, PlayAreaWidth / 2),
                        Clamp(currentlymoving.transform.position.y, -PlayAreaHeight / 2, PlayAreaHeight / 2));
                    if (currentlymoving.CompareTag("Note"))
                    {
                        currentlymoving.transform.position = new Vector3(
                            (float)Math.Round((currentlymoving.transform.position.x + PlayAreaWidth / 2) / (PlayAreaWidth / Config.VerticalDivisors)) * (PlayAreaWidth / Config.VerticalDivisors) - PlayAreaWidth / 2,
                            (float)Math.Round((currentlymoving.transform.position.y + PlayAreaHeight / 2) / (PlayAreaHeight / DivisorValue)) * (PlayAreaHeight / DivisorValue) - PlayAreaHeight / 2);

                        int id = currentlymoving.GetComponent<INote>().NoteID;
                        Note note = CurrentChart.note_list[id];
                        note.x = (currentlymoving.transform.position.x + PlayAreaWidth / 2) / PlayAreaWidth;
                        int tick = (int)Math.Round(CurrentChart.page_list[note.page_index].start_tick + CurrentChart.page_list[note.page_index].PageSize *
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

                        int id = currentlymoving.GetComponent<INote>().NoteID;
                        Tempo tempo = CurrentChart.tempo_list[id];
                        tempo.tick = (int)Math.Round((CurrentPage.scan_line_direction == 1 ? (currentlymoving.transform.position.y + PlayAreaHeight / 2) / PlayAreaHeight :
                            1.0 - (currentlymoving.transform.position.y + PlayAreaHeight / 2) / PlayAreaHeight) * CurrentPage.PageSize) + CurrentPage.start_tick;

                        RemoveTempo(id);
                        AddTempo(tempo);
                    }

                    CalculateTimings();

                    UpdateTime(CurrentPage.start_time);

                    currentlymoving = null;
                }
            }
            isTouchHeld = false;
        }

        if (isTouchHeld && CurrentTool == NoteType.MOVE && currentlymoving != null) // Handle moving notes
        {
            Vector2 touchpos = MainCamera.ScreenToWorldPoint(Input.mousePosition);

            currentlymoving.transform.position = touchpos;
        }
    }

    private static void HighlightNoteWithID(int id)
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("Note"))
        {
            if (obj.GetComponent<INote>().NoteID == id)
            {
                obj.GetComponent<IHighlightable>().Highlight();
            }
        }
    }

    public void GoToPreviousPage()
    {
        if(!IsGameRunning && CurrentChart != null && CurrentPageIndex > 0)
        {
            CurrentPageIndex--;
            UpdateTime(CurrentPage.start_time);
        }
    }

    public void GoToNextPage()
    {
        if (!IsGameRunning && CurrentChart != null && CurrentPageIndex < CurrentChart.page_list.Count)
        {
            CurrentPageIndex++;
            UpdateTime(CurrentPage.start_time);
        }
    }

    public void ChangeTempo(GameObject scanlineNote)
    {
        string bpminput = scanlineNote.GetComponent<ScanlineNoteController>().BPMInputField.text, timeinput = scanlineNote.GetComponent<ScanlineNoteController>().TimeInputField.text;
        int id = scanlineNote.GetComponent<INote>().NoteID;
        if(double.TryParse(bpminput, out double bpm))
        {
            CurrentChart.tempo_list[id].value = (int)Math.Round(120000000 / bpm);
        }
        if(double.TryParse(timeinput, out double time) && id == 0)
        {
            CurrentChart.music_offset = time;
        }
        CalculateTimings();
        UpdateTime(CurrentPage.start_time);
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
        UpdateTime(CurrentPage.start_time);
        GameObject.Find("PlaybackSpeedText").GetComponent<Text>().text = $"{(int)(PlaybackSpeeds[PlaybackSpeedIndex] * 100)}%";
    }

    public void IncreaseOffset()
    {
        Config.UserOffset += Config.PreciseOffsetDelta ? 1 : 5;
        UpdateOffsetText();
    }

    public void DecreaseOffset()
    {
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
            CurrentPage.scan_line_direction = -CurrentPage.scan_line_direction;
            GameObject.Find("SweepChangeButton").GetComponentInChildren<Text>().text = CurrentPage.scan_line_direction == 1 ? "Up" : "Down";
            CalculateTimings();
            UpdateTime(CurrentPage.start_time);
        }
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

            while(CurrentChart.note_list[CurrentChart.note_list.Count - 1].page_index >= CurrentChart.page_list.Count)
            {
                CurrentChart.note_list.RemoveAt(CurrentChart.note_list.Count - 1);
            }

            File.WriteAllText(Path.Combine(CurrentLevelPath, CurrentChart.Data.path), JsonConvert.SerializeObject(CurrentChart, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            }));

            LevelDataChanger.SaveLevel();

            GameObject.Find("ToastText").GetComponent<ToastMessageManager>().CreateToast("Saved chart!");
        }
    }
}
