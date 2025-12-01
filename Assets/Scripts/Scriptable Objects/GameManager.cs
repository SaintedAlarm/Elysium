using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Win Condition")]
    public int enemiesToWin = 10;  // set this to how many enemies your spawner will spawn
    private int enemiesKilled = 0;

    private bool gameOver = false;

    void Awake()
    {
        // simple singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void RegisterEnemyKilled()
    {
        if (gameOver) return;

        enemiesKilled++;
        // Debug.Log($"Enemy killed. Total: {enemiesKilled}");

        if (enemiesKilled >= enemiesToWin)
        {
            WinGame();
        }
    }

    public void WinGame()
    {
        if (gameOver) return;
        gameOver = true;

        Debug.Log("GAME: WIN");
        if (winPanel != null) winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void LoseGame()
    {
        if (gameOver) return;
        gameOver = true;

        Debug.Log("GAME: LOSE");
        if (losePanel != null) losePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // optional – hook this to a button to quit play mode
    public void QuitPlayMode()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
