using UnityEngine;
using System.Collections.Generic;

public class CharacterList : MonoBehaviour
{
    [SerializeField] private List<BattleModePlayerDataSO> playerDataSO = new List<BattleModePlayerDataSO>();

    [Space]
    [SerializeField] private CharacterButton characterButtonPref;
    [SerializeField] private Transform container;

    private List<CharacterButton> characterButtons = new List<CharacterButton>(); 
    

    public void Open()
    {
        foreach (var button in characterButtons)
        {
            Destroy(button.gameObject);
        }
        characterButtons.Clear();

        foreach (var dataSO in playerDataSO)
        {
            CharacterButton characterButton = Instantiate(characterButtonPref, container);
            characterButtons.Add(characterButton);
            characterButton.Init(dataSO);
        }

        characterButtons[0].SetUnlockState(true);
    }
}
