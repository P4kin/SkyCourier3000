using UnityEngine;

public class DeliveryPoint : MonoBehaviour

{
    [Header("Настройки точки доставки")]
    public string pointName = "Склад А";
    public string pointId;
    public int rewardMultiplier = 1;
    public float deliveryRange = 3f;

    [Header("Звуки")]
public AudioClip deliverySound;   // звук пилинька при доставке
private AudioSource audioSource;

    [Header("Визуальные элементы")]
    public ParticleSystem deliveryEffect;
    public Light pointLight;
    public GameObject markerPrefab; // 3D-маркер
    private GameObject markerInstance;

    [Header("Маркер появления")]
    public float markerDistanceThreshold = 50f;

    private bool isActive = true;
    private Transform droneTransform;

    void Start()
    {
        if (pointLight != null)
            pointLight.color = Color.green;

        if (markerPrefab != null)
        {
            markerInstance = Instantiate(markerPrefab, transform.position, Quaternion.identity);
            markerInstance.SetActive(false);
        }
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (droneTransform == null)
        {
            GameObject drone = GameObject.FindGameObjectWithTag("Player");
            if (drone != null)
                droneTransform = drone.transform;
        }

        if (droneTransform != null)
            HandleMarker();

        if (isActive)
            CheckForDroneInRange();
    }

    private void HandleMarker()
    {
        float distance = Vector3.Distance(transform.position, droneTransform.position);
        if (markerInstance != null)
            markerInstance.SetActive(distance <= markerDistanceThreshold);
    }

    private void CheckForDroneInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, deliveryRange);

        foreach (Collider collider in hitColliders)
        {
            if (!collider.CompareTag("Player")) continue;

            DeliverySystem droneDelivery = collider.GetComponent<DeliverySystem>();
            if (droneDelivery != null && droneDelivery.CanDeliver())
            {
                TryDeliver(droneDelivery);
            }
        }
    }

    private void TryDeliver(DeliverySystem droneDelivery)
    {
        CargoItem cargo = droneDelivery.GetCurrentCargo();
        if (cargo == null) return;

        // ❗ КЛЮЧЕВАЯ ПРОВЕРКА
        if (cargo.targetPointId != pointId)
        {
            Debug.Log(
                $"❌ Неверная точка доставки! " +
                $"Груз → {cargo.targetPointId}, эта точка → {pointId}"
            );
            return;
        }

        CompleteDelivery(cargo);
        droneDelivery.CompleteDelivery();
    }

    public void CompleteDelivery(CargoItem cargo)
    {
        if (!isActive) return;

        int finalReward = cargo.reward * rewardMultiplier;

        if (deliveryEffect != null)
            Instantiate(deliveryEffect, transform.position, Quaternion.identity);

        if (pointLight != null)
            pointLight.color = Color.blue;
        if (deliverySound != null && audioSource != null)
        audioSource.PlayOneShot(deliverySound);

        Debug.Log($"✅ Доставка в {pointName} завершена! +{finalReward} кредитов");

        DroneUIManager uiManager = FindObjectOfType<DroneUIManager>();
        if (uiManager != null)
            uiManager.SpawnFloatingPoints(finalReward, transform.position);

        cargo.Deliver();

        isActive = false;
        Invoke(nameof(Reactivate), 3f);
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