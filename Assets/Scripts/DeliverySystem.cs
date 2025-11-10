using UnityEngine;

public class DeliverySystem : MonoBehaviour
{
    [Header("Система доставки")]
    public float deliveryRange = 3f;
    
    [Header("Статистика")]
    public int totalDeliveries = 0;
    public int totalCredits = 0;
    
    private CargoSystem cargoSystem;
    private DeliveryPoint currentDeliveryPoint;
    
    void Start()
    {
        cargoSystem = GetComponent<CargoSystem>();
    }
    
    void Update()
    {
        FindNearestDeliveryPoint();
        
        // Ручная доставка по кнопке F
        if (Input.GetKeyDown(KeyCode.F) && currentDeliveryPoint != null)
        {
            TryDeliverToPoint(currentDeliveryPoint);
        }
    }
    
    private void FindNearestDeliveryPoint()
    {
        DeliveryPoint[] allPoints = FindObjectsOfType<DeliveryPoint>();
        DeliveryPoint nearestPoint = null;
        float nearestDistance = float.MaxValue;
        
        foreach (DeliveryPoint point in allPoints)
        {
            float distance = Vector3.Distance(transform.position, point.transform.position);
            if (distance < deliveryRange && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPoint = point;
            }
        }
        
        currentDeliveryPoint = nearestPoint;
    }
    
    public void TryDeliverToPoint(DeliveryPoint deliveryPoint)
    {
        if (deliveryPoint == null || !cargoSystem.HasCargo()) return;
        
        CargoItem cargo = cargoSystem.GetCargoItem();
        if (cargo != null)
        {
            deliveryPoint.CompleteDelivery(cargo);
            CompleteDelivery();
        }
    }
    
    public void CompleteDelivery()
    {
        if (cargoSystem.HasCargo())
        {
            CargoItem cargo = cargoSystem.GetCargoItem();
            if (cargo != null)
            {
                totalCredits += cargo.reward;
            }
            
            cargoSystem.DetachCargo();
            totalDeliveries++;
            
            Debug.Log($"Всего доставок: {totalDeliveries}, Кредитов: {totalCredits}");
        }
    }
    
    public bool CanDeliver()
    {
        return cargoSystem.HasCargo();
    }
    
    public CargoItem GetCurrentCargo()
    {
        return cargoSystem.GetCargoItem();
    }
}