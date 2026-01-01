using CCE.Rendering;
using NUnit.Framework;

namespace CCE.Tests.Rendering
{
    public class ChartToScreenConverterTests
    {
        [Test]
        public void ClickNoteSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter();

            Assert.AreEqual(2.2345f, converter.ClickNoteSize, 0.001f);
        }

        [Test]
        public void FlickNoteSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter();

            Assert.AreEqual(2.5138f, converter.FlickNoteSize, 0.001f);
        }

        [Test]
        public void HoldNoteSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter();

            Assert.AreEqual(2.2345f, converter.HoldNoteSize, 0.001f);
        }

        [Test]
        public void LongHoldNoteSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter();

            Assert.AreEqual(2.2345f, converter.LongHoldNoteSize, 0.001f);
        }

        [Test]
        public void DragHeadNoteSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter();

            Assert.AreEqual(1.7876f, converter.DragHeadNoteSize, 0.001f);
        }
        [Test]
        public void CDragHeadNoteSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter();

            Assert.AreEqual(2.2345f, converter.CDragHeadNoteSize, 0.001f);
        }

        [Test]
        public void DragChildNoteSizeCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter();

            Assert.AreEqual(1.4524f, converter.DragChildNoteSize, 0.001f);
        }

        [Test]
        public void ScreenXFromChartXCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter();

            Assert.AreEqual(0.0f, converter.ScreenXFromChartX(0.5f), 0.01f);
            Assert.AreEqual(3.64f, converter.ScreenXFromChartX(0.75f), 0.01f);
            Assert.AreEqual(-3.64f, converter.ScreenXFromChartX(0.25f), 0.01f);
        }

        [Test]
        public void ScreenYFromChartYCorrect()
        {
            var converter = new ChartToScreenCoordinatesConverter();

            Assert.AreEqual(-0.23f, converter.ScreenYFromChartY(0.5f), 0.01f);
            Assert.AreEqual(1.52f, converter.ScreenYFromChartY(0.75f), 0.01f);
            Assert.AreEqual(-1.99f, converter.ScreenYFromChartY(0.25f), 0.01f);
        }
    }
}