﻿using System.Collections.Generic;
using UnityEngine;

public class PauseMenuInventoryManagement : MonoBehaviour
{

    [SerializeField] private PauseMenuInventoryManagementSlot[] inventoryManagementSlot = null;

    public GameObject inventoryManagementDraggedItemPrefab;

    [SerializeField] private Sprite transparent16x16 = null;

    [HideInInspector] public GameObject inventoryTextBoxGameobject;


    private void OnEnable()
    {
        EventHandler.InventoryUpdatedEvent += PopulatePlayerInventory;

        // Populate player inventory
        if (InventoryManager.Instance != null)
        {
            PopulatePlayerInventory(InventoryLocation.player, InventoryManager.Instance.inventoryDictionaries[(int)InventoryLocation.player]);
        }
    }

    private void OnDisable()
    {
        EventHandler.InventoryUpdatedEvent -= PopulatePlayerInventory;

        // DestroyInventoryTextBoxGameobject();
    }

    public void DestroyInventoryTextBoxGameobject()
    {
        // Destroy inventory text box if created
        if (inventoryTextBoxGameobject != null)
        {
            Destroy(inventoryTextBoxGameobject);
        }
    }

    public void DestroyCurrentlyDraggedItems()
    {
        // loop through all player inventory items
        for (int i = 0; i < InventoryManager.Instance.inventoryDictionaries[(int)InventoryLocation.player].Count; i++)
        {
            if (inventoryManagementSlot[i].draggedItem != null)
            {
                Destroy(inventoryManagementSlot[i].draggedItem);
            }

        }
    }

    private void PopulatePlayerInventory(InventoryLocation inventoryLocation, Dictionary<int, InventoryItem> playerInventoryList)
    {
        if (inventoryLocation == InventoryLocation.player)
        {
            InitialiseInventoryManagementSlots();

            // loop through all player inventory items
            for (int i = 0; i < InventoryManager.Instance.inventoryDictionaries[(int)InventoryLocation.player].Count; i++)
            {
                // Get inventory item details
                inventoryManagementSlot[i].itemDetails = InventoryManager.Instance.GetItemDetails(playerInventoryList[i].itemCode);
                inventoryManagementSlot[i].itemQuantity = playerInventoryList[i].itemQuantity;

                if (inventoryManagementSlot[i].itemDetails != null)
                {
                    // update inventory management slot with image and quantity
                    inventoryManagementSlot[i].inventoryManagementSlotImage.sprite = inventoryManagementSlot[i].itemDetails.itemSprite;
                    inventoryManagementSlot[i].textMeshProUGUI.text = inventoryManagementSlot[i].itemQuantity.ToString();
                }
            }
        }
    }

    private void InitialiseInventoryManagementSlots()
    {
        // Clear inventory slots
        for (int i = 0; i < Settings.playerMaximumInventoryCapacity; i++)
        {
            inventoryManagementSlot[i].greyedOutImageGO.SetActive(false);
            inventoryManagementSlot[i].itemDetails = null;
            inventoryManagementSlot[i].itemQuantity = 0;
            inventoryManagementSlot[i].inventoryManagementSlotImage.sprite = transparent16x16;
            inventoryManagementSlot[i].textMeshProUGUI.text = "";
        }

        // Grey out unavailable slots
        for (int i = InventoryManager.Instance.inventoryListCapacityIntArray[(int)InventoryLocation.player]; i < Settings.playerMaximumInventoryCapacity; i++)
        {
            inventoryManagementSlot[i].greyedOutImageGO.SetActive(true);
        }
    }

}
