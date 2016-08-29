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

		public static Texture2D MakeFrameTexture(Color outside, Color inside)
		{
			int width  = 16;
			int height = 16;
			int[] map = {
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
				0, 0, 1, 1, 1, 2, 2, 2, 2, 2, 2, 1, 1, 1, 0, 0,
				0, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 0, 0,
				0, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 0,
				0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 0,
				0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 0,
				0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 0,
				0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 0,
				0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 0,
				0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 0,
				0, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 0,
				0, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 0, 0,
				0, 0, 1, 1, 1, 2, 2, 2, 2, 2, 2, 1, 1, 1, 0, 0,
				0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			};

			var pixels = new Color[width * height];
			for (int x = 0 ; x < width ; ++x) {
				for (int y = 0 ; y < height ; ++y) {
					int index = y * width + x;
					switch (map[index]) {
					case 0:
						pixels[index] = Color.clear;
						break;
					case 1:
						pixels[index] = outside;
						break;
					case 2:
						pixels[index] = inside;
						break;
					default:
						pixels[index] = Color.clear;
						break;
					}
				}
			}

			var solid_texture = new Texture2D(width, height);
			solid_texture.SetPixels(pixels);
			solid_texture.wrapMode = TextureWrapMode.Clamp;
			solid_texture.Apply();
			return solid_texture;
		}

		public static Texture2D MakeTexture(Color[] col, int width, int height)
		{
			var texture = new Texture2D(width, height);
			texture.SetPixels(col);
			texture.wrapMode = TextureWrapMode.Repeat;
			texture.Apply();
			return texture;
		}
	}
}
