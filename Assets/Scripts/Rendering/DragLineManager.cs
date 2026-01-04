using System.Collections.Generic;
using CCE.Rendering.Notes;
using UnityEngine;

namespace CCE.Rendering
{
    public class DragLineManager
    {
        private readonly Mesh _quadMesh;
        private readonly Material _lineMaterial;
        private readonly MaterialPropertyBlock props;
        private readonly INoteProvider _noteProvider;

        private readonly float _dragLineWidth;

        public DragLineManager(Mesh quadMesh, Material lineMaterial, INoteProvider noteProvider,
                               IChartToScreenCoordinatesConverter chartToScreenConverter)
        {
            _quadMesh = quadMesh;
            _lineMaterial = lineMaterial;
            _noteProvider = noteProvider;
            props = new MaterialPropertyBlock();
            _dragLineWidth = 0.16f * chartToScreenConverter.ScreenSize / 10.0f;
        }

        struct DragLineRenderData
        {
            public Vector2 Position;
            public Quaternion Rotation;
            public float Size;
        }

        public void Render(double time)
        {
            List<List<DragPathNode>> dragPaths = new List<List<DragPathNode>>();
            foreach (var dragHeadNoteInfo in _noteProvider.GetDragHeadNotes())
            {
                dragPaths.Add(dragHeadNoteInfo.DragPath);
            }
            foreach (var cdragChildNoteInfo in _noteProvider.GetCDragHeadNotes())
            {
                dragPaths.Add(cdragChildNoteInfo.DragPath);
            }

            if (dragPaths.Count == 0)
            {
                return;
            }

            var matrices = new List<Matrix4x4>();
            var tilingProps = new List<Vector4>();

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
                    // setting Z to 1.0f to avoid z-fighting with notes
                    matrices.Add(Matrix4x4.TRS(new Vector3(linePosition.x, linePosition.y, 1.0f), rotation, new Vector3(_dragLineWidth, lineLength, 1)));
                    tilingProps.Add(new Vector4(lineLength / _dragLineWidth, clipStart, clipEnd, 0));
                }
            }

            if (matrices.Count == 0)
            {
                return;
            }

            props.Clear();
            props.SetVectorArray("_TilingProps", tilingProps.ToArray());
            Graphics.DrawMeshInstanced(_quadMesh, 0, _lineMaterial, matrices.ToArray(),
                                       matrices.Count, props,
                                       UnityEngine.Rendering.ShadowCastingMode.Off,
                                       /* receiveShadows= */ false,
                                       /* layer= */ 0);
        }
    }
}
