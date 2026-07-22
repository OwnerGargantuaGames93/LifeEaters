# ROADMAP.md — Life Eaters: Piano di Chiusura Demo

> Documento di pianificazione per portare la demo (mappa/slice già decisa e in parte implementata) a uno stato finito e presentabile, tenendo conto dei vincoli reali di tempo (lavoro full-time, convivenza, sport agonistico) e delle competenze reali (forte in programmazione e audio, debole in arte visiva).
>
> **Principio guida:** sfruttare i punti di forza (codice/shader/audio) per compensare il punto debole (illustrazione), non il contrario. Qualità su un perimetro piccolo batte quantità su un perimetro grande.

---

## 0 · Cosa NON è in questo documento

- Nessun criterio di "vale la pena continuare" — deciso: si continua, punto.
- Nessuna ridiscussione dello scope della demo — la mappa è già decisa ed è il minimo per validare la struttura del gioco.
- Ogni idea nuova che emerge durante il lavoro ("e se aggiungessi...") va scritta nel **Backlog post-demo** (§7), mai implementata ora.

---

## 1 · Ordine di lavorazione delle fasi

| # | Fase | Contenuto | Perché in quest'ordine |
|---|---|---|---|
| 1 | **Gameplay residuo** | Quick use menu (UI+logica), teleport/flashback trigger, 2-3 nuovi Pit Object, status effect definitivi | Lavoro nel tuo dominio di forza, perimetro chiuso, zero dipendenze esterne. Chiuderlo per primo dà terreno stabile sotto tutto il resto. |
| 2 | **Pilot tecnico arte** | Validare su UNA stanza/asset placeholder l'intera pipeline grafica (luci, shader post-process, wind shader) | Vedi §3. Non si producono asset in massa finché la pipeline non è validata — altrimenti si rifà tutto due volte. |
| 3 | **Produzione arte slice** | Tileset, palette, sprite player/nemici/props della demo | Solo dopo l'ok sul pilot. |
| 4 | **Illuminazione + shader + post-processing definitivi** | Applicazione del layer "maledetto" su tutta la slice | In parallelo/subito dopo la fase 3, stanza per stanza. |
| 5 | **Sound design & musica** | Vedi §2 — attraversa più fasi, non è un blocco singolo | Non deve essere l'ultima cosa, ma nemmeno bloccare l'arte. |
| 6 | **UI finale** | Skin definitiva di quick use menu, inventario, dialoghi, ecc. | Dopo che palette e stile visivo sono fissati (fase 3), altrimenti la UI va rifatta. |
| 7 | **Main menu** | — | Dopo che stile e audio sono consolidati (riusa asset/palette/musica già pronti). |
| 8 | **Intro** | Disegni sorella → pixel-art via filtro + voce narrante | Dipendenza esterna già gestita (nessun problema, come confermato) — naturalmente in fondo. |
| 9 | **Ottimizzazione** | Solo sulla slice della demo | Ultimo, su contenuto ormai stabile — ottimizzare prima è tempo sprecato se qualcosa cambia ancora. |
| — | **Playtest settimanale demo intera** | Già previsto | Mantienilo per tutta la durata del progetto, è il tuo miglior rilevatore di scope creep e regressioni. |

---

## 2 · Dove si colloca il Sound Design

Non è un blocco unico in fondo — è distribuito su 4 momenti, sfruttando il fatto che è la tua area più sicura per bilanciare il carico cognitivo delle sere in cui lavori sulla grafica (dove sei meno sicuro).

| Stage | Quando | Cosa | Perché ora |
|---|---|---|---|
| **A — Architettura & placeholder** | In parallelo alla Fase 1 (gameplay residuo) | Estendi il pattern `EventBus` già esistente per agganciare SFX a eventi di gameplay (es. `EJumpAttackHit`, `EPoisonEffectFinished`, `EPlayerRestOnStatue`), imposta gruppi su Audio Mixer, usa SFX placeholder | Il gameplay diventa "vivo" da subito nei tuoi playtest settimanali, senza aspettare la grafica finale. Zero rework dopo: l'architettura resta la stessa, cambiano solo i file audio. |
| **B — Composizione temi** | In parallelo alla Fase 2/3 (pilot + produzione arte) | Comporre il loop ambientale e il tema principale della zona/slice, testandolo contro il mood visivo appena validato | Testare musica contro l'identità visiva reale (non contro forme geometriche) dà feedback affidabile su tono e coerenza. |
| **C — Mix finale** | Dopo il lock arte della slice (fine Fase 4) | Missaggio finale, layering dinamico (transizione combattimento/esplorazione, silenzio/calma alla Statua), normalizzazione loudness | Il missaggio dipende dal contesto visivo/UI finale (es. suoni UI, ambience per bioma) — farlo prima significherebbe rifarlo. |
| **D — Menu e intro** | Con Fase 7/8 | Tema main menu, audio narrante per l'intro | Sincrono naturale con il lavoro della sorella. |

**Consiglio pratico sul carico settimanale:** intercala l'audio (stage sicuro) con l'arte (stage insicuro) nello stesso blocco di ore quando ti serve "ricaricare" — es. se giovedì sera la produzione pixel art ti sta consumando, alterna 30-40 min su un tema musicale o SFX design. Riduce il rischio di burnout sulla skill più debole senza fermare il lavoro.

---

## 3 · Grafica: quando iniziare shader/luci, e con quale pipeline

### 3.1 Cosa è davvero replicabile da Animal Well (e cosa no)

Animal Well **non** gira su Unity: motore C++ da zero di Billy Basso, 7 anni di sviluppo solo. Non è la tecnologia da copiare, ma la **filosofia tecnica** sì. Tecniche confermate dallo stesso Basso ([PlayStation Blog](https://blog.playstation.com/2022/07/20/how-animal-well-taps-into-ps5-hardware-to-elevate-2d-pixel-art-platforming/), [GameDeveloper](https://www.gamedeveloper.com/design/why-animal-well-s-home-brewed-engine-was-key-to-its-success)):

- **Normal map + luce direzionale** sugli sprite, invece di tante luci puntuali dinamiche — Basso stesso evita l'illuminazione realtime pesante perché "clash" con la pixel art. **Questo è direttamente replicabile in URP.**
- **Un solo shader fullscreen** per effetti costosi (fluido/distorsione), non decine di micro-effetti per oggetto.
- Raymarched SDF e fluid sim (Navier-Stokes) — **non replicabili/non necessari** per te: sono soluzioni ad hoc per un motore custom con supersampling enorme (render a 320×180 upscalato a 4K). Ignorali, non è lì che sta il valore per il tuo progetto.
- Niente squash-and-stretch, niente screen shake — scelta stilistica deliberata per restare "disturbante". Tienilo a mente come vincolo di stile, non solo come tecnica.

> **Nota:** l'animazione procedurale via codice (es. Ghost Cat di Animal Well) è stata **scartata** per questo progetto — decisione presa, non riproporla nel backlog.

**Traduzione pratica per te:** il "maledetto" non viene da tanti asset disegnati a mano, viene da (1) luce/normal map ben fatta su pochi sprite, (2) un filtro fullscreen ben scelto, (3) elementi di scena dinamici e "vivi" (erba/liane reattive al passaggio, fulmini in background, gocce che cadono — vedi §3.3) invece di sfondi statici. Tre cose nel tuo dominio di forza.

### 3.2 Sequenza corretta: pilota PRIMA della produzione di massa

Non iniziare a disegnare tile/sprite finali finché non hai validato la pipeline tecnica su geometria placeholder. Ordine:

1. **Light2D + normal map** su uno sprite placeholder (player o singolo tile). Shader `Sprite-Lit-Default` + secondary texture `_NormalMap` nello Sprite Editor. Genera le normal map con **[Laigter](https://github.com/azagaya/laigter)** (gratuito) o plugin Aseprite dedicati. Guida ufficiale: **[Unity Learn — 2D Lighting for Pixel Art](https://learn.unity.com/course/2d-lighting-for-pixel-art)**.
   - Occhio a: banding visibile su sprite con gradienti di luce (workaround: disattiva MSAA, abilita HDR); se hai `Pixel Perfect Camera` su "Stretch Fill" il post-processing può silenziosamente non funzionare in Play mode — verificalo subito, è un bug noto.
2. **Shader fullscreen per il filtro "maledetto"**: in Unity 6/URP moderno si fa con **Fullscreen Shader Graph + Full Screen Pass Renderer Feature**. Parti dai Volume override standard (Chromatic Aberration, Film Grain, Vignette, Bloom, Color Adjustments) e aggiungi un effetto custom sopra:
   - CRT/scanline: [Cyanilux/URP_RetroCRTShader](https://github.com/Cyanilux/URP_RetroCRTShader)
   - Dithering ordinato (Bayer): [gist di brihernandez](https://gist.github.com/brihernandez/8f7dcdef528babfc4995bb6713e7d6bb), [NullTale/DitherFx](https://github.com/NullTale/DitherFx)
   - Quantizzazione palette stile "Obra Dinn": [tutorial Unity Discussions](https://discussions.unity.com/t/tutorial-obra-dinn-1-bit-shader-effect/831511), [writeup di Daniel Ilett](https://danielilett.com/2020-02-26-tut3-9-obra-dithering/)
   - Prova 2-3 combinazioni sul placeholder e scegli PRIMA di produrre asset — la palette finale degli sprite dipenderà da come il filtro la altera.
3. **(IN FORSE, non prioritario) Wind shader ambientale** su vegetazione di sfondo: sprite hanno solo 4 vertici, quindi servirebbe un **UV displacement** (non vero vertex displacement) — vedi [aarthificial/pixelgraphics](https://github.com/aarthificial/pixelgraphics/blob/master/Documentation~/shaders.md) e il relativo [tutorial 80.lv](https://80.lv/articles/tutorial-pixel-art-vegetation-wind-shader-in-unity); setup generico di displacement su [guida Cyanilux](https://www.cyanilux.com/tutorials/vertex-displacement/). **Valuta se serve davvero** prima di investirci tempo: la priorità reale è l'erba/liane **reattive al player** (§3.3), il vento passivo è un nice-to-have rimandabile al Backlog (§7) se il tempo non basta.
4. **Elementi di scena dinamici** (erba/liane reattive al passaggio, fulmini in background, gocce che cadono) — vedi §3.3 per il dettaglio tecnico di ciascuno.
5. Solo quando i pilastri scelti sono approvati sul placeholder (li giudichi tu stesso, magari con un playtest settimanale dedicato) → passi alla produzione reale degli asset (§4), sapendo già come si comporteranno sotto luce/filtro/elementi dinamici.

Fix comunità utili da tenere a portata per l'illuminazione: [Pixel-Perfect-Lighting](https://github.com/Tim-W-James/Pixel-Perfect-Lighting) e [LutLight2D](https://github.com/unitycoder/LutLight2D) (illuminazione a rampa di palette, evita il gradient bleed che rovina la pixel art).

### 3.3 Elementi di scena dinamici (erba/liane reattive, fulmini, gocce)

Priorità reale al posto del vento passivo — rendono la scena viva reagendo al giocatore o creando atmosfera, e sono tutti alla tua portata.

**Erba/liane che si spostano al passaggio del personaggio**

Due approcci, scegli in base al tempo/skill disponibile:

| Approccio | Come funziona | Pro/contro |
|---|---|---|
| **(a) Trigger + tween via codice** (nessuno shader richiesto) | Pivot alla base dello sprite, `Collider2D` trigger, `OnTriggerEnter2D/Exit2D` ruota il pivot di qualche grado (coroutine o DOTween) e torna a riposo | Zero prerequisiti shader, veloce da implementare. Risultato: "flip" discreto per ciuffo, non un piegamento continuo. Rif: [thread Unity Forum](https://forum.unity.com/threads/how-can-i-make-2d-grass-moving-by-itself-and-when-the-character-passes-through-it.368759/), [Adventure Creator forum](https://adventurecreator.org/forum/discussion/8873/how-do-i-trigger-grass-collision-animation-when-user-walks-through-it) |
| **(b) Shader di bend guidato dalla distanza dal player**, parametro passato via `MaterialPropertyBlock` — **stesso pattern già usato su `Projectile.cs`** per `_BaseColor` ecc. | Rif: [prime31 "Interactive 2D Foliage"](https://prime31.github.io/grass2d/), [gamedev.center tutorial](https://gamedev.center/tutorial-how-to-make-an-interactive-grass-shader-in-unity/), [Daniel Ilett — Six Grass Techniques](https://danielilett.com/2022-12-05-tut6-2-six-grass-techniques/), package pronto [Elringus/GrassBending](https://github.com/elringus/GrassBending) | Piegamento continuo e organico, riusa una skill che hai già (MPB). **Attenzione:** un quad sprite standard ha solo 2 triangoli — serve una mesh suddivisa o Sprite Shape, non un `SpriteRenderer` stock |

Consiglio: parti da (a) per le prime liane/ciuffi (più veloce, zero rischio), valuta (b) solo per un set-piece iconico (es. le liane della prima scena) se il tempo lo permette — coerente col principio "hero asset fatti bene, il resto pragmatico" di §4.3.

**Fulmini alieni in background (prima scena)**

Due tecniche combinate, nessun asset complesso richiesto:
- **Flicker/flash del Global Light2D**: coroutine che alza `Light2D.intensity` per ~0.1-0.2s poi torna a riposo. Rif: [Medium — 2D Light Flicker in Unity](https://medium.com/geekculture/2d-light-flicker-in-unity-17554023693a), [gist script di flicker](https://gist.github.com/sinbad/4a9ded6b00cf6063c36a4837b15df969), [manuale URP 2D Lights](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/Lights-2D-intro.html).
- **Fulmine a zig-zag** via Line Renderer con punti generati random (algoritmo midpoint-displacement) o sprite animato a 2-3 frame disegnato a mano: [Envato Tuts+ — Generate 2D Lightning Effects (C#)](https://gamedevelopment.tutsplus.com/tutorials/how-to-generate-shockingly-good-2d-lightning-effects-in-unity-c--cms-21275), [GitHub — Unity-Jagged-Lines](https://github.com/themage107/Unity-Jagged-Lines), [Bloodirony — Lightning Strike 2D](https://www.bloodirony.com/blog/how-to-create-a-lightning-strike-in-unity2d) (doppio Line Renderer + texture a gradiente per il glow).
- Pattern completo: la coroutine fa partire insieme il flash di luce e la generazione/fade del fulmine, poi resetta entrambi. Nessun asset oltre una texture a gradiente per il glow.

**Gocce che cadono**

Usa **Particle System (Shuriken)**, non VFX Graph: è CPU-based, gestisce bene poche particelle "chunky" in stile pixel art via Texture Sheet Animation (frame disegnati a mano), mentre VFX Graph è pensato per grandi volumi GPU e il suo smoothing di default va contro l'estetica pixel art (richiede lavoro extra per essere vincolato). Rif: [Unity — 2D Rain con splash](https://www.youtube.com/watch?v=QP8zj-JQgmI), [EASY 2D Rain Particle in Unity](https://www.youtube.com/watch?v=k1kGVmS-bJQ), [ACKOSMIC — 2D Rain Effect tutorial](https://www.ackosmic.com/en/2d-rain-effect-unity-tutorial/), asset pronto in caso di fretta: [Pixel Art Rain — 2D Rain System (Asset Store)](https://assetstore.unity.com/packages/vfx/particles/pixel-art-rain-2d-rain-system-291801).

Per lo splash all'impatto col terreno: modulo **Collision** (2D, layer Ground, `Lifetime Loss = 1` così la goccia muore all'impatto) + modulo **Sub Emitter** (evento `Birth` su Collision) che spawna un piccolo burst di sprite-splash nel punto di contatto.

---

## 4 · Sprite e Tilemap: pipeline pratica, DIY vs outsourcing

### 4.1 Palette e strumenti

- Scegli **una palette da [Lospec](https://lospec.com/palette-list)** e vincolati ad essa (es. Endesga-32/64) — niente "palette shopping" in corsa, è un altro rabbit hole classico. La palette limitata è parte dello stile "maledetto", non un compromesso.
- Aseprite (che già usi) resta lo strumento giusto. Guide orientate a chi non è un illustratore di formazione: [ziva.sh](https://ziva.sh/blogs/pixel-art-tutorial), [generalistprogrammer.com](https://generalistprogrammer.com/tutorials/aseprite-complete-professional-pixel-art-guide).
- Per i tileset: usa **Rule Tile** di Unity con un set a 16 tile (blob/bitmask) invece di disegnare ogni combinazione a mano — [Unity Learn](https://learn.unity.com/tutorial/using-rule-tiles), [docs 2d-extras](https://github.com/Unity-Technologies/2d-extras/blob/master/Documentation~/RuleTile.md). Coerente col fatto che le tue scene sono già a stanze modulari (SceneHandler additivo).

### 4.2 Ordine di produzione per lo slice della demo

1. Tileset terreno base (via Rule Tile, dopo il pilot §3.2)
2. Layer di parallax/background
3. Sprite player definitivo (poche frame per animazione, come già deciso)
4. 1-2 nemici che appaiono in quella stanza/slice
5. Pit object e props interattivi (statua, porte, forzieri)

Segui l'ordine — non saltare avanti a "nemico 3" se il tileset base non è ancora chiuso, altrimenti perdi il riferimento stilistico che ogni asset successivo deve rispettare.

### 4.3 DIY vs "affittare" un designer

Con 3 discipline in mano (codice + audio + arte) e tempo limitato, l'approccio consigliato è **ibrido**, non tutto-o-niente:

- **Fai tu** gli asset "hero" dove il feel custom conta di più: player, il nemico/creatura più iconico (quello con animazione procedurale "sbagliata" da codice, vedi §3.1), i Pit Object principali. Qui il tuo tempo vale di più perché è quello che definisce l'identità del gioco.
- **Valuta di commissionare** il lavoro ripetitivo/di volume: varianti generiche di tileset, layer di background secondari, recolor di nemici minori. Prezzi indicativi 2025 ([fonte](https://2dwillneverdie.com/blog/how-much-do-sprites-cost/)): ~5$/frame di animazione (3-8$), sprite personaggio base 15-25$ (set animato 20-70$+), tileset 35-60$. Piattaforme: Fiverr Pro, job board di itch.io, forum Pixelation/Pixel Joint. Un vero listino prezzi indie: [thread itch.io](https://itch.io/t/3899084/for-hireart-pixel-art-indie-prices-characters-backgrounds-portraits-tilesets).
- **Alternativa più economica e più veloce da testare subito**: comprare/modificare asset pack esistenti come base invece di commissionare o disegnare tutto da zero — un dev solo (Ansimuz, [testimonianza](https://medium.com/@ansimuz/why-pixel-art-is-the-best-choice-for-solo-indie-developers-e2235bef67d9)) la descrive come un risparmio di tempo enorme. Utile soprattutto per il pilot tecnico di §3.2, dove ti serve solo qualcosa di plausibile su cui testare luce/shader/vento, non arte finale.
- Nessuna di queste scelte è obbligatoria: è una leva da attivare solo se/quando il tempo stringe più del previsto, non una decisione da prendere subito.

---

## 5 · Definition of Done per asset (sviluppato)

Per ogni **classe** di asset (tile, prop, sprite personaggio, layer background, effetto shader, traccia SFX) scrivi UNA riga PRIMA di iniziare, con questi 4 campi:

| Campo | Domanda a cui risponde | Esempio |
|---|---|---|
| **Scope** | Quante frame / varianti / risoluzione? | "Player idle: 4 frame, 32×32px" |
| **Riferimento** | Quale palette e quale altro asset già approvato deve fare da "fratello" stilistico? | "Palette Endesga-32, stessa saturazione del tileset già approvato" |
| **Criterio di stop** | Cosa deve essere vero *in game* perché sia finito? | "Leggibile a distanza di gioco reale, con Light2D e filtro post-process attivi — non solo nell'editor di Aseprite" |
| **Non-negoziabili esclusi** | Cosa NON si aggiunge dopo l'ok | "Niente frame extra, niente redesign palette dopo approvazione" |

**Perché serve:** senza questo, ogni sprite diventa un buco nero di rifiniture — è il rischio concreto più alto per chi (giustamente) non ha ancora l'occhio allenato per sapere "quando è abbastanza buono". Il criterio di stop deve essere verificato **in gioco**, con luce e filtro attivi, non nell'editor isolato — uno sprite può sembrare perfetto in Aseprite e illeggibile sotto il filtro "maledetto".

**Meccanismo pratico:** una tabella (anche solo un file MD o un Trello) con colonne `Asset | Scope | Riferimento | Criterio di stop | Stato | Note`. Una volta segnato **Fatto**, si riapre SOLO per bug conclamato (es. leggibilità compromessa in una configurazione di luce specifica), mai per gusto estetico soggettivo del momento.

**Time-box per tipo di asset** (adatta ai tuoi tempi reali, ma fissa un numero prima di iniziare):
- Singolo tile: max 45 min primo pass + 15 min rifinitura post-test in game, poi stop.
- Sprite personaggio (per animazione): max 1 blocco di lavoro (2h) per la prima versione completa, poi si valuta in game al playtest settimanale.
- Shader/effetto: time-box più permissivo (è debug/iterazione tecnica, non gusto estetico) ma fissa comunque un tetto per sessione.

---

## 6 · Organizzazione settimanale

Slot fissi confermati:
- **Lunedì 22:00-24:00**
- **Giovedì 22:00-24:00** (quando non hai allenamento)
- **Sabato mattina, 2h dopo colazione**

Suggerimento di allocazione per fase (adatta liberamente, è solo un default sensato):
- **Sabato mattina** (mente più fresca, sessione più lunga in un colpo solo) → lavoro tecnico che richiede concentrazione: setup shader/luci (§3), gameplay residuo (Fase 1).
- **Lunedì/giovedì sera** (energia più bassa dopo lavoro/allenamento) → produzione asset ripetitiva (pixel art di tile/varianti), audio (SFX, missaggio), o iterazione su un asset già impostato.
- **Playtest completo della demo ogni settimana** → mantienilo fisso, è il tuo miglior rilevatore di scope creep, regressioni e problemi di leggibilità (specialmente utile per validare il criterio di stop di §5 sotto luce/filtro reali).

---

## 7 · Backlog post-demo

Qualsiasi idea che emerge durante il lavoro ("e se aggiungessi...", nuovo Pit Object, nuova meccanica, nuovo nemico) va annotata qui sotto (o in un file/board dedicato) e **non** implementata prima che la demo sia conclusa.

```
- [ ] (esempio) ...
```

---

## 8 · Checklist gameplay residuo (Fase 1)

- [ ] Quick use menu — UI
- [ ] Quick use menu — logica di utilizzo oggetti
- [ ] Teleport trigger per flashback/tutorial (prima parte del gioco)
- [ ] 2-3 nuovi Pit Object oltre a ClayBlock
- [ ] Implementazione definitiva status effect (Poison/Burn/Frost — vedere §13 di `GAMEPLAY.md` per lo stato attuale)

---

*Life Eaters — Roadmap di chiusura demo — creato 2026-07-22.*
