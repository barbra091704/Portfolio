using Unity.Netcode;

namespace LevelGenerator.Scripts
{
    public class DeadEnd : NetworkBehaviour
    {
        /// <summary>
        /// Bounds node in hierarchy
        /// </summary>
        public Bounds Bounds;
        public NetworkObject networkObject;

        public void Initialize(LevelGenerator levelGenerator)
        {
            networkObject.Spawn(true);
            networkObject.TrySetParent(levelGenerator.Container);
            levelGenerator.RegisterNewDeadEnd(Bounds.Colliders);
        }
    }
}