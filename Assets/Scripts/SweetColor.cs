using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 甜品的颜色面片(sprite)
/// </summary>
public class SweetColor : MonoBehaviour
{
    public enum ColorType
    {
        Yellow,
        purple,
        pink,
        Red,
        Blue,
        Green, 
        /// <summary>
        /// 任意颜色
        /// </summary>
        Any,
        /// <summary>
        /// 同色消除
        /// </summary>
        Same,
        Null
    }

    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    }
    public ColorSprite[] colorSprites;
    private Dictionary<ColorType, Sprite> spriteDic;


    private SpriteRenderer spriteRenderer;  //面片SpriteRenderer组件
    public ColorType color;                //面片颜色
    public int NumColors
    {
        get
        {
            return colorSprites.Length;
        }
    }

    public ColorType Color
    {
        get
        {
            return color;
        }
        set
        {
            SetColor(value); 
        }
    }
    

    public void Awake()
    {
        Init();
    }

    public void Init()
    {
        spriteRenderer = transform.Find("Normal").GetComponent<SpriteRenderer>();
        spriteDic = new Dictionary<ColorType, Sprite>();
        //配置字典
        for (int i = 0; i < colorSprites.Length; i++)
        {
            if (!spriteDic.ContainsKey(colorSprites[i].color))
            {
                spriteDic.Add(colorSprites[i].color, colorSprites[i].sprite);
            }
        }
    }

    public void SetColor(ColorType newColor)
    { 
        if (spriteDic.ContainsKey(newColor))
        { 
            spriteRenderer.sprite = spriteDic[newColor];
            color = newColor;
        }
            
    }
}
