using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
    public LayerMask m_TankMask;
    public ParticleSystem m_ExplosionParticles;       
    public AudioSource m_ExplosionAudio;              
    public float m_MaxDamage = 100f;                  
    public float m_ExplosionForce = 1000f;            
    public float m_MaxLifeTime = 2f;                  
    public float m_ExplosionRadius = 5f;

    //testen
    public Vector3 shussPosition;
    public GameObject speicher;

    private void Start()
    {
        shussPosition = new Vector3 (0, 0, 0);
        TankShooting.OnPositionChanged.AddListener(HandlePositionChanged);

        Destroy(gameObject, m_MaxLifeTime);
    }


    private void OnTriggerEnter(Collider other)
    {
        //print("on triggger enter" + shussPosition);
        // Find all the tanks in an area around the shell and damage them.
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

            if (!targetRigidbody)
            {
                continue;
            }

            targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

            TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();

            if (!targetHealth)
            {
                continue;
            }

            float damage = CalculateDamage(targetRigidbody.position);

            targetHealth.TakeDamage(damage);
        }

        //Unparrent das Partikel children
        m_ExplosionParticles.transform.parent = null;

        m_ExplosionParticles.Play();

        m_ExplosionAudio.Play();

        //TankShooting.SetTargetWithAngle(gameObject.transform.position, 10f);
        //print(TankShooting.staticTransform.position);
        //print(" Mit 350 Grad " + TankShooting.KostaBerechnung(shussPosition, gameObject.transform.position, 350f));
        //print(" Mit 80 Grad " + TankShooting.KostaBerechnung(shussPosition, gameObject.transform.position, 80f));
        //print(" Mit 10 Grad " + TankShooting.KostaBerechnung(shussPosition, gameObject.transform.position, 10f));
        //print("Schuss Position" + shussPosition);
        //print("Wirkliche Position " + gameObject.transform.position);


        Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.duration);
        Destroy(gameObject);
    }

    private void HandlePositionChanged(Vector3 neuePosition)
    {
        // Hier kannst du auf die aktualisierte Position zugreifen
        shussPosition = neuePosition;
        //print("Wau");
    }


    private float CalculateDamage(Vector3 targetPosition)
    {
        // Calculate the amount of damage a target should take based on it's position.
        Vector3 explosionToTarget = targetPosition - transform.position;

        float explosionDisance = explosionToTarget.magnitude;

        float relativeDistance = (m_ExplosionRadius - explosionDisance) / m_ExplosionRadius;

        float damage = relativeDistance * m_MaxDamage;

        damage = Mathf.Max(0f, damage);

        return damage;
    }
}