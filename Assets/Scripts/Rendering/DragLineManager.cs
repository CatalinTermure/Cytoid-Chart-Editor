using UnityEngine;

namespace CCE.Rendering
{
    public class DragLineManager : MonoBehaviour
    {
        [SerializeField] private Mesh _quadMesh;
        [SerializeField] private Material _lineMaterial;

        [Range(0.0f, 1.0f)]
        public float ClipStart = 0.0f;
        [Range(0.0f, 1.0f)]
        public float ClipEnd = 1.0f;

        private MaterialPropertyBlock props;

        private const float DRAG_LINE_WIDTH = 0.16f;

        public void AddDragLine(Vector2 start, Vector2 end)
        {
            //
        }

        void Start()
        {
            props = new MaterialPropertyBlock();
            _lineMaterial.enableInstancing = true;
        }

        void Update()
        {
            props.SetVectorArray("_TilingProps", new Vector4[]
            {
                new Vector4(5 / 0.16f, ClipStart, ClipEnd, 0),
                new Vector4(3 / 0.16f, ClipStart, ClipEnd, 0),
            });
            Graphics.DrawMeshInstanced(_quadMesh, 0, _lineMaterial, new Matrix4x4[]
            {
                Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 45), new Vector3(DRAG_LINE_WIDTH, 5, 1)),
                Matrix4x4.TRS(new Vector3(2, 2, 0), Quaternion.Euler(0, 0, 45), new Vector3(DRAG_LINE_WIDTH, 3, 1)),
            }, 2, props, UnityEngine.Rendering.ShadowCastingMode.Off, false, 0);
        }
    }
}
