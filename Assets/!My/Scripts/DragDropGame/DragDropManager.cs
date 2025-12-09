using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DragDropManager
{
    private List<DragObject> dragObjects = new List<DragObject>();
    private List<DropZone> dropZones = new List<DropZone>();

    private IInput input;
    private DragDropController controller;

    private bool isGamePlay = true;

    public bool IsDrag => currentDragObject != null;

    public bool IsGamePlay => isGamePlay;


    private DragObject currentDragObject;
    public DragObject CurrentDragObject => currentDragObject;

    private static bool isReturnInDropZone = true;

    public event Action<DragObject> OnDrag;
    public event Action<Vector2> OnMove;
    public event Action<DragObject> OnDrop;
    public event Action<DragObject> OnDropInZone;

    public void Init(IInput input)
    {
        ClearAll();
        FindAll();

        this.input = input;
        controller = new DragDropController();
        controller.Init(input, this);

        controller.OnDrag += Drag;
        controller.OnDrop += Drop;
        controller.OnMove += Move;
    }

    private void ClearAll()
    {
        dragObjects.Clear();
        dropZones.Clear();
    }

    private void FindAll()
    {
        DragObject[] foundDragObjects = GameObject.FindObjectsByType<DragObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        DropZone[] foundDropZones = GameObject.FindObjectsByType<DropZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var dragObject in foundDragObjects)
            AddObjectInList(dragObject);

        foreach (var dropZone in foundDropZones)
            AddZoneInList(dropZone);
    }

    private void Drag(DragObject dragObject)
    {
        if (!isGamePlay || IsDrag)
            return;

        currentDragObject = dragObject;
        OnDrag?.Invoke(currentDragObject);
        currentDragObject.Grap();
    }

    private void Move(Vector2 position)
    {
        if (!isGamePlay || !IsDrag)
            return;

        currentDragObject.transform.position = position;
        OnMove?.Invoke(position);
    }

    private void Drop()
    {
        if (!IsDrag)
            return;

        DropZone dropZoneNew = null;
        foreach (var dropZone in dropZones)
        {
            if (dropZone.IsInteractible && dropZone.TestPointInsideZone(currentDragObject.transform.position))
            {
                OnDropInZone?.Invoke(currentDragObject);
                dropZone.Drop(currentDragObject);
                dropZoneNew = dropZone;
                break;
            }
        }

        if (dropZoneNew == null && isReturnInDropZone)
            currentDragObject.ReturnInDropZone();

        OnDrop?.Invoke(currentDragObject);
        currentDragObject.Drop(dropZoneNew);
        currentDragObject = null;
    }

    public void AddObjectInList(DragObject dragObject)
    {
        if (!dragObjects.Contains(dragObject))
            dragObjects.Add(dragObject);
    }

    public void RemoveObjectOutList(DragObject dragObject)
    {
        dragObjects.Remove(dragObject);
    }

    public void AddZoneInList(DropZone dropZone)
    {
        if (!dropZones.Contains(dropZone))
            dropZones.Add(dropZone);
    }

    public void RemoveZoneOutList(DropZone dropZone)
    {
        dropZones.Remove(dropZone);
    }

    public void SetGamePlay(bool gamePlay)
    {
        isGamePlay = gamePlay;
        if (!isGamePlay)
        {
            Drop();
        }
    }
}