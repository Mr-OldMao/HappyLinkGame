using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    //单例
    private static GameManager _instance;
    public static GameManager GetInstance { get => _instance; set => _instance = value; }

    //甜品类型
    public enum SweetsType
    {
        /// <summary>
        /// 空甜品
        /// </summary>
        Empty,
        /// <summary>
        /// 普通甜品 可以移动
        /// </summary>
        Normal,
        /// <summary>
        /// 障碍甜品 不可移动
        /// </summary>
        barrier,
        /// <summary>
        /// 任意甜品 可整行、列匹配
        /// </summary>
        any,
        /// <summary>
        /// 同色甜点 可消除同一种颜色所有甜点
        /// </summary>
        Same,

        Count
    }
    //甜品字典  通过甜品类型拿到甜品预设体
    public Dictionary<SweetsType, GameObject> sweetPrefabDict;
    //甜品结构体  用于赋值给甜品字典
    [System.Serializable]
    public struct SweetPrefab
    {
        public SweetsType type;
        public GameObject prefab;
    }
    public SweetPrefab[] sweetPrefabs;

    //甜品实例   [,]存放位置
    public GameSweet[,] sweets = { };
    public Transform sweetsContainer;

    //大网格的行列数     坐标(列，行)
    public int xColumn;         //列
    public int yRow;            //行
    //网格底层
    public GameObject gridPrefab;
    public Transform gridPrefabContainer;
    //填充等待时间(甜品下落速度)
    public float fillTime = 0.1f;
    //网格 甜品容器的容器
    public GameObject allContainer;

    //鼠标按下的甜品对象
    private GameSweet pressedSweet;
    //鼠标进入的甜品对象
    private GameSweet enterdSweet;
    //用户能否移动甜品
    private bool userCanMoveSweet = true;
    public bool SetGetUserMoveSweet
    {
        set { userCanMoveSweet = value; }
        get { return userCanMoveSweet; }
    }

    /// <summary>
    /// 甜品匹配成功的最少个数 
    /// </summary>
    public int matchCount = 3;

    //private DestroySweet destroySweet; //清除甜品脚本

    void Awake()
    {
        _instance = this;
    }
    void Start()
    {
        Init();
        StartCoroutine(AllFill());
        ChangePosAndScaleForAndroid();
        //测试创建障碍  
        CreateBarrier(10);//Random.Range(2, 7)


    }
    #region 初始化操作

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        sweetPrefabDict = new Dictionary<SweetsType, GameObject>();
        sweets = new GameSweet[xColumn, yRow];

        //初始化 底层网格
        for (int i = 0; i < xColumn; i++)
        {
            for (int j = 0; j < yRow; j++)
            {
                GameObject clone = Instantiate(gridPrefab, new Vector3(i - xColumn / 2 + 0.5f, j - yRow / 2 + 0.5f, 0), Quaternion.identity); //PC
                //GameObject clone = Instantiate(gridPrefab, new Vector3((i - xColumn / 2 + 0.5f)/1.5f, (j - yRow / 2 + 0.5f)/1.5f, 0), Quaternion.identity); //android
                // Debug.Log(new Vector3(i - xColumn / 2, j - yRow / 2, 0));
                clone.transform.SetParent(gridPrefabContainer);
            }
        }
        //初始化字典 
        for (int i = 0; i < sweetPrefabs.Length; i++)
        {
            if (!sweetPrefabDict.ContainsKey(sweetPrefabs[i].type))
                sweetPrefabDict.Add(sweetPrefabs[i].type, sweetPrefabs[i].prefab);
        }

        //初始化甜品实例在网格上
        for (int i = 0; i < xColumn; i++)
        {
            for (int j = 0; j < yRow; j++)
            {
                CreateNewSweet(i, j, SweetsType.Empty);
            }
        }

        //尝试匹配所有关键节点的甜点   
        MatchImportantSweet(2f, null, true);
    }
    /// <summary>
    /// 更改 容器位置、缩放 调整甜品所在位置
    /// </summary>
    private void ChangePosAndScaleForAndroid()
    {
        //更改容器 缩放
        allContainer.transform.localScale = new Vector3(0.6f, 0.6f, 1);
        allContainer.transform.position = new Vector3(0, -0.8f, 0);
    }
    /// <summary>
    /// 甜品 坐标偏移  把虚拟坐标转换为真实坐标(虚拟坐标原点在屏幕左下角，真实坐标原点在屏幕中央)
    /// </summary>
    public Vector3 ChangePosVirtualToReal(int oldX, int oldY)
    {
        //return new Vector3(oldX - xColumn / 2 + 0.5f, oldY - yRow / 2 + 0.5f, 0);  //PC
        return new Vector3((oldX - xColumn / 2 + 0.5f) / 1.7f, (oldY - yRow / 2 + 0.5f) / 1.66f - 0.79f, 0);  //ANDROID     调试好的密度

    }
    #endregion

    #region  创建道具

    /// <summary>
    /// 创建障碍
    /// </summary>
    /// <param name="BarrierCount">障碍个数</param>
    private void CreateBarrier(int BarrierCount)
    {
        for (int i = 0; i < BarrierCount; i++)
        {
            int x = Random.Range(0, xColumn);
            int y = Random.Range(0, yRow);
            Destroy(sweets[x, y].gameObject);
            CreateNewSweet(x, y, SweetsType.barrier);
        }
    }


    /// <summary>
    /// 创建新的甜品
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public GameSweet CreateNewSweet(int x, int y, SweetsType type)
    {
        GameObject newSweet = Instantiate(sweetPrefabDict[type], ChangePosVirtualToReal(x, y), Quaternion.identity);
        newSweet.transform.SetParent(sweetsContainer);
        //配置甜品属性
        sweets[x, y] = newSweet.GetComponent<GameSweet>();
        sweets[x, y].Init(x, y, type);
        return sweets[x, y];
    }
    #endregion

    #region 自动填充甜品逻辑
    /// <summary>
    /// 全部填充
    /// </summary>
    public IEnumerator AllFill()
    {
        while (Fill())
        {
            yield return new WaitForSeconds(fillTime);
        }
    }
    /// <summary>
    /// 单次自动填充
    /// 算法：在每一列中，判断如果当前行的下一行为空(从倒数第二行开始判断)，即为垂直填充
    /// 则当前行以及以上行全部往下移，当前列第一行新增一个甜品
    /// 若当前行的下一行不为空，当前行的下一行的左/右边为空，切前者上方类型为障碍甜品（barrier）时(即当前甜品的同行左/侧右)，
    /// 即为斜向填充
    /// </summary>
    public bool Fill()
    {
        bool filledNotFinished = false;     //本次填充是否完成,是否满足填充条件
                                            //先遍历行再遍历列  由下至上 由左至右遍历(从倒数第二行开始)
        for (int y = 1; y < yRow; y++)
        {
            for (int x = 0; x < xColumn; x++)
            {
                GameSweet sweet = sweets[x, y]; //得到甜品的虚拟坐标 
                                                //可以移动(即当前是普通甜品)，则往下填充
                if (sweet.CanMove())
                {
                    //当前甜品的下一行同列甜品
                    GameSweet sweetDown = sweets[x, y - 1];
                    //当前甜品的下一行左/右列甜品, 
                    GameSweet sweetDown_Left = null;
                    GameSweet sweetDown_Right = null;
                    //当前甜品的同行左/右侧
                    GameSweet sweetLeft = null;
                    GameSweet sweetRight = null;
                    //特殊情况最右侧无右侧，最左侧无左侧
                    //第0列~倒数第2列
                    if (x >= 0 && x < xColumn - 1)
                    {
                        sweetDown_Right = sweets[x + 1, y - 1];
                        sweetRight = sweets[x + 1, y];
                    }
                    //第1列~倒数第1列
                    if (x >= 1 && x < xColumn)
                    {
                        sweetDown_Left = sweets[x - 1, y - 1];
                        sweetLeft = sweets[x - 1, y];
                    }
                    //当前甜品的下一行同列甜品 是否为空  (垂直填充)
                    if (sweetDown.GetSweetsType == SweetsType.Empty)
                    {
                        //当前甜品往下移动
                        sweet.MoveComponent.Move(x, y - 1, fillTime);
                        //修改位置信息
                        sweets[x, y - 1] = sweet;
                        //原来甜品位置 为空
                        CreateNewSweet(x, y, SweetsType.Empty);
                        filledNotFinished = true;
                    }
                    //右斜向填充   存在右下方甜品  且当前甜品右下方甜品类型为空  且当前甜品右侧为障碍
                    else if (sweetDown_Right && sweetDown_Right.GetSweetsType == SweetsType.Empty && sweetRight.GetSweetsType == SweetsType.barrier)
                    {
                        //当前甜品往右下移动
                        sweet.MoveComponent.Move(x + 1, y - 1, fillTime);
                        //修改位置信息
                        sweets[x + 1, y - 1] = sweet;
                        //原来甜品位置 为空
                        CreateNewSweet(x, y, SweetsType.Empty);
                        filledNotFinished = true;
                    }
                    //左斜向填充  存在左下方甜品  且当前甜品左下方甜品类型为空  且当前甜品左侧为障碍
                    else if (sweetDown_Left && sweetDown_Left.GetSweetsType == SweetsType.Empty && sweetLeft.GetSweetsType == SweetsType.barrier)
                    {
                        //当前甜品往左下移动
                        sweet.MoveComponent.Move(x - 1, y - 1, fillTime);
                        //修改位置信息
                        sweets[x - 1, y - 1] = sweet;
                        //原来甜品位置 为空
                        CreateNewSweet(x, y, SweetsType.Empty);
                        filledNotFinished = true;
                    }
                }
            }
        }
        //最上面一行
        for (int i = 0; i < xColumn; i++)
        {
            //游戏场景的第一行
            GameSweet sweet = sweets[i, yRow - 1];
            if (sweet.GetSweetsType == SweetsType.Empty)
            {
                //在游戏场景的第一行的上一行 创建新甜品
                GameObject newSweet = Instantiate(sweetPrefabDict[SweetsType.Normal], ChangePosVirtualToReal(i, yRow - 1), Quaternion.identity);
                newSweet.transform.SetParent(sweetsContainer);
                //配置甜品属性
                sweets[i, yRow - 1] = newSweet.GetComponent<GameSweet>();
                sweets[i, yRow - 1].Init(i, yRow - 1, SweetsType.Normal);
                sweets[i, yRow - 1].MoveComponent.Move(i, yRow - 1, fillTime);
                sweets[i, yRow - 1].SweetColorComponent.SetColor((SweetColor.ColorType)Random.Range(0, sweets[i, yRow - 1].SweetColorComponent.NumColors));

                filledNotFinished = true;
            }
        }
        return filledNotFinished;
    }

    #endregion

    #region 甜品交换逻辑
    /// <summary>
    /// 判断两个甜品是否相邻  (用于甜品交换)
    /// </summary>
    private bool JudgeAround(GameSweet sweet1, GameSweet sweet2)
    {
        bool result = false;
        //获取两个甜品上的位置信息
        if (sweet1.X == sweet2.X && Mathf.Abs(sweet1.Y - sweet2.Y) == 1)
            result = true;
        else if (sweet1.Y == sweet2.Y && Mathf.Abs(sweet1.X - sweet2.X) == 1)
            result = true;
        return result;
    }
    /// <summary>
    /// 交换甜品
    /// </summary>
    /// <param name="sweet1"></param>
    /// <param name="sweet2"></param>
    private void ExchangeSweets(GameSweet sweet1, GameSweet sweet2)
    {
        if (sweet1.CanMove() && sweet2.CanMove())
        {
            //交换时不允许移动甜点
            userCanMoveSweet = false;
            //交换操作
            //交换存储的位置信息
            sweets[sweet1.X, sweet1.Y] = sweet2;
            sweets[sweet2.X, sweet2.Y] = sweet1;
            //移动实际位置
            int[] temp = { sweet1.X, sweet1.Y };
            sweet1.MoveComponent.Move(sweet2.X, sweet2.Y, fillTime);
            sweet2.MoveComponent.Move(temp[0], temp[1], fillTime);
        }
    }
    /// <summary>
    /// 鼠标按下选中甜品
    /// </summary>
    /// <param name="sweet"></param>
    public void PressedSweet(GameSweet sweet)
    {
        if (userCanMoveSweet)
            pressedSweet = sweet;
    }
    /// <summary>
    /// 鼠标按下选中后进入的新甜品
    /// </summary>
    /// <param name="sweet"></param>
    public void EnterdSweet(GameSweet sweet)
    {
        if (userCanMoveSweet)
            enterdSweet = sweet;
    }
    /// <summary>
    /// 释放甜品 用于交换甜品
    /// </summary>
    public void ReleaseSweet()
    {
        //判断用户是否允许移动甜点
        if (userCanMoveSweet)
        {
            //判断两个甜品是否相邻  
            if (JudgeAround(pressedSweet, enterdSweet))
            {
                GameSweet getPressSweet = pressedSweet;
                GameSweet getEnterdSweet = enterdSweet;
                //执行交换
                ExchangeSweets(getPressSweet, getEnterdSweet);

                //甜品匹配判定
                bool isMathchResult_Press = false;
                bool isMathchResult_Enter = false;

                //若其中一次匹配成功则  消除匹配成功的甜品，甜品填充完后，再次尝试匹配
                isMathchResult_Press = MatchSweets(getPressSweet);
                if (!isMathchResult_Press)
                {
                    isMathchResult_Enter = MatchSweets(getEnterdSweet);
                }
                //全部匹配失败则再重新交换
                if (!isMathchResult_Press && !isMathchResult_Enter)
                    StartCoroutine(WaitSomeTimeTodo(0.6f, () =>
                    {
                        ExchangeSweets(getEnterdSweet, getPressSweet);//交换回来
                        //允许用户移动
                        userCanMoveSweet = true;
                    }
                    ));
            }
        }
    }
    #endregion

    #region 甜品匹配逻辑
    #region 甜点匹配控制器 
    ///// <summary>
    ///// 甜点匹配控制器  
    ///// 封装了 目标甜点的匹配机制，匹配成功自动播放动画且延时销毁匹配成功的实体，并自动填充空缺
    ///// </summary>
    //public bool MatchSweetsControl(GameSweet targetSweet)
    //{
    //    Debug.Log(targetSweet);
    //    //尝试匹配甜品 
    //    List<GameSweet> listGameSweet = MatchSweets(targetSweet);
    //    //判断是否匹配成功
    //    if (listGameSweet != null)
    //    {
    //        //处理已匹配成功的甜点
    //        //DisposeFinishedSweet( listGameSweet);
    //        //自动补充空缺
    //        StartCoroutine(WaitSomeTimeTodo(0.8f, () => StartCoroutine(AllFill())));

    //        //尝试再一次匹配  递归当前方法 
    //        MatchImportantSweet(1f, null, true);
    //        return true;
    //    }
    //    else
    //    {
    //        //允许玩家移动甜点
    //        userCanMoveSweet = true;
    //        return false;
    //    }
    //}
    ///// <summary>
    ///// 甜点匹配控制器 的重载 无参方法用于游戏开始调用
    ///// </summary>
    //public void MatchSweetsControl()
    //{
    //    for (int i = 2; i < xColumn; i += 3) //列号= 2、5、8 
    //        for (int j = 0; j < yRow; j++)  //行号= [0,9] 
    //            MatchSweetsControl(sweets[i, j]);
    //    for (int i = 2; i < yRow; i += 3) //行号= 2、5、8 
    //        for (int j = 0; j < xColumn; j++)  //列号= [0,9] 
    //            MatchSweetsControl(sweets[j, i]);
    //} 
    #endregion


    /// <summary>
    /// 匹配甜品 
    /// 匹配成功规则：
    ///     targetSweet=颜色A =》 颜色A 颜色A any    颜色A any 颜色A
    ///     targetSweet=any   =》 any一侧有两个连续相同的颜色甜品、或者any相邻的两侧相邻甜品颜色相同   
    /// 功能：播放销毁动画，填充甜品，销毁之前匹配成功的甜品,增加分数，再次匹配
    /// 返回值： 是否消除成功
    /// </summary>
    /// <param name="targetSweet"></param>
    /// <param name="canXMatch">允许横向匹配</param>
    /// <param name="canYMatch">允许纵向匹配</param>
    public bool MatchSweets(GameSweet targetSweet, bool canXMatch = true, bool canYMatch = true)
    {
        if (!targetSweet.CanMove() || targetSweet.GetSweetsType == SweetsType.Empty) return false;
        SweetColor.ColorType targetColor = targetSweet.SweetColorComponent.Color;
        bool targetColorIsAny = false;                      //当前甜品颜色是否为any 
        bool targetColorIsSame = false;                      //当前甜品颜色是否为same
        int pos_x = targetSweet.X;  //列
        int pos_y = targetSweet.Y;  //行
        List<GameSweet> xListSweet = new List<GameSweet>();  //横向匹配 用于存放行相同的颜色甜品
        List<GameSweet> yListSweet = new List<GameSweet>();  //纵向匹配 用于存放列相同的颜色甜品

        List<GameSweet> finishedSweet = new List<GameSweet>(); //存放匹配成功(即将销毁)的游戏对象 
                                                               ////尝试再一次匹配  递归当前方法 第二种算法所用的数据
                                                               //int finishedSweet_XminIndex = int.MaxValue;         //存放匹配成功(即将销毁)的游戏对象之中所在的最小列索引
                                                               //int finishedSweet_XmaxIndex = int.MinValue;         //存放匹配成功(即将销毁)的游戏对象之中所在的最大列索引
                                                               //int finishedSweet_YminIndex = int.MaxValue;         //存放匹配成功(即将销毁)的游戏对象之中所在的最小行索引

        //把当前甜品放入 横向、纵向匹配表里
        xListSweet.Add(targetSweet);
        yListSweet.Add(targetSweet);
        if (targetColor == SweetColor.ColorType.Any) targetColorIsAny = true;
        if (targetColor == SweetColor.ColorType.Same) targetColorIsSame = true;

        //横向匹配 &&目标甜品不是any颜色的甜品   && 目标甜品不是same颜色的甜品
        if (canXMatch && !targetColorIsAny && !targetColorIsSame)
        {
            // i=0 目标甜品 左侧 ， i=1 目标甜品 右侧 
            for (int i = 0; i <= 1; i++)
            {
                for (int j = 1; j < xColumn; j++)
                {
                    //目标甜品 左侧
                    if (i == 0)
                    {

                        // Debug.Log(pos_x - j + "," + pos_y + ",目标颜色:" + targetColor + ",目标旁颜色：" + sweets[pos_x - j, pos_y].SweetColorComponent.Color);
                        //目标甜品左侧存在甜品 && 可以移动(带有有MoveSweet脚本) &&目标左侧甜品类型不为空类型 &&  
                        if (pos_x - j >= 0 && sweets[pos_x - j, pos_y].CanMove() && sweets[pos_x - j, pos_y].GetSweetsType != SweetsType.Empty)
                        {
                            //两者甜品颜色一致|| 左侧甜品为ang   
                            if (targetColor == sweets[pos_x - j, pos_y].SweetColorComponent.Color || sweets[pos_x - j, pos_y].SweetColorComponent.Color == SweetColor.ColorType.Any)
                                xListSweet.Add(sweets[pos_x - j, pos_y]);
                            else
                                break;
                        }
                        else
                            break;
                    }
                    //目标甜品 右侧
                    else if (i == 1)
                    {
                        if (pos_x + j < xColumn && sweets[pos_x + j, pos_y].CanMove() && sweets[pos_x + j, pos_y].GetSweetsType != SweetsType.Empty)
                        {
                            if (targetColor == sweets[pos_x + j, pos_y].SweetColorComponent.Color || sweets[pos_x + j, pos_y].SweetColorComponent.Color == SweetColor.ColorType.Any)
                            {
                                xListSweet.Add(sweets[pos_x + j, pos_y]);
                            }
                            else
                                break;
                        }
                        else
                            break;
                    }
                }
            }
        }
        //纵向匹配 &&目标甜品不是any颜色的甜品   && 目标甜品不是same颜色的甜品 
        if (canYMatch && !targetColorIsAny && !targetColorIsSame)
        {
            //i = 0 目标甜品 上侧 ， i = 1 目标甜品 下侧
            for (int i = 0; i <= 1; i++)
            {
                for (int j = 1; j < xColumn; j++)
                {
                    //目标甜品 上侧
                    if (i == 0)
                    {
                        if (pos_y + j < yRow && sweets[pos_x, pos_y + j].CanMove() && sweets[pos_x, pos_y + j].GetSweetsType != SweetsType.Empty)
                        {
                            if (targetColor == sweets[pos_x, pos_y + j].SweetColorComponent.Color || sweets[pos_x, pos_y + j].SweetColorComponent.Color == SweetColor.ColorType.Any)
                                yListSweet.Add(sweets[pos_x, pos_y + j]);
                            else
                                break;
                        }
                        else
                            break;
                    }
                    //目标甜品 下侧
                    else if (i == 1)
                    {
                        if (pos_y - j >= 0 && sweets[pos_x, pos_y - j].CanMove() && sweets[pos_x, pos_y - j].GetSweetsType != SweetsType.Empty)
                        {
                            if (targetColor == sweets[pos_x, pos_y - j].SweetColorComponent.Color || sweets[pos_x, pos_y - j].SweetColorComponent.Color == SweetColor.ColorType.Any)
                                yListSweet.Add(sweets[pos_x, pos_y - j]);
                            else
                                break;
                        }
                        else
                            break;
                    }
                }
            }
        }
        //目标甜品是any颜色的甜品   
        if (targetColorIsAny)
        {
            //横向匹配
            // i=0 目标甜品 左侧 ， i=1 目标甜品 右侧, i=2目标甜品左右侧
            for (int i = 0; i <= 2; i++)
            {
                //记录颜色
                SweetColor.ColorType typeColor = SweetColor.ColorType.Null;
                for (int j = 1; j < xColumn; j++)
                {
                    //左
                    if (i == 0)
                    {
                        //首次遍历
                        //目标甜品左侧存在甜品 && 目标甜品左侧甜品可以移动(带有有MoveSweet脚本) &&目标左侧甜品类型不为空类型 &&  
                        if (pos_x - j >= 0 && sweets[pos_x - j, pos_y].CanMove() && sweets[pos_x - j, pos_y].GetSweetsType != SweetsType.Empty)
                        {
                            //记录左侧第一个的颜色
                            if (j == 1)
                            {
                                typeColor = sweets[pos_x - j, pos_y].SweetColorComponent.Color;
                                xListSweet.Add(sweets[pos_x - j, pos_y]);
                            }
                            else
                            {
                                if (typeColor == sweets[pos_x - j, pos_y].SweetColorComponent.Color)
                                {
                                    xListSweet.Add(sweets[pos_x - j, pos_y]);
                                }
                                else
                                {
                                    //第一边遍历执行 && 判断上侧是甜品是否满足消除条件
                                    if (xListSweet.Count < matchCount)
                                    {
                                        xListSweet.Clear();
                                        xListSweet.Add(targetSweet);
                                        Debug.Log("左：" + xListSweet.Count);
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {
                            //第一边遍历执行 && 判断左侧是甜品是否满足消除条件
                            if (xListSweet.Count < matchCount)
                            {
                                xListSweet.Clear();
                                xListSweet.Add(targetSweet);
                                Debug.Log("左：" + xListSweet.Count);
                            }
                            break;
                        }
                    }
                    //右
                    else if (i == 1)
                    {
                        //目标甜品左侧存在甜品 && 可以移动(带有有MoveSweet脚本) &&目标左侧甜品类型不为空类型 &&  
                        if (pos_x + j < xColumn && sweets[pos_x + j, pos_y].CanMove() && sweets[pos_x + j, pos_y].GetSweetsType != SweetsType.Empty)
                        {
                            //记录左侧第一个的颜色
                            if (j == 1)
                            {
                                typeColor = sweets[pos_x + j, pos_y].SweetColorComponent.Color;
                                xListSweet.Add(sweets[pos_x + j, pos_y]);
                            }
                            else
                            {
                                if (typeColor == sweets[pos_x + j, pos_y].SweetColorComponent.Color)
                                {
                                    xListSweet.Add(sweets[pos_x + j, pos_y]);
                                }
                                else
                                {
                                    //第一边遍历执行 && 判断右侧是甜品是否满足消除条件
                                    if (xListSweet.Count < matchCount)
                                    {
                                        xListSweet.Clear();
                                        xListSweet.Add(targetSweet);
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {  //第一边遍历执行 && 判断上侧是甜品是否满足消除条件
                            if (xListSweet.Count < matchCount)
                            {
                                xListSweet.Clear();
                                xListSweet.Add(targetSweet);
                            }
                            break;
                        }
                    }
                    //左右遍历     。。。颜色A any 颜色A 。。。
                    else if (i == 2)
                    {
                        //存在左右侧甜品
                        if (pos_x - j >= 0 && sweets[pos_x - j, pos_y].CanMove() && sweets[pos_x - j, pos_y].GetSweetsType != SweetsType.Empty &&
                            pos_x + j < xColumn && sweets[pos_x + j, pos_y].CanMove() && sweets[pos_x + j, pos_y].GetSweetsType != SweetsType.Empty)
                        {
                            //首次判断
                            if (j == 1)
                            {
                                //左右两边颜色是否相同
                                if (sweets[pos_x - j, pos_y].SweetColorComponent.Color == sweets[pos_x + j, pos_y].SweetColorComponent.Color)
                                {
                                    xListSweet.Add(sweets[pos_x - j, pos_y]);
                                    xListSweet.Add(sweets[pos_x + j, pos_y]);
                                    typeColor = sweets[pos_x - j, pos_y].SweetColorComponent.Color;
                                    Debug.Log("左右：" + xListSweet.Count);
                                }
                                else
                                    break;
                            }
                            else
                            {
                                if (sweets[pos_x - j, pos_y].SweetColorComponent.Color == typeColor)
                                    xListSweet.Add(sweets[pos_x - j, pos_y]);
                                if (sweets[pos_x + j, pos_y].SweetColorComponent.Color == typeColor)
                                    xListSweet.Add(sweets[pos_x + j, pos_y]);
                                if (sweets[pos_x - j, pos_y].SweetColorComponent.Color != typeColor &&
                                    sweets[pos_x + j, pos_y].SweetColorComponent.Color != typeColor)
                                    break;
                            }
                        }
                    }
                }
            }//end 横向匹配

            //纵向匹配
            // i=0 目标甜品 上侧 ， i=1 目标甜品 下侧, i=2目标甜品上下侧
            for (int i = 0; i <= 2; i++)
            {
                //记录颜色
                SweetColor.ColorType typeColor = SweetColor.ColorType.Null;
                for (int j = 1; j < yRow; j++)
                {
                    //上
                    if (i == 0)
                    {
                        //首次遍历
                        //目标甜品上侧存在甜品 && 可以移动(带有有MoveSweet脚本) &&目标上侧甜品类型不为空类型 &&  
                        if (pos_y + j < yRow && sweets[pos_x, pos_y + j].CanMove() && sweets[pos_x, pos_y + j].GetSweetsType != SweetsType.Empty)
                        {
                            //记录左侧第一个的颜色
                            if (j == 1)
                            {
                                typeColor = sweets[pos_x, pos_y + j].SweetColorComponent.Color;
                                yListSweet.Add(sweets[pos_x, pos_y + j]);
                            }
                            else
                            {
                                if (typeColor == sweets[pos_x, pos_y + j].SweetColorComponent.Color)
                                {
                                    yListSweet.Add(sweets[pos_x, pos_y + j]);
                                }
                                else
                                {
                                    //第一边遍历执行 && 判断上侧是甜品是否满足消除条件
                                    if (yListSweet.Count < matchCount)
                                    {
                                        yListSweet.Clear();
                                        yListSweet.Add(targetSweet);
                                    }
                                    break;
                                }

                            }
                        }
                        else
                        {
                            //第一边遍历执行 && 判断上侧是甜品是否满足消除条件
                            if (yListSweet.Count < matchCount)
                            {
                                yListSweet.Clear();
                                yListSweet.Add(targetSweet);
                            }
                            break;
                        }
                    }
                    //下
                    else if (i == 1)
                    {
                        //目标甜品下侧存在甜品 && 可以移动(带有有MoveSweet脚本) &&目标下侧甜品类型不为空类型 &&  
                        if (pos_y - j >= 0 && sweets[pos_x, pos_y - j].CanMove() && sweets[pos_x, pos_y - j].GetSweetsType != SweetsType.Empty)
                        {
                            //记录左侧第一个的颜色
                            if (j == 1)
                            {
                                typeColor = sweets[pos_x, pos_y - j].SweetColorComponent.Color;
                                yListSweet.Add(sweets[pos_x, pos_y - j]);
                            }
                            else
                            {
                                if (typeColor == sweets[pos_x, pos_y - j].SweetColorComponent.Color)
                                {
                                    yListSweet.Add(sweets[pos_x, pos_y - j]);
                                }
                                else
                                {
                                    //第一边遍历执行 && 判断上侧是甜品是否满足消除条件
                                    if (yListSweet.Count < matchCount)
                                    {
                                        yListSweet.Clear();
                                        yListSweet.Add(targetSweet);
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {
                            //第一边遍历执行 && 判断上侧是甜品是否满足消除条件
                            if (yListSweet.Count < matchCount)
                            {
                                yListSweet.Clear();
                                yListSweet.Add(targetSweet);
                            }
                            break;
                        }
                    }
                    //上下遍历     。。。颜色A any 颜色A 。。。
                    else if (i == 2)
                    {
                        //存在左右侧甜品
                        if (pos_y - j >= 0 && sweets[pos_x, pos_y - j].CanMove() && sweets[pos_x, pos_y - j].GetSweetsType != SweetsType.Empty &&
                            pos_y + j < yRow && sweets[pos_x, pos_y + j].CanMove() && sweets[pos_x, pos_y + j].GetSweetsType != SweetsType.Empty)
                        {
                            //首次判断
                            if (j == 1)
                            {
                                //上下两边颜色是否相同
                                if (sweets[pos_x, pos_y - j].SweetColorComponent.Color == sweets[pos_x, pos_y + j].SweetColorComponent.Color)
                                {
                                    yListSweet.Add(sweets[pos_x, pos_y - j]);
                                    yListSweet.Add(sweets[pos_x, pos_y + j]);
                                    typeColor = sweets[pos_x, pos_y - j].SweetColorComponent.Color;
                                }
                                else
                                    break;
                            }
                            else
                            {
                                if (sweets[pos_x, pos_y - j].SweetColorComponent.Color == typeColor)
                                    yListSweet.Add(sweets[pos_x, pos_y - j]);
                                if (sweets[pos_x, pos_y + j].SweetColorComponent.Color == typeColor)
                                    yListSweet.Add(sweets[pos_x, pos_y + j]);
                                if (sweets[pos_x, pos_y - j].SweetColorComponent.Color != typeColor &&
                                    sweets[pos_x, pos_y + j].SweetColorComponent.Color != typeColor)
                                    break;
                            }
                        }
                    }
                }
            }//end 纵向匹配
        }//end 当前颜色为any
        //目标甜品是same甜品
        if (targetColorIsSame)
        {
            SweetColor.ColorType copyColor = SweetColor.ColorType.Null;
            //找到与之交换的甜品的颜色 
            if (pressedSweet && pressedSweet.SweetColorComponent.Color != SweetColor.ColorType.Same)
                copyColor = pressedSweet.SweetColorComponent.Color;
            else if (enterdSweet && enterdSweet.SweetColorComponent.Color != SweetColor.ColorType.Same)
                copyColor = enterdSweet.SweetColorComponent.Color;
            //添加到列表中
            foreach (GameSweet gs in sweets)
            {
                // 排除“饼干”不可移动 && 同色  
                if (gs.CanSetColor() && gs.SweetColorComponent.Color == copyColor)
                {
                    xListSweet.Add(gs);
                }
            }
        }


        //甜品匹配判定结果
        //横向
        //xListSweet 是否存在any类型的甜品
        bool xListSweetExistAny = false;
        if (xListSweet.Count >= 3)
        {
            for (int i = 0; i < xListSweet.Count; i++)
            {
                if (xListSweet[i].SweetColorComponent.Color == SweetColor.ColorType.Any)
                {
                    xListSweetExistAny = true;
                    break;
                }
            }
            if (!xListSweetExistAny)
            {
                for (int i = 0; i < xListSweet.Count; i++)
                {
                    //放入匹配成功列表 准备销毁 
                    finishedSweet.Add(xListSweet[i]);
                }
            }
            else //横向 带有any甜品匹配成功
            {
                for (int i = 0; i < xColumn; i++)
                {
                    //加入any甜品所在的一整行
                    finishedSweet.Add(sweets[i, targetSweet.Y]);
                }
            }
        }
        //纵向
        //yListSweet 是否存在any类型的甜品
        bool yListSweetExistAny = false;
        if (yListSweet.Count >= 3)
        {
            for (int i = 0; i < yListSweet.Count; i++)
            {
                if (yListSweet[i].SweetColorComponent.Color == SweetColor.ColorType.Any)
                {
                    yListSweetExistAny = true;
                    break;
                }
            }
            if (!yListSweetExistAny)
            {
                for (int i = 0; i < yListSweet.Count; i++)
                {
                    //放入匹配成功列表 准备销毁 
                    finishedSweet.Add(yListSweet[i]);
                }
            }
            else
            {
                for (int i = 0; i < yRow; i++)
                {
                    //加入any甜品所在的一整列
                    finishedSweet.Add(sweets[targetSweet.X, i]);
                }
            }
        }
        //横向、纵向任意匹配成功执行
        if (xListSweet.Count >= matchCount || yListSweet.Count >= matchCount)
        {
            //1.处理已匹配成功的甜点
            if (targetColorIsAny || xListSweetExistAny || yListSweetExistAny)
                DisposeFinishedSweet(finishedSweet, true, false);
            else if (targetColorIsSame)
                DisposeFinishedSweet(finishedSweet, false, true);
            else
                DisposeFinishedSweet(finishedSweet);


            //2.增加分数UI   
            Debug.Log("本次匹配成功的个数：" + finishedSweet.Count);
            UIManager.GetInstance.AddCount(finishedSweet.Count);

            //3.一次性消除甜品 大于三个生成新甜品 
            if (finishedSweet.Count > matchCount && targetColor != SweetColor.ColorType.Any && targetColor != SweetColor.ColorType.Same)
            {
                //随机位置产生  可行列匹配甜点
                GameSweet gs = finishedSweet[Random.Range(0, finishedSweet.Count)];
                //当前消除总数为2的整数倍
                if (UIManager.GetInstance.CurEndCount % 2 == 0)
                {
                    //可匹配所有同色甜点 
                    CreateNewSweet(gs.X, gs.Y, SweetsType.Same);
                }
                else
                {
                    //可行列匹配甜点 
                    CreateNewSweet(gs.X, gs.Y, SweetsType.any);
                }
            }


            ////遍历已经匹配成功的甜点 
            //for (int i = 0; i < finishedSweet.Count; i++)
            //{
            //    //记录所在的最小/大列索引   最小行索引    用于尝试再一次匹配算法2
            //    if (finishedSweet[i].X > finishedSweet_XmaxIndex)
            //        finishedSweet_XmaxIndex = finishedSweet[i].X;
            //    if (finishedSweet[i].X < finishedSweet_XminIndex)
            //        finishedSweet_XminIndex = finishedSweet[i].X;
            //    if (finishedSweet[i].Y < finishedSweet_YminIndex)
            //        finishedSweet_YminIndex = finishedSweet[i].Y;
            //}

            //3.自动补充
            StartCoroutine(WaitSomeTimeTodo(0.5f, () => StartCoroutine(AllFill())));

            //4.尝试再一次匹配  递归当前方法
            //算法1 
            MatchImportantSweet(1f, null, true);
            //算法2--- 效率高但是耦合性也高  弃用
            //如果纵向匹配成功 则只匹配那条纵向所有甜点
            //若果横向匹配成功 则匹配左右两个端点的所在纵向所有甜点 以及 匹配成功甜品的下一横行若干节点  
            #region 算法2
            ////仅纵向匹配成功
            //if (finishedSweet_XmaxIndex == finishedSweet_XminIndex)
            //{
            //    for (int y = finishedSweet_YminIndex; y < yRow; y++)
            //    {
            //        //只进行 横向匹配
            //        MatchImportantSweet(0.6f, sweets[finishedSweet_XminIndex, y], false, true, false);
            //    }
            //    //destroySweet_YminIndex的同列下一行 进行纵向匹配
            //    if (finishedSweet_YminIndex - 1 >= 0)
            //    {
            //        MatchImportantSweet(0.6f, sweets[finishedSweet_XminIndex, finishedSweet_YminIndex - 1], false, true, false);
            //    }
            //}
            ////横向匹配成功 或者横向纵向都匹配成功(L形,T形)
            //else
            //{
            //    for (int y = 0; y < yRow; y++)
            //    {
            //        //左端点所在列的所有甜点  只进行横向匹配
            //        MatchImportantSweet(0.6f, sweets[finishedSweet_XminIndex, y], false, true, false);

            //        //右端点所在列的所有甜点   只进行横向匹配
            //        MatchImportantSweet(0.6f, sweets[finishedSweet_XmaxIndex, y], false, true, false);
            //    }
            //    //匹配成功甜品下一横行若干节点 只进行纵向匹配
            //    for (int x = finishedSweet_XminIndex; x <= finishedSweet_XmaxIndex; x++)
            //    {
            //        int downRow = targetSweet.Y - 1;
            //        if (downRow >= 0)
            //            MatchImportantSweet(0.6f, sweets[x, downRow], false, false, true);
            //    }
            //}
            #endregion


            return true;
        }
        else
            return false; //匹配失败 
    }



    /// <summary>
    /// 延时调用
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitSomeTimeTodo(float timer, UnityAction action = null)
    {
        yield return new WaitForSeconds(timer);
        if (action != null)
            action.Invoke();
    }

    /// <summary>
    /// 匹配重要节点甜点   批量回调MatchSweets()方法
    /// [0,xColumn] [0,yRow]  索引号为第2，5，8行，索引号为第2，5，8列
    /// </summary>
    /// <param name="waitTime">等待一会执行</param>
    /// <param name="gameSweet1">匹配某个甜点实体 <GameSweet></param>
    /// <param name="matchImportantSweet">匹配所有关键甜点实体 </param>
    private void MatchImportantSweet(float waitTime, GameSweet gameSweet1 = null, bool matchImportantSweet = false, bool canXMatch = true, bool canYMatch = true)
    {
        StartCoroutine(WaitSomeTimeTodo(waitTime, () =>
        {
            if (gameSweet1 != null)
            {
                MatchSweets(gameSweet1, canXMatch, canYMatch);
            }
            else if (matchImportantSweet)
            {
                //匹配所有节点
                //foreach (GameSweet sweet in gameSweet2)
                //{
                //    MatchSweets(sweet);
                //} 
                //匹配关键节点的甜点
                for (int i = 2; i < xColumn; i += 3) //列号= 2、5、8 
                    for (int j = 0; j < yRow; j++)  //行号= [0,9] 
                    {
                        MatchSweets(sweets[i, j]);
                        //MatchSweetsControl(sweets[i, j]);
                    }
                for (int i = 2; i < yRow; i += 3) //行号= 2、5、8 
                    for (int j = 0; j < xColumn; j++)  //列号= [0,9] 
                    {
                        MatchSweets(sweets[j, i]);
                        //MatchSweetsControl(sweets[j, i]);
                    }
            }
            //允许玩家移动甜点
            userCanMoveSweet = true;
        }
          ));
    }
    /// <summary>
    /// 处理已经完成匹配的甜点
    /// 1.对该处理的甜点 播放销毁动画，延时销毁操作
    /// 2.遍历周围是否有障碍“饼干”，有则使前者 播放销毁动画，延时销毁操作
    /// </summary>
    private void DisposeFinishedSweet(List<GameSweet> finishedSweet, bool isAnySweet = false, bool isSameSweet = false)
    {
        //遍历已经匹配成功的甜点 
        for (int i = 0; i < finishedSweet.Count; i++)
        {
            //把甜品类型 设为空，后续才可自动填充
            finishedSweet[i].Init(finishedSweet[i].X, finishedSweet[i].Y, SweetsType.Empty);
            //播放销毁动画 延时执行销毁
            DestroySweet destroySweet = finishedSweet[i].DestroySweetComponent;
            destroySweet.CanClaer = true;
            //可清除 且  不处于清除状态
            if (destroySweet.CanClaer && !destroySweet.IsClearing)
                destroySweet.DestroyFinishSweet(finishedSweet[i]); //延时销毁
            //播放声音
            if (isAnySweet)
                AudioManager.GetInstance.PlayAudio(AudioManager.AudioType.Destroy_Any);
            else if (isSameSweet)
                AudioManager.GetInstance.PlayAudio(AudioManager.AudioType.Destroy_Same);
            else
                AudioManager.GetInstance.PlayAudio(AudioManager.AudioType.Destroy_Normal);
             
            //对障碍进行操作
            //查询当前甜点周围是否有 不可消除的“饼干”，有则消除
            int sweetLeft = finishedSweet[i].X - 1;
            int sweetRight = finishedSweet[i].X + 1;
            int sweetUp = finishedSweet[i].Y + 1;
            int sweetDown = finishedSweet[i].Y - 1;
            //当前甜点的周围存在（不越界） && 周围类型为障碍
            //左
            if (sweetLeft >= 0 && sweets[sweetLeft, finishedSweet[i].Y].GetSweetsType == SweetsType.barrier)
            {
                //当前障碍“饼干”
                GameSweet gs = sweets[sweetLeft, finishedSweet[i].Y];
                //把其类型 设为空，后续才可自动填充
                gs.Init(gs.X, gs.Y, SweetsType.Empty);
                //播放销毁动画 延时执行销毁
                DestroySweet ds = gs.DestroySweetComponent;
                ds.CanClaer = true;
                //可清除 且  不处于清除状态
                if (ds.CanClaer && !ds.IsClearing)
                    ds.DestroyFinishSweet(gs); //延时销毁
            }
            //右
            if (sweetRight < xColumn && sweets[sweetRight, finishedSweet[i].Y].GetSweetsType == SweetsType.barrier)
            {
                GameSweet gs = sweets[sweetRight, finishedSweet[i].Y];
                gs.Init(gs.X, gs.Y, SweetsType.Empty);
                //播放销毁动画 延时执行销毁
                DestroySweet ds = gs.DestroySweetComponent;
                ds.CanClaer = true;
                //可清除 且  不处于清除状态
                if (ds.CanClaer && !ds.IsClearing)
                    ds.DestroyFinishSweet(gs); //延时销毁 
            }
            //上
            if (sweetUp < yRow && sweets[finishedSweet[i].X, sweetUp].GetSweetsType == SweetsType.barrier)
            {
                GameSweet gs = sweets[finishedSweet[i].X, sweetUp];
                gs.Init(gs.X, gs.Y, SweetsType.Empty);
                //播放销毁动画 延时执行销毁
                DestroySweet ds = gs.DestroySweetComponent;
                ds.CanClaer = true;
                //可清除 且  不处于清除状态
                if (ds.CanClaer && !ds.IsClearing)
                    ds.DestroyFinishSweet(gs); //延时销毁 
            }
            //下
            if (sweetDown >= 0 && sweets[finishedSweet[i].X, sweetDown].GetSweetsType == SweetsType.barrier)
            {
                GameSweet gs = sweets[finishedSweet[i].X, sweetDown];
                gs.Init(gs.X, gs.Y, SweetsType.Empty);
                //播放销毁动画 延时执行销毁
                DestroySweet ds = gs.DestroySweetComponent;
                ds.CanClaer = true;
                //可清除 且  不处于清除状态
                if (ds.CanClaer && !ds.IsClearing)
                    ds.DestroyFinishSweet(gs); //延时销毁 
            }
        }



    }

    #endregion

    #region 游戏结束 重启逻辑

    public void GameOver()
    {
        userCanMoveSweet = false;
        Debug.Log("GameManager.GameOver");
        StopAllCoroutines();
        StartCoroutine(WaitSomeTimeTodo(2f, () => Debug.Log("GameOver:" + userCanMoveSweet)));

    }

    public void RestartGame()
    {
        userCanMoveSweet = true;
        Debug.Log("GameManager.RestartGame");
        //消除之前所有甜品
        foreach (GameSweet gs in sweets)
        {
            try
            {
                gs.GetComponent<Animator>().Play("SweetDestroy");
                Destroy(gs.gameObject, 1f);
            }
            catch (System.Exception)
            {
            }
        }
        Debug.Log("正在重新启动中");
        StartCoroutine(WaitSomeTimeTodo(1f, () =>
        {
            sweets = new GameSweet[xColumn, yRow];
            //初始化甜品实例在网格上
            for (int i = 0; i < xColumn; i++)
            {
                for (int j = 0; j < yRow; j++)
                {
                    CreateNewSweet(i, j, SweetsType.Empty);
                }
            }
            StartCoroutine(AllFill());
            //创建障碍 
            CreateBarrier(Random.Range(3, 10));
            //尝试匹配所有关键节点的甜点   
            MatchImportantSweet(2f, null, true);
        }
        ));
    }

    #endregion

    void Update()
    {
        //test填充算法
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.timeScale == 0)
                Time.timeScale = 1;
            else
                Time.timeScale = 0;
        }
    }

}
