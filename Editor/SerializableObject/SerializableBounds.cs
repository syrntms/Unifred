using System;
using UnityEngine;

namespace Unifred
{
	[Serializable]
	public struct SerializableBounds
	{
		public SerializableVector3 center;
		public SerializableVector3 size;

		public SerializableBounds(Vector3 rCenter, Vector3 rSize)
		{
			center = rCenter;
			size = rSize;
		}
		
		public override string ToString()
		{
			return String.Format("[center {0}, [size {1}]", center.ToString(), size.ToString());
		}
		
		public static implicit operator Bounds(SerializableBounds rValue)
		{
			return new Bounds(rValue.center, rValue.size);
		}
		
		public static implicit operator SerializableBounds(Bounds rValue)
		{
			return new SerializableBounds(rValue.center, rValue.size);
		}
	}
}
