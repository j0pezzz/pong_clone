using UnityEngine;

public class Ball : MonoBehaviour
{
    [Range(1, 20)] public float Speed = 5f;
    public Rigidbody rb;

    Vector3 initPos;
    Vector3 pausedVelocity;

    float xDir, yDir;

    void Awake()
    {
        initPos = transform.position;
        LaunchBall();
        bl_EventHandler.onPauseCall += OnGamePaused;
        bl_EventHandler.onGameFinish += GameFinished;
    }

    void GameFinished() => rb.velocity = Vector3.zero;

    void OnGamePaused(bool paused)
    {
        if (paused)
        {
            pausedVelocity = rb.velocity;
            rb.velocity = Vector3.zero;
        }
        else
        {
            rb.velocity = pausedVelocity;
        }
    }

    public void SetBallToInit()
    {
        transform.position = initPos;
        LaunchBall();
    }

    void LaunchBall()
    {
        do
        {
            xDir = Random.Range(-1f, 1f);
        } while (Mathf.Abs(xDir) < 0.5f);

        yDir = Random.Range(-0.5f, 0.5f);

        Vector3 launchDir = new Vector3(xDir, yDir, 0).normalized;

        rb.velocity = launchDir * Speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Paddle1"))
        {
            float y = HitFactor(transform.position, collision.transform.position, collision.collider.bounds.size.y);

            Vector2 direction = new Vector2(1, y).normalized;

            rb.velocity = direction * Speed;
        }
        else if (collision.gameObject.CompareTag("Paddle2"))
        {
            float y = HitFactor(transform.position, collision.transform.position, collision.collider.bounds.size.y);

            Vector2 direction = new Vector2(-1, y).normalized;

            rb.velocity = direction * Speed;
        }
    }

    float HitFactor(Vector2 ballPos, Vector2 playerPos, float playerHeight)
    {
        return (ballPos.y - playerPos.y) / playerHeight;
    }

    void OnTriggerEnter(Collider other)
    {
        bool enterP1Goal = other.gameObject.CompareTag("Player 1 Goal");
        bool enterP2Goal = other.gameObject.CompareTag("Player 2 Goal");

        if (enterP1Goal)
        {
            GameController.Instance.AddScore("Player 2");
        }

        if (enterP2Goal)
        {
            GameController.Instance.AddScore("Player 1");
        }

        if (enterP1Goal || enterP2Goal)
        {
            GameController.Instance.ResetGame();
        }

    }
}
