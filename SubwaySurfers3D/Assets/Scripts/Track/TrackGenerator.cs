using System.Collections.Generic;
using UnityEngine;
using MetroRunner.Core;

namespace MetroRunner.Track
{
    public class TrackGenerator : MonoBehaviour
    {
        [Header("Chunk Configurations")]
        [SerializeField] private float chunkLength = 30f;
        [SerializeField] private int initialChunkCount = 10;
        [SerializeField] private List<string> chunkPoolKeys = new List<string> { "Track_City", "Track_Subway", "Track_Bridge", "Track_Tunnel" };

        private Queue<GameObject> activeChunks = new Queue<GameObject>();
        private float spawnZ = 0f;
        private Transform playerTransform;

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;

            for (int i = 0; i < initialChunkCount; i++)
            {
                SpawnChunk();
            }
        }

        private void Update()
        {
            if (playerTransform == null) return;

            // Spawn next chunk when player moves forward
            if (playerTransform.position.z + (initialChunkCount * chunkLength / 2f) > spawnZ)
            {
                SpawnChunk();
                RecycleOldestChunk();
            }
        }

        private void SpawnChunk()
        {
            string key = chunkPoolKeys[Random.Range(0, chunkPoolKeys.Count)];
            Vector3 spawnPos = new Vector3(0, 0, spawnZ);

            GameObject chunk = ObjectPooler.Instance.SpawnFromPool(key, spawnPos, Quaternion.identity);
            if (chunk != null)
            {
                activeChunks.Enqueue(chunk);
                spawnZ += chunkLength;
            }
        }

        private void RecycleOldestChunk()
        {
            if (activeChunks.Count > 0)
            {
                GameObject oldChunk = activeChunks.Dequeue();
                ObjectPooler.Instance.ReturnToPool("Track_Chunk", oldChunk);
            }
        }
    }
}
