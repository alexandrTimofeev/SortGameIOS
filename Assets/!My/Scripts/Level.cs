using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Level
{
    public List<Box> Boxes = new List<Box>();
    public bool IsCompleted => Boxes.All(b => b.IsCleared);
    public bool AllUnlocked => Boxes.All(b => !b.IsLocked);
}
