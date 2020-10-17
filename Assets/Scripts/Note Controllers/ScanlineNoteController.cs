using System;
using UnityEngine;
using UnityEngine.UI;

public class ScanlineNoteController : MonoBehaviour, ITempo
{
    public int NoteType { get; set; }
    public int TempoID { get; set; }

    public InputField TimeInputField, BPMInputField;

    private void Start()
    {
        TimeInputField.onEndEdit.AddListener((_) => {
            if (GameLogic.CurrentTool != global::NoteType.MOVE)
            {
                GameLogic.BlockInput = false;
                GameObject.Find("UICanvas").GetComponent<GameLogic>().ChangeTempo(gameObject, true);
            }
        });
        BPMInputField.onEndEdit.AddListener((_) => {
            if (GameLogic.CurrentTool != global::NoteType.MOVE)
            {
                GameLogic.BlockInput = false;
                GameObject.Find("UICanvas").GetComponent<GameLogic>().ChangeTempo(gameObject);
            }
        });
    }

    public void SetPosition(Vector3 pos)
    {
        gameObject.transform.position = pos;
        TimeInputField.transform.position -= new Vector3(Screen.width / 2 * (-pos.x / GlobalState.Width), -(pos.y / GlobalState.Height) * Screen.height / 2f);
        BPMInputField.transform.position -= new Vector3(Screen.width / 2 * (-pos.x / GlobalState.Width), -(pos.y / GlobalState.Height) * Screen.height / 2f);
    }

    public void BlockGlobalInput()
    {
        GameLogic.BlockInput = true;
    }
}
