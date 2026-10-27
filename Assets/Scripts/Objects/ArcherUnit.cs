using Netologia.Systems;
using Netologia.TowerDefence.Settings;
using UnityEngine;
using Zenject;

namespace Netologia.TowerDefence
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class ArcherUnit : MonoBehaviour
    {
        private ProjectileSystem _projectileSystem;
        private Constants _constants;
        private BarracksBuilding _myBarracks;
        private Rigidbody2D _rb;

        private Unit _targetEnemy;
        private float _reloadTimer;

        [Header("Настройки движения")]
        [SerializeField, Range(1f, 20f), Tooltip("Скорость бега лучника")]
        private float _moveSpeed = 4f;

        [Header("Настройки боя")]
        [SerializeField] private float _attackRange = 3f;
        [SerializeField] private float _attackDelay = 1f;
        [SerializeField] private float _damage = 5f;
        [SerializeField] private ElementalType _attackElemental = ElementalType.Physic;

        [Header("Ссылки на префабы")]
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private Transform _firePoint;

        [Header("Настройки сканирования и слоев")]
        [SerializeField] private float _scanRadius = 10f;
        [SerializeField] private LayerMask _enemyLayer;
        [SerializeField] private LayerMask _obstacleLayer; // Сюда выбери слои Башен и Тропы

        [Inject]
        public void Construct(ProjectileSystem projectileSystem, Constants constants)
        {
            _projectileSystem = projectileSystem;
            _constants = constants;
        }

        public void Initialize(BarracksBuilding barracks)
        {
            _myBarracks = barracks;
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
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
                float distanceToEnemy = Vector2.Distance(transform.position, _targetEnemy.transform.position);

                if (distanceToEnemy > _attackRange)
                {
                    MoveWithObstacleAvoidance();
                }
                else
                {
                    // Дошли — стоим и атакуем
                    _rb.velocity = Vector2.zero;

                    if (_reloadTimer <= 0f)
                    {
                        Attack();
                    }
                }
            }
            else
            {
                // 1.6: Если противников нет — стоим на месте
                _rb.velocity = Vector2.zero;
            }
        }

        private void MoveWithObstacleAvoidance()
        {
            Vector2 targetPos = _targetEnemy.transform.position;
            Vector2 currentPos = transform.position;
            Vector2 desiredDirection = (targetPos - currentPos).normalized;

            // Алгоритм контекстного обхода препятствий (умный луч):
            // Проверяем путь напрямую, а также два луча под углами 45 градусов влево и вправо
            Vector2 finalDirection = desiredDirection;
            float checkDistance = 0.8f;

            bool hitCenter = Physics2D.Raycast(currentPos, desiredDirection, checkDistance, _obstacleLayer);

            if (hitCenter)
            {
                Vector2 leftDir = Quaternion.Euler(0, 0, 45) * desiredDirection;
                Vector2 rightDir = Quaternion.Euler(0, 0, -45) * desiredDirection;

                bool hitLeft = Physics2D.Raycast(currentPos, leftDir, checkDistance, _obstacleLayer);
                bool hitRight = Physics2D.Raycast(currentPos, rightDir, checkDistance, _obstacleLayer);

                if (!hitLeft) finalDirection = leftDir;
                else if (!hitRight) finalDirection = rightDir;
                else finalDirection = -desiredDirection * 0.5f; // Если зажат со всех сторон — чуть отходим
            }

            // Двигаем физическое тело
            _rb.velocity = finalDirection * _moveSpeed;

            // Поворот спрайта по горизонтали
            if (desiredDirection.x != 0)
            {
                float angle = desiredDirection.x > 0 ? 0f : 180f;
                transform.rotation = Quaternion.Euler(0, angle, 0);
            }
        }

        private void FindNearestEnemy()
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, _scanRadius, _enemyLayer);
            float shortestDistance = Mathf.Infinity;
            Unit nearestEnemy = null;

            foreach (var col in hitColliders)
            {
                Unit enemy = col.GetComponent<Unit>();
                if (enemy == null || enemy.CurrentHealth <= 0) continue;

                float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
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
                var projectile = _projectileSystem[_projectilePrefab].Get;
                projectile.PrepareData(_firePoint.position, _targetEnemy, _damage, _attackElemental);
            }
        }

        private void OnDestroy()
        {
            if (_myBarracks != null)
            {
                _myBarracks.OnArcherDestroyed();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _scanRadius);
        }
    }
}
