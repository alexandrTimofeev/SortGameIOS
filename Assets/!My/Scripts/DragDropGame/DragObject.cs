using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class DragObject : MonoBehaviour
{
    private DropZone currentDropZone;
    public DropZone CurrentDropZone => currentDropZone;

    public Action<DragObject> OnGrap;
    public Action<DragObject> OnDrop;

    private Vector3 scaleOne = Vector3.one;

    public AudioClip ClipUse;

    private void Awake()
    {
        scaleOne = transform.localScale;
    }

    public void Grap()
    {
        OnGrap?.Invoke(this);
    }

    public void Drop(DropZone dropZone = null)
    {
        //currentDropZone = dropZone;
        OnDrop?.Invoke(this);
    }

    public void Init()
    {
       GameEntryGameplayCCh.DragManager.AddObjectInList(this);
    }

    private void OnDestroy()
    {
        GameEntryGameplayCCh.DragManager.RemoveObjectOutList(this);
    }

    public void ReturnInDropZone()
    {
        if(currentDropZone != null)
            currentDropZone.Drop(this);
    }

    public void SetCurrentDropZone(DropZone dropZone)
    {
        currentDropZone = dropZone;
    }

    public Tweener ScaleCreateAnimation ()
    {
        transform.localScale = Vector3.zero;
        return transform.DOScale(scaleOne, 0.5f);
    }

    public Tweener ScalePunchAnimation()
    {
        return transform.DOPunchScale(scaleOne * 1.5f, 0.5f, 2, 1);
    }
}