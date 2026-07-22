# Life Eaters — Gameplay Reference

> Documento di riferimento gameplay completo.
> Generato dalla codebase — Aggiornato al 2026-06-23.

---

## 01 · Overview e Core Loop

Life Eaters è un 2D platformer con elementi RPG e struttura Metroidvania. Il giocatore esplora ambienti interconnessi, affronta nemici, consuma le loro anime per potenziare il sistema Pit, e scala un sistema RPG di caratteristiche e talenti.

### Loop di gioco principale

1. **Esplora** le zone, apri porte, trova forzieri e NPC.
2. **Combatti** i nemici con jump attack e lanci di oggetti Pit.
3. **Raccogli Vite** (life-souls) droppate dai nemici sconfitti.
4. **Consuma Vite** per ottenere Essenze e sbloccare nuovi oggetti Pit.
5. **Riposa alle Statue** per salvare, ripristinare HP, e aggiornare la posizione degli NPC.
6. **Spendi punti** per aumentare caratteristiche e sbloccare talenti.

### Risorse del giocatore

| Risorsa | Range | Note |
|---|---|---|
| HP | `0 → MaxHealth` | Base: 3 |
| Vite (game-over counter) | `0 → MaxLifes` | Base: 2 |
| Energia | `0 → MaxEnergy` | Base: 3. Usata per generare Pit |

> **Nota sui termini:** "Vita" ha due significati. Le *Vite* (MaxLifes) sono i tentativi prima del Game Over. Le *Life Items* sono drop dei nemici che si consumano per ottenere Essenze — sono oggetti nell'inventario, non il contatore vite.

---

## 02 · Movimento e Controlli

Il movimento avanzato richiede talenti specifici. Quasi tutte le azioni sono bloccate finché il talento corrispondente non viene sbloccato.

### Velocità e fisica

| Parametro | Valore | Note |
|---|---|---|
| Walk Speed | 8 u/s | Velocità base senza Run |
| Run Speed | 12 u/s | Con talento `Run` |
| Dash Speed | 22 u/s | Solo a terra, talento `Dash` |
| Dash Duration | 0.18s base | Scalata dall'attributo DashDuration |
| Jump Force | 5f | Talento `Jump` |
| Jump Hold Time | 0.5s | Tenere premuto aumenta l'altezza |
| Coyote Time | 0.3s | Salta ancora 0.3s dopo aver lasciato il bordo |
| Wall Slide Speed | 2 u/s | Talento `WallJump` |
| Climb Speed | 8 u/s | Talento `Climber` |
| Gravity Scale | 7f | Caduta rapida |
| Jump Hang Accel Mult | 1.2x | Accelerazione durante la salita |

### Movimenti disponibili (richiedono talento)

| Movimento | Talento | Descrizione |
|---|---|---|
| Corsa | `Run` | Velocità aumentata a 12 u/s |
| Salto | `Jump` | Salto base con hold per più altezza |
| Double Jump | `DoubleJump` | Secondo salto in aria |
| Dash | `Dash` | Dash orizzontale a terra |
| Wall Jump | `WallJump` | Salto da parete + wall slide |
| Flying Dash | `FlyingDash` | Dash aereo |
| Smash | `Smash` | Attacco verso il basso in caduta |
| Headbutt | `Headbutt` | Testata in salita |
| Climb | `Climber` | Scalare superfici verticali |
| Swim | `Swim` | Nuotare in liquidi |
| Crouch | — | Disabilita collisione one-way platform per 0.25s |

### Knockback ricevuto

- Forza orizzontale: **25f**
- Forza verticale: **10f**
- Dopo danno ricevuto: **1.5s di invincibilità** (i-frames)

---

## 03 · Sistema di Combattimento

Il giocatore non ha un attacco melee standard. Il combattimento si basa su due meccaniche principali: il **Jump Attack** (piombare sui nemici dall'alto) e il **Throw Attack** (lanciare oggetti Pit contro i nemici).

### Jump Attack

Saltare e cadere su un nemico dall'alto causa danno e fa rimbalzare il giocatore verso l'alto. Richiede il talento `Jump`. Usabile più volte di fila rimbalzando sullo stesso nemico.

```
// Calcolo danno
damage = MAX(1, PlayerJumpAttackBaseDamage - EnemyJumpAttackDefense)

// Rimbalzo sul nemico
vertical_bounce_force = 20f

// Recupero energia su hit
currentEnergy += 1
```

Il Jump Attack può applicare status build-up se il giocatore ha gli attributi `JumpAttackPoisonBuildUp`, `JumpAttackBurnBuildUp`, `JumpAttackFrostBuildUp` > 0 (base: 0 per tutti).

### Throw Attack

Il giocatore afferra un oggetto Pit dal terreno e lo lancia contro i nemici. Richiede i talenti `Grab` e `Throw`. Il danno dipende sia dalle stat del giocatore che dall'oggetto lanciato.

```
// Calcolo danno
totalDamage  = PlayerThrowAttackBaseDamage + PitObjectDamage
damage       = MAX(1, totalDamage - EnemyThrowAttackDefense)

// Status build-up applicati dall'oggetto Pit
poisonBuildUp = PitObjectData.damage.poisonBuildUp
burnBuildUp   = PitObjectData.damage.burnBuildUp
frostBuildUp  = PitObjectData.damage.frostBuildUp

// Bonus speciale: ClayBlock + MeteorPendant equipaggiato
if (pitObject == ClayBlock && equipped(MeteorPendant)):
    objectDamage += objectDamage * 0.5f
```

### Limiti di sollevamento oggetti Pit

Il giocatore può afferrare un oggetto Pit solo se le sue caratteristiche lo consentono:

| Peso oggetto | STR minima richiesta | Difficoltà oggetto | AGI minima richiesta |
|---|---|---|---|
| ≤ 2 | 1 | ≤ 2 | 1 |
| ≤ 5 | 2 | ≤ 5 | 2 |
| ≤ 10 | 4 | ≤ 10 | 3 |
| ≤ 15 | 6 | ≤ 15 | 5 |
| > 15 | 8 | > 15 | 8 |

### Danno ricevuto dal giocatore

```
damageWithDefense = damage.healthDamage - AllAttributes.Defense
finalDamage = MAX(1, damageWithDefense)   // minimo 1 danno

// Eccezione: talento Invincibility → ignora tutto il danno
```

### Morte e penalità

Quando `currentHealth ≤ 0`:
- `currentLifes -= 1`
- HP ripristinato a `MaxHealth`
- **Penalità permanenti:** -1 PoisonResistance, -1 BurnResistance, -1 FrostResistance, -1 ThrowAttackBaseDamage, -1 JumpAttackBaseDamage, -1 Defense
- Se `currentLifes ≤ 0` → **Game Over**

### Proiettili nemici

I nemici non hanno attacchi melee standard. Attaccano tramite **cariche** (chase con contactDamage) e **proiettili ranged**. I proiettili hanno due tipi fisici:

| Tipo | Gravità | Traiettoria | Muri | Visual |
|---|---|---|---|---|
| Ballistic | 0 | Lineare | Non interagisce | Scia (Trail), no outline |
| Physical | > 0 | Arco parabolico | Si ferma/rimbalza su Ground/Wall | Outline solido, no trail |

---

## 04 · Caratteristiche (RPG)

Il sistema RPG è basato su 6 caratteristiche primarie. Ogni punto in una caratteristica aumenta un gruppo di attributi derivati tramite tabelle di crescita non lineari. Tutte partono da 1.

| Caratteristica | Codice | Attributi derivati | Tema |
|---|---|---|---|
| Vitality | `VIT` | MaxHealth, Defense | Resistenza fisica e sopravvivenza |
| Strength | `STR` | ThrowAttackBaseDamage | Potenza nel lanciare oggetti Pit |
| Agility | `AGI` | JumpAttackBaseDamage, DashDuration | Velocità e precisione del salto |
| Human | `HUM` | DropRate, CritRate, Oratory, PoisonResistance | Umanità, carisma, resistenza veleno |
| God | `GOD` | MaxEnergy, EssenceSlots, BurnResistance | Potere divino, capacità Pit, resistenza fuoco |
| Alien | `ALN` | MaxLifes, AlienOratory, FrostResistance | Natura aliena, vite extra, resistenza gelo |

### Tabelle di crescita (valori chiave)

| Caratt. | Lv 2 | Lv 5 | Lv 10 |
|---|---|---|---|
| VIT | +1 HP, +1 DEF | +3 HP, +3 DEF | +5 HP, +7 DEF |
| STR | +1 TATK | +4 TATK | +9 TATK |
| AGI | +1 JATK, +1 DASH | +3 JATK, +2 DASH | +9 JATK, +3 DASH |
| HUM | +1 DROP, +1 ORT, +1 PRES | +2 DROP, +3 ORT, +4 PRES | +5 DROP, +5 ORT, +9 PRES |
| GOD | +1 ESS, +1 BRES | +3 ESS, +4 BRES | +10 ESS, +9 BRES |
| ALN | +1 AORT, +1 FRES | +2 LF, +3 FRES | +7 LF, +5 AORT |

---

## 05 · Attributi Derivati

Calcolati dalla somma di: caratteristiche base + bonus equipment + buff temporanei attivi.

| Attributo | Descrizione | Base |
|---|---|---|
| MaxHealth | Punti salute massimi | 3 |
| MaxLifes | Vite disponibili prima del Game Over | 2 |
| MaxEnergy | Energia per generare oggetti Pit | 3 |
| JumpAttackBaseDamage | Danno base del Jump Attack | 1 |
| ThrowAttackBaseDamage | Danno base del Throw Attack | 1 |
| RipAttackBaseDamage | Danno base dell'attacco Rip | 1 |
| Defense | Riduce il danno ricevuto | 1 |
| DashDuration | Durata del dash | 1 |
| Recovery | Velocità recupero energia | 1 |
| BurnResistance | Soglia build-up per status Burned | 5 |
| PoisonResistance | Soglia build-up per status Poisoned | 5 |
| FrostResistance | Soglia build-up per status Frostbitten | 5 |
| DropRate | Probabilità drop oggetti | 1 |
| CritRate | Probabilità colpo critico | 1 |
| PitNumber | Slot Pit disponibili per generazione | 2 |
| Oratory | Eloquenza umana (sblocca dialoghi) | 1 |
| AlienOratory | Eloquenza aliena (dialoghi alieni) | 1 |
| Grabbing | Capacità di afferrare oggetti pesanti | 1 |
| EssenceSlots | Slot essenze equipaggiabili (scala con GOD) | 3 |
| JumpAttackPoisonBuildUp | Build-up veleno applicato dal Jump Attack | 0 |
| JumpAttackBurnBuildUp | Build-up fuoco applicato dal Jump Attack | 0 |
| JumpAttackFrostBuildUp | Build-up gelo applicato dal Jump Attack | 0 |

> `AllAttributes = Characteristics-derived Attributes + AdditionalAttributes (equipment) + buff temporanei attivi`

---

## 06 · Level Up

Il livello del personaggio è la **somma totale di tutte le caratteristiche**. Per aumentare una caratteristica si spendono *Punti* (la valuta XP del gioco), con un costo crescente calcolato dalla formula:

```
cost(level) = baseXP × (level - offset)^growth + level × scale

// Costanti
baseXP  = 250
offset  = 5
growth  = 1.7

// Scale dipende dal livello corrente
level ≤ 10 → scale = 50
level ≤ 15 → scale = 100
level ≤ 20 → scale = 200
level ≤ 25 → scale = 400
level >  25 → scale = 700
```

### Costi di riferimento

| Livello | Costo approssimativo |
|---|---|
| 6 | ~0 punti |
| 7 | ~300 punti |
| 10 | ~3.000 punti |
| 15 | ~13.500 punti |
| 20 | ~31.500 punti |

### Come si guadagnano i Punti

- Nemici uccisi (ogni nemico ha un `PointsDrop`)
- Oggetti collezionabili nel mondo (bonus, frutti, simboli, diamanti)
- Effetti di consumabili
- Eventi di gameplay specifici

---

## 07 · Sistema Talenti

I talenti sono abilità permanenti che sbloccano azioni, movimenti o bonus passivi. Si acquisiscono spendendo Punti, rispettando eventuali prerequisiti. Alcuni equipment conferiscono talenti aggiuntivi mentre equipaggiati.

### Talenti di movimento

| Talento | Effetto |
|---|---|
| `Run` | Abilita la corsa (12 u/s) |
| `Jump` | Abilita il salto |
| `DoubleJump` | Secondo salto in aria |
| `Dash` | Dash orizzontale a terra |
| `WallJump` | Salto da muro e wall slide |
| `FlyingDash` | Dash aereo |
| `Smash` | Attacco verso il basso in caduta |
| `Headbutt` | Testata verso l'alto |
| `Climber` | Scalare superfici verticali |
| `ExpertClimber` | Versione avanzata dell'arrampicata |
| `Swim` | Nuotare in liquidi |
| `Teleport` | Teletrasporto |

### Talenti di azione / Pit

| Talento | Effetto |
|---|---|
| `Grab` | Afferrare oggetti Pit dal terreno |
| `Throw` | Lanciare oggetti Pit contro i nemici |
| `Pit` | Generare oggetti Pit spendendo Energia |
| `ChargedThrow` | Lancio caricato con più potenza |
| `ThrowDown` | Lanciare verso il basso |
| `Pull` | Richiamare oggetti Pit a distanza |
| `PutDown` | Posare l'oggetto invece di lanciarlo |
| `Juggling` | Gestire più oggetti contemporaneamente |
| `Transporter` | Trasportare oggetti speciali |
| `Mining` | Estrarre materiali dal terreno |
| `HookUp` | Agganciarsi a superfici |

### Talenti passivi / speciali

| Talento | Effetto |
|---|---|
| `Invincibility` | **Rende il player immune a tutti i danni nemici** |
| `Invisibility` | Rende il player invisibile ai nemici |
| `Stop` | Ferma il tempo localmente |
| `PillEater` | Sblocca uso di consumabili speciali |
| `Shelter` | Costruire rifugi |
| `Gourmet` | Effetti potenziati dai consumabili |
| `LifeEater` | Meccanica core: consumare Vite dei nemici |
| `HumanPray`, `AlienPray` | Preghiere umane/aliene |
| `HumanOratory`, `AlienOratory` | Eloquenza avanzata in dialogo |
| `Drain` | Drenare energia dai nemici |
| `Focus` | Concentrazione per azioni speciali |
| `Transfer` | Trasferire effetti ad altri |
| `Metamorphosis` | Trasformazione del personaggio |

---

## 08 · Sistema Pit

La meccanica unica del gioco. Il giocatore genera oggetti fisici dal terreno spendendo Energia, li afferra e li lancia contro i nemici. Gli oggetti disponibili dipendono dalle Essenze equipaggiate.

### Flusso di utilizzo

1. Il giocatore preme il comando "Genera Pit" (talento `Pit` richiesto)
2. Il sistema calcola quali oggetti generare dalle Essenze Standard equipaggiate
3. Gli oggetti spawnano nel terreno vicino al giocatore (area rilevata via BoxCast)
4. Il giocatore si avvicina e preme "Grab" (talento `Grab`) per afferrare
5. Viene verificato **weight** vs STR e **difficulty** vs AGI
6. Il giocatore prende l'oggetto in mano, poi lancia (talento `Throw`)

### Costo energia generazione

```
energyCost = Σ( energyCost di tutti gli oggetti nelle essenze standard equipaggiate )
currentEnergy -= energyCost
```

### Oggetti Pit esistenti

| Oggetto | Tipo primario | Tipo secondario | Categoria | Note |
|---|---|---|---|---|
| `ClayBlock` | Attack | Melee | Structural | +50% danno con MeteorPendant |
| `Wheel` | Attack | Ranged | Miscellaneous | — |
| `Wool` | Defense | Shield | Miscellaneous | — |

### Struttura dati PitObjectData

```csharp
PitObjectId         id;
PitObjectPrimaryType   primaryType;    // Attack | Defense | Bonus
PitObjectSecondaryType secondaryType;  // Melee | Ranged | Shield | Contact | Tools
PitObjectCategory      category;       // Insect | Explosive | Structural | Food | Magical | Plants | Dust | Miscellaneous
DamageOutput           damage;         // hp, burnBuildUp, frostBuildUp, poisonBuildUp
int    weight;       // 1-15+ (check STR)
int    difficulty;   // check AGI
float  energyCost;
PitObjectRarity rarity; // Common | Uncommon | Rare | Epic | Legendary
GameObject @object;    // prefab fisico
```

---

## 09 · Sistema Essenze

Le Essenze si ottengono consumando Life Items (anime dei nemici). Ogni Essenza definisce quali oggetti Pit il giocatore può generare. Le Essenze si equipaggiano in 8 slot divisi in due tipi.

### Tipi di Essenza

| Tipo | Slot | Funzione |
|---|---|---|
| Standard | S1, S2, S3, S4, S5 | Generano versioni randomizzate di oggetti Pit. Hanno una lista di `PitObjectGeneration { PitObjectId, chance }`. |
| Behavioural | B1, B2, B3 | Non generano oggetti. Conferiscono effetti passivi sul sistema Pit o sugli attributi. |

### Layout degli 8 slot

```
Index 0: S1 (Standard)
Index 1: S2 (Standard)
Index 2: B1 (Behavioural)
Index 3: S3 (Standard)
Index 4: B2 (Behavioural)
Index 5: S4 (Standard)
Index 6: B3 (Behavioural)
Index 7: S5 (Standard)
```

Il numero di slot effettivamente sbloccati dipende dall'attributo `EssenceSlots` (scala con GOD).

### Essenze Standard esistenti

| Essenza | Pit Objects associati |
|---|---|
| Shepherd | Wool e oggetti da pastore |
| Miner | ClayBlock e oggetti da minatore |
| Mechanical | Wheel e oggetti meccanici |
| SectSoldier1 | Oggetti da soldato della setta |
| Patient | Oggetti da paziente |

### Essenze Behavioural esistenti

| Essenza | Effetto passivo |
|---|---|
| LazyPerson | Riduce di 2 tile il raggio di spawn pit (oggetti spawnano più vicini al player) |

### EssenceVersion

Quando si ottiene un'Essenza Standard, viene generata una `EssenceVersion`: uno snapshot randomizzato della lista concreta di `PitObjectId` basato sulle probabilità di generazione definite nell'Essenza. Ogni volta che il giocatore ottiene la stessa Essenza, può ricevere una versione con oggetti diversi.

---

## 10 · Sistema Vite (Life Items)

Le *Life Items* (anime) sono oggetti droppati da NPC, nemici e boss. Si trovano nell'inventario e si consumano volontariamente per ottenere Essenze e applicare effetti al giocatore.

> **Attenzione:** "Life Items" ≠ "Vite del giocatore" (MaxLifes). Le Life Items sono collezionabili consumabili; le vite del giocatore sono il contatore di game-over.

### Meccanica di consumo

- Consumare una Life Item applica gli `effects` associati al giocatore
- Se ha un `associatedEssence` diverso da `Empty`, genera una nuova Essenza nell'inventario
- Se `associatedEssence == Empty`, nessuna Essenza viene creata

### Life Items esistenti

| Life Item | Essenza generata |
|---|---|
| ShepardLife | Shepherd |
| ClayOperatorLife | Miner |
| WheelFitter | Mechanical |
| SectSoldierLife | SectSoldier1 |
| PrematureLife | Patient |
| BlueCultLeaderLife | variabile (drop da boss) |
| Life | Empty (solo effetti, nessuna Essenza) |

---

## 11 · Equipment

L'equipment si equipaggia in 4 slot: 1 **Body** (vestito) e 3 **Accessories** (Acc1, Acc2, Acc3). Ogni pezzo può modificare attributi, caratteristiche e concedere talenti aggiuntivi.

### Equipment esistenti

| Equipment | Slot | Effetti noti |
|---|---|---|
| Pajamas | Body | Abbigliamento base |
| MinerSuit | Body | Tuta da minatore |
| RingOfFlesh | Accessorio | Modifica attributi fisici |
| MeteorPendant | Accessorio | **+50% danno ClayBlock** nei Throw Attack |
| MithridatesRing | Accessorio | Modifica resistenza ai veleni |
| MuddyBoots | Accessorio | Modifica attributi di movimento |

### Struttura effetti equipment

```csharp
PlayerAttributes      attributesModifier;         // +/- su attributi derivati
PlayerCharacteristics characteristicsModifier;    // +/- su caratteristiche primarie
List<TalentId>        talentsModifier;            // Talenti aggiuntivi (attivi finché equipaggiato)
```

---

## 12 · Consumabili

I consumabili si usano dall'inventario e applicano uno o più `EffectData` al giocatore. Ogni uso riduce la quantità di 1; a 0 vengono rimossi dall'inventario.

| Consumabile | Funzione principale |
|---|---|
| HeartLeaf | Cura HP |
| SmallVioletClump | Recupero energia (piccolo) |
| VioletClump | Recupero energia |
| LargeVioletClump | Recupero energia (grande) |
| GiantVioletClump | Recupero energia (gigante) |
| PopjuBalm | Effetto curativo speciale |
| Battery | Recupero energia istantaneo |
| FriendPostcard | Effetto speciale (teletrasporto o buff) |
| CannedFood | Cura + buff temporaneo |
| PsiWaveGenerator | Effetto mentale/psichico |
| MagicFormula | Effetto magico — **non usabile in combattimento** |
| WelcomeCocktail | Buff multipli |
| CrustaceanSoup | Cura + buff stato |

> **MagicFormula** controlla `PlayerModel.InCombat()` prima dell'uso. Se il player è in combattimento, l'utilizzo viene bloccato.

---

## 13 · Status Effects

Tre status negativi si accumulano tramite build-up e si attivano quando superano la resistenza del bersaglio. Valgono sia per il giocatore che per i nemici.

### Poison (Veleno)

```
// Accumulo
currentPoisonAmount += poisonBuildUp

// Attivazione
if currentPoisonAmount >= PoisonResistance → isPoisoned = true

// Effetto attivo: danno nel tempo
tick ogni 10s → -1 HP per tick
durata totale: 60s
// Fine effetto → EPoisonEffectFinished → currentPoisonAmount = 0
```

### Burn (Fuoco)

```
// Accumulo
currentBurnAmount += burnBuildUp

// Attivazione
if currentBurnAmount >= BurnResistance → isBurned = true

// Effetto attivo: difesa dimezzata
Defense *= 0.5  (durante il burn)
durata totale: 30s

// Fine effetto
Defense *= 2.0  (ripristino)
currentBurnAmount = 0
```

### Frost (Gelo)

```
// Accumulo
currentFrostAmount += frostBuildUp

// Attivazione
if currentFrostAmount >= FrostResistance → isFrostbitten = true

// Effetto attivo: movimento più lento
durata totale: 30s
// Fine effetto → EFrostbiteEffectFinished → currentFrostAmount = 0
```

### Effetti applicabili via EffectCalculator

| EffectType | Comportamento |
|---|---|
| `Heal` | Cura HP (Static o % MaxHealth) |
| `FocusGain` | Recupera Energia |
| `LifeGain` | Aggiunge Vite con probabilità |
| `CoinsCollected` | Aggiunge monete |
| `PointsCollected` | Aggiunge punti |
| `DefenseBuff` | Buff Difesa (temporaneo / permanente / UntilDeath) |
| `BuffDropRate` | Buff Drop Rate |
| `BuffFrostResistance` | Buff FrostResistance |
| `ReturnToStatue` | Teletrasporta all'ultima statua visitata |
| `UnveilStandardHiddenWalls` | Rivela muri nascosti per `Duration` secondi |
| `SaveGame` | Salva automaticamente sull'ultimo slot |
| `KillPlayer` | Forza HP a 0 (morte immediata) |
| `Poisoned` | Applica poison build-up direttamente |
| `Burned` | Applica burn build-up direttamente |
| `Frostbitten` | Applica frost build-up direttamente |
| `EnergyConsumption` | Consuma energia (valore negativo) |
| `EnergyRecovery` | Recupera energia |

### Struttura EffectData

```csharp
EffectType      Type;
EffectValueType ValueType;  // Static | Percentage
float           Value;
float           Duration;   // 0 = istantaneo, >0 = temporaneo (secondi)
bool            UntilDeath; // persiste fino alla perdita di una vita
float           Probability;// 0-1, usato da LifeGain
```

---

## 14 · Inventario

L'inventario gestisce tutte le categorie di oggetti. Gli oggetti equipaggiabili hanno slot dedicati separati dalla lista generale.

### Categorie

| Categoria | Tipo | Acquisizione |
|---|---|---|
| Consumables | Lista con quantità | Forzieri, drop, shop |
| Equipments | Lista (1 per tipo) | Forzieri, shop |
| Key Items | Lista (chiavi univoche) | Forzieri, drop, shop |
| Essences | Lista + 8 slot equipaggiati | Consumo Life Items |
| Life Items | Lista con quantità | Drop nemici e boss |

### Slot Equipment equipaggiati

- `Body` — 1 slot vestito
- `Acc1`, `Acc2`, `Acc3` — 3 slot accessori

### Slot Essenze equipaggiate (8 totali)

- Standard: `S1 S2 S3 S4 S5` (5 slot)
- Behavioural: `B1 B2 B3` (3 slot)
- Il numero effettivo di slot sbloccati scala con l'attributo `EssenceSlots`

---

## 15 · Sistema Shop

Il negozio si apre tramite dialogo Ink (funzione `OpenShop(shopName)`). Gli acquisti costano *Monete*. Ogni shop è persistente: gli oggetti venduti non vengono riforniti durante il playthrough.

### Meccanica acquisto

1. Il giocatore seleziona un oggetto dal menu shop
2. Il sistema verifica che `coins >= itemPrice`
3. Se sufficiente: `coins -= price`, oggetto aggiunto all'inventario
4. Lo slot shop viene marcato come venduto (non si riappare)

### Monete

- **Fonte:** loot fisso nel mondo, drop nemici, effetti consumabili
- **Uso:** esclusivo per acquisti in shop
- **Salvataggio:** in GameData

### Oggetti vendibili

Keys, Consumabili, Equipment, Life Items.

---

## 16 · Statue e Sistema di Salvataggio

Il salvataggio funziona in stile **Souls-like**: il giocatore riposa a una Statua per salvare. Non c'è auto-save durante il gameplay (eccetto l'effetto `SaveGame` di alcuni consumabili).

### Effetti del riposo alla statua

- **HP ripristinato** al massimo (`currentHealth = MaxHealth`)
- **Energia ripristinata** al massimo (`currentEnergy = MaxEnergy`)
- **Punto di spawn aggiornato** alla posizione della statua
- **Stato NPC aggiornato:** tutti gli NPC rileggono la fase quest corrente e aggiornano visibilità/posizione
- **Nemici Regular e Unique resettati** al loro stato iniziale (respawn)
- **Gioco salvato** su JSON in `Application.persistentDataPath`

### Sistema di salvataggio

- Fino a **6 slot di salvataggio**
- File: `GameDataSlotN.save` (JSON)
- Contiene l'intero stato: inventario, caratteristiche, talenti, status, quest, porte aperte, forzieri aperti, stanze completate

### Respawn dopo morte

- Il giocatore viene teletrasportato all'ultima statua attivata
- Breve periodo di invincibilità al respawn
- Penalità di morte già applicate (vedi sezione Combat)

---

## 17 · Sistema Nemici

I nemici sono completamente data-driven via ScriptableObject. Il comportamento è definito da 6 Behavior SO assegnati nell'Inspector, gestiti da una State Machine con 6 stati concreti.

### Tipi di nemico

| Tipo | Respawn alla statua | Note |
|---|---|---|
| Regular | ✅ Sì | Nemici comuni del mondo |
| Unique | ✅ Sì | Nemici speciali nominati |
| CombatRoom | ❌ No | Muoiono permanentemente quando la room viene completata |
| Boss | ❌ No | Muoiono permanentemente |

### Dati nemico (EnemyModel)

| Campo | Descrizione |
|---|---|
| MaxHealth | HP massimi |
| JumpAttackDefense | Riduce il danno da Jump Attack |
| ThrowAttackDefense | Riduce il danno da Throw Attack |
| PointsDrop | Punti guadagnati uccidendolo |
| PossibleLifeDrops | Lista di `{ LifeId, dropChance }` |
| contactDamageOutput | Danno al tocco |
| MeleAttackDamageOutput | Danno attacco melee |
| PoisonResistance | Soglia build-up veleno (default: 5) |
| BurnResistance | Soglia build-up fuoco (default: 5) |
| FrostResistance | Soglia build-up gelo (default: 5) |
| Phase2HealthThreshold | HP a cui scatta la fase 2 (0 = nessuna fase 2) |

### State Machine — 6 stati

| Stato | Descrizione |
|---|---|
| `DormantState` | Nemico inattivo, non risponde |
| `IdleState` | Pattuglia o sta fermo, controlla aggro/range |
| `ChaseState` | Insegue il giocatore |
| `MeleeAttackState` | Attacco corpo a corpo |
| `CooldownState` | Pausa post-attacco |
| `RangedAttackState` | Attacco a distanza |

**Transizioni default (da IdleState):**
- `IsAggroed` → ChasingState
- `InMeleeAttackRange` → MeleeAttackState
- `InRangedAttackRange` → RangedAttackState

### Behavior SO esistenti

| SO | Stato | Comportamento |
|---|---|---|
| EnemyIdleStayStill | Idle | Fermo, reattivo ad aggro/range |
| EnemyIdleLookLeftAndRight | Idle | Gira la testa a intervalli |
| EnemyIdleCasualPatrolling | Idle | Pattuglia con pause random |
| EnemyChaseToPlayer | Chase | Insegue, perde aggro → Idle. `chaseContinuationTime` configura quanto inseguire dopo perdita aggro |
| EnemyChaseDirectToPlayerUntilWall | Chase | Carica fino al muro → stun → Idle. Base per boss |
| EnemyChaseUntilWallWithStunDebuff | Chase | Come sopra ma applica debuff alle difese durante lo stun |
| EnemyChaseHorizontalTracking | Chase | Si avvicina orizzontalmente, triggera Ranged quando allineato (nemici volanti/sparatori) |
| AnimationBasedMeleeAttack | Melee | Animazione attacco → cooldown o chase |
| EnemyMeleeAttackDiveToPlayer | Melee | Salto verso il player con forza (Preparing → Diving → WaitingAfterDive) |
| StayStillCooldown | Cooldown | Timer fermo → Idle |
| EnemyDormantStayStill | Dormant | Fermo finché non riceve IsAwake |
| EnemyDormantAttachedToCeiling | Dormant | Attaccato al soffitto (gravity=0, kinematic) |
| EnemyDormantSleepInSpecificPosition | Dormant | Si muove verso una posizione specifica e dorme lì |
| PopjueBearDormant | Dormant | Specializzazione Popjue Bear: risponde a `EHomeRouteBridgeMoved` |

### Fasi nemico

I nemici (specialmente i boss) hanno fino a 4 fasi (`Phase1 → Phase2 → Phase3 → Phase4`). La transizione da Phase1 a Phase2 avviene automaticamente quando `CurrentHealth ≤ Phase2HealthThreshold`. In Phase2 i Behavior SO possono cambiare comportamento.

---

## 18 · Stanze di Combattimento

Alcune aree del mondo sono *Combat Rooms*: stanze chiuse che si attivano all'ingresso del giocatore e si completano eliminando tutti i nemici al loro interno.

| Stato | Condizione | Effetti |
|---|---|---|
| Inactive | Giocatore non è entrato / uscito durante un'attivazione | Nemici dormienti, porte aperte |
| Active | Giocatore è dentro | Nemici svegli, muri chiudono le uscite, camera dedicata attiva |
| Cleared | Tutti i nemici morti | Muri si aprono, room salvata come completata, evento `ECombatRoomCompleted` |

> **Reset:** Se il giocatore esce mentre la room è Active, i nemici si resettano e la room torna Inactive. I nemici CombatRoom non respawnano alla statua: una volta cleared, la room rimane permanently cleared.

---

## 19 · Dialogo e Sistema Quest

I dialoghi sono scritti in **Ink** (file `.ink` compilati in JSON). Le quest sono in stile Souls-like: nessun menu, nessun indicatore. Il giocatore deve ricordare da solo il progresso con ogni NPC.

### Sistema Dialogo (Ink)

- Variabili Ink sincronizzate bidirezionalmente con lo stato di gioco (`InkVariables.cs`)
- Funzioni C# chiamabili da Ink: `AdvanceQuest(npcName)`, `OpenShop(shopName)`, `NewTalent(talentName)`
- Il sistema controlla inventario, talenti, e caratteristiche del player per sbloccare rami di dialogo

### Sistema Quest

Ogni NPC ha un `NpcQuestData` ScriptableObject che definisce le sue fasi:

```csharp
NpcQuestPhase {
    string phaseName;        // es: "initial", "helped_player"
    string sceneName;        // scena dove appare (vuoto = nascosto)
    string npcGameObjectId;  // Id del GameObject NPC nella scena
}
```

### Flusso avanzamento quest

1. In Ink, chiama `AdvanceQuest("npcName")`
2. QuestModel aggiorna l'indice della fase corrente dell'NPC
3. Il player riposa alla statua → evento `EPlayerRestOnStatue`
4. Ogni componente NPC rilegge la sua fase e aggiorna visibilità
5. L'NPC si attiva solo se il suo `Id` corrisponde al `npcGameObjectId` della fase corrente

> **NPC mobili:** Un NPC può spostarsi fisicamente tra scene al variare della quest phase. Si creano più GameObject Npc nelle diverse scene con lo stesso `npcName` ma `Id` diversi — solo quello della fase corrente sarà visibile.

---

## 20 · Scene e Struttura Metroidvania

Il mondo è strutturato come una serie di stanze connesse caricate in modo **additivo**, permettendo transizioni fluide senza loading screen completi.

### Dettagli tecnici

- Caricamento scene: **additivo** (una scena persistente + scene stanza)
- Scena persistente: `"GamePlay"` (GameContext, UI, sistemi globali)
- Prima scena di gioco: `"1.1_parent_house"`
- `SceneHandler.cs` + `SceneMountConfig.cs` gestiscono il caricamento

### Porte e sblocchi

| Tipo porta | Condizione di sblocco |
|---|---|
| Porta aperta | Interazione diretta del player |
| Porta bloccata | Richiede `KeyId` specifico nell'inventario |
| Porta event-based | Si apre automaticamente al verificarsi di tutti i `GameplayEventId` richiesti |

Lo stato di tutte le porte aperte è salvato in GameData e persiste tra sessioni.

### Forzieri

- Apribili una volta sola (stato salvato in GameData)
- Possono contenere: consumabili, equipment, chiavi, essenze, life items
- All'apertura pubblicano eventi per ogni tipo di loot trovato

---

*Life Eaters — Documento generato da codebase — 2026-06-23*