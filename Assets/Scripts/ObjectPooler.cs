using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class PoolItem
    {
        public GameObject objectToPool;
        public int maxAmountToPool;
    }
    
    public List<PoolItem> itemsToPool;
    public List<GameObject> pooledXs;
    public List<GameObject> pooledOs;

    
    private void Awake()
    {
        pooledXs = new List<GameObject>();
        for (int i = 0; i < itemsToPool.Count ; i++)
        {
            for (int j = 0; j < itemsToPool[i].maxAmountToPool; j++)
            {
                GameObject obj = Instantiate(itemsToPool[i].objectToPool, transform, true);
                switch (i)
                {
                    case 0:
                        pooledXs.Add(obj);
                        break;
                    case 1:
                        pooledOs.Add(obj);
                        break;
                }
                obj.SetActive(false);
            }
        }
    }
    
    public GameObject GetPooledObject(List<GameObject> pooledObjects)
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        return null;
    }

    public void ResetPool()
    {
        foreach (var pooledO in pooledOs)
        {
            pooledO.SetActive(false);
        }
        
        foreach (var pooledX in pooledXs)
        {
            pooledX.SetActive(false);
        }
    }
}