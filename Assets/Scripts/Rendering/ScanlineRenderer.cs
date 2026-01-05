using CCE.Data;
using UnityEngine;

namespace CCE.Rendering
{
    public class ScanlineRenderer
    {
        private readonly Chart _chart;
        private readonly GameObject _scanline;
        private readonly LineRenderer _lineRenderer;
        private readonly IChartToScreenCoordinatesConverter _chartToScreenCoordinatesConverter;

        public ScanlineRenderer(Chart chart, GameObject scanlinePrefab, IChartToScreenCoordinatesConverter chartToScreenCoordinatesConverter)
        {
            _chart = chart;
            _chartToScreenCoordinatesConverter = chartToScreenCoordinatesConverter;

            Vector2 initialPosition = new(_chartToScreenCoordinatesConverter.ScreenXFromChartX(0.5),
                _chartToScreenCoordinatesConverter.ScreenYFromChartY(0));
            _scanline = Object.Instantiate(scanlinePrefab, initialPosition, Quaternion.identity);
            _lineRenderer = _scanline.GetComponent<LineRenderer>();
            float screenWidth = _chartToScreenCoordinatesConverter.AspectRatio * _chartToScreenCoordinatesConverter.ScreenSize;
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, new Vector3(-screenWidth / 2, 0, 0));
            _lineRenderer.SetPosition(1, new Vector3(screenWidth / 2, 0, 0));
        }

        public void UpdateTime(double time)
        {
            foreach (var page in _chart.PageList)
            {
                if (time >= page.ActualStartTime && time <= page.EndTime)
                {
                    Vector3 scanlinePosition = _scanline.transform.position;
                    float newY = (float)((time - page.ActualStartTime) / (page.EndTime - page.ActualStartTime));
                    if (page.ScanLineDirection == -1)
                    {
                        newY = 1.0f - newY;
                    }
                    scanlinePosition.y = _chartToScreenCoordinatesConverter.ScreenYFromChartY(newY);
                    _scanline.transform.position = scanlinePosition;
                    break;
                }
            }
        }
    }
}
