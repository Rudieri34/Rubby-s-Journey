using InnominatumDigital.Base;
using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    public bool IsGamePaused = false;
    public bool IsMovementAllowed = false;

    public bool TogglePause()
    {
        IsGamePaused = !IsGamePaused;
        if (IsGamePaused)
        {
            Time.timeScale = 0f;
            IsMovementAllowed = false;
        }
        else
        {
            Time.timeScale = 1f;
            IsMovementAllowed = true;
        }
        return IsGamePaused;
    }

    public void SetMovementAllowed(bool allowed)
    {
        IsMovementAllowed = allowed;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            ReloadScene();
        }
    }


    public void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
