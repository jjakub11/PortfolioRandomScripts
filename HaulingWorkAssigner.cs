using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaulingWorkAssigner : WorkAssigner
{
    [Space]
    public List<ItemHauling_WorkOrder> haulingWorkOrders = new List<ItemHauling_WorkOrder>();
    ItemHauling_WorkOrder newWorkOrder;
    public List<CharacterLocalMap> suitableWorkersList = new List<CharacterLocalMap>();

    StorageSpecifiedArea suitableStorageArea;
    TileData suitableStorageTile;
    public void AddNewWorkOrder(ItemContainer itemContainer)
    {
        newWorkOrder = new GameObject().AddComponent<ItemHauling_WorkOrder>();

        newWorkOrder.Fill(itemContainer, this, WorkOrder.WorkOrderType.Hauling);
        itemContainer.haulingWorkOrderActive = newWorkOrder;

        haulingWorkOrders.Add(newWorkOrder);

        newWorkOrder.gameObject.name = "HAULING_WO_" + itemContainer.item.name.ToUpper();
        newWorkOrder.transform.parent = transform;
    }

    public void CancelWorkOrder(ItemContainer itemContainer)
    {
        if (itemContainer.haulingWorkOrderActive && haulingWorkOrders.Contains(itemContainer.haulingWorkOrderActive))
        {
            haulingWorkOrders.Remove(itemContainer.haulingWorkOrderActive);
            if (itemContainer.haulingWorkOrderActive.workerCharacterData)
            {
                itemContainer.haulingWorkOrderActive.workerCharacterData.work.ChangeCurrentWorkOrder(null);
            }

        }

        Destroy(itemContainer.haulingWorkOrderActive.gameObject);
    }

    public void GiveUpWorkOrder(ItemHauling_WorkOrder workOrderToDelay)
    {
        if (workOrderToDelay.workerCharacterData)
        {
            workOrderToDelay.workerCharacterData.work.ChangeCurrentWorkOrder(null);
            workOrderToDelay.AssignWorker(null);
        }
    }

    public override void CheckOrders()
    {
        //Debug.Log("Should !!!! check this");
        //check if reachable by checking neighbors
        //or check directly

        //pawn check if reachable every few seconds

        //check who can reach it
        //then check who is closest

        if (localWorkManager.localMapBase.storageSpecifiedAreas.Count < 1)
        {
            return;
        }

     


        if (haulingWorkOrders.Count > 0)
        {

            foreach (ItemHauling_WorkOrder itemHauling_WorkOrder in haulingWorkOrders)
            {

                if (!itemHauling_WorkOrder.workerCharacterData)
                {

                    if (!itemHauling_WorkOrder.targetStorageTile) 
                    {
                        //Debug.LogWarning("HWO - NO STORAGE TILE");
                        itemHauling_WorkOrder.AssignTargetStorage(FindSuitableStorage(itemHauling_WorkOrder.targetItemContainer.transform.position));
                    }
                   
                    if (itemHauling_WorkOrder.targetStorageTile) 
                    {
                        //Debug.LogWarning("HWO - SUCCESSFULL STORAGE TILE");

                        suitableWorkersList.Clear();
                        foreach (CharacterLocalMap playersCharacter in localWorkManager.localMapBase.playersCharactersList)
                        {
                            if (playersCharacter.characterData.allowedTasks.taskHauling && !playersCharacter.work.HasWorkOrder() && playersCharacter.aiAgent.CanReachDestination(playersCharacter.transform.position, itemHauling_WorkOrder.targetItemContainer.transform.position))
                            {
                                suitableWorkersList.Add(playersCharacter);
                            }
                        }

                        if (suitableWorkersList.Count > 0)
                        {
                           // Debug.LogWarning("HWO - WORKER ASSIGNED");
                            itemHauling_WorkOrder.AssignWorker(GetClosestWorker(suitableWorkersList, itemHauling_WorkOrder.targetItemContainer.transform.position));
                        }
                    }
                   // else Debug.LogWarning("HWO - UNSUCCESSFULL STORAGE TILE");




                }
            }





        }
    }

    TileData FindSuitableStorage(Vector3 itemContainerPosition) 
    {
        suitableStorageArea = null;
        suitableStorageTile = null;
        float closestDistance = 10000f;
        float distance = 0;
        foreach (StorageSpecifiedArea SSA in localWorkManager.localMapBase.storageSpecifiedAreas) 
        {
            distance = Vector3.Distance(itemContainerPosition, SSA.transform.position);
            if (distance < closestDistance && SSA.HasAvalibleFreeTiles()) 
            {
                closestDistance = distance;
                suitableStorageArea = SSA;
            }
        }

        foreach (TileData tileInSSA in suitableStorageArea.usedTiles)
        {
            if (tileInSSA && !tileInSSA.placedItem && !tileInSSA.topBlock && !localWorkManager.OrdersConcerningThisTile(tileInSSA)) 
            {
                suitableStorageTile = tileInSSA;
                return suitableStorageTile;
            }
        }

        return suitableStorageTile;
    }

}
