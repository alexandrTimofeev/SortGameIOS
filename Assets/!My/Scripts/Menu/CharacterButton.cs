using System;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour
{
    [SerializeField] private Image spriteCharacter;
    [SerializeField] private TMP_Text textName;
    [SerializeField] private Image spriteLock;

    private Button button;
    private BattleModePlayerDataSO playerData;

    public void Init(BattleModePlayerDataSO playerData)
    {
        button = GetComponent<Button>();
        this.playerData = playerData;

        SetUnlockState(PlayerPrefs.GetInt($"Unlock_{playerData.ID}", 0) == 1);
        spriteCharacter.sprite = playerData.sprite;

        button.onClick.AddListener(Click);
    }

    private void Click()
    {
        BattleModeGameManager.IDCharacter = playerData.ID;
        GameSessionManagerCCh.ClearForNextLevel();
        GameSceneManager.LoadGame();
    }

    public void SetUnlockState(bool state)
    {
        button.interactable = state;
        textName.text = state ? playerData.Name : "Complete at least three levels for the last character to unlock";
        spriteLock.gameObject.SetActive(!state);
    }
}
