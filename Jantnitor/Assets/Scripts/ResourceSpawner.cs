using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    private float timer = 0f;
    [SerializeField] private float resourceSpawnCD = 1f;
    private BoxCollider spawnZone;
    [SerializeField] GameObject[] resources;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnZone = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < resourceSpawnCD)
        {
            timer += Time.deltaTime;
        }
        else
        {
            spawnResource();
            timer = 0f;
        }
    }

    void spawnResource()
    {
        Vector3 position = new Vector3(Random.Range(spawnZone.bounds.min.x,spawnZone.bounds.max.x), transform.position.y, Random.Range(spawnZone.bounds.min.z, spawnZone.bounds.max.z));
        int index = Random.Range(0,resources.Length);

        GameObject resource = Instantiate(resources[index], position, Quaternion.identity, this.transform);
    }
}
