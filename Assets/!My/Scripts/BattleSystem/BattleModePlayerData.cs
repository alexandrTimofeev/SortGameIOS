using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[Serializable]
public class BattleModePlayerData
{
    public string ID;
    public Sprite sprite;
    public GameObject vfxAttak;

    [Space]
    public string Name;
    public int Health;
    public int MaxHealth;

    [Space]
    public int AvilibleBoxesStart = 6;
    public int StepsStart = 6;

    public event Action<int, int> OnHealthChanged; // (текущее, максимум)
    public event Action<int> OnHealthOrDamage; // (текущее, максимум)
    public event Action OnDeath;

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health < 0) Health = 0;

        OnHealthChanged?.Invoke(Health, MaxHealth);
        OnHealthOrDamage?.Invoke(-damage);
        if (Health <= 0)
            OnDeath?.Invoke();
    }

    public void Heal(int value)
    {
        Health += value;
        if (Health > MaxHealth) Health = MaxHealth;

        OnHealthChanged?.Invoke(Health, MaxHealth);
        OnHealthOrDamage?.Invoke(value);
    }

    public void RemoveMaxHealth (int value)
    {
        SetMaxHealth(MaxHealth - value);
    }

    private void SetMaxHealth(int v)
    {
        MaxHealth = v;
        Health = Mathf.Clamp(Health, - 1, MaxHealth);
        OnHealthChanged?.Invoke(Health, MaxHealth);
    }

    public bool IsDead => Health <= 0;
}

public enum EnemyAttakType { None, LockBox }

[Serializable]
public class BattleModeEnemyData
{
    public string Name;
    public int Health;
    public int AttackPower;
    public EnemyAttakType[] AttackTypes;

    public event Action<int> OnHealthChanged; // текущее здоровье
    public event Action<int> OnHealthOrDamage; // текущее здоровье
    public event Action OnDeath;

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health < 0) Health = 0;

        OnHealthChanged?.Invoke(Health);
        OnHealthOrDamage?.Invoke(-damage);
        if (Health <= 0)
            OnDeath?.Invoke();
    }

    public void Heal (int heal)
    {
        Health += heal;

        OnHealthChanged?.Invoke(Health);
        OnHealthOrDamage?.Invoke(heal);
    }

    public BattleModeEnemyData Clone()
    {
        return new BattleModeEnemyData() { Name = Name, Health = Health, AttackPower = AttackPower, AttackTypes = AttackTypes };
    }

    public void SetLevelDificalty(int levelDificalty)
    {
        Health = (int)(Health * Mathf.Pow(1.15f, levelDificalty));
        AttackPower = (int)(AttackPower * Mathf.Pow(1.25f, levelDificalty));
    }

    public bool IsDead => Health <= 0;
}

public class BattleModeAttackData
{
    public int Damage;
    public string Type;
    public float Multyply = 1f;
    public EnemyAttakType[] AttackTypes;

    public BattleModeAttackData(int damage, string type = "basic", float multyply = 1f, EnemyAttakType[] attackTypes = null)
    {
        Damage = damage;
        Type = type;
        Multyply = multyply;
        AttackTypes = attackTypes;
    }

    public int GetDamage() => (int)(Damage * Multyply);
}