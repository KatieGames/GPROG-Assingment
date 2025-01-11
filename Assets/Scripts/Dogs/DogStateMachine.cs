using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using TMPro;

public class DogStateMachine : PlayerDetection
{
    // State-related Variables
    [Header("State-related Variables")]
    [SerializeField] private State currentState; 
    private Vector3 chosenRestSpot;
    private Vector3 prevRestSpot;
    private Vector3 fleeDestination;
    public bool caught = false;
    private bool playerSeen = false;
    private bool restSpotDecided = false;

    // Navigation & Transform References
    private Transform restSpot;
    private NavMeshAgent agent;

    // Search & Alert Variables
    [Header("Search & Alert Variables")]
    private float alertSearchTimer = 0f;  // time spent in AlertSearching state
    private float alertSearchDuration = 0f;  // duration to look around
    private float currentRotation = 0f;
    [SerializeField] private float maxSearchRotation = 180f;  // maximum rotation per direction
    [SerializeField] private float searchRotationSpeed = 35f;  // speed of rotation
    [SerializeField] private float fleeDistance = 15f;  // distance for fleeing
    public TextMeshProUGUI stateText;

    private enum State
    {
        IdleDigging,
        IdleHiding,
        IdleSleeping,
        PatrolFindingRestSpot,
        AlertSearching,
        Fleeing,
        Caught
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = State.PatrolFindingRestSpot;
        StartCoroutine(StateMachine());
    }

    private IEnumerator StateMachine()
    {
        while (true)
        {
            stateText.text = currentState.ToString();

            // if you are caught just follow the player. If not go through the state machine
            if(!caught)
            {
                int detectionResult = CheckPlayerDetection();

                switch(detectionResult)
                {
                    case 1:
                        // only go to searching if not already running
                        if(currentState != State.Fleeing)
                        {
                            // this means the player has been heard but not very close, we will now look for them
                            currentState = State.AlertSearching;
                            restSpotDecided = false;
                            break;
                        }
                        break;
                    case 2:
                        // player has been heard or seen so run away
                        playerSeen = true;
                        currentState = State.Fleeing;
                        alertSearchTimer = 0f;
                        break;
                }
            }
            else
            {
                // follow player if caught
                currentState = State.Caught;
            }

            // run the different states
            switch (currentState)
            {
                // the idle states are where animations, sounds, etc would be played. As this is just a technical demo these remain empty
                case State.IdleDigging:
                    break;

                case State.IdleHiding:
                    break;

                case State.IdleSleeping:
                    break;

                case State.AlertSearching:
                    // look for the player
                    HandleAlertSearching();
                    break;

                case State.PatrolFindingRestSpot:
                    if(!restSpotDecided)
                    {
                        // Debug.Log("patrol finding rest spot");

                        // get from an array of rest spot locations from the gameplay manager singleton
                        chosenRestSpot = GetNearestIdleZone(transform.position).position;

                        // add a random offset so all the dogs dont just go to the same place
                        chosenRestSpot += new Vector3(
                            Random.Range(-10f, 10f),
                            Random.Range(-10f, 10f),
                            Random.Range(-10f, 10f)
                        );

                        if (chosenRestSpot != null)
                        {
                            // find the nearest valid NavMesh position
                            if (NavMesh.SamplePosition(chosenRestSpot, out NavMeshHit hit1, fleeDistance, NavMesh.AllAreas))
                            {
                                agent.SetDestination(hit1.position);
                                restSpotDecided = true;
                            }
                            else
                            {
                                Debug.LogWarning("Failed to find a valid flee destination on the NavMesh.");
                            }
                        }
                    }

                    // rotate the dog to point the correct way
                    transform.LookAt(new Vector3(chosenRestSpot.x, transform.position.y, chosenRestSpot.z));

                    // if its close enough to the rest spot go to an idle state and clear our target rest spot
                    if(Vector3.Distance(transform.position, chosenRestSpot) < 5f)
                    {
                        // pick a random idle state
                        currentState = (State)Random.Range((int)State.IdleDigging, (int)State.IdleSleeping + 1);
                        restSpotDecided = false;
                    }
                    break;

                case State.Fleeing:
                    if(playerSeen)
                    {
                        // path away from the player
                        // Debug.Log("fleeing from player");

                        // get the direction away from the player
                        Vector3 fleeDirection = (agent.transform.position - player.transform.position).normalized;

                        // scale the flee direction to determine how far to flee
                        fleeDestination = agent.transform.position + fleeDirection * fleeDistance;

                        // find the nearest NavMesh position
                        if (NavMesh.SamplePosition(fleeDestination, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
                        {
                            fleeDestination = hit.position;
                            agent.SetDestination(fleeDestination);
                        }
                        else
                        {
                            Debug.LogWarning("Failed to find a valid flee destination on the NavMesh.");
                        }
                    }

                    // rotate the dog to point the correct way
                    transform.LookAt(new Vector3(fleeDestination.x, transform.position.y, fleeDestination.z));

                    // if its close enough to the rest spot go to an idle state and clear our target rest spot
                    if(Vector3.Distance(transform.position, fleeDestination) < 5f)
                    {
                        restSpotDecided = false;
                        currentState = State.PatrolFindingRestSpot;
                    }
                    playerSeen = false;
                    break;

                case State.Caught:
                    // just set our destination to the player so we follow them
                    // Debug.Log("caught");

                    // maintain a reasonable distance from the player
                    if(Vector3.Distance(transform.position, player.transform.position) > 10f)
                    {
                        // find the nearest valid NavMesh position
                        if (NavMesh.SamplePosition(player.transform.position, out NavMeshHit hit2, fleeDistance, NavMesh.AllAreas))
                        {
                            agent.SetDestination(hit2.position);
                        }
                        else
                        {
                            Debug.LogWarning("Failed to find a valid flee destination on the NavMesh.");
                        }
                    }
                    break;
            }

            yield return null;
        }
    }

    private void HandleAlertSearching()
    {
        agent.ResetPath();

        // if the agent has not started searching
        if (alertSearchTimer == 0f)
        {
            alertSearchDuration = Random.Range(3f, 6f); // set random duration
            currentRotation = 0f; // reset rotation
            // Debug.Log("Alert: Searching for player");
        }

        // slow the rotation amount so it is smooth
        float rotationThisFrame = searchRotationSpeed * Time.deltaTime * Mathf.Sign(maxSearchRotation);
        transform.Rotate(0, rotationThisFrame, 0);
        currentRotation += rotationThisFrame;

        // flip direction when the rotation exceeds limit
        if (Mathf.Abs(currentRotation) >= Mathf.Abs(maxSearchRotation))
        {
            maxSearchRotation = -maxSearchRotation;
            currentRotation = 0f;
        }

        // check for player detection
        int detectionResult = CheckPlayerDetection();

        // player detected
        if (detectionResult == 2)
        {
            currentState = State.Fleeing;
            alertSearchTimer = 0f;
            return;
        }

        // update timer
        alertSearchTimer += Time.deltaTime;

        // if time exceeds the set duration go find a new rest spot
        if (alertSearchTimer >= alertSearchDuration)
        {
            currentState = State.PatrolFindingRestSpot;
            alertSearchTimer = 0f;
        }
    }

    public Transform GetNearestIdleZone(Vector3 currentPosition)
    {
        Transform nearestZone = null;
        float shortestDistance = Mathf.Infinity;

        // iterate over all the rest zones in the gameplay manager
        foreach (Transform zone in GameplayManager.instance.idleZones)
        {
            // get the nearest rest zone that isnt the same as the last one we went to
            float distance = Vector3.Distance(currentPosition, zone.position);
            if (distance < shortestDistance && zone.position != prevRestSpot)
            {
                shortestDistance = distance;
                nearestZone = zone;
            }
        }

        prevRestSpot = nearestZone.position;

        return nearestZone;
    }
}