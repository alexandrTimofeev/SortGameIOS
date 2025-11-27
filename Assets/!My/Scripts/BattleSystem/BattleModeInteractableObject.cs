using System;
using UnityEngine;

public abstract class BattleModeInteractableObject : MonoBehaviour
{
    public event Action OnInteractionComplete;
    protected BattleModeObjectVisuals visuals;

    protected virtual void Awake()
    {
        visuals = GetComponent<BattleModeObjectVisuals>();
    }

    /// <summary>
    /// Активировать объект встречи
    /// </summary>
    /// <param name="playerData">Данные игрока</param>
    /// <param name="playerObj">Объект игрока для анимаций</param>
    public abstract void Activate(BattleModePlayerData playerData, GameObject playerObj);

    public void CompleteInteraction()
    {
        OnInteractionComplete?.Invoke();
    }
}