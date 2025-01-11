using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool isCrouched;
    public GameplayManager gameplayManager;
    [SerializeField] private GameObject catchObject;
    public LayerMask dogs;
    public Transform rayOrigin;

    private bool aiming = false;

    private Coroutine resetVolumeCoroutine;

    public void IncreaseScore()
    {
        // dog has been caught so increase score and remove 1 from the dog count
        gameplayManager.score++;
        gameplayManager.dogCount--;
    }

    private void Start() 
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GetComponentInChildren<PlayerMovement>().OnSoundMade += OnSoundMade;
    }

    void Update()
    {
        // show and hide the aim object
        if (Input.GetButtonDown("Fire2"))
        {
            catchObject.SetActive(true);
            aiming = true;
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            catchObject.SetActive(false);
            aiming = false;
        }

        if (Input.GetButtonDown("Fire1") && aiming)
        {
            Debug.Log("balls");
            if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, 10f, dogs))
            {
                Debug.Log(hit.collider.name);
                // check if the object hit has a specific tag
                if (hit.collider.CompareTag("Dog"))
                {
                    Debug.Log("Dog detected in front!");
                    DogStateMachine statemachine = hit.collider.gameObject.GetComponentInChildren<DogStateMachine>();

                    if(!statemachine.caught)
                    {
                        statemachine.caught = true;
                        IncreaseScore();
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(rayOrigin.position, rayOrigin.position + rayOrigin.forward * 10);
    }

    void OnSoundMade(Vector3 pos)
    {
        if (!isCrouched)
        {
            gameplayManager.currentStepVolume = "Loud";
        }
        else
        {
            gameplayManager.currentStepVolume = "Quiet";
        }

        if (resetVolumeCoroutine != null)
        {
            StopCoroutine(resetVolumeCoroutine);
        }
        resetVolumeCoroutine = StartCoroutine(ResetVolumeAfterDelay(0.5f));
    }

    IEnumerator ResetVolumeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameplayManager.currentStepVolume = "Quiet";
    }
}
