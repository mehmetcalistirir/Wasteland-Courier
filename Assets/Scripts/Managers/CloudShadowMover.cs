using UnityEngine;

public class CloudShadowMover : MonoBehaviour
{
    public float speed = 1f;
    public float resetX = -20f;
    public float startX = 20f;

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        if (transform.position.x < resetX)
        {
            float randomY = Random.Range(2f, 5f);
            transform.position = new Vector3(startX, randomY, transform.position.z);
        }
    }
}
