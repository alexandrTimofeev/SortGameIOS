using UnityEngine;
using System;

/// <summary>
/// Менеджер взаимодействий (диалогов, предметов, событий)
/// Управляет UI и передаёт результаты взаимодействия обратно объекту
/// </summary>
[DefaultExecutionOrder(-10)]
public class BattleModeInteractionManager : MonoBehaviour
{
    public static BattleModeInteractionManager Instance;

    [SerializeField] private BattleModeInteractionUI interactionUI;

    private void Awake()
    {
        Instance = this;
    }

    public void StartInteraction(string text, OptionInteration[] options, Action<int> onChoiceSelected)
    {
        interactionUI.Show(text, options, onChoiceSelected);
    }
}