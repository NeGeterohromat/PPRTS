using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class BuildingUIController : MonoBehaviour
{
    public static BuildingUIController Instance { get; private set; }

    [Header("������")]
    public GameObject ConstructionPanel;
    public GameObject DetailsPanel;

    [Header("������")]
    public MouseSelector mouseSelector;

    [Header("������ ������ ������� (TMP)")]
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
        _currentlySelectedPrefab = PrefabDatabase.GetPrefab<BuildingType>((int)BuildingType.OilStation);
        ShowDetails("�������� �����", "������ �����", "120/�");
    }

    public void SelectGasRig()
    {
        _currentlySelectedPrefab = PrefabDatabase.GetPrefab<BuildingType>((int)BuildingType.GasStation);
        ShowDetails("������� �����", "������ ����", "90/�");
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