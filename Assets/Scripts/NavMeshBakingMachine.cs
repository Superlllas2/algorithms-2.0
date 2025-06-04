using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] private NavMeshSurface surface;

    public void BakeNavMesh()
    {
        surface.BuildNavMesh(); // Dynamic mesh baker
    }
}
