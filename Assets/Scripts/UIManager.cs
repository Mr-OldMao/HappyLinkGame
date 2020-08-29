using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text txt_Timer;
    [SerializeField]
    private float timer;
    private float timerLerp_Scale; //时间缩放 插值数据
    private bool m_IsPauseTimer = false;  //暂停
    public bool SetGetPauseTimer
    {
        set { m_IsPauseTimer = value; }
        get { return m_IsPauseTimer; }
    }

    public Button btn_Setting;
    public Text txt_Score;
    public Text txt_Count;

    public static UIManager m_Instance;
    public static UIManager GetInstance
    {
        get { return m_Instance; }
    }


    //分数  个数 动画变量数据
    private float m_StartScore;
    private int m_EndScore;
    private float m_StartCount;
    private int m_EndCount;
    public int CurEndCount
    {
        get { return m_EndCount; }
    }

    //游戏结束
    public Canvas cvs_GameOver;
    public Button btn_GameOverAgain;
    public Button btn_GameOverReturn;
    public Button btn_GameOverClose;
    public Text txt_ScoreGameOver;
    public Text txt_CountGameOver;
    public AnimationClip animClip_GameOver;

    //设置
    public Canvas cvs_Setting;
    public Button btn_SettingAgain;
    public Button btn_SettingReturn;
    public Button btn_SettingClose;
    public Slider sli_BGM;
    public Slider sli_Sound;



    private bool m_IsGameOver;
    public bool GetIsGameOver
    {
        get { return m_IsGameOver; }
    }


    void Awake()
    {
        m_Instance = this;
    }
    void Start()
    {
        Init();
        ButtonUIEnent();
    }

    private void Init()
    {
        timer = 60f;
        timerLerp_Scale = 1;

        txt_Score.text = "0";
        txt_Count.text = "0";
        m_StartScore = 0;
        m_EndScore = 0;
        m_StartCount = 0;
        m_EndCount = 0;


        cvs_Setting.gameObject.SetActive(false);
        cvs_GameOver.gameObject.SetActive(false);
        m_IsGameOver = false;
    }
    /// <summary>
    /// 按钮事件
    /// </summary>
    private void ButtonUIEnent()
    {
        //设置按钮
        btn_Setting.onClick.AddListener(() =>
        {
            cvs_Setting.gameObject.SetActive(true);
            cvs_Setting.GetComponent<Animator>().SetTrigger("Open");
            //GameManager.GetInstance.StopAllCoroutines();  
            //暂停时间
            m_IsPauseTimer = true;
            //不允许移动甜品
            GameManager.GetInstance.SetGetUserMoveSweet = false;
        }
        );
        //游戏结束事件
        //关闭窗口
        btn_GameOverClose.onClick.AddListener(() =>
        {
            m_IsGameOver = false;
            Animator animatorComponent = cvs_GameOver.GetComponent<Animator>();
            animatorComponent.SetTrigger("Close");
            //animatorComponent.Play(animClip_GameOver.name); 
            StartCoroutine(WaitTimeTodo(animClip_GameOver.length, () => cvs_GameOver.gameObject.SetActive(false)));
        }
        );
        //重新开始
        btn_GameOverAgain.onClick.AddListener(() =>
        {
            m_IsGameOver = false;
            Animator animatorComponent = cvs_GameOver.GetComponent<Animator>();
            animatorComponent.SetTrigger("Close");
            //animatorComponent.Play(animClip_GameOver.name);  
            StopAllCoroutines();
            StartCoroutine(WaitTimeTodo(animClip_GameOver.length, () =>
            {
                cvs_GameOver.gameObject.SetActive(false);
                //重开
                Init();
                SendMessageUpwards("RestartGame");
            }
            ));
        }
        );
        //返回主菜单
        btn_GameOverReturn.onClick.AddListener(() =>
        {
            if (GameObject.FindObjectOfType<AudioManager>())
                Destroy(GameObject.FindObjectOfType<AudioManager>().gameObject);
            StopAllCoroutines();
            SceneManager.LoadScene("Start");
        }
        );

        //设置事件
        //关闭窗口
        btn_SettingClose.onClick.AddListener(() =>
        {
            m_IsGameOver = false;
            Animator animatorComponent = cvs_Setting.GetComponent<Animator>();
            animatorComponent.SetTrigger("Close");
            //animatorComponent.Play(animClip_GameOver.name);  
            StartCoroutine(WaitTimeTodo(animClip_GameOver.length, () => cvs_Setting.gameObject.SetActive(false)));
            //取消暂停
            m_IsPauseTimer = false;
            //允许移动甜品
            GameManager.GetInstance.SetGetUserMoveSweet = true;
        }
        );
        //重新开始
        btn_SettingAgain.onClick.AddListener(() =>
        {
            m_IsGameOver = false;
            Animator animatorComponent = cvs_Setting.GetComponent<Animator>();
            animatorComponent.SetTrigger("Close");
            StopAllCoroutines();
            //animatorComponent.Play(animClip_GameOver.name); 

            StartCoroutine(WaitTimeTodo(animClip_GameOver.length, () =>
            {
                cvs_Setting.gameObject.SetActive(false);
                //取消暂停
                m_IsPauseTimer = false;
                //允许移动甜品
                GameManager.GetInstance.SetGetUserMoveSweet = true;
                //重开
                Init();
                SendMessageUpwards("RestartGame");
            }
            ));
        }
        );
        //返回主菜单
        btn_SettingReturn.onClick.AddListener(() =>
        {
            if (GameObject.FindObjectOfType<AudioManager>())
                Destroy(GameObject.FindObjectOfType<AudioManager>().gameObject);
            StopAllCoroutines();
            SceneManager.LoadScene("Start");
        }
        );
        sli_BGM.onValueChanged.AddListener((v) =>
        {
            AudioManager.GetInstance.AudioVolume(v, true);
        });
        sli_Sound.onValueChanged.AddListener((v) =>
        {
            AudioManager.GetInstance.AudioVolume(v, false);
        });

    }

    /// <summary>
    /// 累加分数 
    /// </summary>
    /// <param name="score">匹配个数</param>
    private void AddScore(int count)
    {
        ////默认100*匹配成功的个数+ 匹配成功个数-最低要求匹配成功个数*200  
        ////3个=》300  4个=》600  5个=》500+400=900
        int score = 100 * count + (count - GameManager.GetInstance.matchCount) * 200;
        //int score = count * 520;

        m_StartScore = int.Parse(txt_Score.text);
        m_EndScore += score;
        // txt_Score.text = (int.Parse(txt_Score.text) + score).ToString();
    }
    /// <summary>
    /// 累加匹配个数
    /// </summary>
    public void AddCount(int count)
    {
        m_StartCount = int.Parse(txt_Count.text);
        m_EndCount += count;
        //txt_Count.text = (int.Parse(txt_Count.text) + count).ToString();
        AddScore(count);
        //增加时间 
        timer = count < 10 ? timer + count : timer + 10;
    }


    void Update()
    {
        if (!m_IsPauseTimer)
        {
            //时间 
            timer -= Time.deltaTime;
            if (timer >= 0)
            {
                txt_Timer.text = ((int)timer).ToString();
                //时间UI缩放
                timerLerp_Scale = Mathf.Lerp(timerLerp_Scale, 0, Time.deltaTime);
                if (timer % 1 <= 0.05F)//当前时间为整数时  当前缩放为1 
                    timerLerp_Scale = 1;
                txt_Timer.rectTransform.localScale = new Vector3(timerLerp_Scale, timerLerp_Scale, 0);
                txt_Timer.color = new Color(1, timerLerp_Scale, timerLerp_Scale, 1);
            }
            else
            {
                txt_Timer.text = "0";
                txt_Timer.color = Color.red;
                txt_Timer.rectTransform.localScale = new Vector3(1, 1, 0);
                m_IsGameOver = true;
            }
        }

        //分数变化动画
        if (Mathf.Abs(m_StartScore - m_EndScore) > 10f)
        {
            ChangeScoreAnim();
            txt_Score.text = ((int)m_StartScore).ToString();
        }
        else
        {
            txt_Score.text = m_EndScore.ToString();
        }

        //个数变化动画
        if (Mathf.Abs(m_StartCount - m_EndCount) > 1f)
        {
            ChangeCountAnim();
            txt_Count.text = ((int)m_StartCount).ToString();
        }
        else
        {
            txt_Count.text = m_EndCount.ToString();
        }

        //游戏结束
        if (m_IsGameOver && !cvs_GameOver.gameObject.activeSelf)
        {
            SendMessageUpwards("GameOver");
        }

    }

    /// <summary>
    /// 分数变化动画
    /// </summary> 
    private void ChangeScoreAnim()
    {
        m_StartScore = Mathf.Lerp(m_StartScore, m_EndScore, 0.2f);
    }
    /// <summary>
    /// 分数变化动画
    /// </summary>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    private void ChangeCountAnim()
    {
        m_StartCount = Mathf.Lerp(m_StartCount, m_EndCount, 0.2f);
    }

    /// <summary>
    /// 结束游戏UI
    /// </summary>
    public void GameOver()
    {
        txt_ScoreGameOver.text = m_EndScore.ToString();
        txt_CountGameOver.text = m_EndCount.ToString();
        cvs_GameOver.gameObject.SetActive(true);
        cvs_GameOver.GetComponent<Animator>().SetTrigger("Open");
        //cvs_GameOver.GetComponent<Animator>().Play(animClip_GameOver.name); 
    }

    IEnumerator WaitTimeTodo(float waitTimer, UnityAction action)
    {
        if (waitTimer <= 0) waitTimer = 0;
        yield return new WaitForSeconds(waitTimer);
        action.Invoke();
    }

}
