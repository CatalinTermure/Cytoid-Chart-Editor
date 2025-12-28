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
            private List<ClickNoteInfo> _clickNotes;

            public FakeNoteProvider(List<ClickNoteInfo> clickNotes)
            {
                _clickNotes = clickNotes;
            }

            public List<ClickNoteInfo> GetClickNotes()
            {
                return _clickNotes;
            }
        }

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
            _fakeNoteProvider = new FakeNoteProvider(_clickNoteInfos);
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
}