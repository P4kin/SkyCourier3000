using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("Скорости движения")]
    public float moveSpeed = 8f;
    public float ascendSpeed = 5f;
    public float descendSpeed = 5f;
    public float rotationSpeed = 90f;

    [Header("Стабилизация")]
    public float hoverForce = 15f;
    public float stabilizationForce = 10f;

    [Header("Визуальные наклоны")]
    public float maxTiltAngle = 15f;
    public float tiltSmoothness = 8f;

    [Header("Влияние груза")]
    public float cargoWeightEffect = 0.3f;

    private Rigidbody rb;
    private Vector3 movementInput;
    private float liftInput;
    private float rotationInput;

    // Визуальная модель для наклонов
    private Transform visualModel;
    private float currentTiltX;
    private float currentTiltZ;

    // Целевая высота для автопарения
    private float targetHeight;

    // Для плавного вращения
    private float targetYaw;
    private float currentYaw;
    private float yawVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.drag = 1.5f;
        rb.angularDrag = 4f;
        
        targetHeight = transform.position.y;
        
        // Ищем визуальную модель (должна быть дочерней)
        visualModel = transform.Find("Visual");
        if (visualModel == null)
        {
            // Если нет отдельной визуальной модели, создаем её
            GameObject visualGO = new GameObject("Visual");
            visualModel = visualGO.transform;
            visualModel.SetParent(transform);
            visualModel.localPosition = Vector3.zero;
            visualModel.localRotation = Quaternion.identity;
            
            // Перемещаем все меши в визуальную модель
            MoveAllMeshesToVisual(visualModel);
        }

        targetYaw = transform.eulerAngles.y;
        currentYaw = targetYaw;
    }

    void Update()
    {
        GetInput();
        HandleRotation();
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyHeightControl();
        ApplyVisualTilt(); // ДОБАВЛЕНО применение наклонов
        ApplyStabilization();
    }

    private void GetInput()
    {
        movementInput = new Vector3(
            Input.GetAxis("Horizontal"),
            0f,
            Input.GetAxis("Vertical")
        );

        liftInput = 0f;
        if (Input.GetKey(KeyCode.Space)) liftInput += 1f;
        if (Input.GetKey(KeyCode.LeftControl)) liftInput -= 1f;

        rotationInput = 0f;
        if (Input.GetKey(KeyCode.Q)) rotationInput -= 1f;
        if (Input.GetKey(KeyCode.E)) rotationInput += 1f;
    }

    private void HandleRotation()
    {
        if (Mathf.Abs(rotationInput) > 0.1f)
        {
            targetYaw += rotationInput * rotationSpeed * Time.deltaTime;
        }

        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, 0.1f);
        transform.rotation = Quaternion.Euler(0, currentYaw, 0);
    }

    private void ApplyMovement()
    {
        if (movementInput.magnitude > 0.1f)
        {
            Vector3 localMovement = transform.forward * movementInput.z + 
                                   transform.right * movementInput.x;
            
            float speedMultiplier = GetCargoSpeedMultiplier();
            Vector3 moveForce = localMovement * moveSpeed * speedMultiplier;
            rb.AddForce(moveForce, ForceMode.Force);
        }
    }

    private void ApplyHeightControl()
    {
        if (liftInput > 0.1f)
        {
            targetHeight += ascendSpeed * Time.deltaTime;
        }
        else if (liftInput < -0.1f)
        {
            targetHeight -= descendSpeed * Time.deltaTime;
        }

        float currentHeight = transform.position.y;
        float heightDifference = targetHeight - currentHeight;
        float hoverPower = heightDifference * hoverForce;
        rb.AddForce(Vector3.up * hoverPower, ForceMode.Force);
    }

    private void ApplyVisualTilt()
    {
        // ВЫЧИСЛЯЕМ наклоны на основе ВВОДА (а не скорости)
        float targetTiltX = movementInput.z * maxTiltAngle;
        float targetTiltZ = -movementInput.x * maxTiltAngle;
        
        // Плавная интерполяция
        currentTiltX = Mathf.Lerp(currentTiltX, targetTiltX, tiltSmoothness * Time.deltaTime);
        currentTiltZ = Mathf.Lerp(currentTiltZ, targetTiltZ, tiltSmoothness * Time.deltaTime);
        
        // ПРИМЕНЯЕМ наклоны к визуальной модели
        if (visualModel != null)
        {
            visualModel.localRotation = Quaternion.Euler(currentTiltX, 0, currentTiltZ);
        }
    }

    private void ApplyStabilization()
    {
        Vector3 angularVel = rb.angularVelocity;
        Vector3 stabilizedAngularVel = new Vector3(
            Mathf.Lerp(angularVel.x, 0, 6f * Time.deltaTime),
            angularVel.y,
            Mathf.Lerp(angularVel.z, 0, 6f * Time.deltaTime)
        );
        rb.angularVelocity = stabilizedAngularVel;

        Vector3 currentEuler = transform.eulerAngles;
        Quaternion targetRot = Quaternion.Euler(0, currentEuler.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 4f * Time.deltaTime);
    }

    private float GetCargoSpeedMultiplier()
    {
        CargoSystem cargoSystem = GetComponent<CargoSystem>();
        if (cargoSystem != null)
        {
            return cargoSystem.GetSpeedMultiplier();
        }
        return 1f - cargoWeightEffect;
    }

    // Вспомогательный метод для перемещения мешей
    private void MoveAllMeshesToVisual(Transform visualParent)
    {
        // Находим все меши и перемещаем их в визуальную модель
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.transform != visualParent && renderer.transform != transform)
            {
                renderer.transform.SetParent(visualParent);
            }
        }
    }

    // Для отладки
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Высота: {transform.position.y:F1}m");
        GUI.Label(new Rect(10, 30, 300, 20), $"Наклоны: X:{currentTiltX:F1}° Z:{currentTiltZ:F1}°");
        GUI.Label(new Rect(10, 50, 300, 20), $"Ввод: {movementInput}");
    }
}