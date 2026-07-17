using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Ball")]
    [SerializeField] 
    private GameObject ballPrefab;

    [SerializeField] 
    private Transform spawnPoint;

    [SerializeField] 
    private BallLauncher launcher;

    [Header("Game")]
    [SerializeField] 
    private int totalPins = 10;

    [SerializeField] 
    private GameObject newGameButton;

    private BallController currentBall;
    private Pin[] pins;

    private Coroutine removeBallCoroutine;
    private Coroutine restartCoroutine;

    private int knockedPins;
    private int throwCount;
    private int score;

    private bool gameStarted;
    private bool roundFinished;

    public int Score => score;
    public bool GameStarted => gameStarted;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        pins = FindObjectsOfType<Pin>();

        SpawnBall();
    }

    private void Update()
    {
        if (!gameStarted && Input.GetMouseButtonDown(0))
        {
            LaunchBall();
        }
    }

    private void SpawnBall()
    {
        GameObject ball = Instantiate(
            ballPrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        currentBall = ball.GetComponent<BallController>();
    }

    private void LaunchBall()
    {
        if (currentBall == null)
            return;

        gameStarted = true;

        launcher.Launch(currentBall.Rigidbody);
        currentBall.PlayRollingSound();

        if (removeBallCoroutine != null)
        {
            StopCoroutine(removeBallCoroutine);
        }

        removeBallCoroutine = StartCoroutine(RemoveBallRoutine());
    }

    private IEnumerator RemoveBallRoutine()
    {
        yield return new WaitForSeconds(7f);

        if (currentBall != null)
        {
            Destroy(currentBall.gameObject);
            currentBall = null;
        }

        while (!AllPinsStopped())
        {
            yield return null;
        }

        roundFinished = true;
        EndRound();
    }

    public void PinKnockedDown()
    {
        knockedPins++;
        score++;

        Debug.Log($"Очки: {score}");

        CheckBonus();
    }

    private void CheckBonus()
    {
        if (knockedPins != totalPins)
            return;

        if (throwCount == 0)
        {
            score += 10;
            Debug.Log("STRIKE!!!");
        }
        else if (throwCount == 1)
        {
            score += 10;
            Debug.Log("SPARE!");
        }
    }

    private bool AllPinsStopped()
    {
        foreach (Pin pin in pins)
        {
            if (pin == null)
                continue;

            Rigidbody rb = pin.GetComponent<Rigidbody>();

            if (rb.velocity.sqrMagnitude > 0.0025f)
                return false;

            if (rb.angularVelocity.sqrMagnitude > 0.0025f)
                return false;
        }

        return true;
    }

    private void EndRound()
    {
        Debug.Log("Раунд завершен");

        newGameButton.SetActive(true);
    }

    public void RestartGame()
    {
        if (restartCoroutine != null)
            StopCoroutine(restartCoroutine);

        if (removeBallCoroutine != null)
        {
            StopCoroutine(removeBallCoroutine);
            removeBallCoroutine = null;
        }

        restartCoroutine = StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        gameStarted = false;
        roundFinished = false;

        if (currentBall != null)
        {
            Destroy(currentBall.gameObject);
            currentBall = null;
        }

        yield return new WaitForFixedUpdate();

        foreach (Pin pin in pins)
        {
            if (pin != null)
                pin.ResetPin();
        }

        yield return new WaitForFixedUpdate();

        knockedPins = 0;
        throwCount = 0;

        launcher.ResetLaunch();

        SpawnBall();

        newGameButton.SetActive(false);

        restartCoroutine = null;
    }
}