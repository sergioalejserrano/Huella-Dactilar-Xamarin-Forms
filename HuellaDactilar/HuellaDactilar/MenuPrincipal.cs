using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;


namespace HuellaDactilar
{
    public class MenuPrincipal : ContentPage
    {
        public MenuPrincipal()
        {
            // Using the Android Support Library v4
            Label lbl = new Label();
            lbl.Text = "Bienvenido";

            Content = lbl;

        }
    }
}
