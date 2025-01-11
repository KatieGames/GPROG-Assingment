using UnityEngine;


public class PlayerDetection : MonoBehaviour
{
    // View Cone Options
    [Header("View Cone Options")]
    [SerializeField] private float viewAngle = 60f;  // field of view angle (cone angle)
    [SerializeField] private float viewDistance = 10f;  // view distance (radius of cone)
    [SerializeField] private LayerMask obstructionLayer;  // obstacles (like walls and stuff)

    // Sound Sphere Options
    [Header("Sound Sphere Options")]
    [SerializeField] private float hearingRadius = 10f;  // Radius of hearing

    // Player Hearing State
    private int playerHeard = 0;  // 0 = no, 1 = alert, 2 = panic run
    protected GameObject player;  // reference to player transform, this needs to be set in runtime or maybe use a singleton

    public int CheckPlayerDetection()
    {
        // if has line of sight to player
        if(PlayerInView())
        {
            // run
            return 2;
        }
        
        // 0 = no, 1 = alert, 2 = panic run
        switch(playerHeard)
        {
            case 0:
                break;
            case 1:
                playerHeard = 0;
                return 1;
            case 2:
                playerHeard = 0;
                return 2;
        }

        return 0;
    }

    public void SetPlayer(GameObject playerObject)
    {
        player = playerObject;

        //subscribe to footstep event
        player.GetComponentInChildren<PlayerMovement>().OnSoundMade += HandleFootstep;
    }

    private void OnDisable()
    {
        // unsubscribe from the footstep event
        player.GetComponentInChildren<PlayerMovement>().OnSoundMade -= HandleFootstep;
    }

    void HandleFootstep(Vector3 footstepPosition)
    {
        float distance = Vector3.Distance(transform.position, footstepPosition);
        if(distance < hearingRadius)
        {
            // do nothing if crouched
            if(!player.GetComponentInParent<Player>().isCrouched)
            {

                // if its really close dog runs immediately
                if(distance < hearingRadius/2)
                {
                    playerHeard = 2;
                }
                else
                {
                    playerHeard = 1;
                }
            }
        }
    }

    bool PlayerInView()
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        float angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);

        // check if the player is within the field of view
        if (angleToPlayer < viewAngle / 2f)
        {
            // check if the player is within the view distance
            if (Vector3.Distance(transform.position, player.transform.position) <= viewDistance)
            {
                // raycast to check for clear line of sight
                RaycastHit hit;
                if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, viewDistance, ~obstructionLayer))
                {
                    if (hit.transform.name.Contains("Player"))
                    {
                        // Debug.Log("Can see player");
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void OnDrawGizmos()
    {
        // field of view cone
        Gizmos.color = Color.yellow;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward * viewDistance;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        // raycast to the player
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.transform.position);
        }

        // hearing sphere
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
    }
}