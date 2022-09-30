# Esame di Sistemi Distribuiti: sistema di coordinamento di una flotta di droni per le consegne

- [Esame di Sistemi Distribuiti: sistema di coordinamento di una flotta di droni per le consegne](#esame-di-sistemi-distribuiti-sistema-di-coordinamento-di-una-flotta-di-droni-per-le-consegne)
  - [Introduzione](#introduzione)
  - [Struttura del codice](#struttura-del-codice)
  - [Compilazione ed esecuzione](#compilazione-ed-esecuzione)
    - [Esecuzione dell'applicazione da console](#esecuzione-dellapplicazione-da-console)
    - [Compilazione manuale](#compilazione-manuale)
  - [Uso dell'applicazione da console](#uso-dellapplicazione-da-console)

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

[...work in progress...]