using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    public GameObject grassPrefab;
    public int rows = 200;
    public int columns = 200;
    public float spacing = 1.0f;
    public Vector3 offset = new Vector3(-99.5f, 0.1f, -99.5f);

    void Start()
    {
        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 position = new Vector3(x * spacing, 0f, z * spacing) + offset;
                GameObject grass = Instantiate(grassPrefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                grass.transform.parent = this.transform;
            }
        }
    }
}
