using UnityEngine;

public class Player : MonoBehaviour
{
    private GameManager gameManager;
    private BoxCollider2D col;
    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        col = GetComponent<BoxCollider2D>();
    }
    void FixedUpdate()
    {
        Vector2 worldSize = Vector2.Scale(col.size, transform.lossyScale);
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, worldSize, 0f);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log("Death");
                gameManager.GameOver();
            }
            else if(hit.CompareTag("Finish"))
            {
                gameManager.Victory();
            }
        }
    }
}
