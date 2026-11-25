using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLives : MonoBehaviour
{
    public static PlayerLives Instance { get; private set; }

    [SerializeField] private int maxLives = 3;
    [SerializeField] private string deathScene = "Echec";

    private int lives;

    public int Lives => lives;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        lives = maxLives;
        UpdateHearts();
    }

    public void LoseLife(int amount = 1)
    {
        if (lives <= 0) return;

        lives -= amount;
        lives = Mathf.Max(lives, 0);
        UpdateHearts();

        Debug.Log("Vies restantes : " + lives);

        if (lives <= 0)
        {
            Invoke(nameof(GoToDeath), 1.2f);
        }
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < maxLives; i++)
        {
            GameObject heart = GameObject.Find("Heart" + (i + 1));
            if (heart != null) heart.SetActive(i < lives);
        }
    }

    private void GoToDeath()
    {
        SceneManager.LoadScene(deathScene);
    }
}
