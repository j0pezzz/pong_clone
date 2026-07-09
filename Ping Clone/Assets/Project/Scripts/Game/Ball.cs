using Fusion;
using Project.Scripts.Game;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    public float Speed = 5f;
    public float MaxSpeed = 30;
    [Tooltip("How often does each speed increment happen?")]
    public float SpeedIncrement = 30;
    [Tooltip("Speed Multiplier")]
    public float SpeedIncrementFactor = 1.1f;

    public Rigidbody rb;
    public MapPrefabs mapPrefabs;

    Vector3 _initPos;
    Vector3 _pausedVelocity;
    float _xDir, _yDir;
    float _elapsedTime;
    float _lastSpeedIncrementTime;
    GameObject _paddle1, _paddle2;
    AIController _paddleController1, _paddleController2;
    bool _initialLaunchDone;

    public override void Spawned()
    {
        if (!NetworkHandler.Instance.IsHost) return;

        _initPos = transform.position;

        LaunchBall();
        bl_EventHandler.Match.OnGlobalGamePause += OnGamePaused;
    }

    public override void FixedUpdateNetwork()
    {
        if (GameManager.Instance.IsGameDone)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        if (_initialLaunchDone)
        {
            _elapsedTime = (Runner.Tick - TimeManager.InitialTick) / (float)Runner.TickRate;

            if (_elapsedTime - _lastSpeedIncrementTime >= SpeedIncrement)
            {
                Speed = Mathf.Min(Speed * SpeedIncrementFactor, MaxSpeed);

                _lastSpeedIncrementTime = _elapsedTime;

                Debug.Log($"Increased speed to {Speed}");
            }
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
            _pausedVelocity = rb.linearVelocity;
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            rb.linearVelocity = _pausedVelocity;
        }
    }

    public void SetBallToInit()
    {
        if (!Runner.IsServer) return;

        transform.position = _initPos;
        LaunchBall();
    }

    void LaunchBall()
    {
        do
        {
            _xDir = Random.Range(-1f, 1f);
        } while (Mathf.Abs(_xDir) < 0.5f);

        _yDir = Random.Range(-0.5f, 0.5f);

        Vector3 launchDir = new Vector3(_xDir, _yDir, 0).normalized;

        rb.linearVelocity = launchDir * Speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!NetworkHandler.Instance.IsHost) return;

        if (GameManager.Instance.IsGameDone) return;

        if (collision.gameObject.CompareTag("Paddle1"))
        {
            float y = HitFactor(transform.position, collision.transform.position, collision.collider.bounds.size.y);

            Vector2 direction = new Vector2(1, y).normalized;

            rb.linearVelocity = direction * Speed;
        }
        else if (collision.gameObject.CompareTag("Paddle2"))
        {
            float y = HitFactor(transform.position, collision.transform.position, collision.collider.bounds.size.y);

            Vector2 direction = new Vector2(-1, y).normalized;

            rb.linearVelocity = direction * Speed;
        }
    }

    float HitFactor(Vector2 ballPos, Vector2 playerPos, float playerHeight) => (ballPos.y - playerPos.y) / playerHeight;

    void OnTriggerEnter(Collider other)
    {
        if (!NetworkHandler.Instance.IsHost) return;

        if (GameManager.Instance.IsGameDone) return;

        Team scoringTeam = GetScoringTeam(other.gameObject.tag);

        if (scoringTeam != Team.None)
        {
            bl_EventHandler.Match.DispatchPointAddition(scoringTeam);

            //Note: Disable these when training.
            SetBallToInit();

            NetworkHandler.Instance.ResetGame();
        }
    }

    Team GetScoringTeam(string nameTag)
    {
        if (nameTag == "Player 1 Goal")
        {
            return Team.Team2;
        }
        
        if (nameTag == "Player 2 Goal")
        {
            return Team.Team1;
        }

        return Team.None;
    }
}
