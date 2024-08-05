using CCE.Core;
using CCE.Game;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CCE.Notes
{
    public class ScanlineNoteController : MonoBehaviour, ITempo
    {
        public InputField TimeInputField;
        [FormerlySerializedAs("BPMInputField")] public InputField BpmInputField;
        public int NoteType { get; set; }

        private void Start()
        {
            TimeInputField.onEndEdit.AddListener(_ =>
            {
                if (GameLogic.CurrentTool != Data.NoteType.Move)
                {
                    GameLogic.BlockInput = false;
                    GameObject.Find("UICanvas").GetComponent<GameLogic>().ChangeTempo(gameObject, true);
                }
            });
            BpmInputField.onEndEdit.AddListener(_ =>
            {
                if (GameLogic.CurrentTool != Data.NoteType.Move)
                {
                    GameLogic.BlockInput = false;
                    GameObject.Find("UICanvas").GetComponent<GameLogic>().ChangeTempo(gameObject);
                }
            });
        }

        public int TempoID { get; set; }

        public void SetPosition(Vector3 pos)
        {
            gameObject.transform.position = pos;
            TimeInputField.transform.position -=
                new Vector3(Screen.width / 2 * (-pos.x / ((float)Screen.width / Screen.height * GlobalState.Height)),
                    -(pos.y / GlobalState.Height) * Screen.height / 2f);
            BpmInputField.transform.position -=
                new Vector3(Screen.width / 2 * (-pos.x / ((float)Screen.width / Screen.height * GlobalState.Height)),
                    -(pos.y / GlobalState.Height) * Screen.height / 2f);
        }

        public void BlockGlobalInput() // added as a click event
        {
            if (GameLogic.CurrentTool != Data.NoteType.Move) GameLogic.BlockInput = true;
        }
    }
}