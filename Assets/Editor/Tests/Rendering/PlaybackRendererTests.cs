using System.Collections.Generic;
using CCE.Rendering;
using CCE.Rendering.Notes;
using NUnit.Framework;
using UnityEngine;

namespace CCE.Tests.Rendering
{
    public class PlaybackRendererTests
    {
        private class FakeChartToScreenCoordinatesConverter : IChartToScreenCoordinatesConverter
        {
            public float ClickNoteSize => 1.1f;
            public float HoldNoteSize => 2.0f;
            public float LongHoldNoteSize => 3.0f;
            public float DragHeadNoteSize => 4.0f;
            public float DragChildNoteSize => 5.0f;
            public float FlickNoteSize => 6.0f;
            public float CDragHeadNoteSize => 7.0f;
            public float ScreenSize => 8.0f;

            public float ScreenXFromChartX(double chartX)
            {
                return (float)chartX * 9.0f;
            }

            public float ScreenYFromChartY(double chartY)
            {
                return (float)chartY * ScreenSize;
            }
        }

        private class FakeNoteProvider : INoteProvider
        {
            private readonly List<ClickNoteInfo> _clickNotes;
            private readonly List<FlickNoteInfo> _flickNotes;
            private readonly List<DragChildNoteInfo> _dragChildNotes;
            private readonly List<HoldNoteInfo> _holdNotes;
            private readonly List<LongHoldNoteInfo> _longHoldNotes;

            public FakeNoteProvider(List<ClickNoteInfo> clickNotes, List<FlickNoteInfo> flickNotes,
                        List<DragChildNoteInfo> dragChildNotes, List<HoldNoteInfo> holdNotes,
                        List<LongHoldNoteInfo> longHoldNotes)
            {
                _clickNotes = clickNotes;
                _flickNotes = flickNotes;
                _dragChildNotes = dragChildNotes;
                _holdNotes = holdNotes;
                _longHoldNotes = longHoldNotes;
            }

            public List<ClickNoteInfo> GetClickNotes()
            {
                return _clickNotes;
            }

            public List<FlickNoteInfo> GetFlickNotes()
            {
                return _flickNotes;
            }

            public List<DragChildNoteInfo> GetDragChildNotes()
            {
                return _dragChildNotes;
            }

            public List<HoldNoteInfo> GetHoldNotes()
            {
                return _holdNotes;
            }

            public List<LongHoldNoteInfo> GetLongHoldNotes()
            {
                return _longHoldNotes;
            }
        }

        [TestFixture]
        public class ClickNotes
        {
            private GameObject _clickNotePrefab;
            private FakeNoteProvider _fakeNoteProvider;
            private IChartToScreenCoordinatesConverter _chartToScreenCoordinatesConverter;
            private List<ClickNoteInfo> _clickNoteInfos;

            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                _clickNotePrefab = Resources.Load<GameObject>("Click Note New");
            }

            [SetUp]
            public void SetUp()
            {
                List<GameObject> clickNotes = new()
                {
                    Object.Instantiate(_clickNotePrefab, Vector3.zero, Quaternion.identity),
                    Object.Instantiate(_clickNotePrefab, Vector3.left, Quaternion.identity),
                    Object.Instantiate(_clickNotePrefab, Vector3.right, Quaternion.identity),
                };
                _clickNoteInfos = new List<ClickNoteInfo>();
                foreach (GameObject clickNote in clickNotes)
                {
                    _clickNoteInfos.Add(clickNote.GetComponent<ClickNoteInfo>());
                }
                _fakeNoteProvider = new FakeNoteProvider(_clickNoteInfos, new List<FlickNoteInfo>(),
                            new List<DragChildNoteInfo>(), new List<HoldNoteInfo>(),
                            new List<LongHoldNoteInfo>());
                _chartToScreenCoordinatesConverter = new FakeChartToScreenCoordinatesConverter();
            }

            [Test]
            public void CanCreateRenderer()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                Assert.IsNotNull(playbackRenderer);
            }

            [Test]
            public void RenderSetsClickNotesAbsoluteNoteSizeCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _clickNoteInfos[0].IntroTime = 0.0f;
                _clickNoteInfos[0].Time = 1.0f;
                _clickNoteInfos[0].Size = 1.0f;
                _clickNoteInfos[1].IntroTime = 0.0f;
                _clickNoteInfos[1].Time = 2.0f;
                _clickNoteInfos[1].Size = 1.0f;
                _clickNoteInfos[2].IntroTime = 0.0f;
                _clickNoteInfos[2].Time = 3.0f;
                _clickNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(1.0f, _clickNoteInfos[0].NoteTransform.localScale.x, 0.01f, "Note 0 X scale should be correct");
                Assert.AreEqual(1.0f, _clickNoteInfos[0].NoteTransform.localScale.y, 0.01f, "Note 0 Y scale should be correct");
                Assert.AreEqual(0.7f, _clickNoteInfos[1].NoteTransform.localScale.x, 0.01f, "Note 1 X scale should be correct");
                Assert.AreEqual(0.7f, _clickNoteInfos[1].NoteTransform.localScale.y, 0.01f, "Note 1 Y scale should be correct");
                Assert.AreEqual(0.30f, _clickNoteInfos[2].NoteTransform.localScale.x, 0.01f, "Note 2 X scale should be correct");
                Assert.AreEqual(0.30f, _clickNoteInfos[2].NoteTransform.localScale.y, 0.01f, "Note 2 Y scale should be correct");
            }

            [Test]
            public void RenderSetsClickNotePositionCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _clickNoteInfos[0].IntroTime = 0.0f;
                _clickNoteInfos[0].Time = 1.0f;
                _clickNoteInfos[0].Size = 1.0f;
                _clickNoteInfos[0].X = 7.1f;
                _clickNoteInfos[0].Y = 13.2f;
                _clickNoteInfos[1].IntroTime = 0.0f;
                _clickNoteInfos[1].Time = 2.0f;
                _clickNoteInfos[1].Size = 1.0f;
                _clickNoteInfos[1].X = 0.0f;
                _clickNoteInfos[1].Y = 0.0f;
                _clickNoteInfos[2].IntroTime = 0.0f;
                _clickNoteInfos[2].Time = 3.0f;
                _clickNoteInfos[2].Size = 0.5f;
                _clickNoteInfos[2].X = -10.0f;
                _clickNoteInfos[2].Y = -3.5f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(7.1f, _clickNoteInfos[0].NoteTransform.localPosition.x, 0.01f, "Note 0 X position should be correct");
                Assert.AreEqual(13.2f, _clickNoteInfos[0].NoteTransform.localPosition.y, 0.01f, "Note 0 Y position should be correct");
                Assert.AreEqual(0.0f, _clickNoteInfos[1].NoteTransform.localPosition.x, 0.01f, "Note 1 X position should be correct");
                Assert.AreEqual(0.0f, _clickNoteInfos[1].NoteTransform.localPosition.y, 0.01f, "Note 1 Y position should be correct");
                Assert.AreEqual(-10.0f, _clickNoteInfos[2].NoteTransform.localPosition.x, 0.01f, "Note 2 X position should be correct");
                Assert.AreEqual(-3.5f, _clickNoteInfos[2].NoteTransform.localPosition.y, 0.01f, "Note 2 Y position should be correct");
            }

            [Test]
            public void RenderSetsClickNotesFillSizeCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _clickNoteInfos[0].IntroTime = 0.0f;
                _clickNoteInfos[0].Time = 1.0f;
                _clickNoteInfos[1].IntroTime = 0.0f;
                _clickNoteInfos[1].Time = 2.0f;
                _clickNoteInfos[2].IntroTime = 0.0f;
                _clickNoteInfos[2].Time = 3.0f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(1.0f, _clickNoteInfos[0].NoteFillTransform.localScale.x, 0.01f, "Note 0 fill X scale should be correct");
                Assert.AreEqual(1.0f, _clickNoteInfos[0].NoteFillTransform.localScale.y, 0.01f, "Note 0 fill Y scale should be correct");
                Assert.AreEqual(0.5f, _clickNoteInfos[1].NoteFillTransform.localScale.x, 0.01f, "Note 1 fill X scale should be correct");
                Assert.AreEqual(0.5f, _clickNoteInfos[1].NoteFillTransform.localScale.y, 0.01f, "Note 1 fill Y scale should be correct");
                Assert.AreEqual(0.33f, _clickNoteInfos[2].NoteFillTransform.localScale.x, 0.01f, "Note 2 fill X scale should be correct");
                Assert.AreEqual(0.33f, _clickNoteInfos[2].NoteFillTransform.localScale.y, 0.01f, "Note 2 fill Y scale should be correct");
            }

            [Test]
            public void RenderSetsClickNotesOpacityCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _clickNoteInfos[0].IntroTime = 0.0f;
                _clickNoteInfos[0].Time = 1.0f;
                _clickNoteInfos[0].Opacity = 1.0f;
                _clickNoteInfos[1].IntroTime = 0.0f;
                _clickNoteInfos[1].Time = 2.0f;
                _clickNoteInfos[1].Opacity = 0.5f;
                _clickNoteInfos[2].IntroTime = 0.0f;
                _clickNoteInfos[2].Time = 3.0f;
                _clickNoteInfos[2].Opacity = 1.0f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(1.0f, _clickNoteInfos[0].NoteFill.color.a, 0.01f, "Note 0 fill opacity should be correct");
                Assert.AreEqual(1.0f, _clickNoteInfos[0].NoteRing.color.a, 0.01f, "Note 0 ring opacity should be correct");
                Assert.AreEqual(0.5f, _clickNoteInfos[1].NoteFill.color.a, 0.01f, "Note 1 fill opacity should be correct");
                Assert.AreEqual(0.5f, _clickNoteInfos[1].NoteRing.color.a, 0.01f, "Note 1 ring opacity should be correct");
                Assert.AreEqual(0.66f, _clickNoteInfos[2].NoteFill.color.a, 0.01f, "Note 2 fill opacity should be correct");
                Assert.AreEqual(0.66f, _clickNoteInfos[2].NoteRing.color.a, 0.01f, "Note 2 ring opacity should be correct");
            }
        }

        [TestFixture]
        public class FlickNotes
        {
            private GameObject _flickNotePrefab;
            private FakeNoteProvider _fakeNoteProvider;
            private IChartToScreenCoordinatesConverter _chartToScreenCoordinatesConverter;
            private List<FlickNoteInfo> _flickNoteInfos;

            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                _flickNotePrefab = Resources.Load<GameObject>("Flick Note New");
            }

            [SetUp]
            public void SetUp()
            {
                List<GameObject> flickNotes = new()
                {
                    Object.Instantiate(_flickNotePrefab, Vector3.zero, Quaternion.identity),
                    Object.Instantiate(_flickNotePrefab, Vector3.left, Quaternion.identity),
                    Object.Instantiate(_flickNotePrefab, Vector3.right, Quaternion.identity),
                };
                _flickNoteInfos = new List<FlickNoteInfo>();
                foreach (GameObject flickNote in flickNotes)
                {
                    _flickNoteInfos.Add(flickNote.GetComponent<FlickNoteInfo>());
                }
                _fakeNoteProvider = new FakeNoteProvider(new List<ClickNoteInfo>(), _flickNoteInfos,
                            new List<DragChildNoteInfo>(), new List<HoldNoteInfo>(),
                            new List<LongHoldNoteInfo>());
                _chartToScreenCoordinatesConverter = new FakeChartToScreenCoordinatesConverter();
            }

            [Test]
            public void RenderSetsFlickNotesAbsoluteNoteSizeCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _flickNoteInfos[0].IntroTime = 0.0f;
                _flickNoteInfos[0].Time = 1.0f;
                _flickNoteInfos[0].Size = 1.0f;
                _flickNoteInfos[1].IntroTime = 0.0f;
                _flickNoteInfos[1].Time = 2.0f;
                _flickNoteInfos[1].Size = 1.0f;
                _flickNoteInfos[2].IntroTime = 0.0f;
                _flickNoteInfos[2].Time = 3.0f;
                _flickNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(1.0f, _flickNoteInfos[0].NoteTransform.localScale.x, 0.01f, "Note 0 X scale should be correct");
                Assert.AreEqual(1.0f, _flickNoteInfos[0].NoteTransform.localScale.y, 0.01f, "Note 0 Y scale should be correct");
                Assert.AreEqual(0.7f, _flickNoteInfos[1].NoteTransform.localScale.x, 0.01f, "Note 1 X scale should be correct");
                Assert.AreEqual(0.7f, _flickNoteInfos[1].NoteTransform.localScale.y, 0.01f, "Note 1 Y scale should be correct");
                Assert.AreEqual(0.30f, _flickNoteInfos[2].NoteTransform.localScale.x, 0.01f, "Note 2 X scale should be correct");
                Assert.AreEqual(0.30f, _flickNoteInfos[2].NoteTransform.localScale.y, 0.01f, "Note 2 Y scale should be correct");
            }

            [Test]
            public void RenderSetsFlickNotePositionCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _flickNoteInfos[0].IntroTime = 0.0f;
                _flickNoteInfos[0].Time = 1.0f;
                _flickNoteInfos[0].X = 7.1f;
                _flickNoteInfos[0].Y = 13.2f;
                _flickNoteInfos[0].Size = 1.0f;
                _flickNoteInfos[1].IntroTime = 0.0f;
                _flickNoteInfos[1].Time = 2.0f;
                _flickNoteInfos[1].X = 0.0f;
                _flickNoteInfos[1].Y = 0.0f;
                _flickNoteInfos[1].Size = 1.0f;
                _flickNoteInfos[2].IntroTime = 0.0f;
                _flickNoteInfos[2].Time = 3.0f;
                _flickNoteInfos[2].X = -10.0f;
                _flickNoteInfos[2].Y = -3.5f;
                _flickNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(7.1f, _flickNoteInfos[0].NoteTransform.localPosition.x, 0.01f, "Note 0 X position should be correct");
                Assert.AreEqual(13.2f, _flickNoteInfos[0].NoteTransform.localPosition.y, 0.01f, "Note 0 Y position should be correct");
                Assert.AreEqual(0.0f, _flickNoteInfos[1].NoteTransform.localPosition.x, 0.01f, "Note 1 X position should be correct");
                Assert.AreEqual(0.0f, _flickNoteInfos[1].NoteTransform.localPosition.y, 0.01f, "Note 1 Y position should be correct");
                Assert.AreEqual(-10.0f, _flickNoteInfos[2].NoteTransform.localPosition.x, 0.01f, "Note 2 X position should be correct");
                Assert.AreEqual(-3.5f, _flickNoteInfos[2].NoteTransform.localPosition.y, 0.01f, "Note 2 Y position should be correct");
            }

            [Test]
            public void RenderSetsFlickNotesFillSizeCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _flickNoteInfos[0].IntroTime = 0.0f;
                _flickNoteInfos[0].Time = 1.0f;
                _flickNoteInfos[0].Size = 1.0f;
                _flickNoteInfos[1].IntroTime = 0.0f;
                _flickNoteInfos[1].Time = 2.0f;
                _flickNoteInfos[1].Size = 1.0f;
                _flickNoteInfos[2].IntroTime = 0.0f;
                _flickNoteInfos[2].Time = 3.0f;
                _flickNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(1.0f, _flickNoteInfos[0].NoteFillTransform.localScale.x, 0.01f, "Note 0 fill X scale should be correct");
                Assert.AreEqual(1.0f, _flickNoteInfos[0].NoteFillTransform.localScale.y, 0.01f, "Note 0 fill Y scale should be correct");
                Assert.AreEqual(0.5f, _flickNoteInfos[1].NoteFillTransform.localScale.x, 0.01f, "Note 1 fill X scale should be correct");
                Assert.AreEqual(0.5f, _flickNoteInfos[1].NoteFillTransform.localScale.y, 0.01f, "Note 1 fill Y scale should be correct");
                Assert.AreEqual(0.33f, _flickNoteInfos[2].NoteFillTransform.localScale.x, 0.01f, "Note 2 fill X scale should be correct");
                Assert.AreEqual(0.33f, _flickNoteInfos[2].NoteFillTransform.localScale.y, 0.01f, "Note 2 fill Y scale should be correct");
            }

            [Test]
            public void RenderSetsFlickNotesOpacityCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _flickNoteInfos[0].IntroTime = 0.0f;
                _flickNoteInfos[0].Time = 1.0f;
                _flickNoteInfos[0].Opacity = 1.0f;
                _flickNoteInfos[0].Size = 0.5f;
                _flickNoteInfos[1].IntroTime = 0.0f;
                _flickNoteInfos[1].Time = 2.0f;
                _flickNoteInfos[1].Opacity = 0.5f;
                _flickNoteInfos[1].Size = 0.5f;
                _flickNoteInfos[2].IntroTime = 0.0f;
                _flickNoteInfos[2].Time = 3.0f;
                _flickNoteInfos[2].Opacity = 1.0f;
                _flickNoteInfos[2].Size = 0.5f;


                playbackRenderer.Render(1.0f);

                Assert.AreEqual(1.0f, _flickNoteInfos[0].NoteFill.color.a, 0.01f, "Note 0 fill opacity should be correct");
                Assert.AreEqual(1.0f, _flickNoteInfos[0].NoteRing.color.a, 0.01f, "Note 0 ring opacity should be correct");
                Assert.AreEqual(1.0f, _flickNoteInfos[0].LeftArrow.color.a, 0.01f, "Note 0 left arrow opacity should be correct");
                Assert.AreEqual(1.0f, _flickNoteInfos[0].RightArrow.color.a, 0.01f, "Note 0 right arrow opacity should be correct");
                Assert.AreEqual(0.5f, _flickNoteInfos[1].NoteFill.color.a, 0.01f, "Note 1 fill opacity should be correct");
                Assert.AreEqual(0.5f, _flickNoteInfos[1].NoteRing.color.a, 0.01f, "Note 1 ring opacity should be correct");
                Assert.AreEqual(0.5f, _flickNoteInfos[1].LeftArrow.color.a, 0.01f, "Note 1 left arrow opacity should be correct");
                Assert.AreEqual(0.5f, _flickNoteInfos[1].RightArrow.color.a, 0.01f, "Note 1 right arrow opacity should be correct");
                Assert.AreEqual(0.66f, _flickNoteInfos[2].NoteFill.color.a, 0.01f, "Note 2 fill opacity should be correct");
                Assert.AreEqual(0.66f, _flickNoteInfos[2].NoteRing.color.a, 0.01f, "Note 2 ring opacity should be correct");
                Assert.AreEqual(0.66f, _flickNoteInfos[2].LeftArrow.color.a, 0.01f, "Note 2 left arrow opacity should be correct");
                Assert.AreEqual(0.66f, _flickNoteInfos[2].RightArrow.color.a, 0.01f, "Note 2 right arrow opacity should be correct");
            }

            [Test]
            public void RenderSetsFlickNotesArrowPositionCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _flickNoteInfos[0].IntroTime = 0.0f;
                _flickNoteInfos[0].Time = 1.25f;
                _flickNoteInfos[0].Size = 1.0f;
                _flickNoteInfos[1].IntroTime = 0.0f;
                _flickNoteInfos[1].Time = 2.25f;
                _flickNoteInfos[1].Size = 0.5f;
                _flickNoteInfos[2].IntroTime = 1.0f;
                _flickNoteInfos[2].Time = 2.0f;
                _flickNoteInfos[2].Size = 2.0f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(0.0f, _flickNoteInfos[0].LeftArrowTransform.localPosition.x, 0.01f, "Note 0 left arrow X position should be correct");
                Assert.AreEqual(0.0f, _flickNoteInfos[0].RightArrowTransform.localPosition.x, 0.01f, "Note 0 right arrow X position should be correct");
                Assert.AreEqual(-1.8f, _flickNoteInfos[1].LeftArrowTransform.localPosition.x, 0.01f, "Note 1 left arrow X position should be correct");
                Assert.AreEqual(1.8f, _flickNoteInfos[1].RightArrowTransform.localPosition.x, 0.01f, "Note 1 right arrow X position should be correct");
                Assert.AreEqual(-1.5f, _flickNoteInfos[2].LeftArrowTransform.localPosition.x, 0.01f, "Note 2 left arrow X position should be correct");
                Assert.AreEqual(1.5f, _flickNoteInfos[2].RightArrowTransform.localPosition.x, 0.01f, "Note 2 right arrow X position should be correct");
            }
        }

        [TestFixture]
        public class DragChildNotes
        {
            private GameObject _dragChildNotePrefab;
            private FakeNoteProvider _fakeNoteProvider;
            private IChartToScreenCoordinatesConverter _chartToScreenCoordinatesConverter;
            private List<DragChildNoteInfo> _dragChildNoteInfos;

            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                _dragChildNotePrefab = Resources.Load<GameObject>("Drag Child New");
            }

            [SetUp]
            public void SetUp()
            {
                List<GameObject> dragChildNotes = new()
                {
                    Object.Instantiate(_dragChildNotePrefab, Vector3.zero, Quaternion.identity),
                    Object.Instantiate(_dragChildNotePrefab, Vector3.left, Quaternion.identity),
                    Object.Instantiate(_dragChildNotePrefab, Vector3.right, Quaternion.identity),
                };
                _dragChildNoteInfos = new List<DragChildNoteInfo>();
                foreach (GameObject dragChildNote in dragChildNotes)
                {
                    _dragChildNoteInfos.Add(dragChildNote.GetComponent<DragChildNoteInfo>());
                }
                _fakeNoteProvider = new FakeNoteProvider(new List<ClickNoteInfo>(),
                            new List<FlickNoteInfo>(), _dragChildNoteInfos,
                            new List<HoldNoteInfo>(), new List<LongHoldNoteInfo>());
                _chartToScreenCoordinatesConverter = new FakeChartToScreenCoordinatesConverter();
            }

            [Test]
            public void RenderSetsDragChildNotesAbsoluteNoteSizeCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _dragChildNoteInfos[0].IntroTime = 0.0f;
                _dragChildNoteInfos[0].Time = 1.0f;
                _dragChildNoteInfos[0].Size = 1.0f;
                _dragChildNoteInfos[1].IntroTime = 0.0f;
                _dragChildNoteInfos[1].Time = 2.0f;
                _dragChildNoteInfos[1].Size = 1.0f;
                _dragChildNoteInfos[2].IntroTime = 0.0f;
                _dragChildNoteInfos[2].Time = 3.0f;
                _dragChildNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(1.0f, _dragChildNoteInfos[0].NoteTransform.localScale.x, 0.01f, "Note 0 X scale should be correct");
                Assert.AreEqual(1.0f, _dragChildNoteInfos[0].NoteTransform.localScale.y, 0.01f, "Note 0 Y scale should be correct");
                Assert.AreEqual(0.85f, _dragChildNoteInfos[1].NoteTransform.localScale.x, 0.01f, "Note 1 X scale should be correct");
                Assert.AreEqual(0.85f, _dragChildNoteInfos[1].NoteTransform.localScale.y, 0.01f, "Note 1 Y scale should be correct");
                Assert.AreEqual(0.4f, _dragChildNoteInfos[2].NoteTransform.localScale.x, 0.01f, "Note 2 X scale should be correct");
                Assert.AreEqual(0.4f, _dragChildNoteInfos[2].NoteTransform.localScale.y, 0.01f, "Note 2 Y scale should be correct");
            }

            [Test]
            public void RenderSetsDragChildNotePositionCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _dragChildNoteInfos[0].IntroTime = 0.0f;
                _dragChildNoteInfos[0].Time = 1.0f;
                _dragChildNoteInfos[0].Size = 1.0f;
                _dragChildNoteInfos[0].X = 7.1f;
                _dragChildNoteInfos[0].Y = 13.2f;
                _dragChildNoteInfos[1].IntroTime = 0.0f;
                _dragChildNoteInfos[1].Time = 2.0f;
                _dragChildNoteInfos[1].Size = 1.0f;
                _dragChildNoteInfos[1].X = 0.0f;
                _dragChildNoteInfos[1].Y = 0.0f;
                _dragChildNoteInfos[2].IntroTime = 0.0f;
                _dragChildNoteInfos[2].Time = 3.0f;
                _dragChildNoteInfos[2].Size = 0.5f;
                _dragChildNoteInfos[2].X = -10.0f;
                _dragChildNoteInfos[2].Y = -3.5f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(7.1f, _dragChildNoteInfos[0].NoteTransform.localPosition.x, 0.01f, "Note 0 X position should be correct");
                Assert.AreEqual(13.2f, _dragChildNoteInfos[0].NoteTransform.localPosition.y, 0.01f, "Note 0 Y position should be correct");
                Assert.AreEqual(0.0f, _dragChildNoteInfos[1].NoteTransform.localPosition.x, 0.01f, "Note 1 X position should be correct");
                Assert.AreEqual(0.0f, _dragChildNoteInfos[1].NoteTransform.localPosition.y, 0.01f, "Note 1 Y position should be correct");
                Assert.AreEqual(-10.0f, _dragChildNoteInfos[2].NoteTransform.localPosition.x, 0.01f, "Note 2 X position should be correct");
                Assert.AreEqual(-3.5f, _dragChildNoteInfos[2].NoteTransform.localPosition.y, 0.01f, "Note 2 Y position should be correct");
            }

            [Test]
            public void RenderSetsDragChildNotesOpacityCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _dragChildNoteInfos[0].IntroTime = 0.0f;
                _dragChildNoteInfos[0].Time = 1.0f;
                _dragChildNoteInfos[0].Opacity = 1.0f;
                _dragChildNoteInfos[1].IntroTime = 0.0f;
                _dragChildNoteInfos[1].Time = 2.0f;
                _dragChildNoteInfos[1].Opacity = 0.5f;
                _dragChildNoteInfos[2].IntroTime = 0.0f;
                _dragChildNoteInfos[2].Time = 3.0f;
                _dragChildNoteInfos[2].Opacity = 1.0f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(1.0f, _dragChildNoteInfos[0].NoteFill.color.a, 0.01f, "Note 0 fill opacity should be correct");
                Assert.AreEqual(0.5f, _dragChildNoteInfos[1].NoteFill.color.a, 0.01f, "Note 1 fill opacity should be correct");
                Assert.AreEqual(0.66f, _dragChildNoteInfos[2].NoteFill.color.a, 0.01f, "Note 2 fill opacity should be correct");
            }
        }

        [TestFixture]
        public class HoldNotes
        {
            private GameObject _holdNotePrefab;
            private FakeNoteProvider _fakeNoteProvider;
            private IChartToScreenCoordinatesConverter _chartToScreenCoordinatesConverter;
            private List<HoldNoteInfo> _holdNoteInfos;

            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                _holdNotePrefab = Resources.Load<GameObject>("Hold Note New");
            }

            [SetUp]
            public void SetUp()
            {
                List<GameObject> holdNotes = new()
            {
                Object.Instantiate(_holdNotePrefab, Vector3.zero, Quaternion.identity),
                Object.Instantiate(_holdNotePrefab, Vector3.left, Quaternion.identity),
                Object.Instantiate(_holdNotePrefab, Vector3.right, Quaternion.identity),
            };
                _holdNoteInfos = new List<HoldNoteInfo>();
                foreach (GameObject holdNote in holdNotes)
                {
                    var holdNoteInfo = holdNote.GetComponent<HoldNoteInfo>();
                    holdNoteInfo.NoteBodyBackground.size = new Vector2(1.0f, 5.0f);
                    _holdNoteInfos.Add(holdNoteInfo);
                }
                _fakeNoteProvider = new FakeNoteProvider(new List<ClickNoteInfo>(),
                                new List<FlickNoteInfo>(), new List<DragChildNoteInfo>(),
                                _holdNoteInfos, new List<LongHoldNoteInfo>());
                _chartToScreenCoordinatesConverter = new FakeChartToScreenCoordinatesConverter();
            }

            [Test]
            public void RenderSetsHoldNotePositionCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _holdNoteInfos[0].IntroTime = 0.0;
                _holdNoteInfos[0].StartTime = 1.0;
                _holdNoteInfos[0].EndTime = 2.0;
                _holdNoteInfos[0].X = 7.1f;
                _holdNoteInfos[0].Y = 13.2f;
                _holdNoteInfos[0].Size = 1.0f;
                _holdNoteInfos[1].IntroTime = 0.0;
                _holdNoteInfos[1].StartTime = 1.0;
                _holdNoteInfos[1].EndTime = 2.0;
                _holdNoteInfos[1].X = 0.0f;
                _holdNoteInfos[1].Y = 0.0f;
                _holdNoteInfos[1].Size = 1.0f;
                _holdNoteInfos[2].IntroTime = 0.0;
                _holdNoteInfos[2].StartTime = 1.0;
                _holdNoteInfos[2].EndTime = 2.0;
                _holdNoteInfos[2].X = -10.0f;
                _holdNoteInfos[2].Y = -3.5f;
                _holdNoteInfos[2].Size = 1.0f;

                playbackRenderer.Render(1.0);

                Assert.AreEqual(7.1f, _holdNoteInfos[0].NoteTransform.localPosition.x, 0.01f, "Note 0 X position should be correct");
                Assert.AreEqual(13.2f, _holdNoteInfos[0].NoteTransform.localPosition.y, 0.01f, "Note 0 Y position should be correct");
                Assert.AreEqual(0.0f, _holdNoteInfos[1].NoteTransform.localPosition.x, 0.01f, "Note 1 X position should be correct");
                Assert.AreEqual(0.0f, _holdNoteInfos[1].NoteTransform.localPosition.y, 0.01f, "Note 1 Y position should be correct");
                Assert.AreEqual(-10.0f, _holdNoteInfos[2].NoteTransform.localPosition.x, 0.01f, "Note 2 X position should be correct");
                Assert.AreEqual(-3.5f, _holdNoteInfos[2].NoteTransform.localPosition.y, 0.01f, "Note 2 Y position should be correct");
            }

            [Test]
            public void RenderSetsHoldNotesAbsoluteNoteSizeCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _holdNoteInfos[0].IntroTime = 0.0;
                _holdNoteInfos[0].StartTime = 1.0;
                _holdNoteInfos[0].EndTime = 2.0;
                _holdNoteInfos[0].Size = 1.0f;
                _holdNoteInfos[1].IntroTime = 0.0;
                _holdNoteInfos[1].StartTime = 2.0;
                _holdNoteInfos[1].EndTime = 3.0;
                _holdNoteInfos[1].Size = 1.0f;
                _holdNoteInfos[2].IntroTime = 0.0;
                _holdNoteInfos[2].StartTime = 3.0;
                _holdNoteInfos[2].EndTime = 4.0;
                _holdNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0);

                Assert.AreEqual(1.0f, _holdNoteInfos[0].NoteTransform.localScale.x, 0.01f, "Note 0 X scale should be correct");
                Assert.AreEqual(1.0f, _holdNoteInfos[0].NoteTransform.localScale.y, 0.01f, "Note 0 Y scale should be correct");
                Assert.AreEqual(0.7f, _holdNoteInfos[1].NoteTransform.localScale.x, 0.01f, "Note 1 X scale should be correct");
                Assert.AreEqual(0.7f, _holdNoteInfos[1].NoteTransform.localScale.y, 0.01f, "Note 1 Y scale should be correct");
                Assert.AreEqual(0.30f, _holdNoteInfos[2].NoteTransform.localScale.x, 0.01f, "Note 2 X scale should be correct");
                Assert.AreEqual(0.30f, _holdNoteInfos[2].NoteTransform.localScale.y, 0.01f, "Note 2 Y scale should be correct");
            }

            [Test]
            public void RenderSetsHoldNotesOpacityCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _holdNoteInfos[0].IntroTime = 0.0;
                _holdNoteInfos[0].StartTime = 1.0;
                _holdNoteInfos[0].Opacity = 1.0f;
                _holdNoteInfos[0].Size = 1.0f;
                _holdNoteInfos[1].IntroTime = 0.0;
                _holdNoteInfos[1].StartTime = 2.0;
                _holdNoteInfos[1].Opacity = 0.5f;
                _holdNoteInfos[1].Size = 1.0f;
                _holdNoteInfos[2].IntroTime = 0.0;
                _holdNoteInfos[2].StartTime = 3.0;
                _holdNoteInfos[2].Opacity = 1.0f;
                _holdNoteInfos[2].Size = 1.0f;

                playbackRenderer.Render(1.0);

                Assert.AreEqual(1.0f, _holdNoteInfos[0].NoteFill.color.a, 0.01f, "Note 0 fill opacity should be correct");
                Assert.AreEqual(1.0f, _holdNoteInfos[0].NoteRing.color.a, 0.01f, "Note 0 ring opacity should be correct");
                Assert.AreEqual(0.5f, _holdNoteInfos[1].NoteFill.color.a, 0.01f, "Note 1 fill opacity should be correct");
                Assert.AreEqual(0.5f, _holdNoteInfos[1].NoteRing.color.a, 0.01f, "Note 1 ring opacity should be correct");
                Assert.AreEqual(0.66f, _holdNoteInfos[2].NoteFill.color.a, 0.01f, "Note 2 fill opacity should be correct");
                Assert.AreEqual(0.66f, _holdNoteInfos[2].NoteRing.color.a, 0.01f, "Note 2 ring opacity should be correct");
            }

            [Test]
            public void RenderSetsHoldNotesBodyBackgroundCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _holdNoteInfos[0].IntroTime = 0.0;
                _holdNoteInfos[0].StartTime = 1.0;
                _holdNoteInfos[0].Opacity = 1.0f;
                _holdNoteInfos[0].Size = 1.0f;
                _holdNoteInfos[1].IntroTime = 0.0;
                _holdNoteInfos[1].StartTime = 2.0;
                _holdNoteInfos[1].Opacity = 0.5f;
                _holdNoteInfos[1].Size = 1.0f;
                _holdNoteInfos[2].IntroTime = 0.0;
                _holdNoteInfos[2].StartTime = 4.0;
                _holdNoteInfos[2].Opacity = 1.0f;
                _holdNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0);

                Assert.AreEqual(1.0f, _holdNoteInfos[0].NoteBodyBackground.color.a, 0.01f, "Note 0 body background opacity should be correct");
                Assert.AreEqual(0.5f, _holdNoteInfos[1].NoteBodyBackground.color.a, 0.01f, "Note 1 body background opacity should be correct");
                Assert.AreEqual(0.5f, _holdNoteInfos[2].NoteBodyBackground.color.a, 0.01f, "Note 2 body background opacity should be correct");
                Assert.AreEqual(1.0f, _holdNoteInfos[0].NoteBodyBackground.size.x, 0.01f, "Note 0 body background X size should be correct");
                Assert.AreEqual(5.0f, _holdNoteInfos[0].NoteBodyBackground.size.y, 0.01f, "Note 0 body background Y size should be correct");
            }

            [Test]
            public void RenderSetsHoldNotesCompletedBodyCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _holdNoteInfos[0].IntroTime = -1.0;
                _holdNoteInfos[0].StartTime = 0.0;
                _holdNoteInfos[0].EndTime = 2.0;
                _holdNoteInfos[0].Opacity = 1.0f;
                _holdNoteInfos[0].Size = 0.5f;
                _holdNoteInfos[1].IntroTime = -1.0;
                _holdNoteInfos[1].StartTime = 0.5;
                _holdNoteInfos[1].EndTime = 1.5;
                _holdNoteInfos[1].Opacity = 1.0f;
                _holdNoteInfos[1].Size = 0.5f;
                _holdNoteInfos[2].IntroTime = -1.0;
                _holdNoteInfos[2].StartTime = 1.0;
                _holdNoteInfos[2].EndTime = 2.0;
                _holdNoteInfos[2].Opacity = 1.0f;
                _holdNoteInfos[2].Size = 1.0f;

                playbackRenderer.Render(1.0);

                Assert.AreEqual(2.5f, _holdNoteInfos[0].NoteCompletedBody.size.y, 0.01f, "Note 0 completed body Y size should be correct");
                Assert.AreEqual(2.5f, _holdNoteInfos[1].NoteCompletedBody.size.y, 0.01f, "Note 1 completed body Y size should be correct");
                Assert.AreEqual(0.0f, _holdNoteInfos[2].NoteCompletedBody.size.y, 0.01f, "Note 2 completed body Y size should be correct");
                Assert.AreEqual(1.0f, _holdNoteInfos[0].NoteCompletedBody.color.a, 0.01f, "Note 0 completed body opacity should be correct");
            }

            [Test]
            public void RenderSetsHoldNotesBodyTransformScaleCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _holdNoteInfos[0].IntroTime = 0.0;
                _holdNoteInfos[0].StartTime = 1.0;
                _holdNoteInfos[0].Size = 1.0f;
                _holdNoteInfos[1].IntroTime = 0.0;
                _holdNoteInfos[1].StartTime = 2.0;
                _holdNoteInfos[1].Size = 2.0f;
                _holdNoteInfos[2].IntroTime = 0.0;
                _holdNoteInfos[2].StartTime = 4.0;
                _holdNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0);

                Assert.AreEqual(0.6f, _holdNoteInfos[0].NoteBodyTransform.localScale.x, 0.01f, "Note 0 body transform X scale should be correct");
                Assert.AreEqual(0.428f, _holdNoteInfos[1].NoteBodyTransform.localScale.x, 0.01f, "Note 1 body transform X scale should be correct");
                Assert.AreEqual(0.714f, _holdNoteInfos[1].NoteBodyTransform.localScale.y, 0.01f, "Note 1 body transform Y scale should be correct");
                Assert.AreEqual(0.272f, _holdNoteInfos[2].NoteBodyTransform.localScale.x, 0.01f, "Note 2 body transform X scale should be correct");
                Assert.AreEqual(3.636f, _holdNoteInfos[2].NoteBodyTransform.localScale.y, 0.01f, "Note 2 body transform Y scale should be correct");
            }
        }

        [TestFixture]
        public class LongHoldNotes
        {
            private GameObject _longHoldNotePrefab;
            private FakeNoteProvider _fakeNoteProvider;
            private IChartToScreenCoordinatesConverter _chartToScreenCoordinatesConverter;
            private List<LongHoldNoteInfo> _longHoldNoteInfos;

            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                _longHoldNotePrefab = Resources.Load<GameObject>("Long Hold Note New");
            }

            [SetUp]
            public void SetUp()
            {
                List<GameObject> longHoldNotes = new()
                {
                    Object.Instantiate(_longHoldNotePrefab, Vector3.zero, Quaternion.identity),
                    Object.Instantiate(_longHoldNotePrefab, Vector3.left, Quaternion.identity),
                    Object.Instantiate(_longHoldNotePrefab, Vector3.right, Quaternion.identity),
                };
                _longHoldNoteInfos = new List<LongHoldNoteInfo>();
                foreach (GameObject longHoldNote in longHoldNotes)
                {
                    _longHoldNoteInfos.Add(longHoldNote.GetComponent<LongHoldNoteInfo>());
                }
                _fakeNoteProvider = new FakeNoteProvider(new List<ClickNoteInfo>(),
                            new List<FlickNoteInfo>(), new List<DragChildNoteInfo>(),
                            new List<HoldNoteInfo>(), _longHoldNoteInfos);
                _chartToScreenCoordinatesConverter = new FakeChartToScreenCoordinatesConverter();
            }

            [Test]
            public void RenderSetsLongHoldNotePositionCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _longHoldNoteInfos[0].IntroTime = 0.0;
                _longHoldNoteInfos[0].StartTime = 1.0;
                _longHoldNoteInfos[0].EndTime = 2.0;
                _longHoldNoteInfos[0].X = 7.1f;
                _longHoldNoteInfos[0].Y = 13.2f;
                _longHoldNoteInfos[0].Size = 1.0f;
                _longHoldNoteInfos[1].IntroTime = 0.0;
                _longHoldNoteInfos[1].StartTime = 1.0;
                _longHoldNoteInfos[1].EndTime = 2.0;
                _longHoldNoteInfos[1].X = 0.0f;
                _longHoldNoteInfos[1].Y = 0.0f;
                _longHoldNoteInfos[1].Size = 2.0f;
                _longHoldNoteInfos[2].IntroTime = 0.0;
                _longHoldNoteInfos[2].StartTime = 1.0;
                _longHoldNoteInfos[2].EndTime = 2.0;
                _longHoldNoteInfos[2].X = -10.0f;
                _longHoldNoteInfos[2].Y = -3.5f;
                _longHoldNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0);

                Assert.AreEqual(7.1f, _longHoldNoteInfos[0].NoteTransform.localPosition.x, 0.01f, "Note 0 X position should be correct");
                Assert.AreEqual(13.2f, _longHoldNoteInfos[0].NoteTransform.localPosition.y, 0.01f, "Note 0 Y position should be correct");
                Assert.AreEqual(0.0f, _longHoldNoteInfos[1].NoteTransform.localPosition.x, 0.01f, "Note 1 X position should be correct");
                Assert.AreEqual(0.0f, _longHoldNoteInfos[1].NoteTransform.localPosition.y, 0.01f, "Note 1 Y position should be correct");
                Assert.AreEqual(-10.0f, _longHoldNoteInfos[2].NoteTransform.localPosition.x, 0.01f, "Note 2 X position should be correct");
                Assert.AreEqual(-3.5f, _longHoldNoteInfos[2].NoteTransform.localPosition.y, 0.01f, "Note 2 Y position should be correct");
            }

            [Test]
            public void RenderSetsLongHoldNotesAbsoluteNoteSizeCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _longHoldNoteInfos[0].IntroTime = 0.0;
                _longHoldNoteInfos[0].StartTime = 1.0;
                _longHoldNoteInfos[0].EndTime = 2.0;
                _longHoldNoteInfos[0].Size = 1.0f;
                _longHoldNoteInfos[1].IntroTime = 0.0;
                _longHoldNoteInfos[1].StartTime = 2.0;
                _longHoldNoteInfos[1].EndTime = 3.0;
                _longHoldNoteInfos[1].Size = 1.0f;
                _longHoldNoteInfos[2].IntroTime = 0.0;
                _longHoldNoteInfos[2].StartTime = 3.0;
                _longHoldNoteInfos[2].EndTime = 4.0;
                _longHoldNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0);

                Assert.AreEqual(1.0f, _longHoldNoteInfos[0].NoteTransform.localScale.x, 0.01f, "Note 0 X scale should be correct");
                Assert.AreEqual(1.0f, _longHoldNoteInfos[0].NoteTransform.localScale.y, 0.01f, "Note 0 Y scale should be correct");
                Assert.AreEqual(0.7f, _longHoldNoteInfos[1].NoteTransform.localScale.x, 0.01f, "Note 1 X scale should be correct");
                Assert.AreEqual(0.7f, _longHoldNoteInfos[1].NoteTransform.localScale.y, 0.01f, "Note 1 Y scale should be correct");
                Assert.AreEqual(0.30f, _longHoldNoteInfos[2].NoteTransform.localScale.x, 0.01f, "Note 2 X scale should be correct");
                Assert.AreEqual(0.30f, _longHoldNoteInfos[2].NoteTransform.localScale.y, 0.01f, "Note 2 Y scale should be correct");
            }

            [Test]
            public void RenderSetsLongHoldNotesOpacityCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _longHoldNoteInfos[0].IntroTime = 0.0;
                _longHoldNoteInfos[0].StartTime = 1.0;
                _longHoldNoteInfos[0].Opacity = 1.0f;
                _longHoldNoteInfos[0].Size = 1.0f;
                _longHoldNoteInfos[1].IntroTime = 0.0;
                _longHoldNoteInfos[1].StartTime = 2.0;
                _longHoldNoteInfos[1].Opacity = 0.5f;
                _longHoldNoteInfos[1].Size = 0.5f;
                _longHoldNoteInfos[2].IntroTime = 0.0;
                _longHoldNoteInfos[2].StartTime = 3.0;
                _longHoldNoteInfos[2].Opacity = 1.0f;
                _longHoldNoteInfos[2].Size = 1.0f;

                playbackRenderer.Render(1.0);

                Assert.AreEqual(1.0f, _longHoldNoteInfos[0].NoteFill.color.a, 0.01f, "Note 0 fill opacity should be correct");
                Assert.AreEqual(1.0f, _longHoldNoteInfos[0].NoteRing.color.a, 0.01f, "Note 0 ring opacity should be correct");
                Assert.AreEqual(0.5f, _longHoldNoteInfos[1].NoteFill.color.a, 0.01f, "Note 1 fill opacity should be correct");
                Assert.AreEqual(0.5f, _longHoldNoteInfos[1].NoteRing.color.a, 0.01f, "Note 1 ring opacity should be correct");
                Assert.AreEqual(0.66f, _longHoldNoteInfos[2].NoteFill.color.a, 0.01f, "Note 2 fill opacity should be correct");
                Assert.AreEqual(0.66f, _longHoldNoteInfos[2].NoteRing.color.a, 0.01f, "Note 2 ring opacity should be correct");
            }

            [Test]
            public void RenderSetsLongHoldNotesBodyBackgroundCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _longHoldNoteInfos[0].IntroTime = 0.0;
                _longHoldNoteInfos[0].StartTime = 1.0;
                _longHoldNoteInfos[0].Opacity = 1.0f;
                _longHoldNoteInfos[0].Size = 1.0f;
                _longHoldNoteInfos[1].IntroTime = 0.0;
                _longHoldNoteInfos[1].StartTime = 2.0;
                _longHoldNoteInfos[1].Opacity = 0.5f;
                _longHoldNoteInfos[1].Size = 0.5f;
                _longHoldNoteInfos[2].IntroTime = 0.0;
                _longHoldNoteInfos[2].StartTime = 3.0;
                _longHoldNoteInfos[2].Opacity = 1.0f;
                _longHoldNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0);

                Assert.AreEqual(1.0f, _longHoldNoteInfos[0].NoteBodyBackgroundTop.color.a, 0.01f, "Note 0 body background top opacity should be correct");
                Assert.AreEqual(1.0f, _longHoldNoteInfos[0].NoteBodyBackgroundBottom.color.a, 0.01f, "Note 0 body background bottom opacity should be correct");
                Assert.AreEqual(0.5f, _longHoldNoteInfos[1].NoteBodyBackgroundTop.color.a, 0.01f, "Note 1 body background top opacity should be correct");
                Assert.AreEqual(0.5f, _longHoldNoteInfos[1].NoteBodyBackgroundBottom.color.a, 0.01f, "Note 1 body background bottom opacity should be correct");
                Assert.AreEqual(0.66f, _longHoldNoteInfos[2].NoteBodyBackgroundTop.color.a, 0.01f, "Note 2 body background top opacity should be correct");
                Assert.AreEqual(0.66f, _longHoldNoteInfos[2].NoteBodyBackgroundBottom.color.a, 0.01f, "Note 2 body background bottom opacity should be correct");
            }

            [Test]
            public void RenderSetsLongHoldNotesCompletedBodyCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _longHoldNoteInfos[0].IntroTime = -1.0;
                _longHoldNoteInfos[0].StartTime = 0.0;
                _longHoldNoteInfos[0].EndTime = 2.0;
                _longHoldNoteInfos[0].Y = 2.0f;
                _longHoldNoteInfos[0].Size = 2.0f;
                _longHoldNoteInfos[1].IntroTime = -1.0;
                _longHoldNoteInfos[1].StartTime = 0.5;
                _longHoldNoteInfos[1].EndTime = 1.5;
                _longHoldNoteInfos[1].Y = 0.0f;
                _longHoldNoteInfos[1].Size = 1.0f;
                _longHoldNoteInfos[2].IntroTime = -1.0;
                _longHoldNoteInfos[2].StartTime = 1.0;
                _longHoldNoteInfos[2].EndTime = 2.0;
                _longHoldNoteInfos[2].Y = -1.0f;
                _longHoldNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0);

                Assert.AreEqual(1.0f, _longHoldNoteInfos[0].NoteCompletedBodyTop.size.x, 0.01f, "Note 0 completed body top X size should be correct");
                Assert.AreEqual(1.0f, _longHoldNoteInfos[0].NoteCompletedBodyBottom.size.x, 0.01f, "Note 0 completed body bottom X size should be correct");
                Assert.AreEqual(1.0f, _longHoldNoteInfos[0].NoteCompletedBodyTop.size.y, 0.01f, "Note 0 completed body top Y size should be correct");
                Assert.AreEqual(3.0f, _longHoldNoteInfos[0].NoteCompletedBodyBottom.size.y, 0.01f, "Note 0 completed body bottom Y size should be correct");
                Assert.AreEqual(2.0f, _longHoldNoteInfos[1].NoteCompletedBodyTop.size.y, 0.01f, "Note 1 completed body top Y size should be correct");
                Assert.AreEqual(2.0f, _longHoldNoteInfos[1].NoteCompletedBodyBottom.size.y, 0.01f, "Note 1 completed body bottom Y size should be correct");
                Assert.AreEqual(0.0f, _longHoldNoteInfos[2].NoteCompletedBodyTop.size.y, 0.01f, "Note 2 completed body top Y size should be correct");
                Assert.AreEqual(0.0f, _longHoldNoteInfos[2].NoteCompletedBodyBottom.size.y, 0.01f, "Note 2 completed body bottom Y size should be correct");
            }

            [Test]
            public void RenderSetsLongHoldNotesBodyTransformScaleCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider, _chartToScreenCoordinatesConverter);
                _longHoldNoteInfos[0].IntroTime = 0.0;
                _longHoldNoteInfos[0].StartTime = 1.0;
                _longHoldNoteInfos[0].Size = 1.0f;
                _longHoldNoteInfos[1].IntroTime = 0.0;
                _longHoldNoteInfos[1].StartTime = 2.0;
                _longHoldNoteInfos[1].Size = 2.0f;
                _longHoldNoteInfos[2].IntroTime = 0.0;
                _longHoldNoteInfos[2].StartTime = 4.0;
                _longHoldNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0);

                Assert.AreEqual(0.6f, _longHoldNoteInfos[0].NoteBodyTransform.localScale.x, 0.01f, "Note 0 body transform X scale should be correct");
                Assert.AreEqual(1.0f, _longHoldNoteInfos[0].NoteBodyTransform.localScale.y, 0.01f, "Note 0 body transform Y scale should be correct");
                Assert.AreEqual(0.428f, _longHoldNoteInfos[1].NoteBodyTransform.localScale.x, 0.01f, "Note 1 body transform X scale should be correct");
                Assert.AreEqual(0.714f, _longHoldNoteInfos[1].NoteBodyTransform.localScale.y, 0.01f, "Note 1 body transform Y scale should be correct");
                Assert.AreEqual(0.272f, _longHoldNoteInfos[2].NoteBodyTransform.localScale.x, 0.01f, "Note 2 body transform X scale should be correct");
                Assert.AreEqual(3.636f, _longHoldNoteInfos[2].NoteBodyTransform.localScale.y, 0.01f, "Note 2 body transform Y scale should be correct");
            }
        }
    }
}