using System.Diagnostics;
using System.Text.Json;
using PokeApiNet;
using Pokemon = PokeApiNet.Pokemon;
using PokemonType = PokeApiNet.PokemonType;

namespace Pokedex.DataScraper
{
    public class Program
    {
        private const int GENERATION = 1;
        private const string DATA_FOLDER = "Data";
        private const string FILENAME = "pokemon.json";
        private const string IMAGES_FOLDER = "PokemonImages";

        private static Dictionary<string, Common.Data.PokemonType> _pokemonTypeMap;
        private static HttpClient _httpClient;

        public static async Task Main()
        {
            _httpClient = new HttpClient();

            Stopwatch stopwatch = Stopwatch.StartNew();

            if (Directory.Exists(DATA_FOLDER)) Directory.Delete(DATA_FOLDER, true);

            Common.Data.Pokemon[] pokemonsData = await GetPokemonsDataAsync(GENERATION);

            using FileStream file = File.Create($"{DATA_FOLDER}/{FILENAME}");
            JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };
            JsonSerializer.Serialize(file, pokemonsData, options);

            Console.WriteLine($"Datos extraídos en un tiempo de {stopwatch.Elapsed}");
        }

        private static async Task<Common.Data.Pokemon[]> GetPokemonsDataAsync(int generation)
        {
            PokeApiClient pokeClient = new PokeApiClient();
            Generation firstGeneration = await pokeClient.GetResourceAsync<Generation>(generation);
            List<PokemonSpecies> species = await pokeClient.GetResourceAsync(firstGeneration.PokemonSpecies);
            List<Pokemon> pokemons = await pokeClient.GetResourceAsync(species.Select(s => s.Varieties.First().Pokemon));

            List<Common.Data.Pokemon> pokemonsData = new List<Common.Data.Pokemon>();

            for (int i = 0; i < pokemons.Count; i++)
            {
                Pokemon pokemon = pokemons[i];
                PokemonSpecies specie = species[i];

                Common.Data.Pokemon pokemonData = new Common.Data.Pokemon()
                {
                    Id = pokemon.Id,
                    Name = specie.Names.First(n => n.Language.Name == "en").Name,
                    ImagePath = await DownloadImageAsync(pokemon.Id),
                    Types = pokemon.Types.OrderBy(t => t.Slot).Select(ConvertToPokemonType).ToArray()
                };

                pokemonsData.Add(pokemonData);
            }

            return pokemonsData.OrderBy(p => p.Id).ToArray();
        }

        private static Common.Data.PokemonType ConvertToPokemonType(PokemonType type)
        {
            Common.Data.PokemonType result;

            if (_pokemonTypeMap == null)
            {
                _pokemonTypeMap = Enum.GetValues<Common.Data.PokemonType>()
                    .ToDictionary(t => t.ToString().ToLower());
            }

            if (!_pokemonTypeMap.TryGetValue(type.Type.Name, out result))
                throw new Exception($"Can't find type for {type.Type.Name}");

            return result;
        }

        private static async Task<string> DownloadImageAsync(int pokemonId)
        {
            string url = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/home/{pokemonId}.png";
            byte[] image = await _httpClient.GetByteArrayAsync(url);
            string fileName = Path.GetFileName(url);

            string folder = $"{DATA_FOLDER}/{IMAGES_FOLDER}";
            Directory.CreateDirectory(folder);
            File.WriteAllBytes($"{folder}/{fileName}", image);

            return fileName;
        }
    }
}
