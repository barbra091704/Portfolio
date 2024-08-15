using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LevelGenerator.Scripts.Exceptions;
using LevelGenerator.Scripts.Helpers;
using LevelGenerator.Scripts.Structure;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace LevelGenerator.Scripts
{
    public class LevelGenerator : NetworkBehaviour
    {
        /// <summary>
        /// LevelGenerator seed
        /// </summary>
        public int Seed;

        /// <summary>
        /// Container for all sections in hierarchy
        /// </summary>
        public NetworkObject SectionContainer;
        private ItemGenerator ItemGenerator;
        public Transform startPosition;
        public LayerMask groundMask;
        public NetworkObject teleporterPrefab;
        public NetworkObject currentTeleporter;
        public bool OverrideTeleporterPos;
        public Vector3 overridePosition;
        private Vector3 raycastDirection;
        public int ExpectedValue;
        public int LevelSize;
        /// <summary>
        /// Maximum allowed distance from the original section
        /// </summary>
        public int MaxAllowedOrder;
        public Section initialSection;

        /// <summary>
        /// Spawnable section prefabs
        /// </summary>
        public Section[] Sections;

        /// <summary>
        /// Spawnable dead ends
        /// </summary>
        public DeadEnd[] DeadEnds;

        /// <summary>
        /// Tags that will be taken into consideration when building the first section
        /// </summary>
        public string[] InitialSectionTags;
        
        /// <summary>
        /// Special section rules, limits and forces the amount of a specific tag
        /// </summary>
        public TagRule[] SpecialRules;

        public List<Section> registeredSections = new();
        
        public NetworkObject Container => SectionContainer != null ? SectionContainer : transform.GetComponent<NetworkObject>();

        public IEnumerable<Collider> RegisteredColliders => registeredSections.SelectMany(s => s.Bounds.Colliders).Union(DeadEndColliders);

        public List<Collider> DeadEndColliders = new();

        protected bool HalfLevelBuilt => registeredSections.Count > LevelSize;

        protected bool LevelBuilt => registeredSections.Count >= LevelSize;

        void Start()
        {
            try
            {
                if (!IsServer) return;

                ItemGenerator = GetComponent<ItemGenerator>();
                SpawnServerRpc();
            }
            catch (IndexOutOfRangeException ex)
            {
                Debug.LogError("Caught an IndexOutOfRangeException: " + ex.Message);
                Cleanup();
                Start();
            }
        }

        [Rpc(SendTo.Server,RequireOwnership = false)]
        public void SpawnServerRpc()
        {
            SpawnTeleporter();
            if (Seed != 0)
                RandomService.SetSeed(Seed);
            else
                Seed = RandomService.Seed;
            
            CheckRuleIntegrity();
            CreateInitialSection();
            DeactivateBounds();
            StartCoroutine(GetAllSpawnPoints());
        }

        private void SpawnTeleporter()
        {
            raycastDirection = new(UnityEngine.Random.Range(-1f,1f), -1f, UnityEngine.Random.Range(-1f,1f));

            if(Physics.Raycast(transform.position, raycastDirection, out RaycastHit hit))
            {
                if (hit.collider.transform != null && hit.collider.gameObject.layer == 3)
                {
                    Debug.DrawLine(transform.position, hit.point, Color.red, 15);

                    Vector3 position = OverrideTeleporterPos ? overridePosition : hit.point; 

                    currentTeleporter = Instantiate(teleporterPrefab.gameObject, position, Quaternion.identity, transform).GetComponent<NetworkObject>();

                    currentTeleporter.Spawn();

                    currentTeleporter.TrySetParent(transform);
                }
            }
        }

        public void Cleanup()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out NetworkObject networkObject) && networkObject.IsSpawned){
                    networkObject.Despawn(true);
                }
            }
            if (currentTeleporter != null && currentTeleporter.IsSpawned)
            {
                currentTeleporter.Despawn(true);
            }
        }

        private IEnumerator GetAllSpawnPoints()
        {
            yield return new WaitForSeconds(1f);
            foreach (Section section in registeredSections)
            {
                ItemGenerator.SpawnPoints.AddRange(section.SpawnPoints);
            }
            for (int i = 0; i < ItemGenerator.SpawnPoints.Count; i++)
            {
                if (ItemGenerator.SpawnPoints[i] == Vector3.zero){
                    ItemGenerator.SpawnPoints.RemoveAt(i);
                }
            }
            ItemGenerator.GenerateItemsUpToExpectedValue();
        }

        protected void CheckRuleIntegrity()
        {
            foreach (var ruleTag in SpecialRules.Select(r => r.Tag))
            {
                if (SpecialRules.Count(r => r.Tag.Equals(ruleTag)) > 1)
                    throw new InvalidRuleDeclarationException();
            }
        }
        protected void CreateInitialSection() { 
            initialSection = Instantiate(PickSectionWithTag(InitialSectionTags), startPosition.position, Quaternion.identity, transform);
            initialSection.Initialize(this, 0);
            StartCoroutine(GenerateNavMesh(initialSection.navMeshSurface));
        }
        IEnumerator GenerateNavMesh(NavMeshSurface navMeshSurface){
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => LevelBuilt == true);
            navMeshSurface.BuildNavMesh();
        }
        public void AddSectionTemplate() => Instantiate(Resources.Load("ShipGeneratorPrefabs/SectionTemplate"), Vector3.zero, Quaternion.identity);
        public void AddDeadEndTemplate() => Instantiate(Resources.Load("ShipGeneratorPrefabs/DeadEndTemplate"), Vector3.zero, Quaternion.identity);

        public bool IsSectionValid(Bounds newSection, Bounds sectionToIgnore) => 
            !RegisteredColliders.Except(sectionToIgnore.Colliders).Any(c => c.bounds.Intersects(newSection.Colliders.First().bounds));

        public void RegisterNewSection(Section newSection)
        {
            registeredSections.Add(newSection);

            if(SpecialRules.Any(r => newSection.Tags.Contains(r.Tag)))
                SpecialRules.First(r => newSection.Tags.Contains(r.Tag)).PlaceRuleSection();

            LevelSize--;
        }
        public void ResetSpecialRules() {
            foreach (var rule in SpecialRules) {
                rule.ResetRule(); // Assuming each TagRule has a method to reset its state
            }
        }
        public void RegisterNewDeadEnd(IEnumerable<Collider> colliders) => DeadEndColliders.AddRange(colliders);

        public Section PickSectionWithTag(string[] tags)
        {
            if (RulesContainTargetTags(tags) && HalfLevelBuilt)
            {
                foreach (var rule in SpecialRules.Where(r => r.NotSatisfied))
                {
                    if (tags.Contains(rule.Tag))
                    {
                        return Sections.Where(x => x.Tags.Contains(rule.Tag)).PickOne();
                    }
                } 
            }

            var pickedTag = PickFromExcludedTags(tags);
            return Sections.Where(x => x.Tags.Contains(pickedTag)).PickOne();
        }

        protected string PickFromExcludedTags(string[] tags)
        {
            var tagsToExclude = SpecialRules.Where(r => r.Completed).Select(rs => rs.Tag);
            return tags.Except(tagsToExclude).PickOne();
        }

        protected bool RulesContainTargetTags(string[] tags) => tags.Intersect(SpecialRules.Where(r => r.NotSatisfied).Select(r => r.Tag)).Any();

        protected void DeactivateBounds()
        {
            foreach (var c in RegisteredColliders)
                c.enabled = false;
        }

    }
}
