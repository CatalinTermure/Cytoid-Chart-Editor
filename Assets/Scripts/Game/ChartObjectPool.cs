using System.Collections.Generic;
using System.Text;
using CCE.Data;
using UnityEngine;

namespace CCE.Game
{
    public class ChartObjectPool
    {
        #region Constants

        private const int ClickNotePoolSize = 24;
        private const int HoldNotePoolSize = 12;
        private const int LongHoldNotePoolSize = 8;
        private const int FlickNotePoolSize = 24;
        private const int DragHeadPoolSize = 4;
        private const int DragChildPoolSize = 48;
        private const int CdragHeadPoolSize = 4;
        private const int CdragChildPoolSize = 48;

        private static readonly int[] _poolSizes = new int[8]
        {
            ClickNotePoolSize,
            HoldNotePoolSize,
            LongHoldNotePoolSize,
            DragHeadPoolSize,
            DragChildPoolSize,
            FlickNotePoolSize,
            CdragHeadPoolSize,
            CdragChildPoolSize
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
            new Queue<GameObject>(ClickNotePoolSize),
            new Queue<GameObject>(HoldNotePoolSize),
            new Queue<GameObject>(LongHoldNotePoolSize),
            new Queue<GameObject>(DragHeadPoolSize),
            new Queue<GameObject>(DragChildPoolSize),
            new Queue<GameObject>(FlickNotePoolSize),
            new Queue<GameObject>(CdragHeadPoolSize),
            new Queue<GameObject>(CdragChildPoolSize)
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
