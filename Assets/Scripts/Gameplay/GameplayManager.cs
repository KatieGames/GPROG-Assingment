using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using TMPro;

public class GameplayManager : MonoBehaviour
{
    // Round properties
    [Header("Round properties")]
    [SerializeField] private float roundLength = 2f;
    [SerializeField] private int maxDogs = 5;
    [HideInInspector] public int score = 0;

    // Player properties
    [Header("Player properties")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private GameObject playerPrefab;
    private GameObject playerObject;

    // Dog spawning
    [Header("Dog spawning")]
    [SerializeField] private GameObject dogPrefab;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private Transform spawnArea; // navmesh plane
    public Transform[] idleZones; // dog spawn points

    // UI Elements
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [HideInInspector] public string currentStepVolume = "Quiet";
    [SerializeField] private TextMeshProUGUI soundText;

    // Misc vars
    private bool gameRunning = true;
    [HideInInspector] public float secondsLeft;
    [HideInInspector] public static GameplayManager instance;
    [HideInInspector] public int dogCount = 0;

    void Awake()
    {
        // makes it a singleton so it works when carried over accross scenes and referenced by its instance
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void StartGame()
    {
        // start music
        AudioManager.instance.PlayMusic("bgm");

        // set the player active
        playerObject = SpawnOnNavmesh(playerSpawnPoint, playerPrefab);
        playerObject.GetComponent<Player>().gameplayManager = this;

        // spawn dogs
        StartCoroutine(SpawnDogs());

        // start timer
        StartCoroutine(StartTimer(roundLength));

    }

    // can be called by timer ending or the gate
    void EndGame(bool outcome)
    {
        // false is time ran out, true is gate ending
        if (outcome)
        {
            // stop player movement or change scene or something
            playerObject.SetActive(false);
            Debug.Log($"Game completed with a score of {score}!");
        }
        else
        {
            // stop player movement or change scene or something
            playerObject.SetActive(false);
            Debug.Log("Player ran out of time!");
        }

        // could show score ui, leaderboard, etc here
    }

    IEnumerator SpawnDogs()
    {
        // start a coroutine randomly spawning dogs at intervals
        while (gameRunning)
        {
            if(dogCount < maxDogs)
            {
                SpawnDog();
                yield return new WaitForSeconds(spawnInterval);
            }
            else
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }

    IEnumerator StartTimer(float waitTime)
    {
        // timer to end the game after a desired length
        secondsLeft = waitTime;

        while (secondsLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            secondsLeft--;
        }

        EndGame(false);
    }

    void SpawnDog()
    {
        // get the bounds of the plane
        Renderer planeRenderer = spawnArea.GetComponent<Renderer>();

        // am using plane renderer bounds because its just convinient and accounts for all the properties of the transform just giving a bounding box.
        Bounds planeBounds = planeRenderer.bounds;

        // generate a random position within the bounds of the plane
        Vector3 randomPosition = new Vector3(
            Random.Range(planeBounds.min.x, planeBounds.max.x),
            planeBounds.center.y, // use the center Y of the plane
            Random.Range(planeBounds.min.z, planeBounds.max.z)
        );
        
        // spawn the dog on the chosen position
        GameObject dog = SpawnOnNavmesh(randomPosition, dogPrefab);

        // setup the dogs parameters
        dog.GetComponent<DogStateMachine>().SetPlayer(playerObject.transform.Find("PlayerObj").gameObject);

        // increase the dog count so we know how many are not caught, this allows limiting the max amount of "wild" dogs in a session
        dogCount++;
    }

    GameObject SpawnOnNavmesh(Vector3 position, GameObject prefab)
    {
        GameObject obj = null;
        // ensure the position is on the NavMesh
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 100f, NavMesh.AllAreas))
        {
            obj = Instantiate(prefab, hit.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Failed to spawn: " + prefab.name);
        }

        return obj;
    }

    // override that supports a full transform
    GameObject SpawnOnNavmesh(Transform position, GameObject prefab)
    {
        GameObject obj = null;
        // ensure the position is on the NavMesh
        if (NavMesh.SamplePosition(position.position, out NavMeshHit hit, 100f, NavMesh.AllAreas))
        {
            obj = Instantiate(prefab, hit.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Failed to spawn: " + prefab.name);
        }

        return obj;
    }

    private void Start() 
    {
        StartGame();
    }

    private void Update() 
    {
        // update ui elements
        scoreText.text = $"Score: {score}";
        timeText.text = $"Time remaining: {secondsLeft}";
        soundText.text = $"Current noise level: {currentStepVolume}";
    }
}
