using System;
using TMPro;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    [SerializeField] GameObject MatchSettings;
    [SerializeField] TMP_Dropdown maxPointDropdown;

    public void ShowMatchSettings(bool active)
    {
        MatchSettings.SetActive(active);
    }

    public void PlayGame(string gameMode)
    {
        if (Enum.TryParse(typeof(GameMode), gameMode, out object mode))
        {
            GameController.Instance.StartGame((GameMode)mode, maxPointDropdown.value);
        }
    }
}
