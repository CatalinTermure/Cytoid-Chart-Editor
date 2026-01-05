using CCE.Rendering;
using NUnit.Framework;
using UnityEngine;

namespace CCE.EditorTests
{
    public class ChartToScreenConverterTests
    {
        private static Rect GetDefaultPlayAreaRect()
        {
            float aspectRatio = 16.0f / 9.0f;
            float height = 10.0f;
            float width = aspectRatio * height;
            return new Rect(-width / 2.0f, -height / 2.0f, width, height);
        }

        private static Rect GetOffsetSmallPlayAreaRect()
        {
            float aspectRatio = 16.0f / 9.0f;
            float height = 5.0f;
            float width = aspectRatio * height;
            return new Rect(-width / 2.0f + 2.5f, -height / 2.0f, width, height);
        }

        [Test]
        public void ClickNoteSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetDefaultPlayAreaRect());

            Assert.AreEqual(2.2345f, converter.ClickNoteSize, 0.001f);
        }

        [Test]
        public void FlickNoteSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetDefaultPlayAreaRect());

            Assert.AreEqual(2.5138f, converter.FlickNoteSize, 0.001f);
        }

        [Test]
        public void HoldNoteSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetDefaultPlayAreaRect());

            Assert.AreEqual(2.2345f, converter.HoldNoteSize, 0.001f);
        }

        [Test]
        public void LongHoldNoteSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetDefaultPlayAreaRect());

            Assert.AreEqual(2.2345f, converter.LongHoldNoteSize, 0.001f);
        }

        [Test]
        public void DragHeadNoteSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetDefaultPlayAreaRect());

            Assert.AreEqual(1.7876f, converter.DragHeadNoteSize, 0.001f);
        }
        [Test]
        public void CDragHeadNoteSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetDefaultPlayAreaRect());

            Assert.AreEqual(2.2345f, converter.CDragHeadNoteSize, 0.001f);
        }

        [Test]
        public void DragChildNoteSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetDefaultPlayAreaRect());

            Assert.AreEqual(1.4524f, converter.DragChildNoteSize, 0.001f);
        }

        [Test]
        public void ScreenXFromChartXCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetDefaultPlayAreaRect());

            Assert.AreEqual(0.0f, converter.ScreenXFromChartX(0.5f), 0.01f);
            Assert.AreEqual(3.64f, converter.ScreenXFromChartX(0.75f), 0.01f);
            Assert.AreEqual(-3.64f, converter.ScreenXFromChartX(0.25f), 0.01f);
        }

        [Test]
        public void ScreenYFromChartYCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetDefaultPlayAreaRect());

            Assert.AreEqual(-0.23f, converter.ScreenYFromChartY(0.5f), 0.01f);
            Assert.AreEqual(1.52f, converter.ScreenYFromChartY(0.75f), 0.01f);
            Assert.AreEqual(-1.99f, converter.ScreenYFromChartY(0.25f), 0.01f);
        }

        [Test]
        public void ScreenSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetDefaultPlayAreaRect());

            Assert.AreEqual(10.0f, converter.ScreenSize, 0.01f);
        }

        [Test]
        public void ClickNoteSizeCorrect_WhenNonDefaultRect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetOffsetSmallPlayAreaRect());

            Assert.AreEqual(1.1173f, converter.ClickNoteSize, 0.001f);
        }

        [Test]
        public void FlickNoteSizeCorrect_WhenNonDefaultRect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetOffsetSmallPlayAreaRect());

            Assert.AreEqual(1.2569f, converter.FlickNoteSize, 0.001f);
        }

        [Test]
        public void HoldNoteSizeCorrect_WhenNonDefaultRect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetOffsetSmallPlayAreaRect());

            Assert.AreEqual(1.1173f, converter.HoldNoteSize, 0.001f);
        }

        [Test]
        public void LongHoldNoteSizeCorrect_WhenNonDefaultRect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetOffsetSmallPlayAreaRect());

            Assert.AreEqual(1.1173f, converter.LongHoldNoteSize, 0.001f);
        }

        [Test]
        public void DragHeadNoteSizeCorrect_WhenNonDefaultRect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetOffsetSmallPlayAreaRect());

            Assert.AreEqual(0.8938f, converter.DragHeadNoteSize, 0.001f);
        }

        [Test]
        public void CDragHeadNoteSizeCorrect_WhenNonDefaultRect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetOffsetSmallPlayAreaRect());

            Assert.AreEqual(1.1173f, converter.CDragHeadNoteSize, 0.001f);
        }

        [Test]
        public void DragChildNoteSizeCorrect_WhenNonDefaultRect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetOffsetSmallPlayAreaRect());

            Assert.AreEqual(0.7262f, converter.DragChildNoteSize, 0.001f);
        }

        [Test]
        public void ScreenXFromChartXCorrect_WhenNonDefaultRect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetOffsetSmallPlayAreaRect());

            Assert.AreEqual(2.5f, converter.ScreenXFromChartX(0.5f), 0.01f);
            Assert.AreEqual(4.32f, converter.ScreenXFromChartX(0.75f), 0.01f);
            Assert.AreEqual(0.68f, converter.ScreenXFromChartX(0.25f), 0.01f);
        }

        [Test]
        public void ScreenYFromChartYCorrect_WhenNonDefaultRect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetOffsetSmallPlayAreaRect());

            Assert.AreEqual(-0.06f, converter.ScreenYFromChartY(0.5f), 0.01f);
            Assert.AreEqual(0.82f, converter.ScreenYFromChartY(0.75f), 0.01f);
            Assert.AreEqual(-0.94f, converter.ScreenYFromChartY(0.25f), 0.01f);
        }

        [Test]
        public void ScreenSizeCorrect_WhenNonDefaultRect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetOffsetSmallPlayAreaRect());

            Assert.AreEqual(5.0f, converter.ScreenSize, 0.01f);
        }

        [Test]
        public void AspectRatioCorrect_DefaultRect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetDefaultPlayAreaRect());

            Assert.AreEqual(16.0f / 9.0f, converter.AspectRatio, 0.01f);
        }

        [Test]
        public void AspectRatioCorrect_NonDefaultRect()
        {
            var converter = new ChartToScreenCoordinatesConverter(GetOffsetSmallPlayAreaRect());

            Assert.AreEqual(16.0f / 9.0f, converter.AspectRatio, 0.01f);
        }

        [Test]
        public void AspectRatioCorrect_SquareRect()
        {
            var rect = new Rect(-5.0f, -5.0f, 10.0f, 10.0f);
            var converter = new ChartToScreenCoordinatesConverter(rect);

            Assert.AreEqual(1.0f, converter.AspectRatio, 0.01f);
        }
    }
}