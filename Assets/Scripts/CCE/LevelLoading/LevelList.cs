using System;
using System.Collections.Generic;
using CCE.Data;
using UnityEngine;

namespace CCE.LevelLoading
{
    [RequireComponent(typeof(LevelListBehaviour))]
    [RequireComponent(typeof(LevelListView))]
    [RequireComponent(typeof(ChartCardController))]
    public class LevelList : MonoBehaviour
    {
        private readonly List<string> _levelDescriptions = new();

        private readonly List<LevelData> _levels = new();
        private string _lastQuery;
        [NonSerialized] public LevelListBehaviour Behaviour;
        [NonSerialized] public ChartCardController ChartCardController;
        [NonSerialized] public LevelPopulator Populator;
        [NonSerialized] public LevelListView View;

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
            var index = _levels.IndexOf(level);
            _levels.RemoveAt(index);
            _levelDescriptions.RemoveAt(index);

            View.RemoveLevel(level);
        }

        public void Query(string query)
        {
            if (_lastQuery == query && _lastQuery != "") return;
            _lastQuery = query;

            var queryParts = query.Split(' ');
            var results = new List<LevelData>();

            for (var i = 0; i < _levelDescriptions.Count; i++)
            {
                var containsAllQueryParts = true;

                foreach (var queryPart in queryParts)
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