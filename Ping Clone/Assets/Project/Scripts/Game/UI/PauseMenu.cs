using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject Content;

    bool paused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
            Content.SetActive(paused);
            bl_EventHandler.DispatchPauseEvent(paused);
        }
    }

    public void Resume()
    {
        paused = false;
        Content.SetActive(paused);
        bl_EventHandler.DispatchPauseEvent(paused);
    }

    public void QuitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
