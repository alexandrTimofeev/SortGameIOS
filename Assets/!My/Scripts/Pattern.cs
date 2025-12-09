using System.Collections.Generic;
using System;

[Serializable]
public class Pattern
{
    public List<DropObjectType> Objects;

    public Pattern(params DropObjectType[] objects)
    {
        Objects = new List<DropObjectType>(objects);
    }

    public Pattern(IEnumerable<DropObjectType> objects)
    {
        Objects = new List<DropObjectType>(objects);
    }

    public bool Matches(List<DropObjectType> other)
    {
        if (other == null || other.Count != Objects.Count) return false;
        for (int i = 0; i < Objects.Count; i++)
            if (Objects[i] != other[i]) return false;
        return true;
    }
}