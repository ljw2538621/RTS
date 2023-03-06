using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WaitBtn : MonoBehaviour, IPointerClickHandler
{
    protected GameObject parentObject;
    protected Image m_Icon;
    public bool IsCancel;
    public int id;
    public Image m_Blood;

    void Awake()
    {
        m_Icon = transform.Find("Image").GetComponent<Image>();
        m_Blood = transform.Find("FullBlood/Blood").GetComponent<Image>();
    }

    void OnEnable()
    {
        
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SetParentObject(GameObject _object)
    {
        parentObject = _object;
    }

    public void SetData(Sprite icon)
    {
        m_Icon.sprite = icon;
        //id = btnID;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        parentObject.GetComponent<CreateUnitMenuBase>().CancelCreateUnit(id);
    }
}
