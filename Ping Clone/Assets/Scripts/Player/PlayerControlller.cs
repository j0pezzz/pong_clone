using UnityEngine;

public class PlayerControlller : MonoBehaviour
{
    [Range(1, 5)] public float Speed = 5f;

    public int PlayerRef = 1;

    Transform m_Transform;

    Vector3 initPosition;

    void Awake()
    {
        initPosition = transform.position;
        m_Transform = transform;
    }

    void Update()
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
            float newY = Mathf.Clamp(transform.position.y + Speed * Time.deltaTime, GameController.Instance.BottomBound, GameController.Instance.TopBound);
            m_Transform.position = new(m_Transform.position.x, newY, m_Transform.position.z);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            float newY = Mathf.Clamp(transform.position.y + -Speed * Time.deltaTime, GameController.Instance.BottomBound, GameController.Instance.TopBound);
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
            float newY = Mathf.Clamp(transform.position.y + Speed * Time.deltaTime, GameController.Instance.BottomBound, GameController.Instance.TopBound);
            m_Transform.position = new(m_Transform.position.x, newY, m_Transform.position.z);
        }
        if (Input.GetKey(KeyCode.S))
        {
            float newY = Mathf.Clamp(transform.position.y + -Speed * Time.deltaTime, GameController.Instance.BottomBound, GameController.Instance.TopBound);
            m_Transform.position = new(m_Transform.position.x, newY, m_Transform.position.z);
        }
    }
}
