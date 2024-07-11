using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Radishmouse;
using StarterAssets;

public class ControlsInputManager : MonoBehaviour
{
    public enum SelectionMode
    {
        DEFAULT,

        AREA_CANCEL,
        AREA_STORAGE,
        //AREA_LANDING, place as device 3x3 or 5x5 or 8x8, free placement

        WORKORDER_CANCEL,
        WORKORDER_MINING,
        WORKORDER_BUILD,
        WORKORDER_HAUL, //to storage
        WORKORDER_SHIPLOAD,

        ATTACK_TARGET

    }

    public SelectionMode currentSelectionMode;


    [Space]
    [Space]
    [Space]
    public List<CharacterLocalMap> selectedCharactersList = new List<CharacterLocalMap>();
    public List<Item> selectedItemsList = new List<Item>();

    public TileData selectedTile;
    public BlockData selectedBlock;
    public ShipLocalMap selectedShip;


    [Space]
    [Header("-----------------------------------------------------")]

    [SerializeField] GameManager gM;
    [SerializeField] StarterAssetsInputs _input;
    [SerializeField] LayerMask selectLayerMask;
    [Space]
    [SerializeField] LayerMask editAreaLayerMask; //ignores UI
    [Space]
    [Header("DIRECT INTERACTION")]
    [SerializeField] LayerMask directInteractionLayerMask; //ignores UI
    public ItemContainer itemC_DI;
    public ShipLocalMap shipLocal_DI;
    [Space]
    [Header("CURSOR")]
    public CursorManager cursorManager;
    [SerializeField] GameObject cursorObject;
    [Header("HIGHLIGHT CURSOR")] //CURRENT HIGHLIGHTS
    [SerializeField] HighlightVisual highlightVisualizer;
    [Space]

    [SerializeField] BlockData cHL_Block;
    [SerializeField] ItemContainer cHL_Item;
    [SerializeField] ShipData cHL_Ship;



    WaitForSeconds highlightCursorCheckDelay = new WaitForSeconds(.05f);



    [Space]
    [Header("INPUT VISUALS")]
    [SerializeField] SetTargetPositionVisual setTargetPositionVisual;

    [Header("DRAG SELECT")]
    [SerializeField] bool dragSelectRunning;
    [SerializeField] TileData dragSelectStarterTile;
    [SerializeField] UILineRenderer selectionLineRenderer;

    [SerializeField] LayerMask dragSelect_CharactersLayerMask;
    [SerializeField] LayerMask dragSelect_BlocksLayerMask;
    [SerializeField] LayerMask dragSelect_TilesLayerMask;
    [Space]
    [SerializeField] Vector3 dragSelectStartingPosition;
    [SerializeField] Vector3 dragSelectPointTwoPosition;
    [SerializeField] Vector3 dragSelectPointThreePosition;
    [SerializeField] Vector3 dragSelectEndingPosition;
    [SerializeField] Vector3 centerPosition;
    [SerializeField] bool gizmosEnabled = true;
    [SerializeField] float gizmoRadius = 1f;


    WaitForSeconds dragSelectRefreshRate = new WaitForSeconds(.025f);
    WaitForSeconds dragInitialDelay = new WaitForSeconds(.25f); //new WaitForSeconds(.5f);
    Quaternion scanBoxRotation;
    Vector3 scanBoxCenter;
    Vector3 scanBoxSize;

    List<CharacterLocalMap> detectedCharactersLocalMapList = new List<CharacterLocalMap>();
    CharacterLocalMap scannedCharacterLocal;
    CharacterData scannedCharacterData;

    List<TileData> detectedTilesList = new List<TileData>();


    [Space]
    [Space]
    [Header("DEVICE PLACEMENT")]
    [SerializeField] bool objectPlacingRunning;
    [SerializeField] LayerMask placeableObjectLayerMask;
    public PlaceableDevice currentPlaceableDevice;
    Vector3 newPlaceablePosition;
    public ItemGFX placeableItemGFX_Mockup;
    float placeableMockupGFX_positionY;
    [Space]
    [SerializeField] Material placeablePositiveMaterial;
    [SerializeField] Material placeableNegativeMaterial;
    [SerializeField] Material placeableInProgressMaterial;

    WaitForSeconds placementRefreshRate = new WaitForSeconds(.01f);

    private void Awake()
    {
        setTargetPositionVisual.ToggleState(false);
        Debug.Log("HLC started");
        StartCoroutine(CursorHighlightCheck());
    }

    public void OnNewMapLoaded() 
    {

    }


    private void OnDrawGizmos()
    {
        if (gizmosEnabled)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(dragSelectStartingPosition, gizmoRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(dragSelectEndingPosition, gizmoRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(dragSelectPointTwoPosition, gizmoRadius);

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(dragSelectPointThreePosition, gizmoRadius);

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(centerPosition, gizmoRadius);



        }

    }



    


    private void Update()
    {
        if (gM.currentGameState == GameManager.GameState.LocalMapMission)
        {
            if (!currentPlaceableDevice && !dragSelectRunning)
            {
                if (_input.holdLMB)
                {
                    PlaceCursor(false);
                    StartDragSelection();
                }
                else
                {
                    PlaceCursor(true && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject());
                   
                }

            }

            
        }


    }

   
    IEnumerator CursorHighlightCheck() 
    {

        while (true) 
        {
            if (gM.currentGameState == GameManager.GameState.LocalMapMission) 
            {
                CursorHighlightObject();
            }
            else 
            {
                CancelCurrentHighlight();
            }
           
            yield return highlightCursorCheckDelay;
        }

    }

    void CursorHighlightObject() 
    {
        //interact with item
        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) //check  
        {
            if (Default_Selection()) 
            {
              
                var mousePos = Input.mousePosition;

                RaycastHit hitInfo;
                var ray = gM.cameraManager.mainCamera.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out hitInfo, 500f, selectLayerMask))
                {
                    if (hitInfo.collider.gameObject.CompareTag("Block") && hitInfo.collider.gameObject.TryGetComponent<BlockData>(out BlockData newBlockData) && cHL_Block != newBlockData)
                    {
                        CancelCurrentHighlight();
                        cHL_Block = newBlockData;
                        highlightVisualizer.BlockGFX(cHL_Block);

                    }

                    else if (hitInfo.collider.gameObject.CompareTag("Item") && hitInfo.collider.gameObject.TryGetComponent<ItemContainer>(out ItemContainer newItemContainer) && cHL_Item != newItemContainer)
                    {
                        CancelCurrentHighlight();
                        cHL_Item = newItemContainer;
                        highlightVisualizer.ItemGFX(cHL_Item);

                    }

                    else if (hitInfo.collider.gameObject.CompareTag("Ship") && hitInfo.collider.gameObject.TryGetComponent<ShipLocalMap>(out ShipLocalMap newShipLocal) && cHL_Ship != newShipLocal)
                    {
                        //CancelCurrentHighlight();
                    }

                    
                }
            }
            else 
            {
                CancelCurrentHighlight();
            }
            

        }
    }

   bool HasCurrentHighlight() 
   {
        return cHL_Block || cHL_Item || cHL_Ship;
   }

    public void CancelCurrentHighlight() 
    {
        if (HasCurrentHighlight()) 
        {
            if (cHL_Block) 
            {
                highlightVisualizer.Reset_BlockVis();
                cHL_Block = null;
            }
            else if (cHL_Item) 
            {
                highlightVisualizer.Reset_ItemVis();
                cHL_Item = null;
            }

            else if (cHL_Ship)
            {
                //highlightVisualizer.Reset_ShipVis(false);
                //cHL_Ship = null;
            }
        }
    }



    //

    void PlaceCursor(bool _state)
    {
        cursorObject.SetActive(_state);
        if (_state)
        {
            RaycastHit hitInfo;
            var ray = gM.cameraManager.mainCamera.ScreenPointToRay(Input.mousePosition);


            if (Physics.Raycast(ray, out hitInfo, 500f, editAreaLayerMask) && Default_Selection())
            {
                //Unselect();
                if (hitInfo.collider.gameObject.CompareTag("Tile"))
                {
                    cursorObject.transform.position = hitInfo.point;
                }
            }

        }
    }

    public void LMB_Click()
    {

        if (currentPlaceableDevice)
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) //check  
            {
                if (PlaceableScanSurroundings())
                {
                    PlaceableConfirm();
                }

            }

        }
        else
        {
            if (placeableItemGFX_Mockup)
            {
                DestroyPlaceableMockup();
            }

            var mousePos = Input.mousePosition;

            RaycastHit hitInfo;
            var ray = gM.cameraManager.mainCamera.ScreenPointToRay(mousePos);
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {

                if (Physics.Raycast(ray, out hitInfo, 500f, selectLayerMask) && Default_Selection())
                {
                    //Unselect();
                    if (hitInfo.collider.gameObject.CompareTag("Tile"))
                    {

                        //move to target position
                        if (selectedCharactersList.Count > 0 && selectedCharactersList[0].characterData.controlAvalible && selectedCharactersList[0].currentBehaviourMode == CharacterLocalMap.CurrentBehaviourMode.CombatMode)
                        {
                            foreach (CharacterLocalMap cLM in selectedCharactersList)
                            {
                                cLM.aiAgent.SetTargetDestination(hitInfo.point);
                                setTargetPositionVisual.StartOnPosition(hitInfo.point);
                            }
                        }
                        else
                        {
                            Unselect();
                            selectedTile = hitInfo.collider.gameObject.GetComponent<TileData>();


                            gM.GUIManager.localMapGUI.ResetGUI();

                            if (selectedTile)
                            {
                                if (selectedTile.assignedSpecifiedArea)
                                {

                                }
                                else if (selectedTile.placedItem) //
                                {

                                }
                            }
                        }
                    }


                    else if (hitInfo.collider.gameObject.CompareTag("Block"))
                    {
                        Unselect();
                        Debug.Log("CLICKED ON BLOCK I");
                        BlockData blockData = hitInfo.collider.gameObject.GetComponent<BlockData>();
                        if (blockData)
                        {
                            Debug.Log("CLICKED ON BLOCK II");
                            SelectThisBlock(blockData);
                        }
                    }
                    else if (hitInfo.collider.gameObject.CompareTag("Character"))
                    {
                        if (!_input.sprint || selectedCharactersList.Count < 1)
                        {
                            Unselect();
                        }
                        CharacterLocalMap charLocalData = hitInfo.collider.gameObject.GetComponent<CharacterLocalMap>();
                        if (charLocalData)
                        {

                            SelectedCharacters_Add(charLocalData);
                            SelectedCharacters();
                        }
                    }

                  
                   

                    else if (hitInfo.collider.gameObject.CompareTag("Ship"))
                    {
                        Unselect();
                        Debug.Log("CLICKED ON SHIP I");
                        ShipLocalMap shipLocalMap = hitInfo.collider.gameObject.GetComponent<ShipLocalMap>();
                        if (shipLocalMap)
                        {
                            Debug.Log("CLICKED ON SHIP II");
                            SelectThisShip(shipLocalMap);
                        }
                    }

                    else if (hitInfo.collider.gameObject.CompareTag("Item"))
                    {
                        if (!_input.sprint || selectedItemsList.Count < 1)
                        {
                            Unselect();
                        }
                        ItemContainer itemContainer = hitInfo.collider.gameObject.GetComponent<ItemContainer>();
                        if (itemContainer)
                        {
                            selectedItemsList.Add(itemContainer.item);
                            SelectedItems();
                        }
                    }


                }


                if (Physics.Raycast(ray, out hitInfo, 500f, selectLayerMask) && AttackTarget_Selection())
                {
                    //play tagged as target gfx anim feedback
                    if (hitInfo.collider && hitInfo.collider.TryGetComponent<DestroyableEntity>(out DestroyableEntity currentTargetData)) 
                    {
                        foreach (CharacterLocalMap selectedCharacter in selectedCharactersList)
                        {

                            if (selectedCharacter.currentBehaviourMode == CharacterLocalMap.CurrentBehaviourMode.CombatMode && selectedCharacter.gameObject != currentTargetData.gameObject) 
                            {
                                selectedCharacter.combat.SetAsCurrentTarget(currentTargetData);
                            }
                          
                        }
                    }
                   
                }
            }
        }

    }

    public void RMB_Click()
    {
        var mousePos = Input.mousePosition;

        RaycastHit hitInfo;
        var ray = gM.cameraManager.mainCamera.ScreenPointToRay(mousePos);
        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            // Unselect();
            Unselect_DirectInteraction();
            if (Physics.Raycast(ray, out hitInfo, 500f, directInteractionLayerMask) && Default_Selection())
            {
                if (hitInfo.collider.gameObject.CompareTag("Item"))
                {
                    itemC_DI = hitInfo.collider.gameObject.GetComponent<ItemContainer>();

                    if (itemC_DI)
                    {
                        gM.GUIManager.localMapGUI.directInteractionsManager.OpenDirectInteraction();
                    }

                }
                if (hitInfo.collider.gameObject.CompareTag("Ship"))
                {
                    shipLocal_DI = hitInfo.collider.gameObject.GetComponent<ShipLocalMap>();
                    if (shipLocal_DI)
                    {
                        gM.GUIManager.localMapGUI.directInteractionsManager.OpenDirectInteraction();
                    }
                }
                }
        }
    }


    public void Unselect()
    {
        selectedTile = null;

        if (selectedCharactersList.Count > 0)
        {
            foreach (CharacterLocalMap cLM in selectedCharactersList)
            {
                if (cLM) 
                {
                    cLM.SetSelectedGFX(false);
                }
            }
            selectedCharactersList.Clear();
        }

        if (selectedItemsList.Count > 0)
        {
            foreach (Item selectedItem in selectedItemsList)
            {
                if (selectedItem) 
                {
                    selectedItem.currentItemContainer.SelectionGFXToggle(false);
                }
            }
            selectedItemsList.Clear();
        }

        else if (selectedShip)
        {
            selectedShip.SetAsSelected(false);
            selectedShip = null;

        }

        else if (selectedBlock)
        {
            selectedBlock.SetAsSelected(false);
            selectedBlock = null;
        }

    }

    public void Unselect_DirectInteraction() 
    {
        itemC_DI = null;
        shipLocal_DI = null;
    }



    public void SelectedCharacters() 
    {

        foreach(CharacterLocalMap characterLocal in selectedCharactersList) 
        {
            characterLocal.SetSelectedGFX(true);
        }

        gM.GUIManager.localMapGUI.OpenSEI();
    }

    public CharacterLocalMap FirstSelectedCharacter() 
    {
        return selectedCharactersList[0];
    }

    public bool SelectedOneOnlyCharacter() 
    {
        return selectedCharactersList.Count == 1;
    }

    public void SelectedCharacters_Add(CharacterLocalMap characterToAdd) 
    {
        if (!selectedCharactersList.Contains(characterToAdd)) 
        {
            selectedCharactersList.Add(characterToAdd);
        }
    }

    public void SelectedCharacters_Remove(CharacterLocalMap characterToRemove)
    {
        if (selectedCharactersList.Contains(characterToRemove))
        {
            selectedCharactersList.Remove(characterToRemove);
        }
    }
    public void SelectThisBlock(BlockData blockData)
    {
        

        selectedBlock = blockData;
        selectedBlock.SetAsSelected(true);

        gM.GUIManager.localMapGUI.OpenSEI();
    }

    public void SelectThisShip(ShipLocalMap shipLocalMap)
    {
       

        selectedShip = shipLocalMap;
        selectedShip.SetAsSelected(true);

        gM.GUIManager.localMapGUI.OpenSEI();
    }

    public void SelectedItems() 
    {
        foreach (Item item in selectedItemsList)
        {
            item.currentItemContainer.SelectionGFXToggle(true);
        }

        gM.GUIManager.localMapGUI.OpenSEI();
    }

    public void AttackTargetSelection() 
    {
        //check current selectionMode

        currentSelectionMode = SelectionMode.ATTACK_TARGET;
    }

    public void SetAsTarget(DestroyableEntity _dE) 
    {

    }

    //SEI BUTTONS

    public void SelectedShipFlyToBase() 
    {
        if (selectedShip) 
        {
            selectedShip.FlyToBaseReturnOrderToggle();
        }
    }

    public void SelectedCharacterSwitchAmmo()
    {
        
    }

    //DRAG SELECTION

    void StartDragSelection()
    {
        dragSelectRunning = true;

        //DrawSelectionLines(new Vector2[] { });
        DrawSelectionLinesLocal(new Vector3[] { });

        StartCoroutine(DragSelectionCR());
    }

    IEnumerator DragSelectionCR()
    {

        RaycastHit hitInfo;
        var mousePos = Input.mousePosition;
        var ray = gM.cameraManager.mainCamera.ScreenPointToRay(mousePos);

        //waitforhalf second to avoid running afrer click


        if (Physics.Raycast(ray, out hitInfo, 500f, editAreaLayerMask))
        {
            if (hitInfo.collider.gameObject.CompareTag("Tile"))
            {
                dragSelectStartingPosition = hitInfo.point;//mousePos; //
                dragSelectStarterTile = hitInfo.collider.gameObject.GetComponent<TileData>();
            }
            else
            {
                DragSelectStop();
                yield break;
            }
        }

        yield return dragInitialDelay;

        mousePos = Input.mousePosition;
        ray = gM.cameraManager.mainCamera.ScreenPointToRay(mousePos);

        if (!_input.holdLMB || (Physics.Raycast(ray, out hitInfo, 500f, editAreaLayerMask) && Vector3.Distance(dragSelectStartingPosition, hitInfo.point) < 1f))
        {
            DragSelectStop();
            yield break;
        }

        Debug.Log("startCoroutinne DRAGSELECT---------------------------------------");

        while (_input.holdLMB)
        {
            mousePos = Input.mousePosition;
            ray = gM.cameraManager.mainCamera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out hitInfo, 500f, editAreaLayerMask))
            {
                if (hitInfo.collider.gameObject.CompareTag("Tile"))
                {
                    dragSelectEndingPosition = hitInfo.point;//mousePos;

                    Vector3 startRod = dragSelectStartingPosition;
                    Vector3 twoRod = dragSelectPointTwoPosition; 
                    Vector3 threeRod = dragSelectPointThreePosition;
                    Vector3 endRod = dragSelectEndingPosition;



                    dragSelectStartingPosition.y = 5f;
                    dragSelectPointTwoPosition.y = 5f;
                    dragSelectPointThreePosition.y = 5f;
                    dragSelectEndingPosition.y = 5f;

                    // Calculate the coordinates of B and D points
                    dragSelectPointTwoPosition.z = dragSelectStartingPosition.z;
                    dragSelectPointTwoPosition.x = dragSelectEndingPosition.x;

                    dragSelectPointThreePosition.z = dragSelectEndingPosition.z;
                    dragSelectPointThreePosition.x = dragSelectStartingPosition.x;


                    //scan was here

                    Vector3[] lineRendererPositions = new Vector3[]
                    {
                        dragSelectStartingPosition,
                        startRod,
                        dragSelectStartingPosition,

                        dragSelectPointTwoPosition,
                        twoRod,
                        dragSelectPointTwoPosition,

                        dragSelectEndingPosition,
                        endRod,
                        dragSelectEndingPosition,

                        dragSelectPointThreePosition,
                        threeRod,
                        dragSelectPointThreePosition,

                        dragSelectStartingPosition,
                    };

                   

                   


                    //DrawSelectionLines(lineRendererPositions);
                    DrawSelectionLinesLocal(lineRendererPositions);
                    Scan();
                }
            }



            yield return dragSelectRefreshRate;
        }

        DragSelectStop();
    }

    public void DrawSelectionLines(Vector2[] _lineRendererPositions)
    {

        selectionLineRenderer.points = _lineRendererPositions;
        selectionLineRenderer.SetVerticesDirty();

    }

    public void DrawSelectionLinesLocal(Vector3[] _lineRendererPositions) 
    {
        gM.localMapManager.selectionLineRenderer.MainSelectionPositions(_lineRendererPositions);
    }
    

    void DragSelectStop()
    {
        SetDetectedTiles();
        DrawSelectionLinesLocal(new Vector3[] { });



        dragSelectRunning = false;
        dragSelectStarterTile = null;
    }

    Vector3 GetWorldPositionFromCanvasPosition(Vector3 _input) 
    {

        Ray ray = gM.cameraManager.mainCamera.ScreenPointToRay(_input);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, editAreaLayerMask))
        {
            if (hitInfo.collider.gameObject.CompareTag("Tile"))
            {
                return hitInfo.point;
            }
        }

        return Vector3.zero;
    }

    void Scan()
    {
       
        scannedCharacterLocal = null;

        Vector3 edit_dragSelectStartingPosition = dragSelectStartingPosition; //GetWorldPositionFromCanvasPosition(dragSelectStartingPosition);
        Vector3 edit_dragSelectPointTwoPosition = dragSelectPointTwoPosition; //GetWorldPositionFromCanvasPosition(dragSelectPointTwoPosition);
        Vector3 edit_dragSelectPointThreePosition = dragSelectPointThreePosition;//GetWorldPositionFromCanvasPosition(dragSelectPointThreePosition);
        Vector3 edit_dragSelectEndingPosition = dragSelectEndingPosition; //GetWorldPositionFromCanvasPosition(dragSelectEndingPosition);




        centerPosition = (edit_dragSelectStartingPosition + edit_dragSelectPointTwoPosition + edit_dragSelectPointThreePosition + edit_dragSelectEndingPosition) / 4f;

        centerPosition.y = 0f;

        scanBoxCenter = centerPosition;

        Vector3 direction = edit_dragSelectPointTwoPosition - edit_dragSelectStartingPosition;

        scanBoxRotation = Quaternion.LookRotation(direction.normalized);
        scanBoxRotation *= Quaternion.Euler(0f, 90f, 0f);

        scanBoxSize = new Vector3(Vector3.Distance(edit_dragSelectPointTwoPosition, edit_dragSelectStartingPosition)/2, 1f, Vector3.Distance(edit_dragSelectPointTwoPosition, edit_dragSelectEndingPosition) / 2);

        //scans for characters first, in character layer mask
        Collider[] hitColliders = Physics.OverlapBox(scanBoxCenter, scanBoxSize, scanBoxRotation, dragSelect_CharactersLayerMask);
        
        if (Default_Selection() && hitColliders.Length > 0 )
        {
            Unselect();

            bool playersCharacters = false;
            Debug.Log("DETECTED CHARACTERS COUNT " + hitColliders.Length);
            foreach (Collider characterCollider in hitColliders)
            {
                scannedCharacterLocal = characterCollider.gameObject.GetComponent<CharacterLocalMap>();
                detectedCharactersLocalMapList.Add(scannedCharacterLocal);
                if (scannedCharacterLocal.characterData.PlayersCharacter()) 
                {
                    playersCharacters = true;
                }
            }

            //if at least one character is players, select just players characters
            if (playersCharacters) //if players characters detected
            {
                foreach (CharacterLocalMap characterLocal in detectedCharactersLocalMapList)
                {
                    if (scannedCharacterLocal.characterData.PlayersCharacter()) 
                    {
                        SelectedCharacters_Add(characterLocal);
                    }
                }
            }
            else //or just first non player character
            {
                selectedCharactersList = detectedCharactersLocalMapList;
            }

            SelectedCharacters();



        }
        else
        {
            

            hitColliders = Physics.OverlapBox(scanBoxCenter, scanBoxSize, scanBoxRotation, dragSelect_TilesLayerMask);

            foreach (TileData tD in detectedTilesList) 
            {

            }

            if (hitColliders.Length > 0)
            {
                // Debug.Log("DETECTED TILES COUNT " + hitColliders.Length);

                List<TileData> currentTilesFromColliders = new List<TileData>();

                if (AreaType_Selection() && dragSelectStarterTile)
                {
                   
                    foreach (Collider c in hitColliders)
                    {
                        selectedTile = c.gameObject.GetComponent<TileData>();
                       

                        if (!selectedTile.topBlock) 
                        {
                            currentTilesFromColliders.Add(selectedTile); //good as is 

                            if (!detectedTilesList.Contains(selectedTile))
                            {
                                if (currentSelectionMode == SelectionMode.AREA_STORAGE) 
                                {
                                    selectedTile.SelectionGFXToggle(true);
                                }
                                else if (currentSelectionMode == SelectionMode.AREA_CANCEL && selectedTile.assignedSpecifiedArea)
                                {
                                    selectedTile.SelectionGFXToggle(true);
                                }
                               
                            }
                        }

                        
                        
                    }

                    foreach(TileData tiD in detectedTilesList) 
                    {
                        if (!currentTilesFromColliders.Contains(tiD)) 
                        {
                            tiD.SelectionGFXToggle(false);
                        }
                    }

                    detectedTilesList = new List<TileData>(currentTilesFromColliders);


                }

                else if (currentSelectionMode == SelectionMode.WORKORDER_BUILD && dragSelectStarterTile)
                {
                    
                    foreach (Collider c in hitColliders)
                    {
                        selectedTile = c.gameObject.GetComponent<TileData>();


                        if (!selectedTile.topBlock && !selectedTile.buildingWorkOrderActive)
                        {
                            currentTilesFromColliders.Add(selectedTile);
                            if (!detectedTilesList.Contains(selectedTile))
                            {
                                selectedTile.SelectionGFXToggle(true);
                            }

                        }



                    }

                    foreach (TileData tiD in detectedTilesList)
                    {
                        if (!currentTilesFromColliders.Contains(tiD))
                        {
                            tiD.SelectionGFXToggle(false);
                        }
                    }

                    detectedTilesList = new List<TileData>(currentTilesFromColliders);

                }

                else if (WorkOrder_Selection() && dragSelectStarterTile)
                {
                    foreach (Collider c in hitColliders)
                    {
                        selectedTile = c.gameObject.GetComponent<TileData>();


                        if (currentSelectionMode == SelectionMode.WORKORDER_MINING && selectedTile.topBlock && !selectedTile.topBlock.miningWorkOrderActive)
                        {
                            currentTilesFromColliders.Add(selectedTile);
                            if (!detectedTilesList.Contains(selectedTile))
                            {
                                selectedTile.topBlock.SelectionGFXToggle(true);
                            }

                        }

                        else if(currentSelectionMode == SelectionMode.WORKORDER_HAUL && selectedTile.placedItem && !selectedTile.placedItem.currentItemContainer.haulingWorkOrderActive)
                        {
                            currentTilesFromColliders.Add(selectedTile);
                            if (!detectedTilesList.Contains(selectedTile))
                            {
                                selectedTile.placedItem.currentItemContainer.SelectionGFXToggle(true);
                            }

                        }

                        else if (currentSelectionMode == SelectionMode.WORKORDER_SHIPLOAD && selectedTile.placedItem && !selectedTile.placedItem.currentItemContainer.deliveryWorkOrderActive)
                        {
                            currentTilesFromColliders.Add(selectedTile);
                            if (!detectedTilesList.Contains(selectedTile))
                            {
                                selectedTile.placedItem.currentItemContainer.SelectionGFXToggle(true);
                            }

                        }


                        else if (currentSelectionMode == SelectionMode.WORKORDER_CANCEL)
                        {
                            if (selectedTile.topBlock && selectedTile.topBlock.miningWorkOrderActive)
                            {
                                currentTilesFromColliders.Add(selectedTile);
                                if (!detectedTilesList.Contains(selectedTile))
                                {
                                    selectedTile.topBlock.SelectionGFXToggle(true);
                                }
                            }
                            else if (selectedTile.placedItem && selectedTile.placedItem.currentItemContainer.haulingWorkOrderActive) 
                            {
                                currentTilesFromColliders.Add(selectedTile);
                                if (!detectedTilesList.Contains(selectedTile))
                                {
                                    selectedTile.placedItem.currentItemContainer.SelectionGFXToggle(true);
                                }
                            }
                        }
                           
                    }


                    foreach (TileData tiD in detectedTilesList)
                    {
                        if (!currentTilesFromColliders.Contains(tiD))
                        {
                            tiD.SelectionGFXToggle(false);
                            if (tiD.topBlock) 
                            {
                                tiD.topBlock.SelectionGFXToggle(false);
                            }
                        }
                    }

                    detectedTilesList = new List<TileData>(currentTilesFromColliders);

                }

            }

        }

        

    }

    void SetDetectedTiles() 
    {
        

        foreach(TileData ti in detectedTilesList) 
        {
            if (currentSelectionMode == SelectionMode.AREA_STORAGE && !selectedTile.topBlock)
            {
                gM.localMapManager.currentLocalMapBase.Tile_SetAsStorage(ti, dragSelectStarterTile);
            }
            else if (currentSelectionMode == SelectionMode.AREA_CANCEL && !selectedTile.topBlock) //currentSelectionMode == SelectionMode.WORKORDER_HAUL && selectedTile.placedItem && !selectedTile.placedItem.haulingWorkOrderActive
            {
                    gM.localMapManager.currentLocalMapBase.Tile_SetAsUnspecified(ti);
            }

            else if (currentSelectionMode == SelectionMode.WORKORDER_BUILD && !ti.topBlock) //if item is placed, remove it
            {
                if (!ti.buildingWorkOrderActive) 
                {
                    gM.localMapManager.currentLocalMapBase.localWorkManager.buildingWorkAssigner.AddNewWorkOrder(ti, BuildSM_CurrentStructureType(), BuildSM_CurrentSelectedSubstance());
                }
                else //change workorder LATER
                {

                }
               
            }

            else if (currentSelectionMode == SelectionMode.WORKORDER_MINING && ti.topBlock && !ti.topBlock.miningWorkOrderActive)
            {
                gM.localMapManager.currentLocalMapBase.localWorkManager.miningWorkAssigner.AddNewWorkOrder(ti.topBlock);
                ti.topBlock.ToggleMineOrderGFX(true);
            }

            else if (currentSelectionMode == SelectionMode.WORKORDER_HAUL && ti.placedItem && ti.placedItem.currentItemContainer && !ti.placedItem.currentItemContainer.haulingWorkOrderActive)
            {
                if (ti.placedItem.currentItemContainer.deliveryWorkOrderActive)
                {

                    if (ti.placedItem.currentItemContainer.deliveryWorkOrderActive.workOrderOrigin == WorkOrder.WorkOrderOrigin.General)
                    {
                        gM.localMapManager.currentLocalMapBase.localWorkManager.deliveryToInventoryWorkAssigner.CancelWorkOrder(ti.placedItem.currentItemContainer);
                    }

                    else
                    {

                    }

                }


                gM.localMapManager.currentLocalMapBase.localWorkManager.haulingWorkAssigner.AddNewWorkOrder(ti.placedItem.currentItemContainer);
                ti.placedItem.currentItemContainer.ToggleHaulOrderGFX(true);
            }

            else if (currentSelectionMode == SelectionMode.WORKORDER_SHIPLOAD && ti.placedItem && ti.placedItem.currentItemContainer && !ti.placedItem.currentItemContainer.deliveryWorkOrderActive)
            {
                if (ti.placedItem.currentItemContainer.haulingWorkOrderActive)
                {
                    
                    if (ti.placedItem.currentItemContainer.haulingWorkOrderActive.workOrderOrigin == WorkOrder.WorkOrderOrigin.General)
                    {
                        gM.localMapManager.currentLocalMapBase.localWorkManager.haulingWorkAssigner.CancelWorkOrder(ti.placedItem.currentItemContainer);
                    }

                    else 
                    {

                    }

                }

                gM.localMapManager.currentLocalMapBase.localWorkManager.deliveryToInventoryWorkAssigner.AddNewWorkOrder_Item(ti.placedItem.currentItemContainer, 0, gM.localMapManager.currentLocalMapBase.playersShipLocal.shipData.shipInventory, gM.localMapManager.currentLocalMapBase.playersShipLocal.transform);
                ti.placedItem.currentItemContainer.ToggleHaulOrderGFX(true);
            }

            else if (currentSelectionMode == SelectionMode.WORKORDER_CANCEL )
            {
                Debug.Log("CANCEL WORKORDER IN CIM 1");
                if (ti.topBlock && ti.topBlock.miningWorkOrderActive) 
                {
                    gM.localMapManager.currentLocalMapBase.localWorkManager.miningWorkAssigner.CancelWorkOrder(ti.topBlock);
                    ti.topBlock.ToggleMineOrderGFX(false);
                }

                else if (ti.placedItem && ti.placedItem.currentItemContainer.haulingWorkOrderActive)
                {
                    gM.localMapManager.currentLocalMapBase.localWorkManager.haulingWorkAssigner.CancelWorkOrder(ti.placedItem.currentItemContainer);
                    ti.placedItem.currentItemContainer.ToggleHaulOrderGFX(false);
                }

                if(ti.buildingWorkOrderActive) 
                {
                    Debug.Log("22 CANCEL WORKORDER IN CIM");
                    gM.localMapManager.currentLocalMapBase.localWorkManager.buildingWorkAssigner.CancelWorkOrder(ti);
                }


            }

            ti.SelectionGFXToggle(false);
            if (ti.topBlock) 
            {
                ti.topBlock.SelectionGFXToggle(false);
            }

        }

        detectedTilesList.Clear();
    }

    bool Default_Selection() 
    {
        if (currentSelectionMode == SelectionMode.DEFAULT) 
        {
            return true;
        }
        else return false;
    }

    bool AreaType_Selection()
    {
        if (currentSelectionMode == SelectionMode.AREA_CANCEL || currentSelectionMode == SelectionMode.AREA_STORAGE)
        {
            return true;
        }
        else return false;
    }

    bool WorkOrder_Selection()
    {
        if (currentSelectionMode == SelectionMode.WORKORDER_CANCEL || currentSelectionMode == SelectionMode.WORKORDER_MINING || currentSelectionMode == SelectionMode.WORKORDER_HAUL || currentSelectionMode == SelectionMode.WORKORDER_SHIPLOAD)
        {
            return true;
        }
        else return false;
    }

    bool AttackTarget_Selection() 
    {
        if (currentSelectionMode == SelectionMode.ATTACK_TARGET)
        {
            return true;
        }
        else return false;
    }

    

    void ResetDragSelection() //not yet because of gizmos, maybe not necessary
    {

    }

    public StructureBlueprint.StructureType BuildSM_CurrentStructureType() 
    {
        return gM.GUIManager.localMapGUI.subMenusUI.buildSubMenu.currentStructureType;
    }

    public SubstanceData BuildSM_CurrentSelectedSubstance()
    {
        return gM.GUIManager.localMapGUI.subMenusUI.buildSubMenu.CurrentSelectedSubstance();
    }

    //PLACEABLES

    public void SetAsCurrentPlaceableDevice(PlaceableDevice _placeable)
    {
        if (objectPlacingRunning)
        {
            ObjectPlacingStop();
        }

        currentPlaceableDevice = _placeable;

        placeableItemGFX_Mockup = Instantiate(currentPlaceableDevice.itemGFXPrefab.gameObject).GetComponent<ItemGFX>();
        placeableMockupGFX_positionY = currentPlaceableDevice.itemGFXPrefab.transform.localPosition.y;

        placeableItemGFX_Mockup.PlacingFeedback();

        objectPlacingRunning = true;
        StartCoroutine(ObjectPlacingCR());
    }

    IEnumerator ObjectPlacingCR() 
    {
        while (true) 
        {
            if (placeableItemGFX_Mockup) //create mockupGFX when setting
            {
                PlaceableMockupPosition();
                PlaceableScanSurroundings();
            }


            yield return placementRefreshRate;
        }
    }

    

    public void PlaceableMockupPosition()
    {
        var mousePos = Input.mousePosition;

        RaycastHit hitInfo;
        var ray = gM.cameraManager.mainCamera.ScreenPointToRay(mousePos);

        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {


            if (Physics.Raycast(ray, out hitInfo, 500f, dragSelect_TilesLayerMask))
            {
                if (hitInfo.collider.gameObject.CompareTag("Tile"))
                {
                    //move to target position
                    if (placeableItemGFX_Mockup && currentPlaceableDevice)
                    {

                        Vector3 newPosition = hitInfo.collider.gameObject.transform.position;
                        newPlaceablePosition = newPosition;
                        newPosition.y += placeableMockupGFX_positionY;
                        placeableItemGFX_Mockup.transform.position = newPosition;
                    }
                }

            }
        }
    }

    bool PlaceableScanSurroundings() 
    {
        Physics.SyncTransforms();

        if (!currentPlaceableDevice) //annoying error
        {
            PlaceableScanPass();
            return false;
        }

        Vector3 size = new Vector3(currentPlaceableDevice.sizeDimensionX / 2f, 2f, currentPlaceableDevice.sizeDimensionZ / 2f);
        Collider[] collider = Physics.OverlapBox(placeableItemGFX_Mockup.transform.position, size, placeableItemGFX_Mockup.transform.rotation, placeableObjectLayerMask );

        if (collider.Length > 0)
        {
            PlaceableScanDeny();
            return false;
        }
        else 
        {
            PlaceableScanPass();
            return true;
        }
            
    }

    void PlaceableScanPass() 
    {
        placeableItemGFX_Mockup.SetPrimaryMaterial(placeablePositiveMaterial);
    }

    void PlaceableScanDeny()
    {
        placeableItemGFX_Mockup.SetPrimaryMaterial(placeableNegativeMaterial);
    }

    public void PlaceableConfirm()
    {
        //if not abstract, create a job to pack and move the item, then unpack
        if (currentPlaceableDevice.placeableType == PlaceableDevice.PlaceableType.ImmediatePlacement) 
        {
            currentPlaceableDevice.transform.position = newPlaceablePosition;
            currentPlaceableDevice.transform.rotation = placeableItemGFX_Mockup.transform.rotation;
            currentPlaceableDevice.PlaceConfirm();

            currentPlaceableDevice.currentPlaceableItemGFX.ResetMaterial();
            currentPlaceableDevice.currentPlaceableItemGFX.ObjectPlaced();

            ObjectPlacingStop();
        }
        else if (currentPlaceableDevice.placeableType == PlaceableDevice.PlaceableType.PhysicalPlacement) 
        {
           
            if (currentPlaceableDevice) 
            {
                Debug.Log("xaxa Has currentPlaceableDevice");
                currentPlaceableDevice.PlaceConfirm();
                SuitableWorker().work.New_Personal_ItemInteractionWorkOrder(currentPlaceableDevice.currentItemContainer, gM.localMapManager.GetNearbyFreeTile(newPlaceablePosition), WorkOrder.WorkOrderType.Hauling);
                ObjectPlacingStop();
            }
            
        }
        

       


    }

    public void CarryThisItemToInventory(ItemContainer itemC, Transform inventoryLocalOwner, Inventory inventory) 
    {

    }

    CharacterLocalMap SuitableWorker() 
    {
        return selectedCharactersList[Random.Range(0, selectedCharactersList.Count)];
    }

    void DestroyPlaceableMockup()
    {
        if (placeableItemGFX_Mockup)
        {
            Destroy(placeableItemGFX_Mockup.gameObject);
        }

    }

    void ResetCurrentPlaceableDevice()
    {
        currentPlaceableDevice = null;
    }

    public void ObjectPlacingStop() //ESC BUTTON TOO
    {
        if (objectPlacingRunning)
        {
            StopCoroutine(ObjectPlacingCR());
            objectPlacingRunning = false;
            ResetCurrentPlaceableDevice();
            DestroyPlaceableMockup();
        }

    }

    //RETURN
    public void Return_Click()
    {
        if (gM.GUIManager.universalGUI.companyControlMenuUI.UI_Enabled() || gM.GUIManager.universalGUI.companyControlMenuUI.currentCCMenuUI != CompanyControlMenuUI.CCMenuUI.None) //can close this ui only when local map is enabled
        {
            gM.GUIManager.universalGUI.ReturnButton();
        }

        else if (currentSelectionMode == ControlsInputManager.SelectionMode.ATTACK_TARGET) 
        {
            currentSelectionMode = ControlsInputManager.SelectionMode.DEFAULT;
        }


        else if (gM.GUIManager.localMapGUI.currentlyOpenUI != LocalMapGUI.CurrentlyOpenUI.None) 
        {
            if (DirectInteraction())
            {
                gM.GUIManager.localMapGUI.directInteractionsManager.CloseUI();
            }
            if (gM.GUIManager.localMapGUI.currentlyOpenUI == LocalMapGUI.CurrentlyOpenUI.LocalInventoryMenu) 
            {
                gM.GUIManager.localMapGUI.localInventoryMenuUI.CloseUI();
            }
            else 
            {
                if (gM.GUIManager.localMapGUI.currentlyOpenUI == LocalMapGUI.CurrentlyOpenUI.ActionButtonSubMenus)
                {
                    gM.GUIManager.localMapGUI.subMenusUI.CloseCurrentSubMenu();
                }
                else if (gM.GUIManager.localMapGUI.currentlyOpenUI == LocalMapGUI.CurrentlyOpenUI.SelectedEntityInteractions)
                {
                    currentSelectionMode = ControlsInputManager.SelectionMode.DEFAULT;
                    gM.GUIManager.localMapGUI.selectedEntityInteractionsUI.CloseAllUIs();
                    Unselect();
                }
                else if (gM.GUIManager.localMapGUI.currentlyOpenUI == LocalMapGUI.CurrentlyOpenUI.SSA_ItemSettings)
                {
                    gM.GUIManager.localMapGUI._SSA_ItemSettingsUI.CloseUI();
                }
            }

            
        }
    }

    public void Tab_Click() 
    {
        if (gM.currentGameState == GameManager.GameState.LocalMapMission) 
        {
            gM.GUIManager.localMapGUI.actionButtonBottomBarUI.BTN_HQ();
        }
    }

    bool DirectInteraction() 
    {
        if (itemC_DI && shipLocal_DI) 
        {
            return true;
        }
        return false;
    }

    public bool Input_Sprint() 
    {
        return _input.sprint;
    }

}