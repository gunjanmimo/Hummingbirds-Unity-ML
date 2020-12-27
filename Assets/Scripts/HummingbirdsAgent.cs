using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// a hummingbird machine learning agent
/// </summary>
public class HummingbirdsAgent : Agent
{
    [Tooltip("Force to apply when moving")]
    public float moveoForce = 2f;

    [Tooltip("speed to pitch up or down")]
    public float pitchSpeed = 100f;

    [Tooltip("speed to rotate around the up axis")]
    public float yawSpeed = 100f;

    [Tooltip("transform at the tip of the beak")]
    public Transform beakTip;

    [Tooltip("the agent camera")]
    public Camera agentCamera;

    [Tooltip("wheater this is training mode or gameplay mode")]
    public bool trainingMode;

    private Rigidbody PlayerRigidbody;
    // the flower area that the agent is in
    private Flower nearestFlower;
    private FlowerArea flowerArea;
    // private float smoothPitchChange = 0f;
    private float smoothPitchChange = 0f;
    // allow for smoother yaw change
    private float smoothYawchange = 0f;

    // maximu angle that the bird can pitch up or down

    private const float maxPitchAngle = 80f;
    // maximum distance from the beak tip to accept nectar collision
    private const float BeakTipRadius = 0.008f;
    // wheather the agent is frozen
    private bool frozen = false;

    public float NectarObtained { get; private set; }


    /// <summary>
    /// init agent
    /// </summary>
    public override void Initialize()
    {
        PlayerRigidbody = GetComponent<Rigidbody>();
        flowerArea = GetComponent<FlowerArea>();
        // if not training mode, no max step, play forever
        if (!trainingMode) MaxStep = 0;
    }
    public override void OnEpisodeBegin()
    {
        if (trainingMode)
        {
            // only reset flowers in training when there  is one agent per area
            flowerArea.ResetFlowers();
        }
        NectarObtained = 0f;
        PlayerRigidbody.velocity = Vector3.zero;
        PlayerRigidbody.angularVelocity = Vector3.zero;
        // default to spawning in front of a flower
        bool inFrontOfFlower = true;
        if (trainingMode)
        {
            inFrontOfFlower = Random.value > 0.5f;
        }
        // move the agent to a new random position
        MoveToSafeRandomPosition(inFrontOfFlower);

        // recalculate the nearest flower now that agent has moved
        UpdateNearestFlower();
    }
    /// <summary>
    /// update the nearest the flower to the agent
    /// </summary>
    private void UpdateNearestFlower()
    {
        foreach (Flower flower in flowerArea.Flowers)
        {
            if (nearestFlower == null && flower.HasNectar)
            {
                nearestFlower = flower;
            }
            else if (flower.HasNectar)
            {
                // calculate distance of this flower and distance to the current nearest flower
                float distanceToFlower = Vector3.Distance(flower.transform.position, beakTip.position);
                float distanceToCurrentNearestFlower = Vector3.Distance(nearestFlower.transform.position, beakTip.position);

                // if current nearest flower is empty and this flower is closer, update the nearst flower
                if (!nearestFlower.HasNectar || distanceToFlower < distanceToCurrentNearestFlower)
                {
                    nearestFlower = flower;
                }
            }

        }
    }

    /// <summary>
    /// move the agent to a safe random position
    /// </summary>
    /// <param name="inFrontOfFlower"></param>
    private void MoveToSafeRandomPosition(bool inFrontOfFlower)
    {
        bool safePositionFound = false;
        int attemptsRemaining = 100;
        Vector3 potentialPosition = Vector3.zero;
        Quaternion potentialRotation = new Quaternion();
        while (!safePositionFound && attemptsRemaining > 0)
        {
            attemptsRemaining--;
            if (inFrontOfFlower)
            {
                Flower randomFlower = flowerArea.Flowers[Random.Range(0, flowerArea.Flowers.Count)];
                // position 10 to 20 cm in front of the flower
                float distaceFromFlower = Random.Range(.1f, .2f);
                potentialPosition = randomFlower.transform.position + randomFlower.FlowerUpVector * distaceFromFlower;
                // point beak at flower
                Vector3 toFlower = randomFlower.FlowerCenterPosition - potentialPosition;
                potentialRotation = Quaternion.LookRotation(toFlower, Vector3.up);
            }
            else
            {
                // pick a random height
                float height = Random.Range(1.2f, 2.5f);
                // pick a random radius from the center of the area
                float radius = Random.Range(2f, 7f);
                // pick a random direction rotated arounf the y axis
                Quaternion direction = Quaternion.Euler(0f, Random.Range(-180f, 180f), 0f);
                // combine height, radius and direction
                potentialPosition = flowerArea.transform.position + Vector3.up * height + direction * Vector3.forward * radius;
                // choose and set random starting pitch and yaw
                float pitch = Random.Range(-60f, 60f);
                float yaw = Random.Range(-180f, 180f);

                potentialRotation = Quaternion.Euler(pitch, yaw, 0f);
            }
            // check to see if the agent will collider
            Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.05f);
            safePositionFound = colliders.Length == 0;
        }
        Debug.Assert(safePositionFound, " could not find a safe position to spawn");

        // set the position and rotation
        transform.position = potentialPosition;
        transform.rotation = potentialRotation;
    }
}
