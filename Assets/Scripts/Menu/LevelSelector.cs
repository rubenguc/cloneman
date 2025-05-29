using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public void SelectLevel(int levelIndex)
    {
        // Assuming you have a method to load levels by index
        SceneManager.LoadSceneAsync(levelIndex);
    }
}
