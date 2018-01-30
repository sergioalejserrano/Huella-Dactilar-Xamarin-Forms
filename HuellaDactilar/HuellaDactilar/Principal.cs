using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;


namespace HuellaDactilar
{
   public class Principal : ContentPage
    {
        ARPage ar = new ARPage();
        public Principal ()
        {
            Button btn = new Button();
            btn.Text = "Huella";
            btn.Clicked += Btn_Clicked;
            Content = btn;
        }

        private async void Btn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(ar);
        }

    }
}
