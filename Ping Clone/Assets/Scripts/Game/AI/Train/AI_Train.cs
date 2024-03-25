using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AI_Train : Agent
{
    public float Speed = 5f;
    public bool IsLeftSide = false;
    public BehaviorParameters behavior;
    public MapPrefabs mapPrefabs;
    public GameObject Ray;

    [Header("Rewards")]
    public float rewardForHittingBall = 0.1f;
    public float penaltyForMissingBall = -1f;
    public float penaltyForOutOfBounds = -0.1f;

    Rigidbody ballRigidbody;

    Vector3 initPos;

    public override void Initialize()
    {
        initPos = transform.localPosition;

        UseMapPrefabBall();
    }

    void UseMapPrefabBall()
    {
        if (!mapPrefabs.Ball.TryGetComponent(out ballRigidbody))
        {
            Debug.LogError("No rigidbody attached to Ball");
        }
    }

    public void SetToInit()
    {
        transform.localPosition = initPos;
    }

    public override void OnEpisodeBegin()
    {
        int random = Random.Range(1, 3);

        switch (random)
        {
            case 1:
                //Left side 'spawn'
                mapPrefabs.Bound.transform.localPosition = new(2.97f, 0, 0);
                initPos = SpawnPointManager.Instance.SpawnPoint1;
                Ray.transform.localEulerAngles = new(0, 90, -90);
                break;
            case 2:
                //Right side 'spawn'
                mapPrefabs.Bound.transform.localPosition = new(-2.97f, 0, 0);
                initPos = SpawnPointManager.Instance.SpawnPoint2;
                Ray.transform.localEulerAngles = new(0, -90, -90);
                break;
        }

        transform.localPosition = initPos;
        mapPrefabs.Ball.SetBallToInit();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);

        CollectMapPrefabObservations(sensor);
    }

    void CollectMapPrefabObservations(VectorSensor sensor)
    {
        if (mapPrefabs.Ball != null)
        {
            sensor.AddObservation(mapPrefabs.Ball.transform.localPosition);
            sensor.AddObservation(ballRigidbody.velocity);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float move = actions.ContinuousActions[0];

        Vector3 velocity = new(0, move * Speed);

        Vector3 smoothedVelocity = Vector3.Lerp(transform.localPosition, transform.localPosition + velocity, Time.deltaTime);

        //velocity = Speed * Time.deltaTime * velocity.normalized;

        transform.localPosition = smoothedVelocity;

        CheckOutOfBounds();
    }

    void CheckOutOfBounds()
    {
        float bufferDistance = 0.25f;
        float distanceFromTop = Mathf.Abs(2.97f - transform.localPosition.y);
        float distanceFromBottom = Mathf.Abs(-2.97f - transform.localPosition.y);

        if (distanceFromTop < bufferDistance || distanceFromBottom < bufferDistance)
        {
            AddReward(penaltyForOutOfBounds);
            EndEpisode();
        }
    }

    public void OnBallHit()
    {
        AddReward(rewardForHittingBall);
    }

    public void OnBallMissed()
    {
        //Debug.LogWarning($"{gameObject.name} {nameof(OnBallMissed)}");
        AddReward(penaltyForMissingBall);
        EndEpisode();
    }

    public void OnScored()
    {
        //Debug.LogWarning($"{gameObject.name} {nameof(OnScored)}");
        AddReward(5);
        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        continuousActions[0] = Input.GetAxisRaw("Vertical");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            AddReward(rewardForHittingBall);
            EndEpisode();
        }

        if (collision.gameObject.CompareTag("Bound"))
        {
            AddReward(penaltyForOutOfBounds);
            EndEpisode();
        }
    }
}
