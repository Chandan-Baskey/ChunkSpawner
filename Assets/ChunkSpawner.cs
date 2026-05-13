using System.Collections.Generic;
using UnityEngine;

public class ChunkSpawner : MonoBehaviour
{
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] GameObject chunkParent;
    [SerializeField] int chunkAmount = 10;
    [SerializeField] int chunkLength = 10;
    [SerializeField] float moveSpeed = 30f;
    List<GameObject> chunk = new List<GameObject>();

    void Start()
    {
        spawnChunk();

    }
    void Update()
    {
        MoveChunk();
    }

    private void spawnChunk()
    {
        for (int i = 0; i < chunkAmount; i++)
        {
            float chunkZ; // Variable to hold the Z position of the chunk   
            chunkZ = transform.position.z + (chunkLength * i); // Subsequent chunks placed after the previous one

            Vector3 chunkPos = new Vector3(transform.position.x, transform.position.y, chunkZ);
            GameObject newChunk = Instantiate(chunkPrefab, chunkPos, Quaternion.identity, chunkParent.transform);
            chunk.Add(newChunk);
        }
    }

    private void NewSpawnChunk()
    {
        float chunkZ; // Variable to hold the Z position of the chunk   
        chunkZ = chunk[chunk.Count - 1].transform.position.z + chunkLength; // Place new chunk after the last one in the list
        Vector3 chunkPos = new Vector3(transform.position.x, transform.position.y, chunkZ);
        GameObject newChunk = Instantiate(chunkPrefab, chunkPos, Quaternion.identity, chunkParent.transform);
        chunk.Add(newChunk);
    }

    private void MoveChunk()
    {
        for (int i = 0; i < chunk.Count; i++)
        {
            //chunk[i].transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
            chunk[i].transform.Translate(-transform.forward * moveSpeed * Time.deltaTime);

            if (chunk[i].transform.position.z <= transform.position.z - chunkLength)
            {
                Destroy(chunk[i]);
                chunk.RemoveAt(i);
                NewSpawnChunk();
            }
        }
    }
}
