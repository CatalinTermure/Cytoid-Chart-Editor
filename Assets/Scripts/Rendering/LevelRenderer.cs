using System.Collections.Generic;
using UnityEngine;

namespace CCE
{
    public class LevelRenderer : MonoBehaviour
    {
        [SerializeField]
        private GameObject _clickNote;

        private List<SpriteRenderer> _noteFills;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Random.InitState(12345678);
            _noteFills = new List<SpriteRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            GameObject note = Instantiate(_clickNote, new Vector3(Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f), 0.0f), Quaternion.identity);
            SpriteRenderer noteFill = note.GetComponentsInChildren<SpriteRenderer>()[0];
            _noteFills.Add(noteFill);

            for (int i = 0; i < _noteFills.Count; i++)
            {
                Color color = _noteFills[i].color;
                _noteFills[i].color = new Color(color.r, color.g, color.b, Random.Range(0.0f, 1.0f));
            }
        }
    }
}
