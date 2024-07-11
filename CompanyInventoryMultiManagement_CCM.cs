using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompanyInventoryMultiManagement_CCM : MonoBehaviour
{
    public enum CurrentInventory 
    {
        Primary,
        Secondary
    }

    public enum CIMMMode 
    {
        None, //so it wont unload null selectedShip on awake
        Deployment,
        Market,
        Recruitment,
        Shipyard //probably stays
    }

    public enum CIMM_Category 
    {
        Characters,
        AllItems,
        Guns,
        MeleeWeapons,
        Apparel,
        Ships,
        HiresAvalible
    }

    public CIMMMode currentMode;
    [Space]
    [Header("INVENTORIES")]
    public Inventory primaryInventory; //BASE INVENTORY
    public Inventory secondaryInventory; //SHIP OR SHOP //move items back to inventory after cancelling  loadout
    [Space]
    public CurrentInventory currentInventory;
    [SerializeField] CIMM_InventorySelectionTabUI inventorySelectionTabUI;
    [Space]
    [Header("SHIP")]
    public ShipData selectedShipData;
    public MissionData currentMissionData;
    [Header("PERSONNEL")]
    public List<CharacterData> displayedCharacters = new List<CharacterData>();
    [Header("SUPPLIES")]
    public List<Item> displayedItems = new List<Item>();
    [Space]
    [Header("CATEGORY BUTTONS")]
    [SerializeField] CIMM_CategorySelectButtonUI currentlySelected_CIMMBTN;
    [Space]
    [SerializeField] CIMM_CategorySelectButtonUI CIMMBTN_Characters;
    [SerializeField] CIMM_CategorySelectButtonUI CIMMBTN_AllItems;
    [SerializeField] CIMM_CategorySelectButtonUI CIMMBTN_Guns;
    [SerializeField] CIMM_CategorySelectButtonUI CIMMBTN_MeleeWeapons;
    [SerializeField] CIMM_CategorySelectButtonUI CIMMBTN_Apparel;
    [SerializeField] CIMM_CategorySelectButtonUI CIMMBTN_Ships;
    [Space]
    [Header("UI")]
    [SerializeField] StackSizeTransferUI stackSizeTransferUI;
    [SerializeField] GameObject topScroll_CategoryButtons; // hide when hiring for example so category cant be changed
    [SerializeField] Transform mainScrollContent;
    [SerializeField] CC_InventoryItemCardUI CC_InventoryItemCardUI_Prefab;
    [Header("SHIP DETAILS")]

    [SerializeField] ShipDetailsTab shipDetailsTab;
    [Space]
    [SerializeField] FundsTab_CIMM fundsTab;
    [SerializeField] MissionDetailsBannerUI missionDetailsBanner;
    [Space]
    public CompanyControlMenuUI companyControlMenuUI;
    public PlayerProfileData playerProfileData;

    CC_InventoryItemCardUI newInventoryItemCard;

    public void InitializeUI(CompanyControlMenuUI _companyControlMenuUI) 
    {
        companyControlMenuUI = _companyControlMenuUI;
        SetUpButtons();

        inventorySelectionTabUI.InitializeUI();
    }

    void ToggleUI(bool _state) 
    {
        gameObject.SetActive(_state);
    }

    void SetUpButtons() 
    {
        if (CIMMBTN_Characters) { CIMMBTN_Characters.SetUpAwake(CIMM_Category.Characters, this); }
        if (CIMMBTN_AllItems) { CIMMBTN_AllItems.SetUpAwake(CIMM_Category.AllItems, this); }
        if (CIMMBTN_Guns) { CIMMBTN_Guns.SetUpAwake(CIMM_Category.Guns, this); }
        if (CIMMBTN_MeleeWeapons) { CIMMBTN_MeleeWeapons.SetUpAwake(CIMM_Category.MeleeWeapons, this); }
        if (CIMMBTN_Apparel) { CIMMBTN_Apparel.SetUpAwake(CIMM_Category.Apparel, this); }
        if (CIMMBTN_Ships) { CIMMBTN_Ships.SetUpAwake(CIMM_Category.Ships, this); }

    }

    void ToggleCIMMBTN(CIMM_CategorySelectButtonUI _btn, bool _state) 
    {
        if (_btn) 
        {
            _btn.ToggleButtonState(_state);
        }
    }

    void DisableAllButtons() 
    {
        ToggleCIMMBTN(CIMMBTN_Characters, false);
        ToggleCIMMBTN(CIMMBTN_AllItems, false);
        ToggleCIMMBTN(CIMMBTN_Guns, false);
        ToggleCIMMBTN(CIMMBTN_MeleeWeapons, false);
        ToggleCIMMBTN(CIMMBTN_Apparel, false);
        ToggleCIMMBTN(CIMMBTN_Ships, false);
    }


    // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // 
    // // // // // // // // // // // // START  HERE // // // // // // // // // // // // // // // // // // // // // // // // // // 
    // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // 

    public void OpenUI_InMode(CIMMMode _currentMode) 
    {
        companyControlMenuUI.currentCCMenuUI = CompanyControlMenuUI.CCMenuUI.CIMM;

        if (!playerProfileData) 
        {
            playerProfileData = companyControlMenuUI.gM.profileData;
        }

        MissionData mDataToKeep = currentMissionData;
        ResetUI();
        currentMissionData = mDataToKeep;

        currentMode = _currentMode;
        
      
        switch (currentMode) 
        {
            case CIMMMode.Deployment:
                Mode_Deployment();
                break;
            case CIMMMode.Market:
                Mode_Market();
                break;
            case CIMMMode.Recruitment:
                Mode_Recruitment();
                break;
            case CIMMMode.Shipyard:
                Mode_Shipyard();
                break;
        }

        fundsTab.RefreshFundsText(playerProfileData.fundsAmount);

        ToggleUI(true);
    }

    public void CloseUI() 
    {
        ExitedDeployment_UnloadCurrentShip();

        ToggleUI(false);
        ResetUI();
    }

    public void SetMissionData(MissionData mInput) 
    {
        currentMissionData = mInput;
    }

    public void SetCurrentShipData(ShipData _shipData) 
    {
        selectedShipData = _shipData;
    }
    
    public void ResetUI() 
    {
        DisableAllButtons();
        shipDetailsTab.CloseUI();
        missionDetailsBanner.CloseUI();

        currentMissionData = null;
        selectedShipData = null;

        newInventoryItemCard = null;

        inventorySelectionTabUI.ToggleState(false);
        primaryInventory = null;
        secondaryInventory = null;

        stackSizeTransferUI.CloseUI();

        currentMode = CIMMMode.None; 
    }

    public void ExitedDeployment_UnloadCurrentShip() 
    {
        if (currentMode == CIMMMode.Deployment && selectedShipData)
        {
            playerProfileData.UnloadCurrentShipInventoryToBase(selectedShipData);

        }
    }

    //MODES

    

    void Mode_Deployment() 
    {
        ToggleCIMMBTN(CIMMBTN_Characters, true);
        ToggleCIMMBTN(CIMMBTN_AllItems, true);
        ToggleCIMMBTN(CIMMBTN_Guns, true);
        ToggleCIMMBTN(CIMMBTN_MeleeWeapons, true);
        ToggleCIMMBTN(CIMMBTN_Apparel, true);

        shipDetailsTab.OpenUI_WithShip(playerProfileData.playersOnlyShip);
        missionDetailsBanner.OpenUI_UsingMission(currentMissionData);


        primaryInventory = playerProfileData.inventory;
        secondaryInventory = selectedShipData.shipInventory;


        inventorySelectionTabUI.OpenInventory(CurrentInventory.Primary);
        inventorySelectionTabUI.NameButtons();
        inventorySelectionTabUI.ToggleState(true);
        //CAT BUTTON AS LAST

        UsedCategoryButton(CIMMBTN_Characters);

        
    }

    void Mode_Market()
    {
        ToggleCIMMBTN(CIMMBTN_AllItems, true);
        ToggleCIMMBTN(CIMMBTN_Guns, true);
        ToggleCIMMBTN(CIMMBTN_MeleeWeapons, true);
        ToggleCIMMBTN(CIMMBTN_Apparel, true);

        primaryInventory = playerProfileData.inventory;
        secondaryInventory = playerProfileData.marketInventory;

        inventorySelectionTabUI.OpenInventory(CurrentInventory.Secondary);
        inventorySelectionTabUI.NameButtons();
        inventorySelectionTabUI.ToggleState(true);
        //CAT BUTTON AS LAST

        UsedCategoryButton(CIMMBTN_AllItems);

    }

    void Mode_Recruitment()
    {
        ToggleCIMMBTN(CIMMBTN_Characters, true);

        inventorySelectionTabUI.OpenInventory(CurrentInventory.Secondary);
        inventorySelectionTabUI.NameButtons();
        inventorySelectionTabUI.ToggleState(true);

        UsedCategoryButton(CIMMBTN_Characters);
    }

    void Mode_Shipyard()
    {

    }

    //

    public void UsedCategoryButton(CIMM_CategorySelectButtonUI usedButton) 
    {
        CleanCategoryListsContent();

        if (currentlySelected_CIMMBTN) 
        {
            currentlySelected_CIMMBTN.Set_Unselected();
        }
        currentlySelected_CIMMBTN = usedButton;


        switch (usedButton.category) 
        {
            case CIMM_Category.Characters:
                Category_Characters();
                break;
            case CIMM_Category.AllItems:
                Category_AllItems();
                break;
            case CIMM_Category.Guns:
                Category_ItemType(Item.ItemType.Gun);
                break;
            case CIMM_Category.MeleeWeapons:
                Category_ItemType(Item.ItemType.MeleeWeapon);
                break;
            case CIMM_Category.Apparel:
                Category_ItemType(Item.ItemType.Apparel);
                break;
            case CIMM_Category.Ships:
                Category_Ships();
                break;
        }
    }

    void CleanCategoryListsContent() 
    {
        for (int i = mainScrollContent.childCount - 1; i >= 0; i--)
        {
            Destroy(mainScrollContent.GetChild(i).gameObject);
        }
    }

    CC_InventoryItemCardUI NewInventoryItemCard() 
    {
        newInventoryItemCard = Instantiate(CC_InventoryItemCardUI_Prefab, mainScrollContent);
        newInventoryItemCard.InitializeCard(InventoryItemCardUI.InventoryCardMode.CompanyControl, this);
        return newInventoryItemCard;
    }

    //check mode for how to set up "items tab" action button - in deploy add to roster, in recruitment 

    void Category_Characters() 
    {
        if (currentMode == CIMMMode.Deployment)
        {
            if (currentInventory == CurrentInventory.Primary) 
            {
                foreach (CharacterData employeeData in playerProfileData.presentCharactersAtBase)
                {
                    newInventoryItemCard = NewInventoryItemCard();

                    newInventoryItemCard.FillCard_Character(employeeData);
                }
            }

            else if (currentInventory == CurrentInventory.Secondary)
            {
                foreach (CharacterData passengerData in selectedShipData.passengers)
                {
                    newInventoryItemCard = NewInventoryItemCard();

                    newInventoryItemCard.FillCard_Character(passengerData);
                }
            }

        }

        if (currentMode == CIMMMode.Recruitment)
        {
            if (currentInventory == CurrentInventory.Primary)
            {
                foreach (CharacterData employeeData in playerProfileData.presentCharactersAtBase)
                {
                    newInventoryItemCard = NewInventoryItemCard();

                    newInventoryItemCard.FillCard_Character(employeeData);
                }
            }

            else if (currentInventory == CurrentInventory.Secondary)
            {
                foreach (CharacterData mercData in companyControlMenuUI.gM.factionManager.mercenaries_FactionData.factionCharacters)
                {
                    newInventoryItemCard = NewInventoryItemCard();

                    newInventoryItemCard.FillCard_Character(mercData);
                }
            }

        }







    }

    void Category_AllItems()
    {
       
            if (currentInventory == CurrentInventory.Primary)
            {
                foreach (Item item in primaryInventory.items)
                {
                    newInventoryItemCard = NewInventoryItemCard();
                    newInventoryItemCard.FillCard_Item(item);
                }
            }
            if (currentInventory == CurrentInventory.Secondary)
            {
                foreach (Item item in secondaryInventory.items)
                {
                    newInventoryItemCard = NewInventoryItemCard();
                    newInventoryItemCard.FillCard_Item(item);
                }
            }
       
    }

    void Category_ItemType(Item.ItemType requestedItemType) 
    {
        
            if (currentInventory == CurrentInventory.Primary)
            {
                foreach (Item item in primaryInventory.items)
                {
                    if (item.itemType == requestedItemType)
                    {
                        newInventoryItemCard = NewInventoryItemCard();
                        newInventoryItemCard.FillCard_Item(item);
                    }
                }
            }
            if (currentInventory == CurrentInventory.Secondary)
            {
                foreach (Item item in secondaryInventory.items)
                {
                    if (item.itemType == requestedItemType)
                    {
                        newInventoryItemCard = NewInventoryItemCard();
                        newInventoryItemCard.FillCard_Item(item);
                    }
                }
            }
        

    }

    void Category_Ships()
    {
       
    }

    void Category_Mercenaries()
    {

    }

    //INVENTORY

    public void OpenInventory(CurrentInventory _requestedInventory) 
    {
        currentInventory = _requestedInventory;

        if (currentlySelected_CIMMBTN) 
        {
            UsedCategoryButton(currentlySelected_CIMMBTN);
        }
       
    }

    //ACTION BUTTONS FINAL FUNCTIONS

    public void ABClick_LOAD_UNLOAD(CC_InventoryItemCardUI card) 
    {
        if (currentMode == CIMMMode.Deployment) 
        {
            if (currentInventory == CurrentInventory.Primary) 
            {
                if (card.cardOwner_Character) 
                {
                    playerProfileData.CharacterLeftBaseFacility(card.cardOwner_Character);
                    selectedShipData.AddPassenger(card.cardOwner_Character);
                }
                else if (card.cardOwner_Item)
                {
                    if (!card.cardOwner_Item.IsItemStackable()) 
                    {
                        secondaryInventory.AddItemToThisInventory(card.cardOwner_Item, primaryInventory, 0);
                    }
                    else 
                    {
                        stackSizeTransferUI.OpenUI_WithItem(card.cardOwner_Item.GetComponent<StackableItem>(), primaryInventory, secondaryInventory);
                    }
                }

               

            }
            else if (currentInventory == CurrentInventory.Secondary)
            {
                if (card.cardOwner_Character)
                {
                    playerProfileData.CharacterPresentAtBaseFacility(card.cardOwner_Character);
                    selectedShipData.RemovePassenger(card.cardOwner_Character);
                }
                else if (card.cardOwner_Item)
                {
                    if (!card.cardOwner_Item.IsItemStackable())
                    {
                        primaryInventory.AddItemToThisInventory(card.cardOwner_Item, secondaryInventory, 0);
                    }
                    else
                    {
                        stackSizeTransferUI.OpenUI_WithItem(card.cardOwner_Item.GetComponent<StackableItem>(), secondaryInventory, primaryInventory);
                    }
                }
            }

            OpenInventory(currentInventory);
        }
    }

    public void ABClick_EQUIP_UNEQUIP(CC_InventoryItemCardUI card)
    {

    }

    public void ABClick_BUY_SELL(CC_InventoryItemCardUI card)
    {
        if (currentInventory == CurrentInventory.Primary) //SELLING
        {

            if (card.cardOwner_Item)
            {
                if (!card.cardOwner_Item.IsItemStackable())
                {

                    secondaryInventory.AddItemToThisInventory(card.cardOwner_Item, primaryInventory, 0);
                    OpenInventory(currentInventory);

                }
                else
                {
                    stackSizeTransferUI.OpenUI_WithItem(card.cardOwner_Item.GetComponent<StackableItem>(), primaryInventory, secondaryInventory);
                }
            }
            else if (card.cardOwner_Character)
            {

            }


        }
        else if (currentInventory == CurrentInventory.Secondary) //BUYING
        {
            if (card.cardOwner_Item)
            {
                if (!card.cardOwner_Item.IsItemStackable())
                {
                    if (playerProfileData.CanAffordThisItem(card.cardOwner_Item))
                    {
                        fundsTab.RefreshFundsText(playerProfileData.fundsAmount);
                        primaryInventory.AddItemToThisInventory(card.cardOwner_Item, secondaryInventory, 0);
                        OpenInventory(currentInventory);
                    }
                       
                }
                else
                {
                    stackSizeTransferUI.OpenUI_WithItem(card.cardOwner_Item.GetComponent<StackableItem>(), secondaryInventory, primaryInventory);
                }
            }
            else if (card.cardOwner_Character)
            {

            }
        }

       
    }

    public void ABClick_INFO(CC_InventoryItemCardUI card)
    {

    }

    public void ABClick_DROP(CC_InventoryItemCardUI card) //drop or throw away
    {

    }

    public void ABClick_HIRE_FIRE(CC_InventoryItemCardUI card)
    {
        if (currentInventory == CurrentInventory.Primary) //FIRE
        {

            if (card.cardOwner_Character)
            {

            }


        }
        else if (currentInventory == CurrentInventory.Secondary) //HIRE
        {
            if (card.cardOwner_Character)
            {

            }
        }
    }

    //

    public void StartMission() 
    {
        currentMissionData.missionInitialShip = selectedShipData;
        currentMissionData.missionInitialShip.currentLocation = WorldLocation.Location.TravelingWorld;

        selectedShipData = null;
       
        if (companyControlMenuUI.gM.profileData.activeMission) 
        {
            companyControlMenuUI.gM.localMapManager.currentLocalMapBase.worldTravelTimerManager.NewTravelTimer(currentMissionData.missionInitialShip, TravelTimerData.ShipDirection.ToLocalMap);
           


            companyControlMenuUI.CloseCurrentCCM();
            companyControlMenuUI.CloseCurrentCCM();
            companyControlMenuUI.CloseCurrentCCM();
            companyControlMenuUI.CloseCurrentCCM();
        }
        else 
        {
            companyControlMenuUI.gM.StartMission(currentMissionData);
        }
       
       
    }


}
