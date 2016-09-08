using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuBehaviour : MonoBehaviour
{

    private void StartGame(Difficulty difficulty)
    {
        Game.SetDifficulty(difficulty);
        Application.LoadLevel(Scene.Game);
    }

    public void ClickVeryEasy()
    {
        StartGame(Difficulty.VeryEasy);
    }

    public void ClickEasy()
    {
        StartGame(Difficulty.Easy);
    }

    public void ClickMedium()
    {
        StartGame(Difficulty.Medium);
    }

    public void ClickHard()
    {
        StartGame(Difficulty.Hard);
    }
}