using System.Collections;
using System.Collections.Generic;
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

    private GameObject highlightedButton;

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
                GameLogic.CurrentTool = GameLogic.CurrentTool == NoteType.CLICK ? NoteType.NONE : NoteType.CLICK;
                break;
            case "Hold":
                if(GameLogic.CurrentTool == NoteType.HOLD)
                {
                    GameLogic.CurrentTool = NoteType.LONG_HOLD;
                }
                else if(GameLogic.CurrentTool == NoteType.LONG_HOLD)
                {
                    GameLogic.CurrentTool = NoteType.NONE;
                }
                else
                {
                    GameLogic.CurrentTool = NoteType.HOLD;
                }
                break;
            case "Drag Head":
                if(GameLogic.CurrentTool == NoteType.DRAG_HEAD)
                {
                    GameLogic.CurrentTool = NoteType.CDRAG_HEAD;
                }
                else if(GameLogic.CurrentTool == NoteType.CDRAG_HEAD)
                {
                    GameLogic.CurrentTool = NoteType.NONE;
                }
                else
                {
                    GameLogic.CurrentTool = NoteType.DRAG_HEAD;
                }
                break;
            case "Flick":
                GameLogic.CurrentTool = GameLogic.CurrentTool == NoteType.FLICK ? NoteType.NONE : NoteType.FLICK;
                break;
            case "Move":
                GameLogic.CurrentTool = GameLogic.CurrentTool == NoteType.MOVE ? NoteType.NONE : NoteType.MOVE;
                break;
            case "Settings":
                GameLogic.CurrentTool = GameLogic.CurrentTool == NoteType.SCANLINE ? NoteType.NONE : NoteType.SCANLINE;
                break;
        }

        if(highlightedButton != null)
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

        switch(GameLogic.CurrentTool)
        {
            case NoteType.CLICK:
                highlightedButton = GameObject.Find("AddClickNoteButton");
                break;
            case NoteType.HOLD:
                highlightedButton = GameObject.Find("AddHoldNoteButton");
                highlightedButton.GetComponent<Image>().sprite = HoldNoteButtonSprite;
                break;
            case NoteType.LONG_HOLD:
                highlightedButton = GameObject.Find("AddHoldNoteButton");
                highlightedButton.GetComponent<Image>().sprite = LongHoldNoteButtonSprite;
                break;
            case NoteType.DRAG_HEAD:
                highlightedButton = GameObject.Find("AddDragNoteButton");
                highlightedButton.GetComponent<Image>().sprite = DragNoteButtonSprite;
                break;
            case NoteType.CDRAG_HEAD:
                highlightedButton = GameObject.Find("AddDragNoteButton");
                highlightedButton.GetComponent<Image>().sprite = CDragNoteButtonSprite;
                break;
            case NoteType.FLICK:
                highlightedButton = GameObject.Find("AddFlickNoteButton");
                break;
            case NoteType.MOVE:
                highlightedButton = GameObject.Find("MoveNoteButton");
                break;
            case NoteType.SCANLINE:
                highlightedButton = GameObject.Find("AddScanlineNoteButton");
                break;
            case NoteType.NONE:
                highlightedButton = null;
                break;
        }

        if(highlightedButton != null)
        {
            highlightedButton.GetComponent<Image>().color = HighlightedColor;
        }
    }

}
