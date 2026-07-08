using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject Content;

    bool _paused;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            _paused = !_paused;
            Content.SetActive(_paused);
            bl_EventHandler.Match.DispatchPauseEvent(_paused);
        }
    }

    public void Resume()
    {
        _paused = false;
        Content.SetActive(_paused);
        bl_EventHandler.Match.DispatchPauseEvent(_paused);
    }

    public void QuitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
