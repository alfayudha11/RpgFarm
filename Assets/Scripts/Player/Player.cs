﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : SingletonMonobehaviour<Player>
{
    // animation tools
    private bool playerToolUseDisabled = false;
    private WaitForSeconds useToolAnimationPause;
    private WaitForSeconds afterUseToolAnimationPause;
    private WaitForSeconds liftToolAnimationPause;
    private WaitForSeconds afterLiftToolAnimationPause;
    private WaitForSeconds pickAnimationPause;
    private WaitForSeconds afterPickAnimationPause;

    private GridCursor gridCursor;

    // picking condition for harvest with basket
    private bool isPickingRight = false;

    private bool facingRight;
    private Animator anim;
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float xInput, yInput;
    private bool _playerInputIsDisabled = false;
    public bool playerInputIsDisabled { get => _playerInputIsDisabled; set => _playerInputIsDisabled = value; }

    // animation
    private int isWalking;
    private int isUsingHoe;

    // camera
    private Camera mainCamera;

    [Header("MoveController")]
    [SerializeField] private float moveSpeed;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        facingRight = true;
        mainCamera = Camera.main;


        isWalking = Animator.StringToHash("isWalking");
        isUsingHoe = Animator.StringToHash("isUsingHoe");

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        gridCursor = FindObjectOfType<GridCursor>();

        useToolAnimationPause = new WaitForSeconds(Settings.useToolAnimationPause);
        afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAnimationPause);
        liftToolAnimationPause = new WaitForSeconds(Settings.liftToolAnimationPause);
        afterLiftToolAnimationPause = new WaitForSeconds(Settings.afterLiftToolAnimationPause);
        pickAnimationPause = new WaitForSeconds(Settings.pickAnimationPause);
        afterPickAnimationPause = new WaitForSeconds(Settings.afterPickAnimationPause);
    }

    void Update()
    {
        if (!playerInputIsDisabled)
        {
            PlayerMovementInput();

            PlayerTestInput();

            PlayerClickInput();
        }
    }

    void FixedUpdate() // physics
    {
        if (!playerInputIsDisabled)
        {

            PlayerMovement();
        }
    }

    void PlayerMovementInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(xInput, yInput).normalized;

        if (xInput != 0 || yInput != 0)
        {
            anim.SetBool(isWalking, true);
        }
        else
        {
            anim.SetBool(isWalking, false);
        }

        if (xInput < 0 && facingRight)
        {
            Flip();
        }
        else if (xInput > 0 && !facingRight)
        {
            Flip();
        }
    }

    void PlayerMovement()
    {
        rb.MovePosition(rb.position + (moveDirection * moveSpeed * Time.fixedDeltaTime));
    }

    private void PlayerClickInput()
    {
        if (!playerToolUseDisabled)
        {
            if (Input.GetMouseButton(0))
            {
                if (gridCursor.CursorIsEnabled)
                {
                    // Get Cursor Grid Position
                    Vector3Int cursorGridPosition = gridCursor.GetGridPositionForCursor();

                    // Get Player Grid Position
                    Vector3Int playerGridPosition = gridCursor.GetGridPositionForPlayer();

                    ProcessPlayerClickInput(cursorGridPosition, playerGridPosition);
                }
            }
        }
    }

    private void ProcessPlayerClickInput(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        ResetMovement();
        Vector3Int playerDirection = GetPlayerClickDirection(cursorGridPosition, playerGridPosition);

        // Get Grid property details at cursor position (the GridCursor validation routine ensures that grid property details are not null)
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        // Get Selected item details
        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails != null)
        {
            switch (itemDetails.itemType)
            {
                case ItemType.Seed:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputSeed(gridPropertyDetails, itemDetails);
                    }
                    break;

                case ItemType.Commodity:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputCommodity(itemDetails);
                    }
                    break;

                case ItemType.Collecting_tool:
                case ItemType.Watering_tool:
                case ItemType.Hoeing_tool:
                    ProcessPlayerClickInputTool(gridPropertyDetails, itemDetails, playerDirection);
                    break;

                case ItemType.none:
                    break;

                case ItemType.count:
                    break;

                default:
                    break;
            }
        }
    }

    private Vector3Int GetPlayerClickDirection(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        if (cursorGridPosition.x > playerGridPosition.x)
        {
            return Vector3Int.right;
        }
        else
        {
            return Vector3Int.left;
        }
    }

    private void ProcessPlayerClickInputSeed(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid && gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.seedItemCode == -1)
        {
            PlantSeedAtCursor(gridPropertyDetails, itemDetails);
        }
        else if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    private void PlantSeedAtCursor(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        // Process if we have cropdetails for the seed
        if (GridPropertiesManager.Instance.GetCropDetails(itemDetails.itemCode) != null)
        {
            // Update grid properties with seed details
            gridPropertyDetails.seedItemCode = itemDetails.itemCode;
            gridPropertyDetails.growthDays = 0;

            // Display planted crop at grid property details
            GridPropertiesManager.Instance.DisplayPlantedCrop(gridPropertyDetails);

            // Remove item from inventory
            EventHandler.CallRemoveSelectedItemFromInventoryEvent();

        }

    }

    private void ProcessPlayerClickInputCommodity(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    private void ProcessPlayerClickInputTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        // Switch on tool
        switch (itemDetails.itemType)
        {
            case ItemType.Hoeing_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    HoeGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;

            case ItemType.Watering_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    WaterGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;

            case ItemType.Collecting_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    CollectInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;

            default:
                break;
        }
    }

    private void HoeGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        // Trigger animation
        StartCoroutine(HoeGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator HoeGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        playerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Set tool animation to hoe in override animation
        // toolCharacterAttribute.partVariantType = PartVariantType.hoe;
        // characterAttributeCustomisationList.Clear();
        // characterAttributeCustomisationList.Add(toolCharacterAttribute);
        // animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        if (playerDirection == Vector3Int.right)
        {
            if (!facingRight)
            {
                Flip();
            }
            anim.SetBool(isUsingHoe, true);
        }
        else if (playerDirection == Vector3Int.left)
        {
            if (facingRight)
            {
                Flip();
            }
            anim.SetBool(isUsingHoe, true);
        }

        yield return useToolAnimationPause;

        // Set Grid property details for dug ground
        if (gridPropertyDetails.daysSinceDug == -1)
        {
            gridPropertyDetails.daysSinceDug = 0;
        }

        // Set grid property to dug
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // Display dug grid tiles
        GridPropertiesManager.Instance.DisplayDugGround(gridPropertyDetails);

        // After animation pause
        yield return afterUseToolAnimationPause;

        anim.SetBool(isUsingHoe, false);

        playerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void WaterGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        // Trigger animation
        StartCoroutine(WaterGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator WaterGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        playerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Set tool animation to watering can in override animation
        // toolCharacterAttribute.partVariantType = PartVariantType.wateringCan;
        // characterAttributeCustomisationList.Clear();
        // characterAttributeCustomisationList.Add(toolCharacterAttribute);
        // animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        // TODO: If there is water in the watering can
        // toolEffect = ToolEffect.watering;

        if (playerDirection == Vector3Int.right)
        {
            if (!facingRight)
            {
                Flip();
            }
            anim.SetBool(isUsingHoe, true);
        }
        else if (playerDirection == Vector3Int.left)
        {
            if (facingRight)
            {
                Flip();
            }
            anim.SetBool(isUsingHoe, true);
        }

        yield return liftToolAnimationPause;

        // Set Grid property details for watered ground
        if (gridPropertyDetails.daysSinceWatered == -1)
        {
            gridPropertyDetails.daysSinceWatered = 0;
        }

        // Set grid property to watered
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // Display watered grid tiles
        GridPropertiesManager.Instance.DisplayWateredGround(gridPropertyDetails);

        // After animation pause
        yield return afterLiftToolAnimationPause;

        anim.SetBool(isUsingHoe, false);

        playerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void CollectInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {

        StartCoroutine(CollectInPlayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }


    private IEnumerator CollectInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        playerInputIsDisabled = true;
        playerToolUseDisabled = true;

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return pickAnimationPause;

        // After animation pause
        yield return afterPickAnimationPause;

        anim.SetBool(isUsingHoe, false);

        playerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    /// <summary>
    /// Method processes crop with equipped item in player direction
    /// </summary>
    private void ProcessCropWithEquippedItemInPlayerDirection(Vector3Int playerDirection, ItemDetails equippedItemDetails, GridPropertyDetails gridPropertyDetails)
    {
        switch (equippedItemDetails.itemType)
        {

            case ItemType.Collecting_tool:

                if (playerDirection == Vector3Int.right)
                {
                    isPickingRight = true;

                    if (!facingRight)
                    {
                        Flip();
                    }
                    anim.SetBool(isUsingHoe, true);
                }
                else if (playerDirection == Vector3Int.left)
                {
                    if (facingRight)
                    {
                        Flip();
                    }
                    anim.SetBool(isUsingHoe, true);
                }
                break;

            case ItemType.none:
                break;
        }

        // Get crop at cursor grid location
        Crop crop = GridPropertiesManager.Instance.GetCropObjectAtGridLocation(gridPropertyDetails);

        // Execute Process Tool Action For crop
        if (crop != null)
        {
            switch (equippedItemDetails.itemType)
            {
                case ItemType.Collecting_tool:
                    crop.ProcessToolAction(equippedItemDetails, isPickingRight);
                    break;
            }
        }
    }

    // TODO: Remove
    /// <summary>
    /// Temp routine for test input
    /// </summary>
    private void PlayerTestInput()
    {
        // Trigger Advance Time
        if (Input.GetKey(KeyCode.T))
        {
            TimeManager.Instance.TestAdvanceGameMinute();
        }

        // Trigger Advance Day
        if (Input.GetKeyDown(KeyCode.G))
        {
            TimeManager.Instance.TestAdvanceGameDay();
        }

        // Test scene unload / load
        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneControllerManager.Instance.FadeAndLoadScene(SceneName.Scene1_Farm.ToString(), transform.position);
        }

    }

    public void DisablePlayerInputAndResetMovement()
    {
        DisablePlayerInput();
        ResetMovement();
    }

    void ResetMovement()
    {
        rb.MovePosition(rb.position);
        anim.SetBool(isWalking, false);
    }

    public void DisablePlayerInput()
    {
        playerInputIsDisabled = true;
    }
    public void EnablePlayerInput()
    {
        playerInputIsDisabled = false;
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    public Vector3 GetPlayerViewPortPosition()
    {
        // Vector3 viewport position for player (0,0) viewport bottom left, (1,1) viewport top right
        return mainCamera.WorldToViewportPoint(transform.position);
    }
}
