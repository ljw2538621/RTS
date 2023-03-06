using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiCameraControl : MonoBehaviour
{
    protected GameObject MainCamera;
    protected bool m_IsMove;
    protected float m_UpSpeed;
    protected float m_MoveSpeed;
    protected float m_MaxSpeed;
    protected float m_AcceleratedSpeed;
    protected float m_DefaultMoveSpeed;
    protected Vector3 m_MoveDir;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        MainCamera = GameObject.Find("Main Camera");
        MainCamera.transform.position = new Vector3(-366.0f, 15.0f, 120.0f);
        m_UpSpeed = 5.0f;
        m_MaxSpeed = 30.0f;
        m_AcceleratedSpeed = 5.0f;
        m_DefaultMoveSpeed = 5.0f;
        m_MoveSpeed = m_DefaultMoveSpeed;
        m_MoveDir = Vector2.zero;
    }

    void Update()
    {
        if (Global.IsCanMoveCamera)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                m_MoveDir = Vector3.forward;
                m_IsMove = true;
                m_MoveSpeed = m_MaxSpeed;
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                m_IsMove = false;
                m_MoveDir = Vector3.zero;
                m_MoveSpeed = m_AcceleratedSpeed;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                m_MoveDir = Vector3.back;
                m_IsMove = true;
                m_MoveSpeed = m_MaxSpeed;
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                m_IsMove = false;
                m_MoveDir = Vector3.zero;
                m_MoveSpeed = m_AcceleratedSpeed;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                m_MoveDir = Vector3.left;
                m_IsMove = true;
                m_MoveSpeed = m_MaxSpeed;
            }
            if (Input.GetKeyUp(KeyCode.A))
            {
                m_IsMove = false;
                m_MoveDir = Vector3.zero;
                m_MoveSpeed = m_AcceleratedSpeed;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                m_MoveDir = Vector3.right;
                m_IsMove = true;
                m_MoveSpeed = m_MaxSpeed;
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                m_IsMove = false;
                m_MoveDir = Vector3.zero;
                m_MoveSpeed = m_AcceleratedSpeed;
            }

            if (m_IsMove)
            {
                if (m_MoveSpeed < m_MaxSpeed)
                {
                    m_MoveSpeed += Time.deltaTime * m_AcceleratedSpeed;
                    if (m_MoveSpeed > m_MaxSpeed)
                    {
                        m_MoveSpeed = m_MaxSpeed;
                    }
                }

                MainCamera.transform.position += m_MoveDir * m_MoveSpeed * Time.deltaTime;
            }
            else
            {
                if (m_MoveSpeed > m_DefaultMoveSpeed)
                {
                    m_MoveSpeed -= Time.deltaTime * m_AcceleratedSpeed * 3.0f;
                    if (m_MoveSpeed < m_DefaultMoveSpeed)
                    {
                        m_MoveSpeed = m_AcceleratedSpeed;
                    }
                }
            }

            // ÉãÏñÍ·Ì§¸ßÓë½µµÍ
            Vector3 v3 = MainCamera.transform.position;
            v3 += Input.GetAxis("Mouse ScrollWheel") * m_UpSpeed * MainCamera.transform.forward;
            if (v3.y <= 25.0f && v3.y >= 5.0f)
            {
                MainCamera.transform.position = v3;
            }
        }
    }

    public void MoveStart(string name)
    {
        m_IsMove = true;
        switch (name)
        {
            case "Up":
            {
                m_MoveDir = Vector3.forward;
            }
            break;
            case "Down":
            {
                m_MoveDir = Vector3.back;
            }
            break;
            case "Left":
            {
                m_MoveDir = Vector3.left;
            }
            break;
            case "Right":
            {
                m_MoveDir = Vector3.right;
            }
            break;
        }
    }

    public void MoveStop()
    {
        m_IsMove = false;
        m_MoveDir = Vector3.zero;
    }
}
