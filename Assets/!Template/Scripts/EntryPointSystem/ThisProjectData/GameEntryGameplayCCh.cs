using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-99)]
public class GameEntryGameplayCCh : GameEntryGameplay
{
    private GlobalMover globalMover;
    private Spawner spawner;
    private GameStateManager stateManager;
    private SpawnManager spawnManager;
    private ResourceSystem resourceSystem;
    private GameSessionManagerCCh gameManager;

    public static GameSessionDataContainer DataContainer { get; private set; }

    private PlayerChCF player;
    private TimerStarter timerStarter;
    private GameTimer gameTimer;

    public static LevelData DataLevel;
    public static ScoreSystem GameScoreSystem;
    public static InteractionChoiceMediator InteractionMediator;

    private GrappableObjectMediator ObjectMediator;
    public static IInput input;
    public static AchiviementMediator AchivMediator;
    public static DragDropManager DragManager;
    public static DragDropGameManager DragGameManager;
    public static BattleModeGameManager BattleModeGManager;

    public override void Init()
    {
        //base.Init();
        Debug.Log("GameEntryGameplay Init");

        InitializeReferences();
        InitializeLevel();
        InitializeInterface();
        InitializeAudio();

        SetupGameManager();
        SetupScoreSystem();
        SetupSpawnerAndProgressBar();
        SetupInterfaceEvents();
        SetupHealthContainer();
        SetupSpawnManager();
        SetupGameStateEvents();
        SetupPlayer();
        SetupWinLoseCondition();
        SetupBonuses();
        SetupAchivmients();
        SetupSkins();
        SetupInteractionMediator();

        BattleModeEnemyEncounter.LevelDificalty = GameSessionManagerCCh.Level;
        BattleModeGManager.GenerateList((GameSessionManagerCCh.Level + 1) * 2, (GameSessionManagerCCh.Level + 1) * 2);

        BattleModeGManager.PlayerData.OnDeath += Lose;
        BattleModeGManager.OnWin += () => stateManager.SetState(GameState.Win);

        gameManager.ApplyNextLevelData();        

        GamePause.SetPause(false);
        stateManager.SetState(GameState.Game);
    }

    private void Lose()
    {
        BattleModeGManager.PlayerData.OnDeath -= Lose;
        stateManager.SetState(GameState.Lose);
    }

    private void InitializeReferences()
    {
        input = InputFabric.GetOrCreateInpit();
        globalMover = Object.FindFirstObjectByType<GlobalMover>();
        spawner = Object.FindFirstObjectByType<Spawner>();
        DragGameManager = Object.FindFirstObjectByType<DragDropGameManager>();

        AchivMediator = new AchiviementMediator();
        ObjectMediator = new GrappableObjectMediator();
        GameScoreSystem = new ScoreSystem();
        DragManager = new DragDropManager();

        GrapCollider.Mediator = ObjectMediator;
        stateManager = new GameStateManager();
        spawnManager = new SpawnManager();
        resourceSystem = new ResourceSystem();
        InteractionMediator = new InteractionChoiceMediator();

        gameManager = (new GameObject("GameManager")).AddComponent<GameSessionManagerCCh>();

        DataContainer = Resources.Load<GameSessionDataContainer>("GameSessionDataContainer").Clone();
        if (DataLevel == null)
            DataLevel = Resources.Load<LevelData>($"Levels/{DataContainer.StandartLevelData}");

        /*timerStarter = (new GameObject()).AddComponent<TimerStarter>();
        if(timerStarter != null && DataLevel != null)
            timerStarter.Play(DataLevel.Timer);
        else
            timerStarter.Stop();
        gameTimer = timerStarter.Timer;*/

        //timerStarter.IsUnTimeScale = true;
    }

    private void InitializeLevel()
    {
        if (DataLevel == null)
            return;

        Transform levelTr = GameObject.Find("Level").transform;
        if (DataLevel.LevelPrefab)
            GameObject.Instantiate(DataLevel.LevelPrefab, levelTr);


        GameObject background = GameObject.Find("Background");
        if (background.TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            if (DataLevel != null && DataLevel.Background)
                spriteRenderer.sprite = DataLevel.Background;
        if (GameSessionManagerCCh.Level > 0)
            background.GetComponent<ColorChanger>().RandomColorGradient();
        }
    }

    private void InitializeInterface()
    {
        InterfaceManager.Init();
        InterfaceManager.BarMediator.ShowForID("Score", 0);
        InterfaceManager.BarMediator.ShowForID("Level", GameSessionManagerCCh.Level + 1);
    }

    private void InitializeAudio()
    {
        AudioManager.Init();
        AudioManager.PlayMusic();

        DataContainer.OnChangeSpeedGame += (speed) => AudioManager.SetSpeedMusic(((speed - 1f) * 0.5f) + 1f);
    }

    private void SetupScoreSystem()
    {
        GameScoreSystem.OnScoreChange += (score, point) =>
        {
            InterfaceManager.BarMediator.ShowForID("Score", score);
        };

        GameScoreSystem.OnAddScore += (score, point) => DataContainer.MoneyContainer.AddValue(score / 80);
        DataContainer.MoneyContainer.OnChangeValue += (value, delta) => InterfaceManager.BarMediator.ShowForID("Money", value);
        DataContainer.MoneyContainer.OnChangeValue += (value, delta) => 
        InterfaceManager.CreateFlyingText($"<size=20>{(delta < 0 ? "" : "+")}{delta}<color=yellow>$", delta < 0 ? Color.red : Color.white, 
        Camera.main.ScreenToWorldPoint(input.GetOverPosition()) + Vector3.forward, null);
        DataContainer.MoneyContainer.UpdateValue();

        //scoreSystem.OnAddScore += InterfaceManager.CreateScoreFlyingText;
        //scoreSystem.OnRemoveScore += InterfaceManager.CreateScoreFlyingText;
    }

    private void SetupSpawnerAndProgressBar()
    {
        if (spawner == null)
            return;

        spawner.OnInstructionProgress += (id, progress) =>
        {
            InterfaceManager.BarMediator.ShowForID("Progress", progress);
        };
    }

    private void SetupInterfaceEvents()
    {
        InterfaceManager.OnClose += (window) =>
        {
            if (window is PauseWindowUI)
                stateManager.BackState();
        };

        InterfaceManager.OnOpen += (window) =>
        {
            if (window is PauseWindowUI)
                stateManager.SetState(GameState.Pause);
        };     

        if (gameTimer != null)
            gameTimer.OnTick += (s) => InterfaceManager.BarMediator.ShowForID("Timer", s);
    }

    private void SetupHealthContainer()
    {
        DataContainer.HealthContainer.OnChangeValue += (life, delta) => InterfaceManager.BarMediator.ShowForID("Life", life);
        DataContainer.HealthContainer.OnChangeValue += (life, delta) => 
        InterfaceManager.BarMediator.SetMaxForID("Life", DataContainer.HealthContainer.ClampRange.y);
        DataContainer.HealthContainer.UpdateValue();
    }

    private void SetupSpawnManager()
    {
        if (DataContainer.UseSpawnManager == false)
            return;

        var settings = Resources.Load<SpawnerSettings>("Spawn/SpawnerSettings");
        spawnManager.Init(spawner, settings);

        if(spawner)
            spawnManager.OnChangeSpeed += spawner.SetSpeed;
        spawnManager.OnChangeSpeed += (speed) => DataContainer.SpeedGame = speed;
        if(globalMover)
            spawnManager.OnChangeSpeed += globalMover.SetSpeedCoef;
    }

    private void SetupGameStateEvents()
    {
        /*
         TutorialWindowUI tutorialWindow = InterfaceManager.CreateAndShowWindow<TutorialWindowUI>();
        tutorialWindow.OnClose += (win) => stateManager.SetState(GameState.Game);
        tutorialWindow.Init(input);
        */

        stateManager.OnWin += () =>
        {
            RecordData recordData = LeaderBoard.GetScore($"score_{LevelSelectWindow.CurrentLvl}");
            bool newRecord = recordData == null || recordData.score < GameScoreSystem.Score;
            InterfaceManager.ShowWinWindow(GameScoreSystem.Score, (recordData != null ? recordData.score : 0));
            LeaderBoard.SaveScore($"score_{LevelSelectWindow.CurrentLvl}", GameScoreSystem.Score);
            LevelSelectWindow.CompliteLvl();

            if (GameSessionManagerCCh.Level + 1 > PlayerPrefs.GetInt("MaxLevel", 0))
                PlayerPrefs.SetInt("MaxLevel", GameSessionManagerCCh.Level);

            if (GameSessionManagerCCh.Level >= 2)
            {
                for (int i = 0; i < BattleModeGManager.PlayerDataSO.Count - 1; i++)
                {
                    BattleModePlayerDataSO item = BattleModeGManager.PlayerDataSO[i];
                    if (item.ID == BattleModeGManager.PlayerData.ID)
                    {
                        PlayerPrefs.SetInt($"Unlock_{BattleModeGManager.PlayerDataSO[i + 1].ID}", 1);
                        break;
                    }
                }
            }

            if (newRecord)
                IQSystem.IncraceIQ();
        };

        stateManager.OnLose += () =>
        {
            //InterfaceManager.ShowLoseWindow(scoreSystem.Score, LeaderBoard.GetBestScore());
            //LeaderBoard.SaveScore($"default", scoreSystem.Score);
            //LevelSelectWindow.CompliteLvl();
            RecordData recordData = LeaderBoard.GetScore($"score_{LevelSelectWindow.CurrentLvl}");
            bool newRecord = recordData == null || recordData.score < GameScoreSystem.Score;
            InterfaceManager.ShowLoseWindow(GameScoreSystem.Score, recordData != null ? recordData.score : 0);
            LeaderBoard.SaveScore($"score_{LevelSelectWindow.CurrentLvl}", GameScoreSystem.Score);

            if (GameSessionManagerCCh.Level > PlayerPrefs.GetInt("MaxLevel", 0))
                PlayerPrefs.SetInt("MaxLevel", GameSessionManagerCCh.Level);

            if (newRecord)            
                IQSystem.IncraceIQ();            
        };

        stateManager.OnStateChange += (state) =>
        {
            GamePause.SetPause(state != GameState.Game);
            AudioManager.PassFilterMusic(state != GameState.Game);

            if (state == GameState.Win || state == GameState.Lose)
            {
                if (player != null)
                    player.gameObject.SetActive(false);
                AudioManager.StopMusic();
                if (gameTimer != null)
                    gameTimer.Stop();
            }
        };
    }

    private void SetupPlayer()
    {
        player = Object.FindFirstObjectByType<PlayerChCF>();

        if (player != null)
        {
            player.Init(input);
            player.OnDamage += (dmgCont) =>
            {
                DataContainer.HealthContainer.RemoveValue(1);
            };

            player.OnGroundUpdate += (plane) =>
            {
                GameScoreSystem.AddScore(DataContainer.AnGroundBonus, player.transform.position + (Vector3.down * 1f));
                InterfaceManager.CreateScoreFlyingText(DataContainer.AnGroundBonus, player.transform.position + (Vector3.down * 1f), 0.05f);
            };
            //DataContainer.OnChangeSpeedGame += (speed) => player.SetSpeedKof(speed);
        }

        DragManager.Init(input);
        if (DragGameManager != null)
        {
            DragGameManager.Init();
            DragGameManager.OnEndGame += () => stateManager.SetState(GameState.Win);
            //DragGameManager.OnEndGame += () => Debug.Log("ON END GAME");
        }
    }

    private void SetupWinLoseCondition()
    {
        if(gameTimer != null)
            gameTimer.OnComplete += () => stateManager.SetState(GameState.Win);

        DataContainer.HealthContainer.OnDownfullValue += (_) =>
        {
            stateManager.SetState(GameState.Lose);
        };
    }

    private void SetupBonuses()
    {
        ObjectMediator.Subscribe<AddScoreGrapAction>((beh, grapOb) =>
        {
            GameScoreSystem.AddScore(beh.AddScore);
            InterfaceManager.CreateScoreFlyingText(beh.AddScore, grapOb.transform.position, 0.005f);
        });

        ObjectMediator.Subscribe<AddLifeGrapAction>((beh, grapOb) =>
        {
            DataContainer.HealthContainer.AddValue(beh.AddLife);
        });

        ObjectMediator.Subscribe<SlowMotionGrapAction>((beh, grapOb) =>
        {
            player.SlowMotion(beh.Duration);
        });

        ObjectMediator.Subscribe<InvictibleGrapAction>((beh, grapOb) =>
        {
            player.Invictible(beh.Duration);
        });
    }

    private void SetupGameManager()
    {
        BattleModeGManager = Object.FindFirstObjectByType<BattleModeGameManager>();
        BattleModeGManager.Init();
        Object.FindFirstObjectByType<BattleModeBattleManager>().Init();

        gameManager.Init();
        gameManager.OnPhaseChange += (phase) =>
        {
            spawnManager.NextPhase();
        };
    }

    private void SetupAchivmients()
    {
        /*AchivMediator.AddAchiviementForEndLevel<GameObject>("Grounds", (grounds) =>
        {
            if (grounds == null || grounds.Count > 0)
            {
                if (grounds.Count >= 6)
                    AchieviementSystem.ForceUnlock("UseAllPlatform");
            }
            else
            {
                AchieviementSystem.ForceUnlock("NotUsePlatform");
            }
        });
        spawner.OnObjectTimerDestroy += (spOb) =>
        {
            if (spOb.GetComponent<GrapObject>().IsActive)
                AchivMediator.ChangeStateAchiviementForEndLevel("GrapAllEggs", false);
        };

        stateManager.OnWin += () =>
        {
            AchivMediator.InvokeEndLevel();
        };

        AchivMediator.AddAchiviementForEndLevel("NotGrapEggs", true, (isNotGrap) =>
        {
            if (isNotGrap)
                AchieviementSystem.ForceUnlock("NotGrapEggs");
        });
        AchivMediator.AddAchiviementForEndLevel("GrapAllEggs", true, (isGrapAll) =>
        {
            if (isGrapAll)
                AchieviementSystem.ForceUnlock("GrapAllEggs");
        });*/

        if (player == null)
            return;

        player.OnGroundUpdate += (go) =>
        {
            AchivMediator.AddInList("Grounds", go, false);
        };
        player.OnGrap += (egg) =>
        {
            AchivMediator.ChangeStateAchiviementForEndLevel("NotGrapEggs", false);
        };
    }

    private void SetupSkins()
    {

        AchivMediator.AddAchiviementForEndLevel("NoDamage", true, (isNoDamage) =>
        {
            if (isNoDamage)
            {
                SkinsSystem.UnlockSkin("NoDamage");
                if (DataLevel.ID == "Last")
                    SkinsSystem.UnlockSkin("NoDamageLast");
            }            
        });

        if (player == null)
            return;

        player.SetSkin(SkinsSystem.GetCurrentSkin());
        player.OnDamage += (egg) =>
        {
            AchivMediator.ChangeStateAchiviementForEndLevel("NoDamage", false);
        };
    }

    private void SetupInteractionMediator()
    {
        InteractionMediator.OnInterationChoice += (interact) =>
        {
            switch (interact.type)
            {
                case OptionInterationType.AddSteps:
                    DragGameManager.AddStepMax((int)interact.valueF);
                    break;
                case OptionInterationType.AddBoxes:
                    DragGameManager.AddAvilibleBoxes((int)interact.valueF);
                    break;
                case OptionInterationType.AddLockBoxes:
                    DragGameManager.AddUnlockBoxes((int)interact.valueF);
                    break;
                case OptionInterationType.AddPatternVariable:
                    LevelGenerator.AddProbabilityForTag(interact.valueStr, (int)interact.valueF);
                    break;
                case OptionInterationType.AddHealth:
                    BattleModeGManager.AddPlayerMaxHeal((int)interact.valueF);
                    break;
                case OptionInterationType.AddMoney:
                    GameEntryGameplayCCh.DataContainer.MoneyContainer.AddValue((int)interact.valueF);
                    break;
                default:
                    break;
            }
        };
    }

    public static Vector3 GetMousePoint (float offsetRandom = 0f)
    {
        return Camera.main.ScreenToWorldPoint(input.GetOverPosition()) + Vector3.forward + (Vector3)(UnityEngine.Random.insideUnitCircle * offsetRandom);
    }
}