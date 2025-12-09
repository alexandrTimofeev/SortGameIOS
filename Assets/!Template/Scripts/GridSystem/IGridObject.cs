using UnityEngine;
using System;
using System.Collections;

public interface IGridObject
{
    public abstract void SetPosition(Vector3 position, Vector2Int gridPosition, out Coroutine enumerator);
    public abstract Vector2Int GetPosition();
    public abstract GameObject GetMono();
}
