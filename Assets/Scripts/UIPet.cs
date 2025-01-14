using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class UIPet : MonoBehaviour
{
    public GWPet focusedPet;

    public TextMeshProUGUI petNameLbl;
    public Image foodBar;
    public Image waterBar;
    public Image fatigueBar;

    [Header("Stats")]
    public TextMeshProUGUI HP_val;
    public TextMeshProUGUI MP_val;
    public TextMeshProUGUI STR_val;
    public TextMeshProUGUI VIT_val;
    public TextMeshProUGUI SAG_val;
    public TextMeshProUGUI INT_val;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        refreshUI();
    }

    private void refreshUI()
    {
        foodBar.fillAmount = focusedPet.    petNeeds.currentHunger / focusedPet.gwSettings.agentTotalHunger;
        waterBar.fillAmount = focusedPet.   petNeeds.currentThirst / focusedPet.gwSettings.agentTotalThirst;
        fatigueBar.fillAmount = focusedPet. petNeeds.currentFatigue / focusedPet.gwSettings.agentTotalFatigue;

        HP_val.text = focusedPet.petStats.GetValue(GWPetStats.STATS.HP).ToString();
        MP_val.text = focusedPet.petStats.GetValue(GWPetStats.STATS.MP).ToString();
        STR_val.text = focusedPet.petStats.GetValue(GWPetStats.STATS.STR).ToString();
        VIT_val.text = focusedPet.petStats.GetValue(GWPetStats.STATS.VIT).ToString();
        SAG_val.text = focusedPet.petStats.GetValue(GWPetStats.STATS.SAG).ToString();
        INT_val.text = focusedPet.petStats.GetValue(GWPetStats.STATS.INT).ToString();
    }
}
