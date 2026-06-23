# CLAUDE.md — Life Eaters: Project Reference

> Questo file è il punto di riferimento centrale per Claude (e chiunque lavori sul progetto).
> Comprende architettura, sistemi di gioco, narrative e note tecniche.
> **Aggiornalo** ogni volta che aggiungi zone, personaggi, sistemi o meccaniche rilevanti.

---

## 🎮 Overview del Progetto

| Campo | Valore |
|---|---|
| **Nome** | Life Eaters |
| **Engine** | Unity (2D URP) |
| **Genere** | 2D Platformer + RPG Lite + Metroidvania |
| **Stato** | Prototipo (Zona 1 e Zona 2 completate senza grafica definitiva) |
| **Lingua codebase** | C# |
| **Sistema dialoghi** | Ink (runtime + variabili sincronizzate) |
| **File di save** | JSON in `Application.persistentDataPath`, fino a 6 slot |

---

## 📁 Struttura Cartelle — Assets/Scripts/

```
Scripts/
├── GameContext.cs              ← Singleton root, inizializza TUTTO
├── Control/                    ← Business logic pura (no MonoBehaviour dove possibile)
│   ├── Player/                 ← PlayerModel, IPlayerModel, UseCase (LevelUp, CalcAttributes)
│   ├── Inventory/              ← InventoryModel, IInventoryModel
│   ├── DialogueHandler/        ← DialogueHandler, InkVariables, InkExternalFunctions
│   ├── Quests/                 ← QuestModel, IQuestModel
│   ├── Pit/                    ← PitModel, IPitModel
│   ├── Shop/                   ← ShopModel, IShopModel
│   ├── GameData/               ← GameDataModel, IGameDataModel (save/load)
│   ├── Effects/                ← EffectCalculator (applica effetti consumabili/equipment)
│   └── Damage/                 ← UCombatSystem, PlayerConditionHandler
├── Boundary/                   ← MonoBehaviour Unity components
│   ├── GamePlay/Player/        ← PlayerController (movement), GrabObjects, PlayerWeekArea
│   ├── GamePlay/Enemy/         ← BaseEnemy, StateMachine, Behaviors SO, TriggerChecks
│   ├── GamePlay/Projectile/    ← Projectile, ProjectilePool, TrackingProjectileController
│   ├── GamePlay/               ← Door, Statue, Life, Chest, Loot, Pit, CombatRoom...
│   ├── Camera/                 ← RoomCamera, CombatRoom, CameraShake
│   ├── Commands/               ← UserInput (tutto il mapping input)
│   └── UI/                     ← Dialogue, GameMenu, StatueMenu, Shop, PlayerStatus...
├── Data/                       ← Entità, ScriptableObjects, Database
│   ├── Entities/Player/        ← PlayerCharacteristics, PlayerAttributes, PlayerStatus
│   ├── Entities/Item/          ← Key, Talent, Equipment, Essence, PitObject, Consumable, Life
│   ├── Entities/Enemy/         ← EnemyModel, EnemyStatus, EnemyPhase, LifeDrop
│   ├── Entities/Effects/       ← EffectData, EffectType, EffectValueType
│   ├── Entities/Projectile/    ← ProjectileData, ProjectileVisualConfig, ProjectileType (enums)
│   ├── Entities/Game/          ← GameSessionData (struttura save file completa)
│   └── Database/               ← DataSource (registry di tutti i DB ScriptableObject)
├── Infra/                      ← Infrastruttura
│   ├── EventBus/               ← EventBus<T> (pub/sub generico)
│   ├── SceneHandler/           ← Caricamento additivo scene, SceneMountConfig
│   └── TilemapController/
└── Utils/                      ← Constants, AnimationStrings, CoroutineRunner, Functions
```

---

## 🏗️ Architettura

Il progetto segue un pattern **Control / Boundary / Data** (simile a Clean Architecture):

- **Control**: logica di business pura. Non dipende da Unity API dove possibile.
- **Boundary**: MonoBehaviour che interagiscono con Unity (fisica, input, UI, camera).
- **Data**: entità serializzabili, ScriptableObject, database.
- **Infra**: EventBus, SceneHandler.

La comunicazione tra sistemi avviene tramite un **EventBus** pub/sub generico (`EventBus.cs`).
Non ci sono riferimenti diretti tra sistemi — tutto passa per eventi o interfacce.

### GameContext
`GameContext.cs` è il singleton root (`DontDestroyOnLoad`). Instanzia e rende disponibili:
- `Player` (IPlayerModel)
- `Inventory` (IInventoryModel)
- `Dialogue` (IDialogueHandler)
- `Quest` (IQuestModel)
- `Pit` (IPitModel)
- `Shop` (IShopModel)
- `GameData` (IGameDataModel)
- `EventBus` (IEventBus)
- `CombatSystem` (UCombatSystem)

---

## 🎯 Meccaniche Core

### 1. Pit System (meccanica unica del gioco)
Il player equipaggia **Essenze** (ottenute consumando "Vite" dei nemici/boss).
Ogni Essenza contiene versioni probabilistiche di **Pit Objects** (oggetti piazzabili nel terreno).
Il player genera oggetti Pit spendendo Energia. Gli oggetti possono essere lanciati contro i nemici.

- `PitModel.cs` — logica generazione, costo energia, ciclo oggetti desiderati
- `PitSpawner.cs` / `Pit.cs` — spawning fisico Unity
- Pit objects (prefab): `WheelPitObject`, `WoolPitObject`, `ClayBlockPitObject`

**Struttura `PitObjectData`:**
```csharp
PitObjectId id;
PitObjectPrimaryType primaryType;    // Attack | Defense | Bonus
PitObjectSecondaryType secondaryType; // Melee | Ranged | Shield | Contact | Tools
PitObjectCategory category;          // Insect | Explosive | Structural | Food | Magical | Plants | Dust | Miscellaneous
DamageOutput damage;
int weight, difficulty;
PitObjectRarity rarity;              // Common | Uncommon | Rare | Epic | Legendary
float energyCost;
GameObject @object;                  // prefab fisico
```

**Pit Objects esistenti:** `ClayBlock`, `Wheel`, `Wool`

### 2. Essence / Life System
Le "Vita" (life-souls) sono droppate da NPC/nemici/boss.
Consumarle dà al player un'**Essenza** con versioni di Pit Objects randomizzati **solo se** la Vita ha un'`associatedEssence` diversa da `Empty`.

**Struttura `EssenceItem`:**
```csharp
EssenceId id;          // Empty | Shepherd | Miner | Mechanical | SectSoldier1 | Patient
EssenceType type;      // Standard | Behavioural
int turnOutEarn;
List<PitObjectGeneration> generations;  // { PitObjectId id, float chance }
```

**`EssenceVersion`** = snapshot randomizzata con lista concreta di PitObjectId.

**Essenze esistenti:** 
- **Standard**: Shepherd, Miner, Mechanical, SectSoldier1, Patient
- **Behavioural**: LazyPerson (riduce di 2 tile il quadrato di analisi per lo spawn dei pit, rendendoli più vicini al player)

**Differenze tra essenze Standard e Behavioural:**
- **Standard**: Generano versioni randomizzate di Pit Objects che possono essere spawnati e lanciati. Si equipaggiano negli slot S1-S5 (5 slot totali).
- **Behavioural**: Non generano oggetti, ma forniscono **effetti passivi** legati al sistema Pit. Si equipaggiano negli slot B1-B3 (3 slot totali). Non hanno versioni diverse per essenza.

**Layout slot (totale 8)**:
- Index 0: S1 (Standard)
- Index 1: S2 (Standard)
- Index 2: B1 (Behavioural)
- Index 3: S3 (Standard)
- Index 4: B2 (Behavioural)
- Index 5: S4 (Standard)
- Index 6: B3 (Behavioural)
- Index 7: S5 (Standard)

**LifeItem:**
```csharp
LifeId id;                      // ClayOperatorLife | WheelFitter | ShepardLife | Life | PrematureLife | SectSoldierLife | BlueCultLeaderLife
List<EffectData> effects;       // effetti aggiuntivi al consumo
EssenceId associatedEssence;    // Empty = non crea essenza
```

### 3. Sistema Effetti
`EffectCalculator` (in `Control/Effects/`) applica `EffectData` al player.

**Effetti implementati:**

| EffectType | Comportamento |
|---|---|
| `Heal` | Cura HP (statico o % MaxHealth) |
| `FocusGain` | Recupera Energia |
| `LifeGain` | Aggiunge Vite (con probabilità) |
| `CoinsCollected` | Aggiunge monete |
| `PointsCollected` | Aggiunge punti |
| `DefenseBuff` | Buff Difesa (temporaneo / permanente / fino alla morte) |
| `BuffDropRate` | Buff Drop Rate (temporaneo o permanente) |
| `BuffFrostResistance` | Buff FrostResistance (temporaneo o permanente) |
| `ReturnToStatue` | Teletrasporta all'ultima statua visitata |
| `UnveilStandardHiddenWalls` | Rivela muri nascosti per `Duration` secondi |
| `SaveGame` | Salva automaticamente sull'ultimo slot |
| `KillPlayer` | Uccide il player (forza HP a 0) |
| `Poisoned` | Applica poison build-up (permanente o temporaneo) |
| `Burned` | Applica burn build-up (permanente o temporaneo) |
| `Frostbitten` | Applica frost build-up (permanente o temporaneo) |
| `EnergyConsumption` | Consuma energia del player (valore negativo) |
| `EnergyRecovery` | Recupera energia del player |

**Effetti definiti ma NON ancora gestiti in `EffectCalculator` (default → warning):**
`PhysicalDamage`, `FireDamage`, `FrostDamage`, `PoisonDamage`, `StunDamage`

**`EffectData` struttura:**
```csharp
EffectType Type;
EffectValueType ValueType;  // Static | Percentage
float Value;
float Duration;             // 0 = istantaneo, >0 = temporaneo
bool UntilDeath;            // Effetto valido finché il player non perde una vita
float Probability;          // 0-1, usato da LifeGain
```

### 4. Sistema Dialogo (Ink)
- File `.ink` compilati in JSON, caricati come `TextAsset`
- `DialogueHandler.cs` gestisce knot navigation, choices, variable sync, save/load
- `InkVariables.cs` — sync bidirezionale variabili Ink ↔ stato di gioco
- `InkExternalFunctions.cs` — funzioni C# chiamabili da Ink: `AdvanceQuest(npcName)`, `OpenShop(shopName)`, `NewTalent(talentName)`


### 5. Sistema Quest (Souls-like)

Le quest non sono esplicitate in nessun menu. Il giocatore deve ricordarsi da solo il progresso di ogni NPC.

**Architettura:**
- `QuestModel.cs` traccia lo stato di ogni NPC (indice fase corrente).
- `NpcQuestData` (ScriptableObject) definisce:
  - `questPhases`: lista di `NpcQuestPhase` contenenti `phaseName`, `sceneName`, `npcGameObjectId`
- `NpcQuestState` mantiene l'indice della fase corrente di ogni NPC nel runtime
- Avanza via `AdvanceQuest(npcName)` chiamata da Ink

**Sistema di movimento NPC:**

Gli NPC possono cambiare posizione tra le scene quando la quest avanza. Le informazioni di posizione sono **integrate direttamente nelle fasi della quest**.

**Struttura `NpcQuestPhase`:**
```csharp
string phaseName;        // Nome della fase (es: "initial", "helped_player")
string sceneName;        // Nome scena dove appare l'NPC (vuoto = nascosto)
string npcGameObjectId;  // Id del GameObject NPC nella scena (da IdentifiableMonoBehaviour)
```

**Flusso:**
1. In Ink, chiami `AdvanceQuest("npcName")` → aggiorna la fase quest in memoria
2. Player riposa alla statua (o carica il gioco) → pubblica `EPlayerRestOnStatue`
3. Ogni componente `Npc`:
   - In fase `Start()`: legge la fase corrente via `GetNpcCurrentPhase()`
   - Su `EPlayerRestOnStatue`: rilegge la fase corrente e aggiorna visibilità
4. L'NPC si attiva solo se il suo `Id` corrisponde al `npcGameObjectId` della fase corrente

**Come configurare un NPC che si muove:**

1. Crea più GameObject `Npc` nelle varie scene con lo stesso `npcName` ma `Id` diversi
2. Nel `NpcQuestData` SO, aggiungi una `NpcQuestPhase` per ogni fase:
   ```
   questPhases[0]:
     phaseName: "initial"
     sceneName: "zone1"
     npcGameObjectId: "merchant_z1"
   
   questPhases[1]:
     phaseName: "moved_to_city"
     sceneName: "city"
     npcGameObjectId: "merchant_city"
   ```
3. Gli NPC in altre posizioni si disattiveranno quando il player riposa/ricarica

**Vantaggi dell'approccio:**
- ✅ Singolo punto di verità: tutte le info fase in `NpcQuestData`
- ✅ Funziona immediatamente allo `Start()` (load/scene change)
- ✅ Movimento avviene solo su rest/load (come enemies respawn)
- ✅ Nessun dato aggiuntivo da salvare in `GameSessionData`

### 6. Save System (Souls-like)
- Si salva riposando alle **Statue** (statue = bonfire)
- JSON salvato in `Application.persistentDataPath/GameDataSlotN.save`
- `GameSessionData.cs` contiene l'intera struttura serializzabile

### 7. Combat
- Attacchi: **Jump Attack** (piombare sui nemici) e **Throw Attack** (lanciare oggetti Pit)
- `UCombatSystem.cs` — calcolo danno con difesa, phase transitions, bonus equipaggiamento (`MeteorPendant`)
- Status effects: **Poison** (DOT), **Burn** (timer), **Frost** (accumulato) con resistenze derivate

### 8. RPG System

**Caratteristiche** (6 primarie):

| Caratteristica | Descrizione | Attributi derivati |
|---|---|---|
| `Vitality` | Vitalità | MaxHealth, Defense |
| `Strength` | Forza | ThrowAttackBaseDamage |
| `Agility` | Agilità | DashDuration, JumpAttackBaseDamage |
| `Human` | Umanità | DropRate, CritRate, Oratory, PoisonResistance |
| `God` | Divinità | MaxEnergy, PitNumber (slot Pit), BurnResistance |
| `Alien` | Alienità | MaxLifes, AlienOratory, FrostResistance |

**Attributi derivati** (19):

| Attributo | Descrizione |
|---|---|
| `MaxHealth` | Punti salute massimi |
| `MaxLifes` | Vite massime |
| `MaxEnergy` | Energia massima (per generare oggetti Pit) |
| `JumpAttackBaseDamage` | Danno base attacco Jump |
| `ThrowAttackBaseDamage` | Danno base attacco Throw |
| `RipAttackBaseDamage` | Danno base attacco Rip |
| `Defense` | Difesa (riduce danno ricevuto) |
| `DashDuration` | Durata dash |
| `Recovery` | Velocità recupero energia |
| `BurnResistance` | Resistenza al fuoco |
| `PoisonResistance` | Resistenza al veleno |
| `FrostResistance` | Resistenza al gelo |
| `DropRate` | Probabilità drop oggetti |
| `CritRate` | Probabilità colpo critico |
| `PitNumber` | Numero slot Pit disponibili |
| `Oratory` | Eloquenza umana (dialoghi) |
| `AlienOratory` | Eloquenza aliena (dialoghi) |
| `Grabbing` | Capacità afferrare oggetti pesanti |
| `EssenceSlots` | Slot essenze equipaggiabili |

**Talenti** (abilità sbloccabili): 
- **Invincibility**: Rende il player immune a tutti i danni nemici quando equipaggiato
- Run, Jump, DoubleJump, Dash, WallJump, Grab, Throw, FlyingDash, Smash, PillEater, Swim, Teleport, Headbutt, Climber, Mining, Stop, HookUp, HumanPray, AlienPray, HumanOratory, AlienOratory, Drain, Focus, Transfer, Metamorphosis, **LifeEater**, **Pit**, ecc.

### 9. Sistema Proiettili Nemici

I nemici non hanno melee attack. Attaccano con **cariche** (chase con contactDamage) e **proiettili ranged** tramite il sistema descritto qui.

#### Grammar Visiva
| Dimensione visiva | Comunica | Valori |
|---|---|---|
| **Colore** | Tipo di effetto | Bianco=HP / Verde=Veleno / Rosso=Burn / Azzurro=Frost |
| **Saturazione** | Tier di pericolo (basso) | Pastello → Vivido (0.3 → 2.0) |
| **Glow/Alone** | Tier di pericolo (alto) | Nessuno → Sottile → Pulsante |
| **Outline** | Fisica | Nessuno=Ballistic / Netto=Physical |
| **Scia (Trail)** | Supporto lettura | Presente=Ballistic / Assente=Physical |

#### Tier visivi (5 livelli)
| Tier | Saturazione | Glow | GlowPulse |
|---|---|---|---|
| I | 0.3 | 0.0 | false |
| II | 1.0 | 0.0 | false |
| III | 1.3 | 0.3 | false |
| IV | 1.6 | 0.6 | true |
| V | 2.0 | 1.0 | true |

#### Tipi fisici
- **Ballistic**: `GravityScale=0`, viaggio lineare, nessun outline, ha TrailRenderer. NON interagisce coi muri.
- **Physical**: `GravityScale>0`, arco parabolico, outline solido, no trail. Si ferma/rimbalza sui muri (layer Ground/Wall).

#### File chiave
```
Data/Entities/Projectile/
├── ProjectileType.cs          ← enum: Ballistic | Physical | DamageTier I-V | EffectColor
├── ProjectileData.cs          ← SO: dati completi proiettile (visual + damage + movement)
└── ProjectileVisualConfig.cs  ← SO: parametri shader per tier (1 SO per tier = 5 asset totali)

Boundary/GamePlay/Projectile/
├── Projectile.cs              ← MonoBehaviour: Init(), ReturnToPool(), visual via MaterialPropertyBlock
├── ProjectilePool.cs          ← Singleton: ObjectPool<Projectile> Unity built-in
└── TrackingProjectileController.cs ← Componente aggiunto a runtime per proiettili homing

Boundary/GamePlay/Enemy/Behaviors/RangedAttack/
├── EnemyRangedAttackPatternSO.cs   ← Behavior SO che integra col sistema nemici esistente
└── Patterns/
    ├── RangedAttackPatternSOBase.cs ← abstract SO base
    ├── SingleShotPatternSO.cs       ← 1 proiettile verso player
    ├── SpreadShotPatternSO.cs       ← ventaglio simultaneo (count + angle)
    ├── BurstShotPatternSO.cs        ← sequenza ritardata (count + delay + direction)
    ├── WaveShotPatternSO.cs         ← ondate di spread (waves + perWave + delay)
    ├── OrbitalShotPatternSO.cs      ← ring 360° (count + rotationOffset)
    └── TrackingShotPatternSO.cs     ← proiettile homing (duration + strength)
```

#### Lifecycle proiettile
- Colpisce player → pubblica `EEnemyBulletHitPlayer` + `EProjectileEffectHitPlayer` per ogni effetto → ReturnToPool
- Colpisce muro/terreno (solo Physical) → ReturnToPool
- Timer `Lifetime` scaduto → ReturnToPool
- `EPlayerEnteredRoom` / `EPlayerEnteredCombatRoom` → tutti i proiettili attivi → ReturnToPool
- Nemico muore → **i proiettili già sparati restano attivi**

#### Shader (da creare in Shader Graph URP)
File: `Assets/Materials/Projectile/ProjectileShader.shadergraph`
Parametri esposti: `_BaseColor`, `_Saturation`, `_GlowRadius`, `_GlowPulse`, `_OutlineWidth`
Tutti settati a runtime via `MaterialPropertyBlock` — un solo Material, nessuna duplicazione.

#### Sprite base
File: `Assets/Sprites/Projectile/projectile_base.png`
Specifiche: 64×64px, PNG con alpha, cerchio bianco con sfumatura radiale centro→bordi trasparenti.
Il bianco puro consente al `_BaseColor` dello shader di moltiplicarsi su qualsiasi colore target.

#### Aggiungere un nuovo pattern di sparo
1. Crea SO estendendo `RangedAttackPatternSOBase`
2. Aggiungi `[CreateAssetMenu(..., menuName = "Enemies/Behaviors/Ranged Attack Patterns/...")]`
3. Implementa `Execute(BaseEnemy enemy, ProjectilePool pool)`
4. Usa helpers della base: `GetSpawnPosition()`, `DirectionToPlayer()`, `Rotate()`

#### Aggiungere un nuovo tipo di proiettile
1. Crea un asset `ProjectileData` SO (Assets/Resources o apposita cartella)
2. Assegna `ProjectileType`, `DamageTier`, `EffectColor`, `VisualConfig`, `DamageOutput`, `OnHitEffects`
3. Crea/riusa un asset `ProjectileVisualConfig` SO per il tier scelto
4. Assegna il `ProjectileData` al campo `ProjectileData` del `RangedAttackPatternSO`

### 10. Scene / Metroidvania- Scene caricate in modo **additivo** (Metroidvania room system)
- `SceneHandler.cs` + `SceneMountConfig.cs`
- Scena persistente: `"GamePlay"`, Prima scena: `"1.1_parent_house"`

---

## 👾 Sistema Nemici

### Architettura (Strategy + State Machine)

Ogni nemico è un `BaseEnemy : IdentifiableMonoBehaviour, ITriggerCheckable`.
La logica di comportamento è completamente **data-driven via ScriptableObject**.

```
BaseEnemy
  ├── EnemyStateMachine           ← gestisce transizioni di stato
  ├── ConcreteStates (6)          ← delegano TUTTO al SO corrispondente
  │   ├── EnemyDormantState
  │   ├── EnemyIdleState
  │   ├── EnemyChaseState
  │   ├── EnemyMeleeAttackState
  │   ├── EnemyCooldownState
  │   └── EnemyRangedAttackState
  └── Behavior SO (6 slot serializzati nell'Inspector)
      ├── EnemyDormantSOBase
      ├── EnemyIdleSOBase
      ├── EnemyChaseSOBase
      ├── EnemyMeleeAttackSOBase
      ├── EnemyCooldownSOBase
      └── EnemyRangedAttackSOBase
```


La base `EnemyIdleSOBase` contiene già le transizioni di default:
- `IsAggroed` → `ChasingState`
- `InMeleeAttackRange` → `MeleeAttackState`
- `InRangedAttackRange` → `RangedAttackState`

Disabilitabili con i flag: `disableAggroCheck`, `disableMeleeAttackRangeCheck`, `disableRangedAttackRangeCheck`.

**Attenzione:** i SO vengono istanziati con `Instantiate(so)` in `Awake()` per evitare stato condiviso tra nemici.

### Tipi di Nemico (`EnemyType`)
| Tipo | Respawn al riposo statua |
|---|---|
| `Regular` | ✅ |
| `Unique` | ✅ |
| `CombatRoom` | ❌ |
| `Boss` | ❌ |

### Fasi (`EnemyPhase`)
`Phase1 / Phase2 / Phase3 / Phase4`
Transizione automatica quando `CurrentHealth <= Phase2HealthThreshold`.

### Dati nemico (`EnemyModel`)
```csharp
string Id;
int MaxHealth, CurrentHealth;
int JumpAttackDefense, ThrowAttackDefense;
int PointsDrop;
EnemyStatus CurrentStatus;    // Dead | Burned | Frozen | Poisoned | Normal
EnemyPhase CurrentPhase;
List<LifeDrop> PossibleLifeDrops;  // { LifeId lifeId, float dropChance }
```

### Behavior SO esistenti
| SO | MenuName Unity | Comportamento |
|---|---|---|
| `EnemyIdleStayStill` | Behaviors/Idle/Stay Still | Fermo, reagisce ad aggro/range |
| `EnemyIdleLookLeftAndRightSO` | Behaviors/Idle/Look Left And Right | Gira a intervalli |
| `EnemyIdleCasualPatrollingSO` | Behaviors/Idle/Causal Patrolling | Pattuglia con pause random |
| `EnemyChaseToPlayerSO` | Behaviors/Chase/Chase Direct To Player | Insegue il player, perde aggro → Idle. `chaseContinuationTime` configura quanto inseguire dopo perdita agro |
| `EnemyChaseDirectToPlayerUntilWallSO` | Behaviors/Chase/Chase Direct To Player Until Wall | Carica fino al muro → stun timer → Idle. Base per boss. `stunOnAttack` configura se si stordisce quando colpito |
| `EnemyChaseDirectToPlayerUntilWallWithStunDebuffSO` | Behaviors/Chase/..(Stunned Debuff) | Come sopra ma applica debuff JumpAttackDefense/ThrowAttackDefense durante lo stun |
| `EnemyChaseHorizontalTrackingSO` | Behaviors/Chase/Chase Horizontally Tracking | Si avvicina orizzontalmente e triggera Ranged quando allineato. Per nemici volanti/sparatori |
| `AnimationBasedMeleeAttackSO` | Behaviors/Melee Attack/Animation Based | Animazione attacco → cooldown o chase |
| `EnemyMeleeAttackDiveToPlayerSO` | Behaviors/Melee Attack/Enemy Dive To Player | Salto in direzione del player con forza. Fasi: Preparing → Diving → WaitingAfterDive → Finished |
| `StayStillCooldownSO` | Behaviors/Cooldown/Stay Still | Timer fermo → Idle |
| `EnemyDormantStayStillSO` | Behaviors/Dormant/Stay Still | Dormiente fermo → sveglia su `IsAwake` |
| `EnemyDormantAttachedToCeilingSO` | Behaviors/Dormant/Attached To Ceiling | Attaccato al soffitto (gravity=0, kinematic) → sveglia su `IsAwake` |
| `EnemyDormantSleepInSpecificPositionSO` | Behaviors/Dormant/Sleep In Specific Position | Si muove verso `sleepPosition` e rimane lì finché sveglio |
| `PopjueBearDormantStateSO` | Behaviors/Dormant/Popjue Bear Want To Sleep | Specializzazione per Popjue Bear: risponde a `EHomeRouteBridgeMoved` |


### Aggiungere un nuovo nemico (checklist)
1. Crea classe C# che estende `BaseEnemy` (può essere vuota)
2. Crea prefab: `BaseEnemy`-subclass + `Rigidbody2D` + `TouchingDirections` + `VisualTip`
3. Aggiungi figli nel prefab: `EnemyAggroCheck`, `EnemyMeleeAttackCheck`, (opzionali: `EnemyRangedAttackCheck`, `EnemyAwakeCheck`) con Collider2D trigger
4. Assegna Behavior SO nei 6 slot dell'Inspector
5. Configura: `MaxHealth`, `PointsDrop`, `JumpAttackDefense`, `ThrowAttackDefense`, `enemyType`, `PossibleLifeDrops`, `contactDamageOutput`, `MeleAttackDamageOutput`

### Aggiungere un nuovo Behavior SO
1. Estendi la base corretta (es. `EnemyIdleSOBase`, `EnemyChaseSOBase`)
2. Aggiungi `[CreateAssetMenu(..., menuName = "Enemies/Behaviors/...")]`
3. Override: `DoEnterLogic`, `DoExitLogic`, `DoFrameUpdateLogic`, `DoPhysicsLogic`, `OnReceiveDamage`
4. Accesso: `Enemy.Rb`, `Enemy.TouchingDirections`, `Enemy.walkDirectionVector`, `Enemy.Player`, `Enemy.Animator`
5. Cambio stato: `Enemy.StateMachine.ChangeState(Enemy.ChasingState)` ecc.

---


## 🔧 Note Tecniche Importanti

### EventBus — Pattern uso
```csharp
_eventBus.Subscribe<EMyEvent>(OnMyEvent);
_eventBus.Publish(new EMyEvent(param));
_eventBus.Unsubscribe<EMyEvent>(OnMyEvent); // sempre in OnDestroy/Dispose
```

### Aggiungere una nuova variabile Ink sincronizzata
1. Aggiungi costante in `Utils/Constants.cs` (prefisso `IV`)
2. Aggiungi caso in `CheckItemsInPossessions()` o `CheckPlayerStats()` in `DialogueHandler.cs`
3. Assicurati che la variabile esista nel `.ink` con lo **stesso nome esatto**

### Aggiungere un nuovo oggetto (chiave, consumabile, equipment, ecc.)
1. Aggiungi il valore nell'enum corrispondente (es. `KeyId`, `ConsumableId`, `EquipmentId`)
2. Aggiungi l'item nel database SO corrispondente
3. Se sincronizzato con Ink → aggiungi variabile in Constants + DialogueHandler

### Aggiungere una nuova essenza comportamentale
1. Aggiungi il valore in `EssenceId` enum in `Data/Entities/Item/Essence.cs`
2. Crea l'item nel database SO con `type = Behavioural` (non serve lista `generations`)
3. Implementa l'effetto passivo nel codice appropriato:
   - Per effetti sul pit spawn → modifica `PitSpawner.cs`
   - Per effetti su attributi player → modifica logica in `PlayerModel.cs`
   - Per altri effetti → crea logica specifica che controlla `InventoryModel.EquippedBehaviouralEssences()`
4. Documenta l'effetto in CLAUDE.md nella sezione "Essenze esistenti"

### Aggiungere un NPC con quest
1. Crea knot nel `.ink`: `npcName_questPhase`
2. Chiama `AdvanceQuest("npcName")` da Ink
3. `QuestModel` sincronizzerà automaticamente `<npcName>QuestState`

### Aggiungere un nuovo effetto
1. Aggiungi il valore in `EffectType` enum in `EffectData.cs`
2. Implementa il caso `switch` in `EffectCalculator.ApplyEffect()` in `Control/Effects/EffectsCalculator.cs`
3. Segui il pattern: istantaneo (modifica diretta) o temporaneo (`TemporaryBuff` coroutine)


---

## 🎨 Gestione Effetti

Per aggiungere un nuovo effetto, Claude implementerà direttamente il codice necessario in `EffectCalculator.cs`.

**Pattern disponibili:**
- **Istantaneo**: modifica diretta dello stato (es. Heal, PointsCollected)
- **Temporaneo con Duration**: usa `TemporaryBuff` coroutine (es. DefenseBuff, BuffDropRate)
- **Con UntilDeath**: l'effetto persiste fino alla perdita di una vita (es. DefenseBuff permanente)

**Step per implementazione:**
1. Aggiungi `EffectType` nell'enum in `Data/Entities/Effects/EffectData.cs`
2. Claude implementerà il caso nel `switch` di `EffectCalculator.ApplyEffect()`
3. Se temporaneo, utilizzerà il pattern `TemporaryBuff(apply, remove, duration)`


---

## 📋 Codice & Best Practices

