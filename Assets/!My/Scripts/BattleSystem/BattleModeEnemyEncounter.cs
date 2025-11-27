using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BattleModeEnemyEncounter : BattleModeInteractableObject
{
    [SerializeField] private BattleModeEnemyData enemyData;
    private BattleModeEnemyData enemyDataClone = null;
    public GameObject attackPrefab;
    [SerializeField] private EnemyUI enemyUI;

    public BattleModeEnemyData EnemyData
    {
        get
        {
            //Debug.Log($"get Enemy {LevelDificalty}");
            if (enemyDataClone == null)
            {
                enemyDataClone = enemyData.Clone();
                enemyDataClone = SetLevelDificalty(enemyDataClone);
            }
            return enemyDataClone;
        }
    }

    public override void Activate(BattleModePlayerData playerData, GameObject playerObj)
    {
        visuals.OnArrivedAtTarget += () =>
        {
            BattleModeBattleManager.Instance.StartBattle(playerData, this, OnBattleComplete);
        };

        visuals.AnimateEntranceFromRight();
        
        enemyUI.Init(EnemyData.Name, EnemyData.Health, EnemyData.AttackPower);
        EnemyData.OnHealthChanged += HealthChangeUI;
    }

    public void HealthChangeUI(int health)
    {        
        enemyUI.SetHealth(health);
    }

    public void DamageChangeUI(int damage, float damageMultyplayer)
    {
        enemyUI.SetDamage(damage, damageMultyplayer);
    }

    private void OnBattleComplete(bool playerWon)
    {
        if (playerWon)
        {
            StartCoroutine(visuals.PlayDeathAnimation(() =>
            {
                CompleteInteraction();
            }));
        }
        else
        {
            //CompleteInteraction();
        }
    }

    public IEnumerator EnemyAttackVisual(GameObject playerObj)
    {
        yield return visuals.AnimateAttack(playerObj, attackPrefab);
    }

    public static int LevelDificalty = 0;
    private BattleModeEnemyData SetLevelDificalty(BattleModeEnemyData enemyDataClone)
    {
        //Debug.Log($"LevelDificalty Enemy {LevelDificalty}");
        enemyDataClone.SetLevelDificalty(LevelDificalty);
        LevelDificalty++;

        return enemyDataClone;
    }
}