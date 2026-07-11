using Fusion;
using Project.Scripts.Game;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleControlller : NetworkBehaviour
{
    [Networked] public NetworkButtons ButtonsPrevious { get; set; }
    [Range(1, 5)] public float Speed = 5f;

    public int PlayerRef = 1;

    Transform _transform;
    Vector3 _initPosition;

    public override void Spawned()
    {
        if (!HasInputAuthority) return;
        
        Debug.Log("Paddle spawned!");
        bl_EventHandler.Match.DispatchInMatchStatus(true);

        _initPosition = transform.position;
        _transform = transform;
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

            float newY = Mathf.Clamp(_transform.position.y + (yDir * Speed) * Runner.DeltaTime, NetworkHandler.Instance.BottomBound, NetworkHandler.Instance.TopBound);
            _transform.position = new Vector3(_transform.position.x, newY, _transform.position.z);
        }
    }

    private float _yDir;

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
        
        float newY = Mathf.Clamp(transform.position.y + (_yDir * Speed) * Time.deltaTime, NetworkHandler.Instance.BottomBound, NetworkHandler.Instance.TopBound);
        _transform.position = new Vector3(_transform.position.x, newY, _transform.position.z);
    }

    public void SetPlayerToInitPosition()
    {
        _transform.position = _initPosition;
    }
}
