using System.Collections.Generic;
using System.Text;
using CCE.Data;
using UnityEngine;

namespace CCE.Game
{
    public class ChartObjectPool
    {
        private static readonly int[] _poolSizes =
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

        private static readonly GameObject[] _prefabs =
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

        private readonly Queue<GameObject>[] _notePools =
        {
            new(ClickNotePoolSize),
            new(HoldNotePoolSize),
            new(LongHoldNotePoolSize),
            new(DragHeadPoolSize),
            new(DragChildPoolSize),
            new(FlickNotePoolSize),
            new(CdragHeadPoolSize),
            new(CdragChildPoolSize)
        };

        public ChartObjectPool()
        {
            InitializePool();
        }

        public GameObject GetNote(NoteType type)
        {
            return _notePools[(int)type].Count > 0
                ? _notePools[(int)type].Dequeue()
                : Object.Instantiate(_prefabs[(int)type]);
        }

        public void ReturnToPool(GameObject obj, int type)
        {
            if (!obj)
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
            for (var i = 0; i < 8; i++)
            {
                _prefabs[i].SetActive(false);
                _notePools[i].Clear();
                for (var j = 0; j < _poolSizes[i]; j++)
                {
                    _notePools[i].Enqueue(Object.Instantiate(_prefabs[i]));
                }
            }
        }

        public string GetPoolSizes()
        {
            var sb = new StringBuilder();
            sb.Append("Pools: ");
            for (var i = 0; i < 8; i++)
            {
                sb.Append(_notePools[i].Count);
                sb.Append(" ");
            }

            return sb.ToString();
        }

        #region Constants

        private const int ClickNotePoolSize = 24;
        private const int HoldNotePoolSize = 12;
        private const int LongHoldNotePoolSize = 8;
        private const int FlickNotePoolSize = 24;
        private const int DragHeadPoolSize = 16;
        private const int DragChildPoolSize = 64;
        private const int CdragHeadPoolSize = 16;
        private const int CdragChildPoolSize = 64;

        #endregion
    }
}