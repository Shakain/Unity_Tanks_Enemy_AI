using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentScript : MonoBehaviour
{
    // Start is called before the first frame update

    public NavMeshAgent agent;
    private float searchRadius;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        searchRadius = 40.5f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator SetSpeed(float speed)
    {
        yield return null;
        agent.speed = speed;
    }

    public IEnumerator SetAngle(float angle)
    {
        yield return null;
        agent.angularSpeed = angle;
    }

    public void ZumPunktBewegen(Vector3 ziel)
    {
        agent.SetDestination(FindePunktUmZiel(ziel));
    }

    private Vector3 FindePunktUmZiel(Vector3 ziel)
    {
        Vector3 randomDirection = Random.insideUnitSphere * searchRadius;
        randomDirection += ziel;
        NavMeshHit navMeshHit;

        // Finde den nächsten Punkt auf dem NavMesh im zufälligen Bereich
        if (NavMesh.SamplePosition(randomDirection, out navMeshHit, searchRadius, NavMesh.AllAreas))
        {
            return navMeshHit.position;
        } else
        {
            return ziel;
        }
    }
}
