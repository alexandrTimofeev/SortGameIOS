using System;
using System.Collections.Generic;

[Serializable]
public class Box
{
    public int Id;
    public bool IsLocked;
    public bool IsCleared;
    public Pattern UnlockPattern;
    public List<DropObjectType> Objects = new List<DropObjectType>();
    public List<List<DropObjectType>> NextObjectSets = new List<List<DropObjectType>>();

    public Pattern AsPattern() => new Pattern(Objects);
    public void Clear()
    {
        IsCleared = true;
        Objects.Clear();
    }
}