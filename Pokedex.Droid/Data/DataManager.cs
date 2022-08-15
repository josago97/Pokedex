using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;

namespace Pokedex.Droid.Data
{
    public static class DataManager
    {
        private const string IMAGES_FOLDER = "PokemonImages";

        public static IReadOnlyList<Pokemon> Pokemons { get; private set; }

        public static async Task InitAsync(Context context)
        {
            using Stream stream = context.Assets.Open("pokemon.json");
            Pokemon[] pokemons = JsonSerializer.Deserialize<Pokemon[]>(stream);

            IEnumerable<Task> tasks = pokemons.Select(async p =>
            {
                using Stream image = context.Assets.Open($"{IMAGES_FOLDER}/{p.ImagePath}");
                p.Image = await BitmapFactory.DecodeStreamAsync(image);
            });

            await Task.WhenAll(tasks);

            Pokemons = pokemons;
        }
    }
}