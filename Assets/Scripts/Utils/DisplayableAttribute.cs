using System;

namespace CCE.Utils
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayableAttribute : Attribute
    {
        public string Filter;
        public float MinValue = 0;
        public float MaxValue = 1;
        public string Name;
        public string Section;
    }
}