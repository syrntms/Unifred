using System;
using UnityEngine;

namespace Unifred
{
	[Serializable]
	public struct SerializableVector4
	{
		public float x;
		public float y;
		public float z;
		public float w;
		
		public SerializableVector4(float rX, float rY, float rZ, float rW)
		{
			x = rX;
			y = rY;
			z = rZ;
			w = rW;
		}
		
		public override string ToString()
		{
			return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
		}
		
		public static implicit operator Vector4(SerializableVector4 rValue)
		{
			return new Vector4(rValue.x, rValue.y, rValue.z, rValue.w);
		}
		
		public static implicit operator SerializableVector4(Vector4 rValue)
		{
			return new SerializableVector4(rValue.x, rValue.y, rValue.z, rValue.w);
		}
	}
}
