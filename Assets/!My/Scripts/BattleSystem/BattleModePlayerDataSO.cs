using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BattleMode/PlayerData")]
public class BattleModePlayerDataSO : ScriptableObject
{
    public string ID;
    public Sprite sprite;
    public GameObject vfxAttak;

    [Space]
    public string Name = "Player";
    public int Health = 100;
    public int MaxHealth = 100;

    [Space]
    public int AvilibleBoxesStart = 6;
    public int StepsBoxesStart = 6;

    public BattleModePlayerData GetPlayerData()
    {
        return new BattleModePlayerData()
        {
            Name = Name,
            Health = Health,
            MaxHealth = MaxHealth,
            AvilibleBoxesStart = AvilibleBoxesStart,
            StepsStart = StepsBoxesStart,
            sprite = sprite,
            ID = ID,
            vfxAttak = vfxAttak
        };
    }
}