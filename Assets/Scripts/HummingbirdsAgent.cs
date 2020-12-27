using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
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
    /// called when and action is received from either the player input or the neural network
    /// vector[i] represent:
    /// index 0: move vector x(+1=right, -1 = left)
    /// index 1: move vector y(+1 = up,-1 = down)
    /// index 2: move vector z(+1 = forward, -1 = backward)
    /// index 3: pitch angle (+1 = pitch up,-1 = pitch down)
    /// index 4: yaw angle (+1 = turn right, -1 = turn left)
    /// </summary>
    /// <param name="vectorAction">the actions to take</param>
    public override void OnActionReceived(float[] vectorAction)
    {
        // don't take actions if frozen
        if (frozen) return;
        // calculate movement vector
        Vector3 move = new Vector3(vectorAction[0], vectorAction[1], vectorAction[2]);

        // add force in the direction of the move vector
        PlayerRigidbody.AddForce(move * moveoForce);

        // get the current position
        Vector3 rotationVector = transform.rotation.eulerAngles;
        // calculate pitch and yaw rotation
        float pitchChange = vectorAction[3];
        float yawChange = vectorAction[4];
        // calculate smooth rotation changae
        smoothPitchChange = Mathf.MoveTowards(smoothPitchChange, pitchChange, 2f * Time.fixedDeltaTime);
        smoothYawchange = Mathf.MoveTowards(smoothYawchange, yawChange, 2f * Time.fixedDeltaTime);


        //calculate new pitch and yaw based on smoothed values
        // clamp pitch to avoid flipping upside down
        float pitch = rotationVector.x + smoothPitchChange * Time.fixedDeltaTime * pitchSpeed;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -maxPitchAngle, maxPitchAngle);
        float yaw = rotationVector.y + smoothYawchange * Time.fixedDeltaTime * yawSpeed;

        // apply new rotation
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    /// <summary>
    /// collect vector observation from the environment
    /// </summary>
    /// <param name="sensor">the vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // If nearestFlower is null, observe an empty array and return early
        if (nearestFlower == null)
        {
            sensor.AddObservation(new float[10]);
            return;
        }


        // Observe the agent's local rotation (4 observations)
        sensor.AddObservation(transform.localRotation.normalized);

        // Get a vector from the beak tip to the nearest flower
        Vector3 toFlower = nearestFlower.FlowerCenterPosition - beakTip.position;

        // Observe a normalized vector pointing to the nearest flower (3 observations)
        sensor.AddObservation(toFlower.normalized);

        // Observe a dot product that indicates whether the beak tip is in front of the flower (1 observation)
        // (+1 means that the beak tip is directly in front of the flower, -1 means directly behind)
        sensor.AddObservation(Vector3.Dot(toFlower.normalized, -nearestFlower.FlowerUpVector.normalized));

        // Observe a dot product that indicates whether the beak is pointing toward the flower (1 observation)
        // (+1 means that the beak is pointing directly at the flower, -1 means directly away)
        sensor.AddObservation(Vector3.Dot(beakTip.forward.normalized, -nearestFlower.FlowerUpVector.normalized));

        // Observe the relative distance from the beak tip to the flower (1 observation)
        sensor.AddObservation(toFlower.magnitude / FlowerArea.AreaDiameter);

        // 10 total observations
    }

    /// <summary>
    /// when behavior type is set to huristic only on the agent's behavior parameter,
    /// the function will be called. it reture value will be into onActionRecives instead of using neural network
    /// </summary>
    /// <param name="actionsOut">an output action array</param>
    public override void Heuristic(float[] actionsOut)
    {
        // create placeholders for all movement/turning
        Vector3 forward = Vector3.zero;
        Vector3 left = Vector3.zero;
        Vector3 up = Vector3.zero;
        float pitch = 0f;
        float yaw = 0f;
        if (Input.GetKey(KeyCode.W)) forward = transform.forward;
        else if (Input.GetKey(KeyCode.S)) forward = -transform.forward;
        // left /right
        if (Input.GetKey(KeyCode.A)) left = -transform.right;
        else if (Input.GetKey(KeyCode.D)) left = transform.right;

        // up/down
        if (Input.GetKey(KeyCode.E)) up = transform.up;
        else if (Input.GetKey(KeyCode.C)) up = -transform.up;

        // pitch up/down
        if (Input.GetKey(KeyCode.UpArrow)) pitch = 1f;
        else if (Input.GetKey(KeyCode.DownArrow)) pitch = -1f;

        //yaw left/right 
        if (Input.GetKey(KeyCode.LeftArrow)) yaw = 1f;
        else if (Input.GetKey(KeyCode.RightArrow)) yaw = 1f;

        // Combine the movement vectors and normalize
        Vector3 combined = (forward + left + up).normalized;

        // Add the 3 movement values, pitch, and yaw to the actionsOut array
        actionsOut[0] = combined.x;
        actionsOut[1] = combined.y;
        actionsOut[2] = combined.z;
        actionsOut[3] = pitch;
        actionsOut[4] = yaw;

    }


    /// <summary>
    /// Prevent the agent from moving and taking actions
    /// </summary>
    public void FreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported in training");
        frozen = true;
        PlayerRigidbody.Sleep();
    }

    /// <summary>
    /// Resume agent movement and actions
    /// </summary>
    public void UnfreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported in training");
        frozen = false;
        PlayerRigidbody.WakeUp();
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

    /// <summary>
    /// called when the agent's collider enters a tigger collider
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        TiggerEnterOrStay(other);
    }
    private void OnTriggerStay(Collider other)
    {
        TiggerEnterOrStay(other);
    }
    /// <summary>
    /// handles when the agents collider enters or stays in a trigger collider
    /// </summary>
    /// <param name="collider"></param>
    private void TiggerEnterOrStay(Collider collider)
    {
        if (collider.CompareTag("nectar"))
        {
            Vector3 closestPointToBeakTip = collider.ClosestPoint(beakTip.position);
            if (Vector3.Distance(beakTip.position, closestPointToBeakTip) < BeakTipRadius)
            {
                Flower flower = flowerArea.GetFlowerFromNectar(collider);
                float nectarReceived = flower.Feed(0.01f);
                NectarObtained += nectarReceived;
                if (trainingMode)
                {
                    float bonus = 0.02f * Mathf.Clamp01(Vector3.Dot(transform.forward.normalized, -nearestFlower.FlowerUpVector.normalized));
                    AddReward(0.01f + bonus);
                }
                // if flower is empty, update the nearest flower
                if (!flower.HasNectar)
                {
                    UpdateNearestFlower();
                }
            }
        }
    }
    /// <summary>
    /// called when the agent collides with something solid
    /// </summary>
    /// <param name="collision">the collision info</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (trainingMode && collision.collider.CompareTag("boundary"))
        {
            AddReward(-0.5f);
        }
    }

    /// <summary>
    /// called every frame
    /// </summary>
    private void Update()
    {
        // draw a line from the beak tip to the nearest flower
        if (nearestFlower != null)
        {
            Debug.DrawLine(beakTip.position, nearestFlower.FlowerCenterPosition, Color.green);
        }
    }
    /// <summary>
    /// called every 0.02 sec
    /// </summary>
    private void FixedUpdate()
    {
        if (nearestFlower != null && !nearestFlower.HasNectar)
        {
            UpdateNearestFlower();

        }
    }
}
