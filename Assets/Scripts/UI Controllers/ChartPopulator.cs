using UnityEngine;
using UnityEngine.UI;

public class ChartPopulator : MonoBehaviour
{
    public GameObject ChartListItem;

    private int listItemCount = 0;

    private static readonly Color NormalColor = new Color(1, 1, 1, 0.5f), HighlightColor = new Color(0.35f, 0.4f, 1f, 0.8f);

    private GameObject _currentChartItem = null;
    public GameObject CurrentChartItem
    {
        get => _currentChartItem;
        set
        {
            if (_currentChartItem != null)
            {
                _currentChartItem.GetComponent<Image>().color = NormalColor;
            }
            _currentChartItem = value;
            _currentChartItem.GetComponent<Image>().color = HighlightColor;
        }
    }

    private void AddChart(LevelData.ChartData chart)
    {
        GameObject obj = Instantiate(ChartListItem);
        (obj.transform as RectTransform).SetParent(gameObject.transform);
        (obj.transform as RectTransform).localPosition = new Vector3(0, -100 * listItemCount);
        (obj.transform as RectTransform).sizeDelta = new Vector2(800, 100);
        obj.transform.localScale = Vector3.one;
        obj.GetComponentInChildren<Text>().text = chart.name ?? chart.type;

        listItemCount++;
        (gameObject.transform as RectTransform).sizeDelta = new Vector2(0, 100 * listItemCount);

        obj.GetComponent<Button>().onClick.AddListener(() =>
        {
            CurrentChartItem = obj;
            GlobalState.LoadChart(chart);
        });
    }

    public void PopulateCharts(LevelData data)
    {
        // Delete already loaded charts
        listItemCount = 0;

        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }
        listItemCount = 0;
        (gameObject.transform as RectTransform).sizeDelta = new Vector2(0, 0);

        // Add an option to create chart
        GameObject obj = Instantiate(ChartListItem);
        (obj.transform as RectTransform).SetParent(gameObject.transform);
        (obj.transform as RectTransform).localPosition = new Vector3(0, -100 * listItemCount);
        (obj.transform as RectTransform).sizeDelta = new Vector2(800, 100);
        obj.transform.localScale = Vector3.one;
        obj.GetComponentInChildren<Text>().text = "Create a new chart";

        listItemCount++;
        (gameObject.transform as RectTransform).sizeDelta = new Vector2(0, 100 * listItemCount);

        obj.GetComponent<Button>().onClick.AddListener(() =>
        {
            CurrentChartItem = obj;
            GlobalState.CreateChart();
        });

        // Load charts
        foreach (var chart in data.charts)
        {
            AddChart(chart);
        }
    }
}
