using System.Collections.Generic;
using System.IO;
using SFML.Audio;
using SFML.Graphics;

namespace GlitchGame
{
    public static class Assets
    {
        private static readonly Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();
        private static readonly Dictionary<string, SoundBuffer> Buffers = new Dictionary<string, SoundBuffer>();
        private static readonly List<Sound> Sounds = new List<Sound>();

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

        public static SoundBuffer LoadSound(string name)
        {
            SoundBuffer soundBuffer;

            if (!Buffers.TryGetValue(name, out soundBuffer))
            {
                soundBuffer = new SoundBuffer(Path.Combine(BaseLocation, name));
                Buffers.Add(name, soundBuffer);
            }

            return soundBuffer;
        }

        public static void PlaySound(string name)
        {
            Sounds.RemoveAll(snd => snd.Status != SoundStatus.Playing);

            var sound = new Sound(LoadSound(name));
            sound.Volume = 10;
            
            sound.Play();
            Sounds.Add(sound);
        }
    }
}
