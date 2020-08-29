using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSweet : MonoBehaviour
{

    private GameSweet m_Sweet;
    //移动协程
    public Coroutine moveCoroutine;
    void Awake()
    {
        m_Sweet = GetComponent<GameSweet>();
    }

    /// <summary>
    /// 开始或者结束一个协程
    /// </summary>
    /// <param name="targetX">目标点x</param>
    /// <param name="targetY">目标点y</param>
    /// <param name="time">移动至目标点所需时间</param>
    public void Move(int targetX, int targetY, float time)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveCoroutine(targetX, targetY, time));
    }


    /// <summary>
    /// 甜品移动插值动画协程
    /// </summary>
    /// <param name="targetX"></param>
    /// <param name="targetY"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator MoveCoroutine(int targetX, int targetY, float time)
    {
        //虚拟坐标
        m_Sweet.X = targetX;
        m_Sweet.Y = targetY;
        //实际坐标
        Vector3 endPos = m_Sweet.gameManagerScr.ChangePosVirtualToReal(targetX, targetY);
        Vector3 startPos = m_Sweet.transform.position;
        //每帧移动一点
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            m_Sweet.transform.position = Vector3.Lerp(startPos, endPos, i / time);
            yield return 0;
        }
        m_Sweet.transform.position = endPos;
        yield return 0;
    }


}
