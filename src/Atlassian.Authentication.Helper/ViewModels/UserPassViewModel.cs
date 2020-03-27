using System;
using System.Collections.Generic;
using System.Reactive;
using ReactiveUI;
using Itofinity.Bitbucket.Authentication;
using Itofinity.Bitbucket.Authentication.BasicAuth;
using Itofinity.Bitbucket.Authentication.Auth;
using Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager;

namespace Atlassian.Authentication.Helper.ViewModels
{
    public class UserPassViewModel : AbstractAuthViewModel
    {
        public UserPassViewModel(string hostUrl, ITrace trace, IHttpClientFactory httpClientFactory, ISettings settings) : base(hostUrl)
        {
            var authenticator = new BasicAuthAuthenticator(trace, httpClientFactory);

            LoginCommand = ReactiveCommand.Create<object>(async param =>
            {
                var scopes = BitbucketConstants.BitbucketCredentialScopes;
                // TODO validate credentials
                var result = await authenticator.AcquireTokenAsync(
                    settings.RemoteUri, scopes, 
                    new BaseAuthCredential(_username, _password));

                if (result.Type == AuthenticationResultType.Success)
                {
                    trace.WriteLine($"Token acquisition for '{settings.RemoteUri}' succeeded");
                    
                    _output.Add("username", result.Token.UserName);
                    _output.Add("password", result.Token.Password);
                    _output.Add("scheme", result.Token.Scheme);
                    _output.Add("authentication", result.Token.Scheme);

                    Success = true;
                }
                else if (result.Type == AuthenticationResultType.TwoFactor)
                {
                    trace.WriteLine($"Token acquisition for '{settings.RemoteUri}' failed");
                    _output.Add("authentication", "2fa");
                    Success = false;
                }
                else
                {
                    trace.WriteLine($"Token acquisition for '{settings.RemoteUri}' failed");
                    Success = false;
                }

                Exit();
            });

            CancelCommand = ReactiveCommand.Create<object>(param =>
            {
                Success = false;
                Exit();
            });
        }

        private string _username;

        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        private string _password;

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public ReactiveCommand<object, Unit> LoginCommand { get; }

        public ReactiveCommand<object, Unit> CancelCommand { get; }
    }
}