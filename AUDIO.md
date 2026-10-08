# AUDIO.md — Life Eaters: Sound Design & Audio Tecnico

> Documento di riferimento per l'audio del gioco, nato da una sessione di brainstorming (ottobre 2026).
> **Decisione presa:** middleware **FMOD Studio**.
> Stato: nessun sistema audio ancora implementato.

---

## 0. Visione

- SFX **retro-platformer** (monete, punti, kill, UI) mescolati a **foley realistico** (passi per superficie, porte, vento, ambienti).
- Musica **puntuale** su zone ed eventi, nessun tema fisso per livello (stile Dark Souls).
- Atmosfera da **"cartuccia maledetta / gioco proibito"**.

### Stato attuale della codebase (al momento della stesura)
- Zero audio: nessun AudioSource/AudioMixer, nessun asset audio, nessun middleware, nessun animation event (`m_Events: []` in tutti i `.anim`).
- Unity 6000.4, niente Addressables.
- AudioListener solo sulla camera della scena persistente `GamePlay` (corretto, va mantenuto così).

---

## 1. Direzione artistica

### Due mondi sonori, una regola
| Strato | Cosa | Trattamento |
|---|---|---|
| **Mondo (diegetico)** | passi, porte, acqua, vento, nemici, impatti pit object, ambienti | realistico, spazializzato, nel riverbero della stanza |
| **Sistema (meta)** | monete, punti, level-up, vite, kill, UI, statua attivata | retro / chip / FM, 2D (non spazializzato) |

**Retro = il gioco ti parla. Realistico = il mondo esiste.**

### Il retro che si corrompe
Il contrasto più interessante è far **contaminare il retro dal mondo**:
- i suoni chip passano parzialmente nel riverbero della stanza (una moneta nelle fogne "suona" nelle fogne);
- wow/flutter da nastro, detune, bitcrush variabile;
- jingle quasi classici con un intervallo sbagliato (tritono, nota finale calante);
- rari glitch intenzionali legati a eventi narrativi, mai casuali.

(Dettagli completi nella sezione 7, "Il filtro maledetto".)

### Musica stile Souls
- **Default = silenzio + ambience.** L'ambiente è la "musica" del 90% del gioco: va trattato come una traccia ambient.
- Musica solo per: **statue** (tema di riposo breve e ricorrente), **boss** (layer per fase), **combat room**, hub/NPC specifici, momenti narrativi, menu principale.
- **Stinger** brevi: scoperte, porte importanti, morte, acquisizione talento.

### Idee legate ai sistemi esistenti
- **Grammar audio dei proiettili = grammar visiva**: colore → timbro (bianco neutro, veleno gorgogliante, burn crepitio, frost vetroso); tier → intensità/layer (tier IV-V con pulsazione sincronizzata al glow). Ballistic con "whoosh", Physical con rimbalzo/impatto.
- **Pit object**: `category` → materiale del suono (Structural = pietra/argilla, Insect = ronzio…), `weight` → peso dell'impatto. Un solo evento parametrico, non un suono per oggetto.
- **Combo monete** con pitch che sale, che si "stona" se il player è avvelenato/maledetto.
- **Blip vocali nei dialoghi Ink** (alla Undertale/Animal Crossing): un timbro per NPC, timbro diverso per gli alieni (lega con Oratory/AlienOratory).
- **Status effect** come layer sonori sul player (filtro, battito, respiro).
- **Acqua**: snapshot "subacqueo" con passa-basso.

---

## 2. Tecnologia: FMOD Studio

### Cos'è una DAW (e perché FMOD è familiare)
DAW = **Digital Audio Workstation**: il software per registrare, comporre, arrangiare, mixare ed esportare (Logic Pro, Ableton, Cubase, Reaper…).
FMOD ha un'interfaccia simile:
- timeline e tracce su cui disporre clip e loop;
- mixer con bus, mandate, effetti e automazioni;
- **parametri** che funzionano come automazioni, ma mossi dal gioco (salute, fase boss, tipo di pavimento) invece che dal tempo.

Differenza di fondo: in Logic produci una traccia lineare; in FMOD costruisci **eventi interattivi** che reagiscono al gioco.
Flusso: **Logic** (crei i suoni) → export file → **FMOD** (assembli eventi) → **Unity** (il codice li attiva).

### Perché FMOD e non l'audio nativo Unity
| | FMOD Studio | Unity nativo |
|---|---|---|
| Workflow | DAW-like | codice + Inspector |
| Randomizzazione pitch/volume/clip | integrata | da scrivere |
| Passi per superficie | parametro `Surface` sull'evento | dizionari SO + codice |
| Musica adattiva (fasi boss, transizioni a battuta) | nativa | molto laboriosa |
| Snapshot (acqua, menu, dialogo, ducking) | nativi | Mixer snapshots, più limitati |
| Live Update mentre giochi | sì | no |
| Costo | licenza indie gratuita entro certe soglie (**verificare termini sul sito FMOD**) | gratis |

---

## 3. Architettura tecnica (agganciata alla codebase)

### Principio: l'audio ascolta l'EventBus
Coerente con Control/Boundary/Data: **nessun sistema di gioco chiama l'audio direttamente**. Componenti nella scena `GamePlay` si sottoscrivono agli eventi e suonano.

```
Boundary/Audio/
├── AudioEventBridge.cs   ← subscribe a EventBus → SFX 2D "di sistema"
├── MusicDirector.cs      ← state machine musica (Silence/Statue/Combat/Boss/Event)
├── AmbienceDirector.cs   ← ambience + riverbero per scena/stanza
├── CurseController.cs    ← calcola il parametro globale "Curse" (vedi sez. 7)
├── PlayerAudio.cs        ← passi, salto, atterraggio, dash (sul prefab player)
├── EnemyAudio.cs         ← emitter spazializzato sui prefab nemici
└── SurfaceTag.cs         ← marca tilemap/collider con SurfaceType
Data/Audio/
├── SurfaceType.cs        ← enum: Stone, Wood, Dirt, Grass, Metal, Water, Flesh...
└── AreaAudioProfile SO   ← per scena/stanza: ambience, riverbero, musica opzionale, Curse base
```

### Eventi già esistenti da riusare
- **Pickup / progressione**: `ECoinCollected`, `EPointsCollected`, `ELifeCollected`, `ELevelUp`, `EBonusCollected`, `EHpRestored`
- **Player**: `EPlayerReceiveDamage`, `EDamageReceived`, `EPlayerReceiveContactDamageByEnemy`, `EPlayerDeathSequenceStarted`, `EPlayerDied`, `EGameOver`
- **Nemici**: `EEnemyHitByPitObjectEvent`, `EEnemyStompedEvent`, `EEnemyDied`, `EEnemyFallInDeadBox`
- **Proiettili**: `EEnemyBulletHitPlayer`, `EProjectileEffectHitPlayer`
- **Statue**: `EPlayerActivateStatue`, `EPlayerRestOnStatue`
- **Zone**: `EPlayerSceneChanged` (SceneHandler), `EPlayerEnteredRoom` / `EPlayerExitedRoom` (RoomCamera, per nome stanza)
- **Combat**: `EPlayerEnteredCombatRoom`, `EPlayerExitedCombatRoom`, `ECombatRoomCompleted`
- **Dialogo**: `EDialogueStarted`, `EDialogueLineDisplayed`, `EDialogueFinished`
- **Pit**: `EPitsGenerated`, `EPlayerGrabbedPitObject`, `EPlayerObjectThrown`, `EPlayerObjectPutDown`
- **Status / altro**: `EPlayerBurnt`, `EPlayerPoisoned`, `EPlayerFrostbitten`, `EPlayerTeleported`, `EDoorOpened`

### Lacune da colmare
1. **Jump / double jump / landing / dash**: nessun evento. Punto naturale: `SetJumpState()` e avvio di `Dashing()` in `Boundary/GamePlay/Player/PlayerController.cs`. Atterraggio = transizione da stato aereo a `ReadyToJump` con `IsGrounded`, con la velocità di caduta come parametro.
2. **Passi**: aggiungere animation event `Footstep` sui frame di contatto delle clip di corsa → `PlayerAudio.Footstep()` (meglio di un timer: sincronia con lo sprite).
3. **Tipo di superficie**: `Boundary/Utils/TouchingDirections.cs` ha già `_groundHits` ma espone solo bool. Esporre il collider a terra → leggere un `SurfaceTag` (uno per tilemap) → parametro `Surface` in FMOD.
4. **Boss**: nessun evento di inizio incontro né di cambio fase. Aggiungere `EBossEncounterStarted/Ended` ed `EEnemyPhaseChanged` in `Enemy.cs` (dove si valuta `Phase2HealthThreshold`).

### Spazializzazione in 2D
- Listener sulla camera. Emitter nemici/oggetti con attenuazione per distanza + panning stereo; niente occlusione.
- Suoni "di sistema" sempre 2D; tutto il resto 3D con min/max distance tarate sulla dimensione di una stanza.

### Mix, bus, snapshot
- Bus: `Master → Music / Ambience / SFX-World / SFX-System / UI / Voice-blips` (+ gruppo `Cursed`, sez. 7).
- Snapshot: `Menu`, `Dialogue` (ducking leggero), `Underwater`, `LowHealth`, `StatueRest`, `Death`.
- Volumi utente (Music/SFX/Ambience) salvati nelle opzioni, non nello slot di save.

---

## 4. Fattibilità

| Fattibile, alto impatto | Fattibile ma da dosare | Sconsigliato per la beta |
|---|---|---|
| Ambience a layer per zona | Passi su superficie anche per nemici (solo grandi/boss) | Musica generativa procedurale |
| Passi player multi-superficie | Riverbero per ogni stanza (meglio per zona/scena) | Doppiaggio |
| Musica boss a layer per fase | Transizioni a battuta ovunque | Occlusione/propagazione realistica |
| Tema statua + stinger | Ambience reattiva allo stato del mondo | Un suono unico per ogni pit object |
| Blip dialogo per NPC | | |
| Grammar sonora proiettili | | |

---

## 5. Suoni realistici: dove trovarli

Non serve registrare: le librerie arrivano già pulite, poi si **trasformano** in Logic.

### Fonti gratuite
- **Sonniss GDC Game Audio Bundle**: rilascio annuale gratuito di decine di GB di librerie professionali, uso commerciale senza royalty. Scaricare anche le edizioni passate.
- **Freesound.org**: enorme, qualità irregolare. Usare solo **CC0** (libero) o **CC-BY** (attribuzione nei crediti). **Evitare CC-BY-NC** (non commerciale).
- **Zapsplat**: gratis con attribuzione, oppure a pagamento senza.

### Fonti a pagamento
- **A Sound Effect** (asoundeffect.com): librerie di sound designer indipendenti, di nicchia → meno riconoscibili.
- **BOOM Library**: qualità altissima, molto diffusa nell'industria.
- **Soundsnap / Pro Sound Effects**: abbonamenti e bundle ampi.

⚠️ **BBC Sound Effects**: la licenza gratuita è solo per uso personale/didattico, **non** per un gioco commerciale.

### Come renderli irriconoscibili
- **Layering**: 2-3 sorgenti per ogni suono importante (porta del castello = scricchiolio legno + tonfo grave + cigolio metallico).
- **Processing**: pitch shift, time-stretch, EQ, reverse, saturazione, granulare (Logic: Alchemy, ChromaVerb, Pitch Shifter, Sample Alchemy).
- **Sorgenti inaspettate**: es. tuono = lamiera scossa rallentata e pitchata giù + tuono vero.
- Il **filtro maledetto** rende tutto ancora più "proprio".

### Pratica
- **Registro licenze**: foglio con suono, fonte, licenza, obbligo di attribuzione (serve per crediti e Steam).
- Per registrare in futuro: un registratore portatile economico (tipo Zoom H1n) e una stanza silenziosa bastano.

---

## 6. Suoni retro: sintetizzarli, non campionarli

Suoni semplici (quadra, triangolo, rumore), veloci da creare e **tuoi al 100%**.

### Strumenti
**Generatori rapidi (gratuiti)**
- **ChipTone** (SFB Games, browser): il migliore per SFX retro.
- **jsfxr / Bfxr**: generazione istantanea di salto/moneta/esplosione, ottimi per prototipare.
- **rFXGen**: simile, interfaccia più pulita.

**Synth per Logic**
- **Magical 8bit Plug 2** (gratuito, AU): synth NES-style.
- **Plogue chipsounds** (pagamento): emulazione accurata di decine di chip reali.
- **Dexed** (gratuito): FM DX7, sapore Mega Drive.
- **Nativi Logic**: Retro Synth, ES2 (pulse width), Bitcrusher.

**Tracker**
- **Furnace** (gratuito, open source): emula NES, Game Boy, Genesis, SNES… utile per jingle "autentici".

### Principi di design
**Una "console immaginaria" con limiti fissi** (coerenza = stessa cartuccia). Esempio NES:
- 2 canali pulse (duty cycle 12.5% / 25% / 50%);
- 1 triangolo per bassi e tonfi;
- 1 canale rumore per colpi, esplosioni, kill.

**Ricette di partenza**
| Suono | Ricetta |
|---|---|
| Moneta | due note rapide in salita (quarta/quinta), pulse breve |
| Salto | sweep di pitch verso l'alto, pulse |
| Danno player | sweep verso il basso + rumore, leggermente dissonante |
| Kill nemico | burst di rumore + arpeggio discendente |
| Punti / level-up | arpeggio maggiore ascendente veloce |
| Vita raccolta | arpeggio lungo con "eco finto" (nota ripetuta a volume calante) |
| UI / cursore | blip cortissimo, pulse 12.5% |

**Il "sbagliato" fin dall'origine**: accordi minori/diminuiti al posto dei maggiori, ultima nota calante o sul tritono, duty cycle che cambia in modo innaturale.

**Ibrido leggero (opzionale)**: un layer realistico molto basso sotto suoni chiave (kill, assorbimento vita) come ponte tra i due mondi.

### Workflow
1. Prototipo in ChipTone/jsfxr → WAV.
2. Rifinitura o ricreazione in Logic (Magical 8bit / chipsounds), export 48 kHz.
3. 2-3 varianti per suoni frequenti; la randomizzazione la fa FMOD.

---

## 7. Il filtro maledetto

**Idea**: il suono di sistema non arriva pulito, passa attraverso un **supporto difettoso** (cartuccia vecchia, nastro consumato, cassa di un vecchio TV). L'intensità varia con lo stato del gioco.

### Catena del segnale
| Elemento | Cosa fa | Effetto percepito |
|---|---|---|
| Banda ristretta | HP ~150 Hz, LP ~6-8 kHz | cassa piccola da TV, suono vecchio |
| Wow & flutter | pitch mod lenta (~0.5-2 Hz) e rapida (~6-12 Hz) | nastro che si stira |
| Saturazione | distorsione morbida nastro/valvola | calore che diventa sporcizia |
| Bitcrush / sample rate | riduzione risoluzione, dosata | digitale degradato |
| Letto di rumore | loop fruscio + ronzio 50 Hz + crackle | il supporto si sente anche nel silenzio |
| Dropout | brevi cali/tagli di volume | contatti sporchi |
| Detune | pochi cent fuori | "qualcosa non va" |
| Mandata al riverbero della stanza | sistema dentro lo spazio del mondo | retro contaminato dal reale |

### Baked vs runtime
Regola: **non cuocere mai nel file ciò che vuoi far variare.**
- **Baked in Logic**: carattere fisso (saturazione leggera, nota "sbagliata", lieve detune).
- **Runtime in FMOD**: banda, wow/flutter, distorsione, fruscio, dropout.
- FMOD offre nativamente: passa-basso/alto, EQ, distorsione, chorus/flanger (chorus a una voce con rate basso ≈ wow), tremolo, pitch shifter, riverbero a convoluzione. Per bitcrush o wow/flutter più realistici: varianti baked in Logic o plugin di terze parti.

### Parametro globale `Curse` (0 → 1)
Una macro che muove tutto insieme:
- **0** → carattere minimo, appena percepibile;
- **0.5** → banda stretta, wow evidente, fruscio udibile;
- **1** → quasi distrutto, dropout frequenti, distorsione marcata.

**Cosa lo fa salire:**
- **Zona**: valore base per area (più alto in fogne/sotterranei/zone proibite);
- **Salute bassa**;
- **Status effect** (veleno → detune, frost → fruscio…);
- **Vite consumate**: più mangi vite, più il gioco si corrompe (legame diretto col tema *Life Eaters*, da usare con misura);
- **Momenti narrativi** scelti a mano (boss, rivelazioni, NPC particolari).

### Eventi glitch scriptati (mai casuali)
- **Morte**: "tape stop" (pitch che scivola giù e rallenta fino al silenzio).
- **Consumo di una Vita**: il jingle balbetta/salta per un istante.
- **Riposo alla statua**: `Curse` → 0, suono pulito per la prima volta. **La statua è l'unico luogo "sano"** del gioco.

### Struttura in FMOD
- **Gruppo bus `Cursed`** che contiene `SFX-System`, `Music` e, in misura minore, `UI`; catena effetti sul bus, parametri automatizzati da `Curse`.
- **Evento "Medium Noise"** (loop fruscio/ronzio) con volume legato a `Curse`.
- **Suoni di mondo fuori dal bus** (realismo intatto). Eccezione per il climax: a Curse quasi massimo si contamina anche il mondo.
- **Snapshot** per picchi temporanei (morte, consumo vita).
- **Seek speed** del parametro per transizioni morbide.

### Lato Unity
- `CurseController` (in `Boundary/Audio/`) si sottoscrive all'EventBus (cambio scena, danno, status, vite consumate, statua).
- Combina base zona (`AreaAudioProfile` SO) + salute + status + vite.
- Invia a FMOD: `RuntimeManager.StudioSystem.setParameterByName("Curse", valore)`.
- Tutto il resto (come suona) vive in FMOD, rifinito a orecchio col Live Update.

### Prototipo in Logic
1. Bus aux con: Channel EQ (banda) → Chorus/Modulation Delay (wow) → Tape Delay/saturazione → Bitcrusher, più una traccia di fruscio in loop.
2. Mappare tutti i parametri su **una sola Smart Control "Curse"**, ognuno col proprio range.
3. Far passare i suoni chip nel bus e muovere la manopola. Poi replicare le curve in FMOD.

### Cautele
- **Affaticamento d'ascolto**: Curse basso per la maggior parte del gioco, valori alti come eccezione.
- **Leggibilità del gameplay**: i suoni critici (danno subito, attacco boss in arrivo) restano chiari anche a Curse alto — esclusi dal bus o con corruzione limitata.

---

## 8. Workflow di produzione (Logic → gioco)
- Export **48 kHz / 24 bit**; mono per SFX spazializzati, stereo per ambience/musica/UI.
- Loop con code gestite in FMOD (tail non incollate nel file).
- 3-6 varianti per suoni ripetitivi + randomizzazione pitch/volume in FMOD.
- Naming: `sfx_world_footstep_stone_01`, `sfx_sys_coin_01`, `amb_sewers_bed`, `mus_boss_crayfish_p1`.
- Loudness indicativa: mix complessivo intorno a −16/−18 LUFS integrati; test in cuffia e su casse piccole.
- Compressione: Vorbis in streaming per musica/ambience lunghe, ADPCM/PCM per SFX brevi.

---

## 9. Roadmap
1. **Fondamenta**: integrazione FMOD, `AudioEventBridge`, bus/snapshot, slider volumi, SFX placeholder sugli eventi esistenti.
2. **Player feel**: eventi jump/land/dash, animation events per i passi, `SurfaceTag` + `TouchingDirections`.
3. **Mondo**: `AmbienceDirector` + `AreaAudioProfile` per zona 1, zona 2, castello, fogne; emitter nemici.
4. **Musica**: `MusicDirector`, tema statua, combat room, boss del castello a layer per fase (nuovi eventi boss).
5. **Sistema & personalità**: palette retro, filtro maledetto + `CurseController`, combo monete, blip dialogo, grammar proiettili, status effect.
6. **Acqua & polish**: snapshot subacqueo, low health, menu principale, passaggio di mix finale.

### Verifica per ogni fase
- Play mode con FMOD Live Update connesso: eventi, livelli, parametri in tempo reale.
- Checklist evento EventBus → suono udibile, nessun doppio trigger, nessun suono orfano su cambio scena/teletrasporto (`EPlayerTeleported`, `EPlayerEnteredRoom`).
- Passi su ogni superficie taggata; transizioni musicali statua/boss/combat room.

---

## 10. Prossimi passi possibili
- Lista completa dei suoni necessari per zona 1, zona 2 e castello, divisa tra "di sistema" e "di mondo".
- Composizione della musica: tutta in Logic oppure collaborazioni/librerie (da decidere).

