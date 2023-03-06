using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum OwnerID
{
    OWNER_NONE,
    OWNER_PLAYER,
    OWNER_RED
}

public enum OwnerColor
{
    OWNER_RED,
    OWNER_GREED,
    OWNER_BLUE
}

public enum FactionID
{
    FID_NONE,
    FID_A,
    FID_B,
    FID_C,
    FID_D,
    FID_E,
    FID_F
}

public enum GameState
{
    GS_NONE,
    GS_PALYING,
    GS_PAUSE
}

public struct InitializationData        // 游戏初始数据
{
    public int food;
    public int wood;
    public int stone;
    public int book;
    public float resourceRate;
    public int villagerNum;

    public InitializationData(int f,int w,int s,int b,float rate,int vNum)
    {
        food = f;
        wood = w;
        stone = s;
        book = b;
        resourceRate = rate;
        villagerNum = vNum;
    }
}

public class Global
{
    static public OwnerColor PlayerColor;
    static public bool IsCanMoveCamera;
    static public GameState gameState = GameState.GS_NONE;
    static public InitializationData initData = new InitializationData(200, 200, 200, 0, 1.0f,3);

    // 声音
    static public AudioClip Clip_ChopWood = Resources.Load<AudioClip>("Audio/ChopWood");
    static public AudioClip Clip_FoodCollect = Resources.Load<AudioClip>("Audio/FoodCollect");
    static public AudioClip Clip_StoneHit = Resources.Load<AudioClip>("Audio/StoneHit");
    static public AudioClip Clip_Construction = Resources.Load<AudioClip>("Audio/Construction-SFX");
    static public AudioClip Clip_Error = Resources.Load<AudioClip>("Audio/Error");
    static public AudioClip Clip_SendToBuild = Resources.Load<AudioClip>("Audio/SendToBuild");
    static public AudioClip Clip_SendToChopWood = Resources.Load<AudioClip>("Audio/SendToChopWood");
    static public AudioClip Clip_SendToGetStone = Resources.Load<AudioClip>("Audio/SendToGetStone");


    // 场景数据初始化 在场景加载之前
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        BuildingManager.GetInstance().InitDictionary();
        UnitManager.GetInstance().InitDictionary();
        ResourceManager.GetInstance().InitDictionary();
    }

    static public string LoadSceneName
    {
        get;
        protected set;
    }
    static public void LoadScene(string sceneName)
    {
        LoadSceneName = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }
}
