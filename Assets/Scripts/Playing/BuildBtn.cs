using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,  IPointerClickHandler
{
    public string m_name;
    public Image image;
    public BuildingManagerData data;
    protected GameObject m_MessageShow;
    protected Text m_MessageFoodText;
    protected Text m_MessageWoodText;
    protected Text m_MessageStoneText;
    protected Text m_MessageBookText;
    protected MasterBase m_PlayerMasterBase;
    protected GameObject m_Player;

    void Awake()
    {
        image = transform.Find("Image").GetComponent<Image>();
        m_MessageShow = GameObject.Find("UiCanvas/PlayerView/FunctionMenu/BuildMenu/MessageShow");
        m_MessageFoodText = m_MessageShow.transform.Find("Food").GetComponent<Text>();
        m_MessageWoodText = m_MessageShow.transform.Find("Wood").GetComponent<Text>();
        m_MessageStoneText = m_MessageShow.transform.Find("Stone").GetComponent<Text>();
        m_MessageBookText = m_MessageShow.transform.Find("Book").GetComponent<Text>();
    }

    void OnEnable()
    {
        m_MessageShow.SetActive(false);
    }

    void Start()
    {
        m_Player = GameObject.Find("Master/Player");
        m_PlayerMasterBase = GameObject.Find("Master/Player").GetComponent<MasterBase>();
        //BuildingManager.GetInstance().GetDataById(2001,out data);
        //image.sprite = data.data.icon;
    }

    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_MessageShow.SetActive(true);
        m_MessageShow.transform.Find("Name").GetComponent<Text>().text = data.data.name;
        m_MessageShow.transform.Find("Message").GetComponent<Text>().text = data.data.descripion;
        
        m_MessageFoodText.text = data.data.food.ToString();
        if (data.data.food > m_PlayerMasterBase.m_Resource.food)
        {
            m_MessageFoodText.color = Color.red;
        }
        else
        {
            m_MessageFoodText.color = Color.black;
        }
        m_MessageWoodText.text = data.data.wood.ToString();
        if (data.data.wood > m_PlayerMasterBase.m_Resource.wood)
        {
            m_MessageWoodText.color = Color.red;
        }
        else
        {
            m_MessageWoodText.color = Color.black;
        }
        m_MessageStoneText.text = data.data.stone.ToString();
        if (data.data.stone > m_PlayerMasterBase.m_Resource.stone)
        {
            m_MessageStoneText.color = Color.red;
        }
        else
        {
            m_MessageStoneText.color = Color.black;
        }
        m_MessageBookText.text = data.data.book.ToString();
        if (data.data.book > m_PlayerMasterBase.m_Resource.book)
        {
            m_MessageBookText.color = Color.red;
        }
        else
        {
            m_MessageBookText.color = Color.black;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_MessageShow.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        m_Player.GetComponent<Player>().Build(data);
    }

    public void InitData(BuildingManagerData _data)
    {
        data = _data;
        image.sprite = _data.data.icon;
    }
}
