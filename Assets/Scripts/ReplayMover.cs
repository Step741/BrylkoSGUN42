using System;
using UnityEngine;

namespace DefaultNamespace
{
	[RequireComponent(typeof(PositionSaver))]
	public class ReplayMover : MonoBehaviour
	{
		private PositionSaver _save;

		private int _index;
		private PositionSaver.Data _prev;
		private float _duration;

		private void Start()
		{
            //todo comment: зачем нужны эти проверки?
            //Позволяют убедиться, что у объекта есть компонент PositionSaver и что в нём есть данные для воспроизведения. Также позволяют избежать ошибок при обращении к пустому списку.
            if (!TryGetComponent(out _save) || _save.Records.Count == 0)
			{
				Debug.LogError("Records incorrect value", this);
                //todo comment: Для чего выключается этот компонент?
                //Компонент отключается, чтобы остановить его выполнение, так как воспроизведение невозможно без данных.
                enabled = false;
			}
		}

		private void Update()
		{
			var curr = _save.Records[_index];
            //todo comment: Что проверяет это условие (с какой целью)?
            //Проверяется, наступил ли момент времени, когда нужно перейти к следующей записанной позиции. Позволяет воспроизвисти движение как при записи.
            if (Time.time > curr.Time)
			{
				_prev = curr;
				_index++;
                //todo comment: Для чего нужна эта проверка?
                //Позволяетубедиться, что индекс не вышел за пределы списка и останавливает воспроизведение, когда достигнут конец.
                if (_index >= _save.Records.Count)
				{
					enabled = false;
					Debug.Log($"<b>{name}</b> finished", this);
				}
			}
            //todo comment: Для чего производятся эти вычисления (как в дальнейшем они применяются)?
            //Вычисляется коэффициент, который показывает, насколько текущий момент времени находится между предыдущей и текущей точкой. Затем коэффициент используется для плавного перемещения объекта между позициями.
            var delta = (Time.time - _prev.Time) / (curr.Time - _prev.Time);
            //todo comment: Зачем нужна эта проверка?
            //Нужна, чтобы избежать значения NaN, что может привести к некорректной работе и ошибкам с позицией объекта.
            if (float.IsNaN(delta)) delta = 0f;
            //todo comment: Опишите, что происходит в этой строчке так подробно, насколько это возможно
            //Выполняется линейная интерполяция между предыдущей и текущей позицией. В зависимости от значения delta объект перемещается от _prev.Position к curr.Position, создавая плавное движение во времени.
            transform.position = Vector3.Lerp(_prev.Position, curr.Position, delta);
		}
	}
}