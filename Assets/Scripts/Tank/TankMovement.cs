using UnityEngine;

public class TankMovement : MonoBehaviour
{
    public int m_PlayerNumber = 1;          //Zugehörigkeit des Tanks
    public float m_Speed = 12f;            
    public float m_TurnSpeed = 180f;       
    public AudioSource m_MovementAudio;    
    public AudioClip m_EngineIdling;      
    public AudioClip m_EngineDriving;      
    public float m_PitchRange = 0.2f;       //Wird nacher für den Random Calculation verwendet

    
    private string m_MovementAxisName;     //horizontal and Vertikel für getAxis abhängig des PlayerTanks
    private string m_TurnAxisName;         // selbest für Turn
    private Rigidbody m_Rigidbody;         
    private float m_MovementInputValue;   //Das sind die Parameter die nacher die Ki berechnen muss ! 
    private float m_TurnInputValue;        //Das sind die Parameter die nacher die Ki berechnen muss ! 
    private float m_OriginalPitch;
    
    //Kosta hinzugefügt
    private InputInterface inputInterface;


    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();

    }

    //Wenn ein tank stirbt wird dieser disable um in wieder zu leben wird er enable 
    private void OnEnable ()
    {
        m_Rigidbody.isKinematic = false;
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
    }


    private void OnDisable ()
    {
        m_Rigidbody.isKinematic = true;
    }


    //Die namen sind unötig für die Ki
    private void Start()
    {
        m_MovementAxisName = "Vertical" + m_PlayerNumber;       // Die Axis names werden gesetz
        m_TurnAxisName = "Horizontal" + m_PlayerNumber;

        m_OriginalPitch = m_MovementAudio.pitch;

        //Kosta
        inputInterface = GetComponent<InputInterface>();
    }
    

    //Update speichert die Inputs des Payer für die Ki muss das Geändert werden.
    private void Update()
    {
        // Store the player's input and make sure the audio for the engine is playing.

        //m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
        //m_TurnInputValue = Input.GetAxis(m_TurnAxisName);

        //Kosta


        //print(m_MovementAxisName);
        m_MovementInputValue = inputInterface.GetAxis(m_MovementAxisName);
        m_TurnInputValue = inputInterface.GetAxis(m_TurnAxisName);

        EngineAudio();
    }


    private void EngineAudio()
    {
        // Play the correct audio clip based on whether or not the tank is moving and what audio is currently playing.
        if (Mathf.Abs(m_MovementInputValue) < 0.1f && Mathf.Abs(m_TurnInputValue) < 0.1f)
        {
            if (m_MovementAudio.clip == m_EngineDriving)
            {
                m_MovementAudio.clip = m_EngineIdling;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        } else
        {
            if (m_MovementAudio.clip == m_EngineIdling)
            {
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
    }


    //In fixUpdate werden die Sachen aktuallisiert die was mit der Phisik engine zu tuen hat evt kommt da auch nacher shot hin wahrscheinlich aber nicht
    private void FixedUpdate()
    {
        // Move and turn the tank.
        Move();
        Turn();
    }


    //Wahrscheinlich muss ich dann nacher in update die move distanz ausrechenen und die parameter so ändern das dann  move richtig ausgeführt wird
    private void Move()
    {
        // Adjust the position of the tank based on the player's input.
        Vector3 movement = transform.forward  * m_MovementInputValue * m_Speed * Time.deltaTime;

        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }


    private void Turn()
    {
        // Adjust the rotation of the tank based on the player's input.
        float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

        Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);

        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }
}