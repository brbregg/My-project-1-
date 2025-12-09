using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointPlatform : MonoBehaviour
{
    [SerializeField] private GameObject[] waypoints;
    private int currnetWaypointIndex = 0;
    [SerializeField] private float speed = 2f;
    void Update()
    {
        if (Vector2.Distance(waypoints[currnetWaypointIndex].transform.position, transform.position) < .1f)
        {
            currnetWaypointIndex++;
            if(currnetWaypointIndex >= waypoints.Length)
            {
                currnetWaypointIndex = 0;
            }
        }
        transform.position = Vector2.MoveTowards(transform.position,waypoints[currnetWaypointIndex].transform.position,Time.deltaTime*speed);
    }
}
