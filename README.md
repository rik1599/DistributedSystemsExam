# Esame di Sistemi Distribuiti: sistema di coordinamento di una flotta di droni per le consegne

- [Esame di Sistemi Distribuiti: sistema di coordinamento di una flotta di droni per le consegne](#esame-di-sistemi-distribuiti-sistema-di-coordinamento-di-una-flotta-di-droni-per-le-consegne)
  - [Introduzione](#introduzione)
  - [Struttura del codice](#struttura-del-codice)
  - [Compilazione ed esecuzione](#compilazione-ed-esecuzione)
    - [Esecuzione dell'applicazione da console](#esecuzione-dellapplicazione-da-console)
    - [Compilazione manuale](#compilazione-manuale)
  - [Uso dell'applicazione da console](#uso-dellapplicazione-da-console)
    - [Help e utilities varie](#help-e-utilities-varie)
    - [Creazione e gestione degli actor system](#creazione-e-gestione-degli-actor-system)
    - [Creazione o impostazione del registro](#creazione-o-impostazione-del-registro)
    - [Creazione di missioni](#creazione-di-missioni)
    - [Panoramica generale di missioni e actor system](#panoramica-generale-di-missioni-e-actor-system)
    - [Monitoraggio delle missioni, notifiche e connessione a missioni remote](#monitoraggio-delle-missioni-notifiche-e-connessione-a-missioni-remote)
    - [Comandi sulle missioni (ping e cancel)](#comandi-sulle-missioni-ping-e-cancel)
    - [Dispiegamento del sistema in rete](#dispiegamento-del-sistema-in-rete)

## Introduzione

Il progetto ha come scopo la realzzazione di una soluzione che permetta ad una *flotta di droni* (simulata) di *coordinare i propri spostamenti in uno spazio aereo*, al fine di evitare le collisioni 

Essendo questo un progetto didattico per un esame, quello che si riporta qui è solo un **prototipo di una situazione simulata** (cioè senza droni fisici). Ciò che però è stato implementato sono gli aspetti di comunicazione legati ai **Sistemi Distribuiti**.

In pratica, il prototipo realizzato è già pensato per essere eseguito in un *ambiente distribuito*, dove i processi dei droni sono dispiegati in macchine diverse (oppure sulla stessa macchina, ma in processi separati che comunicano in rete tramite porte diverse).

In breve, l'idea è la seguente.

- Si immagina i droni siano dispositivi molto semplici, privi di sensoristica e capaci solo di volare ad un altezza fissa (uguale per tutti), velocità costante e in linea retta da un punto A ad un punto B. Non si prevede quindi che il drone sia capace di fare cose complesse come cambiare direzione, fermarsi, "vedere" gli altri droni con dei sensori, ecc.

- Ad ogni missione corrisponde un attore (un processo eseguito direttamente sul drone), che comunica con gli altri *tramite scambio di messaggi*.

- Quando un drone vuole decollare per portare a termine una missione, prima di partire esegue un algoritmo distribuito che gli permette di:
    - conoscere tutte le altre missioni attive nello spazio aereo;
    - contrattare con gli altri droni in attesa per schedulare le partenze in modo da evitare collisioni.

Il tutto è stato implementato nel linguaggio **C\# (.NET 6)**, facendo largo uso del framework [AKKA.NET](https://getakka.net/) per gestire la comunicazione tramite messaggi. 

## Struttura del codice

Il codice è organizzato in una soluzione composta da quattro progetti. 

1. [Actors/](Actors/) contiene il codice principale, cioè quello degli attori del sistema e delle loro classi interne.
2. [DroneSystemAPI/](DroneSystemAPI/) è una collezione di classi che permettono di interfacciarsi al sistema tramite un'API object oriented (utilizzabile per gestire aspetti quali l'inizializzazione dei sistemi di attori e il dispiegamento e il monitoraggio delle missioni).
3. [TerminalUI/](TerminalUI/) è l'implementazione di un semplice prototipo di console per interfacciarsi con il sistema (realizzato sopra l'API object oriented);
4. [UnitTests/](UnitTests/) è una collezione di test automatici usati a supporto dell'attività di sviluppo. Tali test coprono principalmente:
    - il comportamento dei singoli attori,
    - il comportamento dell'API,
    - la simulazione di semplici situazioni di conflitto [work in progress].

Per una panoramica della struttura del codice si può fare riferimento ai diagrammi UML (realizzati tramite *Visual Paradigm* e riportati nel file [Distributed Drones.vpp](Distributed%20Drones.vpp), oppure nelle figure contenute nella cartella [diagrams/](diagrams/)).

## Compilazione ed esecuzione

### Esecuzione dell'applicazione da console

Per usare l'applicazione da console, è sufficente scaricare e avviare l'eseguibile più adatto per il proprio sistema operativo dagli asset dell'[ultima release](https://github.com/rik1599/DistributedSystemsExam/releases/tag/v1.1.1). 


### Compilazione manuale

Per compilare ed eseguire la soluzione, si può fare come spiegato di seguito.

1. Scaricare e installare [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

2. clonare il progetto da git in una propria cartella locale

        git clone https://github.com/rik1599/DistributedSystemsExam

        cd DistributedSystemsExam

3. Aprire un terminale nella cartella del progetto e compilare (le dipendenze verranno recuperate automaticamente):

        dotnet build ./DistributedSystemsExam.sln 

4. Eseguire l'applicazione da console tramite il comando:

        dotnet run --project TerminalUI/

In alternativa a tale procedura, si può sempre fare affidamento a *Visual Studio* (soluzione suggerita se si desidera mettere mano al codice). 

Si noti che compilando come spiegato nell'applicazione da terminale si visualizzeranno anche degli output di debug (forse anche utili a comprendere il funzionamento del sistema).

## Uso dell'applicazione da console

L'interfaccia al sistema, per ora è un'applicazione da terminale. Di seguito si riporta una breve guida su come usare l'applicazione. 

### Help e utilities varie

- Per conoscere tutti i comandi, usare

        help

- Per conoscere tutte le opzioni di un comando, usare
        
        help NOME_COMANDO

- Per pulire la console, usare

        clear


### Creazione e gestione degli actor system

Per eseguire le missioni, è necessario avviare almeno un <code>ActorSystem</code>. Un actor system, in *Akka.NET* è una locazione dove si eseguono gli attori e - in un contesto di rete - è caratterizzata da un **indirizzo IP** e da una **porta**. 

- per avviare un actor system (**gestito dall'istanza locale** dell'applicazione da console), usare il comando: 

        create-actor-system

    Il sistema assegnerà automanticamente una porta libera. Se si desidera scegliere la porta, utilizzare l'opzione <code>-p</code>, come mostrato di seguito:

        create-actor-system -p8080

    L'actor system sarà accessibile anche da altre istanze del programma (ad esempio, creabili eseguendo più volte l'applicazione da console), ma non sarà visibile in rete. Per renderlo visibile in rete bisogna esplicitamente assegnarli l'indirizzo IP con il parametro <code>-h</code>. 
    
    A tal proposito, leggere la nota sul [dispiegamento del sistema in rete](#dispiegamento-del-sistema-in-rete).

- Per spegnere un actor system (tra quelli gestiti da questa istanza dell'applicazione) usare il comando:

        terminate-actor-system -p8080

- se termino questa istanza dell'applicazione, tutti gli actor system ovviamente smetteranno di funzionare, così come gli attori dispiegati in essi.

    Si noti però che, potendo dispiegare attori anche su actor system non gestiti da me, se termino, questi continueranno ad eseguire.


### Creazione o impostazione del registro

Il registro (o *repository*) dei nodi è l'unica componente centralizzata del sistema. Consiste in un server che gli attori delle missioni contattano **una sola volta** per ricevere una lista degli altri attori presenti nel sistema.

Per avviare le missioni, è necessario creare o impostare un registro.

- Per avviare un registro su un actor system (locale o remoto) usare il comando:

        spawn-repository -p8080 [-hHOST]

- Per usare un registro già esistente usare il comando 

        set-repository -p8080 [-hHOST]

- Se si ha già creato/impostato un registro, i comandi <code>spawn</code> e <code>set</code> daranno errore. Usare l'opzione <code>-f</code> per forzare la scelta.

- Come si può osservare, il parametro per l'host <code>-h</code> non è obbligatorio. Di base si utilizza come valore <code>localhost</code>.

### Creazione di missioni

Una missione è visibile come la richiesta di percorrere una tratta da un punto di partenza <code>(START_X,START_Y)</code> ad uno di arrivo <code>(END_X,END_Y)</code>. 

Il comando per avviare una missione (su un actor system locale o remoto) è:

        spawn-mission START_X START_Y END_X END_Y -pPORTA [-hHOST] [-nNOME_MISSIONE] 


- I primi quattro parametri rappresentano le coordinate del punto di partenza e di quello di arrivo, sono **numeri interi** e sono **obbligatori**.

- L'opzione <code>-p</code> è la porta dell'actor system dove avviare l'attore ed è anch'essa obbligatoria. L'host <code>-h</code> invece non è obbligatorio; come per i registri, anche in questo caso se non si specifica viene usato come valore <code>localhost</code>.

- L'opzione <code>-n</code> permette di dare un nome alla missione. Il nome viene usato per identificare la missione, peranto è bene sia univoco almeno all'interno dell'actor system dove è stata spawnata (**anche rispetto a missioni passate già terminate**). 

    Il parametro non è obbligatorio; se non si imposta nulla, il sistema genera automaticamente un codice numerico.

Di seguito, si riporta un esempio di uso del comando per avviare una missione di nome <code>A</code> sull'actor-system creato nei comandi precedenti (si noti che - nel prototipo - le missioni possono essere dispiegate liberamente in qualunque actor system, anche in quelli già usati da altre missioni o dal registro):

        spawn-mission 0 0 100 100 -p8080 -nA

**NOTA**: quando successivamente ci si riferirà alla missione, si dovrà sempre specificare il nome (senza <code>-n</code>), la porta (con l'opzione <code>-p</code>) e - se diverso da <code>localhost</code> - l'indirizzo (con l'opzione <code>-h</code>). Struttura di un comando chiamato sulla missione spawnata nel comando precedente:

        COMANDO_SU_MISSIONE A -p8080 [altre opzioni varie]

Esempio di comando su missione spawnata in rete:

        COMANDO_SU_MISSIONE A -p8080 -h192.168.1.192 [altre opzioni varie]


### Panoramica generale di missioni e actor system

Per visualizzare una panoramica generale del sistema (quindi degli actor system, delle missioni e del registro) usare il comando:

    ls

Con le opzioni <code>-s</code>, <code>-m</code> e <code>-r</code> si può scegliere di visualizzare solo i sistemi, le missioni o il registro.

Si noti che il comando permette di vedere solo ciò che è stato avviato da questa istanza dell'applicazione (o ciò a cui ci si è collegati).

### Monitoraggio delle missioni, notifiche e connessione a missioni remote

Una volta creata una missione, l'interfaccia crea automaticamente una connessione che permette di ricevere notifiche. Dalla release <code>1.1.0</code> le notifiche vengono inviate direttamente dalla missione (ad una serie di "iscritti") ad ogni cambiamento di stato. 

- Per visualizzare tutte le notifiche ricevute per una missione, usare il comando:

        log NOME_MISSIONE -pPORTA [-hHOST]

    Di base, i vari stati vengono presentati in ordine di ricezione con un livello di dettaglio intermedio. Con l'opzione <code>-v</code> (<code>--verbose</code>) si può visualizzare tutti i dettagli di ogni stato; con l'opzione <code>-s</code> (<code>--short</code>) invece si può decidere di visualizzare solo una sintesi.

- Per iniziare a monitorare missioni non lanciate da questa interfaccia, si può usare il comando:

        connect-to-mission NOME_MISSIONE -pPORTA [-hHOST]

    Dopo essersi collegati ad una certa missione, si ricevono immediatamente tutte le notifiche precedenti.

### Comandi sulle missioni (ping e cancel)

Sulle missioni sono stati implementati per ora solo due comandi.

- Per richiedere lo stato attuale di una missione si può usare il comando:

        ping NOME_MISSIONE -pPORTA [-hHOST]

    Per chiamare tale comando, la missione deve essere monitorata (quindi creata con <code>spawn-mission</code> oppure collegata con <code>connect-to-mission</code>). Il comando ping ha inoltre diverse opzioni aggiuntive:

    - con <code>-v</code> (<code>--verbose</code>) e <code>-s</code> (<code>--short</code>) si può visualizzare il risultato in modo completo o sintetico (di base, si usa la stessa politica del comando <code>log</code>);
    - con <code>-l</code> (<code>--log</code>) si può aggiungere il risultato alla lista delle notifiche;
    - con <code>-f</code> (<code>--force</code>) si può forzare il tentativo di ping, anche se la missione è segnata come terminata.

- Per annullare una missione si può utilizzare il comando:

        cancel-mission NOME_MISSIONE -pPORTA [-hHOST]

    Come per <code>ping</code> e <code>log</code>, si può chiamare solo su missioni a cui si è connessi. Con l'opzione <code>-k</code> (<code>--kill</code>) si può inviare una <code>PoisonPill</code> invece che una richiesta di annullamento; ciò porterà la missione a terminare in modo brutale (senza avvertire eventuali nodi con cui stava comunicando).

### Dispiegamento del sistema in rete

Dalla release <code>1.1.1</code> si sono predisposti i comandi che permettono di dispiegare il sistema in rete. Per dispiegare un sistema in rete, in fase di creazione degli ActorSystem si specifica l'indirizzo IP (quello della macchina corrente) con il parametro <code>-h</code>. Esempio:

        create-actor-system -p8080 -h192.168.1.192

L'host verrà poi specificato anche in tutti gli altri comandi, sempre con l'opzione <code>-h</code>.

**NOTA IMPORTANTE**: non mescolare ActorSystem locali e remoti. Se in fase di creazione si assegna all'host un valore <code>-hlocalhost</code>, al momento il sistema non risulterà visibile in rete. Attualmente assegnare esplicitamente il proprio indirizzo IP e la stringa <code>localhost</code> **NON** è equivalente.







