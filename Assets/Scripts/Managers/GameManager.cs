using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5;
    public float m_StartDelay = 3f;
    public float m_EndDelay = 3f;
    public CameraControl m_CameraControl;
    public Text m_MessageText;
    public GameObject[] m_TankPrefabs;
    public TankManager[] m_Tanks;
    public List<Transform> wayPointsForAI;

    private int m_RoundNumber;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;
    public GameConstants gameConstants;
    public GameObject healthPowerup;
    public GameObject speedPowerup;
    public Transform healthPowerupLocation;
    public Transform speedPowerupLocation;
    private bool hasPowerupSpawned;
    private int powerupCounter = 0;

    public delegate void gameEvent();
    public static event gameEvent OnRoundEnd;
    private int alert;
    private AudioSource audiosource;
    public AudioClip undetectedClip;
    public AudioClip detectedClip;


    private void Start()
    {
        audiosource = GetComponent<AudioSource>();
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();
        SetCameraTargets();

        StartCoroutine(GameLoop());

        MenuController.RoundSkip += FinishRound;
        SpeedPowerup.PowerupReset += ResetPowerup;
        HealthPowerup.PowerupReset += ResetPowerup;
        StateController.TurnOnAlarm += PlayAlarm;
        StateController.TurnOffAlarm += StopAlarm;
    }

    private void ResetPowerup()
    {
        hasPowerupSpawned = false;
        powerupCounter++;
    }

    private void PlayAlarm()
    {
        alert++;
        Debug.Log(alert);
        if (alert - 1 == 0)
        {
            Debug.Log("starting alarm");
            audiosource.clip = detectedClip;
            audiosource.Play();
        }
    }

    private void StopAlarm()
    {
        alert--;
        Debug.Log(alert);
        if (alert == 0)
        {
            Debug.Log("Stopping alarm");
            audiosource.clip = undetectedClip;
            audiosource.Play();
        }
    }


    private void SpawnAllTanks()
    {
        m_Tanks[0].m_Instance =
            Instantiate(m_TankPrefabs[0], m_Tanks[0].m_SpawnPoint.position, m_Tanks[0].m_SpawnPoint.rotation) as GameObject;
        m_Tanks[0].m_PlayerNumber = 1;
        m_Tanks[0].SetupPlayerTank();

        for (int i = 1; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefabs[i], m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].SetupAI(wayPointsForAI);
        }
    }

    public void FinishRound()
    {
        foreach (TankManager tank in m_Tanks)
        {
            if (!OneTankLeft())
                tank.m_Instance.SetActive(false);
        }
    }


    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];

        for (int i = 0; i < targets.Length; i++)
            targets[i] = m_Tanks[i].m_Instance.transform;

        m_CameraControl.m_Targets = targets;
    }


    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameWinner != null) SceneManager.LoadScene(0);
        else StartCoroutine(GameLoop());
    }


    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();

        m_CameraControl.SetStartPositionAndSize();

        m_RoundNumber++;
        m_MessageText.text = $"ROUND {m_RoundNumber}";

        yield return m_StartWait;
    }


    private IEnumerator RoundPlaying()
    {
        EnableTankControl();

        m_MessageText.text = string.Empty;

        while (!OneTankLeft()) yield return null;
    }


    private IEnumerator RoundEnding()
    {
        DisableTankControl();

        m_RoundWinner = null;

        m_RoundWinner = GetRoundWinner();
        if (m_RoundWinner != null) m_RoundWinner.m_Wins++;
        if (m_RoundWinner.m_Instance.CompareTag("Player"))
            gameConstants.roundNumber++;

        m_GameWinner = GetGameWinner();

        string message = EndMessage();
        m_MessageText.text = message;

        OnRoundEnd();
        Time.timeScale = 1.0f;
        alert = 0;

        yield return m_EndWait;
    }


    private bool OneTankLeft()
    {
        // should spawn powerup?
        if (Random.Range(0, 1000) == 1 && !hasPowerupSpawned)
            SpawnPowerup();

        int numTanksLeft = 0;

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf) numTanksLeft++;
        }

        return numTanksLeft <= 1;
    }

    private void SpawnPowerup()
    {
        hasPowerupSpawned = true;
        if (powerupCounter % 2 == 1)
        {
            Instantiate(speedPowerup, speedPowerupLocation.position, speedPowerupLocation.rotation);
        }
        else
        {
            Instantiate(healthPowerup, healthPowerupLocation.position, healthPowerupLocation.rotation);
        }
    }

    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        return null;
    }

    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        return null;
    }


    private string EndMessage()
    {
        var sb = new StringBuilder();

        if (m_RoundWinner != null) sb.Append($"{m_RoundWinner.m_ColoredPlayerText} WINS THE ROUND!");
        else sb.Append("DRAW!");

        sb.Append("\n\n\n\n");

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            sb.AppendLine($"{m_Tanks[i].m_ColoredPlayerText}: {m_Tanks[i].m_Wins} WINS");
        }

        if (m_GameWinner != null)
            sb.Append($"{m_GameWinner.m_ColoredPlayerText} WINS THE GAME!");

        return sb.ToString();
    }


    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].Reset();
    }


    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].EnableControl();
    }


    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].DisableControl();
    }
}