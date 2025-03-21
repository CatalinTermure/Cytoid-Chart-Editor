using System;

namespace CCE.Utils
{
    public interface IClassFieldRenderer
    {
        Type FieldType { get; }
        float RenderField(ClassFieldRenderInfo renderInfo);
    }
}