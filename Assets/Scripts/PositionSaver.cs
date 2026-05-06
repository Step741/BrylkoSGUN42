using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DefaultNamespace
{
	public class PositionSaver : MonoBehaviour
	{
        [Serializable]
        public struct Data
		{
			public Vector3 Position;
			public float Time;
		}

        [ReadOnly]
        [SerializeField]
        [Tooltip("Для создания этого файла воспользуйтесь контекстным меню и выберите 'Создать файл'")]
        private TextAsset _json;

        [field: SerializeField]
        [field: HideInInspector]
		public List<Data> Records { get; private set; }

		private void Awake()
		{
            //todo comment: Что будет, если в теле этого условия не сделать выход из метода?
            //Продолжится выполнение и попытка десериализации приведёт к ошибке, так как _json не задан.
            if (_json == null)
			{
				gameObject.SetActive(false);
				Debug.LogError("Please, create TextAsset and add in field _json");
				return;
			}
			
			JsonUtility.FromJsonOverwrite(_json.text, this);
            //todo comment: Для чего нужна эта проверка (что она позволяет избежать)?
            //Проверка нужна, чтобы избежать ошибки "NullReferenceException" при работе со списком, если он не десериализован из json.
            if (Records == null)
				Records = new List<Data>(10);
		}

		private void OnDrawGizmos()
		{
            //todo comment: Зачем нужны эти проверки (что они позволляют избежать)?
            //Позволяют избежать ошибок при обращении к пустому или неинициализированному списку.
            if (Records == null || Records.Count == 0) return;
			var data = Records;
			var prev = data[0].Position;
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(prev, 0.3f);
            //todo comment: Почему итерация начинается не с нулевого элемента?
            //Используется предыдущий элемент "prev", и для первого элемента нет предыдущего.
            for (int i = 1; i < data.Count; i++)
			{
				var curr = data[i].Position;
				Gizmos.DrawWireSphere(curr, 0.3f);
				Gizmos.DrawLine(prev, curr);
				prev = curr;
			}
		}
		
#if UNITY_EDITOR
		[ContextMenu("Create File")]
		private void CreateFile()
		{
            //todo comment: Что происходит в этой строке?
            //Создается файл на диске по указанному пути.
            var stream = File.Create(Path.Combine(Application.dataPath, "Path.txt"));
            //todo comment: Подумайте для чего нужна эта строка? (а потом проверьте догадку, закомментировав)
            //Закрывает поток и высвобождает файл, иначе unity с ним не работает.
            stream.Dispose();
			UnityEditor.AssetDatabase.Refresh();
			//В Unity можно искать объекты по их типу, для этого используется префикс "t:"
			//После нахождения, Юнити возвращает массив гуидов (которые в мета-файлах задаются, например)
			var guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset");
			foreach (var guid in guids)
			{
				//Этой командой можно получить путь к ассету через его гуид
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				//Этой командой можно загрузить сам ассет
				var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                //todo comment: Для чего нужны эти проверки?
                //Позволяют убедиться, что найден корректный ассет и избежать ошибок при загрузке.
                if (asset != null && asset.name == "Path")
				{
					_json = asset;
					UnityEditor.EditorUtility.SetDirty(this);
					UnityEditor.AssetDatabase.SaveAssets();
					UnityEditor.AssetDatabase.Refresh();
                    //todo comment: Почему мы здесь выходим, а не продолжаем итерироваться?
                    //После нахождения нужного ассета дальше поиск не требуется.
                    return;
				}
			}
		}

		private void OnDestroy()
		{
            if (_json == null) return;

            string json = JsonUtility.ToJson(this, true);

            string path = UnityEditor.AssetDatabase.GetAssetPath(_json);

            File.WriteAllText(path, json);

            UnityEditor.AssetDatabase.Refresh();
        }
#endif
	}
}