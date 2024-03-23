using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFinish : MonoBehaviour
{
    [SerializeField] GameObject Content;
    [SerializeField] TextMeshProUGUI WinnerText;

    public void ShowFinish(string winner, string loser, int winScore, int loserScore)
    {
        Content.SetActive(true);

        Color winColor = GameController.Instance.WinnerColor;
        Color loseColor = GameController.Instance.LoserColor;

        WinnerText.text = $"<color=#{winColor}>{winner}</color> won with {winScore} points over <color=#{loseColor}>{loser}'s</color> {loserScore} points";
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
