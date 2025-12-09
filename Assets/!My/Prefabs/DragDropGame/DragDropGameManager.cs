using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragDropGameManager : MonoBehaviour
{
    [SerializeField] private DropBoxGame[] dropBoxGames;

    public static DragDropGameManager instance;
    public LevelGenerator levelGenerator;

    public bool isGamePlay => GameEntryGameplayCCh.DragManager.IsGamePlay;
    private DragDropManager dropManager => GameEntryGameplayCCh.DragManager;

    [SerializeField] private int steps = 10;
    [SerializeField] private int stepsOnStart = 10;
    [SerializeField] private int avilibleBoxes = 5;
    [SerializeField] private int unlockBoxes = 3;
    private int lockBoxes => Mathf.Clamp(avilibleBoxes - unlockBoxes, 0, avilibleBoxes);

    public int Steps => steps;

    public int AvilibleBoxes => avilibleBoxes;
    public int UnlockBoxes => unlockBoxes;

    public static bool NotGetSteps = false;
    public static bool TestAnyEmpty => LevelGenerator.AvailableBoxes.Any(dbg => dbg.IsInteractible && dbg.TestEmpty());

    public event Action<DropBoxClearEvent> OnClear;
    public event Action OnEndGame;
    public event Action<DropBoxGame> OnReroll;
    public event Action<DropBox> OnPlaceFalse;
    public event Action OnStepsEnd;

    public void Init()
    {
        instance = this;

        if (dropBoxGames == null || dropBoxGames.Length == 0)
            dropBoxGames = FindObjectsByType<DropBoxGame>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                            .Where(d => d != null)
                            .ToArray();

        dropManager.OnDropInZone += DropInZoneWork;

        levelGenerator = FindFirstObjectByType<LevelGenerator>(FindObjectsInactive.Include);
        levelGenerator.OnReroll += WorkReroll;
        BattleModeBattleManager.Instance.OnBattleStart += StartBattle;
        BattleModeBattleManager.Instance.OnBattleCompleteGlobal += EndBattle;

        UpdateSteps();
    }

    private void WorkReroll(DropBoxGame game)
    {
        OnReroll?.Invoke(game);
    }

    public void Clear(DropBoxClearEvent dropBoxClearEvent)
    {
        OnClear?.Invoke(dropBoxClearEvent);
        ChangeNotGetSteps(true);
    }

    public void PlaceFalse(DropBox dropBox)
    {
        OnPlaceFalse?.Invoke(dropBox);
    }

    private void EndDropGame(DropBoxGame dropBox)
    {
        foreach (var dropBoxGame in dropBoxGames)
        {
            if (dropBoxGame != null && dropBoxGame.TestEmpty() == false)
                return;
        }

        OnEndGame?.Invoke();
    }

    public void SetGamePlay(bool gamePlay)
    {
        dropManager.SetGamePlay(gamePlay);
    }

    public void StartNextRound()
    {
        UpdateSteps();
        /*foreach (var dropBoxGame in LevelGenerator.AvailableBoxes)
        {
            if (dropBoxGame.IsInteractible && dropBoxGame.TestEmpty())
            {
                //LevelGenerator.RefillBox(dropBoxGame);
                dropBoxGame.TryCreateNextLine();
            }
        }*/
        SetGamePlay(true);
    }

    public void EndRound()
    {
        SetGamePlay(false);
    }

    public void UpdateSteps()
    {
        UpdateSteps(stepsOnStart);
    }

    public void UpdateSteps(int steps)
    {
        this.steps = steps;
        InterfaceManager.BarMediator.SetMaxForID("Steps", stepsOnStart);
        InterfaceManager.BarMediator.ShowForID("Steps", steps);
    }

    // Уменьшение шагов на 1
    public void RemoveStep(int count = 1)
    {
        if (steps <= 0)
            return;

        steps -= count;
        if (steps <= 0)
        {
            steps = 0;
            OnStepsEnd?.Invoke();
        }

        InterfaceManager.BarMediator.ShowForID("Steps", steps);
    }

    // Увеличение шагов на 1
    public void AddStep(int count = 1)
    {
        steps += count;
        steps = Mathf.Clamp(steps, -1, stepsOnStart);

        InterfaceManager.BarMediator.ShowForID("Steps", steps);
    }

    private void DropInZoneWork(DragObject dragObject)
    {
        StartCoroutine(StepDropCoroutine());
    }

    public IEnumerator StepDropCoroutine()
    {
        ChangeNotGetSteps(false);
        yield return new WaitForSeconds(0.6f);
        if (!NotGetSteps)
            RemoveStep();
    }

    public static void ChangeNotGetSteps (bool gstep)
    {
        //Debug.Log($"gstep {gstep}");
        NotGetSteps = gstep;
    }

    public void StartBattle()
    {
        levelGenerator.GenerateLevel(avilibleBoxes, lockBoxes);
    }

    public void EndBattle(bool playerWon)
    {
        levelGenerator.ClearLevel();
        //levelGenerator.SetAvailableBoxes(0);
    }

    public void AddStepMax(int addStepMax)
    {
        stepsOnStart += addStepMax;
        UpdateSteps();
    }

    public void SetSteps(int stepsStart)
    {
        stepsOnStart = stepsStart;
        steps = stepsStart;
        UpdateSteps();
    }

    public void RemoveMaxSteps(int count = 1)
    {
        stepsOnStart -= count;
        UpdateSteps();
    }

    public void AddAvilibleBoxes(int addAvilibleBoxes)
    {
        avilibleBoxes += addAvilibleBoxes;
        if (UnityEngine.Random.value <= 0.5f)
            AddUnlockBoxes(-1);
    }

    public void RemoveAvilibleBoxes(int count)
    {
        avilibleBoxes -= count;
        if (UnityEngine.Random.value <= 0.5f)
            AddUnlockBoxes(1);
    }

    public void SetAvilibleBoxes(int avilibleBoxesStart)
    {
        avilibleBoxes = avilibleBoxesStart;
    }

    public void AddUnlockBoxes(int addUnlockBoxes)
    {
        SetUnlockBoxes(unlockBoxes - addUnlockBoxes);
    }

    public void SetUnlockBoxes(int unlcokBoxes)
    {
        unlockBoxes = Mathf.Clamp(unlcokBoxes, 3, LevelGenerator.AllBoxes.Length - 1);
    }

    public void LockBoxTemp()
    {
        levelGenerator.LockRandomBox();
    }
}

public class DropBoxClearEvent
{
    public DropBox dropBox;
    public DropBoxObject[] dropBoxObjects;

    public DropBoxClearEvent(DropBox dropBox, DropBoxObject[] dropBoxObjects)
    {
        this.dropBox = dropBox;
        this.dropBoxObjects = dropBoxObjects;
    }
}
