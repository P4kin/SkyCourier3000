using UnityEngine;
using System.Collections.Generic;

public class CargoSpawner : MonoBehaviour
{
    public GameObject cargoPrefab;
    public int maxCargoOnMap = 5;

    // ТВОЙ стиль задания зоны спавна ↓
    public Vector3 spawnAreaMin = new Vector3(-10, 1, -10);
    public Vector3 spawnAreaMax = new Vector3(10, 1, 10);

    private List<GameObject> activeCargos = new List<GameObject>();

    void Start()
    {
        SpawnMissing();
    }

    void Update()
    {
        // чистим список от удалённых объектов
        if (activeCargos.Contains(null))
            activeCargos.RemoveAll(c => c == null);

        // спавним недостающие
        SpawnMissing();
    }

    void SpawnMissing()
    {
        while (activeCargos.Count < maxCargoOnMap)
        {
            Vector3 pos = new Vector3(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                spawnAreaMin.y,
                Random.Range(spawnAreaMin.z, spawnAreaMax.z)
            );

            GameObject cargo = Instantiate(cargoPrefab, pos, Quaternion.identity);
            activeCargos.Add(cargo);
        }
    }

    // вызывается грузом при доставке
    public void NotifyCargoRemoved(GameObject cargo)
    {
        activeCargos.Remove(cargo);
    }
}
