using Netologia.Behaviours;
using Netologia.TowerDefence;
using Netologia.TowerDefence.Behaviors;
using Zenject;

namespace Netologia.Systems
{
	public class TowerSystem : GameObjectPoolContainer<Tower>, Director.IManualUpdate
	{
		private UnitSystem _units;				//injected
		private ProjectileSystem _projectiles;  //injected

        public void ManualUpdate()
        {
            var delta = TimeManager.DeltaTime;
            foreach (var pool in this)
            {
                foreach (var tower in pool)
                {
                    if (!tower.DecrementAttackReload(delta))
                        continue;

                    var position = tower.transform.position;

                    //Поверка на выход из радиуса
                    if (tower.HasTarget && tower.Target != null)
                    {
                        float distanceToTarget = UnityEngine.Vector2.Distance(position, tower.Target.transform.position);

                        if (distanceToTarget > tower.Range)
                        {
                            tower.Target = null;
                        }
                    }

                    if (!tower.HasTarget)
                        tower.Target = _units.FindTarget(position, tower.Range);

                    if (!tower.HasTarget) continue;

                    var projectile = _projectiles[tower.Projectile].Get;
                    projectile.PrepareData(position, tower.Target, tower.Damage, tower.AttackElemental);

                    tower.Attack();
                }
            }
        }

        public void OnDespawnUnit(int unitID)
		{
			foreach (var pair in this)
				foreach (var tower in pair)
					if (tower.TargetID == unitID)
						tower.Target = null;
		}
		
		[Inject]
		private void Construct(UnitSystem units, ProjectileSystem projectiles)
		{
			_units = units;
			_projectiles = projectiles;
		}
	}
}