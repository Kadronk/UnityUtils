using System;
using UnityEngine;

namespace Kadronk.PolymorphicFields
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PolymorphableAttribute : PropertyAttribute
    {
        public readonly bool IncludeParent = false;
        public readonly LabelStyle Style = LabelStyle.Append;
        
        public PolymorphableAttribute() { }
        
        public PolymorphableAttribute(LabelStyle labelStyle = LabelStyle.Append, bool includeParent = false) {
            IncludeParent = includeParent;
            Style = labelStyle;
        }
        
        public PolymorphableAttribute(bool includeParent = false, LabelStyle labelStyle = LabelStyle.Append) : this(labelStyle, includeParent) { }
    }
    
    public enum LabelStyle
    {
        /// <summary>Use the provided label.</summary>
        DontChange,
        /// <summary>Appends the type's display name to the label.</summary>
        Append,
        /// <summary>Clears the label and replace it with the type's display name.</summary>
        Replace,
        /// <summary>Empty label, use that if something else should override it.</summary>
        Empty
    }
}
