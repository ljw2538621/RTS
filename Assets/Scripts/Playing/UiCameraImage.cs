using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UiCameraImage : MonoBehaviour, IPointerEnterHandler ,IPointerExitHandler
{
    protected string ObjName;
    protected GameObject m_parent;
    void Start()
    {
        ObjName=gameObject.name;
        m_parent = transform.parent.gameObject;
    }

    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_parent.GetComponent<UiCameraControl>().MoveStart(ObjName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_parent.GetComponent<UiCameraControl>().MoveStop();
    }
}
