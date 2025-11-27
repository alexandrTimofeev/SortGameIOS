using UnityEngine;

public class GameEntryMenuCCh : GameEntryMenu
{

    public override void Init()
    {
        base.Init();
        Debug.Log("GameEntryMenuChR Init");

        InterfaceManager.Init();
        InterfaceManager.OnClickCommand += (cmd) =>
        {
            switch (cmd)
            {
                case InterfaceComand.PlayGame:
                    GameSceneManager.LoadGame();
                    break;
                default:
                    break;
            }
        };

        InterfaceManager.BarMediator.ShowForID("Best", LeaderBoard.GetBestScore());
        InterfaceManager.BarMediator.ShowForID("MaxLevel", PlayerPrefs.GetInt("MaxLevel", 0));
    }
}

public static class IQSystem
{
    public static bool IsIntcraceAnim;

    public static void IncraceIQ()
    {
        int iq = GetIQ();
        SetIQ(iq + Random.Range(1, 3));
        IsIntcraceAnim = true;
        PlayerPrefs.SetInt("IsSeeIQ", 1);
    }

    public static int GetIQ()
    {
        return PlayerPrefs.GetInt("IQ", 99);
    }

    private static void SetIQ(int iq)
    {
        PlayerPrefs.SetInt("IQ", iq);
    }
}
 