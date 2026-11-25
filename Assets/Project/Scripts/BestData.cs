using UnityEngine;

[CreateAssetMenu(fileName = "NewBest", menuName = "BestiarioElement")]
public class BestData : ScriptableObject
{
    [Header("Info")]
    public string bestName;
    [TextArea(3, 5)]public string description;
    public int level;

    [Header("Renders")]
    public Sprite icon;
    public GameObject prefab;
}
