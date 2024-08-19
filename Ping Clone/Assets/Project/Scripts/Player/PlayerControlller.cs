using Fusion;
using UnityEngine;

public class PlayerControlller : NetworkBehaviour
{
    [Networked] public NetworkButtons ButtonsPrevious { get; set; }
    [Range(1, 5)] public float Speed = 5f;

    public int PlayerRef = 1;

    Transform m_Transform;

    Vector3 initPosition;

    public override void Spawned()
    {
        Debug.LogWarning("We are spawned");
        bl_EventHandler.Match.DispatchInMatchStatus(true);

        initPosition = transform.position;
        m_Transform = transform;
    }

    public override void FixedUpdateNetwork()
    {
        if (GameTimer.Instance.IsGameDone > 0) return;
        if (GameTimer.Instance.IsGamePaused > 0) return;

        if (GetInput(out NetworkInputData data))
        {
            NetworkButtons pressed = data.Buttons.GetPressed(ButtonsPrevious);
            NetworkButtons released = data.Buttons.GetReleased(ButtonsPrevious);

            ButtonsPrevious = data.Buttons;

            float yDir = data.Buttons.IsSet(Buttons.Up) ? 1 : data.Buttons.IsSet(Buttons.Down) ? -1 : 0;

            float newY = Mathf.Clamp(transform.position.y + (yDir * Speed) * Runner.DeltaTime, GameController.Instance.BottomBound, GameController.Instance.TopBound);
            m_Transform.position = new(m_Transform.position.x, newY, m_Transform.position.z);
        }
    }

    public void SetPlayerToInitPosition()
    {
        m_Transform.position = initPosition;
    }
}
