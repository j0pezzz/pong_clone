using System;
using TMPro;
using UnityEngine;

public class MenuHandler : Fusion.Behaviour
{
    [SerializeField] GameObject Content;
    [SerializeField] GameObject MatchSettings;
    //TODO: Need to find a better suiting name for 'ChooseScreen' & 'NetworkScreen'.
    [SerializeField] GameObject ChooseScreen;
    [SerializeField] GameObject NetworkScreen;
    [SerializeField] TMP_Dropdown maxPointDropdown;
    [SerializeField] TMP_Dropdown aiDifficulty;
    [SerializeField] GameObject JoinRoomContent;
    [SerializeField] TMP_InputField SessionName;
    [SerializeField] GameObject RoomCreationUI;

    GameMode cacheGameMode;

    void OnEnable()
    {
        bl_EventHandler.Network.Online += OnOnline;
        bl_EventHandler.Match.InMatch += InMatch;
        bl_EventHandler.Menu.CreatingRoom += RoomCreation;
    }

    void OnDisable()
    {
        bl_EventHandler.Network.Online -= OnOnline;
        bl_EventHandler.Match.InMatch -= InMatch;
        bl_EventHandler.Menu.CreatingRoom -= RoomCreation;
    }

    void RoomCreation(bool creating)
    {
        RoomCreationUI.SetActive(creating);
    }

    void OnOnline()
    {
        ChooseScreen.SetActive(false);
        NetworkScreen.SetActive(true);
    }

    void InMatch(bool inMatch)
    {
        //TODO: Need to make this look better using the 'inMatch' bool instead of hardcoding false and true.
        if (inMatch)
        {
            Content.SetActive(false);
            MatchSettings.SetActive(false);
        }
        else
        {
            Content.SetActive(true);
        }
    }

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
