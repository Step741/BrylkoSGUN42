using UnityEngine;
using Zenject;

namespace Netologia.TowerDefence
{
    public class BarracksBuilding : MonoBehaviour
    {
        private DiContainer _container;
        private float _nextSpawnTime;
        private int _activeArchersCount = 0;

        [Header("Настройки спавна")]
        [SerializeField] private GameObject _archerPrefab;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private float _spawnInterval = 8f;
        [SerializeField] private int _maxArchers = 6;

        [Inject]
        public void Construct(DiContainer container)
        {
            _container = container;
        }

        private void Start()
        {
            _nextSpawnTime = TimeManager.Time + _spawnInterval;
        }

        private void Update()
        {
            if (!TimeManager.IsGame) return;

            //Проверяет время и текущее количество активных лучников
            if (TimeManager.Time >= _nextSpawnTime)
            {
                if (_activeArchersCount < _maxArchers)
                {
                    SpawnArcher();
                }

                _nextSpawnTime = TimeManager.Time + _spawnInterval;
            }
        }

        private void SpawnArcher()
        {
            if (_archerPrefab != null && _spawnPoint != null)
            {
                ArcherUnit archer = _container.InstantiatePrefabForComponent<ArcherUnit>(
                    _archerPrefab,
                    _spawnPoint.position,
                    _spawnPoint.rotation,
                    null
                );

                archer.Initialize(this);

                _activeArchersCount++;
            }
        }

        public void OnArcherDestroyed()
        {
            _activeArchersCount--;
            if (_activeArchersCount < 0) _activeArchersCount = 0;
        }
    }
}
