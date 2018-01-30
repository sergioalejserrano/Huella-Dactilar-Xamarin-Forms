using System;
using Foundation;
using HuellaDactilar.Interfaz;
using HuellaDactilar.iOS.DependenciaServicios;
using LocalAuthentication;
using Security;

[assembly: Xamarin.Forms.Dependency(typeof(DependenciaServicios))]
namespace HuellaDactilar.iOS.DependenciaServicios
{
    public class DependenciaServicios : NSObject, IDependenciaServicios
    {
        LAContextReplyHandler replyHandler;

        public DependenciaServicios()
        {
        }

        public void EscanearHuella()
        {

            try
            {
                var context = new LAContext();
                NSError AuthError;
                var localizedReason = new NSString("Por favor, coloque su dedo en el sensor");
                context.LocalizedFallbackTitle = "";

                bool touchIDSetOnDevice = context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out AuthError);

                //Use el método canEvaluatePolicy para probar si el dispositivo está habilitado para TouchID
                if (touchIDSetOnDevice)
                {
                    replyHandler = new LAContextReplyHandler((success, error) => {
                        //Asegúrese de que se ejecute en el hilo principal, no en el fondo
                        this.InvokeOnMainThread(() => {
                            if (success)
                            {
                                //Se ejecuta cuando la validación de la huella fue exitosa
                                Xamarin.Forms.Application.Current.MainPage.Navigation.PushModalAsync(new MainPage());
                            }
                            else
                            {
                                //Se ejecuta cuando no se pudo validar la huella
                                if (!error.LocalizedDescription.Equals("Canceled by user."))
                                {
                                    if (error.LocalizedDescription.Equals("Application retry limit exceeded."))
                                        Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Atención", "Sólo le restan 2 intentos. ¿Seguro que desea continuar?", "Aceptar");
                                    if (error.LocalizedDescription.Equals("Biometry is locked out."))
                                        Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Atención", "Por su seguridad, su ingreso mediante huella dactilar fue desactivado.", "Aceptar");
                                }
                            }
                        });
                    });
                    //Utilice evaluatePolicy para iniciar la operación de autenticación y mostrar la interfaz de usuario como una vista de alerta
                    context.EvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, localizedReason, replyHandler);
                }
                else
                {
                    if ((LAStatus)Convert.ToInt16(AuthError.Code) == LAStatus.TouchIDNotAvailable)
                        Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Atención", "Ingreso mediante huella dactilar no disponible.", "Aceptar");
                    else if ((LAStatus)Convert.ToInt16(AuthError.Code) == LAStatus.TouchIDNotEnrolled)
                        Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Atención", "No hay huellas dactilares registradas.", "Aceptar");
                    else if ((LAStatus)Convert.ToInt16(AuthError.Code) == LAStatus.BiometryNotAvailable)
                        Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Atención", "Este dispositivo no soporta la autenticación de huella dactilar.", "Aceptar");
                    //if((LAStatus)Convert.ToInt16(AuthError.Code) == LAStatus.BiometryNotEnrolled)
                    //Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Atención", "El usuario no se ha inscrito para la autenticación de huella dactilar.", "Aceptar");
                    else
                        HabilitarTecladoDesbloqueo();
                }
            }
            catch (Exception ex)
            {
                var e = ex;
            }
        }

        void HabilitarTecladoDesbloqueo()
        {
            var secRecord = new SecRecord(SecKind.GenericPassword)
            {
                Label = "Keychain Item",
                Description = "fake item for keychain access",
                Account = "Account",
                Service = "com.bncr.sinpePrueba",
                Comment = "Your comment here",
                ValueData = NSData.FromString("my-secret-password"),
                Generic = NSData.FromString("foo")
            };

            secRecord.AccessControl = new SecAccessControl(SecAccessible.WhenPasscodeSetThisDeviceOnly, SecAccessControlCreateFlags.UserPresence);
            SecKeyChain.Add(secRecord);

            var rec = new SecRecord(SecKind.GenericPassword)
            {
                Service = "com.bncr.sinpePrueba",
                UseOperationPrompt = "Digite su contraseña"
            };
            SecStatusCode res;
            SecKeyChain.QueryAsRecord(rec, out res);
            if (SecStatusCode.Success == res || SecStatusCode.ItemNotFound == res)
            {
                //Validación exitosa del Pin
                SecKeyChain.Remove(rec);
                Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Atención", "Ingreso mediante huella dactilar desbloqueado exitosamente.", "Aceptar");
            }
            else
            {
                //Fallo en validación de Pin
                Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Atención", "Ingreso mediante huella dactilar no disponible", "Aceptar");
            }
        }
    }
}
