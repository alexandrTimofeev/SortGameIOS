using System;
using UnityEngine;

public class DropPlace : MonoBehaviour
{
    [SerializeField] private DropZone dropZone;
    public DropZone DropZone => dropZone;

    [SerializeField] private SpriteRenderer placeSprite;
    private DragObject currentDragObject;

    [SerializeField] private bool isInteractible = true;
    public bool IsInteractible => isInteractible;

    public bool IsFull => currentDragObject != null;
    public DragObject CurrentDragObject  => currentDragObject;


    public event Action<DragObject> OnPut;
    public event Action<DragObject> OnGrap;

    void Start()
    {
        dropZone.OnDrop += TryDrop;
    }

    private void TryDrop(DragObject dragObject)
    {
        if (currentDragObject)
            return;

        PutObject(dragObject);
    }

    public void PutObject(DragObject dragObject)
    {
        currentDragObject = dragObject;
        currentDragObject.transform.position = transform.position;
        currentDragObject.OnGrap += GrapObject;
        dragObject.SetCurrentDropZone(dropZone);
        dropZone.SetInteractible(false);

        OnPut?.Invoke(dragObject);
    }

    private void GrapObject(DragObject dragObject)
    {
        if (dragObject != currentDragObject)
            return;

        OnGrap?.Invoke(currentDragObject);
        ThrowCurrentDragObject(false);

        currentDragObject = null;
    }

    public void ThrowCurrentDragObject(bool isNullNow = true)
    {
        if (!currentDragObject) 
            return;

        currentDragObject.transform.position += (Vector3)UnityEngine.Random.insideUnitCircle;
        currentDragObject.OnGrap -= GrapObject;
        dropZone.SetInteractible(isInteractible);

        if (isNullNow)
            currentDragObject = null;
    }

    public void DeliteCurrentDragObject()
    {
        if (currentDragObject == null)
            return;

        ThrowCurrentDragObject(false);
        Destroy(currentDragObject.gameObject);
        currentDragObject = null;

        dropZone.SetInteractible(true);
    }

    public void SetInteractible(bool isInteractible)
    {
        this.isInteractible = isInteractible;
        dropZone.SetInteractible(isInteractible);

        placeSprite.color = IsInteractible ? Color.wheat : Color.gray;
    }
}
