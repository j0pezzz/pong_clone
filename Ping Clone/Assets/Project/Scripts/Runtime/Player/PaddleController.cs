using Fusion;
using Project.Internal.Abstract;
using Project.Scripts.Game;
using Project.Scripts.Runtime.Player;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles inputs, and the paddle movement.
/// </summary>
public class PaddleController : PaddleBase
{
    [SerializeField] private PaddleInputHandler inputHandler;
    [Networked] public NetworkButtons ButtonsPrevious { get; set; }
    
    private float _yDir;

    public override void Spawned()
    {
        if (!HasInputAuthority) return;
        
        Debug.Log("Paddle spawned!");
        bl_EventHandler.Match.DispatchInMatchStatus(true);
    }

    /// <summary>
    /// This handles everything inputs when in Host/Client/Single Mode + not a mobile device.
    /// </summary>
    public override void FixedUpdateNetwork()
    {
        if (GameManager.Instance.IsGameDone || GameManager.Instance.IsGamePaused) return;

        // This is used when on Mobile + playing against AI (GameMode.Single)
        if (GameData.Instance.GetCurrentPlatform().IsMobile)
        {
            float yDir = inputHandler.SInputs.MoveUpwards ? 1 : inputHandler.SInputs.MoveDownwards ? -1 : 0;

            float newY = Mathf.Clamp(transform.position.y + (yDir * speed) * Runner.DeltaTime, NetworkHandler.Instance.BottomBound, NetworkHandler.Instance.TopBound);

            Vector3 newPosition = transform.position;
            newPosition.y = newY;
            transform.position = newPosition;
            return;
        }
        
        if (GetInput(out NetworkInputData data))
        {
            ButtonsPrevious = data.Buttons;

            float yDir = data.Buttons.IsSet(Buttons.Up) ? 1 : data.Buttons.IsSet(Buttons.Down) ? -1 : 0;

            float newY = Mathf.Clamp(transform.position.y + (yDir * speed) * Runner.DeltaTime, NetworkHandler.Instance.BottomBound, NetworkHandler.Instance.TopBound);

            Vector3 newPosition = transform.position;
            newPosition.y = newY;
            transform.position = newPosition;
        }
    }

    /// <summary>
    /// This handles everything inputs when in Shared Mode + not a mobile device.
    /// </summary>
    private void Update()
    {
        if (Runner.ProvideInput) return;
        if (GameManager.Instance.IsGameDone || GameManager.Instance.IsGamePaused) return;

        if (GameData.Instance.GetCurrentPlatform().IsMobile)
        {
            _yDir = inputHandler.SInputs.MoveUpwards ? 1 : inputHandler.SInputs.MoveDownwards ? -1 : 0;
        }
        else
        {
            _yDir = Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0;   
        }
    }

    private void FixedUpdate()
    {
        if (Runner.ProvideInput) return;
        if (GameManager.Instance.IsGameDone || GameManager.Instance.IsGamePaused) return;
        
        float newY = Mathf.Clamp(transform.position.y + (_yDir * speed) * Time.deltaTime, NetworkHandler.Instance.BottomBound, NetworkHandler.Instance.TopBound);
        
        Vector3 newPosition = transform.position;
        newPosition.y = newY;
        transform.position = newPosition;
    }
}
