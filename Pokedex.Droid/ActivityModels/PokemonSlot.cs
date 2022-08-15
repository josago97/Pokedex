using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.Graphics.Drawable;
using Pokedex.Common.Data;
using Sharplus.Droid.Adapters;
using Pokemon = Pokedex.Droid.Data.Pokemon;

namespace Pokedex.Droid.ActivityModels
{
    public class PokemonSlot : Slot<Pokemon>
    {
        private const float BACKGROUND_COLOR_SATURATION_FACTOR = 0.5f;
        private static Dictionary<PokemonType, Color> _pokemonTypeColors;

        static PokemonSlot()
        {
            _pokemonTypeColors = new Dictionary<PokemonType, Color>();

            foreach (PokemonType type in Enum.GetValues(typeof(PokemonType)))
            {
                int argb = Application.Context.Resources.GetIdentifier(Enum.GetName(type.GetType(), type).ToLower(), "color", Application.Context.PackageName); ;
                Color color = new Color(ContextCompat.GetColor(Application.Context, argb));

                _pokemonTypeColors.Add(type, color);
            }
        }

        public override int Layout => Resource.Layout.pokemon_slot;

        public PokemonSlot(Pokemon data) : base(data)
        {
        }

        public override void Show(View view, ViewGroup parent)
        {
            view.FindViewById<TextView>(Resource.Id.number).Text = $"#{Data.Id:000}";
            view.FindViewById<TextView>(Resource.Id.name).Text = Data.Name;
            view.FindViewById<ImageView>(Resource.Id.image).SetImageBitmap(Data.Image);
            view.FindViewById<CardView>(Resource.Id.background).SetCardBackgroundColor(GetColor(Data.Types.First()));

            SetTypeView(view.FindViewById<TextView>(Resource.Id.type1), Data.Types[0]);
            TextView type2TextView = view.FindViewById<TextView>(Resource.Id.type2);

            if (Data.Types.Length > 1)
            {
                type2TextView.Visibility = ViewStates.Visible;
                SetTypeView(type2TextView, Data.Types[1]);
            }
            else
            {
                type2TextView.Visibility = ViewStates.Gone;
            }
        }

        /*public override void Show(View view, ViewGroup parent)
        {
            GridView grid = (GridView)parent;
            int size = grid.ColumnWidth;

            //view.LayoutParameters = new GridView.LayoutParams(size, size);

            view.FindViewById<TextView>(Resource.Id.number).Text = $"#{Data.Id:000}";
            view.FindViewById<TextView>(Resource.Id.name).Text = Data.Name;
            _ = view.FindViewById<Sharplus.Droid.Widgets.ImageView>(Resource.Id.image).SetImageUrlAsync(Data.ImageUrl);
            view.FindViewById<CardView>(Resource.Id.background).SetCardBackgroundColor(GetColor(Data.Types.First()));

            SetTypeView(view.FindViewById<TextView>(Resource.Id.type1), Data.Types[0]);
            TextView type2TextView = view.FindViewById<TextView>(Resource.Id.type2);

            if (Data.Types.Length > 1) 
            {
                type2TextView.Visibility = ViewStates.Visible;
                SetTypeView(type2TextView, Data.Types[1]);
            }
            else
            {
                type2TextView.Visibility = ViewStates.Invisible;
            }
        }*/

        private Color GetColor(PokemonType type)
        {
            Color typeColor = _pokemonTypeColors[type];
            float[] hsv = new float[3];
            Color.ColorToHSV(typeColor, hsv);

            hsv[1] *= BACKGROUND_COLOR_SATURATION_FACTOR;

            return Color.HSVToColor(hsv);
        }

        private void SetTypeView(TextView textView, PokemonType type)
        {
            textView.Text = GetPokemonTypeName(textView.Resources, type);
            LayerDrawable layerDrawable = textView.Background.Mutate() as LayerDrawable;
            Drawable drawable = layerDrawable.FindDrawableByLayerId(Resource.Id.background);
            DrawableCompat.SetTint(drawable, _pokemonTypeColors[type]);
            DrawableCompat.SetTintMode(drawable, PorterDuff.Mode.Multiply);
        }

        private string GetPokemonTypeName(Resources resources, PokemonType type)
        {
            int resourceId = type switch
            {
                PokemonType.Bug => Resource.String.type_bug,
                PokemonType.Dark => Resource.String.type_dark,
                PokemonType.Dragon => Resource.String.type_dragon,
                PokemonType.Electric => Resource.String.type_electric,
                PokemonType.Fairy => Resource.String.type_fairy,
                PokemonType.Fighting => Resource.String.type_fighting,
                PokemonType.Fire => Resource.String.type_fire,
                PokemonType.Flying => Resource.String.type_flying,
                PokemonType.Ghost => Resource.String.type_ghost,
                PokemonType.Grass => Resource.String.type_grass,
                PokemonType.Ground => Resource.String.type_ground,
                PokemonType.Ice => Resource.String.type_ice,
                PokemonType.Normal => Resource.String.type_normal,
                PokemonType.Poison => Resource.String.type_poison,
                PokemonType.Psychic => Resource.String.type_psychic,
                PokemonType.Rock => Resource.String.type_rock,
                PokemonType.Steel => Resource.String.type_steel,
                _ => Resource.String.type_water
            };

            return resources.GetString(resourceId);
        }
    }
}