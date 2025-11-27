using NUnit.Framework.Internal;
using System;
using UnityEngine;

public class BattleModeInteractionObject : BattleModeInteractableObject
{
    [SerializeField] private string interactionText;
    [SerializeField] private OptionInteration[] options;

    public event Action<OptionInteration> OnChoiceMade;

    public override void Activate(BattleModePlayerData playerData, GameObject playerObj)
    {
        visuals.OnArrivedAtTarget += () =>
        {
            // Запуск диалога после появления объекта
            BattleModeInteractionManager.Instance.StartInteraction(interactionText, options, ChoiceMade);
        };

        visuals.AnimateEntranceFromRight();
    }

    private void ChoiceMade(int optionIndex)
    {
        OnChoiceMade?.Invoke(options[optionIndex]);
        GameEntryGameplayCCh.InteractionMediator.InvokeInteraction(options[optionIndex]);
        CompleteInteraction();
    }
}
public enum OptionInterationType
{
    None,
    AddSteps,
    AddBoxes,
    AddLockBoxes,
    AddPatternVariable,
    AddHealth,
    AddMoney
}

[System.Serializable]
public class OptionInteration
{
    public string text;
    public OptionInterationType type = OptionInterationType.None;
    public string valueStr;
    public float valueF;

    [Space]
    public bool IsUseBonusText = true; // использовать BonusText?
    public bool IsBonusTextWithoutBorder = false; // BonusText без скобок

    [Space]
    public int needMoney;
    public int needHealth;
    public int needBoxes;
    public int needUnlockBoxes;
    public int needMaxSteps;

    public bool IsContinsBonusText()
    {
        return type != OptionInterationType.None;
    }

    public string GetText()
    {
        if (!IsUseBonusText || type == OptionInterationType.None)
            return text + $" {GetTextCost()}";

        string bonus = GetBonusText();
        if (string.IsNullOrEmpty(bonus))
            return text;

        if (IsBonusTextWithoutBorder)
            return $"{text} {bonus} {GetTextCost()}";
        else
            return $"{text} ({bonus}) {GetTextCost()}";
    }

    public string GetBonusText()
    {
        switch (type)
        {
            case OptionInterationType.AddSteps:
                return valueF >= 0
                    ? $"Gain {Mathf.Abs(valueF)} extra move{(Mathf.Abs(valueF) == 1 ? "" : "s")}"
                    : $"Lose {Mathf.Abs(valueF)} move{(Mathf.Abs(valueF) == 1 ? "" : "s")}";

            case OptionInterationType.AddBoxes:
                return valueF >= 0
                    ? $"Add {Mathf.Abs(valueF)} box{(Mathf.Abs(valueF) == 1 ? "" : "es")} (max {LevelGenerator.AllBoxes.Length})"
                    : $"Remove {Mathf.Abs(valueF)} box{(Mathf.Abs(valueF) == 1 ? "" : "es")} (max {LevelGenerator.AllBoxes.Length})";

            case OptionInterationType.AddLockBoxes:
                return valueF >= 0
                    ? $"LockBox {Mathf.Abs(valueF)} box{(Mathf.Abs(valueF) == 1 ? "" : "es")}"
                    : $"Unlock {Mathf.Abs(valueF)} box{(Mathf.Abs(valueF) == 1 ? "" : "es")}";

            case OptionInterationType.AddPatternVariable:
                return valueF >= 0
                    ? $"Increase chance of \"{BattleModeBattleManager.GetNameAttakFromID(valueStr)}\" " +
                    $"by {Mathf.Abs(valueF)}% (now {LevelGenerator.GetProbability(valueStr, true)}%)"
                    : $"Decrease chance of \"{BattleModeBattleManager.GetNameAttakFromID(valueStr)}\" " +
                    $"by {Mathf.Abs(valueF)}% (now {LevelGenerator.GetProbability(valueStr, true)}%)";

            case OptionInterationType.AddHealth:
                return valueF >= 0
                    ? $"Increase maximum health by {Mathf.Abs(valueF)} point{(Mathf.Abs(valueF) == 1 ? "" : "s")}"
                    : $"Reduce maximum health by {Mathf.Abs(valueF)} point{(Mathf.Abs(valueF) == 1 ? "" : "s")}";

            case OptionInterationType.AddMoney:
                return valueF >= 0
                    ? $"Receive money {Mathf.Abs(valueF)}"
                    : $"Pay money {Mathf.Abs(valueF)}";

            default:
                return string.Empty;
        }
    }

    public bool IsAvilibleChoice()
    {
        return GameEntryGameplayCCh.DataContainer.MoneyContainer.Value >= needMoney &&
            GameEntryGameplayCCh.BattleModeGManager.PlayerData.MaxHealth - 1 >= needHealth &&
            GameEntryGameplayCCh.DragGameManager.Steps - 1 >= needMaxSteps &&
            GameEntryGameplayCCh.DragGameManager.AvilibleBoxes - 1 >= needBoxes &&
            GameEntryGameplayCCh.DragGameManager.UnlockBoxes >= needUnlockBoxes;
    }

    private string GetTextCost()
    {
        if (needMoney <= 0 && needHealth <= 0 && needMaxSteps <= 0 && needBoxes <= 0 && needUnlockBoxes <= 0)
            return "";

        string txtRet = " [";
        AddResourceInText(needMoney, "<color=green>$</color>");
        AddResourceInText(needHealth, "<color=green>+</color>");
        AddResourceInText(needMaxSteps, "<color=green>steps</color>");
        AddResourceInText(needBoxes, "<color=green>boxes</color>");
        AddResourceInText(needUnlockBoxes, "<color=green>unlocks</color>");
        txtRet += "]";

        void AddResourceInText(int res, string resTxt)
        {
            if (res > 0)
                txtRet += $"-{res}{resTxt}|";
        }

        return txtRet;
    }

    public void Buy()
    {
        if (needMoney > 0)
            GameEntryGameplayCCh.DataContainer.MoneyContainer.RemoveValue(needMoney);

        if (needHealth > 0)
            GameEntryGameplayCCh.BattleModeGManager.PlayerData.RemoveMaxHealth(needHealth);

        if (needMaxSteps > 0)
            GameEntryGameplayCCh.DragGameManager.RemoveStep(needMaxSteps);

        if (needBoxes > 0)
            GameEntryGameplayCCh.DragGameManager.RemoveAvilibleBoxes(needBoxes);

        if (needUnlockBoxes > 0)
            GameEntryGameplayCCh.DragGameManager.AddUnlockBoxes(-needUnlockBoxes);
    }
}