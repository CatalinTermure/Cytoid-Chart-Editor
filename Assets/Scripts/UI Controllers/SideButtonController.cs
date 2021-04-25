using System.Collections;
using System.Collections.Generic;
using CCE.Data;
using CCE.Game;
using UnityEngine;
using UnityEngine.UI;

public class SideButtonController : MonoBehaviour
{
    /// <summary>
    /// The default color of side buttons.
    /// </summary>
    public Color DefaultColor;

    /// <summary>
    /// The color of side buttons when higlighted
    /// </summary>
    public Color HighlightedColor;

    [HideInInspector]
    public GameObject highlightedButton;

    public Sprite HoldNoteButtonSprite, LongHoldNoteButtonSprite, DragNoteButtonSprite, CDragNoteButtonSprite;

    /// <summary>
    /// Highlights the pressed side button and changes the current note added or tool used.
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
                GameLogic.CurrentTool = GameLogic.CurrentTool == NoteType.Scanline ? NoteType.None : NoteType.Scanline;
                break;
        }

        if (highlightedButton != null)
        {
            if (highlightedButton.CompareTag("Hold"))
            {
                highlightedButton.GetComponent<Image>().sprite = HoldNoteButtonSprite;
            }
            else if (highlightedButton.CompareTag("Drag Head"))
            {
                highlightedButton.GetComponent<Image>().sprite = DragNoteButtonSprite;
            }

            highlightedButton.GetComponent<Image>().color = DefaultColor;
        }

        switch (GameLogic.CurrentTool)
        {
            case NoteType.Click:
                highlightedButton = GameObject.Find("AddClickNoteButton");
                break;
            case NoteType.Hold:
                highlightedButton = GameObject.Find("AddHoldNoteButton");
                highlightedButton.GetComponent<Image>().sprite = HoldNoteButtonSprite;
                break;
            case NoteType.LongHold:
                highlightedButton = GameObject.Find("AddHoldNoteButton");
                highlightedButton.GetComponent<Image>().sprite = LongHoldNoteButtonSprite;
                break;
            case NoteType.DragHead:
                highlightedButton = GameObject.Find("AddDragNoteButton");
                highlightedButton.GetComponent<Image>().sprite = DragNoteButtonSprite;
                break;
            case NoteType.CDragHead:
                highlightedButton = GameObject.Find("AddDragNoteButton");
                highlightedButton.GetComponent<Image>().sprite = CDragNoteButtonSprite;
                break;
            case NoteType.Flick:
                highlightedButton = GameObject.Find("AddFlickNoteButton");
                break;
            case NoteType.Move:
                highlightedButton = GameObject.Find("MoveNoteButton");
                break;
            case NoteType.Scanline:
                highlightedButton = GameObject.Find("AddScanlineNoteButton");
                break;
            case NoteType.None:
                highlightedButton = null;
                break;
        }

        if (highlightedButton != null)
        {
            highlightedButton.GetComponent<Image>().color = HighlightedColor;
        }
    }


    public void ChangeTool(NoteType tool)
    {
        if (highlightedButton != null)
        {
            if (highlightedButton.CompareTag("Hold"))
            {
                highlightedButton.GetComponent<Image>().sprite = HoldNoteButtonSprite;
            }
            else if (highlightedButton.CompareTag("Drag Head"))
            {
                highlightedButton.GetComponent<Image>().sprite = DragNoteButtonSprite;
            }

            highlightedButton.GetComponent<Image>().color = DefaultColor;
        }

        if (tool == GameLogic.CurrentTool)
        {
            GameLogic.CurrentTool = NoteType.None;
            highlightedButton = null;
        }
        else
        {
            GameLogic.CurrentTool = tool;

            switch (GameLogic.CurrentTool)
            {
                case NoteType.Click:
                    highlightedButton = GameObject.Find("AddClickNoteButton");
                    break;
                case NoteType.Hold:
                    highlightedButton = GameObject.Find("AddHoldNoteButton");
                    highlightedButton.GetComponent<Image>().sprite = HoldNoteButtonSprite;
                    break;
                case NoteType.LongHold:
                    highlightedButton = GameObject.Find("AddHoldNoteButton");
                    highlightedButton.GetComponent<Image>().sprite = LongHoldNoteButtonSprite;
                    break;
                case NoteType.DragHead:
                    highlightedButton = GameObject.Find("AddDragNoteButton");
                    highlightedButton.GetComponent<Image>().sprite = DragNoteButtonSprite;
                    break;
                case NoteType.CDragHead:
                    highlightedButton = GameObject.Find("AddDragNoteButton");
                    highlightedButton.GetComponent<Image>().sprite = CDragNoteButtonSprite;
                    break;
                case NoteType.Flick:
                    highlightedButton = GameObject.Find("AddFlickNoteButton");
                    break;
                case NoteType.Move:
                    highlightedButton = GameObject.Find("MoveNoteButton");
                    break;
                case NoteType.Scanline:
                    highlightedButton = GameObject.Find("AddScanlineNoteButton");
                    break;
                case NoteType.None:
                    highlightedButton = null;
                    break;
            }

            if (highlightedButton != null)
            {
                highlightedButton.GetComponent<Image>().color = HighlightedColor;
            }
        }
    }

}
