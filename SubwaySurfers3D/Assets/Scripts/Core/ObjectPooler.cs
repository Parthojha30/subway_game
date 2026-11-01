using System.Collections.Generic;
using UnityEngine;

namespace MetroRunner.Core
{
    [System.Serializable]
    public class PoolItem
    {
        public string poolKey;
        public GameObject prefab;
        public int initialSize = 10;
        public bool expandable = true;
    }

    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        [SerializeField] private List<PoolItem> poolItems = new List<PoolItem>();
        private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitializePools();
        }

        private void InitializePools()
        {
            foreach (var item in poolItems)
            {
                Queue<GameObject> objectQueue = new Queue<GameObject>();

                for (int i = 0; i < item.initialSize; i++)
                {
                    GameObject obj = Instantiate(item.prefab, transform);
                    obj.SetActive(false);
                    objectQueue.Enqueue(obj);
                }

                poolDictionary.Add(item.poolKey, objectQueue);
            }
        }

        public GameObject SpawnFromPool(string poolKey, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(poolKey))
            {
                Debug.LogWarning($"Pool with key '{poolKey}' does not exist!");
                return null;
            }

            Queue<GameObject> objectQueue = poolDictionary[poolKey];

            if (objectQueue.Count == 0)
            {
                PoolItem item = poolItems.Find(x => x.poolKey == poolKey);
                if (item != null && item.expandable)
                {
                    GameObject newObj = Instantiate(item.prefab, transform);
                    newObj.SetActive(false);
                    objectQueue.Enqueue(newObj);
                }
                else
                {
                    return null;
                }
            }

            GameObject objToSpawn = objectQueue.Dequeue();
            objToSpawn.transform.position = position;
            objToSpawn.transform.rotation = rotation;
            objToSpawn.SetActive(true);

            return objToSpawn;
        }

        public void ReturnToPool(string poolKey, GameObject obj)
        {
            obj.SetActive(false);
            if (poolDictionary.ContainsKey(poolKey))
            {
                poolDictionary[poolKey].Enqueue(obj);
            }
        }
    }
}
