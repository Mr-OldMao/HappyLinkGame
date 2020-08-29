using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏甜品道具  
/// 属性坐标，类型 
/// 调用 MoveSweet SweetColor DestroySweet 脚本
/// </summary>
public class GameSweet : MonoBehaviour
{
    private int x;
    private int y;
    private GameManager.SweetsType type;
    [HideInInspector]
    public GameManager gameManagerScr;

    private MoveSweet moveComponent;
    private SweetColor sweetColorComponent;
    private DestroySweet destroySweetComponent;

    public int X
    {
        get
        {
            return x;
        }
        set
        {
            if (CanMove())
            {
                x = value;
            }
        }
    }
    public int Y
    {
        get
        {
            return y;
        }
        set
        {
            if (CanMove())
            {
                y = value;
            }
        }
    }
    public GameManager.SweetsType GetSweetsType { get => type; }
    //获取move脚本
    public MoveSweet MoveComponent { get => moveComponent; }
    //获取SweetColor脚本
    public SweetColor SweetColorComponent
    {
        get { return sweetColorComponent; }
    }
    //获取清楚（摧毁）甜点脚本
    public DestroySweet DestroySweetComponent
    {
        get { return destroySweetComponent; }
    }



    /// <summary>
    /// 是否允许移动
    /// </summary>
    /// <returns></returns>
    public bool CanMove()
    {
        return MoveComponent != null;
    }
    /// <summary>
    /// 是否允许设置颜色面片
    /// </summary>
    public bool CanSetColor()
    {
        return SweetColorComponent != null;
    }

    void Awake()
    {
        moveComponent = GetComponent<MoveSweet>();
        sweetColorComponent = GetComponent<SweetColor>();
        destroySweetComponent = GetComponent<DestroySweet>();
    }

    public void Init(int x, int y, GameManager.SweetsType type)
    {
        this.x = x;
        this.y = y;
        this.type = type;
        this.gameManagerScr = GameManager.GetInstance;
    }


    private void OnMouseEnter()
    {
        gameManagerScr.EnterdSweet(this);
    }
    private void OnMouseDown()
    {
        gameManagerScr.PressedSweet(this);
    }
    private void OnMouseUp()
    {
        //释放甜品 -- 交换甜品
        gameManagerScr.ReleaseSweet();
    }

}
