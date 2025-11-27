using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressObserverPosFix : ProgressObserver
{
    private Vector3 startPos;
    private Vector3 endPos;

    public void Init (Vector3 startPos, Vector3 endPos)
    {
        this.startPos = startPos;
        this.endPos = endPos;
    }

    public void UpdateData(Vector3 position)
    {
        SetProgress((position - startPos).magnitude / (startPos - endPos).magnitude);
    }
}
