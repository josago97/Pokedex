using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using Pokedex.Common.MachineLearning;
using Pokedex.Droid.ActivityModels;
using Pokedex.Droid.Data;
using Pokedex.Droid.Logic;
using Sharplus.Droid.Adapters;

namespace Pokedex.Droid.Activities
{
    [Activity(Label = "@string/search_pokemon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SearchActivity : BaseActivity
    {
        private const double MIN_PROBABILITY = 0.01;
        private const int POKEMON_COUNT = 3;

        private ActivityResultLauncher _searchByCameraActivityResultLauncher;
        private ActivityResultLauncher _searchByFileActivityResultLauncher;

        private ImageView _imageView;
        private ProgressBar _progressBar;
        private TextView _noResultsTextView;
        private ListView _resultsListView;
        private ListAdapter<PokemonSlot> _resultsListAdapter;
        private CancellationTokenSource _searchCancellationTokenSource;
        private WebService _apiService;

        protected override int Layout => Resource.Layout.search_activity;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _imageView = FindViewById<ImageView>(Resource.Id.image);
            FindViewById<Button>(Resource.Id.searchByCamera).Click += OnSearchByCameraButtonClicked;
            FindViewById<Button>(Resource.Id.searchByFile).Click += OnSearchByFileButtonClicked;
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            _noResultsTextView = FindViewById<TextView>(Resource.Id.noResultsText);
            _resultsListView = FindViewById<ListView>(Resource.Id.list);
            _resultsListView.Adapter = _resultsListAdapter = new ListAdapter<PokemonSlot>(this);

            _apiService = new WebService();

            _searchByCameraActivityResultLauncher = RegisterForActivityResult(
                new ActivityResultContracts.StartActivityForResult(),
                new ActivityResultCallback(SearchByCamera));

            _searchByFileActivityResultLauncher = RegisterForActivityResult(
                new ActivityResultContracts.StartActivityForResult(),
                new ActivityResultCallback(SearchByFile));

            _progressBar.Visibility = ViewStates.Gone;
            _noResultsTextView.Visibility = ViewStates.Gone;
            _resultsListView.Visibility = ViewStates.Gone;
        }

        private void OnSearchByCameraButtonClicked(object sender, EventArgs e)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);

            _searchByCameraActivityResultLauncher.Launch(intent);
        }

        private void OnSearchByFileButtonClicked(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("image/*");

            _searchByFileActivityResultLauncher.Launch(intent);
        }

        private async void SearchByCamera(ActivityResult activityResult)
        {
            if (activityResult == null || activityResult.ResultCode != (int)Result.Ok) return;

            using Bitmap image = activityResult.Data.Extras.Get("data") as Bitmap;
            await SearchAsync(image);
        }

        private async void SearchByFile(ActivityResult activityResult)
        {
            if (activityResult == null || activityResult.ResultCode != (int)Result.Ok) return;

            Android.Net.Uri imagePath = activityResult.Data.Data;
            using Stream stream = ContentResolver.OpenInputStream(imagePath);
            using Bitmap image = await BitmapFactory.DecodeStreamAsync(stream);
            await SearchAsync(image);
        }

        private async Task SearchAsync(Bitmap image)
        {
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();

            _imageView.SetImageBitmap(image);
            _progressBar.Visibility = ViewStates.Visible;
            _noResultsTextView.Visibility = ViewStates.Gone;
            _resultsListView.Visibility = ViewStates.Gone;

            using MemoryStream stream = new MemoryStream();
            image.Compress(Bitmap.CompressFormat.Png, 0, stream);

            Pokemon[] pokemons;

            try
            {
                OutputModel result = await _apiService.ClassifyAsync(stream.ToArray(), _searchCancellationTokenSource.Token);
                int[] pokemonsId = SelectBestMatches(result);
                pokemons = pokemonsId.Select(i => DataManager.Pokemons[i]).ToArray();
            }
            catch (Exception e)
            {
                pokemons = new Pokemon[0];
                Toast.MakeText(this, "Server connection error", ToastLength.Long).Show();
            }

            UpdateResultList(pokemons);
            _progressBar.Visibility = ViewStates.Gone;
        }

        private int[] SelectBestMatches(OutputModel classification)
        {
            return classification.Score.Select((s, i) => new { Id = i, Score = s })
                .Where(x => x.Score > MIN_PROBABILITY)
                .OrderByDescending(x => x.Score)
                .Take(POKEMON_COUNT)
                .Select(x => x.Id)
                .ToArray();
        }

        private void UpdateResultList(IEnumerable<Pokemon> pokemons)
        {
            PokemonSlot[] slots = pokemons.Select(p => new PokemonSlot(p)).ToArray();

            if (slots.Length == 0)
            {
                _noResultsTextView.Visibility = ViewStates.Visible;
                _resultsListView.Visibility = ViewStates.Gone;
            }
            else
            {
                _resultsListAdapter.Update(slots);

                _noResultsTextView.Visibility = ViewStates.Gone;
                _resultsListView.Visibility = ViewStates.Visible;
            }
        }
    }
}