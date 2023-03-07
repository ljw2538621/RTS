using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public struct BuildingData
{
    public int id;
    public string name;
    public string descripion;                   // 描述
    public float hp;                                // 血量
    public float maxhp;
    public FactionID factionID;         // 阵营id
    public float range;
    public float data1;
    public float data2;
    public float data3;
    public float data4;

    // 所需资源
    public int food;

    public int wood;
    public int stone;
    public int book;
    public Sprite icon;
}

public enum BuildingState
{
    BS_NONE,
    BS_CREATE,                  // 选择位置建造
    BS_ISNOCREATE,         // 不能建造
    BS_BUILDING,            // 正在建造
    BS_NORMAL               // 可以使用
}

public struct UnitNode
{
    public int id;
    public float passTime;
    public float finishTime;
    public Sprite icon;
}

public class BuildingBase : MonoBehaviour
{
    public GameObject m_Master;
    protected bool m_IsBuilded;
    protected bool m_IsSelect;

    public bool m_IsLive
    {
        get;
        protected set;
    }

    protected BuildingData m_data;
    protected GameObject m_Plane;
    protected GameObject m_Model;
    protected GameObject[] m_WorkerArr;
    protected GameObject m_UnitSpawn;
    protected GameObject m_GotoPos;
    protected GameObject[] m_BuildingStateObjs;
    protected GameObject m_BloodBar;
    protected NavMeshObstacle m_Obstacle;
    protected AudioSource m_AudioSource;

    public BuildingState m_State
    {
        get;
        protected set;
    }

    protected float m_ModelHeight;
    protected GameObject m_MessageMenu;
    protected List<UnitNode> m_CreateList;
    protected int[] m_UnitIdArr;
    protected bool m_IsCanCreateUnit;

    protected void BaseAwake()
    {
        m_MessageMenu = GameObject.Find("UiCanvas/PlayerView/MessageMenu");
        m_BloodBar = transform.Find("BloodBar").gameObject;
        int num = transform.Find("BuildingStateObjs").childCount;
        m_BuildingStateObjs = new GameObject[num];
        for (int i = 0; i < num; i++)
        {
            m_BuildingStateObjs[i] = transform.Find("BuildingStateObjs").GetChild(i).gameObject;
            m_BuildingStateObjs[i].SetActive(false);
        }
        m_Plane = transform.Find("Plane").gameObject;
        m_UnitSpawn = transform.Find("UnitSpawn").gameObject;
        m_Obstacle = GetComponent<NavMeshObstacle>();
        m_AudioSource = GetComponent<AudioSource>();
        m_IsBuilded = true;
        m_State = BuildingState.BS_NONE;
        num = transform.Find("WorkerPositions").childCount;
        m_WorkerArr = new GameObject[num];
        for (int i = 0; i < num; i++)
        {
            m_WorkerArr[i] = null;
        }

        m_CreateList = new List<UnitNode>();
        m_IsCanCreateUnit = false;
    }

    protected void BaseOnEnable()
    {
        m_IsLive = true;
        m_Obstacle.carving = false;
        m_Obstacle.enabled = false;
    }

    protected void BaseStart()
    {
    }

    protected void BaseUpdate()
    {
        if (m_IsLive)
        {
            switch (m_State)
            {
                case BuildingState.BS_NONE:
                break;

                case BuildingState.BS_CREATE:
                break;

                case BuildingState.BS_ISNOCREATE:
                break;

                case BuildingState.BS_BUILDING:
                {
                    if (m_data.hp >= m_data.maxhp)
                    {
                        m_data.hp = m_data.maxhp;
                        transform.Find("UnderConstruction").gameObject.SetActive(false);
                        Vector3 v3 = m_Model.transform.localPosition;
                        v3.y = 0.0f;
                        m_Model.transform.localPosition = v3;
                        m_State = BuildingState.BS_NORMAL;
                        for (int i = 0; i < m_WorkerArr.Length; i++)
                        {
                            if (m_WorkerArr[i] != null)
                            {
                                m_WorkerArr[i].GetComponent<UnitBase>().SetIdleState();
                                m_WorkerArr[i] = null;
                            }
                        }
                    }
                    else
                    {
                        Vector3 v3 = m_Model.transform.localPosition;
                        v3.y = -m_ModelHeight * ((m_data.maxhp - m_data.hp) / m_data.maxhp);
                        m_Model.transform.localPosition = v3;
                    }
                }
                break;

                case BuildingState.BS_NORMAL:
                {
                    BuildingNormalActionUpdate();
                }
                break;

                default:
                break;
            }
        }
    }

    protected void BaseLateUpdate()
    {
    }

    public GameObject GetMaster()
    {
        return m_Master;
    }

    public void SetMaster(GameObject master)
    {
        m_Master = master;
    }

    public virtual void SetState(BuildingState state)
    {
        switch (state)
        {
            case BuildingState.BS_NONE:
            break;

            case BuildingState.BS_CREATE:
            break;

            case BuildingState.BS_ISNOCREATE:
            break;

            case BuildingState.BS_BUILDING:
            {
                m_data.hp = 0.0f;
                transform.Find("UnderConstruction").gameObject.SetActive(true);
                Vector3 v3 = m_Model.transform.localPosition;
                v3.y = -m_ModelHeight;
                m_Model.transform.localPosition = v3;

                m_Obstacle.enabled = true;
                m_Obstacle.carving = true;
            }
            break;

            case BuildingState.BS_NORMAL:
            break;

            default:
            break;
        }
        m_State = state;
    }

    public virtual bool AddHp(float add)
    {
        if (m_data.hp >= m_data.maxhp)
        {
            return false;
        }
        else
        {
            if (m_State == BuildingState.BS_BUILDING)
            {
                m_data.hp += add / 2.0f;
                if (m_data.hp >= m_data.maxhp)
                {
                    m_data.hp = m_data.maxhp;
                }
            }
            else
            {
                m_data.hp += add;
                if (m_data.hp >= m_data.maxhp)
                {
                    m_data.hp = m_data.maxhp;
                }
                if (m_State == BuildingState.BS_NORMAL)
                {
                    int length = m_BuildingStateObjs.Length + 1;
                    int index = (int)((1.0f - m_data.hp / m_data.maxhp) * (float)length);
                    for (int i = index; i < length - 1; ++i)
                    {
                        if (m_BuildingStateObjs[i].activeSelf)
                        {
                            m_BuildingStateObjs[i].SetActive(false);
                        }
                    }
                }
            }
        }
        RefreshBloodBar();
        return true;
    }

    public virtual bool BeAttacked(float value)
    {
        m_data.hp -= value;
        if (m_data.hp <= 0)
        {
            m_data.hp = 0;
            m_IsLive = false;
            RefreshBloodBar();
            return false;
        }
        else
        {
            if (m_State == BuildingState.BS_NORMAL)
            {
                int length = m_BuildingStateObjs.Length + 1;
                int index = (int)((1.0f - m_data.hp / m_data.maxhp) * (float)length);
                for (int i = 0; i < index; ++i)
                {
                    if (!m_BuildingStateObjs[i].activeSelf)
                    {
                        m_BuildingStateObjs[i].SetActive(true);
                    }
                }
            }
            RefreshBloodBar();
            return true;
        }
    }

    public BuildingData GetBuildingData()
    {
        return m_data;
    }

    public void SetBuildingData(BuildingData data)
    {
        m_data = data;
    }

    public void SetBloodBarMaterial(bool isPlayer)
    {
        m_BloodBar.GetComponent<BloodBar>().SetBarMaterial(isPlayer);
    }

    public void RefreshBloodBar()
    {
        m_BloodBar.GetComponent<BloodBar>().SetBloodValue(m_data.hp / m_data.maxhp);
    }

    public bool WorkerToWorkPos(GameObject worker)
    {
        int length = m_WorkerArr.Length;
        for (int i = 0; i < length; i++)
        {
            if (m_WorkerArr[i] == null)
            {
                m_WorkerArr[i] = worker;
                worker.GetComponent<UnitBase>().MoveTo(
                    transform.Find("WorkerPositions").GetChild(i).position);
                return true;
            }
        }
        return false;
    }

    public virtual void BeSelect(bool isOpen, GameObject user)
    {
        if (isOpen)
        {
            m_AudioSource.Play();
            m_Plane.SetActive(true);
            if (user == m_Master)
            {
                m_MessageMenu.SetActive(true);
                m_MessageMenu.GetComponent<MessageMenu>().SetMessage(gameObject, Message_Type.MT_BUILDING);
            }
        }
        else
        {
            m_Plane.SetActive(false);
            if (user == m_Master)
            {
                m_MessageMenu.SetActive(false);
            }
        }
    }

    public virtual void BuildingNormalActionUpdate()
    {
        if (m_CreateList.Count > 0)
        {
            UnitNode node = m_CreateList[0];
            node.passTime += Time.deltaTime;
            if (node.passTime >= node.finishTime)
            {
                GameObject unit = m_Master.GetComponent<MasterBase>().CreateUnitById(node.id);
                unit.transform.position = m_UnitSpawn.transform.position;
                unit.GetComponent<UnitBase>().MoveTo(m_GotoPos.transform.position);
                unit.GetComponent<UnitBase>().SetSpawnBuilding(gameObject);
                m_CreateList.RemoveAt(0);
            }
            else
            {
                m_CreateList[0] = node;
            }
        }
    }

    public virtual void OpenFunctionMenu()
    {
    }

    public virtual void OnRigthMouseClicked()
    {
    }

    public void CancelCreateUnitNode(int index)
    {
        if (m_CreateList != null)
        {
            UnitData unitData = UnitManager.GetInstance().GetDataById(m_CreateList[index].id);
            m_Master.GetComponent<MasterBase>().AddResource(unitData.food, unitData.wood, unitData.stone);
            m_CreateList.RemoveAt(index);
        }
    }

    public virtual int AddCreateUnitNode(int id)
    {
        if (m_CreateList.Count < 5)
        {
            UnitData unitData = UnitManager.GetInstance().GetDataById(id);
            if (m_Master.GetComponent<MasterBase>().ReduceResource(unitData.food, unitData.wood, unitData.stone))
            {
                UnitNode node = new UnitNode();
                node.id = id;
                node.passTime = 0.0f;
                node.finishTime = 10.0f;
                node.icon = UnitManager.GetInstance().GetSpriteById(id);
                m_CreateList.Add(node);
                return 1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return -1;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_State == BuildingState.BS_CREATE)
        {
            m_Plane.SetActive(false);
            m_State = BuildingState.BS_ISNOCREATE;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (m_State == BuildingState.BS_ISNOCREATE)
        {
            m_Plane.SetActive(true);
            m_State = BuildingState.BS_CREATE;
        }
    }

    public List<UnitNode> GetCreateList()
    {
        return m_CreateList;
    }

    public int FindUnitCountFromList(int id)
    {
        int count = 0;
        for (int i = 0; i < m_CreateList.Count; ++i)
        {
            if (m_CreateList[i].id == id)
            {
                ++count;
            }
        }
        return count;
    }
}
