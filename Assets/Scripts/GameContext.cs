using System.Collections;
using Boundary.Player;
using Boundary.UI;
using Boundary.UI.GameMenu;
using Boundary.UI.StatueMenu;
using Control.Damage;
using Control.Damage.UseCase;
using Control.DialogueHandler;
using Control.GameData;
using Control.Inventory;
using Control.Pit;
using Control.Player;
using Control.Quests;
using Control.Shop;
using Infra.EventBus;
using Infra.SceneHandler;
using UnityEngine;

public class GameContext: MonoBehaviour
{
    public static GameContext Instance { get; private set; }
    
    // Models
    public IInventoryModel Inventory { get; private set; }
    public IPlayerModel Player { get; private set; }
    public IGameDataModel GameData { get; private set; }
    public IPitModel Pit { get; private set; }
    public IQuestModel Quests { get; set; }
    public IShopModel ShopModel { get; set; }
    
    // Handlers
    public IDialogueHandler DialogueHandler { get; set; }
    public IStatueMenuHandler StatueMenuHandler { get; set; }
    public IGameMenuHandler GameMenuHandler { get; private set; }
    
    // Infrastructure
    public IEventBus EventBus { get; private set; }
    private ISceneHandler SceneHandler { get; set; }
    
    // Use Cases / Services
    public UCombatSystem CombatSystem { get; private set; }
    public PlayerConditionHandler PlayerConditionHandler { get; private set; }

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize infrastructure
        EventBus = new EventBus();
        SceneHandler = GetComponentInParent<SceneHandler>();
        DialogueHandler = GetComponentInParent<DialogueHandler>();
        StatueMenuHandler = GetComponentInParent<StatueMenuHandler>();
        GameMenuHandler = new GameMenuHandler(EventBus);
        
        // Initialize models
        Inventory = new InventoryModel(EventBus);
        Pit = new PitModel(EventBus, Inventory);
        Player = new PlayerModel(EventBus, Pit);
        Quests = GetComponentInParent<QuestModel>();
        ShopModel = new ShopModel(EventBus, Player);
        
        // Initialize use cases / services
        CombatSystem = new UCombatSystem(Player, Inventory);
        PlayerConditionHandler = new PlayerConditionHandler(EventBus);
        
        // Initialize GameData (needs Quests for saving quest states)
        GameData = new GameDataModel(EventBus, Inventory, Player, SceneHandler, DialogueHandler, ShopModel, Quests);
        
        // Static global classes
        ItemUsageController.Initialize(Player, GameData);
        
        Debug.Log("GameContext initialized");
        
        SubscribeToEvents();
        
        // TODO: Here we should present the main menu instead of starting a new game directly
        StartCoroutine(AutomaticNewGameStart());
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();

        DialogueHandler.Dispose();
        Quests.Dispose();
        ShopModel.Dispose();
    }

    private IEnumerator AutomaticNewGameStart()
    {
        // Wait one frame to ensure all systems are initialized
        yield return null;
        
        EventBus.Publish(new ENewGameStartUiCommand());
    }

    private void SubscribeToEvents()
    {
        EventBus.Subscribe<EGameOverSequenceFinished>(OnGameOverSequenceFinished);
    }

    private void UnsubscribeFromEvents()
    {
        EventBus.Unsubscribe<EGameOverSequenceFinished>(OnGameOverSequenceFinished);
    }

    #region Event Handlers

    // Fired only after PlayerController's death visual sequence finishes (bounce, fall, offscreen) -
    // quitting immediately on EGameOver would cut the animation off mid-play.
    private static void OnGameOverSequenceFinished(EGameOverSequenceFinished e)
    {
        // DEBUG: Quit execution
        Debug.Log("Game Over sequence finished in GameContext. Quitting application.");
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    #endregion
}