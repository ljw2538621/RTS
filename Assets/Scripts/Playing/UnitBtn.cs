using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    //public string m_name;
    public GameObject parentObject;
    public Image image;
    public int id;
    protected UnitData m_UnitData;
    protected GameObject m_MessageShow;
    protected Text m_MessageFoodText;
    protected Text m_MessageWoodText;
    protected Text m_MessageStoneText;
    protected Text m_MessageBookText;
    //protected MasterBase m_PlayerMasterBase;
    protected GameObject m_Player;

    void Awake()
    {
        image = transform.Find("Image").GetComponent<Image>();
    }

    void OnEnable()
    {
        //m_MessageShow.SetActive(false);
    }

    void Start()
    {
        m_MessageShow = transform.Find("../../../MessageShow").gameObject;
        m_MessageFoodText = m_MessageShow.transform.Find("Food").GetComponent<Text>();
        m_MessageWoodText = m_MessageShow.transform.Find("Wood").GetComponent<Text>();
        m_MessageStoneText = m_MessageShow.transform.Find("Stone").GetComponent<Text>();
        m_MessageBookText = m_MessageShow.transform.Find("Book").GetComponent<Text>();

        m_Player = GameObject.Find("Master/Player");
        //m_PlayerMasterBase = GameObject.Find("Master/Player").GetComponent<MasterBase>();
    }

    void Update()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_MessageShow.SetActive(true);
        m_MessageShow.transform.Find("Name").GetComponent<Text>().text = m_UnitData.name;
        m_MessageShow.transform.Find("Message").GetComponent<Text>().text = m_UnitData.descripion;

        MasterBase m_PlayerMasterBase = m_Player.GetComponent<MasterBase>();
        m_MessageFoodText.text = m_UnitData.food.ToString();
        if (m_UnitData.food > m_PlayerMasterBase.m_Resource.food)
        {
            m_MessageFoodText.color = Color.red;
        }
        else
        {
            m_MessageFoodText.color = Color.black;
        }
        m_MessageWoodText.text = m_UnitData.wood.ToString();
        if (m_UnitData.wood > m_PlayerMasterBase.m_Resource.wood)
        {
            m_MessageWoodText.color = Color.red;
        }
        else
        {
            m_MessageWoodText.color = Color.black;
        }
        m_MessageStoneText.text = m_UnitData.stone.ToString();
        if (m_UnitData.stone > m_PlayerMasterBase.m_Resource.stone)
        {
            m_MessageStoneText.color = Color.red;
        }
        else
        {
            m_MessageStoneText.color = Color.black;
        }
        m_MessageBookText.text = "0";
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_MessageShow.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        parentObject.GetComponent<CreateUnitMenuBase>().AddCreateUnit(id);
    }

    public void InitData(int _id,Sprite sprite , UnitData unitData)
    {
        id = _id;
        image.sprite = sprite;
        m_UnitData = unitData;
    }
}
