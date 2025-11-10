using UnityEngine;

public class CargoItem : MonoBehaviour
{
    [Header("Параметры груза")]
    public string cargoName = "Посылка";
    public float weight = 2f;
    public int reward = 50;
    public bool isFragile = false;
    
    private bool isPickedUp = false;
    private BoxCollider triggerCollider;
    
    void Start()
    {
        SetupColliders();
        gameObject.tag = "Cargo";
    }
    
    private void SetupColliders()
    {
        // Физический коллайдер
        BoxCollider physicsCollider = GetComponent<BoxCollider>();
        if (physicsCollider == null)
        {
            physicsCollider = gameObject.AddComponent<BoxCollider>();
        }
        physicsCollider.isTrigger = false;
        
        // Триггер для подбора (больше модели)
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
            {
                TryPickup(cargoSystem);
            }
        }
    }
    
    public void TryPickup(CargoSystem droneCargoSystem)
    {
        if (droneCargoSystem.AttachCargo(gameObject, weight))
        {
            isPickedUp = true;
            
            // Отключаем триггерный коллайдер
            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }
        }
    }
    
    public void Deliver()
    {
        Debug.Log($"Доставлен груз: {cargoName} +{reward} кредитов");
        Destroy(gameObject);
    }
}