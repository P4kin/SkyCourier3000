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

    [Header("Визуальные наклоны")]
    public float maxTiltAngle = 15f;
    public float tiltSmoothness = 8f;

    [Header("Влияние груза")]
    public float cargoWeightEffect = 0.3f;

    [Header("Столкновения")]
    public float collisionPushForce = 1f;   // основной коэффициент
    public float collisionAngularKick = 2f; // вращение при ударе
    public float stunDuration = 0.5f;       // длительность стана
    public float verticalImpulse = 2f;      // вертикальный импульс
    public float minCollisionSpeed = 1f;    // минимальная скорость для срабатывания
    public float minPushForce = 2f;         // минимальная сила отскока
    public float maxPushForce = 6f;         // максимальная сила отскока

    private Rigidbody rb;
    private Transform visualModel;

    private Vector3 movementInput;
    private float liftInput;
    private float rotationInput;

    private float targetYaw;
    private float currentYaw;
    private float yawVelocity;

    private float currentTiltX;
    private float currentTiltZ;

    private float targetHeight;

    private bool isStunned = false;
    private float stunTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.drag = 1.5f;
        rb.angularDrag = 4f;

        targetHeight = transform.position.y;

        visualModel = transform.Find("Visual");
        if (visualModel == null)
        {
            GameObject visualGO = new GameObject("Visual");
            visualModel = visualGO.transform;
            visualModel.SetParent(transform);
            visualModel.localPosition = Vector3.zero;
            visualModel.localRotation = Quaternion.identity;
            MoveAllMeshesToVisual(visualModel);
        }

        targetYaw = transform.eulerAngles.y;
        currentYaw = targetYaw;
    }

    void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
                isStunned = false;
        }

        GetInput();
        HandleRotation();
    }

    void FixedUpdate()
    {
        if (!isStunned)
        {
            ApplyMovement();
            ApplyHeightControl();
            ApplyStabilization();
        }

        ApplyVisualTilt();
    }

    private void GetInput()
    {
        movementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
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
            targetYaw += rotationInput * rotationSpeed * Time.deltaTime;

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
            rb.AddForce(localMovement * moveSpeed * speedMultiplier, ForceMode.Force);
        }
    }

    private void ApplyHeightControl()
    {
        if (liftInput > 0.1f)
            targetHeight += ascendSpeed * Time.deltaTime;
        else if (liftInput < -0.1f)
            targetHeight -= descendSpeed * Time.deltaTime;

        float heightDiff = targetHeight - transform.position.y;
        rb.AddForce(Vector3.up * heightDiff * hoverForce, ForceMode.Force);
    }

    private void ApplyStabilization()
    {
        Vector3 angularVel = rb.angularVelocity;
        rb.angularVelocity = new Vector3(
            Mathf.Lerp(angularVel.x, 0, 6f * Time.deltaTime),
            angularVel.y,
            Mathf.Lerp(angularVel.z, 0, 6f * Time.deltaTime)
        );

        Quaternion targetRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 4f * Time.deltaTime);
    }

    private void ApplyVisualTilt()
    {
        float targetTiltX = movementInput.z * maxTiltAngle;
        float targetTiltZ = -movementInput.x * maxTiltAngle;

        if (isStunned)
        {
            targetTiltX += Random.Range(-15f, 15f);
            targetTiltZ += Random.Range(-15f, 15f);
        }

        currentTiltX = Mathf.Lerp(currentTiltX, targetTiltX, tiltSmoothness * Time.deltaTime);
        currentTiltZ = Mathf.Lerp(currentTiltZ, targetTiltZ, tiltSmoothness * Time.deltaTime);

        if (visualModel != null)
            visualModel.localRotation = Quaternion.Euler(currentTiltX, 0, currentTiltZ);
    }

    private float GetCargoSpeedMultiplier()
    {
        CargoSystem cargo = GetComponent<CargoSystem>();
        return cargo != null ? cargo.GetSpeedMultiplier() : 1f - cargoWeightEffect;
    }

    private void MoveAllMeshesToVisual(Transform visualParent)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.transform != visualParent && renderer.transform != transform)
                renderer.transform.SetParent(visualParent);
        }
    }

    // ============================
    // Отскок с ограничением и направлением против движения
    // ============================
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length == 0) return;

        Vector3 horizontalVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        float speed = horizontalVel.magnitude;
        if (speed < minCollisionSpeed) return;

        // Ограничиваем силу
        float pushMag = Mathf.Clamp(speed * collisionPushForce, minPushForce, maxPushForce);

        // Определяем направление отскока против движения, с учётом нормали столкновения
        Vector3 collisionNormal = collision.contacts[0].normal;
        Vector3 pushDir = Vector3.ProjectOnPlane(-horizontalVel, collisionNormal).normalized;

        // Сбрасываем горизонтальную скорость
        rb.velocity -= horizontalVel;

        // Применяем отскок
        Vector3 push = pushDir * pushMag + Vector3.up * verticalImpulse;
        rb.AddForce(push, ForceMode.VelocityChange);

        // Вращение для эффекта удара
        rb.angularVelocity = Random.onUnitSphere * collisionAngularKick;

        // Стан
        isStunned = true;
        stunTimer = stunDuration;
    }
}
