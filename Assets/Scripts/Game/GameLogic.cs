using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CCE.Commands;
using CCE.Core;
using CCE.Data;
using CCE.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static CCE.Core.GlobalState;

namespace CCE.Game
{
    public class GameLogic : MonoBehaviour
    {
        // TODO: break up this monster class into pieces
        private static GameLogic _instance;

        public static NoteType CurrentTool = NoteType.None;

        public static bool BlockInput;

        private static int _currentPageIndexOverride = -1;

        private static int _playbackSpeedIndex = 3;

        private static string _logPath;

        private static int _beatDivisorValue = 8;

        public AudioMixerGroup HalfSpeedMixer, ThreeQuarterSpeedMixer;

        [HideInInspector] public int CurrentPageIndex;

        private readonly int[] _allowedDivisors = {1, 2, 3, 4, 6, 8, 12, 16};
        private readonly Dictionary<int, bool> _isObjectMovingDict = new Dictionary<int, bool>();
        private readonly List<MovingNote> _movingNotes = new List<MovingNote>();
        private readonly float[] _playbackSpeeds = {0.25f, 0.5f, 0.75f, 1.0f};

        private readonly StringBuilder _timeTextBuilder = new StringBuilder(32);

        private int _currentDragID;

        private int _currentHitsoundIndex;
        private GameObject _currentlyMovingObject;
        private int _currentNoteIndex;
        private int _currentTempoIndex;

        private bool _isStartScheduled;

        private bool _isTouchHeld;

        private Rect _lastSafeArea = new Rect(0, 0, Screen.width, Screen.height);
        private float _lockedY;

        private bool _lockY;
        private GameObject _lockYText;

        private ChartObjectPool _objectPool;

        private double _saveEditorOffsetScheduledTime = -1;
        private Vector2 _startMovePos;

        [SerializeField]
        private LineRenderer utilityLineRenderer;

        public static GameLogic Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<GameLogic>();
                return _instance;
            }
            set => _instance = value;
        }

        private Page CurrentPage => CurrentChart.PageList[CurrentPageIndex];

        private double ScheduledTime { get; set; }

        public void Awake()
        {
            _selectionBox = GameObject.Find("SelectionBox");
            _selectionBox.SetActive(false);
            _playPauseButton = GameObject.Find("PlayPauseButton");
            _mainCamera = Camera.main;
            _lockYText = GameObject.Find("LockYText");
            _lockYText.SetActive(false);
            utilityLineRenderer = GetComponent<LineRenderer>();

            AudioManager.SetHitsoundVolume(Config.HitsoundVolume);
            AudioManager.SetMusicVolume(Config.MusicVolume);

            if (CurrentChart == null) return;

            CalculateTimings();

            CalculateDragIDs();

            _objectPool = new ChartObjectPool();

            if (_currentPageIndexOverride != -1)
            {
                CurrentPageIndex = _currentPageIndexOverride;
                _currentPageIndexOverride = -1;
            }
            else
            {
                CurrentPageIndex = 0;
            }

            UpdateTime(CurrentPage.ActualStartTime);

            UpdatePlaybackSpeed();
        }

        private void Start()
        {
            gameObject.GetComponent<SideButtonController>().ChangeTool(CurrentTool);

            UIAdjuster.AdjustToResolution();

            // Keep current page index after navigating to editor/level/chart options
            GameObject.Find("LevelOptionsButton").GetComponent<Button>().onClick
                .AddListener(() => _currentPageIndexOverride = CurrentPageIndex);
            GameObject.Find("EditorSettingsButton").GetComponent<Button>().onClick
                .AddListener(() => _currentPageIndexOverride = CurrentPageIndex);

            // Create vertical divisor lines
            if (Config.VerticalLineAccent)
            {
                int mid = (Config.VerticalDivisors - Config.VerticalDivisors % 2) / 2;

                GameObject obj;

                for (int i = 1; i < mid; i++)
                {
                    obj = Instantiate(DivisorLinePrefab);
                    obj.transform.position =
                        new Vector3(PlayAreaWidth / Config.VerticalDivisors * i - PlayAreaWidth / 2, 0);
                    obj.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
                    obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaHeight, 0.05f);
                    obj.tag = "GridLine";
                }

                for (int i = mid + 1 + Config.VerticalDivisors % 2; i < Config.VerticalDivisors; i++)
                {
                    obj = Instantiate(DivisorLinePrefab);
                    obj.transform.position =
                        new Vector3(PlayAreaWidth / Config.VerticalDivisors * i - PlayAreaWidth / 2, 0);
                    obj.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
                    obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaHeight, 0.05f);
                    obj.tag = "GridLine";
                }

                if (Config.VerticalDivisors % 2 == 1)
                {
                    obj = Instantiate(DivisorLinePrefab);
                    obj.transform.position =
                        new Vector3(PlayAreaWidth / Config.VerticalDivisors * (mid + 1) - PlayAreaWidth / 2, 0);
                    obj.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
                    obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaHeight, 0.15f);
                    obj.tag = "GridLine";
                    obj.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.59f);
                }

                obj = Instantiate(DivisorLinePrefab);
                obj.transform.position =
                    new Vector3(PlayAreaWidth / Config.VerticalDivisors * mid - PlayAreaWidth / 2, 0);
                obj.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
                obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaHeight, 0.15f);
                obj.tag = "GridLine";
                obj.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.59f);
            }
            else
            {
                for (int i = 0; i <= Config.VerticalDivisors; i++)
                {
                    GameObject obj = Instantiate(DivisorLinePrefab);
                    obj.transform.position =
                        new Vector3(PlayAreaWidth / Config.VerticalDivisors * i - PlayAreaWidth / 2, 0);
                    obj.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
                    obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaHeight, 0.1f);
                    obj.tag = "GridLine";
                }
            }

            // Create horizontal divisor lines
            BeatDivisorSlider.value = _beatDivisorValue;

            RenderDivisorLines();

            UpdateOffsetText();

            GameObject.Find("BeatDivisorInputField").GetComponent<InputField>()
                .onEndEdit.AddListener(s => SetBeatDivisorValueUnsafe(Int32.Parse(s)));
        }

        private void Update()
        {
#if UNITY_STANDALONE
            if (WasPressed(HotkeyManager.PlayPause))
            {
                PlayPause();
            }
#endif
            if (_saveEditorOffsetScheduledTime > 0 && Time.time > _saveEditorOffsetScheduledTime)
            {
                SaveConfig();
                _saveEditorOffsetScheduledTime = -1;
            }

            if (_isStartScheduled)
            {
                if (AudioSettings.dspTime > ScheduledTime)
                {
                    IsGameRunning = true;
                    _playPauseButton.SetActive(true);
                    _isStartScheduled = false;
                }
            }

            if (IsGameRunning)
            {
                if (!AudioManager.IsPlaying) // If the music ended
                {
                    IsGameRunning = false;
                    CurrentPageIndex = 0;
                    UpdateTime(0);
                    return;
                }

                if (Config.UpdateTimelineWhileRunning)
                {
                    Timeline.SetValueWithoutNotify((float) (AudioManager.Time / AudioManager.MaxTime));
                }

                double time = AudioManager.Time + Offset;

                while (_currentHitsoundIndex < _hitsoundTimings.Count &&
                       _hitsoundTimings[_currentHitsoundIndex] <= time)
                {
                    AudioManager.PlayHitsound();
                    _currentHitsoundIndex++;
                }

                while (_currentNoteIndex < _noteSpawns.Count && _noteSpawns[_currentNoteIndex].Time <= time)
                {
                    SpawnNote(CurrentChart.NoteList[_noteSpawns[_currentNoteIndex].ID],
                        time - _noteSpawns[_currentNoteIndex].Time);
                    _currentNoteIndex++;
                }

                while (CurrentPageIndex < CurrentChart.PageList.Count && CurrentPage.EndTime < time)
                {
                    CurrentPageIndex++;

                    if (CurrentChart.PageList[CurrentPageIndex - 1].ActualPageSize != CurrentPage.ActualPageSize)
                    {
                        UpdateBpmText();
                    }
                }

                while (_currentTempoIndex + 1 < CurrentChart.TempoList.Count &&
                       CurrentChart.TempoList[_currentTempoIndex + 1].Time < time)
                {
                    _currentTempoIndex++;
                    UpdateBpmText();
                }

                PageText.text = CurrentPageIndex.ToString();
                TimeText.text = TimestampParser.Serialize(time);

                if (CurrentPageIndex <
                    CurrentChart.PageList.Count) // in case the pages don't go to the end of the chart
                {
                    double currentTick = CurrentChart.TempoList[_currentTempoIndex].Tick +
                                         (time - CurrentChart.TempoList[_currentTempoIndex].Time) * 1000000 /
                                         CurrentChart.TempoList[_currentTempoIndex].Value * CurrentChart.TimeBase;

                    Scanline.transform.position = new Vector3(0, CurrentPage.ScanLineDirection == 1
                        ? PlayAreaHeight * (float) ((currentTick - CurrentPage.StartTick) / CurrentPage.PageSize - 0.5)
                        : PlayAreaHeight *
                          (float) (0.5 - (currentTick - CurrentPage.StartTick) / CurrentPage.PageSize));
                }
            }
            else
            {
#if UNITY_STANDALONE
                HandlePCInput();
#endif
                HandleInput();
            }

            if (Screen.safeArea != _lastSafeArea)
            {
                _lastSafeArea = Screen.safeArea;
                if (Screen.orientation == ScreenOrientation.LandscapeLeft)
                {
                    foreach (RectTransform transform in LeftNotchObstructedObjects)
                    {
                        transform.anchoredPosition = new Vector2(Screen.safeArea.x, transform.anchoredPosition.y);
                    }
                }
                else if (Screen.orientation == ScreenOrientation.LandscapeRight)
                {
                    foreach (RectTransform transform in RightNotchObstructedObjects)
                    {
                        transform.anchoredPosition = new Vector2(-Screen.safeArea.x, transform.anchoredPosition.y);
                    }
                }
            }
        }

        /// <summary>
        ///     Calculates the start_time and end_time of pages, the Time of tempos and the Time, y, approach_time and hold_time of
        ///     notes.
        /// </summary>
        private void CalculateTimings()
        {
            int timebase = CurrentChart.TimeBase;
            List<Note> notes = CurrentChart.NoteList;
            List<Tempo> tempos = CurrentChart.TempoList;
            List<Page> pages = CurrentChart.PageList;

            _hitsoundTimings.Clear();
            _noteSpawns.Clear();

            int ni = 0, pi = 0, ti = 0, n = notes.Count, p = pages.Count, t = tempos.Count;
            double tempoSum = 0; // Time from tick 0 to the start of tempo ti

            // Calculate page times and tempo times
            while (ti < t && pi < p)
            {
                if (ti + 1 < t &&
                    pages[pi].StartTick >= tempos[ti + 1].Tick) // If page is not on this tempo, go to next tempo
                {
                    tempos[ti].Time = tempoSum;
                    tempoSum += (double) tempos[ti].Value * (tempos[ti + 1].Tick - tempos[ti].Tick) / timebase /
                                1000000;
                    ti++;
                }
                else
                {
                    // If page starts at tempo ti and ends at tempo tj
                    if (ti + 1 < t && pages[pi].EndTick > tempos[ti + 1].Tick)
                    {
                        int tj = ti + 1;
                        double auxTempoSum = 0; // Time from start of tempo (t1 + 1) to start of tempo t2

                        while (tj + 1 < t && pages[pi].EndTick >= tempos[tj + 1].Tick)
                        {
                            auxTempoSum += (double) tempos[tj].Value * (tempos[tj + 1].Tick - tempos[tj].Tick) /
                                           timebase / 1000000;
                            tj++;
                        }

                        pages[pi].StartTime = tempoSum + (double) tempos[ti].Value *
                            (pages[pi].StartTick - tempos[ti].Tick) / timebase / 1000000;
                        pages[pi].EndTime =
                            pages[pi].StartTime + (double) tempos[ti].Value *
                                                (tempos[ti + 1].Tick - pages[pi].StartTick) / timebase / 1000000
                                                + auxTempoSum + (double) tempos[tj].Value *
                                                (pages[pi].EndTick - tempos[tj].Tick) / timebase / 1000000;
                    }
                    else
                    {
                        // If page starts and ends at tempo t1
                        pages[pi].StartTime = tempoSum + (double) tempos[ti].Value *
                            (pages[pi].StartTick - tempos[ti].Tick) / timebase / 1000000;
                        pages[pi].EndTime = tempoSum + (double) tempos[ti].Value *
                            (pages[pi].EndTick - tempos[ti].Tick) / timebase / 1000000;
                    }


                    if (pi != 0)
                    {
                        pages[pi].ActualStartTick = pages[pi - 1].EndTick;
                        pages[pi].ActualStartTime = pages[pi - 1].EndTime;
                    }
                    else
                    {
                        pages[pi].ActualStartTick = 0;
                        pages[pi].ActualStartTime = 0;
                    }

                    pi++;
                }
            }

            while (ti + 1 < t)
            {
                tempos[ti].Time = tempoSum;
                tempoSum += (double) tempos[ti].Value * (tempos[ti + 1].Tick - tempos[ti].Tick) / timebase / 1000000;
                ti++;
            }

            if (ti < t)
            {
                tempos[ti].Time = tempoSum;
            }

            double realMaxTime = AudioManager.MaxTime + CurrentChart.MusicOffset;
            if (pages[p - 1].EndTime < realMaxTime) // Add pages in case the page_list ends before the music
            {
                double lastTempoTime =
                    CurrentChart.TempoList.Last().Value /
                    1000000.0; // Calculate Time of the pages in respect to the last tempo
                while (pages[p - 1].EndTime < realMaxTime)
                {
                    pages.Add(new Page
                    {
                        StartTick = pages[p - 1].EndTick,
                        EndTick = pages[p - 1].EndTick + timebase,
                        StartTime = pages[p - 1].EndTime,
                        EndTime = pages[p - 1].EndTime + lastTempoTime,
                        ScanLineDirection = -pages[p - 1].ScanLineDirection
                    });
                    p++;
                }

                CalculateTimings();
                return;
            }

            if (pages[p - 1].ActualStartTime > realMaxTime && n > 0 && notes[n - 1].PageIndex + 1 < p)
            {
                while (pages[p - 1].ActualStartTime > realMaxTime && notes[n - 1].PageIndex + 1 < p)
                {
                    pages.RemoveAt(p - 1);
                    p--;
                }

                CalculateTimings();
                return;
            }

            tempos[ti].Time = tempoSum;

            tempoSum = 0;
            ti = 0;

            // Calculate note Time, hold Time, AR and others
            while (ti < t && ni < n)
            {
                if (ti + 1 < t && notes[ni].Tick >= tempos[ti + 1].Tick)
                {
                    tempoSum += (double) tempos[ti].Value * (tempos[ti + 1].Tick - tempos[ti].Tick) / timebase /
                                1000000;
                    ti++;
                }
                else
                {
                    notes[ni].ActualOpacity = notes[ni].Opacity < 0 ? CurrentChart.Opacity : notes[ni].Opacity;
                    notes[ni].ActualSize = notes[ni].Size < 0 ? CurrentChart.Size : notes[ni].Size;

                    notes[ni].Time = tempoSum + (double) tempos[ti].Value * (notes[ni].Tick - tempos[ti].Tick) /
                        timebase / 1000000;
                    _hitsoundTimings.Add(notes[ni].Time);

                    notes[ni].Y = pages[notes[ni].PageIndex].ScanLineDirection == 1
                        ? (double) (notes[ni].Tick - pages[notes[ni].PageIndex].ActualStartTick) /
                          (pages[notes[ni].PageIndex].EndTick - pages[notes[ni].PageIndex].ActualStartTick)
                        : 1.0 - (double) (notes[ni].Tick - pages[notes[ni].PageIndex].ActualStartTick) /
                        (pages[notes[ni].PageIndex].EndTick - pages[notes[ni].PageIndex].ActualStartTick);

                    if (notes[ni].PageIndex == 0)
                    {
                        notes[ni].ApproachTime = 1.367 / notes[ni].ApproachRate;
                    }
                    else
                    {
                        Page currPage = pages[notes[ni].PageIndex];
                        Page prevPage = pages[notes[ni].PageIndex - 1];
                        double pageRatio = (double) (notes[ni].Tick - currPage.ActualStartTick) /
                                           (currPage.EndTick - currPage.ActualStartTick);

                        notes[ni].ApproachTime = 1.367 / (notes[ni].ApproachRate * Math.Max(1.0,
                            1.367 / ((currPage.EndTime - currPage.ActualStartTime) * pageRatio +
                                     (prevPage.EndTime - prevPage.ActualStartTime) * (1.367 - pageRatio))));
                    }

                    if (notes[ni].Type == 1 || notes[ni].Type == 2)
                    {
                        int holdEndTick = notes[ni].Tick + notes[ni].HoldTick;
                        // Calculate hold Time in the same way as page end Time
                        if (ti + 1 < t && holdEndTick > tempos[ti + 1].Tick)
                        {
                            int tj = ti + 1;
                            double auxTempoSum = 0;
                            while (tj + 1 < t && holdEndTick >= tempos[tj + 1].Tick)
                            {
                                auxTempoSum += (double) tempos[tj].Value * (tempos[tj + 1].Tick - tempos[tj].Tick) /
                                               timebase / 1000000;
                                tj++;
                            }

                            notes[ni].HoldTime =
                                auxTempoSum + (double) tempos[ti].Value * (tempos[ti + 1].Tick - notes[ni].Tick) /
                                            timebase / 1000000
                                            + (double) tempos[tj].Value * (holdEndTick - tempos[tj].Tick) / timebase /
                                            1000000;
                        }
                        else
                        {
                            notes[ni].HoldTime = (double) tempos[ti].Value * notes[ni].HoldTick / timebase / 1000000;
                        }

                        if (Config.PlayHitsoundsOnHoldEnd)
                        {
                            _hitsoundTimings.Add(notes[ni].Time + notes[ni].HoldTime);
                        }
                    }
                    else
                    {
                        notes[ni].HoldTime = 0;
                    }

                    _noteSpawns.Add(new NoteSpawnTime
                        {Time = notes[ni].Time - notes[ni].ApproachTime, ID = notes[ni].ID});

                    ni++;
                }
            }

            _noteSpawns.Sort(
                (a, b) =>
                    Math.Abs(a.Time - b.Time) < 0.0001
                        ? CurrentChart.NoteList[a.ID].PageIndex.CompareTo(CurrentChart.NoteList[b.ID].PageIndex)
                        : a.Time.CompareTo(b.Time)
            );
            _hitsoundTimings.Sort();
            // if performance is an issue because of this, then create a list of hold end timings
            // and merge it with the hitsound timings / note spawns list

            CalculateDragIDs();
        }

        private void CalculateDragIDs()
        {
            int dragID = -1;
            foreach (Note note in CurrentChart.NoteList
                .Where(note => note.Type == (int) NoteType.CDragHead || note.Type == (int) NoteType.DragHead))
            {
                dragID++;
                int id = note.ID;
                while (CurrentChart.NoteList[id].NextID >= 0)
                {
                    CurrentChart.NoteList[id].DragID = dragID;
                    id = CurrentChart.NoteList[id].NextID;
                }

                CurrentChart.NoteList[id].DragID = dragID;
            }

            _currentDragID = dragID + 1;
        }

        public static int GetDragParent(int id)
        {
            int i = id - 1;
            while (i >= 0)
            {
                if (CurrentChart.NoteList[i].NextID == id)
                {
                    return i;
                }

                i--;
            }

            return -1;
        }

        /// <summary>
        ///     Adds the note passed as parameter to the <see cref="CurrentChart" />'s note_list so that the list remains sorted by
        ///     tick.
        /// </summary>
        /// <param name="noteToAdd"> The note to be added, its ID will be modified. </param>
        /// <returns> Returns the position it was added to. </returns>
        public int AddNoteInternal(Note noteToAdd)
        {
            int pos = 0; // Determine the position to be inserted in
            // Currently using sequential search and not binary because we already have necessary O(N) complexity
            // following so it does not make much of a difference, to be changed if performance is hit because of this.
            while (pos < CurrentChart.NoteList.Count && CurrentChart.NoteList[pos].Tick < noteToAdd.Tick)
            {
                pos++;
            }

            noteToAdd.ID = pos;
            foreach (Note note in CurrentChart.NoteList)
            {
                if (note.Type == (int) NoteType.DragHead ||
                    note.Type == (int) NoteType.DragChild ||
                    note.Type == (int) NoteType.CDragChild ||
                    note.Type == (int) NoteType.CDragHead)
                {
                    if (note.NextID == -1 && note.DragID == noteToAdd.DragID)
                    {
                        note.NextID = noteToAdd.ID;
                    }

                    if (note.NextID >= pos)
                    {
                        note.NextID++;
                    }
                }
                else
                {
                    note.NextID = 0;
                }
            }

            if (CurrentChart.NoteList.Count == 0)
            {
                CurrentChart.NoteList.Add(noteToAdd);
                return 0;
            }

            // Use a classic insertion algorithm while modifying ids accordingly
            CurrentChart.NoteList.Add(new Note());
            CurrentChart.NoteList.Last().ID = CurrentChart.NoteList.Count - 1;
            for (int i = CurrentChart.NoteList.Count - 1; i > pos; i--)
            {
                CurrentChart.NoteList[i] = CurrentChart.NoteList[i - 1];
                CurrentChart.NoteList[i].ID = i;
            }

            CurrentChart.NoteList[pos] = noteToAdd;
            GameObject.Find("NoteCountText").GetComponent<Text>().text = $"Note count: {CurrentChart.NoteList.Count}";
            return pos;
        }

        private static int AddNote(Note note)
        {
            var cmd = new PlaceNotesCommand(new[] {note});
            CommandSystem.AppendInvoke(cmd);
            return cmd.AffectedNoteIDs[0];
        }

        /// <summary>
        ///     Removes the note with the specified ID.
        /// </summary>
        public void RemoveNoteInternal(int noteID)
        {
            for (int i = 0; i < _noteSpawns.Count; i++)
            {
                if (_noteSpawns[i].ID != noteID) continue;
                _noteSpawns.RemoveAt(i);
                for (int j = 0; j < _noteSpawns.Count; j++)
                {
                    _noteSpawns[j] = new NoteSpawnTime
                    {
                        ID = _noteSpawns[j].ID - (_noteSpawns[j].ID > noteID ? 1 : 0),
                        Time = _noteSpawns[j].Time
                    };
                }

                break;
            }

            if (CurrentChart.NoteList[noteID].Type == (int) NoteType.CDragHead ||
                CurrentChart.NoteList[noteID].Type == (int) NoteType.DragHead)
                // If the deleted note is a (c)drag head, then make the next note the head instead
            {
                int nxt = CurrentChart.NoteList[noteID].NextID;
                if (nxt > 0)
                {
                    CurrentChart.NoteList[nxt].Type = CurrentChart.NoteList[noteID].Type;
                }
            }

            foreach (Note note in CurrentChart.NoteList)
            {
                if (note.NextID > noteID)
                {
                    note.NextID--;
                }
                else if (note.NextID == noteID)
                    // If the deleted note is part of a (c)drag chain,
                    // then remove it from the chain while keeping the chain valid
                {
                    note.NextID = CurrentChart.NoteList[noteID].NextID -
                                  (CurrentChart.NoteList[noteID].NextID > noteID ? 1 : 0);
                }
            }

            for (int i = 0; i < _hitsoundTimings.Count; i++)
            {
                if (Math.Abs(CurrentChart.NoteList[noteID].Time - _hitsoundTimings[i]) < 0.001)
                {
                    _hitsoundTimings.RemoveAt(i);
                    break;
                }
            }

            if (Config.PlayHitsoundsOnHoldEnd && (CurrentChart.NoteList[noteID].Type == (int) NoteType.Hold ||
                                                  CurrentChart.NoteList[noteID].Type == (int) NoteType.LongHold))
            {
                for (int i = 0; i < _hitsoundTimings.Count; i++)
                {
                    if (Math.Abs(CurrentChart.NoteList[noteID].Time + CurrentChart.NoteList[noteID].HoldTime -
                                 _hitsoundTimings[i]) < 0.001)
                    {
                        _hitsoundTimings.RemoveAt(i);
                        break;
                    }
                }
            }

            // Use a classic deletion algorithm while modifying ids accordingly
            for (int i = noteID; i + 1 < CurrentChart.NoteList.Count; i++)
            {
                CurrentChart.NoteList[i] = CurrentChart.NoteList[i + 1];
                CurrentChart.NoteList[i].ID = i;
            }

            CurrentChart.NoteList.RemoveAt(CurrentChart.NoteList.Count - 1);
            GameObject.Find("NoteCountText").GetComponent<Text>().text = $"Note count: {CurrentChart.NoteList.Count}";
        }

        private static void RemoveNote(int noteID)
        {
            var cmd = new RemoveNotesCommand(new[] {noteID});
            CommandSystem.AppendInvoke(cmd);
        }

        /// <summary>
        ///     Spawn a note in world space.
        /// </summary>
        /// <param name="note"> The note data that contains the necessary note properties. </param>
        /// <param name="delay"> The delay, in seconds, from when the note is spawned to when it should have been spawned. </param>
        /// <param name="lowerOpacity">
        ///     If the note should appear with lower(1/3rd) opacity, reserved for notes on the previous
        ///     page.
        /// </param>
        private void SpawnNote(Note note, double delay, bool lowerOpacity = false)
        {
            GameObject obj = _objectPool.GetNote((NoteType) note.Type);

            var noteController = obj.GetComponent<NoteController>();

            noteController.ParentPool = _objectPool;
            noteController.Initialize(note);

            obj.SetActive(true);

            int colorIndex = ColorIndexes[note.Type];

            ColorUtility.TryParseHtmlString(note.FillColor ??
                                            CurrentChart.FillColors[
                                                CurrentChart.PageList[note.PageIndex].ScanLineDirection == 1
                                                    ? colorIndex
                                                    : colorIndex + 1] ??
                                            DefaultFillColors[
                                                CurrentChart.PageList[note.PageIndex].ScanLineDirection == 1
                                                    ? colorIndex
                                                    : colorIndex + 1],
                out Color noteColor);

            noteColor.a = (float) note.ActualOpacity / (lowerOpacity ? 3 : 1);
            noteController.ChangeNoteColor(noteColor);

            noteController.SetDelay((float) delay);

            noteController.PlaybackSpeed = _playbackSpeeds[_playbackSpeedIndex];

            if (noteController.Notetype != (int) NoteType.LongHold) return;
            if (note.Tick + note.HoldTick >= CurrentPage.StartTick)
            {
                obj.GetComponent<LongHoldNoteController>().FinishIndicator.transform.position =
                    new Vector3(
                        obj.transform.position.x,
                        CurrentPage.ScanLineDirection *
                        (PlayAreaHeight * (note.Tick + note.HoldTick - CurrentPage.ActualStartTick) /
                            (int) CurrentPage.ActualPageSize - PlayAreaHeight / 2)
                    );
            }
            else
            {
                obj.GetComponent<LongHoldNoteController>().FinishIndicator.transform.position =
                    new Vector3(obj.transform.position.x, 10000);
            }
        }

        /// <summary>
        ///     Spawn a scanline/tempo note from the <see cref="Tempo" /> with the specified ID.
        /// </summary>
        /// <param name="id"> The ID of the <see cref="Tempo" /> this note represents. </param>
        private void SpawnScanlineNote(int id)
        {
            GameObject obj = Instantiate(ScanlineNotePrefab);

            obj.GetComponent<ScanlineNoteController>().TempoID = id;

            obj.GetComponent<ScanlineNoteController>().SetPosition(
                new Vector3(
                    -PlayAreaWidth / 2 - 1,
                    -PlayAreaHeight / 2 + PlayAreaHeight * (float) (CurrentPage.ScanLineDirection == 1
                        ? (CurrentChart.TempoList[id].Tick - CurrentPage.ActualStartTick) / CurrentPage.ActualPageSize
                        : 1.0 - (CurrentChart.TempoList[id].Tick - CurrentPage.ActualStartTick) /
                        CurrentPage.ActualPageSize)
                )
            );

            obj.GetComponent<ScanlineNoteController>().TimeInputField.text =
                (CurrentChart.TempoList[id].Time - CurrentChart.MusicOffset).ToString(CultureInfo.InvariantCulture);

            double bpm = 120000000.0 / CurrentChart.TempoList[id].Value * 480 / CurrentChart.TimeBase;

            obj.GetComponent<ScanlineNoteController>().BPMInputField.text =
                Math.Round(bpm, 2).ToString(CultureInfo.InvariantCulture);
        }


        /// <summary>
        ///     Adds a tempo to the <see cref="CurrentChart" />'s tempo_list while keeping the list sorted by tick.
        /// </summary>
        /// <param name="tempo"> The tempo to be added. </param>
        private void AddTempo(Tempo tempo)
        {
            int poz = 0;
            while (poz < CurrentChart.TempoList.Count && tempo.Tick > CurrentChart.TempoList[poz].Tick)
            {
                poz++;
            }

            if (poz < CurrentChart.TempoList.Count && CurrentChart.TempoList[poz].Tick == tempo.Tick)
            {
                poz++;
                tempo.Tick++;
            }

            CurrentChart.TempoList.Insert(poz, tempo);
        }

        /// <summary>
        ///     Removes the tempo with the specified ID.
        /// </summary>
        /// <param name="id"> The ID of the tempo to be removed. </param>
        private void RemoveTempo(int id)
        {
            CurrentChart.TempoList.RemoveAt(id);
            CalculateTimings();
        }

        /// <summary>
        ///     Schedules the start or pauses the current chart.
        /// </summary>
        public void PlayPause()
        {
            if (CurrentChart == null || _isStartScheduled)
            {
                return;
            }

            if (IsGameRunning)
            {
                AudioManager.Pause();
                IsGameRunning = false;
                _isStartScheduled = false;

                if (CurrentPageIndex < CurrentChart.PageList.Count)
                {
                    UpdateTime(CurrentPage.ActualStartTime);
                }
                else
                {
                    CurrentPageIndex = CurrentChart.PageList.Count - 1;
                    UpdateTime(CurrentPage.ActualStartTime);
                }
            }
            else
            {
                _playPauseButton.SetActive(false);
                ScheduledTime = AudioManager.Play();
                _isStartScheduled = true;
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
            foreach (Note note in CurrentChart.NoteList)
            {
                if ((note.Type != (int) NoteType.CDragHead && note.Type != (int) NoteType.DragHead) ||
                    note.PageIndex >= CurrentPageIndex) continue;
                int id = note.ID;
                while (CurrentChart.NoteList[id].NextID > 0)
                {
                    if (CurrentChart.NoteList[id].PageIndex == CurrentPageIndex)
                    {
                        SpawnNote(note, CurrentPage.ActualStartTime - note.Time + note.ApproachTime);
                        break;
                    }

                    id = CurrentChart.NoteList[id].NextID;
                }
            }
        }

        public void BeatDivisorValueChanged()
        {
            for (int i = 0; i < 8; i++)
            {
                if (_allowedDivisors[i] <= (int) BeatDivisorSlider.value)
                {
                    _beatDivisorValue = _allowedDivisors[i];
                }
            }

            BeatDivisorSlider.SetValueWithoutNotify(_beatDivisorValue);
            RenderDivisorLines();
        }

        private void SetBeatDivisorValueUnsafe(int val)
        {
            val = Clamp(val, 1, 64);
            BeatDivisorSlider.SetValueWithoutNotify(val);
            _beatDivisorValue = val;
            RenderDivisorLines();
        }

        private void RenderDivisorLines()
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("DivisorLine"))
            {
                Destroy(obj);
            }

            int interval = 1000;
            if (_beatDivisorValue % 3 == 0)
            {
                interval = 3;
            }
            else if (_beatDivisorValue % 4 == 0)
            {
                interval = 4;
            }

            for (int i = 1; i < _beatDivisorValue; i++)
            {
                GameObject obj = Instantiate(DivisorLinePrefab);
                obj.transform.position = new Vector3(0, PlayAreaHeight / _beatDivisorValue * i - PlayAreaHeight / 2);
                obj.GetComponent<SpriteRenderer>().size = new Vector2(PlayAreaWidth,
                    i % interval == 0 && Config.HorizontalLineAccents ? 0.175f : 0.1f);
            }

            GameObject.Find("BeatDivisorInputField").GetComponent<InputField>().text = _beatDivisorValue.ToString();
        }

        public void TimelineValueChange()
        {
            if (CurrentChart == null)
            {
                Timeline.SetValueWithoutNotify(0);
                return;
            }

            if (!IsGameRunning)
            {
                double time = Timeline.value * AudioManager.MaxTime;

                CurrentPageIndex = SnapTimeToPage(time);

                time = CurrentPage.ActualStartTime;

                UpdateTime(time);
            }
        }

        public static void RefreshNote(int noteID)
        {
            bool needUpdate = false;
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
            {
                if (obj.GetComponent<NoteController>().NoteID == noteID)
                {
                    int type = obj.GetComponent<NoteController>().Notetype;
                    if (type == (int) NoteType.DragHead || type == (int) NoteType.DragChild ||
                        type == (int) NoteType.CDragHead || type == (int) NoteType.CDragChild)
                    {
                        needUpdate = true;
                    }

                    obj.GetComponent<NoteController>().Initialize(CurrentChart.NoteList[noteID]);
                    if (type == (int) NoteType.LongHold)
                    {
                        Note note = CurrentChart.NoteList[obj.GetComponent<NoteController>().NoteID];
                        Page currentPage = GameObject.Find("UICanvas").GetComponent<GameLogic>().CurrentPage;
                        if (note.Tick + note.HoldTick >= currentPage.StartTick)
                        {
                            obj.GetComponent<LongHoldNoteController>().FinishIndicator.transform.position = new Vector3(
                                obj.transform.position.x, currentPage.ScanLineDirection *
                                                          (PlayAreaHeight *
                                                              (note.Tick + note.HoldTick -
                                                               currentPage.ActualStartTick) /
                                                              (int) currentPage.ActualPageSize - PlayAreaHeight / 2));
                        }
                        else
                        {
                            obj.GetComponent<LongHoldNoteController>().FinishIndicator.transform.position =
                                new Vector3(obj.transform.position.x, 10000);
                        }
                    }
                }
            }

            if (needUpdate)
            {
                var g = GameObject.Find("UICanvas").GetComponent<GameLogic>();
                g.UpdateTime(g.CurrentPage.StartTime);
            }
        }

        public static void ForceUpdate()
        {
            var obj = GameObject.Find("UICanvas").GetComponent<GameLogic>();
            obj.CalculateTimings();
            obj.UpdateTime(obj.CurrentPage.StartTime);
        }
        
        private void UpdateBpmText()
        {
            double scanlineBpm = 120000000.0 / CurrentChart.TempoList[_currentTempoIndex].Value
                * 480.0 / CurrentPage.ActualPageSize * 480.0 / CurrentChart.TimeBase;
            GameObject.Find("CurrentBPMText").GetComponentInChildren<Text>().text =
                $"BPM: {Math.Round(scanlineBpm, 2)}";
        }

        /// <summary>
        ///     Updates the current Time to the <paramref name="time" /> specified.
        /// </summary>
        /// <param name="time"> The specified Time the chart should be at. </param>
        private void UpdateTime(double time)
        {
            NotePropsManager.Clear();
            MakeButtonsInteractable();

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
            {
                _objectPool.ReturnToPool(obj, obj.GetComponent<NoteController>().Notetype);
            }

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("ScanlineNote"))
            {
                Destroy(obj);
            }

            _currentHitsoundIndex = 0;

            _currentNoteIndex = 0;

            for (int i = 0; i < _noteSpawns.Count; i++)
            {
                if (_noteSpawns[i].Time <= time)
                {
                    if (CurrentChart.NoteList[_noteSpawns[i].ID].PageIndex == CurrentPageIndex ||
                        Config.ShowApproachingNotesWhilePaused &&
                        CurrentChart.NoteList[_noteSpawns[i].ID].PageIndex > CurrentPageIndex)
                    {
                        SpawnNote(CurrentChart.NoteList[_noteSpawns[i].ID], time - _noteSpawns[i].Time);
                        _currentNoteIndex = i + 1;
                    }
                    else if ((CurrentChart.NoteList[_noteSpawns[i].ID].Type == (int) NoteType.Hold ||
                              CurrentChart.NoteList[_noteSpawns[i].ID].Type == (int) NoteType.LongHold)
                             && CurrentChart.NoteList[_noteSpawns[i].ID].Time +
                             CurrentChart.NoteList[_noteSpawns[i].ID].HoldTime >= time &&
                             (CurrentChart.NoteList[_noteSpawns[i].ID].PageIndex <= CurrentPageIndex ||
                              Config.ShowApproachingNotesWhilePaused))
                    {
                        SpawnNote(CurrentChart.NoteList[_noteSpawns[i].ID], time - _noteSpawns[i].Time);
                        _currentNoteIndex = i + 1;
                    }
                    else if (CurrentChart.NoteList[_noteSpawns[i].ID].PageIndex + 1 == CurrentPageIndex)
                    {
                        SpawnNote(CurrentChart.NoteList[_noteSpawns[i].ID], 10000, true);
                        _currentNoteIndex = i + 1;
                    }
                    else if (CurrentChart.NoteList[_noteSpawns[i].ID].PageIndex < CurrentPageIndex)
                    {
                        _currentNoteIndex = i + 1;
                    }
                }
                else if (CurrentChart.NoteList[_noteSpawns[i].ID].PageIndex == CurrentPageIndex)
                {
                    SpawnNote(CurrentChart.NoteList[_noteSpawns[i].ID], 10000);
                }
            }

            // Optimize if necessary
            for (int i = 0; i < _hitsoundTimings.Count; i++)
            {
                if (_hitsoundTimings[i] < time)
                {
                    _currentHitsoundIndex = i + 1;
                }
            }

            for (int i = 0; i < CurrentChart.TempoList.Count; i++)
            {
                if (CurrentChart.TempoList[i].Tick <= CurrentPage.EndTick &&
                    CurrentChart.TempoList[i].Tick >= CurrentPage.ActualStartTick)
                {
                    SpawnScanlineNote(i);
                }

                if (CurrentChart.TempoList[i].Time <= time)
                {
                    _currentTempoIndex = i;
                }
            }

            UpdateBpmText();

            GameObject.Find("SweepChangeButton").GetComponentInChildren<Text>().text =
                CurrentPage.ScanLineDirection == 1 ? "Up" : "Down";

            GameObject.Find("PageText").GetComponent<Text>().text = CurrentPageIndex.ToString();
            int milliseconds = (int) ((time - CurrentChart.MusicOffset) * 1000 -
                                      Math.Floor(time - CurrentChart.MusicOffset) * 1000);
            if (time < CurrentChart.MusicOffset && milliseconds != 0)
            {
                milliseconds = 1000 - milliseconds;
            }

            TimeText.text = TimestampParser.Serialize(time - CurrentChart.MusicOffset);

            GameObject.Find("NoteCountText").GetComponent<Text>().text = $"Note count: {CurrentChart.NoteList.Count}";

            Scanline.transform.position = new Vector3(0, CurrentPage.ScanLineDirection == 1
                ? PlayAreaHeight * (float) ((time - CurrentPage.ActualStartTime) /
                    (CurrentPage.EndTime - CurrentPage.ActualStartTime) - 0.5f)
                : PlayAreaHeight * (0.5f - (float) ((time - CurrentPage.ActualStartTime) /
                                                    (CurrentPage.EndTime - CurrentPage.ActualStartTime))));

            AudioManager.Time = time - Offset;

            Timeline.SetValueWithoutNotify((float) (AudioManager.Time / AudioManager.MaxTime));
        }

        public void Dragify()
        {
            var highlighted = new List<int>();

            bool isFullDrag = true, isFullCDrag = true;

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
            {
                if (obj.GetComponent<IHighlightable>().Highlighted)
                {
                    highlighted.Add(obj.GetComponent<NoteController>().NoteID);
                    int noteType = obj.GetComponent<NoteController>().Notetype;
                    if (noteType != (int) NoteType.DragHead && noteType != (int) NoteType.DragChild)
                    {
                        isFullDrag = false;
                    }

                    if (noteType != (int) NoteType.CDragHead && noteType != (int) NoteType.CDragChild)
                    {
                        isFullCDrag = false;
                    }
                }
            }

            highlighted.Sort();

            if (highlighted.Count > 1)
            {
                int targetType;
                if (isFullDrag)
                {
                    CurrentChart.NoteList[highlighted[0]].Type = (int) NoteType.CDragHead;

                    if (CurrentChart.NoteList[highlighted[0]].NextID > 0)
                    {
                        Note note = CurrentChart.NoteList[CurrentChart.NoteList[highlighted[0]].NextID];
                        if (note.Type == (int) NoteType.CDragChild)
                        {
                            note.Type = (int) NoteType.CDragHead;
                        }
                        else
                        {
                            note.Type = (int) NoteType.DragHead;
                        }
                    }

                    CurrentChart.NoteList[highlighted[0]].NextID = highlighted[1];
                    CurrentChart.NoteList[highlighted[0]].HoldTick = 0;
                    targetType = (int) NoteType.CDragChild;
                }
                else if (isFullCDrag)
                {
                    targetType = (int) NoteType.Click;
                }
                else
                {
                    CurrentChart.NoteList[highlighted[0]].Type = (int) NoteType.DragHead;
                    CurrentChart.NoteList[highlighted[0]].NextID = highlighted[1];
                    CurrentChart.NoteList[highlighted[0]].HoldTick = 0;
                    targetType = (int) NoteType.DragChild;
                }

                if (targetType == (int) NoteType.Click)
                {
                    foreach (int highlightedID in highlighted)
                    {
                        CurrentChart.NoteList[highlightedID].Type = targetType;

                        int dragParent = GetDragParent(highlightedID);

                        if (dragParent > -1)
                        {
                            CurrentChart.NoteList[dragParent].NextID = -1;
                        }

                        if (CurrentChart.NoteList[highlightedID].NextID > 0)
                        {
                            Note note = CurrentChart.NoteList[CurrentChart.NoteList[highlightedID].NextID];
                            if (note.Type == (int) NoteType.CDragChild)
                            {
                                note.Type = (int) NoteType.CDragHead;
                            }
                            else if (note.Type == (int) NoteType.DragChild)
                            {
                                note.Type = (int) NoteType.DragHead;
                            }
                        }

                        CurrentChart.NoteList[highlightedID].NextID = -1;
                        CurrentChart.NoteList[highlightedID].HoldTick = 0;
                        CurrentChart.NoteList[highlightedID].DragID = -1;
                    }
                }
                else
                {
                    foreach (int highlightedID in highlighted)
                    {
                        int dragParent = GetDragParent(highlightedID);
                        if (dragParent > -1 && dragParent != highlighted[0])
                        {
                            CurrentChart.NoteList[dragParent].NextID = -1;
                        }
                    }

                    for (int i = 1; i + 1 < highlighted.Count; i++)
                    {
                        CurrentChart.NoteList[highlighted[i]].Type = targetType;

                        if (CurrentChart.NoteList[highlighted[i]].NextID > 0)
                        {
                            Note note = CurrentChart.NoteList[CurrentChart.NoteList[highlighted[i]].NextID];
                            if (note.Type == (int) NoteType.CDragChild)
                            {
                                note.Type = (int) NoteType.CDragHead;
                            }
                            else
                            {
                                note.Type = (int) NoteType.DragHead;
                            }
                        }

                        CurrentChart.NoteList[highlighted[i]].NextID = highlighted[i + 1];
                        CurrentChart.NoteList[highlighted[i]].HoldTick = 0;
                    }

                    CurrentChart.NoteList[highlighted[highlighted.Count - 1]].Type = targetType;
                    CurrentChart.NoteList[highlighted[highlighted.Count - 1]].HoldTick = 0;

                    if (targetType == (int) NoteType.CDragChild)
                    {
                        int id = CurrentChart.NoteList[highlighted.Count - 1].NextID;
                        while (id > 0)
                        {
                            CurrentChart.NoteList[id].Type = targetType;
                            id = CurrentChart.NoteList[id].NextID;
                        }
                    }
                    else
                    {
                        CurrentChart.NoteList[highlighted[highlighted.Count - 1]].NextID = -1;
                    }
                }
            }

            CalculateTimings();
            UpdateTime(CurrentPage.ActualStartTime);

            for (int i = 0; i < highlighted.Count; i++)
            {
                HighlightNoteWithID(highlighted[i]);
            }
        }

        private void HandleInput()
        {
            // TODO: break this down to reduce nesting
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 touchPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

                _isTouchHeld = true;

                if (CurrentTool == NoteType.None)
                {
#if UNITY_STANDALONE
                    _mouseStartPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    if (Math.Abs(_mouseStartPos.x) < PlayAreaWidth / 2 + 3 &&
                        Math.Abs(_mouseStartPos.y) < PlayAreaHeight / 2 + 2)
                    {
                        _selectionBox.SetActive(true);
                        _selectionBox.GetComponent<SpriteRenderer>().size = new Vector2(0, 0);
                        _mouseDragStarted = true;
                    }
#endif
                    var objectsToHighlight = new List<GameObject>();

                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                    {
                        if (obj.GetComponent<IHighlightable>().Highlighted &&
                            obj.GetComponent<NoteController>().Notetype ==
                            (int) NoteType.Hold) // Modifying hold_time for short holds
                        {
                            var holdNoteController = obj.GetComponent<HoldNoteController>();
                            int id = holdNoteController.NoteID;
                            if (holdNoteController.UpArrowCollider.OverlapPoint(touchPos))
                            {
                                CurrentChart.NoteList[id].HoldTick += CurrentChart.TimeBase / _beatDivisorValue;
                                if (CurrentChart.NoteList[id].HoldTick + CurrentChart.NoteList[id].Tick >
                                    CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex].EndTick)
                                {
                                    CurrentChart.NoteList[id].HoldTick =
                                        CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex].EndTick -
                                        CurrentChart.NoteList[id].Tick;
                                }

                                CalculateTimings();
                                UpdateTime(CurrentPage.ActualStartTime);
                                HighlightNoteWithID(id);
                            }
                            else if (holdNoteController.DownArrowCollider.OverlapPoint(touchPos))
                            {
                                CurrentChart.NoteList[id].HoldTick -= CurrentChart.TimeBase / _beatDivisorValue;
                                if (CurrentChart.NoteList[id].HoldTick < 0)
                                {
                                    CurrentChart.NoteList[id].HoldTick = 1;
                                }

                                CalculateTimings();
                                UpdateTime(CurrentPage.ActualStartTime);
                                HighlightNoteWithID(id);
                            }
                        }
                        else if (obj.GetComponent<IHighlightable>().Highlighted &&
                                 obj.GetComponent<NoteController>().Notetype ==
                                 (int) NoteType.LongHold) // Modifying hold_time for long holds
                        {
                            var holdNoteController = obj.GetComponent<LongHoldNoteController>();
                            int id = holdNoteController.NoteID;
                            if (holdNoteController.UpArrowCollider.OverlapPoint(touchPos))
                            {
                                CurrentChart.NoteList[id].HoldTick += CurrentChart.TimeBase / _beatDivisorValue;

                                CalculateTimings();
                                UpdateTime(CurrentPage.ActualStartTime);
                                HighlightNoteWithID(id);
                            }
                            else if (holdNoteController.DownArrowCollider.OverlapPoint(touchPos))
                            {
                                CurrentChart.NoteList[id].HoldTick -= CurrentChart.TimeBase / _beatDivisorValue;

                                if (CurrentChart.NoteList[id].HoldTick < 0)
                                {
                                    CurrentChart.NoteList[id].HoldTick = 1;
                                }

                                CalculateTimings();
                                UpdateTime(CurrentPage.ActualStartTime);
                                HighlightNoteWithID(id);
                            }
                        }

                        if (obj.GetComponentInChildren<CircleCollider2D>()
                            .OverlapPoint(touchPos)) // Deciding which note to highlight
                        {
#if UNITY_STANDALONE
                            if (Input.GetKey(KeyCode.LeftShift))
                            {
                                objectsToHighlight.Add(obj);
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

                                objectsToHighlight.Add(obj);
                            }

                            continue;
#endif
                            objectsToHighlight.Add(obj);
                        }
                    }

                    int idToHighlight = -1;
                    bool onlyCurrentPage = false;
                    foreach (int id in objectsToHighlight.Select(toHighlight =>
                        toHighlight.GetComponent<NoteController>().NoteID))
                    {
                        if (CurrentChart.NoteList[id].PageIndex == CurrentPageIndex)
                        {
                            if (!onlyCurrentPage)
                            {
                                idToHighlight = id;
                            }

                            onlyCurrentPage = true;
                            idToHighlight = Math.Max(idToHighlight, id);
                        }
                        else if (!onlyCurrentPage)
                        {
                            idToHighlight = Math.Max(idToHighlight, id);
                        }
                    }

                    if (idToHighlight >= 0)
                    {
                        HighlightNoteWithID(idToHighlight);
                    }
                }
                else if (CurrentTool == NoteType.Move) // Starting the move of notes
                {
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                    {
                        int noteID = obj.GetComponent<NoteController>().NoteID;
                        if (!Config.InteractWithNotesOnOtherPages &&
                            CurrentChart.NoteList[noteID].PageIndex != CurrentPageIndex ||
                            !obj.GetComponentInChildren<Collider2D>().OverlapPoint(touchPos) ||
                            _currentlyMovingObject == obj ||
                            _isObjectMovingDict.ContainsKey(noteID))
                        {
                            continue;
                        }

                        if (obj.GetComponent<IHighlightable>().Highlighted)
                        {
                            _startMovePos = touchPos;
                            foreach (GameObject obj2 in GameObject.FindGameObjectsWithTag("Note"))
                            {
                                if (!Config.InteractWithNotesOnOtherPages &&
                                    CurrentChart.NoteList[obj2.GetComponent<NoteController>().NoteID].PageIndex !=
                                    CurrentPageIndex || !obj2.GetComponent<IHighlightable>().Highlighted)
                                {
                                    continue;
                                }

                                _movingNotes.Add(new MovingNote
                                {
                                    Object = obj2,
                                    NoteID = obj2.GetComponent<NoteController>().NoteID,
                                    ReferencePosition = obj2.transform.position
                                });
                                _isObjectMovingDict.Add(obj2.GetComponent<NoteController>().NoteID, true);
                            }

                            _movingNotes.Sort((a, b) => a.NoteID.CompareTo(b.NoteID));
                            _currentlyMovingObject = null;
                            break;
                        }

                        _currentlyMovingObject = obj;
                        _lockedY = _currentlyMovingObject.transform.position.y;
                        _movingNotes.Clear();
                        _isObjectMovingDict.Clear();
                    }

                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("ScanlineNote"))
                    {
                        if (obj.GetComponent<ITempo>().TempoID != 0 &&
                            obj.GetComponentInChildren<Collider2D>().OverlapPoint(touchPos))
                        {
                            _currentlyMovingObject = obj;
                            break;
                        }
                    }
                }
                else if (CurrentChart != null && touchPos.x < PlayAreaWidth / 2 && touchPos.x > -PlayAreaWidth / 2 &&
                         touchPos.y < PlayAreaHeight / 2 && touchPos.y > -PlayAreaHeight / 2)
                    // Adding notes
                {
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                    {
                        if (obj.GetComponent<NoteController>().Notetype == (int) CurrentTool &&
                            Math.Abs(touchPos.y - obj.transform.position.y) < PlayAreaHeight / _beatDivisorValue / 2
                            && Math.Abs(touchPos.x - obj.transform.position.x) < PlayAreaHeight / _beatDivisorValue / 2)
                        {
                            return;
                        }
                    }

                    double noteX = (touchPos.x + PlayAreaWidth / 2) / PlayAreaWidth;
                    if (Config.HorizontalSnap)
                    {
                        noteX =
                            Math.Round((touchPos.x + PlayAreaWidth / 2) / (PlayAreaWidth / Config.VerticalDivisors)) /
                            Config.VerticalDivisors;
                    }
                    else
                    {
                        noteX = Math.Round(noteX, 2);
                    }

                    if (CurrentTool == NoteType.Click) // Add click note
                    {
                        AddNote(new Note
                        {
                            X = noteX,
                            PageIndex = CurrentPageIndex,
                            Type = (int)NoteType.Click,
                            ID = -1,
                            HoldTick = 0,
                            NextID = 0,
                            Tick = (int)GetTickForTouchPosition(touchPos)
                        });

                        CalculateTimings();
                        UpdateTime(CurrentPage.ActualStartTime);
                    }
                    else if (CurrentTool == NoteType.Hold || CurrentTool == NoteType.LongHold) // Add hold notes
                    {
                        int tick = (int)GetTickForTouchPosition(touchPos);

                        if (!Config.ShowApproachingNotesWhilePaused && tick == CurrentPage.EndTick)
                        {
                            return;
                        }
                        
                        Note note = new Note
                        {
                            X = noteX,
                            PageIndex = CurrentPageIndex + (tick == CurrentPage.EndTick ? 1 : 0),
                            Type = (int)CurrentTool,
                            ID = -1,
                            HoldTick = CurrentChart.TimeBase / _beatDivisorValue,
                            NextID = 0,
                            Tick = tick
                        };


                        StartCoroutine(HandleHoldNoteDrag(touchPos, note));

                        
                    }
                    else if (CurrentTool == NoteType.Flick) // Add flick note
                    {
                        AddNote(new Note
                        {
                            X = noteX,
                            PageIndex = CurrentPageIndex,
                            Type = (int) NoteType.Flick,
                            ID = -1,
                            HoldTick = 0,
                            NextID = 0,
                            Tick = (int) GetTickForTouchPosition(touchPos)
                        });

                        CalculateTimings();
                        UpdateTime(CurrentPage.ActualStartTime);
                    }
                    else if (CurrentTool == NoteType.DragHead) // Add drag head and child
                    {
                        bool existsHighlightedDragHead = false;
                        int idToHighlight = -1;
                        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                        {
                            int noteType = obj.GetComponent<NoteController>().Notetype;
                            int noteID = obj.GetComponent<NoteController>().NoteID;
                            if (obj.GetComponent<IHighlightable>().Highlighted &&
                                CurrentChart.NoteList[noteID].NextID == -1 && (noteType == (int) NoteType.DragHead ||
                                                                               noteType == (int) NoteType.DragChild))
                                // Add drag child
                            {
                                int tick = (int) GetTickForTouchPosition(touchPos);

                                if (CurrentChart.NoteList[noteID].Tick < tick)
                                {
                                    int id = AddNote(new Note
                                    {
                                        X = noteX,
                                        PageIndex = CurrentPageIndex,
                                        Type = (int) NoteType.DragChild,
                                        ID = -1,
                                        HoldTick = 0,
                                        NextID = -1,
                                        Tick = tick,
                                        DragID = CurrentChart.NoteList[noteID].DragID
                                    });
                                    idToHighlight = id;
                                    CurrentChart.NoteList[noteID].NextID = id;
                                }

                                existsHighlightedDragHead = true;
                                break;
                            }
                        }

                        if (!existsHighlightedDragHead) // Add drag head
                        {
                            idToHighlight = AddNote(new Note
                            {
                                X = noteX,
                                PageIndex = CurrentPageIndex,
                                Type = (int) NoteType.DragHead,
                                ID = -1,
                                HoldTick = 0,
                                NextID = -1,
                                Tick = (int) GetTickForTouchPosition(touchPos),
                                DragID = _currentDragID + 1
                            });
                            _currentDragID++;
                        }

                        CalculateTimings();
                        UpdateTime(CurrentPage.ActualStartTime);
                        HighlightNoteWithID(idToHighlight);
                    }
                    else if (CurrentTool == NoteType.CDragHead) // Add cdrag head and child
                    {
                        bool existsHighlightedDragHead = false;
                        int idToHighlight = -1;
                        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                        {
                            int noteID = obj.GetComponent<NoteController>().NoteID;
                            int noteType = obj.GetComponent<NoteController>().Notetype;
                            if (obj.GetComponent<IHighlightable>().Highlighted &&
                                CurrentChart.NoteList[noteID].NextID == -1 && (noteType == (int) NoteType.CDragHead ||
                                                                               noteType == (int) NoteType.CDragChild))
                                // Add cdrag child
                            {
                                int tick = (int) GetTickForTouchPosition(touchPos);

                                if (CurrentChart.NoteList[obj.GetComponent<NoteController>().NoteID].Tick < tick)
                                {
                                    int id = AddNote(new Note
                                    {
                                        X = noteX,
                                        PageIndex = CurrentPageIndex,
                                        Type = (int) NoteType.CDragChild,
                                        ID = -1,
                                        HoldTick = 0,
                                        NextID = -1,
                                        Tick = tick,
                                        DragID = CurrentChart.NoteList[noteID].Tick
                                    });
                                    idToHighlight = id;
                                    CurrentChart.NoteList[obj.GetComponent<NoteController>().NoteID].NextID = id;
                                }

                                existsHighlightedDragHead = true;
                                break;
                            }
                        }

                        if (!existsHighlightedDragHead) // Add cdrag head
                        {
                            idToHighlight = AddNote(new Note
                            {
                                X = noteX,
                                PageIndex = CurrentPageIndex,
                                Type = (int) NoteType.CDragHead,
                                ID = -1,
                                HoldTick = 0,
                                NextID = -1,
                                Tick = (int) GetTickForTouchPosition(touchPos),
                                DragID = _currentDragID + 1
                            });
                            _currentDragID++;
                        }

                        CalculateTimings();
                        UpdateTime(CurrentPage.ActualStartTime);
                        HighlightNoteWithID(idToHighlight);
                    }
                    else if (CurrentTool == NoteType.Scanline) // Add scanline/tempo note
                    {
                        AddTempo(new Tempo
                        {
                            Tick = (int) GetTickForTouchPosition(touchPos),
                            Value = CurrentChart.TempoList[_currentTempoIndex].Value
                        });

                        CalculateTimings();
                        UpdateTime(CurrentPage.ActualStartTime);
                    }

                    GameObject.Find("NoteCountText").GetComponent<Text>().text =
                        $"Note count: {CurrentChart.NoteList.Count}";
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
#if UNITY_STANDALONE
                if (_mouseDragStarted)
                {
                    _selectionBox.SetActive(false);

                    Vector2 pos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

                    if (CurrentChart != null && GetDistance(pos.x, pos.y, _mouseStartPos.x, _mouseStartPos.y) > 0.1)
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

                        bool deselectAll = true;
                        float lx = Math.Min(pos.x, _mouseStartPos.x),
                            rx = Math.Max(pos.x, _mouseStartPos.x),
                            uy = Math.Max(pos.y, _mouseStartPos.y),
                            dy = Math.Min(pos.y, _mouseStartPos.y);
                        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                        {
                            if (obj.transform.position.x >= lx && obj.transform.position.x <= rx &&
                                obj.transform.position.y >= dy && obj.transform.position.y <= uy &&
                                !obj.GetComponent<IHighlightable>().Highlighted)
                            {
                                deselectAll = false;
                                HighlightObject(obj);
                            }
                        }

                        if (deselectAll)
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

                    _mouseDragStarted = false;
                }
#endif
                if (_currentlyMovingObject != null) // Finish the move and update the chart
                {
                    Vector3 objectPosition = _currentlyMovingObject.transform.position;
                    if (objectPosition.x > PlayAreaWidth / 2 + 2 || objectPosition.x < -PlayAreaWidth / 2 - 2)
                    {
                        if (_currentlyMovingObject.CompareTag("ScanlineNote"))
                        {
                            RemoveTempo(_currentlyMovingObject.GetComponent<ITempo>().TempoID);
                            Destroy(_currentlyMovingObject);
                        }
                        else
                        {
                            RemoveNote(_currentlyMovingObject.GetComponent<NoteController>().NoteID);
                            _objectPool.ReturnToPool(_currentlyMovingObject,
                                _currentlyMovingObject.GetComponent<NoteController>().Notetype);
                        }

                        CalculateTimings();
                        UpdateTime(CurrentPage.ActualStartTime);
                    }
                    else
                    {
                        _currentlyMovingObject.transform.position = new Vector3(
                            Clamp(objectPosition.x, -PlayAreaWidth / 2, PlayAreaWidth / 2),
                            _lockY ? _lockedY : Clamp(objectPosition.y, -PlayAreaHeight / 2, PlayAreaHeight / 2));


                        if (_currentlyMovingObject.CompareTag("Note"))
                        {
                            int id = _currentlyMovingObject.GetComponent<NoteController>().NoteID;
                            Note note = CurrentChart.NoteList[id];

                            float newX = _currentlyMovingObject.transform.position.x;
                            if (Config.HorizontalSnap)
                            {
                                newX = (float) Math.Round(
                                        (_currentlyMovingObject.transform.position.x + PlayAreaWidth / 2) /
                                        (PlayAreaWidth / Config.VerticalDivisors)) *
                                    (PlayAreaWidth / Config.VerticalDivisors) - PlayAreaWidth / 2;
                            }

                            Vector3 newPosition = new Vector3(newX,
                                (float) Math.Round((_currentlyMovingObject.transform.position.y + PlayAreaHeight / 2) /
                                                   (PlayAreaHeight / _beatDivisorValue)) *
                                (PlayAreaHeight / _beatDivisorValue) - PlayAreaHeight / 2);

                            _currentlyMovingObject.transform.position = newPosition;
                            
                            note.X = (newPosition.x + PlayAreaWidth / 2) / PlayAreaWidth;
                            if (!Config.HorizontalSnap)
                            {
                                note.X = Math.Round(note.X, 2);
                            }

                            int tick = (int) Math.Round(CurrentChart.PageList[note.PageIndex].ActualStartTick +
                                                        CurrentChart.PageList[note.PageIndex].ActualPageSize *
                                                        (CurrentChart.PageList[note.PageIndex].ScanLineDirection == 1
                                                            ? (_currentlyMovingObject.transform.position.y +
                                                               PlayAreaHeight / 2) / PlayAreaHeight
                                                            : 1.0f - (newPosition.y + PlayAreaHeight / 2) /
                                                            PlayAreaHeight));
                            // TODO: make a conversion function for this ^

                            tick = Clamp(tick, CurrentChart.PageList[note.PageIndex].ActualStartTick,
                                CurrentChart.PageList[note.PageIndex].EndTick);

                            int dragParent = GetDragParent(note.ID);
                            if (dragParent > 0)
                            {
                                tick = Math.Max(tick, CurrentChart.NoteList[dragParent].Tick);
                            }

                            if (CurrentChart.NoteList[note.ID].NextID > 0)
                            {
                                tick = Math.Min(tick,
                                    CurrentChart.NoteList[CurrentChart.NoteList[note.ID].NextID].Tick);
                            }

                            note.Tick = tick;

                            if (note.Type == (int) NoteType.Hold &&
                                tick + note.HoldTick > CurrentChart.PageList[note.PageIndex].EndTick)
                            {
                                note.HoldTick = CurrentChart.PageList[note.PageIndex].EndTick - tick;
                            }

                            // Fixing ID
                            while (id < CurrentChart.NoteList.Count - 1 &&
                                   CurrentChart.NoteList[id].Tick > CurrentChart.NoteList[id + 1].Tick)
                            {
                                int pid1 = GetDragParent(id), pid2 = GetDragParent(id + 1);
                                if (pid1 >= 0)
                                {
                                    CurrentChart.NoteList[pid1].NextID++;
                                }

                                if (pid2 >= 0)
                                {
                                    CurrentChart.NoteList[pid2].NextID--;
                                }

                                Note aux = CurrentChart.NoteList[id];
                                CurrentChart.NoteList[id] = CurrentChart.NoteList[id + 1];
                                CurrentChart.NoteList[id + 1] = aux;
                                CurrentChart.NoteList[id].ID = id;
                                CurrentChart.NoteList[id + 1].ID = id + 1;
                                id++;
                            }

                            while (id > 0 && CurrentChart.NoteList[id].Tick < CurrentChart.NoteList[id - 1].Tick)
                            {
                                int pid1 = GetDragParent(id), pid2 = GetDragParent(id - 1);
                                if (pid1 >= 0)
                                {
                                    CurrentChart.NoteList[pid1].NextID--;
                                }

                                if (pid2 >= 0)
                                {
                                    CurrentChart.NoteList[pid2].NextID++;
                                }

                                Note aux = CurrentChart.NoteList[id];
                                CurrentChart.NoteList[id] = CurrentChart.NoteList[id - 1];
                                CurrentChart.NoteList[id - 1] = aux;
                                CurrentChart.NoteList[id].ID = id;
                                CurrentChart.NoteList[id - 1].ID = id - 1;
                                id--;
                            }
                        }
                        else if (_currentlyMovingObject.CompareTag("ScanlineNote"))
                        {
                            Vector3 position = _currentlyMovingObject.transform.position;

                            position = new Vector3(-PlayAreaWidth / 2 - 1,
                                (float) Math.Round((position.y + PlayAreaHeight / 2) /
                                                   (PlayAreaHeight / _beatDivisorValue)) *
                                (PlayAreaHeight / _beatDivisorValue) - PlayAreaHeight / 2);
                            _currentlyMovingObject.transform.position = position;

                            int id = _currentlyMovingObject.GetComponent<ITempo>().TempoID;
                            Tempo tempo = CurrentChart.TempoList[id];
                            tempo.Tick = (int) Math.Round(
                                (CurrentPage.ScanLineDirection == 1
                                    ? (position.y + PlayAreaHeight / 2) / PlayAreaHeight
                                    : 1.0 - (position.y + PlayAreaHeight / 2) / PlayAreaHeight)
                                * CurrentPage.ActualPageSize) + CurrentPage.ActualStartTick;

                            RemoveTempo(id);
                            AddTempo(tempo);
                        }

                        CalculateTimings();

                        UpdateTime(CurrentPage.ActualStartTime);
                    }

                    _currentlyMovingObject = null;
                }
                else if (_movingNotes.Count > 0) // finishing the move for multiple notes
                {
                    for (int i = 0; i < _movingNotes.Count; i++)
                    {
                        if (_movingNotes[i].Object.transform.position.x >= PlayAreaWidth / 2 + 2 ||
                            _movingNotes[i].Object.transform.position.x <= -PlayAreaWidth / 2 - 2) continue;

                        _movingNotes[i].Object.transform.position = new Vector3(
                            Clamp(_movingNotes[i].Object.transform.position.x, -PlayAreaWidth / 2, PlayAreaWidth / 2),
                            _lockY
                                ? _movingNotes[i].ReferencePosition.y
                                : Clamp(_movingNotes[i].Object.transform.position.y, -PlayAreaHeight / 2,
                                    PlayAreaHeight / 2));

                        float noteX = _movingNotes[i].Object.transform.position.x;
                        if (Config.HorizontalSnap)
                        {
                            noteX = (float) Math.Round((noteX + PlayAreaWidth / 2) /
                                                       (PlayAreaWidth / Config.VerticalDivisors)) *
                                (PlayAreaWidth / Config.VerticalDivisors) - PlayAreaWidth / 2;
                        }
                        else
                        {
                            noteX = (float) Math.Round(noteX, 2);
                        }

                        CurrentChart.NoteList[_movingNotes[i].NoteID].X = (noteX + PlayAreaWidth / 2) / PlayAreaWidth;

                        _movingNotes[i].Object.transform.position = new Vector3(noteX,
                            (float) Math.Round((_movingNotes[i].Object.transform.position.y + PlayAreaHeight / 2) /
                                               (PlayAreaHeight / _beatDivisorValue)) *
                            (PlayAreaHeight / _beatDivisorValue) - PlayAreaHeight / 2);

                        int pageindex = CurrentChart.NoteList[_movingNotes[i].NoteID].PageIndex;

                        int tick = (int) Math.Round(CurrentChart.PageList[pageindex].ActualStartTick +
                                                    CurrentChart.PageList[pageindex].ActualPageSize *
                                                    (CurrentChart.PageList[pageindex].ScanLineDirection == 1
                                                        ? (_movingNotes[i].Object.transform.position.y +
                                                           PlayAreaHeight / 2) / PlayAreaHeight
                                                        : 1.0f - (_movingNotes[i].Object.transform.position.y +
                                                                  PlayAreaHeight / 2) / PlayAreaHeight));

                        tick = Clamp(tick, CurrentChart.PageList[pageindex].ActualStartTick,
                            CurrentChart.PageList[pageindex].EndTick);

                        int dragParent = GetDragParent(_movingNotes[i].NoteID);
                        if (dragParent > 0)
                        {
                            tick = Math.Max(tick, CurrentChart.NoteList[dragParent].Tick);
                        }

                        if (CurrentChart.NoteList[_movingNotes[i].NoteID].NextID > 0)
                        {
                            tick = Math.Min(tick,
                                CurrentChart.NoteList[CurrentChart.NoteList[_movingNotes[i].NoteID].NextID].Tick);
                        }

                        CurrentChart.NoteList[_movingNotes[i].NoteID].Tick = tick;

                        Note note = CurrentChart.NoteList[_movingNotes[i].NoteID];
                        if (note.Type == (int) NoteType.Hold &&
                            tick + note.HoldTick > CurrentChart.PageList[note.PageIndex].EndTick)
                        {
                            note.HoldTick = CurrentChart.PageList[note.PageIndex].EndTick - tick;
                        }
                    }

                    var ids = new List<int>();

                    int deletedCount = 0;
                    for (int i = 0; i < _movingNotes.Count; i++)
                    {
                        if (_movingNotes[i].Object.transform.position.x > PlayAreaWidth / 2 + 2 ||
                            _movingNotes[i].Object.transform.position.x < -PlayAreaWidth / 2 - 2)
                        {
                            RemoveNote(_movingNotes[i].NoteID - deletedCount);
                            deletedCount++;
                        }
                        else
                        {
                            ids.Add(_movingNotes[i].NoteID - deletedCount);
                        }
                    }

                    // fixing IDs
                    for (int i = 0; i < ids.Count; i++)
                    {
                        int id = ids[i];
                        while (id < CurrentChart.NoteList.Count - 1 &&
                               CurrentChart.NoteList[id].Tick > CurrentChart.NoteList[id + 1].Tick)
                        {
                            int pid1 = GetDragParent(id), pid2 = GetDragParent(id + 1);
                            if (pid1 >= 0)
                            {
                                CurrentChart.NoteList[pid1].NextID++;
                            }

                            if (pid2 >= 0)
                            {
                                CurrentChart.NoteList[pid2].NextID--;
                            }

                            Note aux = CurrentChart.NoteList[id];
                            CurrentChart.NoteList[id] = CurrentChart.NoteList[id + 1];
                            CurrentChart.NoteList[id + 1] = aux;
                            CurrentChart.NoteList[id].ID = id;
                            CurrentChart.NoteList[id + 1].ID = id + 1;
                            id++;
                        }

                        while (id > 0 && CurrentChart.NoteList[id].Tick < CurrentChart.NoteList[id - 1].Tick)
                        {
                            int pid1 = GetDragParent(id), pid2 = GetDragParent(id - 1);
                            if (pid1 >= 0)
                            {
                                CurrentChart.NoteList[pid1].NextID--;
                            }

                            if (pid2 >= 0)
                            {
                                CurrentChart.NoteList[pid2].NextID++;
                            }

                            Note aux = CurrentChart.NoteList[id];
                            CurrentChart.NoteList[id] = CurrentChart.NoteList[id - 1];
                            CurrentChart.NoteList[id - 1] = aux;
                            CurrentChart.NoteList[id].ID = id;
                            CurrentChart.NoteList[id - 1].ID = id - 1;
                            id--;
                        }

                        ids[i] = id;
                    }

                    CalculateTimings();
                    UpdateTime(CurrentPage.ActualStartTime);

                    for (int i = 0; i < ids.Count; i++)
                    {
                        HighlightNoteWithID(ids[i]);
                    }

                    _movingNotes.Clear();
                }

                _isObjectMovingDict.Clear();
                _isTouchHeld = false;
            }

            if (_isTouchHeld && CurrentTool == NoteType.Move) // Handle moving notes
            {
                if (_currentlyMovingObject != null)
                {
                    Vector2 touchPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

                    _currentlyMovingObject.transform.position = new Vector3(touchPos.x, _lockY ? _lockedY : touchPos.y);
                }
                else if (_movingNotes.Count > 0)
                {
                    Vector2 touchPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

                    for (int i = 0; i < _movingNotes.Count; i++)
                    {
                        _movingNotes[i].Object.transform.position = new Vector3(
                            _movingNotes[i].ReferencePosition.x + (touchPos.x - _startMovePos.x),
                            _lockY
                                ? _movingNotes[i].ReferencePosition.y
                                : _movingNotes[i].ReferencePosition.y + (touchPos.y - _startMovePos.y));
                    }
                }
            }
        }

        private double GetTickForTouchPosition(Vector2 touchPos)
        {
            return (CurrentPage.ActualStartTick + CurrentPage.ActualPageSize *
                   (CurrentPage.ScanLineDirection == 1
                   ? Math.Round((touchPos.y + PlayAreaHeight / 2) 
                   / (PlayAreaHeight / _beatDivisorValue)) /
                   _beatDivisorValue
                   : 1.0f - Math.Round((touchPos.y + PlayAreaHeight / 2) /
                   (PlayAreaHeight / _beatDivisorValue)) / _beatDivisorValue));
        }

        private void HighlightNoteWithID(int id)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
            {
                if (obj.GetComponent<NoteController>().NoteID == id)
                {
                    HighlightObject(obj);
                }
            }
        }

        public void GoToPreviousPage()
        {
            if (!IsGameRunning && CurrentChart != null && CurrentPageIndex > 0)
            {
                CurrentPageIndex--;
                UpdateTime(CurrentPage.ActualStartTime);
            }
        }

        public void GoToNextPage()
        {
            if (!IsGameRunning && CurrentChart != null && CurrentPageIndex < CurrentChart.PageList.Count)
            {
                CurrentPageIndex++;
                UpdateTime(CurrentPage.ActualStartTime);
            }
        }


        IEnumerator HandleHoldNoteDrag(Vector2 startPos, Note note)
        {
            Vector2 currentPos = startPos;
            utilityLineRenderer.enabled = true;
            while (Input.GetMouseButton(0))
            {
                currentPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                utilityLineRenderer.SetPositions(new Vector3[] { startPos, new Vector2(startPos.x, currentPos.y) });
                yield return null;
            }
            int endTick = Mathf.Max(0, Math.Min((int)GetTickForTouchPosition(currentPos), CurrentPage.EndTick) - note.Tick);
            utilityLineRenderer.enabled = false;
            if (endTick > note.HoldTick)
            {
                note.HoldTick = endTick;
            }

            AddNote(note);

            CalculateTimings();
            UpdateTime(CurrentPage.ActualStartTime);
        }
        public void ChangeTempo(GameObject scanlineNote, bool updateOffset = false)
        {
            string bpmInput = scanlineNote.GetComponent<ScanlineNoteController>().BPMInputField.text;
            string timeInput = scanlineNote.GetComponent<ScanlineNoteController>().TimeInputField.text;
            int id = scanlineNote.GetComponent<ITempo>().TempoID;

            if (Double.TryParse(bpmInput, out double bpm))
            {
                CurrentChart.TempoList[id].Value = (long) Math.Round(120000000 / bpm);
            }

            if (updateOffset && Double.TryParse(timeInput, out double time) && id == 0)
            {
                CurrentChart.MusicOffset = -time;
            }

            CalculateTimings();
            UpdateTime(CurrentPage.ActualStartTime);
        }

        public void MoveByBeatSnap(int division, bool forward = true)
        {
            double nextPageTime = CurrentChart.PageList[CurrentPageIndex + 1].ActualStartTime;
            double pageStartTime = CurrentPage.ActualStartTime;
            double pageLength = nextPageTime - pageStartTime;
            double timeToMove = pageLength / (float) division;
            if (forward)
            {
                if (AudioManager.Time + timeToMove > nextPageTime)
                {
                    CurrentPageIndex++;
                    UpdateTime(CurrentPage.ActualStartTime);
                }
                else
                {
                    UpdateTime(AudioManager.Time + timeToMove);
                }
            }
            else
            {
                if (AudioManager.Time - timeToMove < pageStartTime)
                {
                    CurrentPageIndex--;
                    UpdateTime(CurrentPage.EndTime);
                }
                else
                {
                    UpdateTime(AudioManager.Time - timeToMove);
                }
            }
        }


        public void IncreasePlaybackSpeed()
        {
            if (CurrentChart == null || _playbackSpeedIndex == _playbackSpeeds.Length - 1 || IsGameRunning
                || _isStartScheduled)
                return;

            _playbackSpeedIndex++;
            UpdatePlaybackSpeed();
        }

        public void DecreasePlaybackSpeed()
        {
            if (CurrentChart == null || _playbackSpeedIndex == 0 || IsGameRunning || _isStartScheduled) return;

            _playbackSpeedIndex--;
            UpdatePlaybackSpeed();
        }

        private void UpdatePlaybackSpeed()
        {
            AudioManager.PlaybackSpeed = _playbackSpeeds[_playbackSpeedIndex];
            UpdateTime(CurrentPage.ActualStartTime);
            GameObject.Find("PlaybackSpeedText").GetComponent<Text>().text =
                $"{(int) (_playbackSpeeds[_playbackSpeedIndex] * 100)}%";
        }

        public void IncreaseOffset()
        {
            _saveEditorOffsetScheduledTime = Time.time + 0.75f;
            Config.UserOffset += Config.PreciseOffsetDelta ? 1 : 5;
            UpdateOffsetText();
        }

        public void DecreaseOffset()
        {
            _saveEditorOffsetScheduledTime = Time.time + 0.75f;
            Config.UserOffset -= Config.PreciseOffsetDelta ? 1 : 5;
            UpdateOffsetText();
        }

        private void UpdateOffsetText()
        {
            GameObject.Find("OffsetText").GetComponent<Text>().text = $"{Config.UserOffset}ms";
        }

        public void ChangeSweepDirection()
        {
            if (CurrentChart != null)
            {
                for (int i = CurrentPageIndex; i < CurrentChart.PageList.Count; i++)
                {
                    CurrentChart.PageList[i].ScanLineDirection = -CurrentChart.PageList[i].ScanLineDirection;
                }

                GameObject.Find("SweepChangeButton").GetComponentInChildren<Text>().text =
                    CurrentPage.ScanLineDirection == 1 ? "Up" : "Down";
                CalculateTimings();
                UpdateTime(CurrentPage.ActualStartTime);
            }
        }

        private void HighlightObject(GameObject obj)
        {
            var highlightInfo = obj.GetComponent<IHighlightable>();
            highlightInfo.Highlight();
            if (highlightInfo.Highlighted)
            {
                if (obj.CompareTag("Note"))
                {
                    Note note = CurrentChart.NoteList[obj.GetComponent<NoteController>().NoteID];
                    NotePropsManager.Add(note);

                    // Fix hold arrows overlapping with buttons/timeline
                    if (obj.GetComponent<NoteController>().Notetype == (int) NoteType.Hold ||
                        obj.GetComponent<NoteController>().Notetype == (int) NoteType.LongHold)
                    {
                        Bounds upBounds, downBounds;
                        if (obj.GetComponent<NoteController>().Notetype == (int) NoteType.LongHold)
                        {
                            upBounds = obj.GetComponent<LongHoldNoteController>().UpArrowCollider.bounds;
                            downBounds = obj.GetComponent<LongHoldNoteController>().DownArrowCollider.bounds;
                        }
                        else
                        {
                            upBounds = obj.GetComponent<HoldNoteController>().UpArrowCollider.bounds;
                            downBounds = obj.GetComponent<HoldNoteController>().DownArrowCollider.bounds;
                        }

                        var bounds = new Vector3[4];

                        GameObject.Find("LevelOptionsButton").GetComponent<RectTransform>().GetWorldCorners(bounds);
                        var bound = new Bounds(
                            new Vector3((bounds[0].x + bounds[1].x + bounds[2].x + bounds[3].x) / 4,
                                (bounds[0].y + bounds[1].y + bounds[2].y + bounds[3].y) / 4, upBounds.center.z),
                            new Vector3((bounds[3].x - bounds[0].x) / 2, (bounds[1].y - bounds[0].y) / 2, 3));
                        if (bound.Intersects(upBounds) || bound.Intersects(downBounds))
                        {
                            GameObject.Find("LevelOptionsButton").GetComponent<Button>().interactable = false;
                        }

                        GameObject.Find("EditorSettingsButton").GetComponent<RectTransform>().GetWorldCorners(bounds);
                        bound = new Bounds(
                            new Vector3((bounds[0].x + bounds[1].x + bounds[2].x + bounds[3].x) / 4,
                                (bounds[0].y + bounds[1].y + bounds[2].y + bounds[3].y) / 4, upBounds.center.z),
                            new Vector3((bounds[3].x - bounds[0].x) / 2, (bounds[1].y - bounds[0].y) / 2, 3));
                        if (bound.Intersects(upBounds) || bound.Intersects(downBounds))
                        {
                            GameObject.Find("EditorSettingsButton").GetComponent<Button>().interactable = false;
                        }

                        GameObject.Find("SaveButton").GetComponent<RectTransform>().GetWorldCorners(bounds);
                        bound = new Bounds(
                            new Vector3((bounds[0].x + bounds[1].x + bounds[2].x + bounds[3].x) / 4,
                                (bounds[0].y + bounds[1].y + bounds[2].y + bounds[3].y) / 4, upBounds.center.z),
                            new Vector3((bounds[3].x - bounds[0].x) / 2, (bounds[1].y - bounds[0].y) / 2, 3));
                        if (bound.Intersects(upBounds) || bound.Intersects(downBounds))
                        {
                            GameObject.Find("SaveButton").GetComponent<Button>().interactable = false;
                        }

                        GameObject.Find("Timeline").GetComponent<RectTransform>().GetWorldCorners(bounds);
                        bound = new Bounds(
                            new Vector3((bounds[0].x + bounds[1].x + bounds[2].x + bounds[3].x) / 4,
                                (bounds[0].y + bounds[1].y + bounds[2].y + bounds[3].y) / 4, upBounds.center.z),
                            new Vector3((bounds[3].x - bounds[0].x) / 2, (bounds[1].y - bounds[0].y) / 2, 3));
                        if (bound.Intersects(upBounds) || bound.Intersects(downBounds))
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
                    Note note = CurrentChart.NoteList[obj.GetComponent<NoteController>().NoteID];
                    NotePropsManager.Remove(note);
                }
            }

            if (NotePropsManager.IsEmpty)
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
            if (CurrentChart != null)
            {
                CurrentChart.EventOrderList.Clear();

                for (int i = 1; i < CurrentChart.TempoList.Count; i++)
                {
                    CurrentChart.EventOrderList.Add(new EventOrder
                    {
                        Tick = CurrentChart.TempoList[i].Tick - CurrentChart.TimeBase,
                        EventList = new List<EventOrder.Event>(1)
                    });
                    CurrentChart.EventOrderList[i - 1].EventList.Add(new EventOrder.Event
                    {
                        Type = CurrentChart.TempoList[i].Value > CurrentChart.TempoList[i - 1].Value ? 1 : 0,
                        Args = CurrentChart.TempoList[i].Value > CurrentChart.TempoList[i - 1].Value ? "G" : "R"
                    });
                }

                while (CurrentChart.NoteList.Count > 0 &&
                       CurrentChart.NoteList[CurrentChart.NoteList.Count - 1].PageIndex >= CurrentChart.PageList.Count)
                {
                    CurrentChart.NoteList.RemoveAt(CurrentChart.NoteList.Count - 1);
                }

                File.WriteAllText(Path.Combine(CurrentLevelPath, CurrentChart.Data.Path), JsonConvert.SerializeObject(
                    CurrentChart, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }));

                string levelDirPath = Path.Combine(Config.LevelStoragePath, CurrentLevel.ID);
                File.WriteAllText(Path.Combine(levelDirPath, "level.json"),
                    JsonConvert.SerializeObject(CurrentLevel, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented
                    }));

                GameObject.Find("ToastText").GetComponent<ToastMessageManager>().CreateToast("Saved chart!");
            }
        }

        public void CopySelection()
        {
            Clipboard.Clear();
            Clipboard.ReferencePageIndex = CurrentPageIndex;
            Clipboard.ReferenceTick = CurrentPage.ActualStartTick;
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
            {
                if (obj.GetComponent<IHighlightable>().Highlighted)
                {
                    int id = obj.GetComponent<NoteController>().NoteID;
                    Clipboard.Add(CurrentChart.NoteList[id]);
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
            if (notes.Count > 0)
            {
                foreach (Note note in notes)
                {
                    note.Tick += CurrentPage.StartTick - Clipboard.ReferenceTick;
                    note.PageIndex += CurrentPageIndex - Clipboard.ReferencePageIndex;
                    note.ID = -1;
                    if (note.Type == (int) NoteType.DragChild || note.Type == (int) NoteType.DragHead ||
                        note.Type == (int) NoteType.CDragChild || note.Type == (int) NoteType.CDragHead)
                    {
                        note.NextID = -1;
                        note.DragID += 1000005;
                    }

                    AddNote(note);
                    int j = note.ID + 1;
                    while (j < CurrentChart.NoteList.Count &&
                           CurrentChart.NoteList[j].Tick == CurrentChart.NoteList[note.ID].Tick)
                    {
                        if (CurrentChart.NoteList[j].X - CurrentChart.NoteList[note.ID].X < 0.01)
                        {
                            CurrentChart.NoteList[note.ID].X += 0.02;
                            break;
                        }

                        j++;
                    }
                }
            }

            FixDrags();
            CalculateTimings();
            UpdateTime(CurrentPage.ActualStartTime);
            foreach (Note note in notes)
            {
                HighlightNoteWithID(note.ID);
            }
        }

        private void FixDrags()
        {
            // TODO: get rid of this in favor of proper drag ID management
            var dragChains = new Dictionary<int, List<int>>();
            for (int i = 0; i < CurrentChart.NoteList.Count; i++)
            {
                if (CurrentChart.NoteList[i].DragID > 1000000)
                {
                    if (!dragChains.ContainsKey(CurrentChart.NoteList[i].DragID))
                    {
                        dragChains.Add(CurrentChart.NoteList[i].DragID, new List<int>());
                    }

                    dragChains[CurrentChart.NoteList[i].DragID].Add(i);
                }
            }

            foreach (List<int> chain in dragChains.Values)
            {
                chain.Sort();
                for (int i = 0; i + 1 < chain.Count; i++)
                {
                    CurrentChart.NoteList[chain[i]].NextID = chain[i + 1];
                }

                CurrentChart.NoteList[chain[chain.Count - 1]].NextID = -1;
            }
        }

        public void MirrorSelection()
        {
            if (IsGameRunning)
            {
                return;
            }

            var notes = new List<int>();
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
            {
                if (!obj.GetComponent<IHighlightable>().Highlighted) continue;

                int id = obj.GetComponent<NoteController>().NoteID;
                notes.Add(id);
                CurrentChart.NoteList[id].X = 1 - CurrentChart.NoteList[id].X;
            }

            UpdateTime(CurrentPage.ActualStartTime);
            foreach (int noteID in notes)
            {
                HighlightNoteWithID(noteID);
            }
        }

        private struct MovingNote
        {
            public GameObject Object;
            public int NoteID;
            public Vector2 ReferencePosition;
        }

        #region Prefabs for instantiating

        public GameObject ScanlineNotePrefab;
        public GameObject DivisorLinePrefab;

        #endregion

        #region Cached GameObjects

        private GameObject _playPauseButton;

        public NotePropertiesManager NotePropsManager;

        [SerializeField] private Text PageText;
        [SerializeField] private Text TimeText;
        [SerializeField] private Text BpmText;
        [SerializeField] private List<RectTransform> LeftNotchObstructedObjects;
        [SerializeField] private List<RectTransform> RightNotchObstructedObjects;

        public GameObject Scanline;

        public Slider Timeline;

        private GameObject _selectionBox;

        private readonly List<double> _hitsoundTimings = new List<double>();

        private struct NoteSpawnTime
        {
            public double Time;
            public int ID;
        }

        private readonly List<NoteSpawnTime> _noteSpawns = new List<NoteSpawnTime>();

        private static Camera _mainCamera;

        public Slider BeatDivisorSlider;

        #endregion

#if UNITY_STANDALONE
        private const double _nudgeDistance = 0.01;

        private bool _mouseDragStarted;
        private Vector2 _mouseStartPos = new Vector2(0, 0);

        private void HandlePCInput()
        {
            if (BlockInput)
            {
                return;
            }

            if (Input.GetMouseButton(0) && _mouseDragStarted)
            {
                Vector2 pos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                _selectionBox.transform.position =
                    new Vector2((pos.x + _mouseStartPos.x) / 2, (pos.y + _mouseStartPos.y) / 2);
                _selectionBox.GetComponent<SpriteRenderer>().size = new Vector2(Math.Abs(pos.x - _mouseStartPos.x),
                    Math.Abs(pos.y - _mouseStartPos.y));
            }

            var toHighlight = new List<Note>();

            if (WasPressed(HotkeyManager.MoveTool))
            {
                gameObject.GetComponent<SideButtonController>().HighlightButton(GameObject.Find("MoveNoteButton"));
            }
            else if (WasPressed(HotkeyManager.ClickNote))
            {
                gameObject.GetComponent<SideButtonController>().ChangeTool(NoteType.Click);
            }
            else if (WasPressed(HotkeyManager.HoldNote))
            {
                gameObject.GetComponent<SideButtonController>().ChangeTool(NoteType.Hold);
            }
            else if (WasPressed(HotkeyManager.LongHoldNote))
            {
                gameObject.GetComponent<SideButtonController>().ChangeTool(NoteType.LongHold);
            }
            else if (WasPressed(HotkeyManager.DragNote))
            {
                gameObject.GetComponent<SideButtonController>().ChangeTool(NoteType.DragHead);
            }
            else if (WasPressed(HotkeyManager.CDragNote))
            {
                gameObject.GetComponent<SideButtonController>().ChangeTool(NoteType.CDragHead);
            }
            else if (WasPressed(HotkeyManager.FlickNote))
            {
                gameObject.GetComponent<SideButtonController>().ChangeTool(NoteType.Flick);
            }
            else if (WasPressed(HotkeyManager.ScanlineNote))
            {
                gameObject.GetComponent<SideButtonController>().ChangeTool(NoteType.Scanline);
            }

            else if (WasPressed(HotkeyManager.DecreasePlaybackSpeed))
            {
                DecreasePlaybackSpeed();
            }
            else if (WasPressed(HotkeyManager.IncreasePlaybackSpeed))
            {
                IncreasePlaybackSpeed();
            }

            if (CurrentChart == null)
            {
                return;
            }

            if (Input.GetMouseButtonDown(1))
            {
                Vector2 touchPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                bool toUpdate = false;
                var toRemove = new List<int>();

                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (obj.GetComponentInChildren<Collider2D>().OverlapPoint(touchPos))
                    {
                        toRemove.Add(obj.GetComponent<NoteController>().NoteID);
                        toUpdate = true;
                    }
                }

                if (toUpdate)
                {
                    toRemove.Sort();
                    for (int i = 0; i < toRemove.Count; i++)
                    {
                        RemoveNote(toRemove[i] - i);
                    }

                    UpdateTime(CurrentPage.StartTime);
                }
            }

            float axis = Input.GetAxis("Mouse ScrollWheel");

            if (Math.Abs(axis) > 0.05f)
            {
                int pageCount = (int) Math.Round(axis * 10, 1);
                CurrentPageIndex += pageCount;
                CurrentPageIndex = Clamp(CurrentPageIndex, 0, CurrentChart.PageList.Count - 1);
                UpdateTime(CurrentPage.ActualStartTime);
            }

            if (WasPressed(HotkeyManager.Copy))
            {
                CopySelection();
            }

            else if (WasPressed(HotkeyManager.Paste))
            {
                Paste();
            }

            else if (WasPressed(HotkeyManager.SelectAll))
            {
                bool allNotesOnCurrentPageSelected = true, noNotesSelected = true, allNotesSelected = true;
                GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");
                foreach (GameObject note in notes)
                {
                    bool highlighted = note.GetComponent<IHighlightable>().Highlighted;
                    if (!highlighted &&
                        CurrentChart.NoteList[note.GetComponent<NoteController>().NoteID].PageIndex == CurrentPageIndex)
                    {
                        allNotesOnCurrentPageSelected = false;
                    }
                    else if (highlighted)
                    {
                        noNotesSelected = false;
                    }

                    if (!highlighted)
                    {
                        allNotesSelected = false;
                    }
                }

                if (allNotesOnCurrentPageSelected && !allNotesSelected)
                {
                    foreach (GameObject note in notes)
                    {
                        if (!note.GetComponent<IHighlightable>().Highlighted)
                        {
                            HighlightObject(note);
                        }
                    }
                }
                else if (noNotesSelected)
                {
                    foreach (GameObject note in notes)
                    {
                        if (!note.GetComponent<IHighlightable>().Highlighted &&
                            CurrentChart.NoteList[note.GetComponent<NoteController>().NoteID].PageIndex ==
                            CurrentPageIndex)
                        {
                            HighlightObject(note);
                        }
                    }
                }
                else if (!noNotesSelected)
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

            else if (WasPressed(HotkeyManager.Mirror))
            {
                MirrorSelection();
            }

            else if (WasPressed(HotkeyManager.Flip))
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (!obj.GetComponent<IHighlightable>().Highlighted) continue;

                    int id = obj.GetComponent<NoteController>().NoteID;
                    toHighlight.Add(CurrentChart.NoteList[id]);
                    CurrentChart.NoteList[id].Tick = (int) Math.Round(
                        CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex].ActualStartTick +
                        CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex].ActualPageSize * (1.0 -
                            (CurrentChart.NoteList[id].Tick - CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex]
                                .ActualStartTick)
                            / CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex].ActualPageSize));
                    // TODO: conversion function
                }

                FixIDs();
                CalculateTimings();
                UpdateTime(CurrentPage.StartTime);
            }

            else if (WasPressed(HotkeyManager.NudgeLeft))
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (!obj.GetComponent<IHighlightable>().Highlighted) continue;

                    toHighlight.Add(CurrentChart.NoteList[obj.GetComponent<NoteController>().NoteID]);
                    CurrentChart.NoteList[obj.GetComponent<NoteController>().NoteID].X -= _nudgeDistance;
                }

                UpdateTime(CurrentPage.StartTime);
            }

            else if (WasPressed(HotkeyManager.NudgeRight))
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (!obj.GetComponent<IHighlightable>().Highlighted) continue;

                    toHighlight.Add(CurrentChart.NoteList[obj.GetComponent<NoteController>().NoteID]);
                    CurrentChart.NoteList[obj.GetComponent<NoteController>().NoteID].X += _nudgeDistance;
                }

                UpdateTime(CurrentPage.StartTime);
            }

            else if (WasPressed(HotkeyManager.NudgeUp))
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (!obj.GetComponent<IHighlightable>().Highlighted) continue;
                    
                    int id = obj.GetComponent<NoteController>().NoteID;
                    toHighlight.Add(CurrentChart.NoteList[id]);
                    if (CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex].ScanLineDirection > 0)
                    {
                        IncreaseTick(id);
                    }
                    else
                    {
                        DecreaseTick(id);
                    }
                }

                FixIDs();
                CalculateTimings();
                UpdateTime(CurrentPage.StartTime);
            }

            else if (WasPressed(HotkeyManager.NudgeDown))
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (!obj.GetComponent<IHighlightable>().Highlighted) continue;
                    
                    int id = obj.GetComponent<NoteController>().NoteID;
                    toHighlight.Add(CurrentChart.NoteList[id]);
                    if (CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex].ScanLineDirection > 0)
                    {
                        DecreaseTick(id);
                    }
                    else
                    {
                        IncreaseTick(id);
                    }
                }

                FixIDs();
                CalculateTimings();
                UpdateTime(CurrentPage.StartTime);
            }

            else if (WasPressed(HotkeyManager.IncreaseHoldTime))
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    var controller = obj.GetComponent<NoteController>();
                    if (!obj.GetComponent<IHighlightable>().Highlighted ||
                        (controller.Notetype != (int) NoteType.Hold && controller.Notetype != (int) NoteType.LongHold))
                        continue;
                    
                    int id = controller.NoteID;
                    int deltaTick = CurrentChart.TimeBase / _beatDivisorValue;
                    CurrentChart.NoteList[id].HoldTick += deltaTick;
                    if (controller.Notetype == (int) NoteType.Hold)
                    {
                        CurrentChart.NoteList[id].HoldTick = Math.Min(CurrentChart.NoteList[id].HoldTick,
                            CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex].EndTick -
                            CurrentChart.NoteList[id].Tick);
                    }

                    CalculateTimings();
                    UpdateTime(CurrentPage.ActualStartTime);
                    HighlightNoteWithID(controller.NoteID);
                }
            }

            else if (WasPressed(HotkeyManager.DecreaseHoldTime))
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    var controller = obj.GetComponent<NoteController>();
                    if (!obj.GetComponent<IHighlightable>().Highlighted ||
                        (controller.Notetype != (int) NoteType.Hold && controller.Notetype != (int) NoteType.LongHold))
                        continue;
                    
                    int id = controller.NoteID;
                    int deltaTick = CurrentChart.TimeBase / _beatDivisorValue;
                    CurrentChart.NoteList[id].HoldTick -= deltaTick;
                    CurrentChart.NoteList[id].HoldTick = Math.Max(CurrentChart.NoteList[id].HoldTick, 0);
                    CalculateTimings();
                    UpdateTime(CurrentPage.ActualStartTime);
                    HighlightNoteWithID(controller.NoteID);
                }
            }

            else if (WasPressed(HotkeyManager.PreviousPage))
            {
                GoToPreviousPage();
            }

            else if (WasPressed(HotkeyManager.NextPage))
            {
                GoToNextPage();
            }

            else if (WasPressed(HotkeyManager.BackToStart))
            {
                CurrentPageIndex = 0;
                UpdateTime(CurrentPage.ActualStartTime);
            }

            else if (WasPressed(HotkeyManager.Save))
            {
                SaveChart();
            }

            else if (WasPressed(HotkeyManager.Delete))
            {
                List<int> toRemove = 
                    (from obj in GameObject.FindGameObjectsWithTag("Note")
                    where obj.GetComponent<IHighlightable>().Highlighted
                    select obj.GetComponent<NoteController>().NoteID).ToList();

                toRemove.Sort();
                for (int i = 0; i < toRemove.Count; i++)
                {
                    RemoveNote(toRemove[i] - i);
                }

                UpdateTime(CurrentPage.ActualStartTime);
            }

            else if (WasPressed(HotkeyManager.LockY))
            {
                _lockY = !_lockY;
                _lockYText.SetActive(_lockY);
            }

            else if (WasPressed(HotkeyManager.ClickNoteTransform))
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (!obj.GetComponent<IHighlightable>().Highlighted) continue;
                    
                    int id = obj.GetComponent<NoteController>().NoteID;
                    CurrentChart.NoteList[id].Type = (int) NoteType.Click;
                    CurrentChart.NoteList[id].HoldTick = 0;
                    CurrentChart.NoteList[id].DragID = -1;

                    int dragParent = GetDragParent(id);
                    if (dragParent > -1)
                    {
                        CurrentChart.NoteList[dragParent].NextID = -1;
                    }

                    if (CurrentChart.NoteList[id].NextID > 0)
                    {
                        Note note = CurrentChart.NoteList[CurrentChart.NoteList[id].NextID];

                        if (note.Type == (int) NoteType.CDragChild)
                        {
                            note.Type = (int) NoteType.CDragHead;
                        }
                        else if (note.Type == (int) NoteType.DragChild)
                        {
                            note.Type = (int) NoteType.DragHead;
                        }
                    }

                    CurrentChart.NoteList[id].NextID = 0;
                }

                CalculateTimings();
                UpdateTime(CurrentPage.ActualStartTime);
            }

            else if (WasPressed(HotkeyManager.FlickNoteTransform))
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (!obj.GetComponent<IHighlightable>().Highlighted) continue;
                    
                    int id = obj.GetComponent<NoteController>().NoteID;
                    CurrentChart.NoteList[id].Type = (int) NoteType.Flick;
                    CurrentChart.NoteList[id].HoldTick = 0;
                    CurrentChart.NoteList[id].DragID = -1;

                    int dragParent = GetDragParent(id);
                    if (dragParent > -1)
                    {
                        CurrentChart.NoteList[dragParent].NextID = -1;
                    }

                    if (CurrentChart.NoteList[id].NextID > 0)
                    {
                        Note note = CurrentChart.NoteList[CurrentChart.NoteList[id].NextID];

                        if (note.Type == (int) NoteType.CDragChild)
                        {
                            note.Type = (int) NoteType.CDragHead;
                        }
                        else if (note.Type == (int) NoteType.DragChild)
                        {
                            note.Type = (int) NoteType.DragHead;
                        }
                    }

                    CurrentChart.NoteList[id].NextID = 0;
                }

                CalculateTimings();
                UpdateTime(CurrentPage.ActualStartTime);
            }

            else if (WasPressed(HotkeyManager.HoldNoteTransform))
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (!obj.GetComponent<IHighlightable>().Highlighted) continue;
                    
                    int id = obj.GetComponent<NoteController>().NoteID;
                    CurrentChart.NoteList[id].Type = (int) NoteType.Hold;
                    CurrentChart.NoteList[id].HoldTick = Math.Min(
                        (CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex].EndTick -
                         CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex].ActualStartTick) /
                        _beatDivisorValue,
                        CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex].EndTick -
                        CurrentChart.NoteList[id].Tick);
                    CurrentChart.NoteList[id].DragID = -1;

                    int dragParent = GetDragParent(id);
                    if (dragParent > -1)
                    {
                        CurrentChart.NoteList[dragParent].NextID = -1;
                    }

                    if (CurrentChart.NoteList[id].NextID > 0)
                    {
                        Note note = CurrentChart.NoteList[CurrentChart.NoteList[id].NextID];

                        if (note.Type == (int) NoteType.CDragChild)
                        {
                            note.Type = (int) NoteType.CDragHead;
                        }
                        else if (note.Type == (int) NoteType.DragChild)
                        {
                            note.Type = (int) NoteType.DragHead;
                        }
                    }

                    CurrentChart.NoteList[id].NextID = 0;
                }

                CalculateTimings();
                UpdateTime(CurrentPage.ActualStartTime);
            }

            else if (WasPressed(HotkeyManager.LongHoldNoteTransform))
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (!obj.GetComponent<IHighlightable>().Highlighted) continue;
                    
                    int id = obj.GetComponent<NoteController>().NoteID;
                    CurrentChart.NoteList[id].Type = (int) NoteType.LongHold;
                    CurrentChart.NoteList[id].HoldTick = Math.Min(
                        (CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex].EndTick -
                         CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex].ActualStartTick) /
                        _beatDivisorValue,
                        CurrentChart.PageList[CurrentChart.NoteList[id].PageIndex].EndTick -
                        CurrentChart.NoteList[id].Tick);
                    CurrentChart.NoteList[id].DragID = -1;

                    int dragParent = GetDragParent(id);
                    if (dragParent > -1)
                    {
                        CurrentChart.NoteList[dragParent].NextID = -1;
                    }

                    if (CurrentChart.NoteList[id].NextID > 0)
                    {
                        Note note = CurrentChart.NoteList[CurrentChart.NoteList[id].NextID];

                        if (note.Type == (int) NoteType.CDragChild)
                        {
                            note.Type = (int) NoteType.CDragHead;
                        }
                        else if (note.Type == (int) NoteType.DragChild)
                        {
                            note.Type = (int) NoteType.DragHead;
                        }
                    }

                    CurrentChart.NoteList[id].NextID = 0;
                }

                CalculateTimings();
                UpdateTime(CurrentPage.ActualStartTime);
            }

            else if (WasPressed(HotkeyManager.DragNoteTransform))
            {
                var ids = new List<int>();
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (!obj.GetComponent<IHighlightable>().Highlighted) continue;
                    
                    int id = obj.GetComponent<NoteController>().NoteID;
                    CurrentChart.NoteList[id].HoldTick = 0;
                    CurrentChart.NoteList[id].DragID = _currentDragID + 1;

                    int dragParent = GetDragParent(id);
                    if (dragParent > -1)
                    {
                        CurrentChart.NoteList[dragParent].NextID = -1;
                    }

                    ids.Add(id);
                }

                if (ids.Count > 1)
                {
                    CurrentChart.NoteList[ids[0]].Type = (int) NoteType.DragHead;
                    CurrentChart.NoteList[ids[0]].NextID = ids[1];
                    for (int i = 1; i + 1 < ids.Count; i++)
                    {
                        CurrentChart.NoteList[ids[i]].Type = (int) NoteType.DragChild;

                        if (CurrentChart.NoteList[ids[i]].NextID > 0)
                        {
                            Note note = CurrentChart.NoteList[CurrentChart.NoteList[ids[i]].NextID];
                            if (note.Type == (int) NoteType.CDragChild)
                            {
                                note.Type = (int) NoteType.CDragHead;
                            }
                            else
                            {
                                note.Type = (int) NoteType.DragHead;
                            }
                        }

                        CurrentChart.NoteList[ids[i]].NextID = ids[i + 1];
                    }

                    CurrentChart.NoteList[ids[ids.Count - 1]].Type = (int) NoteType.DragChild;

                    if (CurrentChart.NoteList[ids[ids.Count - 1]].NextID > 0)
                    {
                        int id = CurrentChart.NoteList[ids[ids.Count - 1]].NextID;

                        while (id > 0)
                        {
                            CurrentChart.NoteList[id].Type = (int) NoteType.DragChild;
                            id = CurrentChart.NoteList[id].NextID;
                        }
                    }
                    else
                    {
                        CurrentChart.NoteList[ids[ids.Count - 1]].NextID = -1;
                    }
                }

                CalculateTimings();
                UpdateTime(CurrentPage.ActualStartTime);
            }

            else if (WasPressed(HotkeyManager.CDragNoteTransform))
            {
                var ids = new List<int>();
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Note"))
                {
                    if (!obj.GetComponent<IHighlightable>().Highlighted) continue;
                    
                    int id = obj.GetComponent<NoteController>().NoteID;
                    CurrentChart.NoteList[id].HoldTick = 0;
                    CurrentChart.NoteList[id].DragID = _currentDragID + 1;

                    int dragParent = GetDragParent(id);
                    if (dragParent > -1)
                    {
                        CurrentChart.NoteList[dragParent].NextID = -1;
                    }

                    ids.Add(id);
                }

                if (ids.Count > 1)
                {
                    CurrentChart.NoteList[ids[0]].Type = (int) NoteType.CDragHead;
                    CurrentChart.NoteList[ids[0]].NextID = ids[1];
                    for (int i = 1; i + 1 < ids.Count; i++)
                    {
                        CurrentChart.NoteList[ids[i]].Type = (int) NoteType.CDragChild;

                        if (CurrentChart.NoteList[ids[i]].NextID > 0)
                        {
                            Note note = CurrentChart.NoteList[CurrentChart.NoteList[ids[i]].NextID];
                            if (note.Type == (int) NoteType.CDragChild)
                            {
                                note.Type = (int) NoteType.CDragHead;
                            }
                            else
                            {
                                note.Type = (int) NoteType.DragHead;
                            }
                        }

                        CurrentChart.NoteList[ids[i]].NextID = ids[i + 1];
                    }

                    CurrentChart.NoteList[ids[ids.Count - 1]].Type = (int) NoteType.CDragChild;

                    if (CurrentChart.NoteList[ids[ids.Count - 1]].NextID > 0)
                    {
                        int id = CurrentChart.NoteList[ids[ids.Count - 1]].NextID;

                        while (id > 0)
                        {
                            CurrentChart.NoteList[id].Type = (int) NoteType.CDragChild;
                            id = CurrentChart.NoteList[id].NextID;
                        }
                    }
                    else
                    {
                        CurrentChart.NoteList[ids[ids.Count - 1]].NextID = -1;
                    }
                }

                CalculateTimings();
                UpdateTime(CurrentPage.ActualStartTime);
            }
            else if (WasPressed(HotkeyManager.Undo))
                CommandSystem.Undo();
            else if (WasPressed(HotkeyManager.Redo))
                CommandSystem.Redo();

            foreach (Note noteToHighlight in toHighlight)
            {
                HighlightNoteWithID(noteToHighlight.ID);
            }
        }

        private static void IncreaseTick(int noteID)
        {
            Note note = CurrentChart.NoteList[noteID];
            Page p = CurrentChart.PageList[note.PageIndex];
            int deltaTick = (int) p.ActualPageSize / _beatDivisorValue;
            switch (note.Type)
            {
                case (int) NoteType.Hold when note.Tick + note.HoldTick + deltaTick <= p.EndTick:
                    note.Tick += deltaTick;
                    break;
                case (int) NoteType.Hold:
                    note.Tick += deltaTick;
                    note.HoldTick = p.EndTick - note.Tick;
                    break;
                case (int) NoteType.LongHold:
                    note.Tick += deltaTick;
                    break;
                default:
                {
                    if ((note.Type == (int) NoteType.DragHead || note.Type == (int) NoteType.DragChild ||
                         note.Type == (int) NoteType.CDragChild || note.Type == (int) NoteType.CDragHead)
                        && note.NextID > 0)
                    {
                        if (note.Tick + deltaTick <= Math.Min(p.EndTick, CurrentChart.NoteList[note.NextID].Tick))
                        {
                            note.Tick += deltaTick;
                        }
                        else
                        {
                            note.Tick = Math.Min(p.EndTick, CurrentChart.NoteList[note.NextID].Tick);
                        }
                    }
                    else
                    {
                        if (note.Tick + deltaTick <= p.EndTick)
                        {
                            note.Tick += deltaTick;
                        }
                        else
                        {
                            note.Tick = p.EndTick;
                        }
                    }

                    break;
                }
            }
        }

        private static void DecreaseTick(int noteID)
        {
            Note note = CurrentChart.NoteList[noteID];
            Page p = CurrentChart.PageList[note.PageIndex];
            int deltaTick = (int) p.ActualPageSize / _beatDivisorValue;
            
            if ((note.Type == (int) NoteType.DragHead || note.Type == (int) NoteType.DragChild ||
                 note.Type == (int) NoteType.CDragChild || note.Type == (int) NoteType.CDragHead)
                && GetDragParent(note.ID) > -1)
            {
                int parent = GetDragParent(note.ID);
                if (note.Tick - deltaTick >= Math.Max(p.ActualStartTick, CurrentChart.NoteList[parent].Tick))
                {
                    note.Tick -= deltaTick;
                }
                else
                {
                    note.Tick = Math.Max(p.ActualStartTick, CurrentChart.NoteList[parent].Tick);
                }
            }
            else
            {
                if (note.Tick - deltaTick >= p.ActualStartTick)
                {
                    note.Tick -= deltaTick;
                }
                else
                {
                    note.Tick = p.ActualStartTick;
                }
            }
        }

        private void FixIDs()
        {
            for (int i = 0; i < CurrentChart.NoteList.Count; i++)
            {
                int id = i;
                while (id < CurrentChart.NoteList.Count - 1 &&
                       CurrentChart.NoteList[id].Tick > CurrentChart.NoteList[id + 1].Tick)
                {
                    int pid1 = GetDragParent(id), pid2 = GetDragParent(id + 1);
                    if (pid1 >= 0)
                    {
                        CurrentChart.NoteList[pid1].NextID++;
                    }

                    if (pid2 >= 0)
                    {
                        CurrentChart.NoteList[pid2].NextID--;
                    }

                    Note aux = CurrentChart.NoteList[id];
                    CurrentChart.NoteList[id] = CurrentChart.NoteList[id + 1];
                    CurrentChart.NoteList[id + 1] = aux;
                    CurrentChart.NoteList[id].ID = id;
                    CurrentChart.NoteList[id + 1].ID = id + 1;
                    id++;
                }

                while (id > 0 && CurrentChart.NoteList[id].Tick < CurrentChart.NoteList[id - 1].Tick)
                {
                    int pid1 = GetDragParent(id), pid2 = GetDragParent(id - 1);
                    if (pid1 >= 0)
                    {
                        CurrentChart.NoteList[pid1].NextID--;
                    }

                    if (pid2 >= 0)
                    {
                        CurrentChart.NoteList[pid2].NextID++;
                    }

                    Note aux = CurrentChart.NoteList[id];
                    CurrentChart.NoteList[id] = CurrentChart.NoteList[id - 1];
                    CurrentChart.NoteList[id - 1] = aux;
                    CurrentChart.NoteList[id].ID = id;
                    CurrentChart.NoteList[id - 1].ID = id - 1;
                    id--;
                }
            }
        }

#endif
    }
}