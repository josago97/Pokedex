using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Pokedex.Droid.ActivityModels;
using Pokedex.Droid.Data;
using Sharplus.Droid.Adapters;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Pokedex.Droid.Activities
{
    [Activity(Label = "@string/app_name", ScreenOrientation = ScreenOrientation.Portrait)]
    public class PokemonListActivity : BaseActivity
    {
        private ProgressBar _progressBar;
        private TextView _noResultsTextView;
        private GridView _pokemonGridView;
        private ListAdapter<Pokemon> _listAdapter;
        private string _lastQuery;
        private CancellationTokenSource _searchCancellationTokenSource;

        protected override int Layout => Resource.Layout.pokemon_list_activity;
        protected override bool CanNavigateUp => false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            _noResultsTextView = FindViewById<TextView>(Resource.Id.noResultsText);
            _pokemonGridView = FindViewById<GridView>(Resource.Id.pokemonList);

            _listAdapter = new ListAdapter<Pokemon>(this);
            _pokemonGridView.Adapter = _listAdapter;
            UpdatePokemonList(DataManager.Pokemons);
            _progressBar.Visibility = ViewStates.Gone;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.pokemon_list_menu_activity, menu);

            IMenuItem menuItem = menu.FindItem(Resource.Id.searchList);
            SearchView searchView = (SearchView)menuItem.ActionView;
            //searchView.OnActionViewExpanded();
            searchView.QueryHint = $"{Resources.GetString(Resource.String.enter_pokemon_name)}...";
            searchView.QueryTextChange += OnQueryTextChanged;

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.searchImage)
            {
                StartActivity(typeof(SearchActivity));
            }

            return base.OnOptionsItemSelected(item);
        }

        private void UpdatePokemonList(IEnumerable<Pokemon> pokemons)
        {
            PokemonSlot[] slots = pokemons.Select(p => new PokemonSlot(p)).ToArray();

            if (slots.Length == 0)
            {
                _noResultsTextView.Visibility = ViewStates.Visible;
                _pokemonGridView.Visibility = ViewStates.Gone;
            }
            else
            {
                _listAdapter.Update(slots);

                _noResultsTextView.Visibility = ViewStates.Gone;
                _pokemonGridView.Visibility = ViewStates.Visible;
            }
        }

        private async void OnQueryTextChanged(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            string query = e.NewText.Trim();

            if (_lastQuery != query)
            {
                _lastQuery = query;
                _searchCancellationTokenSource?.Cancel();
                _searchCancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = _searchCancellationTokenSource.Token;

                _progressBar.Visibility = ViewStates.Visible;
                _noResultsTextView.Visibility = ViewStates.Gone;
                _pokemonGridView.Visibility = ViewStates.Gone;

                IEnumerable<Pokemon> pokemons = await Task.Run(() => SearchPokemons(query));

                if (!cancellationToken.IsCancellationRequested)
                {
                    _progressBar.Visibility = ViewStates.Gone;

                    UpdatePokemonList(pokemons);
                }
            }
        }

        private IEnumerable<Pokemon> SearchPokemons(string query)
        {
            IEnumerable<Pokemon> result;

            if (string.IsNullOrEmpty(query))
                result = DataManager.Pokemons;
            else
                result = DataManager.Pokemons
                    .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

            return result;
        }
    }
}