using CCE.Data;
using System;
using System.Collections.Generic;
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
        [NonSerialized] public LevelPopulator Populator;

        private readonly List<LevelData> _levels = new List<LevelData>();
        private readonly List<string> _levelDescriptions = new List<string>();
        private string _lastQuery;

        private void Awake()
        {
            Behaviour = gameObject.GetComponent<LevelListBehaviour>();
            View = gameObject.GetComponent<LevelListView>();
            ChartCardController = gameObject.GetComponent<ChartCardController>();
        }

        private void Start()
        {
            Populator = new LevelPopulator(this);
            StartCoroutine(Populator.PopulateLevelsCoroutine());
        }

        public void AddLevel(LevelData level)
        {
            _levels.Add(level);

            _levelDescriptions.Add($"{level.ID} " +
                $"{level.Title} {level.TitleLocalized} " +
                $"{level.Artist} {level.ArtistLocalized} " +
                $"{level.Illustrator} " +
                $"{level.Charter} {level.Storyboarder}");
        }

        public void RemoveLevel(LevelData level)
        {
            int index = _levels.IndexOf(level);
            _levels.RemoveAt(index);
            _levelDescriptions.RemoveAt(index);
            
            View.RemoveLevel(level);
        }

        public void Query(string query)
        {
            if (_lastQuery == query) return;
            _lastQuery = query;

            string[] queryParts = query.Split(' ');
            List<LevelData> results = new List<LevelData>();
            
            for (int i = 0; i < _levelDescriptions.Count; i++)
            {
                bool containsAllQueryParts = true;

                foreach (string queryPart in queryParts)
                {
                    if (_levelDescriptions[i].IndexOf(queryPart,
                            StringComparison.InvariantCultureIgnoreCase) == -1)
                    {
                        containsAllQueryParts = false;
                        break;
                    }
                }

                if (containsAllQueryParts)
                {
                    results.Add(_levels[i]);
                }
            }

            View.Initialize(results);
        }
    }
}