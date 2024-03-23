using System;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    public void PlayGame(string gameMode)
    {
        if (Enum.TryParse(typeof(GameMode), gameMode, out object mode))
        {
            GameController.Instance.StartGame((GameMode)mode);
        }
    }
}
