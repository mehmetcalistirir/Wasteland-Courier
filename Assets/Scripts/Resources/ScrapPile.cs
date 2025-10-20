using UnityEngine;

public class ScrapPile : MonoBehaviour
{
    public GameObject[] partPrefabs; // Inspector'a ekleyeceğiz

    public void OnCollected()
    {
        if (Random.value < 0.2f)
        {
            int index = Random.Range(0, partPrefabs.Length);
            Instantiate(partPrefabs[index], transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Debug.Log("🔧 Silah parçası düştü!");
        }

        Destroy(gameObject);
    }
}
