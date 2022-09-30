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
    - [Monitoraggio delle missioni](#monitoraggio-delle-missioni)
    - [Note](#note)

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

Per usare l'applicazione da console, è sufficente scaricare e avviare l'eseguibile più adatto per il proprio sistema operativo dagli asset dell'[ultima release](https://github.com/rik1599/DistributedSystemsExam/releases/tag/v1.1.0). 


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

    L'actor system sarà accessibile anche da altre istanze del programma (ad esempio, creabili eseguendo più volte l'applicazione da console), ma non sarà visibile in rete (a tal proposito, vedere le [note](#note)).

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


### Panoramica generale di missioni e actor system

[...work in progress...]


### Monitoraggio delle missioni

[...work in progress...]

### Note

- Nella release <code>1.1.0</code> **NON** si sono predispositi i comandi per spawnare actor system visibili in rete. Nel codice attualmente caricato sul branch <code>dev</code> però si è implementata l'opzione <code>-h</code>, che permette di fissare un IP e spawnare un actor system esposto alla rete:

        create-actor-system -p8080 -hINDIRIZZO_IP





