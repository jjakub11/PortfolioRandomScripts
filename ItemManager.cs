using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemManager : MonoBehaviour
{
    public enum RandomItemGenInventoryType 
    {
        Market,
        MeleeWeapon,
        RangedWeapon,
        TorsoWear,
        PantsWear,
        ShoesWear,
    }

    public GameManager gM;
    [SerializeField] ResourceItem resourceItemPrefab;
    [SerializeField] ItemContainer itemContainerPrefab;
    [Space]
    [Header("----- ----- ----- ----- ----- -----")]
    [Header("RANGED WEAPONS")]
    [Space]
    [SerializeField] RangedWeaponItem SMG;
    [Header("----- ----- ----- ----- ----- -----")]
    [Header("MELEE WEAPONS")]
    [Space]
    [SerializeField] MeleeWeaponItem Pipe;
    [Header("----- ----- ----- ----- ----- -----")]
    [Header("APPAREL ITEMS")]
    [Space]
    [SerializeField] ApparelItem A_Item_TShirt;
    [SerializeField] ApparelItem A_Item_TShirtLongSleeve;
   
    [SerializeField] ApparelItem A_Item_Pants;
    [SerializeField] ApparelItem A_Item_Shorts;

    [SerializeField] ApparelItem A_Item_Boots;
    [SerializeField] ApparelItem A_Item_Shoes;

    [SerializeField] ApparelItem A_Item_Jumpsuit;
    [SerializeField] ApparelItem A_Item_HazardSuit; // with build in gloves and socks

    [Space]
    [Header("----- ----- ----- ----- ----- -----")]
    [SerializeField] ItemsMaterialsManager itemsMaterialsManager;
   
   
   
  

    public ItemContainer GetItemContainer_Instance() 
    {
        return Instantiate(itemContainerPrefab);
    }

    public ResourceItem GetResourceItem_Instance(SubstanceData _substanceData, int amount)
    {
        ResourceItem resourceItem = Instantiate(resourceItemPrefab);
        resourceItem.itemName = _substanceData.substanceName;
        resourceItem.itemSubstance = _substanceData;
        resourceItem.itemStackCount = amount;

        return resourceItem;
    }

    public RangedWeaponItem GetSMG_Item_Instance()
    {
        RangedWeaponItem newSMG = Instantiate(SMG);

        return newSMG;
    }

    public ApparelItem GetShirt_Item_Instance()
    {
        ApparelItem newShirt = Instantiate(A_Item_TShirt);

        return newShirt;
    }

    public MeleeWeaponItem GetPipe_Item_Instance()
    {
        MeleeWeaponItem newShirt = Instantiate(Pipe);

        return newShirt;
    }

   

    public ApparelItem GetRandomApparelItem(CharacterData characterData, ApparelItem.ApparelPlacement apparelPlacement) 
    {
        Item returnItem = null;
        if (apparelPlacement == ApparelItem.ApparelPlacement.Torso_Inner) 
        {
            returnItem = GetRandomItemObject(RandomItemGenInventoryType.TorsoWear, characterData.entityFactionAffiliation.factionInfo.affiliation);
        }
        else  if (apparelPlacement == ApparelItem.ApparelPlacement.Legs)
        {
            returnItem = GetRandomItemObject(RandomItemGenInventoryType.PantsWear, characterData.entityFactionAffiliation.factionInfo.affiliation);
        }

        if (returnItem) return returnItem.GetComponent<ApparelItem>();

        return null;
    }

    public RangedWeaponItem GetRandomRangedWeaponItem(CharacterData characterData)
    {
        Item returnItem = null;

        returnItem = GetRandomItemObject(RandomItemGenInventoryType.RangedWeapon, characterData.entityFactionAffiliation.factionInfo.affiliation);

        if (returnItem) return returnItem.GetComponent<RangedWeaponItem>();

        return null;
    }

    public MeleeWeaponItem GetRandomMeleeWeaponItem(CharacterData characterData)
    {
        Item returnItem = null;

        returnItem = GetRandomItemObject(RandomItemGenInventoryType.MeleeWeapon, characterData.entityFactionAffiliation.factionInfo.affiliation);

        if (returnItem) return returnItem.GetComponent<MeleeWeaponItem>();

        return null;
    }



    public Item GetRandomItemObject(RandomItemGenInventoryType _itemGenInventoryType, FactionStaticInfo.Affiliation _affiliation)
    {
        bool placementInWorld = true;
        

        int rNum = UnityEngine.Random.Range(0, 9); //orig 9

        List<Tuple<double, Item>> itemsListsTuple = GetItemGenInventory(_itemGenInventoryType, _affiliation);

        //itemsListsTuple.Add(new Tuple<double, List<GameObject>>(1, clothingItemsTorso)); //OLD EXAMPLE



       



        double maxValue = 0;
        foreach (Tuple<double, Item> objectsList in itemsListsTuple)
        {
            maxValue += objectsList.Item1;
        }

        int randomNum = UnityEngine.Random.Range(0, (int)maxValue);
        double cumulative = 0;

        Item selectedObject = itemsListsTuple[itemsListsTuple.Count-1].Item2;

        foreach (var tuple in itemsListsTuple)
        {
            cumulative += tuple.Item1;
            if (randomNum <= cumulative)
            {
                selectedObject = tuple.Item2;
                break;
            }
        }



        if (selectedObject) 
        {
            selectedObject = Instantiate(selectedObject);

            if (selectedObject.TryGetComponent<ApparelItem>(out ApparelItem apparelData))
            {


                SubstanceData targetSubstance = gM.substanceManager.GetRandomSubstance(apparelData.apparelSubstanceType);
                Material newMaterial;

                if (gM.substanceManager.ItemCanBePainted(targetSubstance))
                {
                    newMaterial = gM.substanceManager.GetRandomPaintedMaterial();
                }
                else
                {
                    newMaterial = targetSubstance.substanceMaterial;
                }

                apparelData.SetUpApparelItem(targetSubstance, newMaterial);

            }
            else if (selectedObject.TryGetComponent<MeleeWeaponItem>(out MeleeWeaponItem meleeWeaponData))
            {

            }
            else if (selectedObject.TryGetComponent<RangedWeaponItem>(out RangedWeaponItem rangedWeaponData))
            {
                //apparelManager.SetColorsForThisWeapon(rangedWeaponData);
            }


            if (selectedObject.TryGetComponent<StackableItem>(out StackableItem stackableItem))
            {

                if (!placementInWorld || stackableItem.itemType == Item.ItemType.Ammunition)
                {
                    stackableItem.ItemStackGenerate();
                }


            }


        }
        else 
        {
           // Debug.LogError("RANDOM ITEM selectedObject ERROR");
        }

       
        return selectedObject;
    }

    public List<Tuple<double, Item>> GetItemGenInventory(RandomItemGenInventoryType targetType, FactionStaticInfo.Affiliation affiliation) 
    {
        List<Tuple<double, Item>> newList = new List<Tuple<double, Item>>();

        if (targetType == RandomItemGenInventoryType.Market) 
        {
            newList.Add(new Tuple<double, Item>(1, SMG));
            newList.Add(new Tuple<double, Item>(1, Pipe));

            newList.Add(new Tuple<double, Item>(1, A_Item_TShirt));
            newList.Add(new Tuple<double, Item>(1, A_Item_TShirtLongSleeve));

            newList.Add(new Tuple<double, Item>(1, A_Item_Pants));
            

            newList.Add(new Tuple<double, Item>(1, A_Item_Jumpsuit));
            newList.Add(new Tuple<double, Item>(1, A_Item_HazardSuit));

            newList.Add(new Tuple<double, Item>(1, A_Item_Shorts));
        }
        else if (targetType == RandomItemGenInventoryType.TorsoWear)
        {

            if (affiliation == FactionStaticInfo.Affiliation.Nomads) 
            {
                newList.Add(new Tuple<double, Item>(8, null));
                newList.Add(new Tuple<double, Item>(5, A_Item_TShirt));
                newList.Add(new Tuple<double, Item>(1, A_Item_TShirtLongSleeve));
            }
            else if (affiliation == FactionStaticInfo.Affiliation.Pirates)
            {
                newList.Add(new Tuple<double, Item>(8, A_Item_TShirt));
                newList.Add(new Tuple<double, Item>(5, A_Item_TShirt));
                newList.Add(new Tuple<double, Item>(1, A_Item_TShirtLongSleeve));
            }
            else if (affiliation == FactionStaticInfo.Affiliation.Mercenary)
            {
                newList.Add(new Tuple<double, Item>(2, A_Item_TShirt));
                newList.Add(new Tuple<double, Item>(2, A_Item_TShirt));
                newList.Add(new Tuple<double, Item>(2, A_Item_TShirtLongSleeve));
            }

        }

        else if (targetType == RandomItemGenInventoryType.PantsWear)
        {
           
            newList.Add(new Tuple<double, Item>(3, A_Item_Pants));
            newList.Add(new Tuple<double, Item>(1, A_Item_Shorts));

        }

        else if (targetType == RandomItemGenInventoryType.RangedWeapon)
        {

            newList.Add(new Tuple<double, Item>(5, null));
            newList.Add(new Tuple<double, Item>(5, SMG));

        }

        else if (targetType == RandomItemGenInventoryType.MeleeWeapon)
        {

            newList.Add(new Tuple<double, Item>(5, null));
            newList.Add(new Tuple<double, Item>(5, Pipe));

        }





        return newList;

    }

    


}
