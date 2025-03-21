using System;
using System.Reflection;
using CCE.Data;
using CCE.Utils;
using UnityEngine;

namespace CCE.GameUtils
{
    public class BackgroundClassFieldRenderer : MonoBehaviour, IClassFieldRenderer
    {
        [SerializeField] private GameObject BackgroundDisplayTemplate;

        public Type FieldType => typeof(Level.BackgroundData);

        public float RenderField(ClassFieldRenderInfo renderInfo)
        {
            var obj = Instantiate(BackgroundDisplayTemplate, renderInfo.FillTarget);
            obj.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(renderInfo.ElementLeftMargin, renderInfo.CurrentElementTopMargin);

            obj.GetComponent<ClassFieldDisplay>().FieldName.text =
                renderInfo.FieldInfo.GetCustomAttribute<DisplayableAttribute>().Name ?? renderInfo.FieldInfo.Name;

            obj.GetComponent<ImagePicker>().OnImagePicked += path =>
            {
                if ((Level.BackgroundData)renderInfo.FieldInfo.GetValue(renderInfo.TargetObject) == null)
                {
                    renderInfo.FieldInfo.SetValue(renderInfo.TargetObject, new Level.BackgroundData
                    {
                        Path = path
                    });
                }
                else
                {
                    ((Level.BackgroundData)renderInfo.FieldInfo.GetValue(renderInfo.TargetObject)).Path = path;
                }
            };

            return renderInfo.ElementSpacing * 1.5f;
        }
    }
}