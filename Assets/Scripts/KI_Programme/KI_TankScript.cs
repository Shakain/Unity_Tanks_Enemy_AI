using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;

public class KI_TankScript : MonoBehaviour
{
    public GameObject enemyTank;
    public int playerNumber;

    //Nicht gekplappt
    


    //Eigene Kompunenten
    private Transform fireTransForm;
    //Die eigentliche transform ist nicht richtig angelegt deswegen brauche ich die Referenz von einem Childe
    private Transform mainTransform;
    private InputInterface inputInterface;
    private TankShooting shootScript;

    //Für die KI für den NAvAgent
    private GameObject navAgent;
    private AgentScript agentScript;
    private NavMeshAgent agentComponent;


    //Für die Zielen benötigt
    //Ist der Input String
    private string ki_FireButton;
    private float calcSchussStärke;
    private bool isAmZielen;
    private float zeitPerDistanceSteigung = 0.05882352941f;
    private float zeitPerDistanceGrundWert = -0.8823529412f;

    private float maxReichweite;
    public float trefferVarianz;
    public bool isRadomMoven;


    //Für den Loop da
    private float timer = 0;


    // Start is called before the first frame update
    private void OnEnable()
    {
        print("ich wurde enabelt");
    }
    void Start()
    {
        inputInterface = GetComponent<InputInterface>();
        shootScript = GetComponent<TankShooting>();
        isAmZielen = false;
        ki_FireButton = ki_FireButton = "";

        maxReichweite = 40.5f;
        //isRadomMoven = false;

        fireTransForm = transform.Find("FireTransform");
        mainTransform = transform.Find("TankRenderers");


        navAgent = Instantiate(new GameObject(), fireTransForm.position, fireTransForm.rotation);
        navAgent.AddComponent<NavMeshAgent>();
        navAgent.AddComponent<AgentScript>();
        agentScript = navAgent.GetComponent<AgentScript>();

        agentComponent = navAgent.GetComponent<NavMeshAgent>();
        agentComponent.speed = gameObject.GetComponent<TankMovement>().m_Speed;
        agentComponent.acceleration = gameObject.GetComponent<TankMovement>().m_Speed;
        agentComponent.angularSpeed = gameObject.GetComponent<TankMovement>().m_TurnSpeed;
        agentComponent.autoBraking = true;
    }

    // Update is called once per frame
    void Update()
    {
        BereiteInputsVor();
        KI_Verhalten();
        StartCoroutine(inputInterface.setPrevs());

    }

    /// <summary>
    /// Diese Beiden Mehoden müssen immer anfang aufgerufen werden
    /// </summary>
    private void BereiteInputsVor()
    {
        inputInterface.Clear();

        inputInterface.AddButtonPressed(ki_FireButton);
    }


    /// <summary>
    /// Dasnist die Ki Main Methoden grob berechnet sie alle Relevanten Werte setz den NAvMesch im Position und entscheidet ob sich bewgt oder geziehlt wird.
    /// </summary>
    private void KI_Verhalten()
    {
        timer += Time.deltaTime;
        //Wenn der Selbst aufgegebene wenn der Timer über die Cooldown zeit hinaus ist und man nicht scho am zielen ist wird ein Ziehlvorgang gestartet.
        if (timer > BerechneZeitPerDistance(mainTransform.position, enemyTank.transform.Find("TankRenderers").position))
        {
            if (!isAmZielen)
            {
                inputInterface.currentAxis_Values["Vertical" + playerNumber] = 0f;
                isAmZielen = true;
            }
        }
        else
        {
            isAmZielen = false;
        }


        //Immer wenn sich der Agent sich seinen Ziel Punkt nah ist wir er sich einen Neuen Punkt in angriff reichweite des Tanks suchen und sein ziehl darauf setzen
        if (agentComponent.remainingDistance < 1)
        {
            agentScript.ZumPunktBewegen(enemyTank.transform.Find("TankRenderers").position);
        }

        //Damit der Agent sich nicht zu weit weg des Tanks befendet
        AgentNeuPositionierung();
        

        if (isAmZielen)
        {
            //BerechneWurfparabel(fireTransForm.position, BerechneVarianz(enemyTank.transform.Find("TankRenderers").position, trefferVarianz), 10);
            BerechneWurfparabel(fireTransForm.position, enemyTank.transform.Find("TankRenderers").position, 10);
            Zielen();
        }
        else
        {
            ki_FireButton = "";
            Bewegen();
        }
       
        //Absicherung da es sonst unter bestimmten zuständen möglich wäre das man nicht mehr schießt.
        if (timer > 3)
        {
            Shoot();
        }
    }


    /// <summary>
    /// Berechnet die Zeit schwischen den Schüssen der KI äbhängig von der Entferung des Ziel. Wenn sie weiter weg ist braucht man dauert es länger
    /// </summary>
    /// <param name="eigenePos"></param>
    /// <param name="enemyPos"></param>
    /// <returns></returns>
    private float BerechneZeitPerDistance(Vector3 eigenePos, Vector3 enemyPos)
    {
        Vector3 planarTarget = new Vector3(enemyPos.x, 0, enemyPos.z);
        Vector3 planarPostion = new Vector3(eigenePos.x, 0, eigenePos.z);

        float distance = Vector3.Distance(planarTarget, planarPostion);

        //Funktin f(x)=mx+b m = zeitPerDistanceSteigung , x = distance, b = zeitPerDistanceGrundWert


        return distance * zeitPerDistanceSteigung + zeitPerDistanceGrundWert;
    }

    /// <summary>
    /// Berechnet den Winkel den der Panzer braucht um sich zu ziel zu rotieren un richtig sich danach aus.Entscheidet ob man schiepen kann und stellt alle Parameter passend ein.
    /// </summary>
    private void Zielen()
    {
        //Winkel berechnung
        Vector3 relativePos = enemyTank.transform.Find("TankRenderers").position - transform.Find("TankRenderers").position;
        Vector3 forward = transform.forward;
        float angle = Vector3.Angle(relativePos, forward);

        //Wenn der Winkel kleiner als angebeben ist wird nicht mehr verändert.
        if (Mathf.Abs(angle) < 3)
        {
            
            inputInterface.currentAxis_Values["Horizontal" + playerNumber] = 0f;

            //Wenn die benötigte stärke oder die Maximale stärke errecht geschossen
            if (shootScript.m_CurrentLaunchForce >= calcSchussStärke || shootScript.m_CurrentLaunchForce >= shootScript.m_MaxLaunchForce)
            {
                Shoot();
            } else
            {
                if(DarfManSchiessen())
                {
                    ki_FireButton = ki_FireButton = "Fire" + playerNumber;
                } else
                {
                    //Wenn es nicht nicht geeignet ist zu schiesse wird der Zielen Vorgang abgebrochen
                    isAmZielen = false;
                    timer = 0f;
      
                }

            }
        } else
        {
            if (Vector3.Cross(forward, relativePos).y < 0)
            {
                inputInterface.currentAxis_Values["Horizontal" + playerNumber] = -1.0f;
            }
            else
            {
                inputInterface.currentAxis_Values["Horizontal" + playerNumber] = 1.0f;
            }
        }

    }

 
    /// <summary>
    /// Macht alles nötige was gebraucht wird wenn geschossen wird.
    /// </summary>
    private void Shoot()
    {
        ki_FireButton = "";
        isAmZielen = false;
        timer = 0f;
    }

    /// <summary>
    /// Cast ein Ray vor dem Panzer um zu schauen ob sich etwas anderes als der gegnerische Panzer vor einem Befindet.
    /// </summary>
    /// <returns>True = der Panzer oder nicht ist vor einem: Flase = Etwas ist vor einem</returns>
    private bool DarfManSchiessen()
    {
        if (Physics.Raycast(mainTransform.position, mainTransform.TransformDirection(Vector3.forward), out RaycastHit hitInfoo, 5f))
        {
            print(hitInfoo.collider.gameObject == enemyTank);
            return hitInfoo.collider.gameObject == enemyTank;
        }
        return true;
    }

    /// <summary>
    /// Wenn sich der Agent zu weit weg bewegt wird er wieder zu tank zurück Teleportiert. Um die Genauigkeit zu bewahren.
    /// </summary>
    private void AgentNeuPositionierung()
    {
        if(!IsImPunktRadius(mainTransform.position, navAgent.transform.position, 5))
        {
            navAgent.transform.position = fireTransForm.position;
            navAgent.transform.rotation = fireTransForm.rotation;
        }
    }

    /// <summary>
    /// Rotiert sich immer so das er den NavMeshAgent anschaut und bewegt sich nach vorne wenn der Nav Agent eine bestimmten entfernug weg ist.
    /// </summary>
    private void Bewegen()
    {

        //Wenn der nav Mesh nicht in deiner Distance ist bewege dich
        if(Vector3.Distance(mainTransform.position, navAgent.transform.position) <= 1f)
        {
            inputInterface.currentAxis_Values["Vertical" + playerNumber] = 0f; 
        } else
        {
            inputInterface.currentAxis_Values["Vertical" + playerNumber] = 1.0f;
        }


        Vector3 relativePos = navAgent.transform.position -mainTransform.position;
        Vector3 forward = transform.forward;
        float angle = Vector3.Angle(relativePos, forward);


        //Wenn die winkelabstant kleiner als angegeben ist dann wird nicht rotuiert sonst wirt passend rotiert.
        if (Mathf.Abs(angle) < 5)
        {

            inputInterface.currentAxis_Values["Horizontal" + playerNumber] = 0f;

           
        }
        else
        {
            if (Vector3.Cross(forward, relativePos).y < 0)
            {
                inputInterface.currentAxis_Values["Horizontal" + playerNumber] = -1.0f;
            }
            else
            {
                inputInterface.currentAxis_Values["Horizontal" + playerNumber] = 1.0f;
            }
        }
    }

    /// <summary>
    /// Berechnet ob sich die übergebene Position im Radius der zweiten Position findet
    /// </summary>
    /// <param name="aktuellePosition">Position bei der man sich frag ob sie im Radius ist</param>
    /// <param name="mittelPunkt">Der Mittelpunkt des Kreis Bereichs</param>
    /// <param name="radius">Die Reichweite um den Mittelpunkt herum</param>
    /// <returns></returns>
    private bool IsImPunktRadius(Vector3 aktuellePosition, Vector3 mittelPunkt, float radius)
    {

        return Vector3.Distance(aktuellePosition, mittelPunkt) <= radius;
    }

    
    /// <summary>
    /// Berechenet ein Punkt in Radius des Ziels abhängig der treffervarianz und der nähe des Zieln.
    /// </summary>
    /// <param name="zielPosition">Der Mittelpunkt des Radius</param>
    /// <param name="treffervarianz">Der Wert der Maximal möglichen abweichung des Mittelpunks</param>
    /// <returns></returns>
    public Vector3 BerechneVarianz(Vector3 zielPosition, float treffervarianz)
    {
        Vector3 planarTarget = new Vector3(zielPosition.x, 0, zielPosition.z);
        Vector3 planarPostion = new Vector3(mainTransform.position.x, 0, mainTransform.position.z);

        float distance = Vector3.Distance(planarTarget, planarPostion);

        float varianz = distance * (trefferVarianz/maxReichweite);

        Vector3 randomDirection = Random.insideUnitSphere;


        Vector3 randomPoint = zielPosition + randomDirection * varianz;

        print(randomPoint);
        return new Vector3(randomPoint.x, 1.7f, randomPoint.z);
    }

    /// <summary>
    /// Berechnet die Benötigte velocity aus um das Projektiel auf den angegebenen punkt mit angegebenen winkel zu treefen.
    /// Diese Methode habe ich aus dem Internet und obwohl ich sie zwar teilweise verstehe struggle ich mit meinem Physikalischen verständnis sin kommplett zu verstehen da die Berechnen 
    /// eines Kugelswurf und dessen umformung nicht ganz aufgeht
    /// https://www.leifiphysik.de/mechanik/waagerechter-und-schraeger-wurf/aufgabe/kugelstossen
    /// </summary>
    /// <param name="wurfPosition">Position des Panzers</param>
    /// <param name="zielPosition">Position des Ziels</param>
    /// <param name="winkel">Der Wikel mit dem man Schießt</param>
    public void BerechneWurfparabel(Vector3 wurfPosition, Vector3 zielPosition, float winkel)
    {
        // Entferne 'static' aus den Variablen, da sie jetzt als Parameter übergeben werden
        Vector3 p = wurfPosition;

        float gravity = Physics.gravity.magnitude;
        float angle = winkel * Mathf.Deg2Rad;

        Vector3 planarTarget = new Vector3(zielPosition.x, 0, zielPosition.z);
        Vector3 planarPostion = new Vector3(wurfPosition.x, 0, wurfPosition.z);

        float distance = Vector3.Distance(planarTarget, planarPostion);
        float yOffset = wurfPosition.y - zielPosition.y;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        calcSchussStärke = initialVelocity;
        //print("INtialBitte " + initialVelocity);
        //print("Bitte ?" + finalVelocity);
    }

    //FriedHof
    
    /*
     private float rechneBenoetigteHoehe(float x)
    {
        /*
        float launchAngleRad = launchAngle * Mathf.Deg2Rad;

        float timeOfFlight = (2 * initialVelocity * Mathf.Sin(launchAngleRad)) / gravity;
        float maxHeight = initialHeight + (initialVelocity * Mathf.Sin(launchAngleRad) * timeOfFlight) - (0.5f * gravity * Mathf.Pow(timeOfFlight, 2));

        float height = initialHeight + initialVelocity * Mathf.Sin(launchAngleRad) * ((x / initialVelocity) + (Mathf.Sqrt(Mathf.Pow((x / initialVelocity), 2) + (2 * (maxHeight - initialHeight) / gravity))));

        return height;
        

        return 0;
    }

        /// <summary>
    /// Berechnet den Winkel die der Panzer braucht um zur zielPosition rotiert zu sein.
    /// </summary>
    /// <param name="zielPosition">Die Andere position zu dem der Rotierungs Winkel Berechnet wird</param>
    /// <returns>Float den Exaxten Wnkel</returns>
    private float BerechneWinkel(Vector3 zielPosition)
    {
        Vector3 relativePos = zielPosition - eingeneMainPosition.position;
        Vector3 forward = transform.forward;
        float angle = Vector3.Angle(relativePos, forward);
        return angle;
    }

     * if (!shootScript.m_Fired)
            {
                print("Ich Wurde gefeuert");
                ki_FireButton = "";
            }*/

            /*if (isAufladen)
            {
                Aufladen();
            }
            
            /*
            if (Input.GetKey(KeyCode.F))
            {
                //print("F");
                //print("Schuss Position" + fireTransForm.position);
                //print("Schuss stärke" + Schussstärke(fireTransForm.position, enemyTank.transform.Find("TankRenderers").position, 10));
                //calcSchussStärke = 25f;
                //Aufladen();
                ki_FireButton = ki_FireButton = "Fire" + playerNumber;
            } else
            {
                //ki_FireButton = ki_FireButton = "Fire" + playerNumber;
            }
      //Sehr viel versucht das nicht geklappt hat !!!
        //rotation = Quaternion.LookRotation(relativePos);



        //print(zielErfassung.transform.rotation.y);
        
        if(zielErfassung.transform.rotation.y > 0)
        {
            //Links
            inputInterface.currentAxis_Values["Horizontal2"] = -1.0f;
            print("If1");
        } 
        
        if (zielErfassung.transform.rotation.y < 0)
        {
            //Rechts
            inputInterface.currentAxis_Values["Horizontal2"] = 1.0f;
            print("if2");
        }

        if (zielErfassung.transform.rotation.y < .1f && zielErfassung.transform.rotation.y > -.1f)
        {
            print("Fire");
            inputInterface.currentAxis_Values["Horizontal2"] =0f;
        }

      private float VectorenAbstand(Vector3 v1, Vector3 v2)
    {
        return Mathf.Sqrt(Mathf.Pow(v2.x - v1.x, 2) + 0 + Mathf.Pow(v2.z - v1.z, 2));
    }


private float Schussstärke(Vector3 schussPos, Vector3 zielPoss, float winkel)
    {
        //Vector3 test = wirklichePosition;
        //print("bitte = " + wirklichePosition);
        print("schuss!!!!! " + schussPos);
        float distance = VectorenAbstand(schussPos, zielPoss);
        print("Der Abstand entspricht " + distance);
        print("Das ziel Befindet sich bei" + zielPoss);
        float geschwindigkeit = Mathf.Sqrt((Physics.gravity.magnitude / 2 * (Mathf.Tan(winkel) * distance - schussPos.y))) * (distance / Mathf.Cos(winkel));
        return geschwindigkeit;
    }

    private void Aufladen()
    {
        
        print("");
        print("");
        print("current " +  m_CurrentLaunchForce);
        print("calcSchuss" + calcSchussStärke);
        print("MaxForce" + m_CurrentLaunchForce);

        if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
        {
            //at max carge , not yet fired
            m_CurrentLaunchForce = m_MaxLaunchForce;

            //m_CurrentLaunchForce = m_MinLaunchForce;
        }
        else if (inputInterface.GetButtonDown(ki_FireButton))
        {
            //have we pressed fire for the first time?
            m_Fired = false;
            m_CurrentLaunchForce = m_MinLaunchForce;

        }
        else if (inputInterface.GetButton(ki_FireButton) && !m_Fired)
        {
            //Holding the fire button, nit yet fired
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
        }
        else if (inputInterface.GetButtonUp(ki_FireButton) && !m_Fired)
        {
            //we released the button, having not fired yet
            m_CurrentLaunchForce = m_MinLaunchForce;
        }

        if (m_CurrentLaunchForce >= calcSchussStärke /*|| m_CurrentLaunchForce >= m_MaxLaunchForce)
        {
            //print("HAbe gefeuert");
            //inputInterface.GetButtonUp(m_FireButton);
            isAmZielen = false;
            ki_FireButton = "";
            m_CurrentLaunchForce = m_MinLaunchForce;
        } else
        {
            //inputInterface.GetButtonDown(m_FireButton);
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
            //print("bin am Auflande");
        }


    }

    float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        // Berechne die Differenz zwischen den Punkten
        Vector3 difference = b - a;

        // Berechne den Winkel mit Atan2
        float angle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        // Der Winkel könnte negativ sein, normalisiere ihn
        if (angle < 0)
        {
            angle += 360f;
        }

        return angle;
    }
    */

}
