using System;

public class ProgressObserver
{
    public event Action<float> OnProgressChange;
    public event Action<int> OnProgressEnd;

    private float progress;
    private int progressEtap;

    public virtual void SetProgress (float progressSet)
    {
        if (progress >= progressSet)
            return;

        progress = progressSet;

        if (progress >= 1f)
        {
            progressEtap++;
            progress = 0f;
            OnProgressEnd?.Invoke(progressEtap);
        }

        OnProgressChange?.Invoke(progress);
    }

    public virtual void UpdateData(params object[] vs)
    {
        float progressValue = (float)vs[0];
        SetProgress(progressValue);
    }
}
