using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private BoxCollider2D box;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
    }
  
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            collider.GetComponent<Health>().InstantDie();
        }
    }
}
