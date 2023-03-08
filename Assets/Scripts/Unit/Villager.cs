using RTSEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Villager : UnitBase
{
    public enum VillagerState
    {
        V_NONE,
        V_IDLE,
        V_TOBUILD,
        V_BUILDING,
        V_TOGET,
        V_GETTING,
        V_PUTBACK
    }

    protected GameObject m_Building;
    protected GameObject m_Resource;
    protected VillagerState m_StateVillager;
    protected ResourceNode m_InHandResourceNode;
    protected RuntimeAnimatorController m_AnimatorControllerMain;
    protected RuntimeAnimatorController m_AnimatorControllerToTake;

    private void Awake()
    {
        base.BaseAwake();
        m_Data.type = UnitType.UT_VILLAGER;
        m_AnimatorControllerMain = m_Animator.runtimeAnimatorController;
        m_AnimatorControllerToTake = Resources.Load<RuntimeAnimatorController>("Prefabs/Animatior/DropOffAnimatorOverride");
    }

    private void OnEnable()
    {
        base.BaseOnEnable();
        m_StateVillager = VillagerState.V_IDLE;
        m_Resource = null;
        m_Building = null;
    }

    private void Start()
    {
        base.BaseStart();
    }

    private void Update()
    {
        base.BaseUpdate();
        if (m_IsLive)
        {
            switch (m_StateVillager)
            {
                case VillagerState.V_IDLE:
                    break;

                case VillagerState.V_TOBUILD:
                    {
                        if (!m_MoveBool)
                        {
                            if (m_Building != null)
                            {
                                if ((m_Building.GetComponent<BuildingBase>().GetBuildingData().hp <
                                        m_Building.GetComponent<BuildingBase>().GetBuildingData().maxhp) &&
                                        m_Building.GetComponent<BuildingBase>().WorkerToWorkPos(gameObject))
                                {
                                    m_Animator.SetBool("IsBuilding", true);
                                    transform.Find("Model/right/Hammer").gameObject.SetActive(true);
                                    m_StateVillager = VillagerState.V_BUILDING;
                                    m_ActionWaitTime = 0.0f;
                                }
                                else
                                {
                                    m_Building = null;
                                    m_StateVillager = VillagerState.V_IDLE;
                                }
                            }
                            else
                            {
                                m_StateVillager = VillagerState.V_IDLE;
                            }
                        }
                    }
                    break;

                case VillagerState.V_BUILDING:
                    {
                        if (!m_MoveBool && m_Building != null)
                        {
                            m_ActionWaitTime += Time.deltaTime;
                            if (m_ActionWaitTime >= m_Data.attackSpeed)
                            {
                                m_ActionWaitTime -= m_Data.attackSpeed;
                                m_AudioSource.PlayOneShot(Global.Clip_Construction);
                                if (m_Building.GetComponent<BuildingBase>().AddHp(m_Data.attack))
                                {
                                    Vector3 v3 = m_Building.transform.position;
                                    v3.y = transform.position.y;
                                    transform.LookAt(v3);
                                }
                                else
                                {
                                    SetIdleState();
                                    m_StateVillager = VillagerState.V_IDLE;
                                }
                            }
                        }
                        else
                        {
                            if (m_Building == null)
                            {
                                SetIdleState();
                            }
                        }
                    }
                    break;

                case VillagerState.V_TOGET:
                    {
                        if (!m_MoveBool)
                        {
                            if (m_Resource.activeSelf)
                            {
                                m_Animator.SetBool("IsCollecting", true);
                                m_Animator.Play("Collecting");
                                m_StateVillager = VillagerState.V_GETTING;
                                m_ActionWaitTime = 0.0f;
                                switch (m_Resource.GetComponent<ResourceBase>().m_resourceNode.type)
                                {
                                    case ResourceType.RT_FOOD:
                                        m_AudioSource.clip = Global.Clip_FoodCollect;
                                        break;

                                    case ResourceType.RT_WOOD:
                                        m_AudioSource.clip = Global.Clip_ChopWood;
                                        break;

                                    case ResourceType.RT_STONE:
                                        m_AudioSource.clip = Global.Clip_StoneHit;
                                        break;

                                    default:
                                        m_AudioSource.clip = null;
                                        break;
                                }
                            }
                        }
                    }
                    break;

                case VillagerState.V_GETTING:
                    {
                        if (!m_MoveBool)
                        {
                            if (m_Resource.activeSelf)
                            {
                                if (!m_AudioSource.isPlaying)
                                {
                                    m_AudioSource.Play();
                                }
                                m_ActionWaitTime += Time.deltaTime;
                                Vector3 v3 = m_Resource.transform.position;
                                v3.y = transform.position.y;
                                transform.LookAt(v3);
                                if (m_ActionWaitTime >= m_Data.attackSpeed * 2.0f)
                                {
                                    m_ActionWaitTime = 0.0f;
                                    m_InHandResourceNode = m_Resource.GetComponent<ResourceBase>().GetResourceNode();
                                    m_Animator.SetBool("IsCollecting", false);
                                    m_Animator.runtimeAnimatorController = m_AnimatorControllerToTake;
                                    m_Animator.SetBool("IsMoving", true);
                                    transform.Find("Model/DropOffBox").gameObject.SetActive(true);
                                    m_StateVillager = VillagerState.V_PUTBACK;
                                    if (m_SpawnBuilding != null)
                                    {
                                        MoveTo(m_SpawnBuilding.transform.Find("DropOffPos").position);
                                    }
                                }
                            }
                            else
                            {
                                m_Animator.SetBool("IsCollecting", false);
                                m_Resource = GetNearResource(m_InHandResourceNode.type);
                                if (m_Resource != null)
                                {
                                    m_StateVillager = VillagerState.V_TOGET;
                                    MoveTo(m_Resource.transform.position);
                                }
                                else
                                {
                                    m_StateVillager = VillagerState.V_IDLE;
                                }
                            }
                        }
                    }
                    break;

                case VillagerState.V_PUTBACK:
                    {
                        if (!m_MoveBool)
                        {
                            m_ActionWaitTime += Time.deltaTime;
                            if (m_SpawnBuilding != null)
                            {
                                Vector3 v3 = m_SpawnBuilding.transform.position;
                                v3.y = transform.position.y;
                                transform.LookAt(v3);
                                m_Animator.SetBool("IsMoving", true);
                                if (m_ActionWaitTime >= m_Data.attackSpeed)
                                {
                                    m_ActionWaitTime = 0.0f;
                                    m_Animator.SetBool("IsMoving", false);
                                    m_Animator.runtimeAnimatorController = m_AnimatorControllerMain;
                                    transform.Find("Model/DropOffBox").gameObject.SetActive(false);
                                    switch (m_InHandResourceNode.type)
                                    {
                                        case ResourceType.RT_FOOD:
                                            {
                                                m_SpawnBuilding.GetComponent<BuildingBase>().GetMaster().
                                                    GetComponent<MasterBase>().AddResource(m_InHandResourceNode.number);
                                            }
                                            break;

                                        case ResourceType.RT_WOOD:
                                            {
                                                m_SpawnBuilding.GetComponent<BuildingBase>().GetMaster().
                                                    GetComponent<MasterBase>().AddResource(0, m_InHandResourceNode.number);
                                            }
                                            break;

                                        case ResourceType.RT_STONE:
                                            {
                                                m_SpawnBuilding.GetComponent<BuildingBase>().GetMaster().
                                                    GetComponent<MasterBase>().AddResource(0, 0, m_InHandResourceNode.number);
                                            }
                                            break;

                                        case ResourceType.RT_BOOK:
                                            {
                                                m_SpawnBuilding.GetComponent<BuildingBase>().GetMaster().
                                                    GetComponent<MasterBase>().AddResource(0, 0, 0, m_InHandResourceNode.number);
                                            }
                                            break;

                                        case ResourceType.RT_TREASURE:
                                            {
                                                m_SpawnBuilding.GetComponent<BuildingBase>().GetMaster().
                                                    GetComponent<MasterBase>().AddResource(
                                                    m_InHandResourceNode.number,
                                                    m_InHandResourceNode.number,
                                                    m_InHandResourceNode.number);
                                            }
                                            break;
                                    }
                                    if (m_Resource.activeSelf)
                                    {
                                        m_StateVillager = VillagerState.V_TOGET;
                                        MoveTo(m_Resource.transform.position);
                                    }
                                    else
                                    {
                                        m_Resource = GetNearResource(m_InHandResourceNode.type);
                                        if (m_Resource != null)
                                        {
                                            m_StateVillager = VillagerState.V_TOGET;
                                            MoveTo(m_Resource.transform.position);
                                        }
                                        else
                                        {
                                            m_StateVillager = VillagerState.V_IDLE;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                m_SpawnBuilding = m_Master.GetComponent<MasterBase>().GetMainCapital();
                            }
                        }
                    }
                    break;
            }
        }
    }

    private void LateUpdate()
    {
        base.BaseLateUpdate();
    }

    public GameObject GetNearResource(ResourceType type)
    {
        GameObject ret = null;
        switch (type)
        {
            case ResourceType.RT_FOOD:
                {
                    ret = MapResources.GetInstance().GetNearFood(transform.position);
                }
                break;

            case ResourceType.RT_WOOD:
                {
                    ret = MapResources.GetInstance().GetNearWood(transform.position);
                }
                break;

            case ResourceType.RT_STONE:
                {
                    ret = MapResources.GetInstance().GetNearStone(transform.position);
                }
                break;

            case ResourceType.RT_BOOK:
                {
                    ret = null;
                }
                break;

            case ResourceType.RT_TREASURE:
                {
                    ret = null;
                }
                break;
        }
        return ret;
    }

    public override void SetIdleState()
    {
        base.SetIdleState();
        m_StateVillager = VillagerState.V_IDLE;
        transform.Find("Model/right/Hammer").gameObject.SetActive(false);
        transform.Find("Model/DropOffBox").gameObject.SetActive(false);
        m_Building = null;
        m_Resource = null;
        m_Animator.runtimeAnimatorController = m_AnimatorControllerMain;
    }

    public override void SetAttackTarget(GameObject other)
    {
        string layerName = LayerMask.LayerToName(other.layer);
        switch (layerName)
        {
            case "Building":
                {
                    ActionToBuilding(other);
                }
                break;
        }
    }

    public override void AttackUpdate()
    {
    }

    public override void ActionToBuilding(GameObject building)
    {
        m_AudioSource.PlayOneShot(Global.Clip_SendToBuild);
        m_Building = building;
        m_StateVillager = VillagerState.V_TOBUILD;
        MoveTo(building.transform.position);
        m_IsAttack = false;
        m_Animator.SetBool("IsAttacking", false);
    }

    public override void ActionToResource(GameObject resource)
    {
        m_Resource = resource;
        switch (m_Resource.GetComponent<ResourceBase>().m_resourceNode.type)
        {
            case ResourceType.RT_FOOD:
                break;

            case ResourceType.RT_WOOD:
                m_AudioSource.PlayOneShot(Global.Clip_SendToChopWood);
                break;

            case ResourceType.RT_STONE:
                m_AudioSource.PlayOneShot(Global.Clip_SendToGetStone);
                break;

            default:
                m_AudioSource.clip = null;
                break;
        }
        m_StateVillager = VillagerState.V_TOGET;
        MoveTo(resource.transform.position);
        m_IsAttack = false;
        m_Animator.SetBool("IsAttacking", false);
    }

    public VillagerState GetState()
    {
        return m_StateVillager;
    }

    public ResourceType GetCollectingResourceType()
    {
        return m_Resource.GetComponent<ResourceBase>().GetResourceType();
    }
}
