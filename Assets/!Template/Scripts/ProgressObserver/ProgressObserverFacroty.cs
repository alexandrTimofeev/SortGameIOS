using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ProgressObserverFacroty
{
    private static ProgressObserver progressObserver;

    public static ProgressObserver CreateAndInit()
    {
        if (progressObserver == null)        
            progressObserver = new ProgressObserver();

        return progressObserver;
    }

    public static ProgressObserver CreateAndInit(Vector3 startPos, Vector3 endPos)
    {
        if(progressObserver == null || (progressObserver as ProgressObserverPosFix) == null)        
            progressObserver = new ProgressObserverPosFix();

        (progressObserver as ProgressObserverPosFix).Init(startPos, endPos);

        return progressObserver;
    }

    public static ProgressObserver CreateAndInit(float endDist, float startDist = 0f)
    {
        if (progressObserver == null || (progressObserver as ProgressObserverDistFix) == null)
            progressObserver = new ProgressObserverDistFix();

        (progressObserver as ProgressObserverDistFix).Init(startDist, endDist);

        Debug.Log($"CreateAndInit startDist = {startDist}, endDist = {endDist}");

        return progressObserver;
    }

    public static void Clear()
    {
        progressObserver = null;
    }
}