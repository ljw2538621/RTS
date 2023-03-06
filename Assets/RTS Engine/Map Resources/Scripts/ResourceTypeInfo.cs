using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTSEngine;

[CreateAssetMenu(fileName = "NewResourceType", menuName = "RTS Engine/Resource Type", order = 2)]
public class ResourceTypeInfo : ScriptableObject {

    [SerializeField]
    private string _name = "new_resource"; //Resource name
    public string GetName () { return _name; }

    [SerializeField]
    private int startingAmount = 10; //the amount that each team will start with.
    public int GetStartingAmount () { return startingAmount; }

    [SerializeField]
    private Sprite icon = null; //resource Icon.
    public Sprite GetIcon () { return icon; }

    [SerializeField]
    private Color minimapIconColor = Color.green; //the color of the minimap's icon for this resource
    public Color GetMinimapIconColor () { return minimapIconColor; }

    //Audio clips:
    [SerializeField]
    private List<AudioClip> collectionAudio = new List<AudioClip>(); //audio played each time the unit collects some of this resource.
    public AudioClip[] GetCollectionAudio () { return collectionAudio.ToArray(); }
}
