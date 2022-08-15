using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace Pokedex.Droid.Activities
{
    public abstract class BaseActivity : AppCompatActivity
    {
        protected abstract int Layout { get; }
        protected virtual bool CanNavigateUp => true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Layout);
            SupportActionBar.SetDisplayHomeAsUpEnabled(CanNavigateUp);
        }

        public override bool OnSupportNavigateUp()
        {
            Finish();
            return true;
        }
    }
}