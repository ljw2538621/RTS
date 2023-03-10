using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MusicMenu : MonoBehaviour
{
    protected AudioMixer m_AudioMixer;
    protected GameObject m_MasterMusic;
    protected GameObject m_EffectMusic;
    protected GameObject m_BackMusic;

    private void Awake()
    {
        m_AudioMixer = Resources.Load<AudioMixer>("Audio/AudioMixer/GameAudioMixer");
        m_MasterMusic = transform.Find("Master").gameObject;
        m_EffectMusic = transform.Find("Effect").gameObject;
        m_BackMusic = transform.Find("Back").gameObject;
    }

    private void OnEnable()
    {
        float MasterVolume;
        float EffectVolume;
        float BackVolume;
        m_AudioMixer.GetFloat("MasterVolume", out MasterVolume);
        m_AudioMixer.GetFloat("EffectVolume", out EffectVolume);
        m_AudioMixer.GetFloat("BackVolume", out BackVolume);
        if (MasterVolume < -40.0f)
        {
            MasterVolume = -40.0f;
        }
        if (EffectVolume < -40.0f)
        {
            EffectVolume = -40.0f;
        }
        if (BackVolume < -40.0f)
        {
            BackVolume = -40.0f;
        }
        //MasterVolume = (int)MasterVolume;
        //EffectVolume = (int)EffectVolume;
        //BackVolume = (int)BackVolume;
        MasterVolume = (MasterVolume + 40.0f) / 40.0f * 100.0f;
        EffectVolume = (EffectVolume + 40.0f) / 40.0f * 100.0f;
        BackVolume = (BackVolume + 40.0f) / 40.0f * 100.0f;

        m_MasterMusic.transform.Find("Slider").GetComponent<Slider>().value = MasterVolume;
        m_MasterMusic.transform.Find("Value").GetComponent<Text>().text = ((int)MasterVolume).ToString();
        m_EffectMusic.transform.Find("Slider").GetComponent<Slider>().value = EffectVolume;
        m_EffectMusic.transform.Find("Value").GetComponent<Text>().text = ((int)EffectVolume).ToString();
        m_BackMusic.transform.Find("Slider").GetComponent<Slider>().value = BackVolume;
        m_BackMusic.transform.Find("Value").GetComponent<Text>().text = ((int)BackVolume).ToString();
    }

    private void Start()
    {
        transform.Find("ReturnBtn").GetComponent<Button>().onClick.AddListener(OnReturnBtnClicked);
        m_MasterMusic.transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(OnMasterValueChanged);
        m_MasterMusic.transform.Find("Slider/LeftBtn").GetComponent<Button>().onClick.AddListener(OnMasterLeftBtnClicked);
        m_MasterMusic.transform.Find("Slider/RightBtn").GetComponent<Button>().onClick.AddListener(OnMasterRightBtnClicked);
        m_EffectMusic.transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(OnEffectValueChanged);
        m_EffectMusic.transform.Find("Slider/LeftBtn").GetComponent<Button>().onClick.AddListener(OnEffectLeftBtnClicked);
        m_EffectMusic.transform.Find("Slider/RightBtn").GetComponent<Button>().onClick.AddListener(OnEffectRightBtnClicked);
        m_BackMusic.transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(OnBackValueChanged);
        m_BackMusic.transform.Find("Slider/LeftBtn").GetComponent<Button>().onClick.AddListener(OnBackLeftBtnClicked);
        m_BackMusic.transform.Find("Slider/RightBtn").GetComponent<Button>().onClick.AddListener(OnBackRightBtnClicked);
    }

    private void Update()
    {
    }

    public void OnReturnBtnClicked()
    {
    }

    public void OnMasterValueChanged(float value)
    {
        value = (int)value;
        m_MasterMusic.transform.Find("Slider").GetComponent<Slider>().value = value;
        m_MasterMusic.transform.Find("Value").GetComponent<Text>().text = ((int)value).ToString();
        value = value / 100.0f * 40.0f - 40.0f;
        if (value < -39.7f)
        {
            value = -80.0f;
        }
        m_AudioMixer.SetFloat("MasterVolume", value);
    }

    public void OnEffectValueChanged(float value)
    {
        value = (int)value;
        m_EffectMusic.transform.Find("Slider").GetComponent<Slider>().value = value;
        m_EffectMusic.transform.Find("Value").GetComponent<Text>().text = ((int)value).ToString();
        value = value / 100.0f * 40.0f - 40.0f;
        if (value < -39.7f)
        {
            value = -80.0f;
        }
        m_AudioMixer.SetFloat("EffectVolume", value);
    }

    public void OnBackValueChanged(float value)
    {
        value = (int)value;
        m_BackMusic.transform.Find("Slider").GetComponent<Slider>().value = value;
        m_BackMusic.transform.Find("Value").GetComponent<Text>().text = ((int)value).ToString();
        value = value / 100.0f * 40.0f - 40.0f;
        if (value < -39.7f)
        {
            value = -80.0f;
        }
        m_AudioMixer.SetFloat("BackVolume", value);
    }

    public void OnMasterLeftBtnClicked()
    {
        float value = m_MasterMusic.transform.Find("Slider").GetComponent<Slider>().value;
        value -= 1.0f;
        if (value < 1.0f)
        {
            value = 0.0f;
        }
        OnMasterValueChanged(value);
    }

    public void OnMasterRightBtnClicked()
    {
        float value = m_MasterMusic.transform.Find("Slider").GetComponent<Slider>().value;
        value += 1.0f;
        if (value > 100.0f)
        {
            value = 100.0f;
        }
        OnMasterValueChanged(value);
    }

    public void OnEffectLeftBtnClicked()
    {
        float value = m_EffectMusic.transform.Find("Slider").GetComponent<Slider>().value;
        value -= 1.0f;
        if (value < 1.0f)
        {
            value = 0.0f;
        }
        OnEffectValueChanged(value);
    }

    public void OnEffectRightBtnClicked()
    {
        float value = m_EffectMusic.transform.Find("Slider").GetComponent<Slider>().value;
        value += 1.0f;
        if (value > 100.0f)
        {
            value = 100.0f;
        }
        OnEffectValueChanged(value);
    }

    public void OnBackLeftBtnClicked()
    {
        float value = m_BackMusic.transform.Find("Slider").GetComponent<Slider>().value;
        value -= 1.0f;
        if (value < 1.0f)
        {
            value = 0.0f;
        }
        OnBackValueChanged(value);
    }

    public void OnBackRightBtnClicked()
    {
        float value = m_BackMusic.transform.Find("Slider").GetComponent<Slider>().value;
        value += 1.0f;
        if (value > 100.0f)
        {
            value = 100.0f;
        }
        OnBackValueChanged(value);
    }
}
