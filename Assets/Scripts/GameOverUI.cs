using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Ссылки на элементы UI")]
    public GameObject panel;       // Вся панель Game Over
    public TMP_Text creditsText;   // Текст "Вы набрали: ..."
    public Button restartButton;   // Кнопка "Заново"

    void Start()
    {
        // Панель по умолчанию выключена
        if (panel != null)
            panel.SetActive(false);

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    // Этот метод вызываем из BatterySystem или таймера миссии
    public void Show(int totalCredits)
    {
        if (panel != null) panel.SetActive(true);
        if (creditsText != null) creditsText.text = $"Вы набрали: {totalCredits}";
    }

    private void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}