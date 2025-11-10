using UnityEngine;

public class DeliveryPoint : MonoBehaviour
{
    [Header("Настройки точки доставки")]
    public string pointName = "Склад А";
    public int rewardMultiplier = 1;
    public float deliveryRange = 3f;
    
    [Header("Визуальные элементы")]
    public ParticleSystem deliveryEffect;
    public Light pointLight;
    
    private bool isActive = true;
    
    void Start()
    {
        if (pointLight != null)
        {
            pointLight.color = Color.green;
        }
    }
    
    void Update()
    {
        if (isActive)
        {
            CheckForDroneInRange();
        }
    }
    
    private void CheckForDroneInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, deliveryRange);
        
        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Player"))
            {
                DeliverySystem droneDelivery = collider.GetComponent<DeliverySystem>();
                if (droneDelivery != null && droneDelivery.CanDeliver())
                {
                    TryDeliver(droneDelivery);
                }
            }
        }
    }
    
    private void TryDeliver(DeliverySystem droneDelivery)
    {
        CargoItem cargo = droneDelivery.GetCurrentCargo();
        if (cargo != null)
        {
            CompleteDelivery(cargo);
            droneDelivery.CompleteDelivery();
        }
    }
    
    public void CompleteDelivery(CargoItem cargo)
    {
        if (!isActive) return;
        
        int finalReward = cargo.reward * rewardMultiplier;
        
        // Эффекты
        if (deliveryEffect != null)
            Instantiate(deliveryEffect, transform.position, Quaternion.identity);
            
        if (pointLight != null)
            pointLight.color = Color.blue;
        
        Debug.Log($"Доставка в {pointName} завершена! +{finalReward} кредитов");
        
        // Доставляем груз
        cargo.Deliver();
        
        // Временно деактивируем точку
        isActive = false;
        Invoke("Reactivate", 3f);
    }
    
    private void Reactivate()
    {
        isActive = true;
        if (pointLight != null)
            pointLight.color = Color.green;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, deliveryRange);
    }
}