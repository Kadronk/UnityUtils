using System;

namespace Kadronk.PolymorphicFields
{
    public class DisplayNameAttribute : Attribute
    {
        public string Name;

        public DisplayNameAttribute(string name)
        {
            Name = name;
        }
    }
}