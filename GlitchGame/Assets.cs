using System.Collections.Generic;
using System.IO;
using SFML.Graphics;

namespace GlitchGame
{
    public static class Assets
    {
        private static readonly Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();

        public static string BaseLocation = "Data/";

        public static Texture LoadTexture(string name)
        {
            Texture texture;

            if (Textures.TryGetValue(name, out texture))
                return texture;

            texture = new Texture(Path.Combine(BaseLocation, name));
            //texture.Smooth = true;
            Textures.Add(name, texture);

            return texture;
        }
    }
}
