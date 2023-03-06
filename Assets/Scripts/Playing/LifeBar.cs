using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeBar : MonoBehaviour
{
    protected GameObject m_BarSpt;
    protected GameObject m_BarPlane;
    protected GameObject m_Camera;

    private void Awake()
    {
        m_BarSpt = transform.Find("BarSpt").gameObject;
        m_BarPlane = transform.Find("BarSpt/BarPlane").gameObject;
    }

    private void Start()
    {
        m_Camera = GameObject.Find("Main Camera");
    }

    private void Update()
    {
        transform.LookAt(m_Camera.transform);
        //Quaternion quaternion = transform.rotation;
        //quaternion.y = 0;
        //transform.rotation = quaternion;
        //Debug.Log(quaternion);
    }
}
