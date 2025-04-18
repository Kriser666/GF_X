﻿
using UnityEngine;

/// <summary>
/// 热更Const
/// </summary>
public static partial class Const
{
    internal const long DefaultVibrateDuration = 50;//安卓手机震动强度
    internal static readonly float SHOW_CLOSE_INTERVAL = 1f;//出现关闭按钮的延迟

    public static readonly string HORIZONTAL = "Horizontal";
    public static readonly bool RepeatLevel = true;//是否循环关卡

    // 主页面常量
    public static readonly string VEHICLE_ID = "VEHICLE_ID";
    public static readonly string MODIFY_ID = "MODIFY_ID";
    public static readonly string RAW_IMAGE = "RAW_IMAGE";
    public static readonly string SET_RAW_IMAGE_CALLBACK = "SET_RAW_IMAGE_CALLBACK";

    // 游戏中常量
    public static readonly string PART_ID_LIST = "PART_ID_LIST";

    public static readonly string ADDITION_SYMBOL = "+";
    public static readonly string SUBTRACTION_SYMBOL = "-";
    // 改装详情常量
    public static readonly string CUR_TOTAL_POWER = "CUR_TOTAL_POWER";
    public static readonly string CUR_TOTAL_BRAKE = "CUR_TOTAL_BRAKE";
    public static readonly string CUR_TOTAL_ACCELERATION = "CUR_TOTAL_ACCELERATION";
    // Url参数
    public static readonly string URL_PARAMS = "URL_PARAMS";
    // 存档的请求地址
    public static readonly string SAVED_GAMES_URL = "{0}";
    // 开始改装、历史改装的字体颜色
    public static readonly Color TEXT_ADD_COLOR = new(242f / 255f, 132f / 255f, 28f / 255f);
}
