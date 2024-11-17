using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using SearchLight.ViewModels;

namespace SearchLight.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            DataContext = new SettingsWindowViewModel();

            // ウィンドウが閉じられる時のイベントをキャンセルしてウィンドウを隠す
            Closing += (s, e) =>
            {
                ((Window)s).Hide();
                e.Cancel = true;
            };

            var nv = this.FindControl<NavigationView>("navigationMenu");
            nv.SelectionChanged += OnNavigationMenuSelectionChanged;
            nv.SelectedItem = nv.MenuItems.ElementAt(0);
        }

        private void OnNavigationMenuSelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (e.SelectedItem is NavigationViewItem nvi)
            {
                /*var smpPage = $"SearchLight.Views.Settings.{nvi.Tag}";
                var pg = Activator.CreateInstance(Type.GetType(smpPage));
                (sender as NavigationView).Content = pg;*/
            }
        }
    }
}