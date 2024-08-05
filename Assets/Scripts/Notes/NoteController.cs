using System;
using System.Diagnostics;
using CCE.Core;
using CCE.Data;
using CCE.Game;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.Notes
{
    public abstract class NoteController : MonoBehaviour, IHighlightable
    {
        /// <summary>
        ///     Time it takes from the note appearing on screen to its start time.
        /// </summary>
        [HideInInspector] public float ApproachTime;

        [HideInInspector] public float PlaybackSpeed;

        public GameObject HighlightBorder;

        [HideInInspector] public int Notetype, NoteID;

        protected float ApproachPercentage;

        /// <summary>
        ///     Time delay from the time the note should first appear to when the stopwatch starts.
        /// </summary>
        protected float Delay;

        private GameObject _infoText;

        private Text _infotxt;

        /// <summary>
        ///     Stopwatch for keeping track of the animation time.
        /// </summary>
        protected Stopwatch NoteStopwatch;

        public ChartObjectPool ParentPool;

        private void Awake()
        {
            Highlighted = true;
            Highlight();
        }

        private void Start()
        {
            if (GlobalState.IsGameRunning)
                UpdateVisuals();
            else
                ChangeToPausedVisuals();
        }

        private void Update()
        {
            if (GlobalState.IsGameRunning)
            {
                if (!NoteStopwatch.IsRunning)
                {
                    NoteStopwatch.Start();
                    Destroy(_infoText);
                }

                UpdateVisuals();
            }
            else
            {
                if (NoteStopwatch.IsRunning)
                {
                    NoteStopwatch.Stop();
                    ChangeToPausedVisuals();
                }

                _infoText.transform.position = gameObject.transform.position;
                UpdateInfoText();
            }
        }

        private void OnEnable()
        {
            _infoText = Instantiate(GameObject.Find("IDText"), GameObject.Find("OverlayCanvas").transform);
            _infoText.transform.position = gameObject.transform.position;
            _infotxt = _infoText.GetComponent<Text>();
        }

        private void OnDisable()
        {
            Destroy(_infoText);
        }

        public bool Highlighted { get; set; }

        public abstract void Highlight();

        public void SetDelay(float delay)
        {
            Delay = delay;
        }

        public void UpdateInfoText()
        {
            if (GameLogic.CurrentTool != NoteType.Move)
                _infotxt.text = "";
            else
                switch (GlobalState.ShownNoteInfo)
                {
                    case GlobalState.NoteInfo.NoteID:
                        _infotxt.text = NoteID.ToString();
                        break;
                    case GlobalState.NoteInfo.NoteX:
                        _infotxt.text =
                            (Math.Floor(GlobalState.CurrentChart.NoteList[NoteID].X * 100) / 100).ToString("F2");
                        break;
                    case GlobalState.NoteInfo.NoteY:
                        _infotxt.text = GlobalState.CurrentChart.NoteList[NoteID].Y.ToString("F2");
                        break;
                }
        }

        protected abstract void UpdateVisuals();
        protected abstract void ChangeToPausedVisuals();

        public abstract void ChangeNoteColor(Color color);

        public abstract void Initialize(Note note);
    }
}