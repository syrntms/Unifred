using System;
using System.Linq;
using UnityEngine;

namespace Unifred
{
	[Serializable]
	public struct SerializableAnimationCurve
	{
		public SerializableKeyframe[] keys;
		public int postWrapMode;
		public int preWrapMode;

		public SerializableAnimationCurve(Keyframe[] rKeys)
		{
			keys = rKeys.Select(rKey => (SerializableKeyframe)rKey) .ToArray();
			postWrapMode = preWrapMode = default(int);
		}
		
		public override string ToString()
		{
			return String.Format("SerializableAnimationCurve");
		}
		
		public static implicit operator AnimationCurve(SerializableAnimationCurve rValue)
		{
			var animationCurve = new AnimationCurve(rValue.keys.Select(key => (Keyframe)key).ToArray());
			animationCurve.preWrapMode = (WrapMode)rValue.preWrapMode;
			animationCurve.postWrapMode = (WrapMode)rValue.postWrapMode;
			return animationCurve;
		}
		
		public static implicit operator SerializableAnimationCurve(AnimationCurve rValue)
		{
			var serializableAnimationCurve = new SerializableAnimationCurve(rValue.keys);
			serializableAnimationCurve.preWrapMode = (int)rValue.preWrapMode;
			serializableAnimationCurve.postWrapMode = (int)rValue.postWrapMode;
			return serializableAnimationCurve;
		}
	}
}
