using System;
using UnityEngine;

namespace CCE.LevelLoading
{
    [RequireComponent(typeof(LevelListBehaviour))]
    [RequireComponent(typeof(LevelListView))]
    [RequireComponent(typeof(ChartCardController))]
    public class LevelList : MonoBehaviour
    {
        [NonSerialized] public LevelListBehaviour Behaviour;
        [NonSerialized] public LevelListView View;
        [NonSerialized] public ChartCardController ChartCardController;

        private void Awake()
        {
            Behaviour = gameObject.GetComponent<LevelListBehaviour>();
            View = gameObject.GetComponent<LevelListView>();
            ChartCardController = gameObject.GetComponent<ChartCardController>();
        }
    }
}