using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHauling_WorkOrder : WorkOrder
{
    [Space]
    [Header("TARGET")]
    public ItemContainer targetItemContainer;
    public TileData targetStorageTile;
    HaulingWorkAssigner haulingWorkAssigner;

    public void Fill(ItemContainer _itemContainer, HaulingWorkAssigner _haulingWorkAssigner, WorkOrderType _workOrderType)
    {
        workOrderType = _workOrderType;
        targetItemContainer = _itemContainer;
        haulingWorkAssigner = _haulingWorkAssigner;
    }

    public override bool WorkOrderCheckPass()
    {
        if (!targetItemContainer) 
        {
            CancelOrder();
            return false;
        }
        if (!targetStorageTile) 
        {
            //try assign new tile
            if (!targetStorageTile)
            {
                return false;
            }
        }
        else if (!workerCharacterData.characterData.allowedTasks.taskHauling && workOrderOrigin == WorkOrder.WorkOrderOrigin.General)
        {
            GiveUpWorkOrder();
            return false;
        }
        return true;

    }

    public void AssignTargetStorage(TileData _storageTile) 
    {
        targetStorageTile = _storageTile;
    }

    public void CancelOrder()
    {
        if (workOrderOrigin == WorkOrderOrigin.Personal) 
        {
            targetItemContainer.haulingWorkOrderActive = null;
            targetItemContainer = null;
            targetStorageTile = null;
            workerCharacterData.work.ChangeCurrentWorkOrder(null);
            ToggleActiveState(false);
        }
        else 
        {
            haulingWorkAssigner.CancelWorkOrder(targetItemContainer);
        }
       
    }

    public void GiveUpWorkOrder()
    {
        if (workOrderOrigin == WorkOrderOrigin.Personal)
        {
            CancelOrder();
        }
        else
        {
            haulingWorkAssigner.GiveUpWorkOrder(this);
        }
       
    }

   
    
}
