using System;

namespace CCE.Data
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayableAttribute : Attribute
    {
        public string Filter;
        public float MaxValue = 1;
        public float MinValue = 0;
        public string Name;
        public string Section;
    }
}