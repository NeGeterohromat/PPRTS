using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class BuildingUIController : MonoBehaviour
{
    public static BuildingUIController Instance { get; private set; }

    [Header("Панели")]
    public GameObject ConstructionPanel;
    public GameObject DetailsPanel;

    [Header("Префабы зданий")]
    public GameObject OilRigPrefab;
    public GameObject GasRigPrefab;

    [Header("Ссылки")]
    public MouseSelector mouseSelector;

    [Header("Тексты панели деталей (TMP)")]
    public TextMeshProUGUI TitleText;       
    public TextMeshProUGUI ResourceTypeText; 
    public TextMeshProUGUI ProductionRateText; 

    private GameObject _currentlySelectedPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenConstructionMenu()
    {
        ConstructionPanel.SetActive(true);
        DetailsPanel.SetActive(false);
    }

    public void SelectOilRig()
    {
        _currentlySelectedPrefab = OilRigPrefab;
        ShowDetails("Нефтяная вышка", "Добыча нефти", "120/ч");
    }

    public void SelectGasRig()
    {
        _currentlySelectedPrefab = GasRigPrefab;
        ShowDetails("Газовая вышка", "Добыча газа", "90/ч");
    }

    public void ShowDetails(string title, string resourceType, string rate)
    {
        DetailsPanel.SetActive(true);
        TitleText.text = title;
        ResourceTypeText.text = resourceType;
        ProductionRateText.text = rate;
    }

    public void OnBuildButtonClicked()
    {
        mouseSelector.StartPlacementMode(_currentlySelectedPrefab);
        //ConstructionPanel.SetActive(false);
        DetailsPanel.SetActive(false);

        TitleText.text = "";
        ResourceTypeText.text = "";
        ProductionRateText.text = "";

        _currentlySelectedPrefab = null;
    }
}