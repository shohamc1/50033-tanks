using UnityEngine;

public class HealthPowerup : MonoBehaviour
{
    public delegate void gameEvent();
    public static event gameEvent PowerupReset;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var health = other.GetComponent<TankHealth>();
            health.m_CurrentHealth = 100f;
            health.SetHealthUI();

            PowerupReset();
            Destroy(this.gameObject);
        }
    }
}