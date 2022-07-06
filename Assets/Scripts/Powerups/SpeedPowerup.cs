using UnityEngine;
using System.Collections;

public class SpeedPowerup : MonoBehaviour
{
    public delegate void gameEvent();
    public static event gameEvent PowerupReset;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var shooting = other.GetComponent<TankMovement>();
            shooting.m_Speed = 24f;

            StartCoroutine(disablePowerup(other));

            this.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
        }
    }

    IEnumerator disablePowerup(Collider other)
    {
        yield return new WaitForSeconds(5.0f);

        var shooting = other.GetComponent<TankMovement>();
        shooting.m_Speed = 12f;

        PowerupReset();
        Destroy(this.gameObject);
    }
}