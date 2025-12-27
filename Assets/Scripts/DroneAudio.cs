using UnityEngine;

public class DroneAudio : MonoBehaviour
{
    [Header("Звуки")]
    public AudioClip propellerLoopClip;   // Жужжание дрона
    public AudioClip deliveryClip;        // Пилиньк при доставке

    private AudioSource propellerSource;
    private AudioSource sfxSource;

    private BatterySystem batterySystem;
    private DeliverySystem deliverySystem;

    void Start()
    {
        batterySystem = GetComponent<BatterySystem>();
        deliverySystem = GetComponent<DeliverySystem>();

        // AudioSource для жужжания (loop)
        propellerSource = gameObject.AddComponent<AudioSource>();
        propellerSource.clip = propellerLoopClip;
        propellerSource.loop = true;
        propellerSource.playOnAwake = false;

        if (propellerLoopClip != null)
            propellerSource.Play();

        // AudioSource для одноразовых звуков (SFX)
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    void Update()
    {
        // Жужжание идет только если батарея > 0
        if (batterySystem != null)
        {
            if (batterySystem.GetCurrentBattery() <= 0)
            {
                if (propellerSource.isPlaying)
                    propellerSource.Stop();
            }
            else
            {
                if (!propellerSource.isPlaying && propellerLoopClip != null)
                    propellerSource.Play();
            }
        }
    }

    // Вызвать при доставке груза
    public void PlayDeliverySound()
    {
        if (deliveryClip != null && sfxSource != null)
            sfxSource.PlayOneShot(deliveryClip);
    }
}