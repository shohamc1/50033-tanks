using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuController : MonoBehaviour
{
    public delegate void gameEvent();
    public static event gameEvent RoundSkip;

    // Start is called before the first frame update
    void Start()
    {
        TankHealth.OnPlayerDeath += EnableFastForward;
        GameManager.OnRoundEnd += DisableFastForward;

        DisableFastForward();
    }

    public void EnableFastForward()
    {
        foreach (Transform eachChild in transform)
        {
            if (eachChild.name == "FastForward" || eachChild.name == "Skip")
            {
                eachChild.gameObject.SetActive(true);
            }
        }
    }

    public void DisableFastForward()
    {
        foreach (Transform eachChild in transform)
        {
            if (eachChild.name == "FastForward" || eachChild.name == "Skip")
            {
                eachChild.gameObject.SetActive(false);
            }
        }
    }


    // Update is called once per frame


    public void FastForwardClicked()
    {

        Time.timeScale = 8.0f;
    }

    public void Skip()
    {
        RoundSkip();
    }

}
