using System;
using TMPro;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    [SerializeField] GameObject MatchSettings;
    [SerializeField] TMP_Dropdown maxPointDropdown;
    [SerializeField] TMP_Dropdown aiDifficulty;

    GameMode cacheGameMode;

    public void ShowMatchSettings(bool active)
    {
        MatchSettings.SetActive(active);

        if (cacheGameMode == GameMode.PvP)
        {
            aiDifficulty.gameObject.SetActive(!active);
        }
        else
        {
            aiDifficulty.gameObject.SetActive(active);
        }
    }

    public void SetGameMode(string gameMode) => cacheGameMode = (GameMode)Enum.Parse(typeof(GameMode), gameMode);

    public void PlayGame()
    {
        AIDifficulty difficulty = (AIDifficulty)aiDifficulty.value;
        string points = maxPointDropdown.options[maxPointDropdown.value].text;
        GameController.Instance.StartGame(cacheGameMode, points, difficulty);
    }
}
