using System.Collections.Generic;
using UnityEngine;

public class RefreshItemSelectionScript : MonoBehaviour
{
    [SerializeField] List<Transform> spawnPoints = new();
    [SerializeField] List<BuyScript> buyScripts = new();
    [SerializeField] GameObject itemPrefab;

    void Start()
    {
        SpawnItems();
    }

    public void SpawnItems()
    {
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            buyScripts[i].DeleteItemBeingSold();
            GameObject newItem = Instantiate(itemPrefab);
            // spawnPoints[i].localPosition = Vector3.zero;
            newItem.transform.position = spawnPoints[i].position;
            buyScripts[i].SetItemBeingSold(newItem);
        }
    }
}
