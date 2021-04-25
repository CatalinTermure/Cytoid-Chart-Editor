using System.Collections.Generic;
using System.Text;
using CCE.Data;
using UnityEngine;

namespace CCE.Game
{
    public class ChartObjectPool
    {
        #region Constants

        private const int _clickNotePoolSize = 24;
        private const int _holdNotePoolSize = 12;
        private const int _longHoldNotePoolSize = 8;
        private const int _flickNotePoolSize = 24;
        private const int _dragHeadPoolSize = 4;
        private const int _dragChildPoolSize = 48;
        private const int _cdragHeadPoolSize = 4;
        private const int _cdragChildPoolSize = 48;

        private static readonly int[] _poolSizes = new int[8]
        {
            _clickNotePoolSize,
            _holdNotePoolSize,
            _longHoldNotePoolSize,
            _dragHeadPoolSize,
            _dragChildPoolSize,
            _flickNotePoolSize,
            _cdragHeadPoolSize,
            _cdragChildPoolSize
        };

        #endregion

        private static readonly GameObject[] _prefabs = new GameObject[8]
        {
            (GameObject)Resources.Load("ClickNote"),
            (GameObject)Resources.Load("HoldNote"),
            (GameObject)Resources.Load("LongHoldNote"),
            (GameObject)Resources.Load("DragHeadNote"),
            (GameObject)Resources.Load("DragChildNote"),
            (GameObject)Resources.Load("FlickNote"),
            (GameObject)Resources.Load("CDragHead"),
            (GameObject)Resources.Load("DragChildNote")
        };

        private readonly Queue<GameObject>[] _notePools = new Queue<GameObject>[8] {
            new Queue<GameObject>(_clickNotePoolSize),
            new Queue<GameObject>(_holdNotePoolSize),
            new Queue<GameObject>(_longHoldNotePoolSize),
            new Queue<GameObject>(_dragHeadPoolSize),
            new Queue<GameObject>(_dragChildPoolSize),
            new Queue<GameObject>(_flickNotePoolSize),
            new Queue<GameObject>(_cdragHeadPoolSize),
            new Queue<GameObject>(_cdragChildPoolSize)
        };

        public ChartObjectPool()
        {
            InitializePool();
        }

        public GameObject GetNote(NoteType type)
        {
            return _notePools[(int)type].Count > 0 ? _notePools[(int)type].Dequeue() : Object.Instantiate(_prefabs[(int)type]);
        }

        public void ReturnToPool(GameObject obj, int type)
        {
            if (obj == null)
            {
                return;
            }
            if (_notePools[type].Count < _poolSizes[type])
            {
                obj.SetActive(false);
                _notePools[type].Enqueue(obj);
            }
            else
            {
                Object.Destroy(obj);
            }
        }

        private void InitializePool()
        {
            for (int i = 0; i < 8; i++)
            {
                _prefabs[i].SetActive(false);
                _notePools[i].Clear();
                for (int j = 0; j < _poolSizes[i]; j++)
                {
                    _notePools[i].Enqueue(Object.Instantiate(_prefabs[i]));
                }
            }
        }

        public string GetPoolSizes()
        {
            var sb = new StringBuilder();
            sb.Append("Pools: ");
            for (int i = 0; i < 8; i++)
            {
                sb.Append(_notePools[i].Count);
                sb.Append(" ");
            }
            return sb.ToString();
        }
    }
}
