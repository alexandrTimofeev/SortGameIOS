using UnityEngine;
using TMPro;
using DG.Tweening;

public class EnemyUI : MonoBehaviour
{
    [SerializeField] private TextMeshPro tmpName;
    [SerializeField] private TextMeshPro tmpHealth;
    [SerializeField] private TextMeshPro tmpDamage;
    [SerializeField] private Color tmpColorDamageMultyply = Color.yellow;

    [Space]
    [SerializeField] private float durationHealthChange = 3f;
    [SerializeField] private float scaleDamageChange = 0.8f;

    private int health;

    public void Init (string name, int health, int damage, float damageMultyply = 1f)
    {
        tmpName.text = name;
        SetHealth(health);
        SetDamage(damage, damageMultyply);
    }

    public void SetHealth(int health)
    {
        DOVirtual.Int(this.health, health, durationHealthChange,
            (f) =>
            {
                this.health = f;
                UpdateHealthImmidiatly();
            });
    }

    private void UpdateHealthImmidiatly ()
    {
        tmpHealth.text = $"{health}";
    }

    public void SetDamage(int damage, float damageMultyply = 1f)
    {
        int realDamage = (int)(damage * damageMultyply);
        tmpDamage.text = $"Dmg: {realDamage}";
        if (realDamage != damage)
            tmpDamage.text += $" (x{damageMultyply})";
        tmpDamage.color = realDamage != damage ? Color.white : tmpColorDamageMultyply;

        tmpDamage.transform.DOPunchPosition(Random.insideUnitCircle * scaleDamageChange, 0.8f, 4);
    }
}
