using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodBar : MonoBehaviour
{
    protected GameObject m_BarSpt;
    protected GameObject m_BarPlane;
    protected GameObject m_BackSpt;
    protected GameObject m_Camera;
    protected Quaternion direction;

    private void Awake()
    {
        m_BarSpt = transform.Find("BarSpt").gameObject;
        m_BarPlane = transform.Find("BarSpt/BarPlane").gameObject;
        m_BackSpt = transform.Find("BackSpt").gameObject;
    }

    private void Start()
    {
        m_Camera = GameObject.Find("Main Camera");
        direction = Quaternion.FromToRotation(Vector3.forward, Vector3.up);
    }

    private void Update()
    {
        transform.rotation = m_Camera.transform.rotation * direction;
    }

    public void SetBarMaterial(bool isPlayer)
    {
        if (isPlayer)
        {
            m_BarPlane.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/BloodBar");
        }
        else
        {
            m_BarPlane.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/BloodBarEnemy");
        }
    }

    public void SetBloodValue(float value)
    {
        if (value > 1.0f)
        {
            value = 1.0f;
        }
        else
        {
            if (value < 0.000001f)
            {
                value = 0.0f;
            }
        }
        m_BarSpt.transform.localScale = new Vector3(value, 1.0f, 1.0f);
        m_BackSpt.transform.localScale = new Vector3(value - 1.0f, 1.0f, 1.0f);
    }
}
