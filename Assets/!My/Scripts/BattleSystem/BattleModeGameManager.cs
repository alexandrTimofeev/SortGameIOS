using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

/// <summary>
/// Главный управляющий класс игрового цикла.
/// Отвечает за переход между объектами (взаимодействия, враги и т.д.)
/// Теперь объекты создаются из префабов и удаляются после встречи.
/// </summary>
public class BattleModeGameManager : MonoBehaviour
{
    [Header("Настройки игры")]
    [SerializeField] private List<GameObject> encounterPrefabs; // Префабы встреч
    [SerializeField] private List<GameObject> enemyPrefabs; // Префабы встреч

    [Header("Настройки игры")]
    [SerializeField] private List<GameObject> encountersList;
    [SerializeField] private Transform spawnPoint;              // Точка появления объектов

    [Header("Игрок")]
    public GameObject playerGameObject; // ссылка на объект игрока в сцене
    [SerializeField] private List<BattleModePlayerDataSO> playerDataSO = new List<BattleModePlayerDataSO>(); // ScriptableObject игрока
    public static string IDCharacter = "";
    [SerializeField] private BattleModePlayerData playerData;     // Данные игрока

    private int currentIndex = 0;
    private BattleModeInteractableObject currentObjectInstance;

    public BattleModePlayerData PlayerData => playerData;

    public List<BattleModePlayerDataSO> PlayerDataSO => playerDataSO;

    public event Action OnWin;

    public void Init()
    {
        if (GameSessionManagerCCh.PlayerData != null)
            playerData = GameSessionManagerCCh.PlayerData;
        else if (playerDataSO.Count != 0)
        {
            BattleModePlayerDataSO so = playerDataSO.FirstOrDefault((data) => data.ID == IDCharacter);
            playerData = (so != null) ? so.GetPlayerData() : playerDataSO[0].GetPlayerData();
        }

        BattleModeBattleManager.Instance.SetPlayerSprite(playerData.sprite);
        BattleModeBattleManager.Instance.SetVFXPlayerAttak(playerData.vfxAttak);
        GameEntryGameplayCCh.DragGameManager.SetAvilibleBoxes(playerData.AvilibleBoxesStart);
        GameEntryGameplayCCh.DragGameManager.SetSteps(playerData.StepsStart);

            playerData.OnHealthChanged += (health, maxHealth) =>
            {
                InterfaceManager.BarMediator.SetMaxForID("Health", maxHealth);
                InterfaceManager.BarMediator.ShowForID("Health", health);
            };
        playerData.Heal(0);

        GameSessionManagerCCh.PlayerData = playerData;
    }

    public void GenerateList (int encounters, int enemies)
    {
        encountersList.Clear();
        int encounterAtEnemy = encounters / enemies;

        encountersList.Add(enemyPrefabs[0]);
        for (int i = 0; i < enemies; i++)
        {
            for (int q = 0; q < encounterAtEnemy; q++)
            {
                encountersList.Add(encounterPrefabs[UnityEngine.Random.Range(0, encounterPrefabs.Count)]);
            }
            encountersList.Add(enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Count)]);
        }
    }
    
    private void Start()
    {
        FindAnyObjectByType<BattleModeBattleManager>().BattleClearEvent += ClearEventWork;

        StartGame();
    }

    public void StartGame()
{
    // Загружаем данные игрока из ScriptableObject
    currentIndex = 0;
    ActivateNextObject();
}

    public void ActivateNextObject()
    {
        // Если все объекты закончились
        if (currentIndex >= encountersList.Count)
        {
            OnWin?.Invoke();
            return;
        }

        // Удаляем предыдущий объект, если остался
        if (currentObjectInstance != null)
        {
            currentObjectInstance.OnInteractionComplete -= HandleObjectComplete;
            Destroy(currentObjectInstance.gameObject);
        }

        // Создаем новый объект встречи
        GameObject newObj = Instantiate(encountersList[currentIndex], spawnPoint.position, Quaternion.identity);
        currentObjectInstance = newObj.GetComponent<BattleModeInteractableObject>();

        if (currentObjectInstance == null)
        {
            Debug.LogError($"❌ Префаб {encountersList[currentIndex].name} не содержит BattleModeInteractableObject!");
            return;
        }

        // Подписываемся на событие завершения взаимодействия
        currentObjectInstance.OnInteractionComplete += HandleObjectComplete;

        // Активируем объект (передаем данные игрока)
        currentObjectInstance.Activate(playerData, playerGameObject);

        currentIndex++;
    }

    private void HandleObjectComplete()
    {
        // Удаляем объект после завершения встречи
        if (currentObjectInstance != null)
        {
            currentObjectInstance.OnInteractionComplete -= HandleObjectComplete;
            Destroy(currentObjectInstance.gameObject);
        }

        // Переходим к следующему объекту
        ActivateNextObject();
    }

    public void AddPlayerMaxHeal(int addMaxHeal)
    {
        playerData.MaxHealth += addMaxHeal;
        playerData.Heal(addMaxHeal);
    }

    private void ClearEventWork(BattleManagerClearEvent clearEvent)
    {
        if (clearEvent == null)
            return;

        foreach (var attackData in clearEvent.attackDatas)
        {
            int addScore = attackData.GetDamage() * 10;
            GameEntryGameplayCCh.GameScoreSystem.AddScore(addScore,  Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward);
        }

        if (clearEvent.tagsAttak.Contains("3"))        
            GameEntryGameplayCCh.GameScoreSystem.AddScore(300, Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward);        
        if (clearEvent.tagsAttak.Contains("4"))
            GameEntryGameplayCCh.GameScoreSystem.AddScore(300, Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward);
        if (clearEvent.tagsAttak.Contains("5"))
            GameEntryGameplayCCh.GameScoreSystem.AddScore(300,  Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward);
        if (clearEvent.tagsAttak.Contains("6"))
            GameEntryGameplayCCh.GameScoreSystem.AddScore(300,  Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward);
    }

    public void EndBattleWork()
    {
        playerData.Heal(playerData.MaxHealth / 5);
        GameEntryGameplayCCh.DragGameManager.UpdateSteps();
    }
}