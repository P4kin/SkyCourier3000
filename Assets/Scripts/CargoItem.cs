using UnityEngine;

public class CargoItem : MonoBehaviour
{
    [Header("Параметры груза")]
    public string cargoName;
    public float weight;
    public int reward;
    public bool isFragile;

    [Header("Назначенная точка доставки")]
    [HideInInspector]
    public string targetPointId;

    private bool isPickedUp = false;
    private BoxCollider triggerCollider;

    // 🔹 Пулы данных
    private static readonly string[] cargoNames =
    {
        "Медикаменты", "Электроника", "Запчасти",
        "Продукты", "Документы", "Оборудование"
    };

    void Start()
    {
        GenerateCargoData();
        SetupColliders();
        AssignRandomDeliveryPoint();
        gameObject.tag = "Cargo";
    }

    // -----------------------------
    //  Генерация уникального груза
    // -----------------------------
    private void GenerateCargoData()
    {
        cargoName = cargoNames[Random.Range(0, cargoNames.Length)];
        weight = Random.Range(1.5f, 6f);
        reward = Mathf.RoundToInt(weight * Random.Range(25f, 40f));
        isFragile = Random.value < 0.25f;

        if (isFragile)
            reward += 30;
    }

    private void AssignRandomDeliveryPoint()
    {
        DeliveryPoint[] points = FindObjectsOfType<DeliveryPoint>();
        if (points.Length == 0)
        {
            Debug.LogWarning("Нет DeliveryPoint на сцене!");
            targetPointId = "";
            return;
        }

        DeliveryPoint selected = points[Random.Range(0, points.Length)];
        targetPointId = selected.pointId;
    }

    private void SetupColliders()
    {
        BoxCollider physicsCollider = GetComponent<BoxCollider>();
        if (physicsCollider == null)
            physicsCollider = gameObject.AddComponent<BoxCollider>();

        physicsCollider.isTrigger = false;

        triggerCollider = gameObject.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = Vector3.one * 1.5f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isPickedUp && other.CompareTag("Player"))
        {
            CargoSystem cargoSystem = other.GetComponent<CargoSystem>();
            if (cargoSystem != null)
                TryPickup(cargoSystem);
        }
    }

    public void TryPickup(CargoSystem droneCargoSystem)
    {
        if (droneCargoSystem.AttachCargo(gameObject, weight))
        {
            isPickedUp = true;
            if (triggerCollider != null)
                triggerCollider.enabled = false;
        }
    }

    public void Deliver()
    {
        CargoSpawner spawner = FindObjectOfType<CargoSpawner>();
        if (spawner != null)
            spawner.NotifyCargoRemoved(gameObject);

        Destroy(gameObject);
    }
}