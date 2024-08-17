using Fusion;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    [Range(1, 20)] public float Speed = 5f;
    public float MaxSpeed = 30;
    [Tooltip("Time in seconds for each speed increment")]
    public float SpeedIncrement = 30;
    [Tooltip("Multiplier applied to the speed")]
    public float SpeedIncrementFactor = 1.1f;

    public Rigidbody rb;
    public MapPrefabs mapPrefabs;

    Vector3 initPos;
    Vector3 pausedVelocity;

    float xDir, yDir;
    float elapsedTime = 0;

    GameObject paddle1, paddle2;
    AIController paddleController1, paddleController2;

    bool _initialLaunchDone = false;

    public override void Spawned()
    {
        if (!Runner.IsServer) return;

        initPos = transform.position;

        LaunchBall();
        bl_EventHandler.Match.onPauseCall += OnGamePaused;
    }

    public override void FixedUpdateNetwork()
    {
        if (GameController.Instance == null) return;

        if (GameTimer.Instance.IsGameDone > 0)
        {
            rb.velocity = Vector3.zero;
        }

        if (_initialLaunchDone)
        {
            elapsedTime = (Runner.Tick - GameTimer._initialTick) / (float)Runner.TickRate;

            float incrementCount = Mathf.Floor(elapsedTime / SpeedIncrement);
            float newSpeed = Speed * Mathf.Pow(SpeedIncrementFactor, incrementCount);

            Speed = Mathf.Min(newSpeed, MaxSpeed);

            rb.velocity = rb.velocity.normalized * Speed;
        }
        else
        {
            LaunchBall();
            _initialLaunchDone = true;
        }
    }

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
        if (!Runner.IsServer) return;

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
        if (!Runner.IsServer) return;

        if (GameTimer.Instance != null && GameTimer.Instance.IsGameDone > 0) return;

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

    float HitFactor(Vector2 ballPos, Vector2 playerPos, float playerHeight) => (ballPos.y - playerPos.y) / playerHeight;

    void OnTriggerEnter(Collider other)
    {
        if (!Runner.IsServer) return;

        if (GameTimer.Instance != null && GameTimer.Instance.IsGameDone > 0) return;

        Team scoringTeam = GetScoringTeam(other.gameObject.tag);

        if (scoringTeam != Team.None)
        {
            GameTimer.AddScore(scoringTeam);

            //Note: Disable these when training.
            SetBallToInit();

            GameController.Instance.ResetGame();
        }
    }

    Team GetScoringTeam(string tag)
    {
        if (tag == "Player 1 Goal")
        {
            return Team.Team2;
        }
        else if (tag == "Player 2 Goal")
        {
            return Team.Team1;
        }

        return Team.None;
    }
}
