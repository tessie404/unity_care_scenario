using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bound to a UI button: reloads the active scene so another decision sequence can be played.
/// Different handle orders produce different simulated time and TotalLoss / Regret — use reruns to contrast “different people / different choices.”
/// </summary>
public class RestartGame : MonoBehaviour
{
    public void Restart()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}