using Netologia.Systems;
using Netologia.TowerDefence.Settings;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Netologia.TowerDefence
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class ArcherUnit : MonoBehaviour
    {
        private ProjectileSystem _projectileSystem;
        private Constants _constants;
        private BarracksBuilding _myBarracks;

        private NavMeshAgent _agent;
        private Unit _targetEnemy;
        private float _reloadTimer;

        [Header("Настройки движения")]
        [SerializeField, Range(1f, 20f), Tooltip("Скорость бега лучника")]
        private float _moveSpeed = 6f;

        [Header("Настройки боя")]
        [SerializeField] private float _attackRange = 5f;
        [SerializeField] private float _attackDelay = 1f;
        [SerializeField] private float _damage = 5f;
        [SerializeField] private ElementalType _attackElemental = ElementalType.Physic;

        [Header("Ссылки на префабы")]
        [SerializeField] private Projectile _projectilePrefab; //Ссылка на префаб снаряда для пула
        [SerializeField] private Transform _firePoint;

        [Header("Настройки сканирования")]
        [SerializeField] private float _scanRadius = 15f;
        [SerializeField] private LayerMask _enemyLayer;

        [Inject]
        public void Construct(ProjectileSystem projectileSystem, Constants constants)
        {
            _projectileSystem = projectileSystem;
            _constants = constants;
        }

        //Метод для связи с казармой сразу после спавна через Zenject
        public void Initialize(BarracksBuilding barracks)
        {
            _myBarracks = barracks;
        }

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();

            _agent.stoppingDistance = _attackRange - 0.5f;

            //Применяет скорость к NavMeshAgent
            _agent.speed = _moveSpeed;
        }

        private void Update()
        {
            if (!TimeManager.IsGame) return;

            FindNearestEnemy();

            if (_reloadTimer > 0)
            {
                _reloadTimer -= Time.deltaTime;
            }

            if (_targetEnemy != null)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, _targetEnemy.transform.position);

                //Бежит к противнику на расстояние выстрела
                _agent.SetDestination(_targetEnemy.transform.position);
                FaceTarget();

                if (distanceToEnemy <= _attackRange && _reloadTimer <= 0f)
                {
                    Attack();
                }
            }
            else
            {
                //Если противников нет, стоит на месте
                if (_agent.hasPath)
                {
                    _agent.ResetPath();
                }
            }
        }

        private void FindNearestEnemy()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _scanRadius, _enemyLayer);
            float shortestDistance = Mathf.Infinity;
            Unit nearestEnemy = null;

            foreach (var collider in hitColliders)
            {
                Unit enemy = collider.GetComponent<Unit>();

                if (enemy == null || enemy.CurrentHealth <= 0) continue;

                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = enemy;
                }
            }

            _targetEnemy = nearestEnemy;
        }

        private void Attack()
        {
            _reloadTimer = _attackDelay;

            if (_projectilePrefab != null && _firePoint != null && _targetEnemy != null)
            {
                //Получает снаряд из пула
                var projectile = _projectileSystem[_projectilePrefab].Get;

                projectile.PrepareData(_firePoint.position, _targetEnemy, _damage, _attackElemental);
            }
        }

        private void FaceTarget()
        {
            Vector3 direction = (_targetEnemy.transform.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            }
        }

        private void OnDestroy()
        {
            //Посылает сигнал в казарму, чтобы освободить место в лимите из 6 лучников.
            if (_myBarracks != null)
            {
                _myBarracks.OnArcherDestroyed();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
}
