using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    public Button btn_Exit;
    public Button btn_StartGame;

    public void Start()
    {
        AudioManager.GetInstance.PlayAudio(AudioManager.AudioType.BGM_StartScene);
        btn_Exit.onClick.AddListener(() => { Application.Quit(); });
        btn_StartGame.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Main");
            AudioManager.GetInstance.PlayAudio(AudioManager.AudioType.BGM_MainScene);
        });
    }



}
