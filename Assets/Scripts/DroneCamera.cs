using UnityEngine;

public class DroneCamera : MonoBehaviour
{
    [Header("Настройки камеры")]
    public Transform drone;
    public Vector3 cameraOffset = new Vector3(0, 1.5f, -4f);
    public float followSpeed = 6f;
    public float rotationSpeed = 4f;
    
    [Header("Стабильность")]
    public bool enableCameraShake = true;
    public float shakeIntensity = 0.1f;
    
    private Vector3 currentVelocity;
    private Quaternion droneInitialRotation;
    
    void Start()
    {
        if (drone == null)
        {
            Debug.LogError("Drone not assigned to DroneCamera!");
            return;
        }
        
        // Запоминаем начальное вращение дрона
        droneInitialRotation = drone.rotation;
        
        // Устанавливаем начальную позицию
        transform.position = drone.position + cameraOffset;
    }
    
    void LateUpdate()
    {
        if (drone == null) return;
        
        FollowDrone();
        LookAtDrone();
    }
    
    private void FollowDrone()
    {
        // Целевая позиция камеры
        Vector3 targetPosition = drone.position + 
                               drone.forward * cameraOffset.z + 
                               drone.up * cameraOffset.y + 
                               drone.right * cameraOffset.x;
        
        // Плавное следование
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, 
                                              ref currentVelocity, 1f / followSpeed);
    }
    
    private void LookAtDrone()
    {
        // Смотрим на точку немного впереди дрона для лучшего обзора
        Vector3 lookTarget = drone.position + drone.forward * 2f + Vector3.up * 0.5f;
        
        // Плавный поворот камеры
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                                            rotationSpeed * Time.deltaTime);
    }
}