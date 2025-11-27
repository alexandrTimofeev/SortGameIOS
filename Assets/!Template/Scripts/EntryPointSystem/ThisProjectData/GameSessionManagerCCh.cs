using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class GameSessionManagerCCh : MonoBehaviour
{
    public event Action<int> OnPhaseChange;
    private int phase;

    public static int Level;
    public static BattleModePlayerData PlayerData;
    public static EndLevelData NextLevelData;

    public CanonProjectilePlayer ProjectilePlayer { get; private set; }

    public void Init()
    {
        //StartCoroutine(CoroutineNextPhase());
        //ApplyNextLevelData();
    }

    private IEnumerator CoroutineNextPhase()
    {
        while (true)
        {
            float tm = GameEntryGameplayCCh.DataContainer.TimerPhase;
            while (tm > 0)
            {
                if(GamePause.IsPause == false)
                    tm -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            NextPhase();
            yield return new WaitForEndOfFrame();
        }
    }

    private void NextPhase()
    {
        phase++;
        OnPhaseChange?.Invoke(phase);
    }

    public void CalculateStart (Action onEnd, params Action[] actions)
    {
        StartCoroutine(CoroutineCalculate(onEnd, actions));
    }

    private IEnumerator CoroutineCalculate(Action onEnd, params Action[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            yield return new WaitForSeconds(1f);
            actions[i]?.Invoke();
        }
        yield return new WaitForSeconds(1f);
        onEnd?.Invoke();
    }

    public void SetPlayer(CanonProjectilePlayer projectilePlayer)
    {
        ProjectilePlayer = projectilePlayer;
    }

    public static void ClearForNextLevel ()
    {
        Level = 0;
        PlayerData = null;
        NextLevelData = null;
        BattleModeEnemyEncounter.LevelDificalty = 0;
    }

    public void ApplyNextLevelData()
    {
        if (NextLevelData == null)
            return;

        GameEntryGameplayCCh.DragGameManager.SetSteps(NextLevelData.maxSteps);
        GameEntryGameplayCCh.DragGameManager.SetAvilibleBoxes(NextLevelData.avilibleBoxes);
        GameEntryGameplayCCh.DragGameManager.SetUnlockBoxes(NextLevelData.unlcokBoxes);
        GameEntryGameplayCCh.DataContainer.MoneyContainer.SetValue(NextLevelData.money);
        //BattleModeEnemyEncounter.LevelDificalty;
    }
}

public class EndLevelData
{
    public int maxSteps;
    public int avilibleBoxes;
    public int unlcokBoxes;
    public int money;

    public EndLevelData(int maxSteps, int avilibleBoxes, int unlcokBoxes, int money)
    {
        this.maxSteps = maxSteps;
        this.avilibleBoxes = avilibleBoxes;
        this.unlcokBoxes = unlcokBoxes;
        this.money = money;
    }
}