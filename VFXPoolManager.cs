using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXPoolManager : MonoBehaviour
{
    [Header("MELEE HIT")]
    [SerializeField] PooledVFX meleeHit_Prefab;
    [SerializeField] Transform meleeHit_ParentObject;
    [Space]
    [SerializeField] List<PooledVFX> meleeHit_Pool = new List<PooledVFX>();
    [Header("------------------------------------------------")]
    [Space]
    [Header("MELEE HIT")]
    [SerializeField] BulletTraceRenderer bulletTrace_Prefab;
    [SerializeField] Transform bulletTrace_ParentObject;
    [Space]
    [SerializeField] List<PooledVFX> bulletTrace_Pool = new List<PooledVFX>();
    [Header("------------------------------------------------")]
    [Space]
    public static VFXPoolManager instance;

   
   

    private void Awake()
    {
        if (instance) 
        {
            Destroy(instance);
        }

        instance = this;
        Build();
    }

    void Build() 
    {
        BuildPool(meleeHit_Prefab, meleeHit_ParentObject, meleeHit_Pool, 30);
        BuildPool(bulletTrace_Prefab, bulletTrace_ParentObject, bulletTrace_Pool, 50);
    }

    void BuildPool(PooledVFX prefabPVFX, Transform _parent, List<PooledVFX> poolList, int _count) 
    {
        PooledVFX newPVFX;
        for (int i = 0; i < _count; i++) 
        {
            newPVFX = Instantiate(prefabPVFX, _parent);
            newPVFX.SetUpOnInstantiate(this, poolList);

            poolList.Add(newPVFX);

        }
    }

    //GETTERS

    public void PlayMeleeHit_PVFX(Collider col) 
    {
        List<PooledVFX> selectedList = meleeHit_Pool;
        PooledVFX selectedPVFX = null;


        if (selectedList.Count > 0)
        {
            selectedPVFX = selectedList[0];
            if (selectedPVFX && selectedList.Contains(selectedPVFX)) 
            {
                selectedPVFX.transform.position = PositionInsideCollider(col);
                selectedPVFX.PlayPVFX();

                selectedList.Remove(selectedPVFX);
            }
        }

        SoftRes();
    }

    public BulletTraceRenderer GetBulletTracePVFX() 
    {
        List<PooledVFX> selectedList = bulletTrace_Pool;
        BulletTraceRenderer selectedPVFX = null;

        if (selectedList.Count > 0)
        {
            selectedPVFX = selectedList[0].GetComponent<BulletTraceRenderer>();
            if (selectedPVFX && selectedList.Contains(selectedPVFX))
            {
                selectedList.Remove(selectedPVFX);
            }
        }

        return selectedPVFX;

    }

    void SoftRes() 
    {
        
    }

    Vector3 PositionInsideCollider(Collider col) 
    {
        Bounds bounds = col.bounds;

        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z));

    }
}
