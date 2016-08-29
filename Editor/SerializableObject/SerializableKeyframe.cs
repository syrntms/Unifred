using System;
using UnityEngine;

namespace Unifred
{
	[Serializable]
	public struct SerializableKeyframe
	{
		public float time;
		public float value;
		public float inTangent;
		public float outTangent;
		public int tangentMode;
		
		public SerializableKeyframe(float rTime, float rValue, float rInTangent, float rOutTangent)
		{
			time = rTime;
			value = rValue;
			inTangent = rInTangent;
			outTangent = rOutTangent;
			tangentMode = default(int);
		}
		
		public override string ToString()
		{
			return String.Format("SerializableKeyframe");
		}
		
		public static implicit operator Keyframe(SerializableKeyframe rValue)
		{
			var keyframe = new Keyframe(rValue.time, rValue.value, rValue.inTangent, rValue.outTangent);
			keyframe.tangentMode = rValue.tangentMode;
			return keyframe;
		}
		
		public static implicit operator SerializableKeyframe(Keyframe rValue)
		{
			var serializableKeyframe = new SerializableKeyframe(rValue.time, rValue.value, rValue.inTangent, rValue.outTangent);
			serializableKeyframe.tangentMode = rValue.tangentMode;
			return serializableKeyframe;
		}

	}
}
