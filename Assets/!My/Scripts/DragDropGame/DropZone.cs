using System.Collections;
using UnityEngine;
using System;

public class DropZone : MonoBehaviour
{
    [SerializeField] private Vector2 size = Vector2.one;

    [SerializeField] private bool isInteractible = true;
    public bool IsInteractible => isInteractible;

    public event Action<DragObject> OnDrop;

    public bool TestPointInsideZone (Vector2 point)
    {
        return point.y > transform.position.y - (size.y / 2f) &&
            point.y < transform.position.y + (size.y / 2f) &&
            point.x > transform.position.x - (size.x / 2f) &&
            point.x < transform.position.x + (size.x / 2f);
    }

    public void Drop (DragObject dragObject)
    {
        OnDrop?.Invoke(dragObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, size);
    }

    public void SetInteractible(bool isInteractible)
    {
        this.isInteractible = isInteractible;
    }
}