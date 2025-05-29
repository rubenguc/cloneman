using Unity.Cinemachine;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private AudioClip checkpoint;
    private Health playerHealth;
    [SerializeField] private CinemachineCamera virtualCamera;

    // private UIManager uiManager;

    private void Awake()
    {

        playerHealth = GetComponent<Health>();
        // uiManager = FindObjectOfType<UIManager>();
    }

    public void RespawnCheck()
    {
        // if (currentCheckpoint == null) 
        // {
        //     // uiManager.GameOver();
        //     return;
        // }

        playerHealth.Respawn(); //Restore player health and reset animation
        transform.position = Vector3.zero; //Move player to checkpoint location
        Debug.Log("Player respawned at checkpoint: " + transform.position);

        if (virtualCamera != null)
        {
            // Reset the virtual camera position (adjust the position as needed)
            virtualCamera.transform.position = Vector3.zero;
        }
    }
}