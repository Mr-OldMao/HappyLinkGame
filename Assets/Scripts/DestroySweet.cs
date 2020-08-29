using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于清除 匹配成功 的甜品
/// </summary>
public class DestroySweet : MonoBehaviour
{
    public AnimationClip animDestroy;

    private bool canClear = false;
    private bool isClearing = false;
    /// <summary>
    /// 是否可以被清除
    /// </summary>
    public bool CanClaer
    {
        set { canClear = value; }
        get { return canClear; }
    }
    /// <summary>
    /// 是否正在被清除
    /// </summary>
    public bool IsClearing
    {
        get { return isClearing; }
    }


    /// <summary>
    /// 清除已经 匹配成功的甜品
    /// </summary>
    /// <param name="finishSweet"></param>
    public void DestroyFinishSweet(GameSweet finishSweet)
    {
        isClearing = true;
        StartCoroutine(DelayDestroy(finishSweet));
    }

    /// <summary>
    /// 延时清除 逻辑
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayDestroy(GameSweet finishSweet)
    {
        //播放销毁动画
        GetComponent<Animator>().Play(animDestroy.name);
        yield return new WaitForSeconds(animDestroy.length); 
        //增加分数UI   
        //UIManager.GetInstance.AddCount(1);
        Destroy(finishSweet.gameObject);
    }

    
}