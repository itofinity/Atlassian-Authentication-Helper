using Atlassian.Authentication.Helper.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace Atlassian.Authentication.Helper.Views
{
    public class AuthCodeWindow : AbstractAuthWindow, IAuthWindow
    {
        public AuthCodeWindow()
        {
            InitializeComponent();
            this.DataContextChanged += (s,e) =>
                {
                    ViewModel.ExitEvent += (s,e) =>
                        {
                            this.Close();
                        };
                };
        }

        public override bool Success => true;

        public override string Response => "authcode=12345";

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public IAuthViewModel ViewModel
        {
            get
            {
                return (IAuthViewModel) this.DataContext;
            }
        }
    }
}