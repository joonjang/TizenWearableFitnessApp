using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FitCompanion.Common
{
    public interface INavigationService 
    {
        ViewModelBase PreviousPageViewModel { get; }
        Task InitializeAsync();
        Task NavigateToAsync<TViewModel>() where TViewModel : ViewModelBase;
        Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : ViewModelBase;
        Task RemoveLastFromBackStackAsync();
        Task RemoveBackStackAsync();
        Task GoBackAsync();
    }
}
