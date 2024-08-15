using System.Collections.Generic;
using System.Linq;
using LevelGenerator.Scripts.Helpers;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;

namespace LevelGenerator.Scripts
{
    public class Section : NetworkBehaviour
    {
        public bool isDoor;
        public Transform door;
        /// <summary>
        /// Section tags
        /// </summary>
        public string[] Tags;


        /// <summary>
        /// Tags that this section can annex
        /// </summary>
        public string[] CreatesTags;

        public List<Vector3> SpawnPoints = new(); 

        /// <summary>
        /// Exits node in hierarchy
        /// </summary>
        public Exits Exits;

        /// <summary>
        /// Bounds node in hierarchy
        /// </summary>
        public Bounds Bounds;
        public Collider boundsCollider;
        public NavMeshSurface navMeshSurface;

        /// <summary>
        /// Chances of the section spawning a dead end
        /// </summary>
        public NetworkObject networkObject;
        public int DeadEndChance;


        protected LevelGenerator LevelGenerator;
        protected int order;
        
        public void Initialize(LevelGenerator levelGenerator, int sourceOrder)
        {
            LevelGenerator = levelGenerator;
            networkObject.Spawn(true);
            networkObject.TrySetParent(LevelGenerator.Container);
            LevelGenerator.RegisterNewSection(this);
            order = sourceOrder + 1;
            GenerateAnnexes();
            GenerateSpawnPoints();
        }
        private void GenerateSpawnPoints()
        {
            for (int i = 0; i < SpawnPoints.Count; i++)
            {   
                Vector3 randomPosition = new(
                    Random.Range(boundsCollider.bounds.min.x, boundsCollider.bounds.max.x),
                    boundsCollider.bounds.center.y,
                    Random.Range(boundsCollider.bounds.min.z, boundsCollider.bounds.max.z)
                );

                if (Physics.Raycast(randomPosition, Vector3.down, 20))
                {
                    SpawnPoints[i] = randomPosition;
                }
            }
        }
        protected void GenerateAnnexes()
        {
            if (CreatesTags.Any())
            {
                foreach (var e in Exits.ExitSpots)
                {
                    if (LevelGenerator.LevelSize > 0 && order < LevelGenerator.MaxAllowedOrder)
                    {
                        // If DeadEndChance is 0, directly call GenerateSection(e)
                        if (DeadEndChance == 0)
                        {
                            GenerateSection(e);
                        }
                        else
                        {
                            // If DeadEndChance is not 0, roll and decide based on the roll
                            if (RandomService.RollD100(DeadEndChance))
                                PlaceDeadEnd(e);
                            else
                                GenerateSection(e);
                        }
                    }
                    else
                    {
                        PlaceDeadEnd(e);
                    }
                }
            }
        }

        protected void GenerateSection(Transform exit)
        {
            var candidate = IsAdvancedExit(exit)
                ? BuildSectionFromExit(exit.GetComponent<AdvancedExit>())
                : BuildSectionFromExit(exit);
                
            if (LevelGenerator.IsSectionValid(candidate.Bounds, Bounds))
            {
                candidate.Initialize(LevelGenerator, order);
            }
            else
            {
                Destroy(candidate.gameObject);
                PlaceDeadEnd(exit);
            }
        }

        protected void PlaceDeadEnd(Transform exit) => Instantiate(LevelGenerator.DeadEnds.PickOne(), exit).Initialize(LevelGenerator);

        protected bool IsAdvancedExit(Transform exit) => exit.GetComponent<AdvancedExit>() != null;

        protected Section BuildSectionFromExit(Transform exit) => Instantiate(LevelGenerator.PickSectionWithTag(CreatesTags), exit).GetComponent<Section>();

        protected Section BuildSectionFromExit(AdvancedExit exit) => Instantiate(LevelGenerator.PickSectionWithTag(exit.CreatesTags), exit.transform).GetComponent<Section>();
    }
}