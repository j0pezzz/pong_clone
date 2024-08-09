using System;
using TMPro;
using UnityEngine;

public class MenuHandler : Fusion.Behaviour
{
    [SerializeField] GameObject MatchSettings;
    //TODO: Need to find a better suiting name for 'ChooseScreen' & 'NetworkScreen'.
    [SerializeField] GameObject ChooseScreen;
    [SerializeField] GameObject NetworkScreen;
    [SerializeField] TMP_Dropdown maxPointDropdown;
    [SerializeField] TMP_Dropdown aiDifficulty;
    [SerializeField] GameObject JoinRoomContent;
    [SerializeField] TMP_InputField SessionName;

    GameMode cacheGameMode;

    void ShowMatchSettings(bool active)
    {
        MatchSettings.SetActive(active);

        aiDifficulty.gameObject.SetActive(cacheGameMode == GameMode.PvE || cacheGameMode == GameMode.EvE);
    }

    public void SetGameMode(string gameMode) 
    { 
        cacheGameMode = (GameMode)Enum.Parse(typeof(GameMode), gameMode);
        ChooseScreen.SetActive(cacheGameMode == GameMode.PvP);
        ShowMatchSettings(true);
    }

    public void PlayGame()
    {
        string points = maxPointDropdown.options[maxPointDropdown.value].text;

        if (!GameController.Instance.IsOnline)
        {
            AIDifficulty difficulty = (AIDifficulty)aiDifficulty.value;
            GameController.Instance.StartGame(cacheGameMode, points, difficulty);
        }
    }

    public void PlayOnline(bool online)
    {
        if (online)
        {
            GameController.Instance.StartRunner();
        }
    }

    public void HostRoom()
    {
        StartCoroutine(GameController.Instance.HostRoom());
    }

    public void JoinRoom(bool isJoining)
    {
        if (!isJoining)
        {
            JoinRoomContent.SetActive(true);
        }
        else
        {
            JoinRoomContent.SetActive(false);

            StartCoroutine(GameController.Instance.JoinRoom(SessionName.text));
        }
    }
}
