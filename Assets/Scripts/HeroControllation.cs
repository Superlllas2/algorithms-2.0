using UnityEngine;
using UnityEngine.AI;

public class HeroController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Camera mainCamera;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("No camera found");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(Input.mousePosition);
            // Checking where we click from the camera perspective
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                Debug.Log("Clicked at: " + hit.point);
                agent.SetDestination(hit.point);
            } 
            else
            {
                Debug.Log("Raycast missed");
            }
        }
    }
}