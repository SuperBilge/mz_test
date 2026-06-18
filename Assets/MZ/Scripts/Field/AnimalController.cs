using UnityEngine;
using UnityEngine.AI;

namespace MZ.Field
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AnimalController : FieldEntity
    {
        [HideInInspector] public FeedController targetFeed;

        [SerializeField] private NavMeshAgent _agent;

        private void Awake()
        {
            if (_agent == null)
                _agent = GetComponent<NavMeshAgent>();
        }

        public void WarpAgent()
        {
            _agent.Warp(transform.position);
        }

        public void SetAgentDestination(Vector3 worldPosition)
        {
            _agent.SetDestination(worldPosition);
        }

        public bool IsAtDestination()
        {
            if (_agent.pathPending) return false;
            if (!_agent.hasPath) return false;
            return _agent.remainingDistance <= _agent.stoppingDistance;
        }

        public float AgentSpeed
        {
            set => _agent.speed = value;
        }
    }
}
