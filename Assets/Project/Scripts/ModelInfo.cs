using UnityEngine;
using TMPro;

public class ModelInfo : MonoBehaviour
{
    [SerializeField] private BestData data;
    public Animator animator;
    public TextMeshProUGUI txName;
    public BestData BestData { get => data; set => data = value; }
}
