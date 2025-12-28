using System.Collections.Generic;
using CCE.Rendering;
using CCE.Rendering.Notes;
using NUnit.Framework;
using UnityEngine;

namespace CCE.Tests.Rendering
{
    public class PlaybackRendererTests
    {
        private class FakeNoteProvider : INoteProvider
        {
            private readonly List<ClickNoteInfo> _clickNotes;
            private readonly List<FlickNoteInfo> _flickNotes;

            public FakeNoteProvider(List<ClickNoteInfo> clickNotes, List<FlickNoteInfo> flickNotes)
            {
                _clickNotes = clickNotes;
                _flickNotes = flickNotes;
            }

            public List<ClickNoteInfo> GetClickNotes()
            {
                return _clickNotes;
            }

            public List<FlickNoteInfo> GetFlickNotes()
            {
                return _flickNotes;
            }
        }

        [TestFixture]
        public class ClickNotes
        {
            private GameObject _clickNotePrefab;
            private FakeNoteProvider _fakeNoteProvider;
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
                _fakeNoteProvider = new FakeNoteProvider(_clickNoteInfos, new List<FlickNoteInfo>());
            }

            [Test]
            public void CanCreateRenderer()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider);
                Assert.IsNotNull(playbackRenderer);
            }

            [Test]
            public void RenderSetsClickNotesAbsoluteNoteSizeCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider);
                _clickNoteInfos[0].StartTime = 0.0f;
                _clickNoteInfos[0].EndTime = 1.0f;
                _clickNoteInfos[0].Size = 1.0f;
                _clickNoteInfos[1].StartTime = 0.0f;
                _clickNoteInfos[1].EndTime = 2.0f;
                _clickNoteInfos[1].Size = 1.0f;
                _clickNoteInfos[2].StartTime = 0.0f;
                _clickNoteInfos[2].EndTime = 3.0f;
                _clickNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(1.0f, _clickNoteInfos[0].NoteTransform.localScale.x, 0.01f);
                Assert.AreEqual(1.0f, _clickNoteInfos[0].NoteTransform.localScale.y, 0.01f);
                Assert.AreEqual(0.7f, _clickNoteInfos[1].NoteTransform.localScale.x, 0.01f);
                Assert.AreEqual(0.7f, _clickNoteInfos[1].NoteTransform.localScale.y, 0.01f);
                Assert.AreEqual(0.30f, _clickNoteInfos[2].NoteTransform.localScale.x, 0.01f);
                Assert.AreEqual(0.30f, _clickNoteInfos[2].NoteTransform.localScale.y, 0.01f);
            }

            [Test]
            public void RenderSetsClickNotePositionCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider);
                _clickNoteInfos[0].StartTime = 0.0f;
                _clickNoteInfos[0].EndTime = 1.0f;
                _clickNoteInfos[0].Size = 1.0f;
                _clickNoteInfos[0].X = 7.1f;
                _clickNoteInfos[0].Y = 13.2f;
                _clickNoteInfos[1].StartTime = 0.0f;
                _clickNoteInfos[1].EndTime = 2.0f;
                _clickNoteInfos[1].Size = 1.0f;
                _clickNoteInfos[1].X = 0.0f;
                _clickNoteInfos[1].Y = 0.0f;
                _clickNoteInfos[2].StartTime = 0.0f;
                _clickNoteInfos[2].EndTime = 3.0f;
                _clickNoteInfos[2].Size = 0.5f;
                _clickNoteInfos[2].X = -10.0f;
                _clickNoteInfos[2].Y = -3.5f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(7.1f, _clickNoteInfos[0].NoteTransform.localPosition.x, 0.01f);
                Assert.AreEqual(13.2f, _clickNoteInfos[0].NoteTransform.localPosition.y, 0.01f);
                Assert.AreEqual(0.0f, _clickNoteInfos[1].NoteTransform.localPosition.x, 0.01f);
                Assert.AreEqual(0.0f, _clickNoteInfos[1].NoteTransform.localPosition.y, 0.01f);
                Assert.AreEqual(-10.0f, _clickNoteInfos[2].NoteTransform.localPosition.x, 0.01f);
                Assert.AreEqual(-3.5f, _clickNoteInfos[2].NoteTransform.localPosition.y, 0.01f);
            }

            [Test]
            public void RenderSetsClickNotesFillSizeCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider);
                _clickNoteInfos[0].StartTime = 0.0f;
                _clickNoteInfos[0].EndTime = 1.0f;
                _clickNoteInfos[1].StartTime = 0.0f;
                _clickNoteInfos[1].EndTime = 2.0f;
                _clickNoteInfos[2].StartTime = 0.0f;
                _clickNoteInfos[2].EndTime = 3.0f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(1.0f, _clickNoteInfos[0].NoteFillTransform.localScale.x, 0.01f);
                Assert.AreEqual(1.0f, _clickNoteInfos[0].NoteFillTransform.localScale.y, 0.01f);
                Assert.AreEqual(0.5f, _clickNoteInfos[1].NoteFillTransform.localScale.x, 0.01f);
                Assert.AreEqual(0.5f, _clickNoteInfos[1].NoteFillTransform.localScale.y, 0.01f);
                Assert.AreEqual(0.33f, _clickNoteInfos[2].NoteFillTransform.localScale.x, 0.01f);
                Assert.AreEqual(0.33f, _clickNoteInfos[2].NoteFillTransform.localScale.y, 0.01f);
            }

            [Test]
            public void RenderSetsClickNotesOpacityCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider);
                _clickNoteInfos[0].StartTime = 0.0f;
                _clickNoteInfos[0].EndTime = 1.0f;
                _clickNoteInfos[0].Opacity = 1.0f;
                _clickNoteInfos[1].StartTime = 0.0f;
                _clickNoteInfos[1].EndTime = 2.0f;
                _clickNoteInfos[1].Opacity = 0.5f;
                _clickNoteInfos[2].StartTime = 0.0f;
                _clickNoteInfos[2].EndTime = 3.0f;
                _clickNoteInfos[2].Opacity = 1.0f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(1.0f, _clickNoteInfos[0].NoteFill.color.a, 0.01f);
                Assert.AreEqual(1.0f, _clickNoteInfos[0].NoteRing.color.a, 0.01f);
                Assert.AreEqual(0.25f, _clickNoteInfos[1].NoteFill.color.a, 0.01f);
                Assert.AreEqual(0.25f, _clickNoteInfos[1].NoteRing.color.a, 0.01f);
                Assert.AreEqual(0.33f, _clickNoteInfos[2].NoteFill.color.a, 0.01f);
                Assert.AreEqual(0.33f, _clickNoteInfos[2].NoteRing.color.a, 0.01f);
            }
        }

        [TestFixture]
        public class FlickNotes
        {
            private GameObject _flickNotePrefab;
            private FakeNoteProvider _fakeNoteProvider;
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
                _fakeNoteProvider = new FakeNoteProvider(new List<ClickNoteInfo>(), _flickNoteInfos);
            }

            [Test]
            public void RenderSetsFlickNotesAbsoluteNoteSizeCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider);
                _flickNoteInfos[0].StartTime = 0.0f;
                _flickNoteInfos[0].EndTime = 1.0f;
                _flickNoteInfos[0].Size = 1.0f;
                _flickNoteInfos[1].StartTime = 0.0f;
                _flickNoteInfos[1].EndTime = 2.0f;
                _flickNoteInfos[1].Size = 1.0f;
                _flickNoteInfos[2].StartTime = 0.0f;
                _flickNoteInfos[2].EndTime = 3.0f;
                _flickNoteInfos[2].Size = 0.5f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(1.0f, _flickNoteInfos[0].NoteTransform.localScale.x, 0.01f);
                Assert.AreEqual(1.0f, _flickNoteInfos[0].NoteTransform.localScale.y, 0.01f);
                Assert.AreEqual(0.7f, _flickNoteInfos[1].NoteTransform.localScale.x, 0.01f);
                Assert.AreEqual(0.7f, _flickNoteInfos[1].NoteTransform.localScale.y, 0.01f);
                Assert.AreEqual(0.30f, _flickNoteInfos[2].NoteTransform.localScale.x, 0.01f);
                Assert.AreEqual(0.30f, _flickNoteInfos[2].NoteTransform.localScale.y, 0.01f);
            }

            [Test]
            public void RenderSetsFlickNotePositionCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider);
                _flickNoteInfos[0].StartTime = 0.0f;
                _flickNoteInfos[0].EndTime = 1.0f;
                _flickNoteInfos[0].X = 7.1f;
                _flickNoteInfos[0].Y = 13.2f;
                _flickNoteInfos[1].StartTime = 0.0f;
                _flickNoteInfos[1].EndTime = 2.0f;
                _flickNoteInfos[1].X = 0.0f;
                _flickNoteInfos[1].Y = 0.0f;
                _flickNoteInfos[2].StartTime = 0.0f;
                _flickNoteInfos[2].EndTime = 3.0f;
                _flickNoteInfos[2].X = -10.0f;
                _flickNoteInfos[2].Y = -3.5f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(7.1f, _flickNoteInfos[0].NoteTransform.localPosition.x, 0.01f);
                Assert.AreEqual(13.2f, _flickNoteInfos[0].NoteTransform.localPosition.y, 0.01f);
                Assert.AreEqual(0.0f, _flickNoteInfos[1].NoteTransform.localPosition.x, 0.01f);
                Assert.AreEqual(0.0f, _flickNoteInfos[1].NoteTransform.localPosition.y, 0.01f);
                Assert.AreEqual(-10.0f, _flickNoteInfos[2].NoteTransform.localPosition.x, 0.01f);
                Assert.AreEqual(-3.5f, _flickNoteInfos[2].NoteTransform.localPosition.y, 0.01f);
            }

            [Test]
            public void RenderSetsFlickNotesFillSizeCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider);
                _flickNoteInfos[0].StartTime = 0.0f;
                _flickNoteInfos[0].EndTime = 1.0f;
                _flickNoteInfos[1].StartTime = 0.0f;
                _flickNoteInfos[1].EndTime = 2.0f;
                _flickNoteInfos[2].StartTime = 0.0f;
                _flickNoteInfos[2].EndTime = 3.0f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(1.0f, _flickNoteInfos[0].NoteFillTransform.localScale.x, 0.01f);
                Assert.AreEqual(1.0f, _flickNoteInfos[0].NoteFillTransform.localScale.y, 0.01f);
                Assert.AreEqual(0.5f, _flickNoteInfos[1].NoteFillTransform.localScale.x, 0.01f);
                Assert.AreEqual(0.5f, _flickNoteInfos[1].NoteFillTransform.localScale.y, 0.01f);
                Assert.AreEqual(0.33f, _flickNoteInfos[2].NoteFillTransform.localScale.x, 0.01f);
                Assert.AreEqual(0.33f, _flickNoteInfos[2].NoteFillTransform.localScale.y, 0.01f);
            }

            [Test]
            public void RenderSetsFlickNotesOpacityCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider);
                _flickNoteInfos[0].StartTime = 0.0f;
                _flickNoteInfos[0].EndTime = 1.0f;
                _flickNoteInfos[0].Opacity = 1.0f;
                _flickNoteInfos[1].StartTime = 0.0f;
                _flickNoteInfos[1].EndTime = 2.0f;
                _flickNoteInfos[1].Opacity = 0.5f;
                _flickNoteInfos[2].StartTime = 0.0f;
                _flickNoteInfos[2].EndTime = 3.0f;
                _flickNoteInfos[2].Opacity = 1.0f;

                playbackRenderer.Render(1.0f);

                Assert.AreEqual(1.0f, _flickNoteInfos[0].NoteFill.color.a, 0.01f);
                Assert.AreEqual(1.0f, _flickNoteInfos[0].NoteRing.color.a, 0.01f);
                Assert.AreEqual(1.0f, _flickNoteInfos[0].LeftArrow.color.a, 0.01f);
                Assert.AreEqual(1.0f, _flickNoteInfos[0].RightArrow.color.a, 0.01f);
                Assert.AreEqual(0.25f, _flickNoteInfos[1].NoteFill.color.a, 0.01f);
                Assert.AreEqual(0.25f, _flickNoteInfos[1].NoteRing.color.a, 0.01f);
                Assert.AreEqual(0.25f, _flickNoteInfos[1].LeftArrow.color.a, 0.01f);
                Assert.AreEqual(0.25f, _flickNoteInfos[1].RightArrow.color.a, 0.01f);
                Assert.AreEqual(0.33f, _flickNoteInfos[2].NoteFill.color.a, 0.01f);
                Assert.AreEqual(0.33f, _flickNoteInfos[2].NoteRing.color.a, 0.01f);
                Assert.AreEqual(0.33f, _flickNoteInfos[2].LeftArrow.color.a, 0.01f);
                Assert.AreEqual(0.33f, _flickNoteInfos[2].RightArrow.color.a, 0.01f);
            }

            [Test]
            public void RenderSetsFlickNotesArrowPositionCorrectly()
            {
                PlaybackRenderer playbackRenderer = new(_fakeNoteProvider);
                _flickNoteInfos[0].StartTime = 0.0f;
                _flickNoteInfos[0].EndTime = 1.25f;
                _flickNoteInfos[1].StartTime = 0.0f;
                _flickNoteInfos[1].EndTime = 2.25f;
                _flickNoteInfos[2].StartTime = 1.0f;
                _flickNoteInfos[2].EndTime = 2.0f;

                playbackRenderer.Render(1.0f);

                float maxOffset = 5.0f * 0.3f;

                Assert.AreEqual(0.0f, _flickNoteInfos[0].LeftArrowTransform.localPosition.x, 0.01f);
                Assert.AreEqual(0.0f, _flickNoteInfos[0].RightArrowTransform.localPosition.x, 0.01f);
                Assert.AreEqual(-maxOffset * 0.5f, _flickNoteInfos[1].LeftArrowTransform.localPosition.x, 0.01f);
                Assert.AreEqual(maxOffset * 0.5f, _flickNoteInfos[1].RightArrowTransform.localPosition.x, 0.01f);
                Assert.AreEqual(-maxOffset, _flickNoteInfos[2].LeftArrowTransform.localPosition.x, 0.01f);
                Assert.AreEqual(maxOffset, _flickNoteInfos[2].RightArrowTransform.localPosition.x, 0.01f);
            }
        }
    }
}