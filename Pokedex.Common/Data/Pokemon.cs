namespace Pokedex.Common.Data
{
    public class Pokemon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PokemonType[] Types { get; set; }
        public string ImagePath { get; set; }      
    }
}
