using System.Collections.Generic;
using CCE.Rendering.Notes;
using UnityEngine;

namespace CCE.Rendering
{
    public class DragLineManager
    {
        private readonly Mesh _quadMesh;
        private readonly Material _lineMaterial;
        private readonly MaterialPropertyBlock _props;
        private readonly INoteProvider _noteProvider;

        private readonly float _dragLineWidth;
        private readonly float _dragChildSize;

        public DragLineManager(Mesh quadMesh, Material lineMaterial, INoteProvider noteProvider,
                               IChartToScreenCoordinatesConverter chartToScreenConverter)
        {
            _quadMesh = quadMesh;
            _lineMaterial = lineMaterial;
            _noteProvider = noteProvider;
            _props = new MaterialPropertyBlock();
            _dragLineWidth = 0.16f * chartToScreenConverter.ScreenSize / 10.0f;
            _dragChildSize = chartToScreenConverter.DragChildNoteSize;
        }

        public void Render(double time)
        {
            int dragNoteCount = 0;
            List<List<DragPathNode>> dragPaths = new();
            List<DragHeadNoteInfo> dragHeadNoteInfos = _noteProvider.GetDragHeadNotes();
            List<CDragHeadNoteInfo> cdragHeadNoteInfos = _noteProvider.GetCDragHeadNotes();

            foreach (var dragHeadNoteInfo in dragHeadNoteInfos)
            {
                dragPaths.Add(dragHeadNoteInfo.DragPath);
                dragNoteCount++;
            }
            foreach (var cdragChildNoteInfo in cdragHeadNoteInfos)
            {
                dragPaths.Add(cdragChildNoteInfo.DragPath);
            }

            if (dragPaths.Count == 0)
            {
                return;
            }

            var matrices = new List<Matrix4x4>();
            var tilingProps = new List<Vector4>();

            int currentDragIndex = 0;
            foreach (var dragPath in dragPaths)
            {
                for (int i = 2; i < dragPath.Count; i++)
                {
                    var startNode = dragPath[i - 1];
                    var endNode = dragPath[i];

                    if (time < startNode.IntroTime || time > endNode.Time)
                    {
                        continue;
                    }

                    Vector2 startPos = new(startNode.X, startNode.Y);
                    Vector2 endPos = new(endNode.X, endNode.Y);
                    Vector2 linePosition = (startPos + endPos) / 2.0f;
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, endPos - startPos);
                    float lineLength = Vector2.Distance(startPos, endPos);
                    float clipStart = (float)(time - startNode.Time) / (float)(endNode.Time - startNode.Time);
                    float clipEnd = 1.0f - (float)(endNode.IntroTime - time - 0.133f) / (float)(endNode.IntroTime - startNode.IntroTime);

                    // Adjust clipStart and clipEnd to not overlap with the notes themselves.
                    // In Cytoid this is done with sprite masks, but we don't have this for DrawMeshInstanced.
                    if (currentDragIndex < dragNoteCount && i == 2)
                    {
                        float approachPercentage = Mathf.Clamp01((float)(time - dragHeadNoteInfos[currentDragIndex].IntroTime) /
                                                                 (float)(dragHeadNoteInfos[currentDragIndex].StartTime - dragHeadNoteInfos[currentDragIndex].IntroTime));
                        float dragHeadSize = dragHeadNoteInfos[currentDragIndex].Size * (0.7f + 0.3f * approachPercentage);
                        clipStart = Mathf.Max(clipStart, dragHeadSize / 2 / lineLength);
                    }
                    else if (currentDragIndex >= dragNoteCount && i == 2)
                    {
                        float approachPercentage = Mathf.Clamp01((float)(time - cdragHeadNoteInfos[currentDragIndex - dragNoteCount].IntroTime) /
                                                                 (float)(cdragHeadNoteInfos[currentDragIndex - dragNoteCount].StartTime - cdragHeadNoteInfos[currentDragIndex - dragNoteCount].IntroTime));
                        float cdragHeadSize = cdragHeadNoteInfos[currentDragIndex - dragNoteCount].Size * (0.4f + 0.6f * approachPercentage);
                        clipStart = Mathf.Max(clipStart, cdragHeadSize / 2 / lineLength);
                    }
                    float dragChildRelativeSize = _dragChildSize / 4.5f / lineLength;
                    clipEnd = Mathf.Min(clipEnd, 1.0f - dragChildRelativeSize);
                    clipStart = Mathf.Max(clipStart, dragChildRelativeSize);

                    // setting Z to 1.0f to avoid z-fighting with notes
                    matrices.Add(Matrix4x4.TRS(new Vector3(linePosition.x, linePosition.y, 1.0f), rotation, new Vector3(_dragLineWidth, lineLength, 1)));
                    tilingProps.Add(new Vector4(lineLength / _dragLineWidth, clipStart, clipEnd, 0));
                }

                currentDragIndex++;
            }

            if (matrices.Count == 0)
            {
                return;
            }

            _props.Clear();
            _props.SetVectorArray("_TilingProps", tilingProps.ToArray());
            Graphics.DrawMeshInstanced(_quadMesh, 0, _lineMaterial, matrices.ToArray(),
                                       matrices.Count, _props,
                                       UnityEngine.Rendering.ShadowCastingMode.Off,
                                       /* receiveShadows= */ false,
                                       /* layer= */ 0);
        }
    }
}
