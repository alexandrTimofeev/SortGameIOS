using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class GameSessionManager : MonoBehaviour
{
    public event Action<int> OnPhaseChange;
    public event Action OnPhaseEnd;
    private int phase;

    public CanonProjectilePlayer ProjectilePlayer { get; private set; }

    private CrackGridManager gridManager;
    private GlobalInputPlayerControllerMonoBase globalInput;

    public void Init(GlobalInputPlayerControllerMonoBase globalInput, ScoreSystem scoreSystem)
    {
        //StartCoroutine(CoroutineNextPhase());
        gridManager = FindObjectOfType<CrackGridManager>();
        gridManager.Init(scoreSystem);
        this.globalInput = globalInput;

        gridManager.OnEndBrokeBlock += () => gridManager.MoveInEmptySpaceBlocks();

        gridManager.OnStartBrokeBlock += () => globalInput.SetEnabled(false);
        gridManager.OnEndMoveBlock += () => globalInput.SetEnabled(true);
        gridManager.OnEndMoveBlock += () => OnPhaseEnd.Invoke();
        OnPhaseEnd += () => Debug.Log($"Save {scoreSystem.Score}");
    }

    public void NextPhase()
    {
        phase++;
        OnPhaseChange?.Invoke(phase);
    }

    /*private IEnumerator CoroutineNextPhase()
    {
        while (true)
        {
            float tm = GameEntryGameplay.DataContainer.TimerPhase;
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
    }*/
}