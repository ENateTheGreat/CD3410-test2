/* Author: E. Nathan Lee
 * Date: 12/13/2025
 * Description: Snake game controller script for Unity, managing snake movement, apple spawning, scoring, and game over conditions.
 */

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SnakeController : MonoBehaviour
{
    public GameObject applePrefab; // Apple obj
    public AudioClip biteClip; // Bite audio
    private Transform appleTransform; // Apple "position"
    private Vector2Int appleCell; // Apple location

    public int minAppleDistance = 5; // Min distance from head to apple
    public float biteVolume = 0.5f; // Bite volume


    public AudioSource audioSource; // Sound source

    public GameObject headPrefab; // Head obj
    public GameObject bodyPrefab; // Body obj

    public GridManager gridManager; // Grid manager

    public float moveInterval = 0.5f; // Move interval
    public Vector2Int startCell = new Vector2Int(10, 10); // Start cell

    private readonly List<Transform> segments = new(); // Snake segments
    private readonly List<Vector2Int> segmentCells = new(); // Snake segment cells

    private Vector2Int direction = Vector2Int.up; // Current direction
    private Vector2Int dirQueue = Vector2Int.right; // Queued direction

    private int count = 0; // Apple count
    public TMP_Text countText; // Apple count text

    public TMP_Text gameOver; // Game over text
    public bool gameOverState = false; // Game over state
    public AudioClip deathClip; // Zap sound
    public Button restartButton; // Restart button

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>(); // Load audio source
    }

    // Start is called before the first frame update
    void Start()
    {
        // Snake Init
        var head = Instantiate(headPrefab);
        segments.Add(head.transform);

        segmentCells.Add(startCell);
        head.transform.position = gridManager.GridToWorld(startCell);

        Grow();
        Grow();
        Grow();
        Grow();

        SpawnOrMoveApple(forceSpawn: true); // Spawn apple

        StartCoroutine(MoveLoop()); // Start movement engine

        // init text
        countText.text = count.ToString();
        gameOver.text = "";
        restartButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    { // Movement input   
        if (Input.GetKeyDown(KeyCode.W)) TryQueueDir(Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.S)) TryQueueDir(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.A)) TryQueueDir(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.D)) TryQueueDir(Vector2Int.right);
    }

    private void TryQueueDir(Vector2Int newDir) // Queue direction change
    {
        if (newDir == -direction) return; // Prevent reversing
        dirQueue = newDir;
    }

    private IEnumerator MoveLoop() // Movement engine
    {
        while (true)
        {
            yield return new WaitForSeconds(moveInterval);
            Move();
        }
    }

    private void Move() // Movement function
    {
        if (gameOverState) return; // Game over exit
        direction = dirQueue; // Apply queued direction

        Vector2Int nextHeadCell = segmentCells[0] + direction; // Next head cell

        if (!gridManager.IsInsideGrid(nextHeadCell)) // Game over check
        {
            gameOverState = true;
            gameOver.text = "Game Over";
            audioSource.PlayOneShot(deathClip, biteVolume);
            restartButton.gameObject.SetActive(true);
            return;
        }

        for (int i = 1; i < segmentCells.Count; i++) // Self collision game over check
        {
            if (segmentCells[i] == nextHeadCell)
            {
                gameOverState = true;
                gameOver.text = "Game Over";
                audioSource.PlayOneShot(deathClip, biteVolume);
                restartButton.gameObject.SetActive(true);
                return;
            }
        }

        for (int i = segmentCells.Count - 1; i > 0; i--) // Move body
        {
            segmentCells[i] = segmentCells[i - 1];
        }

        segmentCells[0] = nextHeadCell; // Head move

        for (int i = 0; i < segments.Count; i++) // Update segment positions
        {
            segments[i].position = gridManager.GridToWorld(segmentCells[i]);
        }

        CheckAppleCollision(); // Check apple collision
    }

    public void Grow() // Grow snake on apple eat
    {
        Vector2Int tailCell = segmentCells[segmentCells.Count - 1]; // Find tail

        // Create body segment
        var body = Instantiate(bodyPrefab); 
        segments.Add(body.transform);
        segmentCells.Add(tailCell);
        body.transform.position = gridManager.GridToWorld(tailCell);
    }

    private void CheckAppleCollision() // Check apple collision function
    {
        if (segmentCells[0] != appleCell) return; // Exit on no apple

        audioSource.PlayOneShot(biteClip, biteVolume); // Sound bite

        Grow();
        // Score
        count++;
        countText.text = count.ToString();

        SpawnOrMoveApple(forceSpawn: false); // Respawn apple
    }

    private void SpawnOrMoveApple(bool forceSpawn) // Respawn apple function
    {
        if (forceSpawn || appleTransform == null) // Spawn apple if needed
        {
            var appleGo = Instantiate(applePrefab);
            appleTransform = appleGo.transform;
        }

        appleCell = FindValidAppleCell(); // Find valid apple cell
        appleTransform.position = gridManager.GridToWorld(appleCell);
    }

    private Vector2Int FindValidAppleCell() // Find valid apple cell function
    {
        const int maxAttempts = 500; // Tries to find valid cell
        Vector2Int head = segmentCells[0]; // Head Location

        for (int attempt = 0; attempt < maxAttempts; attempt++) // Cell check loop
        {
            int x = Random.Range(0, 20);
            int y = Random.Range(0, 20);
            var candidate = new Vector2Int(x, y);

            int dist = Mathf.Abs(candidate.x - head.x) + Mathf.Abs(candidate.y - head.y);
            if (dist < minAppleDistance) continue;

            // not on snake check
            bool onSnake = false;
            for (int i = 0; i < segmentCells.Count; i++)
            {
                if (segmentCells[i] == candidate) { onSnake = true; break; }
            }
            if (onSnake) continue;

            return candidate;
        }

        for (int attempt = 0; attempt < maxAttempts; attempt++) // Fallback cell check loop
        {
            int x = Random.Range(0, 20);
            int y = Random.Range(0, 20);
            var candidate = new Vector2Int(x, y);

            bool onSnake = false;
            for (int i = 0; i < segmentCells.Count; i++)
            {
                if (segmentCells[i] == candidate) { onSnake = true; break; }
            }
            if (!onSnake) return candidate;
        }

        return head; // If you win, could add victory function
    }

    public void RestartGame() // Restart function
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
