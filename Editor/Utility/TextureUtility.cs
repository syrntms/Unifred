using UnityEngine;

namespace Unifred
{
	public class TextureUtility
	{
		public static Texture2D MakeSolidTexture(Color col)
		{
			int width  = 1;
			int height = 1;
			var pixels = new Color[width * height];
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = col;
			}
			var solid_texture = new Texture2D(width, height);
			solid_texture.SetPixels(pixels);
			solid_texture.wrapMode = TextureWrapMode.Repeat;
			solid_texture.Apply();
			return solid_texture;
		}
		
	}
}
