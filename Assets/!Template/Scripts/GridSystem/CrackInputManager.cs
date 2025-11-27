using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrackInputManager : GlobalInputPlayerControllerMonoBase
{
    [SerializeField] private CrackGridManager gridManager;
    protected IInput input;

    public override void Init(IInput input)
    {
        this.input = input;
        input.OnBegan += ClickToPoint;
    }

    private void ClickToPoint(Vector2 point)
    {
        if (!IsEnabled)
            return;

        gridManager.ClickPoint(Camera.main.ScreenToWorldPoint(point));
    }
}

public abstract class GlobalInputPlayerControllerMonoBase : MonoBehaviour, IGlobalInputPlayerController
{
    protected bool IsEnabled = true;

    public abstract void Init(IInput input);

    public void SetEnabled(bool on)
    {
        IsEnabled = on;
    }
}

public interface IGlobalInputPlayerController
{
    public abstract void Init(IInput input);
}