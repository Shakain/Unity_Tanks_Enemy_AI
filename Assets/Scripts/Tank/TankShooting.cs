using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class PositionChangedEvent : UnityEvent<Vector3> { }

public class TankShooting : MonoBehaviour
{
    public int m_PlayerNumber = 1;       
    public Rigidbody m_Shell;            
    public Transform m_FireTransform;    
    public Slider m_AimSlider;           
    public AudioSource m_ShootingAudio;  
    public AudioClip m_ChargingClip;     
    public AudioClip m_FireClip;         
    public float m_MinLaunchForce = 15f; 
    public float m_MaxLaunchForce = 30f; 
    public float m_MaxChargeTime = 0.75f;

    //Wichtige parameter zum berechnen des Schusses
    [HideInInspector]
    public string m_FireButton;
    [HideInInspector]
    public float m_CurrentLaunchForce;
    [HideInInspector]
    public float m_ChargeSpeed;         
    public bool m_Fired;
    
    //zum testen
    public static Transform staticTransform;
    private static ProjectileArc projectileArc;
    public Vector3 wirklichePosition;
    public GameObject speicher;

    //Kosta
    InputInterface inputInterface;

    [SerializeField]
    private LineRenderer lineRenderer;
    [Header("Display Controls")]
    [Range(10, 100)]
    private int linePoints = 25;
    [SerializeField]
    [Range(0.01f, 0.25f)]
    private float timeBetweenPoints = 0.1f;
    [SerializeField]
    private Transform releasePosition;
    private LayerMask shellCollissionmask;

    private void OnEnable()
    {
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_AimSlider.value = m_MinLaunchForce;
    }


    private void Start()
    {
        //Testen
       
        //projectileArc = new ProjectileArc();

        m_FireButton = "Fire" + m_PlayerNumber;

        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;

        //Kosta
        inputInterface = GetComponent<InputInterface>();
        int shellLayer = m_Shell.gameObject.layer;

        for (int i = 0; i < 32; i++)
        {
            if (!Physics.GetIgnoreLayerCollision(shellLayer, i))
            {
                //Die zeile hier ist Magic die beschreibung sagt das sie ein layer auf einem Layer mask added
                shellCollissionmask |= 1 << i;
            }
        }
    }

    private void Awake()
    {
        
    }


    private void Update()
    {
        staticTransform = m_FireTransform;
        wirklichePosition = new Vector3(transform.position.x + m_FireTransform.localPosition.x, transform.position.y + m_FireTransform.localPosition.y, transform.position.z + m_FireTransform.localPosition.z);
        //print("Position in Update " + wirklichePosition);
        //OnPositionChanged.Invoke(wirklichePosition);
        //print(wirklichePosition);
        //print(transform.position);
        // Track the current state of the fire button and make decisions based on the current launch force.
        m_AimSlider.value = m_MinLaunchForce;


       //Original nutzt Input statt inputinerface

        if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
        {
            //at max carge , not yet fired
            m_CurrentLaunchForce = m_MaxLaunchForce;
            Fire();
        }
        else if (inputInterface.GetButtonDown(m_FireButton))
        {
            //have we pressed fire for the first time?
            m_Fired = false;
            m_CurrentLaunchForce = m_MinLaunchForce;

            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play();
        }
        else if (inputInterface.GetButton(m_FireButton) && !m_Fired)
        {
            //Holding the fire button, nit yet fired
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

            m_AimSlider.value = m_CurrentLaunchForce;

            DrawProjection();
        }
        else if (inputInterface.GetButtonUp(m_FireButton) && !m_Fired)
        {
            //we released the button, having not fired yet
            Fire();
        }
    }


    private void Fire()
    {
        //print("rotatin x = " + m_FireTransform.rotation.x + "y = " + m_FireTransform.rotation.y + " z = " + m_FireTransform.rotation.z);

        lineRenderer.enabled = false;//Kosta
        // Instantiate and launch the shell.
        m_Fired = true;

        
        Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

        //print("Rotation " + m_FireTransform.rotation);
        //print("Position " + m_FireTransform.position);
        //print("forward" + m_FireTransform.forward);
        //print("Force " + m_CurrentLaunchForce * m_FireTransform.forward);
        //                          Force
        shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;
        shellInstance.GetComponent<ShellExplosion>().shussPosition = wirklichePosition;
        //print("Wirkliche Speed = " + m_CurrentLaunchForce);

        float flugzeit = (2f * shellInstance.velocity.y) / Mathf.Abs(Physics.gravity.y);

        // Berechne die horizontale Verschiebung basierend auf der Geschwindigkeit in der XZ-Ebene
        Vector3 horizontalVerschiebung = new Vector3(shellInstance.velocity.x, 0f, shellInstance.velocity.z) * flugzeit;

        // Berechne die endgültige Position, indem du die aktuelle Position um die horizontale Verschiebung erhöhst
        Vector3 endPosition = m_FireTransform.position + horizontalVerschiebung;

        //endPosition = endPosition + m_FireTransform.position;
        //Debug.Log("Projektil landet bei Position: " + endPosition);

        //Vector3 v = m_FireTransform.rotation * new Vector3(0, 0, 1);

        //float t = v.z * m_CurrentLaunchForce / (-9.81f * Time.deltaTime);


        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();

        m_CurrentLaunchForce = 0;
    }

    public static PositionChangedEvent OnPositionChanged = new PositionChangedEvent();

    public static float KostaBerechnung(Vector3 schussPos, Vector3 zielPoss, float winkel)
    {
        //Vector3 test = wirklichePosition;
        //print("bitte = " + wirklichePosition);
        print("schuss!!!!! " + schussPos);
        float distance = VectorenAbstand(schussPos, zielPoss);
        print("Der Abstand entspricht " + distance);
        float geschwindigkeit = Mathf.Sqrt((Physics.gravity.magnitude/2*(Mathf.Tan(winkel) * distance - schussPos.y))) * (distance/Mathf.Cos(winkel));
        return geschwindigkeit;
    }

    public static float VectorenAbstand(Vector3 v1, Vector3 v2)
    {
        return Mathf.Sqrt(Mathf.Pow(v2.x- v1.x, 2) + 0 + Mathf.Pow(v2.z- v1.z, 2));
    }

    static public void SetTargetWithAngle(Vector3 point, float angle)
    {
        //print("angel" + angle);
        float normalizedAngle = angle % 360f;
        if (normalizedAngle < 0)
        {
            normalizedAngle += 360f;
        }

        //Debug.Log("normelizedAngel" + normalizedAngle);

        float currentAngle = angle;
        //print("current angle = " + currentAngle);

        Vector3 direction = point - staticTransform.position;
        //print("direction" + direction);
        float yOffset = -direction.y;
        //print("yOffset" + yOffset);
        //print("//////////////////////1111111//////////////////////////");
        direction = Math3d.ProjectVectorOnPlane(Vector3.up, direction);
        //print("direction2" + direction);
        float distance = direction.magnitude;
        //Debug.Log("distance: " + distance);
        //Debug.Log("gravity: " + Physics.gravity.magnitude);
        //Debug.Log("angle: " + angle * Mathf.Deg2Rad);

        float numerator = distance * Mathf.Sqrt(Physics.gravity.magnitude) * Mathf.Sqrt(1 / Mathf.Cos(angle * Mathf.Deg2Rad));

        //Debug.Log("distance: " + distance);
        //Debug.Log("gravity: " + Physics.gravity.magnitude);
        //Debug.Log("yOffset" + yOffset);
        //Debug.Log("angle: " + angle * Mathf.Deg2Rad);
        float denominator = Mathf.Sqrt(2 * distance * Mathf.Sin(angle * Mathf.Deg2Rad) + 2 * yOffset * Mathf.Cos(angle * Mathf.Deg2Rad));

        //Debug.Log("numerator: " + numerator);
        //Debug.Log("denominator: " + denominator);

        float currentSpeed = ProjectileMath.LaunchSpeed(distance, yOffset, Physics.gravity.magnitude, angle * Mathf.Deg2Rad);
        //print("/////////////////////22222222///////////////////////////");
        //print((distance * Mathf.Sqrt(Physics.gravity.magnitude) * Mathf.Sqrt(1 / Mathf.Cos(angle * Mathf.Deg2Rad))) / Mathf.Sqrt(2 * distance * Mathf.Sin(angle * Mathf.Deg2Rad) + 2 * yOffset * Mathf.Cos(angle * Mathf.Deg2Rad)));

        //print("//////////////////////333333333//////////////////////////");


        //projectileArc.UpdateArc(currentSpeed, distance, Physics.gravity.magnitude, currentAngle * Mathf.Deg2Rad, direction, true);
        //SetTurret(direction, currentAngle);

        float currentTimeOfFlight = ProjectileMath.TimeOfFlight(currentSpeed, currentAngle * Mathf.Deg2Rad, yOffset, Physics.gravity.magnitude);
        print("Der Typ         =" + currentSpeed);
    }


    private static void Test()
    {
        //var rigid = GetComponent<Rigidbody>();

        Vector3 p = staticTransform.position;

        float gravity = Physics.gravity.magnitude;
        // Selected angle in radians
        float angle = staticTransform.rotation.x * Mathf.Deg2Rad;

        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPostion = new Vector3(staticTransform.position.x, 0, staticTransform.position.z);

        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPostion);
        // Distance along the y axis between objects
        float yOffset = staticTransform.position.y - p.y;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        // Rotate our velocity to match the direction between the two objects
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        // Fire!
        print(finalVelocity);

        // Alternative way:
        // rigid.AddForce(finalVelocity * rigid.mass, ForceMode.Impulse);
    }

    private void DrawProjection()
    {
        /*
        lineRenderer.enabled = true;
        lineRenderer.positionCount = Mathf.CeilToInt(linePoints / timeBetweenPoints) + 1;

        Vector3 startPosition = m_FireTransform.position;
        Vector3 startVelocity = m_CurrentLaunchForce * m_FireTransform.forward / m_Shell.mass;

        int i = 0;
        lineRenderer.SetPosition(i, startPosition);

        //die Schleife spiegelt soweit wie ich das verstehe d=v(kein i)*t + 1/2*a*t^2
        //vi ist der Durchlauf die wie weit es ist
        //t = Zeit
        //a = beschleinigung
        for (float time = 0; time < timeBetweenPoints; time += timeBetweenPoints)
        {
            //(klein i)
            i++;
            //v(klein i) * t
            Vector3 point = startPosition + time * startVelocity;
            // 1/2*a*t^2
            //                              v(klein i)      t     |      1/2*a          |     t^2
            point.y = startPosition.y + startPosition.y * time + (Physics.gravity.y / 2f * time * time);

            //Das +
            lineRenderer.SetPosition(i, point);
            print(i);
            print(point);
            
        }*/
        
        lineRenderer.enabled = true;
        lineRenderer.positionCount = Mathf.CeilToInt(linePoints / timeBetweenPoints) + 1;
        Vector3 startPosition = releasePosition.position;
        Vector3 startVelocity = m_CurrentLaunchForce * releasePosition.forward / m_Shell.mass;
        int i = 0;
        lineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < linePoints; time += timeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            lineRenderer.SetPosition(i, point);

            Vector3 lastPosition = lineRenderer.GetPosition(i - 1);

            if (Physics.Raycast(lastPosition,
                (point - lastPosition).normalized,
                out RaycastHit hit,
                (point - lastPosition).magnitude,
                shellCollissionmask))
            {
                lineRenderer.SetPosition(i, hit.point);
                lineRenderer.positionCount = i + 1;
                return;
            }
        }
    }
  
}