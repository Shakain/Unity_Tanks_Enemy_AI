Unity TANKS! ist eine Tutorial-Reihe von Unity Technologies, die zeigt, wie man ein kleines Shooter-Spiel mit Panzern entwickelt. Ursprünglich war es als 1 vs 1 Spiel zwischen zwei echten Spielern konzipiert.

Diesen Ansatz habe ich aufgegriffen und einen Algorithmus entwickelt, der einen feindlichen Spieler simuliert, gegen den man spielen kann, also eine Enemy-AI.
Dafür habe ich das InputSystem umgeschrieben, um die Schnittstelle in Scripts/Allgemein unter dem Namen InputInterface. Die KI-Arbeit findet selbst in Scripts/KI_Programme statt, speziell in den Skripten AgentScript und mein Hauptwerk KI_TankScript.

Abgesehen von leichten Änderungen im Manager und eine provosorischer ziel laser stammt der Rest nicht von mir.






