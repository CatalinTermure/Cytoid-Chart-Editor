using System;
using UnityEngine;
using UnityEngine.UI;

public class ScanlineNoteController : MonoBehaviour, INote
{
    public int NoteType { get; set; }
    public int NoteID { get; set; }

    public InputField TimeInputField, BPMInputField;

    private void Start()
    {
        TimeInputField.onEndEdit.AddListener((_) => GameObject.Find("UICanvas").GetComponent<GameLogic>().ChangeTempo(gameObject));
        BPMInputField.onEndEdit.AddListener((_) => GameObject.Find("UICanvas").GetComponent<GameLogic>().ChangeTempo(gameObject));
    }

    public void SetPosition(Vector3 pos)
    {
        gameObject.transform.position = pos;
        TimeInputField.transform.position -= new Vector3(Screen.width / 2 * (-pos.x / GlobalState.Width), -(pos.y / GlobalState.Height) * Screen.height / 2f);
        BPMInputField.transform.position -= new Vector3(Screen.width / 2 * (-pos.x / GlobalState.Width), -(pos.y / GlobalState.Height) * Screen.height / 2f);
    }
}
