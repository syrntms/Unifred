using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unifred
{
	public abstract class UnifredFeatureBase<T>
	{
		#region edit here of impl class, if you want new feature
		public virtual string GetDescription() {return string.Empty;}
		public virtual bool IsMultipleSelect() {return false;}
		public virtual Vector2 GetWindowSize() {return new Vector2(600, 400);}
		public virtual void OnDestroy() {}
		public virtual void OnInit() {}

		public abstract IEnumerable<T> UpdateCandidate(string word);
		public abstract void Draw(string word, T obj, bool is_selected);
		public abstract void Select(string search_word, IEnumerable<T> obj_list);
		public abstract float GetRowHeight();
		#endregion


		protected List<KeyValuePair<string, string>> _GetHistory()
		{
			string encoded = EditorUserSettings.GetConfigValue("UnifredHistory");
			if (string.IsNullOrEmpty(encoded)) {
				return new List<KeyValuePair<string, string>>();
			}
			return SerializeUtility.DeserializeObject<List<KeyValuePair<string, string>>>(encoded);
		}

		protected void _SaveHistory(string key, string searchWord)
		{
			if (string.IsNullOrEmpty(key)
			    || string.IsNullOrEmpty(searchWord)
		    ) {
				return;
			}

			List<KeyValuePair<string, string>> history = _GetHistory();
			var push_pair = new KeyValuePair<string, string>(key, searchWord);
			history.Add(push_pair);
			history.Reverse();
			var unique_history = history.Distinct( new HistoryLogCompare() );

			if (unique_history.Count() > Config.HISTORY_COUNT) {
				unique_history = unique_history.Skip(unique_history.Count() - Config.HISTORY_COUNT);
			}
			string serialized_history = SerializeUtility.SerializeObject<List<KeyValuePair<string, string>>>(unique_history.Reverse().ToList());
			EditorUserSettings.SetConfigValue("UnifredHistory", serialized_history);
		}
	}

	internal class HistoryLogCompare : IEqualityComparer<KeyValuePair<string, string>>
	{
		public bool Equals(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
		{
			return x.Key == y.Key && x.Value == y.Value;
		}
		
		public int GetHashCode(KeyValuePair<string, string> obj)
		{
			return obj.GetHashCode();
		}
	}

}
