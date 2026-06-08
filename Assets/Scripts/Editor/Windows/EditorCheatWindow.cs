using System.Reflection;
using Netologia.Necro.Controllers;
using Netologia.Necro.Datas;
using UnityEditor;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace Netologia.Necro.Editor.Cheates
{
	/// <summary>
	/// Окно, у которого не реализован метод OnGUI
	/// </summary>
	public class EditorCheatWindow : EditorWindow
	{
		//Контролс "OnlyEditor"
		private EditorControls _controls;
		
		[MenuItem("Netologia/Windows/EditorCheatWindow", priority = 1)]
		public static void ShowExample() 
			=> GetWindow<EditorCheatWindow>(false, "Editor Cheat Window", true);
		
		//Вызывается при переходе в плеймод и при открытии редактора
		private void OnEnable()
			//Это событие вызывается при смене состояния игры в редакторе
			=> EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		//Вызывается при выходе из плеймода и при закрытии редактора
		private void OnDisable()
			=> EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

		private void OnPlayModeStateChanged(PlayModeStateChange obj)
		{
			switch (obj)
			{
				//Вход в плеймод (происходит после выхода из EditMode)
				case PlayModeStateChange.EnteredPlayMode:
					//На время игры создаем контролс и подписываемся на его экшены
					_controls = new EditorControls();
					_controls.Cheats.Enable();
					_controls.Cheats.NextTurn.performed += OnNextTurnPerformed;
					_controls.Cheats.Kill.performed += OnKillPerformed;
					break;
				//Выход из плеймода (происходит перед входом в EditMode)
				case PlayModeStateChange.ExitingPlayMode:
					//Для простоты, чтобы не париться с проверками - при выходе очищаем память
					_controls.Disable();
					_controls.Cheats.NextTurn.performed -= OnNextTurnPerformed;
					_controls.Cheats.Kill.performed -= OnKillPerformed;
					_controls.Dispose();
					_controls = null;
					break;
				//Вход в режим редактора
				case PlayModeStateChange.EnteredEditMode:
				//Выход из режима редактора
				case PlayModeStateChange.ExitingEditMode:
					break;
			}
		}
		
		//Чит на смену хода
		private void OnNextTurnPerformed(InputAction.CallbackContext obj)
		{
			if (!FindControllerAndData(out var data))
				return;
			
			//можно напрямую задать data.Event = GameEvent.NewTurn;
			//но так не интересно, поэтому:
			//Получаем тип
			var type = data.GetType();
			//Получаем инфу про свойство (оно публичное не статическое)
			var property = type.GetProperty(nameof(ISharedData.Event));
			//Передаем в свойство новое значение
			property.SetValue(data, GameEvent.NewTurn);
		}

		//Чит на убийство выбранного юнита (не нашего, а противника в статусе PerformMove ли PerformAttack
		private void OnKillPerformed(InputAction.CallbackContext obj)
		{
			if (!FindControllerAndData(out var data))
				return;
			
			//читываем значение из свойства
			var dataType = data.GetType();
			var property = dataType.GetProperty(nameof(ISharedData.Target));
			var target = property.GetValue(data) as Cell;
			if (target == null)
			{
				Debug.LogError("Can't destroy non selected target");
				return;
			}
			if (target.Unit == null)
			{
				Debug.LogError("Can't kill non selected unit");
				return;
			}
			
			Destroy(target.Unit.gameObject);
			target.Unit = null;

			//просто для примера, это не нужно, так как публичный статик 
			var method = typeof(EventExtensions).GetMethod(nameof(EventExtensions.NextTurn), 
				BindingFlags.Static | BindingFlags.Public);
			method.Invoke(typeof(EventExtensions), new object[] { data });
		}

		//здесь происходит отражение BattleController, для получения даты
		private bool FindControllerAndData(out ISharedData data)
		{
			var controller = FindObjectOfType<BattleController>();
			if (controller == null)
			{
				Debug.LogError($"Can't find <b>{nameof(BattleController)}</b>");
				data = default;
				return false;
			}
			
			//по факту, в этом коде единственное место где действительно необходима рефлексия:
			var type = controller.GetType();
			//BindingFlags.Instance - для не статических сущностей | BindingFlags.NonPublic - для не public сущностей
			var field = type.GetField("_data", BindingFlags.Instance | BindingFlags.NonPublic);
			data = field.GetValue(controller) as ISharedData;
			if (data == null)
			{
				Debug.LogError($"Field <b>{nameof(BattleController)}._data</b> is null");
				return false;
			}

			return true;
		}
	}
}