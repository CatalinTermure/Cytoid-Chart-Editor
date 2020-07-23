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
        TimeInputField.transform.position -= new Vector3(700, - (pos.y / GlobalState.Height) * 540);
        BPMInputField.transform.position -= new Vector3(700, -(pos.y / GlobalState.Height) * 540);
    }
}
