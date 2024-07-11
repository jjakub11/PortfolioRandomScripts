using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkAssigner : MonoBehaviour
{
   public LocalWorkManager localWorkManager;
    

    public virtual void CheckOrders() 
    {
        Debug.Log("Should not check this");
    }


    public CharacterLocalMap GetClosestWorker(List<CharacterLocalMap> _suitableWorkersList, Vector3 targetPosition)
    {
        float distance;
        float lowestDistance = Vector3.Distance(_suitableWorkersList[0].transform.position, targetPosition);
        CharacterLocalMap closestCharacter = _suitableWorkersList[0];
        foreach (CharacterLocalMap cw in _suitableWorkersList)
        {
            distance = Vector3.Distance(cw.transform.position, targetPosition);
            if (distance < lowestDistance)
            {
                closestCharacter = cw;
                lowestDistance = distance;
            }
        }

        return closestCharacter;
    }
}
