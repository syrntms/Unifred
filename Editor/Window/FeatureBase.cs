using System.Collections.Generic;
using UnityEngine;

namespace Unifred
{
	public abstract class UnifredFeatureBase<T>
	{
		#region edit here of impl class, if you want new feature
		public virtual string GetDescription() {return string.Empty;}
		public virtual string GetGuiSkinPrefabPath() {return string.Empty;}
		public virtual bool IsMultipleSelect() {return false;}
		public virtual Vector2 GetWindowSize() {return new Vector2(600, 400);}
		public virtual void OnDestroy() {}
		public virtual void OnInit() {}

		public abstract IEnumerable<T> UpdateCandidate(string word);
		public abstract void Draw(string word, IEnumerable<T> obj_list, IEnumerable<IntRange> selected_list, int offset, int count);
		public abstract void Select(string search_word, IEnumerable<T> obj_list);
		public abstract float GetRowHeight();
		#endregion
	}
}
