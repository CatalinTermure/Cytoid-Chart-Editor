using System.Collections.Generic;
using System.Text;
using CCE.Data;
using CCE.Rendering.Notes;
using UnityEngine;

namespace CCE.Rendering
{
    public class PoolSizes
    {
        public int ClickNotePoolSize;
        public int HoldNotePoolSize;
        public int LongHoldNotePoolSize;
        public int DragHeadPoolSize;
        public int DragChildPoolSize;
        public int FlickNotePoolSize;
        public int CDragHeadPoolSize;
        public int CDragChildPoolSize;
    }

    public class ChartObjectPool
    {
        private readonly List<int> _poolSizes;
        private readonly List<GameObject> _prefabs;
        private readonly List<Queue<GameObject>> _notePools;

        protected ChartObjectPool() { }

        public ChartObjectPool(NotePrefabs prefabs, PoolSizes poolSizes = null)
        {
            poolSizes ??= new PoolSizes
            {
                ClickNotePoolSize = ClickNotePoolSize,
                HoldNotePoolSize = HoldNotePoolSize,
                LongHoldNotePoolSize = LongHoldNotePoolSize,
                FlickNotePoolSize = FlickNotePoolSize,
                DragHeadPoolSize = DragHeadPoolSize,
                DragChildPoolSize = DragChildPoolSize,
                CDragHeadPoolSize = CDragHeadPoolSize,
                CDragChildPoolSize = CDragChildPoolSize
            };

            _poolSizes = new List<int>(new int[] {
                poolSizes.ClickNotePoolSize,
                poolSizes.HoldNotePoolSize,
                poolSizes.LongHoldNotePoolSize,
                poolSizes.DragHeadPoolSize,
                poolSizes.DragChildPoolSize,
                poolSizes.FlickNotePoolSize,
                poolSizes.CDragHeadPoolSize,
                poolSizes.CDragChildPoolSize
            });
            _prefabs = new List<GameObject>(new GameObject[] {
                prefabs.ClickNote,
                prefabs.HoldNote,
                prefabs.LongHoldNote,
                prefabs.DragHeadNote,
                prefabs.DragChildNote,
                prefabs.FlickNote,
                prefabs.CDragHeadNote,
                prefabs.DragChildNote
            });
            _notePools = new List<Queue<GameObject>>(_poolSizes.Count);
            for (var i = 0; i < _poolSizes.Count; i++)
            {
                _notePools.Add(new Queue<GameObject>(_poolSizes[i]));
            }

            InitializePool();
        }

        // TODO: see if making this virtual has any performance impact
        public virtual GameObject GetNote(NoteType type)
        {
            return _notePools[(int)type].Count > 0
                ? _notePools[(int)type].Dequeue()
                : Object.Instantiate(_prefabs[(int)type]);
        }

        // TODO: see if making this virtual has any performance impact
        public virtual void ReturnToPool(GameObject obj, NoteType type)
        {
            if (!obj)
            {
                return;
            }

            if (_notePools[(int)type].Count < _poolSizes[(int)type])
            {
                obj.SetActive(false);
                _notePools[(int)type].Enqueue(obj);
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
        private const int CDragHeadPoolSize = 16;
        private const int CDragChildPoolSize = 64;

        #endregion
    }
}