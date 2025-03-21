using System.Reflection;
using UnityEngine;

namespace CCE.Utils
{
    public record ClassFieldRenderInfo
    {
        public FieldInfo FieldInfo;
        public object TargetObject;
        public float CurrentElementTopMargin;
        public float ElementLeftMargin;
        public float ElementSpacing;
        public RectTransform FillTarget;
    }
}