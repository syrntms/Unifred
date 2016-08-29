using System;
using UnityEngine;

namespace Unifred
{
	[Serializable]
	public struct SerializableColor
	{
		public float r;
		public float g;
		public float b;
		public float a;
		
		public SerializableColor(float rR, float rG, float rB, float rA)
		{
			r = rR;
			g = rG;
			b = rB;
			a = rA;
		}
		
		public override string ToString()
		{
			return String.Format("[{0}, {1}, {2}, {3}]", r, g, b, a);
		}
		
		public static implicit operator Color(SerializableColor rValue)
		{
			return new Color(rValue.r, rValue.g, rValue.b, rValue.a);
		}
		
		public static implicit operator SerializableColor(Color rValue)
		{
			return new SerializableColor(rValue.r, rValue.g, rValue.b, rValue.a);
		}
	}
}
