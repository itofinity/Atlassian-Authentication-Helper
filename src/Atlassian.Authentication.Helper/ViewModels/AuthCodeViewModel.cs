using System;
using System.Collections.Generic;
using System.Reactive;
using ReactiveUI;
using Itofinity.Bitbucket.Authentication;
using Itofinity.Bitbucket.Authentication.OAuth;
using Itofinity.Bitbucket.Authentication.Auth;
using Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager;

namespace Atlassian.Authentication.Helper.ViewModels
{
    public class AuthCodeViewModel : AbstractAuthViewModel
    {
        public AuthCodeViewModel(string hostUrl, ITrace trace, IHttpClientFactory httpClientFactory, ISettings settings) : base(hostUrl)
        {
            var authenticator = new OAuthAuthenticator(trace, httpClientFactory, settings);

            AuthenticateCommand = ReactiveCommand.Create<object>(async param =>
            {
                
                var scopes = BitbucketConstants.BitbucketCredentialScopes;

                AuthenticationResult result = await authenticator.AcquireTokenAsync(
                    settings.RemoteUri, scopes, 
                    new ExtendedCredential("not used", "anywhere", "at all"));

                if (result.Type == AuthenticationResultType.Success)
                {
                    trace.WriteLine($"Token acquisition for '{settings.RemoteUri}' succeeded");
                    

                    var usernameResult = await authenticator.AquireUserDetailsAsync(settings.RemoteUri, result.Token.Password);
                    if (usernameResult.Type == AuthenticationResultType.Success)
                    {
                        _output.Add("username", usernameResult.Token.UserName);
                        _output.Add("accesstoken", result.Token.Password);
                        _output.Add("refreshtoken", result.RefreshToken.Password);
                        _output.Add("scheme", result.Token.Scheme);
                        _output.Add("authentication", result.Token.Scheme);

                        Success = true;
                    }
                    else
                    {
                        Success = false;
                    }
                    
                }
                else
                {
                    trace.WriteLine($"Token acquisition for '{settings.RemoteUri}' failed");
                    Success = false;
                }

                // TODO OAuth
                // TODO validate credentials
                Exit();
            });

            CancelCommand = ReactiveCommand.Create<object>(param =>
            {
                Success = false;
                Exit();
            });
        }

        public ReactiveCommand<object, Unit> AuthenticateCommand { get; }

        public ReactiveCommand<object, Unit> CancelCommand { get; }

        public string Authcode { get; private set; }
        public string Username { get; private set; }


    }
}