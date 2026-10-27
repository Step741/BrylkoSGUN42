using System.Collections;
using Netologia.Systems;
using Netologia.TowerDefence.Settings;
using UnityEngine;
using Zenject;

namespace Netologia.TowerDefence
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(Animator))]
    public class ArcherUnit : MonoBehaviour
    {
        private ProjectileSystem _projectileSystem;
        private Constants _constants;
        private BarracksBuilding _myBarracks;
        private Rigidbody2D _rb;
        private Animator _animator;

        private Unit _targetEnemy;
        private float _reloadTimer;
        private bool _isChasing = true;

        private Coroutine _attackCoroutine;

        [Header("Настройки движения")]
        [SerializeField, Range(1f, 20f), Tooltip("Скорость бега лучника")]
        private float _moveSpeed = 4f;

        [Header("Настройки боя")]
        [SerializeField] private float _attackRange = 3f;
        [SerializeField] private float _attackDelay = 1f;
        [SerializeField] private float _damage = 5f;
        [SerializeField] private ElementalType _attackElemental = ElementalType.Physic;

        [Tooltip("Задержка вылета стрелы в секундах от начала анимации")]
        [SerializeField] private float _arrowSpawnDelay = 0.15f;

        [Header("Ссылки на префабы")]
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private Transform _firePoint;

        [Header("Настройки сканирования и слоев")]
        [SerializeField] private float _scanRadius = 10f;
        [SerializeField] private LayerMask _enemyLayer;
        [SerializeField] private LayerMask _obstacleLayer;

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
            _animator = GetComponent<Animator>();
            _rb.freezeRotation = true;
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

                if (distanceToEnemy <= _attackRange && _reloadTimer <= 0f)
                {
                    Attack();
                }
            }

            bool isMoving = _rb.velocity.sqrMagnitude > 0.01f;
            _animator.SetBool("IsRunning", isMoving);
        }

        private void FixedUpdate()
        {
            if (!TimeManager.IsGame)
            {
                _rb.velocity = Vector2.zero;
                return;
            }

            if (_targetEnemy != null)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, _targetEnemy.transform.position);

                if (_isChasing)
                {
                    if (distanceToEnemy <= _attackRange)
                    {
                        _isChasing = false;
                    }
                }
                else
                {
                    float startChasingThreshold = _attackRange + 0.4f;
                    if (distanceToEnemy > startChasingThreshold)
                    {
                        _isChasing = true;
                    }
                }

                if (_isChasing)
                {
                    MoveWithObstacleAvoidanceFixed();
                }
                else
                {
                    _rb.velocity = Vector2.zero;
                }
            }
            else
            {
                _isChasing = false;
                _rb.velocity = Vector2.zero;
            }
        }

        private void MoveWithObstacleAvoidanceFixed()
        {
            Vector2 targetPos = _targetEnemy.transform.position;
            Vector2 currentPos = transform.position;
            Vector2 desiredDirection = (targetPos - currentPos).normalized;

            Vector2 finalDirection = desiredDirection;

            float checkDistance = 0.8f;
            float startOffset = 0.25f;

            Vector2 raycastStart = currentPos + desiredDirection * startOffset;

            bool hitCenter = Physics2D.Raycast(raycastStart, desiredDirection, checkDistance, _obstacleLayer);

            if (hitCenter)
            {
                Vector2 leftDir = Quaternion.Euler(0, 0, 45) * desiredDirection;
                Vector2 rightDir = Quaternion.Euler(0, 0, -45) * desiredDirection;

                Vector2 raycastStartLeft = currentPos + leftDir * startOffset;
                Vector2 raycastStartRight = currentPos + rightDir * startOffset;

                bool hitLeft = Physics2D.Raycast(raycastStartLeft, leftDir, checkDistance, _obstacleLayer);
                bool hitRight = Physics2D.Raycast(raycastStartRight, rightDir, checkDistance, _obstacleLayer);

                if (!hitLeft) finalDirection = leftDir;
                else if (!hitRight) finalDirection = rightDir;
                else finalDirection = -desiredDirection * 0.5f;
            }

            _rb.velocity = finalDirection * _moveSpeed;

            if (desiredDirection.x != 0)
            {
                float angle = desiredDirection.x > 0 ? 0f : 180f;

                if (Mathf.Abs(transform.rotation.eulerAngles.y - angle) > 1f)
                {
                    transform.rotation = Quaternion.Euler(0, angle, 0);
                }
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
                _animator.SetTrigger("Shoot");

                if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);

                _attackCoroutine = StartCoroutine(SpawnArrowWithDelay(_targetEnemy));
            }
        }

        private IEnumerator SpawnArrowWithDelay(Unit currentTarget)
        {
            yield return new WaitForSeconds(_arrowSpawnDelay);

            if (this != null && _projectileSystem != null && currentTarget != null && currentTarget.CurrentHealth > 0 && _projectilePrefab != null && _firePoint != null)
            {
                var projectile = _projectileSystem[_projectilePrefab].Get;
                projectile.PrepareData(_firePoint.position, currentTarget, _damage, _attackElemental);
            }

            _attackCoroutine = null;
        }

        private void OnDestroy()
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }

            if (_myBarracks != null && _myBarracks.gameObject != null)
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
