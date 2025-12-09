using System;
using System.Collections;
using System.Drawing;
using UnityEngine;

public class DragDropController
{
    private IInput input;
    private DragDropManager dragDropManager;

    private DragObject overDragObject;

    private bool isOver => overDragObject;
    private bool isDrag => dragDropManager.IsDrag;

    public event Action<DragObject> OnOver;
    public event Action<DragObject> OnOverEnd;
    public event Action<DragObject> OnDrag;
    public event Action<Vector2> OnMove;
    public event Action OnDrop;

    public void Init(IInput input, DragDropManager dragDropManager)
    {
        this.input = input;
        this.dragDropManager = dragDropManager;

        input.OnBegan += InvokeBegan;
        input.OnEnded += InvokeEnded;
        input.OnAnyPosition += InvokeAnyPosition;
    }

    private void InvokeAnyPosition(Vector2 vector)
    {
        TryOver(vector);

        if (isDrag)
            Move(vector);
    }

    private void InvokeEnded(Vector2 vector)
    {
        if (isDrag == false)
            return;

        overDragObject = null;
        OnDrop?.Invoke();
    }

    private void InvokeBegan(Vector2 vector)
    {
        TryOver(vector);

        if (isDrag)
            return;

        if (isOver)
            OnDrag?.Invoke(overDragObject);
    }

    private void TryOver(Vector2 point)
    {
        DragObject dragObject = GetDragObjectInPoint(point);

        if (dragObject)
        {
            if (dragObject == overDragObject)
                return;

            if (overDragObject != null)
                OnOverEnd?.Invoke(overDragObject);

            overDragObject = dragObject;
            OnOver?.Invoke(overDragObject);

            return;
        }

        if (overDragObject != null)        
            OnOverEnd?.Invoke(overDragObject);        
        overDragObject = null;
    }

    private void Move(Vector2 point)
    {
        if (isDrag == false)
            return;

        OnMove?.Invoke(Camera.main.ScreenToWorldPoint(point));
    }

    private DragObject GetDragObjectInPoint(Vector2 point, bool convertInCameraPosition = true)
    {
        if (convertInCameraPosition)
            point = Camera.main.ScreenToWorldPoint(point);
        Collider2D collider2D = Physics2D.OverlapCircle(point, 0.05f);
        if (collider2D && collider2D.TryGetComponent(out DragObject dragObject))
            return dragObject;
        return null;
    }
}