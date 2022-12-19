using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCSLot : MonoBehaviour
{
    private Player player;
    private Inventory inventory;

    public Image itemImage;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemPrice;
    public TextMeshProUGUI itemAmount;

    public GameObject itemToBuy;
    public int _ItemAmount;
    public TextMeshProUGUI buyPriceText;
    
        
    void Start()
    {
        player = FindObjectOfType<Player>(); 
        //inventory = FindObjectOfType<inventory>();//gak bisa di panggil butuh script inventory
        itemName.text = itemToBuy.GetComponent<Spawn>().itemName;
        itemImage= itemToBuy.GetComponent<Image>().sprite;
        buyPriceText.text = itemToBuy.GetComponentInChildern<Spawn>().itemPrice + "Gold";

    }

    // Update is called once per frame
    void Update()
    {
        itemAmount.text = "Amount :" + _ItemAmount.ToString();
    }

    public void buy()
    {
        for (int i = 0; i < inventory.slots.Leght; i++)
        {
            if(inventory.isFull[i]==true&& inventory.slots[i].transform.GetComponent<Slot>().amount < 15 && player.Gold >= itemToBuy.GetComponentInChildren<Spawn>().itemprice && _ItemAmount > 0)
            {
                if (itemName.text == inventory.slots[i].transform.GetComponentInChildren<Spawn>().itemName)
                {
                    _ItemAmount -= 1;
                    inventory.slots[i].GetComponent<Slot>().amount += 1;
                    player.Gold -= itemToBuy.GetComponentInChildren<Spawn>().itemPrice;
                    break;
                    
                }
            }
            else if(inventory.isFull[i]== false&& player.Gold>= itemToBuy.GetComponentInChildren<Spawn>().itemprice && _ItemAmount > 0)
            {
                _ItemAmount -= 1;
                player.Gold -= itemToBuy.GetComponentInChildern<Spawn>().itemPrice;
                inventory.slots[i].GetComponent<Slot>().ItemName.text = itemName.text;
                inventory.isFull[i] = true;
                Instantiate(itemToBuy, inventory.slots[i].transform, false);
                inventory.slots[i].GetComponent<Slot>().amount += 1;
                break;

            }
        }
    }

    public void sell()
    {
        for(int i = 0; i < inventory.slots.Leght; i++)
        {
            if (inventory.slots[i].transform.GetComponent<Slot>().amount != 0)
            {
                if (itemName.text == inventory.slots[i].transform.GetComponentInChildren<Spawn>().itemName)
                {
                    _ItemAmount += 1;
                    inventory.slots[i].GetComponent<Slot>().amount -= 1;
                    player.Gold += itemToBuy.GetComponentInChildren<Spawn>().itemPrice / 2;
                    if (inventory.slots[i].GetComponent<Slot>().amount == 0)
                    {
                        inventory.slots[i].GetComponent<Slot>().ItemName.text = string.Empty;
                        GameObject.Destroy(inventory.slots[i].transform.GetComponentInChildren<Spawn>().gameObject);
                        
                    }
                    break;
                }
            }
        }
    }
}
