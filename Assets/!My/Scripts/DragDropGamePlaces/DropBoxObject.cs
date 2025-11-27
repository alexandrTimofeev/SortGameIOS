using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DropBoxObject : MonoBehaviour
{
    public DropObjectType Type;
    public List<string> Tags = new List<string>();

    public bool AnyTag (params string[] tags)
    {
        return Tags.Any(tg => tags.Contains(tg));
    }

    public bool AllTag(params string[] tags)
    {
        return Tags.All(tg => tags.Contains(tg));
    }

    public void Init()
    {
        GetComponent<DragObject>().Init();
    }
}