using Android.Widget;
using Android.Graphics;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using FitCompanion.Droid;

[assembly: ResolutionGroupName(nameof(FitCompanion))]
[assembly: ExportEffect(typeof(StepperColorEffect), nameof(StepperColorEffect))]
namespace FitCompanion.Droid
{
    public class StepperColorEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Element is Stepper element && Control is LinearLayout control)
            {
                control.GetChildAt(0).Background.SetColorFilter(FitCompanion.StepperColorEffect.GetColor(element).ToAndroid(), PorterDuff.Mode.Multiply);
                control.GetChildAt(1).Background.SetColorFilter(FitCompanion.StepperColorEffect.GetColor(element).ToAndroid(), PorterDuff.Mode.Multiply);
            }
        }

        protected override void OnDetached()
        {
            if (Element is Stepper && Control is LinearLayout control)
            {
                control.GetChildAt(0).Background.SetColorFilter(Xamarin.Forms.Color.Gray.ToAndroid(), PorterDuff.Mode.Multiply);
                control.GetChildAt(1).Background.SetColorFilter(Xamarin.Forms.Color.Gray.ToAndroid(), PorterDuff.Mode.Multiply);
            }
        }
    }
}