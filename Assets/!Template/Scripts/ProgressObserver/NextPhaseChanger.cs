using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NextPhaseChanger : MonoBehaviour
{
    public UnityEvent<int> OnPhaseChange;

    public void ChangePhase (int phase)
    {
        OnPhaseChange?.Invoke(phase);
    }
}
