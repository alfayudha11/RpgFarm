using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    public int Gold;
    public TextMeshProUGUI GoldAmountText;
    

    // Update is called once per frame
    void Update()
    {
        GoldAmountText.text = Gold + "Gold";
    }
}
