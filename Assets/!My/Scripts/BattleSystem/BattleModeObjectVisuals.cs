using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BattleModeObjectVisuals : MonoBehaviour
{
    public event Action OnArrivedAtTarget;

    [Header("Параметры")]
    public float moveDuration = 1f;
    public float spawnOffsetX = 5f;
    public float attackDuration = 0.3f;
    public float hitShakeDuration = 0.2f;
    public float hitShakeStrength = 0.2f;
    public int hitShakeVibrato = 10;
    public float hitShakeRandomness = 90f;
    public float deathFadeDuration = 0.5f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
    }

    #region Entrance

    /// <summary>
    /// Появление объекта справа и движение к своей позиции
    /// </summary>
    public void AnimateEntranceFromRight()
    {
        Vector3 targetPosition = transform.position;             // текущая позиция на сцене
        Vector3 startPos = targetPosition + Vector3.right * spawnOffsetX;
        transform.position = startPos;

        transform.DOMove(targetPosition, moveDuration)
                 .SetEase(Ease.OutCubic)
                 .OnComplete(() => OnArrivedAtTarget?.Invoke());
    }

    #endregion

    #region Attack

    public IEnumerator AnimateAttack(GameObject target, GameObject attackPrefab)
    {
        if (attackPrefab == null) yield break;

        GameObject attackObj = Instantiate(attackPrefab, transform.position, Quaternion.identity);
        yield return attackObj.transform.DOMove(target.transform.position, attackDuration).WaitForCompletion();

        // Эффект попадания
        target.transform.DOShakePosition(hitShakeDuration, hitShakeStrength, 10, 90);

        Destroy(attackObj);
    }

    #endregion

    #region Death

    public IEnumerator PlayDeathAnimation(Action onComplete = null)
    {
        if (spriteRenderer != null)
            yield return spriteRenderer.DOFade(0f, deathFadeDuration).WaitForCompletion();

        onComplete?.Invoke();
        Destroy(gameObject);
    }

    #endregion

    /// <summary>
    /// Эффект попадания (shake). Возвращает IEnumerator для использования в Coroutine.
    /// </summary>
    public IEnumerator AnimateHit()
    {
        bool finished = false;

        // shake позиции
        transform.DOShakePosition(hitShakeDuration, hitShakeStrength, hitShakeVibrato, hitShakeRandomness)
                 .OnComplete(() => finished = true);

        // ждём окончания анимации
        yield return new WaitUntil(() => finished);
    }
}