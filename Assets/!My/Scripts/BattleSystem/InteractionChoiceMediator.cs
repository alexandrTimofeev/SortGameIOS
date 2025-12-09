using System;
using System.Collections;
using UnityEngine;

public class InteractionChoiceMediator
{
    public event Action<OptionInteration> OnInterationChoice;

    public void InvokeInteraction (OptionInteration interaction)
    {
        interaction.Buy();
        OnInterationChoice?.Invoke (interaction);
    }
}