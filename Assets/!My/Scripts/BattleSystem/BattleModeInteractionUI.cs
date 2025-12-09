using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Отвечает за визуальное отображение окна взаимодействия (диалог, предмет, событие)
/// </summary>
public class BattleModeInteractionUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text interactionText;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private Button optionButtonPrefab;

    private Action<int> onOptionSelected;

    private void Awake()
    {
        Hide();
    }

    /// <summary>
    /// Отображает окно взаимодействия
    /// </summary>
    public void Show(string text, OptionInteration[] options, Action<int> onSelect)
    {
        ClearOptions();

        interactionText.text = text;
        onOptionSelected = onSelect;
        panelRoot.SetActive(true);

        for (int i = 0; i < options.Length; i++)
        {
            int index = i;
            Button newButton = Instantiate(optionButtonPrefab, optionsContainer);
            TMP_Text btnText = newButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = options[i].GetText();

            newButton.onClick.AddListener(() => OnOptionClick(index));
            newButton.interactable = options[i].IsAvilibleChoice();
        }
    }

    private void OnOptionClick(int index)
    {
        onOptionSelected?.Invoke(index);
        Hide();
    }

    private void ClearOptions()
    {
        foreach (Transform child in optionsContainer)
            Destroy(child.gameObject);
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
        ClearOptions();
    }
}