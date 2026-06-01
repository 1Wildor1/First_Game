using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int monstersToKill = 20;

    private int killedMonsters = 0;
    public GameObject losePanel;
    public GameObject winPanel;
    public TextMeshProUGUI killsText;
    public void LoseGame()
    {
        if (losePanel != null)
            losePanel.SetActive(true);

        Time.timeScale = 0f;
    }
    void UpdateUI()
    {
        if (killsText != null)
        {
            killsText.text = $"{killedMonsters}/{monstersToKill}";
        }
    }
    private void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    public void MonsterKilled()
    {
        killedMonsters++;

        UpdateUI();

        Debug.Log($"Killed: {killedMonsters}/{monstersToKill}");

        if (killedMonsters >= monstersToKill)
        {
            WinGame();
        }
    }
    void WinGame()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
    }
}