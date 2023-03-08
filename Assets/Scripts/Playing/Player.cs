using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MasterBase
{
    public enum MouseState
    {
        MS_NONE,
        MS_NORMAL,
        MS_GET,
        MS_ATTACK,
    }

    protected GameObject m_GameCamera;
    protected GameObject m_UiCanvas;
    protected GameObject m_FunctionMenu;
    protected GameObject m_PlayerView;
    protected GameObject m_BuildingInHand;
    protected GameObject m_MessageMenu;
    protected GameObject m_SelectResource;
    protected Vector3 m_MouseDownPos;
    protected bool m_IsBuilding;
    protected MouseState m_MouseState;
    protected Texture2D[] m_MouseTex;
    protected AudioSource m_AudioSource;

    private void Awake()
    {
        base.BaseAwake();
    }

    private void Start()
    {
        base.BaseStart();
        m_MainCapital.GetComponent<BuildingBase>().SetBloodBarMaterial(true);
        m_GameCamera = GameObject.Find("Main Camera");

        m_UiCanvas = GameObject.Find("UiCanvas");
        m_PlayerView = GameObject.Find("UiCanvas/PlayerView");
        m_FunctionMenu = m_PlayerView.transform.Find("FunctionMenu").gameObject;
        m_AudioSource = GetComponent<AudioSource>();

        m_PlayerView.GetComponent<PlayerView>().Refresh();
        Global.gameState = GameState.GS_PALYING;
        m_IsBuilding = false;
        m_BuildingInHand = null;

        m_MessageMenu = GameObject.Find("UiCanvas/PlayerView/MessageMenu");

        m_MouseTex = new Texture2D[4];
        m_MouseTex[0] = null;
        m_MouseTex[1] = Resources.Load<Texture2D>("Sprite/Ui/cursorHand_grey");
        m_MouseTex[2] = Resources.Load<Texture2D>("Sprite/Ui/cursorGauntlet_grey");
        m_MouseTex[3] = Resources.Load<Texture2D>("Sprite/Ui/cursorSword_silver");
    }

    private void Update()
    {
        base.BaseUpdate();

        if (!m_IsBuilding)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Ray _ray = m_GameCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                RaycastHit _hitpoint;
                if (Physics.Raycast(_ray, out _hitpoint))
                {
                    string layerName = LayerMask.LayerToName(_hitpoint.collider.gameObject.layer);
                    switch (layerName)
                    {
                        case "Unit":
                            {
                                if (m_SelectUnitList.Count > 0)
                                {
                                    if (_hitpoint.collider.gameObject.GetComponent<UnitBase>().m_Master != gameObject)
                                    {
                                        m_MouseState = MouseState.MS_ATTACK;
                                    }
                                    else
                                    {
                                        m_MouseState = MouseState.MS_NORMAL;
                                    }
                                }
                                else
                                {
                                    m_MouseState = MouseState.MS_NORMAL;
                                }
                            }
                            break;

                        case "Building":
                            {
                                if (m_SelectUnitList.Count > 0)
                                {
                                    if (_hitpoint.collider.gameObject.GetComponent<BuildingBase>().m_Master != gameObject)
                                    {
                                        m_MouseState = MouseState.MS_ATTACK;
                                    }
                                    else
                                    {
                                        m_MouseState = MouseState.MS_NORMAL;
                                    }
                                }
                                else
                                {
                                    m_MouseState = MouseState.MS_NORMAL;
                                }
                            }
                            break;

                        case "Terrain":
                            {
                                m_MouseState = MouseState.MS_NORMAL;
                            }
                            break;

                        case "Resources":
                            {
                                if (m_SelectUnitList.Count > 0)
                                {
                                    m_MouseState = MouseState.MS_GET;
                                }
                                else
                                {
                                    m_MouseState = MouseState.MS_NORMAL;
                                }
                            }
                            break;
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        m_MouseDownPos = Input.mousePosition;
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (Mathf.Abs((Input.mousePosition - m_MouseDownPos).magnitude) < 5.0f)
                        {
                            if (m_SelectUnitList.Count > 0)
                            {
                                for (int i = 0; i < m_SelectUnitList.Count; i++)
                                {
                                    m_SelectUnitList[i].GetComponent<UnitBase>().BeSelect(false);
                                }
                                m_SelectUnitList.Clear();
                            }
                            if (m_BuildingSelected != null)
                            {
                                m_BuildingSelected.GetComponent<BuildingBase>().BeSelect(false, gameObject);
                            }
                            if (m_SelectResource != null)
                            {
                                m_SelectResource.GetComponent<ResourceBase>().BeSelect(false);
                                m_SelectResource = null;
                            }
                            m_MessageMenu.SetActive(false);
                            m_FunctionMenu.GetComponent<FunctionMenu>().CloseMenu();

                            switch (layerName)
                            {
                                case "Unit":
                                    {
                                        if (_hitpoint.collider.gameObject.GetComponent<UnitBase>().m_Master == gameObject)
                                        {
                                            m_SelectUnitList.Add(_hitpoint.collider.gameObject);
                                            _hitpoint.collider.gameObject.GetComponent<UnitBase>().BeSelect(true);
                                            m_FunctionMenu.GetComponent<FunctionMenu>().OpenMenu(
                                                _hitpoint.collider.gameObject.GetComponent<UnitBase>().m_Data.type);
                                        }
                                        m_MessageMenu.SetActive(true);
                                        m_MessageMenu.GetComponent<MessageMenu>().SetMessage(_hitpoint.collider.gameObject, Message_Type.MT_UNIT);
                                    }
                                    break;

                                case "Building":
                                    {
                                        m_BuildingSelected = _hitpoint.collider.gameObject;
                                        m_BuildingSelected.GetComponent<BuildingBase>().BeSelect(true, gameObject);
                                        m_FunctionMenu.GetComponent<FunctionMenu>().CloseMenu();
                                        m_BuildingSelected.GetComponent<BuildingBase>().OpenFunctionMenu();
                                    }
                                    break;

                                case "Terrain":
                                    {
                                        m_FunctionMenu.GetComponent<FunctionMenu>().CloseMenu();
                                    }
                                    break;

                                case "Resources":
                                    {
                                        m_SelectResource = _hitpoint.collider.gameObject;
                                        m_SelectResource.GetComponent<ResourceBase>().BeSelect(true);
                                    }
                                    break;

                                default:
                                    {
                                        m_FunctionMenu.GetComponent<FunctionMenu>().CloseMenu();
                                    }
                                    break;
                            }
                        }
                    }

                    if (Input.GetMouseButtonDown(1))
                    {
                        m_MouseDownPos = Input.mousePosition;
                    }
                    if (Input.GetMouseButtonUp(1))
                    {
                        if (Mathf.Abs((Input.mousePosition - m_MouseDownPos).magnitude) < 5.0f)
                        {
                            if (m_SelectResource != null)
                            {
                                m_SelectResource.GetComponent<ResourceBase>().BeSelect(false);
                                m_SelectResource = null;
                            }
                            switch (layerName)
                            {
                                case "Terrain":
                                    {
                                        if (m_SelectUnitList.Count > 0)
                                        {
                                            for (int i = 0; i < m_SelectUnitList.Count; ++i)
                                            {
                                                m_SelectUnitList[i].GetComponent<UnitBase>().SetIdleState();
                                                m_SelectUnitList[i].GetComponent<UnitBase>().Move(_hitpoint.point);
                                            }
                                        }

                                        if (m_BuildingSelected != null)
                                        {
                                            m_BuildingSelected.GetComponent<BuildingBase>().OnRigthMouseClicked();
                                        }
                                    }
                                    break;

                                case "Unit":
                                    {
                                        UnitAttack(_hitpoint.collider.gameObject);
                                    }
                                    break;

                                case "Building":
                                    {
                                        UnitAttack(_hitpoint.collider.gameObject);
                                    }
                                    break;

                                case "Resources":
                                    {
                                        UnitToResource(_hitpoint.collider.gameObject);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                m_MouseState = MouseState.MS_NORMAL;
            }

            Cursor.SetCursor(m_MouseTex[(int)m_MouseState], Vector2.zero, CursorMode.Auto);
        }
        else
        {
            if (m_BuildingInHand != null)
            {
                Ray _ray = m_GameCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                RaycastHit _hitpoint;
                int layerMask = 1 << LayerMask.NameToLayer("Terrain");
                if (Physics.Raycast(_ray, out _hitpoint, 999.0f, layerMask))
                {
                    m_BuildingInHand.transform.position = _hitpoint.point + new Vector3(0, 0.0001f, 0);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 targetPos = Camera.main.WorldToScreenPoint(m_BuildingInHand.transform.position);
                    targetPos.z = 0;
                    if (Mathf.Abs((Input.mousePosition - targetPos).magnitude) <= 1.0f)
                    {
                        if (m_BuildingInHand.GetComponent<BuildingBase>().m_State == BuildingState.BS_CREATE)
                        {
                            BuildingData buildingData = m_BuildingInHand.GetComponent<BuildingBase>().GetBuildingData();
                            if (ReduceResource(buildingData.food, buildingData.wood, buildingData.stone))
                            {
                                m_BuildingInHand.GetComponent<BuildingBase>().SetState(BuildingState.BS_BUILDING);
                                m_BuildingInHand.transform.SetParent(transform.Find("Building"));
                                m_BuildingInHand.GetComponent<BuildingBase>().BeSelect(false, gameObject);
                                m_BuildingInHand.GetComponent<BuildingBase>().SetMaster(gameObject);
                                m_BuildingList.Add(m_BuildingInHand);
                                for (int i = 0; i < m_SelectUnitList.Count; i++)
                                {
                                    m_SelectUnitList[i].GetComponent<UnitBase>().ActionToBuilding(m_BuildingInHand);
                                    m_SelectUnitList[i].GetComponent<UnitBase>().BeSelect(false);
                                }
                                m_SelectUnitList.Clear();
                                m_FunctionMenu.GetComponent<FunctionMenu>().CloseMenu();
                                m_BuildingInHand = null;
                                m_IsBuilding = false;
                            }
                            else
                            {
                                m_AudioSource.PlayOneShot(Global.Clip_Error);
                            }
                        }
                        else
                        {
                            m_AudioSource.PlayOneShot(Global.Clip_Error);
                        }
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    if (m_BuildingInHand != null)
                    {
                        GameObject.Destroy(m_BuildingInHand);
                        m_BuildingInHand = null;
                        m_IsBuilding = false;
                    }
                }
            }
            else
            {
                m_IsBuilding = false;
            }
        }
    }

    public void MouseLeftDown(GameObject other = null)
    {
        if (other == null)
        {
        }
    }

    public Vector2 GetVector3ToVectro2(Vector3 point)
    {
        Vector2 ret = new Vector2();
        Vector3 targetPos = Camera.main.WorldToScreenPoint(point);
        RectTransform canvasRt = m_UiCanvas.GetComponent<RectTransform>();
        float resolutionRatioWidth = canvasRt.sizeDelta.x;
        float resolutionRatioHeight = canvasRt.sizeDelta.y;

        return ret;
    }

    public void SelectInitByView(Vector2 begin, Vector2 end)
    {
        if (!m_IsBuilding)
        {
            if (m_BuildingSelected != null)
            {
                m_BuildingSelected.GetComponent<BuildingBase>().BeSelect(false, gameObject);
            }
            if (m_SelectResource != null)
            {
                m_SelectResource.GetComponent<ResourceBase>().BeSelect(false);
                m_SelectResource = null;
            }
            if (m_SelectUnitList.Count > 0)
            {
                for (int i = 0; i < m_SelectUnitList.Count; i++)
                {
                    m_SelectUnitList[i].GetComponent<UnitBase>().BeSelect(false);
                }
                m_SelectUnitList.Clear();
            }

            int length = m_UnitList.Count;
            for (int i = 0; i < length; ++i)
            {
                Vector3 targetPos = Camera.main.WorldToScreenPoint(m_UnitList[i].transform.position);
                RectTransform canvasRt = GameObject.Find("UiCanvas").GetComponent<RectTransform>();
                float resolutionRatioWidth = canvasRt.sizeDelta.x;
                float resolutionRatioHeight = canvasRt.sizeDelta.y;
                float widthRatio = resolutionRatioWidth / Screen.width;
                float heightRatio = resolutionRatioHeight / Screen.height;
                //先分别乘以宽高比值
                targetPos.x *= widthRatio;
                targetPos.y *= heightRatio;
                //计算在中心点的屏幕坐标
                //targetPos.x -= resolutionRatioWidth * 0.5f;
                //targetPos.y -= resolutionRatioHeight * 0.5f;

                if (targetPos.x > begin.x &&
                    targetPos.x < end.x &&
                    targetPos.y < begin.y &&
                    targetPos.y > end.y)
                {
                    m_SelectUnitList.Add(m_UnitList[i]);
                    m_UnitList[i].GetComponent<UnitBase>().BeSelect(true);
                }
            }

            if (m_SelectUnitList.Count > 0)
            {
                int count = m_SelectUnitList.Count;
                UnitType type = m_SelectUnitList[0].GetComponent<UnitBase>().m_Data.type;
                for (int i = 0; i < count; i++)
                {
                    if (type != m_SelectUnitList[i].GetComponent<UnitBase>().m_Data.type)
                    {
                        type = UnitType.UT_NORMAL;
                        break;
                    }
                }

                m_FunctionMenu.GetComponent<FunctionMenu>().OpenMenu(type);
                //switch (type)
                //{
                //    case UnitType.UT_NORMAL:
                //    {
                //    }
                //    break;
                //    case UnitType.UT_VILLAGER:
                //    {
                //        m_FunctionMenu.GetComponent<FunctionMenu>().OpenMenu(type);
                //    }
                //    break;
                //    case UnitType.UT_SOLDIER:
                //    break;
                //    case UnitType.UT_HEALER:
                //    break;
                //    case UnitType.UT_FLY:
                //    break;
                //    case UnitType.UT_KING:
                //    break;
                //    default:
                //    break;
                //}
            }
        }
    }

    public void Build(BuildingManagerData data)
    {
        if (m_BuildingInHand != null)
        {
            GameObject.Destroy(m_BuildingInHand);
            m_BuildingInHand = null;
        }
        m_IsBuilding = true;
        GameObject go = GameObject.Instantiate(data.prefabs);
        go.GetComponent<BuildingBase>().SetBuildingData(data.data);
        go.GetComponent<BuildingBase>().SetMaster(gameObject);
        go.GetComponent<BuildingBase>().SetBloodBarMaterial(true);
        go.GetComponent<BuildingBase>().SetState(BuildingState.BS_CREATE);
        m_BuildingInHand = go;
    }

    public override GameObject CreateUnitById(int id)
    {
        UnitManagerData unitManagerData;
        UnitManager.GetInstance().GetUnitDataById(id, out unitManagerData);
        GameObject unit = GameObject.Instantiate(unitManagerData.prefabs);
        unit.transform.SetParent(m_Unit.transform);
        unit.GetComponent<UnitBase>().SetData(unitManagerData.data);
        unit.GetComponent<UnitBase>().SetBloodBarMaterial(true);
        unit.GetComponent<UnitBase>().m_Master = gameObject;
        m_UnitList.Add(unit);
        m_Resource.people = m_UnitList.Count;
        m_PlayerView.GetComponent<PlayerView>().Refresh();
        return unit;
    }

    public override void AddResource(int food, int wood = 0, int stone = 0, int book = 0)
    {
        m_Resource.food += food;
        if (m_Resource.food > m_Resource.foodMax)
        {
            m_Resource.food = m_Resource.foodMax;
        }
        m_Resource.wood += wood;
        if (m_Resource.wood > m_Resource.woodMax)
        {
            m_Resource.wood = m_Resource.woodMax;
        }
        m_Resource.stone += stone;
        if (m_Resource.stone > m_Resource.stoneMax)
        {
            m_Resource.stone = m_Resource.stoneMax;
        }
        m_Resource.book += book;
        if (m_Resource.book > m_Resource.bookMax)
        {
            m_Resource.book = m_Resource.bookMax;
        }
        m_PlayerView.GetComponent<PlayerView>().Refresh();
    }

    public override bool ReduceResource(int food, int wood = 0, int stone = 0)
    {
        if (m_Resource.food >= food &&
            m_Resource.wood >= wood &&
            m_Resource.stone >= stone)
        {
            m_Resource.food -= food;
            m_Resource.wood -= wood;
            m_Resource.stone -= stone;
            m_PlayerView.GetComponent<PlayerView>().Refresh();
            return true;
        }
        return false;
    }
}