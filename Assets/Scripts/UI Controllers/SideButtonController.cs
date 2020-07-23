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

    /// <summary>
    /// Highlights the pressed side button and changes the current note added or tool used.
    /// </summary>
    /// <param name="btn"> The button that was pressed. </param>
    public void HighlightButton(GameObject btn)
    {
        if(btn.Equals(highlightedButton))
        {
            highlightedButton.GetComponent<Image>().color = DefaultColor;
            highlightedButton = null;
            GameLogic.CurrentTool = NoteType.NONE;
        }
        else
        {
            if (highlightedButton != null)
            {
                highlightedButton.GetComponent<Image>().color = DefaultColor;
            }

            btn.GetComponent<Image>().color = HighlightedColor;
            highlightedButton = btn;

            switch (btn.tag)
            {
                case "Click":
                    GameLogic.CurrentTool = NoteType.CLICK;
                    break;
                case "Hold":
                    GameLogic.CurrentTool = NoteType.HOLD;
                    break;
                case "Long Hold":
                    GameLogic.CurrentTool = NoteType.LONG_HOLD;
                    break;
                case "Drag Head":
                    GameLogic.CurrentTool = NoteType.DRAG_HEAD;
                    break;
                case "CDrag Head":
                    GameLogic.CurrentTool = NoteType.CDRAG_HEAD;
                    break;
                case "Flick":
                    GameLogic.CurrentTool = NoteType.FLICK;
                    break;
                case "Move":
                    GameLogic.CurrentTool = NoteType.MOVE;
                    break;
                case "Settings":
                    GameLogic.CurrentTool = NoteType.SETTINGS;
                    break;
                default:
                    Debug.LogError("Fuck me");
                    break;
            }
        }
    }

}
