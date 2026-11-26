using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class UIEvents : MonoBehaviour
{
    public UnityEvent<float> OnUpdateWood = new();
    public UnityEvent<float> OnUpdateCoal = new();
    public UnityEvent<float> OnUpdateSteel = new();
    public UnityEvent<float> OnUpdateOil = new();
    public UnityEvent<float> OnUpdateGas = new();

    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI coalText;
    [SerializeField] private TextMeshProUGUI steelText;
    [SerializeField] private TextMeshProUGUI oilText;
    [SerializeField] private TextMeshProUGUI gasText;

    private void Start()
    {
        OnUpdateWood.AddListener(UpdateWoodUI);
        OnUpdateCoal.AddListener(UpdateCoalUI);
        OnUpdateSteel.AddListener(UpdateSteelUI);
        OnUpdateOil.AddListener(UpdateOilUI);
        OnUpdateGas.AddListener(UpdateGasUI);
    }

    private void UpdateWoodUI(float value) => woodText.text = value.ToString("F0"); 
    private void UpdateCoalUI(float value) => coalText.text = value.ToString("F0");
    private void UpdateSteelUI(float value) => steelText.text = value.ToString("F0");
    private void UpdateOilUI(float value) => oilText.text = value.ToString("F0");
    private void UpdateGasUI(float value) => gasText.text = value.ToString("F0");
}