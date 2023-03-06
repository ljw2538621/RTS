using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingMenu : MonoBehaviour
{
    protected List<GameObject> m_ChildList;
    protected GameObject m_Menu;

    void Awake()
    {
        m_ChildList = new List<GameObject>();
        int length = transform.childCount;
        for (int i = 0; i < length; i++)
        {
            m_ChildList.Add(transform.GetChild(i).gameObject);
        }

        m_Menu = transform.Find("Menu").gameObject;
    }

    void OnEnable()
    {
        int length = m_ChildList.Count;
        for (int i = 0; i < length; i++)
        {
            m_ChildList[i].SetActive(false);
        }

        m_Menu.SetActive(true);
        Global.gameState = GameState.GS_PAUSE;
    }

    void Start()
    {
        m_Menu.transform.Find("ContinueBtn").GetComponent<Button>().onClick.AddListener(OnContinueBtnClicked);
        m_Menu.transform.Find("SettingBtn").GetComponent<Button>().onClick.AddListener(OnSettingBtnClicked);
        m_Menu.transform.Find("ReturnMainMenuBtn").GetComponent<Button>().onClick.AddListener(OnReturnMainMenuBtnClicked);
    }

    void Update()
    {
        
    }

    public void OnContinueBtnClicked()
    {
        Global.gameState = GameState.GS_PALYING;
        gameObject.SetActive(false);
    }

    public void OnSettingBtnClicked()
    {

    }

    public void OnReturnMainMenuBtnClicked()
    {
        Global.LoadScene("MainMenu");
    }
}
