using Fusion;
using UnityEngine;

public class PlayerControlller : NetworkBehaviour
{
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
        if (GameController.Instance.IsGameDone || GameController.Instance.IsGamePaused) return;

        MovePlayer();
    }

    public void SetPlayerToInitPosition()
    {
        m_Transform.position = initPosition;
    }

    void MovePlayer()
    {
        switch (PlayerRef)
        {
            case 1:
                UseWSKeys();
                break;
            case 2:
                UseArrowKeys();
                break;
        }
    }

    /// <summary>
    /// Handles input for Up and Down arrow.
    /// </summary>
    void UseArrowKeys()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            float newY = Mathf.Clamp(transform.position.y + Speed * Runner.DeltaTime, GameController.Instance.BottomBound, GameController.Instance.TopBound);
            m_Transform.position = new(m_Transform.position.x, newY, m_Transform.position.z);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            float newY = Mathf.Clamp(transform.position.y + -Speed * Runner.DeltaTime, GameController.Instance.BottomBound, GameController.Instance.TopBound);
            m_Transform.position = new(m_Transform.position.x, newY, m_Transform.position.z);
        }
    }

    /// <summary>
    /// Handles input for W and S keys.
    /// </summary>
    void UseWSKeys()
    {
        if (Input.GetKey(KeyCode.W))
        {
            float newY = Mathf.Clamp(m_Transform.position.y + Speed * Runner.DeltaTime, GameController.Instance.BottomBound, GameController.Instance.TopBound);
            m_Transform.position = new(m_Transform.position.x, newY, m_Transform.position.z);
        }
        if (Input.GetKey(KeyCode.S))
        {
            float newY = Mathf.Clamp(m_Transform.position.y + -Speed * Runner.DeltaTime, GameController.Instance.BottomBound, GameController.Instance.TopBound);
            m_Transform.position = new(m_Transform.position.x, newY, m_Transform.position.z);
        }
    }
}
