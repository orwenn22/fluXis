using System;
using JetBrains.Annotations;

namespace fluXis.Utils.Attributes;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Property)]
public class PlaceholderAttribute : Attribute
{
    public string Placeholder { get; set; }

    public PlaceholderAttribute(string placeholder)
    {
        Placeholder = placeholder;
    }
}
