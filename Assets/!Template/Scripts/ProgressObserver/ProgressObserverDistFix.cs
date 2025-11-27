public class ProgressObserverDistFix : ProgressObserver
{
    private float startDist;
    private float endDist;

    public void Init(float startDist, float endDist)
    {
        this.startDist = startDist;
        this.endDist = endDist;
        UpdateData(startDist);
    }

    public void UpdateData(float dist)
    {
        SetProgress((dist - startDist) / (endDist - startDist));
    }

    public override void UpdateData(params object[] vs)
    {
        UpdateData((float)vs[0]);
    }
}