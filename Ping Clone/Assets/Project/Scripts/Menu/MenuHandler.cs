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
    [SerializeField] GameObject JoiningRoomUI;
    [SerializeField] GameObject NoRoomUI;
    [SerializeField] TextMeshProUGUI ErrorMessage;

    GameMode cacheGameMode;

    void OnEnable()
    {
        bl_EventHandler.Network.Online += OnOnline;
        bl_EventHandler.Match.onMatch += InMatch;
        bl_EventHandler.Menu.CreatingRoom += RoomCreation;
        bl_EventHandler.Menu.JoinRoom += JoiningRoom;
        bl_EventHandler.Menu.NoRoom += NoRoomToJoin;
    }

    void OnDisable()
    {
        bl_EventHandler.Network.Online -= OnOnline;
        bl_EventHandler.Match.onMatch -= InMatch;
        bl_EventHandler.Menu.CreatingRoom -= RoomCreation;
        bl_EventHandler.Menu.JoinRoom -= JoiningRoom;
        bl_EventHandler.Menu.NoRoom -= NoRoomToJoin;
    }

    void RoomCreation(bool creating)
    {
        RoomCreationUI.SetActive(creating);
    }

    void JoiningRoom(bool joining)
    {
        JoiningRoomUI.SetActive(joining);
    }

    void NoRoomToJoin(string message) 
    { 
        NoRoomUI.SetActive(true);
        ErrorMessage.text = message;
    }

    void OnOnline(bool online)
    {
        if (online)
        {
            ChooseScreen.SetActive(false);
            NetworkScreen.SetActive(true);
        }
        else
        {
            NetworkScreen.SetActive(false);
            ChooseScreen.SetActive(true);
        }
    }

    void InMatch(bool inMatch)
    {
        //TODO: Need to make this look better using the 'inMatch' bool instead of hardcoding false and true??
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

    public void SetGameMode(string gameMode) 
    { 
        cacheGameMode = (GameMode)Enum.Parse(typeof(GameMode), gameMode);
        ChooseScreen.SetActive(cacheGameMode == GameMode.PvP);
    }

    public void PlayGame()
    {
        string points = maxPointDropdown.options[maxPointDropdown.value].text;

        if (!GameController.Instance.IsOnline)
        {
            AIDifficulty difficulty = (AIDifficulty)aiDifficulty.value;
            GameController.Instance.StartGameOffline(cacheGameMode, points, difficulty);
        }
        else
        {
            StartCoroutine(GameController.Instance.HostRoom(points));
        }
    }

    public void PlayOnline(bool online)
    {
        if (online)
        {
            GameController.Instance.StartRunner();
        }
        else
        {
            GameController.Instance.StopRunner();
        }
    }

    public void HostRoom()
    {
        NetworkScreen.SetActive(false);
        MatchSettings.SetActive(true);

        aiDifficulty.gameObject.SetActive(!GameController.Instance.IsOnline);
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
