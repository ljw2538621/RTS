using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AI;
using static Villager;

public enum UnitType
{
    UT_NORMAL,
    UT_VILLAGER,
    UT_SOLDIER,
    UT_HEALER,
    UT_FLY,
    UT_KING
}

public struct UnitData
{
    public int id;
    public string name;
    public string descripion;                   // 描述
    public UnitType type;
    public float hp;
    public float maxhp;
    public float moveSpeed;
    public float attack;
    public float attackSpeed;
    public float attackRange;
    public float rotateSpeed;
    public int food;                            // 消耗
    public int wood;
    public int stone;
    public Sprite icon;
}

public class UnitBase : MonoBehaviour
{
    public UnitData m_Data;
    public GameObject m_Master;
    protected GameObject m_BloodBar;
    protected GameObject m_SpawnBuilding;
    protected float m_AttackWaitTime;
    protected bool m_IsSelect;
    protected Animator m_Animator;
    protected NavMeshAgent m_Navigate;
    protected NavMeshObstacle m_Obstacle;
    protected AudioSource m_AudioSource;
    protected GameObject m_PlaneObj;
    protected int m_MoveIndexInt;
    protected bool m_MoveBool;
    protected Vector3 m_destination;
    protected GameObject m_AttackObject;
    protected bool m_IsAttack;

    public bool m_IsLive
    {
        get;
        protected set;
    }

    protected float m_ActionWaitTime;

    protected void BaseAwake()
    {
        m_Data = new UnitData();
        m_Data.maxhp = 100.0f;
        m_Data.hp = m_Data.maxhp;
        m_Data.moveSpeed = 5.0f;
        m_Data.attack = 50.0f;
        m_Data.attackRange = 1.0f;
        m_Data.attackSpeed = 1.0f;
        m_Data.rotateSpeed = 10.0f;
        m_Animator = GetComponent<Animator>();
        m_Navigate = GetComponent<NavMeshAgent>();
        m_Obstacle = GetComponent<NavMeshObstacle>();
        m_AudioSource = GetComponent<AudioSource>();
        //m_remainingDistance = 0.0f;

        m_BloodBar = transform.Find("BloodBar").gameObject;
        m_PlaneObj = transform.Find("Plane").gameObject;
        m_PlaneObj.SetActive(false);
        m_IsSelect = false;
        m_MoveIndexInt = 0;
        m_MoveBool = false;
    }

    protected void BaseOnEnable()
    {
        //m_remainingDistance = 0.0f;
        m_Navigate.enabled = false;
        m_Obstacle.carving = false;
        m_Obstacle.enabled = true;

        m_IsAttack = false;
        m_AttackWaitTime = 0.0f;
        m_AttackObject = null;

        m_IsLive = true;
        m_Animator.SetBool("IsDead", false);
        m_Animator.SetBool("IsIdle", true);
    }

    protected void BaseStart()
    {
    }

    protected void BaseUpdate()
    {
        if (m_IsLive)
        {
            if (m_IsAttack)
            {
                AttackUpdate();
            }
            MoveUpdate();
        }
    }

    protected void BaseLateUpdate()
    {
    }

    public void SetBloodBarMaterial(bool isPlayer)
    {
        m_BloodBar.GetComponent<BloodBar>().SetBarMaterial(isPlayer);
    }

    public void RefreshBloodBar()
    {
        m_BloodBar.GetComponent<BloodBar>().SetBloodValue(m_Data.hp / m_Data.maxhp);
    }

    public void SetSpawnBuilding(GameObject go)
    {
        m_SpawnBuilding = go;
    }

    public virtual void SetAttackTarget(GameObject other)
    {
        m_AttackObject = other;
        m_IsAttack = true;
    }

    public virtual void AttackUpdate()
    {
        if (m_AttackWaitTime < m_Data.attackSpeed)
        {
            m_AttackWaitTime += Time.deltaTime;
        }
        else
        {
            if (m_AttackObject == null)
            {
                return;
            }

            int attackState = -1;
            // 返回 -1 代表无法攻击
            // 返回 0  代表需要移动到对象位置
            // 返回 1  代表可以攻击
            string layer = LayerMask.LayerToName(m_AttackObject.layer);
            switch (layer)
            {
                case "Unit":
                {
                    if (m_AttackObject.GetComponent<UnitBase>().m_IsLive)
                    {
                        if (Mathf.Abs((transform.position - m_AttackObject.transform.position).magnitude) >
                            m_Data.attackRange)
                        {
                            attackState = 0;
                        }
                        else
                        {
                            attackState = 1;
                        }
                    }
                    else
                    {
                        attackState = -1;
                    }
                }
                break;

                case "Building":
                {
                    if (m_AttackObject.GetComponent<BuildingBase>().m_IsLive)
                    {
                        if (Mathf.Abs((transform.position - m_AttackObject.transform.position).magnitude) >
                            m_Data.attackRange + m_AttackObject.GetComponent<BuildingBase>().GetBuildingData().range)
                        {
                            attackState = 0;
                        }
                        else
                        {
                            attackState = 1;
                        }
                    }
                    else
                    {
                        attackState = -1;
                    }
                }
                break;
            }

            switch (attackState)
            {
                case -1:    //返回 -1 代表无法攻击
                {
                    m_AttackObject = null;
                }
                break;

                case 0:     //返回 0  代表需要移动到对象位置
                {
                    MoveTo(m_AttackObject.transform.position);
                }
                break;

                case 1:     //返回 1  代表可以攻击
                {
                    AttackAction(m_AttackObject);
                }
                break;
            }
        }
    }

    public virtual void BeAttacked(float attack)
    {
        if (m_IsLive)
        {
            m_Data.hp -= attack;
            if (m_Data.hp < 0)
            {
                m_Data.hp = 0;
                m_IsLive = false;
            }
        }
    }

    public virtual void AttackAction(GameObject other)
    {
        m_MoveBool = false;
        Vector3 eyeLine = other.transform.position - transform.position;
        eyeLine.y = transform.position.y;
        eyeLine.Normalize();
        transform.LookAt(transform.position + eyeLine);

        if (other != null)
        {
            string layer = LayerMask.LayerToName(other.layer);
            switch (layer)
            {
                case "Unit":
                {
                    if (other.GetComponent<UnitBase>().m_IsLive)
                    {
                        other.GetComponent<UnitBase>().BeAttacked(m_Data.attack);
                    }
                }
                break;

                case "Building":
                {
                    if (other.GetComponent<BuildingBase>().m_IsLive)
                    {
                        if (!other.GetComponent<BuildingBase>().BeAttacked(m_Data.attack))
                        {
                            m_IsAttack = false;
                            m_AttackObject = null;
                        }
                    }
                }
                break;
            }

            if (!m_Animator.GetBool("IsAttacking"))
            {
                m_Animator.SetBool("IsAttacking", true);
            }

            m_AttackWaitTime -= m_Data.attackSpeed;
        }
    }

    public virtual void Move(Vector3 destination)
    {
        if (m_IsAttack)
        {
            m_IsAttack = false;
            m_AttackObject = null;
            if (m_Animator.GetBool("IsAttacking"))
            {
                m_Animator.SetBool("IsAttacking", false);
            }
        }
        MoveTo(destination);
    }

    public virtual void MoveTo(Vector3 destination)
    {
        m_destination = destination;
        m_MoveBool = true;
    }

    public virtual void MoveUpdate()
    {
        if (m_Navigate.enabled)
        {
            if ((transform.position - m_Navigate.destination).magnitude < 2.0f)
            {
                m_MoveBool = false;
            }
        }

        if (m_MoveBool)
        {
            if (m_Obstacle.enabled)
            {
                m_Obstacle.carving = false;
                m_Obstacle.enabled = false;
            }
            else
            {
                if (!m_Navigate.enabled)
                {
                    m_Navigate.enabled = true;
                    m_Navigate.destination = m_destination;
                    m_Animator.SetBool("IsMoving", true);
                    m_Animator.Play("Moving");
                }
                else
                {
                    if (m_Navigate.destination != m_destination)
                    {
                        m_Navigate.destination = m_destination;
                        //m_remainingDistance = Mathf.Abs((transform.position - m_destination).magnitude);
                        //m_Animator.SetBool("IsMoving", true);
                    }
                }
            }
        }
        else
        {
            if (m_Navigate.enabled)
            {
                m_Navigate.enabled = false;
                m_Obstacle.enabled = true;
                m_Obstacle.carving = true;
                m_Animator.SetBool("IsMoving", false);
            }
        }
    }

    public virtual void ActionToBuilding(GameObject building)
    {
    }

    public virtual void ActionToResource(GameObject resource)
    {
    }

    public virtual void SetIdleState()
    {
        m_IsAttack = false;
        m_MoveBool = false;

        m_Animator.SetBool("IsAttacking", false);
        m_Animator.SetBool("IsMoving", false);
        m_Animator.SetBool("IsBuilding", false);
        m_Animator.SetBool("IsHealing", false);
        m_Animator.SetBool("IsCollecting", false);
    }

    public void SetData(UnitData data)
    {
        m_Data = data;
        m_Navigate.speed = m_Data.moveSpeed;
    }

    public void BeAssaulted(int value)
    {
        m_Data.hp -= value;
        if (m_Data.hp <= 0)
        {
            m_Data.hp = 0;
            m_IsLive = false;
            m_Animator.SetBool("IsDead", true);
            m_Master.GetComponent<MasterBase>().RemoveUnitFromList(gameObject);
        }
    }

    public void BeSelect(bool IsSelect)
    {
        if (m_IsSelect != IsSelect)
        {
            if (IsSelect)
            {
                m_AudioSource.Play();
                m_PlaneObj.SetActive(true);
            }
            else
            {
                m_PlaneObj.SetActive(false);
            }
            m_IsSelect = IsSelect;
        }
    }
}
