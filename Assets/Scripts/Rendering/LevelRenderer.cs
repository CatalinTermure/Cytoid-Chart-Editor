using System.Collections.Generic;
using CCE.Rendering.Notes;
using UnityEngine;

namespace CCE.Rendering
{
    public class LevelRenderer : MonoBehaviour
    {
        [SerializeField]
        private GameObject _clickNote;

        private List<SpriteRenderer> _noteFills;

        void Start()
        {
            Random.InitState(12345678);
            _noteFills = new List<SpriteRenderer>();
        }

        void Update()
        {
            GameObject note = Instantiate(_clickNote, new Vector3(Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f), 0.0f), Quaternion.identity);
            ClickNoteInfo noteInfo = note.GetComponent<ClickNoteInfo>();
            _noteFills.Add(noteInfo.NoteFill);

            for (int i = 0; i < _noteFills.Count; i++)
            {
                Color color = _noteFills[i].color;
                _noteFills[i].color = new Color(color.r, color.g, color.b, Random.Range(0.0f, 1.0f));
            }
        }
    }
}
