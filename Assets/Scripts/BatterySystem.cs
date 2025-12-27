using UnityEngine;

public class BatterySystem : MonoBehaviour
{
    [Header("Система батареи")]
    public float maxBattery = 2500f;           
    public float baseDrainRate = 1f;          
    public float movementDrainRate = 0.5f;    
    public float liftDrainRate = 0.8f;        
    public float lowBatteryThreshold = 100f;  
    
    private float currentBattery;
    private DroneController droneController;
    private Rigidbody droneRb;
    public GameOverUI gameOverUI;
    
    void Start()
    {
        currentBattery = maxBattery;
        droneController = GetComponent<DroneController>();
        droneRb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        if (droneController == null || droneRb == null) return;
        
        CalculateBatteryDrain();
        
        if (currentBattery <= 0)
        {
            // Дрон теряет мощность
            droneController.enabled = false;
            Debug.Log("Battery EMPTY! Drone disabled.");
            if (droneRb != null)
            droneRb.useGravity = true;
            ShowGameOver();
        }
        else if (currentBattery < lowBatteryThreshold)
        {
            // Включить предупреждение
            Debug.Log("Low Battery! " + currentBattery.ToString("F1") + " units left");
        }
    }

   private void ShowGameOver()
{
    if (gameOverUI != null)
    {
        DeliverySystem ds = GetComponent<DeliverySystem>();
        gameOverUI.Show(ds != null ? ds.totalCredits : 0);
    }
}
    
    private void CalculateBatteryDrain()
    {
        float drainAmount = 0f;
        
        // 1. Базовый расход (даже когда дрон просто парит)
        drainAmount += baseDrainRate * Time.deltaTime;
        
        // 2. Расход от движения (горизонтальная скорость)
        Vector3 horizontalVelocity = new Vector3(droneRb.velocity.x, 0, droneRb.velocity.z);
        float movementDrain = horizontalVelocity.magnitude * movementDrainRate * Time.deltaTime;
        drainAmount += movementDrain;
        
        // 3. Расход от подъема/спуска (вертикальная скорость)
        if (droneRb.velocity.y > 0.1f) // Поднимается - больше расход
        {
            float liftDrain = droneRb.velocity.y * liftDrainRate * Time.deltaTime;
            drainAmount += liftDrain;
        }
        else if (droneRb.velocity.y < -0.1f) // Спускается - меньше расход
        {
            float descentDrain = Mathf.Abs(droneRb.velocity.y) * liftDrainRate * 0.3f * Time.deltaTime;
            drainAmount += descentDrain;
        }
        
        currentBattery -= drainAmount;
        currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);
    }
    
    public void RechargeBattery(float amount)
    {
        currentBattery += amount;
        currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);
        Debug.Log($"Battery recharged! Current: {currentBattery}/{maxBattery}");
    }
    
    public float GetBatteryPercentage()
    {
        return (currentBattery / maxBattery) * 100f;
    }
    
    public bool IsBatteryLow()
    {
        return currentBattery < lowBatteryThreshold;
    }
    
    // Для UI
    public float GetCurrentBattery() => currentBattery;
    public float GetMaxBattery() => maxBattery;
}