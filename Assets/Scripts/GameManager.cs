using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;


    // Variables
    private bool isGamePaused;

    // Referencias a otros GO
    public GameObject pauseMenu;


    public static GameManager Instance() 
    {
        if (instance == null)
            instance = new GameObject("GameManager").AddComponent<GameManager>();
        return instance;
    }

    // Awake
    private void Awake()
    {
        if (instance != null && instance != this)
            Object.Destroy(gameObject);
        else 
        {
            instance = this;





            Object.DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void ExitGame() 
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }

    // Pausar/despausar el juego
    public bool IsGamePaused() 
    {
        return isGamePaused;
    }

    public void PauseGame() 
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        isGamePaused = pauseMenu.activeSelf;
        Cursor.visible = isGamePaused;
    }
}
