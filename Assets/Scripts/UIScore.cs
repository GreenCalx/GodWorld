using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
public class UIScore : MonoBehaviour
{
    public TMP_Text uiBlueScore;
    public TMP_Text uiPurpleScore;
    // Start is called before the first frame update
    void Start()
    {
        raza();
    }

    public void SetBlueScore(float iScore)
    {
        uiBlueScore.text = iScore.ToString();
    }

    public void SetPurpleScore(float iScore)
    {
        uiPurpleScore.text = iScore.ToString();
    }

    public void raza()
    {
        uiBlueScore.text    = "0";
        uiPurpleScore.text  = "0";
    }

}
