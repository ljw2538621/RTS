using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateUnitMenuBase : MonoBehaviour
{
    protected GameObject m_Content;
    protected GameObject m_BtnPrefabas;
    protected GameObject m_WaitMenu;
    protected List<UnitNode> m_CreateList;
    protected GameObject[] m_WaitBtn;
    protected List<GameObject> m_WaitBtnList;
    public GameObject m_Owner;

    protected void BaseAwake()
    {
        m_Content = transform.Find("Viewport/Content").gameObject;
        m_BtnPrefabas = Resources.Load<GameObject>("Prefabs/Ui/UnitBtn");

        m_WaitMenu = transform.Find("WaitMenu").gameObject;
        m_WaitBtn = new GameObject[5];
        m_WaitBtnList = new List<GameObject>();
        GameObject waitBtnPre = Resources.Load<GameObject>("Prefabs/Ui/WaitBtn");
        for (int i = 0; i < 5; i++)
        {
            GameObject go = GameObject.Instantiate(waitBtnPre);
            go.GetComponent<WaitBtn>().id = i;
            go.GetComponent<WaitBtn>().SetParentObject(gameObject);
            go.transform.SetParent(m_WaitMenu.transform);
            m_WaitBtn[i] = go;
        }
    }

    protected void BaseOnEnable()
    {
        transform.Find("MessageShow").gameObject.SetActive(false);
    }

    protected void BaseStart()
    {
        //TextAsset textAsset = Resources.Load<TextAsset>("Data/CapitalData");
        //string[] allline = textAsset.text.Split('\n');
        //int itemNum = allline.Length;

        //for (int iN = 1; iN < itemNum; ++iN)
        //{
        //    int id = int.Parse(allline[iN]);
        //    UnitManagerData mainData;
        //    UnitManager.GetInstance().GetUnitDataById(id, out mainData);
        //    GameObject btn = GameObject.Instantiate(m_BtnPrefabas);
        //    btn.GetComponent<UnitBtn>().InitData(mainData.id, mainData.data.icon, mainData.data);
        //    btn.transform.SetParent(m_Content.transform);
        //}
    }

    protected void BaseUpdate()
    {

    }

    protected void BaseLateUpdate()
    {
        if (m_Owner != null)
        {
            int length = m_CreateList.Count;
            for (int i = 0; i < 5; ++i)
            {
                if (i < length)
                {
                    m_WaitBtn[i].SetActive(true);
                    m_WaitBtn[i].GetComponent<WaitBtn>().SetData(m_CreateList[i].icon);
                    m_WaitBtn[i].GetComponent<WaitBtn>().m_Blood.fillAmount = m_CreateList[i].passTime / m_CreateList[i].finishTime;
                }
                else
                {
                    if (m_WaitBtn[i].activeSelf)
                    {
                        m_WaitBtn[i].SetActive(false);
                    }
                }
            }
        }
    }

    protected void BaseOnDisable()
    {
        m_Owner = null;
    }

    public void OpenMenu(GameObject other)
    {
        m_Owner = other;
        m_CreateList = m_Owner.GetComponent<BuildingBase>().GetCreateList();
        gameObject.SetActive(true);
    }

    public void CancelCreateUnit(int index)
    {
        m_Owner.GetComponent<BuildingBase>().CancelCreateUnitNode(index);
    }

    public void AddCreateUnit(int id)
    {
        m_Owner.GetComponent<BuildingBase>().AddCreateUnitNode(id);
    }
}
