using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ChartObjectPool
{
    #region Constants

    private const int CLICK_NOTE_POOL_SIZE = 24;
    private const int HOLD_NOTE_POOL_SIZE = 12;
    private const int LONG_HOLD_NOTE_POOL_SIZE = 8;
    private const int FLICK_NOTE_POOL_SIZE = 24;
    private const int DRAG_HEAD_POOL_SIZE = 4;
    private const int DRAG_CHILD_POOL_SIZE = 48;
    private const int CDRAG_HEAD_POOL_SIZE = 4;
    private const int CDRAG_CHILD_POOL_SIZE = 48;
    private const int DRAG_CONNECTOR_POOL_SIZE = 48;

    private static readonly int[] POOL_SIZES = new int[9]
    {
        CLICK_NOTE_POOL_SIZE,
        HOLD_NOTE_POOL_SIZE,
        LONG_HOLD_NOTE_POOL_SIZE,
        DRAG_HEAD_POOL_SIZE,
        DRAG_CHILD_POOL_SIZE,
        FLICK_NOTE_POOL_SIZE,
        CDRAG_HEAD_POOL_SIZE,
        CDRAG_CHILD_POOL_SIZE,
        DRAG_CONNECTOR_POOL_SIZE
    };

    #endregion

    private static readonly GameObject[] Prefabs = new GameObject[9]
    {
        (GameObject)Resources.Load("ClickNote"),
        (GameObject)Resources.Load("HoldNote"),
        (GameObject)Resources.Load("LongHoldNote"),
        (GameObject)Resources.Load("DragHeadNote"),
        (GameObject)Resources.Load("DragChildNote"),
        (GameObject)Resources.Load("FlickNote"),
        (GameObject)Resources.Load("CDragHead"),
        (GameObject)Resources.Load("DragChildNote"),
        (GameObject)Resources.Load("DragConnector")
    };

    private readonly Queue<GameObject>[] NotePools = new Queue<GameObject>[9] {
        new Queue<GameObject>(CLICK_NOTE_POOL_SIZE),
        new Queue<GameObject>(HOLD_NOTE_POOL_SIZE),
        new Queue<GameObject>(LONG_HOLD_NOTE_POOL_SIZE),
        new Queue<GameObject>(DRAG_HEAD_POOL_SIZE),
        new Queue<GameObject>(DRAG_CHILD_POOL_SIZE),
        new Queue<GameObject>(FLICK_NOTE_POOL_SIZE),
        new Queue<GameObject>(CDRAG_HEAD_POOL_SIZE),
        new Queue<GameObject>(CDRAG_CHILD_POOL_SIZE),
        new Queue<GameObject>(DRAG_CONNECTOR_POOL_SIZE)
    };

    public ChartObjectPool()
    {
        InitializePool();
    }

    public GameObject GetNote(NoteType type)
    {
        return NotePools[(int)type].Count > 0 ? NotePools[(int)type].Dequeue() : Object.Instantiate(Prefabs[(int)type]);
    }

    public GameObject GetDragConnector()
    {
        return NotePools[8].Count > 0 ? NotePools[8].Dequeue() : Object.Instantiate(Prefabs[8]);
    }

    public void ReturnToPool(GameObject obj, int type)
    {
        if(NotePools[type].Count < POOL_SIZES[type])
        {
            obj.SetActive(false);
            NotePools[type].Enqueue(obj);
        }
        else
        {
            Object.Destroy(obj);
        }
    }

    private void InitializePool()
    {
        for(int i = 0; i < 9; i++)
        {
            Prefabs[i].SetActive(false);
            NotePools[i].Clear();
            for(int j = 0; j < POOL_SIZES[i]; j++)
            {
                NotePools[i].Enqueue(Object.Instantiate(Prefabs[i]));
            }
        }
    }

    public string GetPoolSizes()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Pools: ");
        for(int i = 0; i < 9; i++)
        {
            sb.Append(NotePools[i].Count);
            sb.Append(" ");
        }
        return sb.ToString();
    }
}
