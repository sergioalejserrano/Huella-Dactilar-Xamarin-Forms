using System;
using System.Windows.Input;
using HuellaDactilar.Interfaz;
using Xamarin.Forms;

namespace HuellaDactilar.VistaModelo
{
    public class HuellaDactilarViewModel
    {
        public HuellaDactilarViewModel()
        {
            this.commandEscanear = new Command(CommandEscanear);
        }

        ICommand commandEscanear;

        public ICommand EscanearHuella
        {
            get
            {
                return commandEscanear;
            }
        }

        void CommandEscanear(object valor)
        {
            DependencyService.Get<IDependenciaServicios>().EscanearHuella();
        }
    }
}
