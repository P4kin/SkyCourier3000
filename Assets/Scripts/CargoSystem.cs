using UnityEngine;

public class CargoSystem : MonoBehaviour
{
    [Header("Система груза")]
    public Transform cargoAnchor;
    public float maxCargoWeight = 10f;
    public float speedMultiplier = 0.7f;

    [Header("Текущий груз")]
    public GameObject currentCargo;
    public float currentCargoWeight;

    private Rigidbody cargoRb;

    void Start()
    {
        if (cargoAnchor == null)
        {
            cargoAnchor = transform.Find("CargoAnchor");
            if (cargoAnchor == null)
            {
                GameObject anchor = new GameObject("CargoAnchor");
                cargoAnchor = anchor.transform;
                cargoAnchor.SetParent(transform);
                cargoAnchor.localPosition = new Vector3(0, -0.5f, 0);
            }
        }
    }

    public bool HasCargo()
    {
        return currentCargo != null;
    }

    public bool AttachCargo(GameObject cargo, float weight)
    {
        if (currentCargoWeight + weight > maxCargoWeight)
        {
            Debug.Log("Слишком тяжелый груз!");
            return false;
        }

        if (currentCargo != null)
        {
            Debug.Log("Уже несем другой груз!");
            return false;
        }

        // Присоединяем груз
        currentCargo = cargo;
        currentCargoWeight = weight;

        cargo.transform.SetParent(cargoAnchor);
        cargo.transform.localPosition = Vector3.zero;
        cargo.transform.localRotation = Quaternion.identity;

        cargoRb = cargo.GetComponent<Rigidbody>();
        if (cargoRb != null)
        {
            cargoRb.isKinematic = true;
        }

        Debug.Log($"Подобран груз: {cargo.name} ({weight}кг)");
        return true;
    }

    public void DetachCargo()
    {
        if (currentCargo != null)
        {
            // Включаем физику
            if (cargoRb != null)
            {
                cargoRb.isKinematic = false;
            }

            // Отсоединяем от дрона
            currentCargo.transform.SetParent(null);

            currentCargo = null;
            currentCargoWeight = 0f;
            cargoRb = null;

            Debug.Log("Груз отсоединен");
        }
    }

    public float GetSpeedMultiplier()
    {
        return currentCargoWeight <= 0 ? 1f : speedMultiplier;
    }

    public GameObject GetCargoObject()
    {
        return currentCargo;
    }

    public CargoItem GetCargoItem()
    {
        return currentCargo != null ? currentCargo.GetComponent<CargoItem>() : null;
    }
}
