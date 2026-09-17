using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Commands;
using Controllers;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Signals;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class MissileCreatorManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables

        #endregion

        #region Serialized Variables
        [SerializeField] private bool isTutorial = true;
        [Header("Spawn Clearance Settings")]
        [SerializeField] private float horizontalClearance = 1.0f;
        [SerializeField] private float verticalClearance = 2.5f;
        [SerializeField] private float spawnRangeX = 5.0f;
        [SerializeField] private int maxPlacementAttempts = 20;
        #endregion

        #region Private Variables
        private MissileLevelData _data;
        private int _levelId;
        private int _index = 0;
        private int _destroyedMissileCount = 0;
        private float _lastPosX = float.MinValue;

        private float _percentageIndex = 0;
        private List<Range> _rangeList;
        private bool _isLevelFailed = false;
        private bool _isLevelCompleted = false;
        private EnumCastCommand _enumCastCommand;
        private int _additionalClusterMissiles = 0;
        private bool _isCreatingContinue = true;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            _data = GetData();
            _rangeList = new List<Range>(); 
            _enumCastCommand = new EnumCastCommand();
        }

        private void Start()
        {
            isTutorial = LevelSignals.Instance.onGetLevelId() == 0;
        }

        public MissileLevelData GetData() => Resources.Load<CD_MissileCreator>("Data/CD_MissileCreator").Data;

        #region Event Subscription

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onLevelFailed += OnLevelFailed;
            CoreGameSignals.Instance.onLevelSuccessful += OnLevelSuccess;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
            MissileSignals.Instance.onMissileDestroyed += OnMissileDestroyed;
            TutorialSignals.Instance.onTutorialSatisfied += OnTutorialSatisfied;
            MissileSignals.Instance.onExploderMissileCreated += OnExploderMissileCreated;
        }

        #endregion

        private IEnumerator InstantiateMissile()
        {
            if (_index >= _data.MissileData[_levelId].MissileCount)
            {
                _isCreatingContinue = false;
                CheckWinCondition();
                yield break;
            }

            int typeIndex = GetMissileType();
            var typeList = _data.MissileData[_levelId].MissileTypeList;

            MissileEnums missileType = typeList[typeIndex];
            if (!isTutorial)
            {
                if (missileType != MissileEnums.Missile_3_Exploder)
                {
                    _index++;
                }
            }

            PoolEnums poolType = _enumCastCommand.EnumToEnum<PoolEnums, MissileEnums>(missileType);
            GameObject missile = PoolSignals.Instance.onGetObject(poolType);

            ClusterMissileCheck(missile);
            SetMissilePosition(missile);
            
            yield return new WaitForSeconds(_data.MissileData[_levelId].MissileCreateOffset);

            if (_index >= _data.MissileData[_levelId].MissileCount)
            {
                _isCreatingContinue = false;
                CheckWinCondition();
                yield break;
            }
            StartCoroutine(InstantiateMissile());
        }

        private void ClusterMissileCheck(GameObject missile)
        {
            if (missile.TryGetComponent<Controllers.Missile.Abilities.ClusterMissileAbility>(out var cluster))
            {
                OnClusterSplit(cluster.ChildCount);
            }
        }

        private void SetMissilePosition(GameObject missile)
        {
            Vector3 missilePos = DetermineSpawnPosition(missile);
            _lastPosX = missilePos.x;
            missile.transform.position = missilePos;
            missile.SetActive(true);
        }

        private Vector3 DetermineSpawnPosition(GameObject missile)
        {
            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                float posX = transform.position.x + Random.Range(-spawnRangeX, spawnRangeX);

                if (_lastPosX > float.MinValue && Mathf.Abs(_lastPosX - posX) <= 0.3f)
                {
                    continue;
                }

                Vector3 candidatePos = new Vector3(posX, transform.position.y, 0f);

                if (!IsLocationOccupied(candidatePos, missile))
                {
                    return candidatePos;
                }
            }

            return FindBestAvailablePosition(missile);
        }

        private bool IsLocationOccupied(Vector3 candidatePosition, GameObject missileToSpawn = null)
        {
            // 1. Direct coordinate check on active living missiles
            var activeMissiles = FindObjectsByType<MissileManager>(FindObjectsSortMode.None);
            for (int i = 0; i < activeMissiles.Length; i++)
            {
                var other = activeMissiles[i];
                if (other == null || !other.gameObject.activeInHierarchy || other.IsDead)
                {
                    continue;
                }

                if (missileToSpawn != null && other.gameObject == missileToSpawn)
                {
                    continue;
                }

                float diffX = Mathf.Abs(other.transform.position.x - candidatePosition.x);
                float diffY = Mathf.Abs(other.transform.position.y - candidatePosition.y);

                if (diffX < horizontalClearance && diffY < verticalClearance)
                {
                    return true;
                }
            }

            // 2. Physics check with OverlapBox to catch any active colliders in the spawn clearance volume
            Physics.SyncTransforms();
            Vector3 boxCenter = candidatePosition - new Vector3(0f, verticalClearance * 0.5f, 0f);
            Vector3 halfExtents = new Vector3(horizontalClearance * 0.5f, verticalClearance * 0.5f, 1f);
            Collider[] colliders = Physics.OverlapBox(boxCenter, halfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);

            for (int i = 0; i < colliders.Length; i++)
            {
                var col = colliders[i];
                if (col == null) continue;

                if (missileToSpawn != null && (col.gameObject == missileToSpawn || col.transform.IsChildOf(missileToSpawn.transform)))
                {
                    continue;
                }

                var otherMissile = col.GetComponentInParent<MissileManager>();
                if (otherMissile != null && otherMissile.gameObject.activeInHierarchy && !otherMissile.IsDead)
                {
                    if (missileToSpawn != null && otherMissile.gameObject == missileToSpawn)
                    {
                        continue;
                    }
                    return true;
                }
            }

            return false;
        }

        private Vector3 FindBestAvailablePosition(GameObject missile)
        {
            var activeMissiles = FindObjectsByType<MissileManager>(FindObjectsSortMode.None);
            float bestX = transform.position.x;
            float maxMinDistance = -1f;

            int samples = 20;
            float step = (spawnRangeX * 2f) / samples;

            for (int i = 0; i <= samples; i++)
            {
                float testX = (transform.position.x - spawnRangeX) + (i * step);
                Vector3 testPos = new Vector3(testX, transform.position.y, 0f);

                float minDistance = float.MaxValue;
                bool missileNearby = false;

                for (int j = 0; j < activeMissiles.Length; j++)
                {
                    var other = activeMissiles[j];
                    if (other == null || !other.gameObject.activeInHierarchy || other.IsDead) continue;
                    if (missile != null && other.gameObject == missile) continue;

                    if (Mathf.Abs(other.transform.position.y - testPos.y) < verticalClearance)
                    {
                        missileNearby = true;
                        float d = Mathf.Abs(other.transform.position.x - testX);
                        if (d < minDistance) minDistance = d;
                    }
                }

                if (!missileNearby)
                {
                    return testPos;
                }

                if (minDistance > maxMinDistance)
                {
                    maxMinDistance = minDistance;
                    bestX = testX;
                }
            }

            return new Vector3(bestX, transform.position.y, 0f);
        }

        private int GetMissileType()
        {
            var typeList = _data.MissileData[_levelId].MissileTypeList;
            int typeCount = typeList != null ? typeList.Count : 0;
            if (typeCount <= 1 || _rangeList.Count == 0)
            {
                return 0;
            }

            int rand = Random.Range(0, 100);

            for (int i = 0; i < typeCount; i++)
            {
                if (i < _rangeList.Count && rand >= _rangeList[i].Start.Value && rand <= _rangeList[i].End.Value)
                {
                    return i;
                }
            }

            return 0;
        }

        private void SetRange()
        {
            _percentageIndex = 0;
            _rangeList.Clear();

            var levelData = _data.MissileData[_levelId];
            var typeList = levelData.MissileTypeList;
            int typeCount = typeList != null ? typeList.Count : 0;
            Debug.Log("type count: " + typeCount);
            if (typeCount == 0)
            {
                return;
            }

            float addedValue = 0f;

            for (int i = 0; i < typeCount; i++)
            {
                float weight = 1f;
                if (levelData.PercentageList != null && i < levelData.PercentageList.Count)
                {
                    weight = levelData.PercentageList[i];
                }
                addedValue += weight;
            }

            if (addedValue <= 0f)
            {
                addedValue = 1f;
            }

            float unitValue = 100f / addedValue;

            for (int i = 0; i < typeCount; i++)
            {
                float weight = 1f;
                if (levelData.PercentageList != null && i < levelData.PercentageList.Count)
                {
                    weight = levelData.PercentageList[i];
                }

                int endValue = (int)(_percentageIndex + unitValue * weight);
                _rangeList.Add(new Range((int)_percentageIndex, endValue));
                _percentageIndex = endValue;
            }
        }

        private void OnPlay()
        {
            _isLevelFailed = false;
            _isLevelCompleted = false;
            int rawLevel = LevelSignals.Instance.onGetLevelId();
            int totalLevels = _data.MissileData != null ? _data.MissileData.Count : 0;
            if (rawLevel < totalLevels)
            {
                _levelId = rawLevel;
            }
            else if (totalLevels > 1)
            {
                _levelId = 1 + ((rawLevel - 1) % (totalLevels - 1));
            }
            else
            {
                _levelId = 0;
            }
            SetRange();
            _isCreatingContinue = true;
            StartCoroutine(InstantiateMissile());
        }

        private void OnClusterSplit(int childCount)
        {
            if (!isTutorial)
            {
                _additionalClusterMissiles += (childCount);
                Debug.Log("cluster +1");
            }
        }

        private void OnMissileDestroyed(float strength = 0f)
        {
            if (!isTutorial)
            {
                ++_destroyedMissileCount;
            }
            Debug.Log("destroyed missile count: " + _destroyedMissileCount + " additional: " + _additionalClusterMissiles + "\n instantiated missile count: " + _index);
            CheckWinCondition();
        }

        private void CheckWinCondition()
        {
            if (_isLevelCompleted || _isLevelFailed || _isCreatingContinue || _destroyedMissileCount == 0)
            {
                return;
            }

            int totalRequired = _data.MissileData[_levelId].MissileCount + _additionalClusterMissiles;
            bool countSatisfied = _destroyedMissileCount >= totalRequired;

            if (countSatisfied)
            {
                _isLevelCompleted = true;

                CoreGameSignals.Instance.onLevelSuccessful?.Invoke();
                AudioSignals.Instance.onPlaySound(AudioSoundEnums.Win);
                GameObject confeti = PoolSignals.Instance.onGetObject(PoolEnums.Confetti);
                if (confeti != null)
                {
                    confeti.transform.position = Vector3.zero;
                    confeti.SetActive(true);
                }
            }
        }

        private bool HasActiveMissiles()
        {
            var missiles = FindObjectsByType<MissileManager>(FindObjectsSortMode.None);
            for (int i = 0; i < missiles.Length; i++)
            {
                if (missiles[i] != null && missiles[i].gameObject.activeInHierarchy && !missiles[i].IsDead)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnExploderMissileCreated()
        {
            ++_index;
            Debug.Log("exploder created, index is increased 1");
        }

        private void OnLevelSuccess()
        {
            _percentageIndex = 0;
            _rangeList.Clear();
            ResetSettings();
        }

        private void OnLevelFailed()
        {
            _isLevelFailed = true;
            _percentageIndex = 0;
            _rangeList.Clear();
            ResetSettings();
        }

        private void ResetSettings()
        {
            _index = 0;
            _destroyedMissileCount = 0;
            _additionalClusterMissiles = 0;
            _isLevelCompleted = false;
            _lastPosX = float.MinValue;
            StopAllCoroutines();
        }

        private void OnRestartLevel()
        {
            _isLevelFailed = false;
            _isLevelCompleted = false;
            ResetSettings();
        }

        private void OnTutorialSatisfied()
        {
            isTutorial = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 left = new Vector3(transform.position.x - spawnRangeX, transform.position.y, 0f);
            Vector3 right = new Vector3(transform.position.x + spawnRangeX, transform.position.y, 0f);
            Gizmos.DrawLine(left, right);

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
            Vector3 center = new Vector3(transform.position.x, transform.position.y - verticalClearance * 0.5f, 0f);
            Vector3 size = new Vector3(spawnRangeX * 2f + horizontalClearance, verticalClearance, 1f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}