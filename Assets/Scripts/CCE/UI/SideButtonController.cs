using CCE.Data;
using CCE.Game;
using UnityEngine;
using UnityEngine.UI;

namespace CCE.UI
{
    public class SideButtonController : MonoBehaviour
    {
        /// <summary>
        ///     The default color of side buttons.
        /// </summary>
        public Color DefaultColor;

        /// <summary>
        ///     The color of side buttons when higlighted
        /// </summary>
        public Color HighlightedColor;

        [HideInInspector] public GameObject HighlightedButton;

        public Sprite HoldNoteButtonSprite, LongHoldNoteButtonSprite, DragNoteButtonSprite, CDragNoteButtonSprite;

        /// <summary>
        ///     Highlights the pressed side button and changes the current note added or tool used.
        /// </summary>
        /// <param name="btn"> The button that was pressed. </param>
        public void HighlightButton(GameObject btn)
        {
            switch (btn.tag)
            {
                case "Click":
                    GameLogic.CurrentTool = GameLogic.CurrentTool == NoteType.Click ? NoteType.None : NoteType.Click;
                    break;
                case "Hold":
                    if (GameLogic.CurrentTool == NoteType.Hold)
                    {
                        GameLogic.CurrentTool = NoteType.LongHold;
                    }
                    else if (GameLogic.CurrentTool == NoteType.LongHold)
                    {
                        GameLogic.CurrentTool = NoteType.None;
                    }
                    else
                    {
                        GameLogic.CurrentTool = NoteType.Hold;
                    }

                    break;
                case "Drag Head":
                    if (GameLogic.CurrentTool == NoteType.DragHead)
                    {
                        GameLogic.CurrentTool = NoteType.CDragHead;
                    }
                    else if (GameLogic.CurrentTool == NoteType.CDragHead)
                    {
                        GameLogic.CurrentTool = NoteType.None;
                    }
                    else
                    {
                        GameLogic.CurrentTool = NoteType.DragHead;
                    }

                    break;
                case "Flick":
                    GameLogic.CurrentTool = GameLogic.CurrentTool == NoteType.Flick ? NoteType.None : NoteType.Flick;
                    break;
                case "Move":
                    GameLogic.CurrentTool = GameLogic.CurrentTool == NoteType.Move ? NoteType.None : NoteType.Move;
                    break;
                case "Settings":
                    GameLogic.CurrentTool =
                        GameLogic.CurrentTool == NoteType.Scanline ? NoteType.None : NoteType.Scanline;
                    break;
            }

            if (HighlightedButton != null)
            {
                if (HighlightedButton.CompareTag("Hold"))
                {
                    HighlightedButton.GetComponent<Image>().sprite = HoldNoteButtonSprite;
                }
                else if (HighlightedButton.CompareTag("Drag Head"))
                {
                    HighlightedButton.GetComponent<Image>().sprite = DragNoteButtonSprite;
                }

                HighlightedButton.GetComponent<Image>().color = DefaultColor;
            }

            switch (GameLogic.CurrentTool)
            {
                case NoteType.Click:
                    HighlightedButton = GameObject.Find("AddClickNoteButton");
                    break;
                case NoteType.Hold:
                    HighlightedButton = GameObject.Find("AddHoldNoteButton");
                    HighlightedButton.GetComponent<Image>().sprite = HoldNoteButtonSprite;
                    break;
                case NoteType.LongHold:
                    HighlightedButton = GameObject.Find("AddHoldNoteButton");
                    HighlightedButton.GetComponent<Image>().sprite = LongHoldNoteButtonSprite;
                    break;
                case NoteType.DragHead:
                    HighlightedButton = GameObject.Find("AddDragNoteButton");
                    HighlightedButton.GetComponent<Image>().sprite = DragNoteButtonSprite;
                    break;
                case NoteType.CDragHead:
                    HighlightedButton = GameObject.Find("AddDragNoteButton");
                    HighlightedButton.GetComponent<Image>().sprite = CDragNoteButtonSprite;
                    break;
                case NoteType.Flick:
                    HighlightedButton = GameObject.Find("AddFlickNoteButton");
                    break;
                case NoteType.Move:
                    HighlightedButton = GameObject.Find("MoveNoteButton");
                    break;
                case NoteType.Scanline:
                    HighlightedButton = GameObject.Find("AddScanlineNoteButton");
                    break;
                case NoteType.None:
                    HighlightedButton = null;
                    break;
            }

            if (HighlightedButton != null)
            {
                HighlightedButton.GetComponent<Image>().color = HighlightedColor;
            }
        }


        public void ChangeTool(NoteType tool)
        {
            if (HighlightedButton != null)
            {
                if (HighlightedButton.CompareTag("Hold"))
                {
                    HighlightedButton.GetComponent<Image>().sprite = HoldNoteButtonSprite;
                }
                else if (HighlightedButton.CompareTag("Drag Head"))
                {
                    HighlightedButton.GetComponent<Image>().sprite = DragNoteButtonSprite;
                }

                HighlightedButton.GetComponent<Image>().color = DefaultColor;
            }

            if (tool == GameLogic.CurrentTool)
            {
                GameLogic.CurrentTool = NoteType.None;
                HighlightedButton = null;
            }
            else
            {
                GameLogic.CurrentTool = tool;

                switch (GameLogic.CurrentTool)
                {
                    case NoteType.Click:
                        HighlightedButton = GameObject.Find("AddClickNoteButton");
                        break;
                    case NoteType.Hold:
                        HighlightedButton = GameObject.Find("AddHoldNoteButton");
                        HighlightedButton.GetComponent<Image>().sprite = HoldNoteButtonSprite;
                        break;
                    case NoteType.LongHold:
                        HighlightedButton = GameObject.Find("AddHoldNoteButton");
                        HighlightedButton.GetComponent<Image>().sprite = LongHoldNoteButtonSprite;
                        break;
                    case NoteType.DragHead:
                        HighlightedButton = GameObject.Find("AddDragNoteButton");
                        HighlightedButton.GetComponent<Image>().sprite = DragNoteButtonSprite;
                        break;
                    case NoteType.CDragHead:
                        HighlightedButton = GameObject.Find("AddDragNoteButton");
                        HighlightedButton.GetComponent<Image>().sprite = CDragNoteButtonSprite;
                        break;
                    case NoteType.Flick:
                        HighlightedButton = GameObject.Find("AddFlickNoteButton");
                        break;
                    case NoteType.Move:
                        HighlightedButton = GameObject.Find("MoveNoteButton");
                        break;
                    case NoteType.Scanline:
                        HighlightedButton = GameObject.Find("AddScanlineNoteButton");
                        break;
                    case NoteType.None:
                        HighlightedButton = null;
                        break;
                }

                if (HighlightedButton != null)
                {
                    HighlightedButton.GetComponent<Image>().color = HighlightedColor;
                }
            }
        }
    }
}