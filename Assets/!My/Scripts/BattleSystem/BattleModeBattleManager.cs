using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleModeBattleManager : MonoBehaviour
{
    private static BattleModeBattleManager instance;
    public static BattleModeBattleManager Instance
    {
        get { if (instance == null) instance = FindFirstObjectByType<BattleModeBattleManager>();  return instance; }
    }

    [Header("Настройки")]
    public GameObject playerAttackPrefab; // Префаб атаки игрока
    public List<DDOConvertAttackData> convertAttackDatas = new List<DDOConvertAttackData>();

    private BattleModePlayerData playerData;    
    [SerializeField] private GameObject playerObj;

    private BattleModeEnemyEncounter enemyEncounter;
    private BattleModeEnemyData enemyData;
    private BattleModeObjectVisuals enemyVisuals;
    private bool playerTurn = true;

    private DragDropGameManager dragDropGameManager;
    private DragDropManager dragDropManager => GameEntryGameplayCCh.DragManager;

    private float damageEnemyMultyply = 1f;
    private float nextMultyply = 1f;

    [Space]
    [SerializeField] private GameObject multyplyGO;
    [SerializeField] private TextMeshPro multyplyTmp;

    public event Action OnBattleStart;
    private event Action<bool> OnBattleComplete;
    public event Action<bool> OnBattleCompleteGlobal;
    public event Action<BattleManagerClearEvent> BattleClearEvent;

    private event Action OnPlayerLose;

    public void Init()
    {
        dragDropGameManager = FindFirstObjectByType<DragDropGameManager>();
        if (dragDropGameManager != null)
        {
            dragDropGameManager.OnClear += DragDropBattleWork;
            dragDropGameManager.OnReroll += RerollWork;
            dragDropGameManager.OnStepsEnd += StepsEndWork;
        }

        GameEntryGameplayCCh.BattleModeGManager.PlayerData.OnHealthOrDamage += CreatePlayerDamageUI;
    }

    #region StartBattle

    public void StartBattle(BattleModePlayerData playerData,
                            BattleModeEnemyEncounter enemyEncounter,
                            Action<bool> onComplete)
    {
        this.playerData = GameEntryGameplayCCh.BattleModeGManager.PlayerData;
        this.enemyEncounter = enemyEncounter;
        this.enemyData = enemyEncounter.EnemyData;
        this.enemyVisuals = enemyEncounter.GetComponent<BattleModeObjectVisuals>();
        this.OnBattleComplete = onComplete;

        enemyData.OnHealthOrDamage += CreateEnemyDamageUI;
        playerTurn = true;
        Debug.Log($"Бой начался: {playerData.Name} против {enemyData.Name}");

        OnBattleStart?.Invoke();
    }

    #endregion

    #region Player Attack

    public void PlayerAttack(BattleModeAttackData attackData)
    {
        if (!playerTurn || enemyData == null || enemyData.IsDead) return;
        StartCoroutine(PlayerAttackRoutine(attackData));
    }

    private IEnumerator PlayerAttackRoutine(BattleModeAttackData attackData)
    {
        // Анимация атаки игрока
        if (playerAttackPrefab != null && enemyVisuals != null)
        {
            yield return playerObj.GetComponent<BattleModeObjectVisuals>()
                                  .AnimateAttack(enemyEncounter.gameObject, playerAttackPrefab);
        }

        // Применяем урон
        enemyData.TakeDamage(attackData.GetDamage());
        Debug.Log($"{playerData.Name} атакует {enemyData.Name} на {attackData.Damage} урона!");

        if (enemyData.IsDead)
        {
            yield return enemyVisuals.PlayDeathAnimation(() =>
            {
                EndBattle(true);
            });
        }

        // **NextTurn() не вызывается автоматически**
    }

    #endregion

    #region Enemy Attack

    private IEnumerator EnemyAttackRoutine(BattleModeAttackData attackData)
    {
        // 1. Анимация атаки врага
        yield return enemyVisuals.AnimateAttack(playerObj, enemyEncounter.attackPrefab);

        // 2. Наносим урон
        playerData.TakeDamage(attackData.GetDamage());
        DoEnemyAttackEffect(attackData.AttackTypes);
        damageEnemyMultyply = 1f;

        // 3. Эффект попадания
        yield return playerObj.GetComponent<BattleModeObjectVisuals>().AnimateHit();

        // 4. Проверка смерти и завершение
        if (playerData.IsDead)
        {
            EndBattle(false);
            yield break;
        }

        // 5. Передача хода обратно игроку
        NextTurn();
    }

    private void DoEnemyAttackEffect(EnemyAttakType[] attackTypes)
    {
        foreach (EnemyAttakType tp in attackTypes)
            DoEnemyAttackEffect(tp);
    }

    private void DoEnemyAttackEffect(EnemyAttakType tp)
    {
        switch (tp)
        {
            case EnemyAttakType.None:
                break;
            case EnemyAttakType.LockBox:
                dragDropGameManager.LockBoxTemp();
                break;
            default:
                break;
        }
    }

    #endregion

    #region EndBattle
    private void EndBattle(bool playerWon)
    {
        Debug.Log($"Бой окончен. Победитель: {(playerWon ? "Игрок" : "Монстр")}");

        // Уведомляем менеджер игры, что встреча закончена
        if (playerWon && enemyEncounter != null)
        {
            enemyEncounter.CompleteInteraction();
            enemyEncounter = null;
        }
        if (!playerWon)
            OnPlayerLose?.Invoke();

        // Вызываем callback для боевого менеджера
        OnBattleComplete?.Invoke(playerWon);
        OnBattleComplete = null;
        OnBattleCompleteGlobal?.Invoke(playerWon);

        GameEntryGameplayCCh.BattleModeGManager.EndBattleWork();

        // Сброс состояния
        playerTurn = true;
        enemyData = null;
        enemyVisuals = null;
    }


    #endregion

    #region Drag & Drop Integration

    public void NextTurn()
    {
        if (enemyData == null)
            return;

        playerTurn = !playerTurn;


        if (!playerTurn)
        {
            // Ход врага
            StartCoroutine(EnemyAttackRoutine(new BattleModeAttackData(enemyData.AttackPower, multyply: damageEnemyMultyply, attackTypes: enemyData.AttackTypes)));
            dragDropGameManager.EndRound();
        }
        else
        {
            dragDropGameManager.StartNextRound();
            Debug.Log("Ход игрока.");
        }
    }

    private void DragDropBattleWork(DropBoxClearEvent clearEvent)
    {
        int dmg = 0;
        foreach (var dropBoxObject in clearEvent.dropBoxObjects)
        {
            dmg += GetDamageDDO(dropBoxObject);
        }

        BattleManagerClearEvent battleManager = new();
        if (clearEvent.dropBoxObjects[0].AnyTag("1"))
        {
            BattleModeAttackData attackData = new((int)(dmg), multyply: nextMultyply);
            PlayerAttack(attackData);
            battleManager.tagsAttak.Add("1");
            battleManager.attackDatas.Add(attackData);
        }

        if (clearEvent.dropBoxObjects[0].AnyTag("2"))
        {
            BattleModeAttackData attackData = new((int)(dmg), multyply: nextMultyply);
            PlayerAttack(new BattleModeAttackData((int)(dmg * 1.8f), multyply: nextMultyply));
            battleManager.tagsAttak.Add("2");
            battleManager.attackDatas.Add(attackData);
        }

        if (clearEvent.dropBoxObjects[0].AnyTag("3"))
        {
            if (UnityEngine.Random.value <= 0.6f)
            {
                dragDropGameManager.AddStep((int)(1 * nextMultyply));
                InterfaceManager.CreateFlyingText($"<size=35>+{1 * nextMultyply} steps", Color.white, GameEntryGameplayCCh.GetMousePoint(0.2f), null);
            }
            else
                InterfaceManager.CreateFlyingText($"<size=35>+0 steps", Color.gray, GameEntryGameplayCCh.GetMousePoint(0.2f), null);

            battleManager.tagsAttak.Add("3");
        }

        if (clearEvent.dropBoxObjects[0].AnyTag("4"))
        {
            playerData.Heal((int)(15 * nextMultyply));
            battleManager.tagsAttak.Add("4");
        }

        if (clearEvent.dropBoxObjects[0].AnyTag("5"))
        {
            SetDamageEnemyMultyplayer(damageEnemyMultyply * Mathf.Pow(0.7f, nextMultyply));
            battleManager.tagsAttak.Add("5");
        }

        if (clearEvent.dropBoxObjects[0].AnyTag("6"))
        {
            SetPlayerMultyply(nextMultyply + 1);
            battleManager.tagsAttak.Add("6");
        }
        else
            SetPlayerMultyply();

        battleManager.clearEvent = clearEvent;
        BattleClearEvent?.Invoke(battleManager);
    }

    private void SetPlayerMultyply(float multyply = 1f)
    {
        nextMultyply = multyply;
        multyplyGO.SetActive(multyply != 1f);
        multyplyTmp.text = $"x{System.Math.Round(multyply, 1)}";
    }

    private void SetDamageEnemyMultyplayer(float multyplayer)
    {
        damageEnemyMultyply = multyplayer;
        enemyEncounter.DamageChangeUI(enemyData.AttackPower, damageEnemyMultyply);
    }

    private void RerollWork(DropBoxGame game)
    {
        NextTurn();
    }

    private void StepsEndWork()
    {
        if (DragDropGameManager.TestAnyEmpty)
            EndRerolIsEmpty();
        NextTurn();
    }

    public void ClickNextTurn ()
    {
        if (!DragDropGameManager.TestAnyEmpty)
            LevelGenerator.RefillRandomBox();
        else
        {
            EndRerolIsEmpty();
        }

        if (playerTurn)
            NextTurn();
    }

    private static void EndRerolIsEmpty()
    {
        foreach (var dropBoxGame in LevelGenerator.AvailableBoxes)
        {
            if (dropBoxGame.IsInteractible && dropBoxGame.TestEmpty())
            {
                //LevelGenerator.RefillBox(dropBoxGame);
                dropBoxGame.TryCreateNextLine();
            }
        }
    }

    private int GetDamageDDO(DropBoxObject dropBoxObject)
    {
        int dmg = 0;
        foreach (var dDOConvert in convertAttackDatas)
        {
            if (dropBoxObject.AnyTag(dDOConvert.id))
                dmg += dDOConvert.damage;
        }
        return dmg;
    }

    #endregion

    private void CreatePlayerDamageUI(int changeHealth)
    {
        CreateDamageUI(changeHealth, playerObj.transform.position + (Vector3.back * 0.5f));
    }

    private void CreateEnemyDamageUI(int changeHealth)
    {
        if (enemyEncounter == null)
            return;

        CreateDamageUI(changeHealth, enemyEncounter.transform.position + (Vector3.back * 0.5f));
    }

    private void CreateDamageUI(int damage, Vector3 point)
    {
        InterfaceManager.CreateFlyingText($"{damage}", damage < 0 ? Color.red : Color.green, point, null);
    }

    public void SetPlayerSprite(Sprite sprite)
    {
        playerObj.GetComponent<BattleModeObjectVisuals>().SetSprite(sprite);
    }

    public void SetVFXPlayerAttak(GameObject vfxAttak)
    {
        playerAttackPrefab = vfxAttak;
    }

    public static string GetNameAttakFromID (string id)
    {
        foreach (var item in instance.convertAttackDatas)
        {
            if (item.id == id)
            {
                return item.title;
            }
        }
        return id;
    }

    private void OnDestroy()
    {
        GameEntryGameplayCCh.BattleModeGManager.PlayerData.OnHealthOrDamage -= CreatePlayerDamageUI;
    }
}

[Serializable]
public class DDOConvertAttackData
{
    public string id;
    public int damage;

    [Space]
    public string title;
}

public class BattleManagerClearEvent
{
    public DropBoxClearEvent clearEvent;
    public List<string> tagsAttak = new List<string>();
    public List<BattleModeAttackData> attackDatas = new List<BattleModeAttackData>();
}