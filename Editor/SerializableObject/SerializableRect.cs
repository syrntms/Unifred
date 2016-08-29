using System;
using UnityEngine;

namespace Unifred
{
	[Serializable]
	public struct SerializableRect
	{
		public float x;
		public float y;
		public float w;
		public float h;

		public SerializableRect(float rX, float rY, float rW, float rH)
		{
			x = rX;
			y = rY;
			w = rW;
			h = rH;
		}
		
		public override string ToString()
		{
			return String.Format("[{0}, {1}, {2}, {3}]", x, y, w, h);
		}
		
		public static implicit operator Rect(SerializableRect rValue)
		{
			return new Rect(rValue.x, rValue.y, rValue.w, rValue.h);
		}
		
		public static implicit operator SerializableRect(Rect rValue)
		{
			return new SerializableRect(rValue.x, rValue.y, rValue.width, rValue.height);
		}
	}
}
