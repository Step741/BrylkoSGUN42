using UnityEngine;

namespace DefaultNamespace
{
	
	[RequireComponent(typeof(PositionSaver))]
	public class EditorMover : MonoBehaviour
	{
		private PositionSaver _save;
		private float _currentDelay;

        //todo comment: Что произойдёт, если _delay > _duration?
        //Запись позиций может не произойти ни разу, поскольку время выполнения закончится раньше задержки.
        [Range(0.2f, 1.0f)]
        [SerializeField]
        private float _delay = 0.5f;
        [SerializeField]
        private float _duration = 5f;

		private void Start()
		{
            _duration = Mathf.Max(0.2f, _duration);
            if (_duration <= _delay)
            {
                _duration = _delay * 5f;
            }
            //todo comment: Почему этот поиск производится здесь, а не в начале метода Update?
            //Чтобы не выполнять поиск каждый кадр, плохо для производительности.
            _save = GetComponent<PositionSaver>();
			_save.Records.Clear();
		}

		private void Update()
		{
			_duration -= Time.deltaTime;
			if (_duration <= 0f)
			{
				enabled = false;
				Debug.Log($"<b>{name}</b> finished", this);
				return;
			}

            //todo comment: Почему не написать (_delay -= Time.deltaTime;) по аналогии с полем _duration?
            //_delay константа интервала, а _currentDelay текущее значение таймера.
            _currentDelay -= Time.deltaTime;
			if (_currentDelay <= 0f)
			{
				_currentDelay = _delay;
				_save.Records.Add(new PositionSaver.Data
				{
					Position = transform.position,
                    //todo comment: Для чего сохраняется значение игрового времени?
                    //Чтобы потом воспроизвести движение с тем же интервалом.
                    Time = Time.time,
				});
			}
		}
	}
}