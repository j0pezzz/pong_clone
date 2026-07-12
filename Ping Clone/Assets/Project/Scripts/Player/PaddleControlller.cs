using Fusion;
using Project.Internal.Abstract;
using Project.Scripts.Game;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleControlller : PaddleBase
{
    [Networked] public NetworkButtons ButtonsPrevious { get; set; }
    
    private float _yDir;

    public override void Spawned()
    {
        if (!HasInputAuthority) return;
        
        Debug.Log("Paddle spawned!");
        bl_EventHandler.Match.DispatchInMatchStatus(true);
    }

    /// <summary>
    /// This handles everything input related when in Host/Client/Single Mode.
    /// </summary>
    public override void FixedUpdateNetwork()
    {
        if (!Runner.ProvideInput) return;
        if (GameManager.Instance.IsGameDone || GameManager.Instance.IsGamePaused) return;

        if (GetInput(out NetworkInputData data))
        {
            ButtonsPrevious = data.Buttons;

            float yDir = data.Buttons.IsSet(Buttons.Up) ? 1 : data.Buttons.IsSet(Buttons.Down) ? -1 : 0;

            float newY = Mathf.Clamp(transform.position.y + (yDir * speed) * Runner.DeltaTime, NetworkHandler.Instance.BottomBound, NetworkHandler.Instance.TopBound);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    /// <summary>
    /// This handles everything input related when in Shared Mode.
    /// </summary>
    private void Update()
    {
        if (Runner.ProvideInput) return;
        if (GameManager.Instance.IsGameDone || GameManager.Instance.IsGamePaused) return;
        
        _yDir = Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0;
            
        //float newY = Mathf.Clamp(transform.position.y + (yDir * Speed) * Time.deltaTime, NetworkHandler.Instance.BottomBound, NetworkHandler.Instance.TopBound);
        //_transform.position = new Vector3(_transform.position.x, newY, _transform.position.z);
    }

    private void FixedUpdate()
    {
        if (Runner.ProvideInput) return;
        if (GameManager.Instance.IsGameDone || GameManager.Instance.IsGamePaused) return;
        
        float newY = Mathf.Clamp(transform.position.y + (_yDir * speed) * Time.deltaTime, NetworkHandler.Instance.BottomBound, NetworkHandler.Instance.TopBound);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
