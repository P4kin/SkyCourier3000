using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DroneUIManager : MonoBehaviour
{
    [Header("Ссылки на дрон")]
    public BatterySystem batterySystem;
    public CargoSystem cargoSystem;
    public Camera mainCamera;

    [Header("UI элементы")]
    public TMP_Text batteryText;
    public GameObject cargoInfoPanel;
    public TMP_Text cargoNameText;
    public TMP_Text cargoWeightText;
    public TMP_Text cargoRewardText;
    public TMP_Text deliveryTimerText;

    [Header("NavBar (компас)")]
    public RectTransform navBar;
    public GameObject targetMarkerPrefab; // Prefab с TMP_Text внутри
    public float navBarWidth = 400f;
    public float smoothSpeed = 5f;
    public float maxAngle = 90f;

    [Header("Таймер доставки")]
    public float deliveryTimeMax = 180f;
    private float deliveryTimeRemaining;
    private bool missionFailed = false;

    [Header("Статистика")]
    public TMP_Text creditsText; // Отображение кредитов
    private int totalCredits = 0;

    [Header("Floating Points")]
    public GameObject floatingPointsPrefab; // TMP_Text prefab
    public RectTransform uiCanvas;          // Canvas для текста
    public float floatingMoveSpeed = 50f;
    public float floatingDuration = 1f;
    private List<FloatingText> floatingTexts = new List<FloatingText>();

    private Dictionary<GameObject, GameObject> markerDict = new Dictionary<GameObject, GameObject>();

    void Start()
    {
        deliveryTimeRemaining = deliveryTimeMax;
    }

    void Update()
    {
        UpdateBatteryUI();
        UpdateCargoUI();
        UpdateDeliveryTimerUI();
        UpdateNavBar();
        UpdateFloatingPoints();
    }

    #region Battery
    private void UpdateBatteryUI()
    {
        if (batterySystem != null && batteryText != null)
            batteryText.text = $"{batterySystem.GetBatteryPercentage():F0}%";
    }
    #endregion

    #region Cargo UI
    private void UpdateCargoUI()
    {
        if (cargoInfoPanel == null) return;

        if (cargoSystem != null && cargoSystem.HasCargo())
        {
            cargoInfoPanel.SetActive(true);
            CargoItem cargo = cargoSystem.GetCargoItem();
            if (cargo != null)
            {
                cargoNameText.text = cargo.cargoName;
                cargoWeightText.text = $"Вес: {cargo.weight} кг";
                cargoRewardText.text = $"Награда: {cargo.reward}";
            }
        }
        else
        {
            cargoInfoPanel.SetActive(false);
        }
    }
    #endregion

    #region Delivery Timer
    private void UpdateDeliveryTimerUI()
    {
        if (deliveryTimerText == null) return;

        if (cargoSystem != null && cargoSystem.HasCargo() && !missionFailed)
        {
            deliveryTimerText.gameObject.SetActive(true);
            deliveryTimeRemaining -= Time.deltaTime;

            int minutes = Mathf.FloorToInt(deliveryTimeRemaining / 60f);
            int seconds = Mathf.FloorToInt(deliveryTimeRemaining % 60f);
            deliveryTimerText.text = $"{minutes:00}:{seconds:00}";

            if (deliveryTimeRemaining <= 0f)
                MissionFailed();
        }
        else
        {
            deliveryTimerText.gameObject.SetActive(false);
            deliveryTimeRemaining = deliveryTimeMax;
            missionFailed = false;
        }
    }

    private void MissionFailed()
{
    missionFailed = true;
    deliveryTimerText.text = "Доставка не удалась";

    // Удаляем груз
    if (cargoSystem != null && cargoSystem.HasCargo())
    {
        GameObject cargoObj = cargoSystem.GetCargoObject();
        cargoSystem.DetachCargo();

        if (cargoObj != null)
            Destroy(cargoObj);
    }

    Debug.Log("Доставка провалена — груз потерян");
}
    #endregion

    #region NavBar (Compass)
    private void UpdateNavBar()
    {
        if (mainCamera == null || navBar == null || targetMarkerPrefab == null) return;

        List<GameObject> targets = GetCurrentTargets();
        SyncMarkers(targets);

        foreach (var kvp in markerDict)
        {
            GameObject target = kvp.Key;
            GameObject marker = kvp.Value;

            if (!targets.Contains(target))
            {
                marker.SetActive(false);
                continue;
            }

            marker.SetActive(true);

            Vector3 dirToTarget = target.transform.position - mainCamera.transform.position;
            dirToTarget.y = 0;
            Vector3 forward = mainCamera.transform.forward;
            forward.y = 0;

            float angle = Vector3.SignedAngle(forward, dirToTarget, Vector3.up);
            float normalized = Mathf.Clamp(angle / maxAngle, -1f, 1f);

            if (Vector3.Dot(dirToTarget.normalized, forward.normalized) < 0)
                normalized = angle > 0 ? 1f : -1f;

            float targetX = normalized * (navBarWidth / 2f);

            marker.transform.localPosition = Vector3.Lerp(
                marker.transform.localPosition,
                new Vector3(targetX, 0f, 0f),
                Time.deltaTime * smoothSpeed
            );

            TMP_Text t = marker.GetComponentInChildren<TMP_Text>();
            if (t != null)
            {
                float distance = Vector3.Distance(mainCamera.transform.position, target.transform.position);
                t.text = target.name + "\n" + distance.ToString("F0") + " м";
            }
        }
    }

    private List<GameObject> GetCurrentTargets()
    {
        List<GameObject> targets = new List<GameObject>();

        if (cargoSystem != null && cargoSystem.HasCargo())
        {
            CargoItem cargo = cargoSystem.GetCargoItem();
            if (cargo != null)
            {
                DeliveryPoint dp = FindDeliveryPointById(cargo.targetPointId);
                if (dp != null) targets.Add(dp.gameObject);
            }
        }
        else
        {
            CargoItem[] allCargo = FindObjectsOfType<CargoItem>();
            foreach (var cargo in allCargo)
            {
                if (cargo != null) targets.Add(cargo.gameObject);
            }
        }

        return targets;
    }

    private DeliveryPoint FindDeliveryPointById(string id)
    {
        DeliveryPoint[] points = FindObjectsOfType<DeliveryPoint>();
        foreach (var point in points)
        {
            if (point.pointId == id) return point;
        }
        return null;
    }

    private void SyncMarkers(List<GameObject> targets)
    {
        foreach (var t in targets)
        {
            if (!markerDict.ContainsKey(t))
            {
                GameObject marker = Instantiate(targetMarkerPrefab, navBar);
                markerDict.Add(t, marker);
            }
        }

        foreach (var kvp in markerDict)
        {
            if (!targets.Contains(kvp.Key))
                kvp.Value.SetActive(false);
        }
    }
    #endregion

    #region Floating Points
    public void SpawnFloatingPoints(int reward, Vector3 worldPosition)
    {
        if (floatingPointsPrefab == null || uiCanvas == null) return;

        GameObject fp = Instantiate(floatingPointsPrefab, uiCanvas);
        fp.SetActive(true);

        TMP_Text tmp = fp.GetComponent<TMP_Text>();
        if (tmp != null)
            tmp.text = $"+{reward}";

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition + Vector3.up * 2f);
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiCanvas, screenPos, null, out anchoredPos
        );
        fp.GetComponent<RectTransform>().anchoredPosition = anchoredPos;

        floatingTexts.Add(new FloatingText { textObj = fp, timer = 0f });
        totalCredits += reward; // reward передается из DeliveryPoint
        if (creditsText != null)
        creditsText.text = $"Кредиты: {totalCredits}";
    }

    private void UpdateFloatingPoints()
    {
        for (int i = floatingTexts.Count - 1; i >= 0; i--)
        {
            FloatingText ft = floatingTexts[i];
            ft.textObj.GetComponent<RectTransform>().anchoredPosition += Vector2.up * floatingMoveSpeed * Time.deltaTime;
            ft.timer += Time.deltaTime;
            if (ft.timer >= floatingDuration)
            {
                Destroy(ft.textObj);
                floatingTexts.RemoveAt(i);
            }
        }
        
    }
    public void UpdateCreditsUI(int totalCredits)
{
    if (creditsText != null)
        creditsText.text = $"Кредиты: {totalCredits}";
}

    

    private class FloatingText
    {
        public GameObject textObj;
        public float timer;
    }

    public void RestartGame()
{
    // Перезагружаем текущую сцену
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
    #endregion
}
