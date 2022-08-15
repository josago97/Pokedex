using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Pokedex.Droid.Data;
using Sharplus.System;

namespace Pokedex.Droid.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : BaseActivity
    {
        protected override int Layout => Resource.Layout.activity_main;
        protected override bool CanNavigateUp => false;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            EnvironmentUtils.LoadVariables(Assets.Open("Environment.env"));
            await DataManager.InitAsync(this);
            
            StartActivity(typeof(PokemonListActivity));
            Finish();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}