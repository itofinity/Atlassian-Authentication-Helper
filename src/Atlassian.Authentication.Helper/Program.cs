using System.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Authentication.Helper.ViewModels;
using Atlassian.Authentication.Helper.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using McMaster.Extensions.CommandLineUtils;
using Itofinity.Bitbucket.Authentication.BasicAuth;
using Itofinity.Bitbucket.Authentication.OAuth;
using Itofinity.Bitbucket.Authentication;
using Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager;

namespace Atlassian.Authentication.Helper
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => BuildAvaloniaApp().Start(AppMain, args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UseReactiveUI()
                .UsePlatformDetect()
                .LogToDebug();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Avalonia.Application app, string[] args)
        {
            var cla = new CommandLineApplication();
            cla.HelpOption();
            var optionPromptType = cla.Option("-p|--prompt <USERPASS,AUTHCODE>", "The prompt type",
            CommandOptionType.SingleValue);
            var optionHostUrl = cla.Option("-h|--host <HOST_URL>", "The URL of the target host, displayed to the user and used to determine host type",
            CommandOptionType.SingleValue);
            var optionProxyUrl = cla.Option("--proxy <PROXY_URL>", "The URL of the proxy",
            CommandOptionType.SingleValue);
            var optionVerifySsl = cla.Option("--verifyssl <TRUE_FALSE>", "Whether SSL shoudl be verified or not",
            CommandOptionType.SingleValue);
            var optionOAuthConsumerKey = cla.Option("--oauthkey <KEY>", "The OAuth Consumer Key to use during OAuth flows",
            CommandOptionType.SingleValue);
            var optionOAuthConsumerSecret = cla.Option("--oauthsecret <SECRET>", "The OAuth Consumer Secret to use during OAuth flows",
            CommandOptionType.SingleValue);
            var optionTrace = cla.Option("--trace <TRUE_FALSE_FILEPATH>", "False for no logging, True to log to console, Filepath to log to file",
            CommandOptionType.SingleValue);

            cla.OnExecute(() =>
                    {
                        var prompt = optionPromptType.HasValue()
                            ? optionPromptType.Value()
                            : "userpass";

                        var hostUrl = optionHostUrl.HasValue()
                            ? optionHostUrl.Value()
                            : BitbucketConstants.BitbucketBaseUrl; // default to https://bitbucket.org
                        var proxyUrl = optionProxyUrl.HasValue()
                            ? optionProxyUrl.Value()
                            : null; // default to no proxy
                        var verifySsl = optionVerifySsl.HasValue()
                            ? optionVerifySsl.Value()
                            : "true"; // default to true
                        var oAuthConsumerKey = optionOAuthConsumerKey.HasValue()
                            ? optionOAuthConsumerKey.Value()
                            : null; // default to no key
                        var oAuthConsumerSecret = optionOAuthConsumerSecret.HasValue()
                            ? optionOAuthConsumerSecret.Value()
                            : null; // default to no secret
                        var traceConfig = optionTrace.HasValue()
                            ? optionTrace.Value()
                            : null; // default to no logging  

                        var streams = new StandardStreams();

                        using(var trace = new Itofinity.Bitbucket.Authentication.Helpers.Microsoft.Git.CredentialManager.Trace())
                        using(var settings = new Settings(hostUrl, proxyUrl, verifySsl, oAuthConsumerKey, oAuthConsumerSecret, traceConfig))
                        {
                            var httpClientFactory = new HttpClientFactory(trace, settings, streams);

                            // Enable tracing
                            ConfigureTrace(trace, settings, streams);

                            var viewModel = GetViewModel(prompt, hostUrl, trace, httpClientFactory, settings);
                            var window = GetWindow(viewModel);
                            app.Run(window);

                            if (viewModel.Output != null
                                    && viewModel.Output.Any())
                            {
                                streams.Out.WriteDictionary(viewModel.Output);
                            }
                        }

                        return 0;
                    });

            var result = cla.Execute(args);
        }

        private static void ConfigureTrace(ITrace trace, ISettings settings, StandardStreams streams)
        {
            if (settings.GetTracingEnabled(out string traceValue))
            {
                if (traceValue.IsTruthy()) // Trace to stderr
                {
                    trace.AddListener(streams.Error);
                }
                else if (Path.IsPathRooted(traceValue)) // Trace to a file
                {
                    try
                    {
                        Stream stream = File.Open(traceValue, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                        TextWriter _traceFileWriter = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 4096, leaveOpen: false);

                        trace.AddListener(_traceFileWriter);
                    }
                    catch (Exception ex)
                    {
                        streams.Error.WriteLine($"warning: unable to trace to file '{traceValue}': {ex.Message}");
                    }
                }
                else
                {
                    streams.Error.WriteLine($"warning: unknown value for trace '{traceValue}'");
                }
            }
        }

        private static IAuthViewModel GetViewModel(string prompt, string hostUrl, ITrace trace, IHttpClientFactory httpClientFactory, ISettings settings)
        {
            switch (prompt.ToLower())
            {
                case "authcode":
                    return new AuthCodeViewModel(hostUrl, trace, httpClientFactory, settings);
                case "userpass":
                default:
                    return new UserPassViewModel(hostUrl, trace, httpClientFactory, settings);
            }
        }

        private static Window GetWindow(IAuthViewModel viewModel)
        {
            switch (viewModel.GetType().Name.ToLower())
            {
                case "authcodeviewmodel":
                    return new AuthCodeWindow() { DataContext = viewModel };
                case "userpassviewmodel":
                default:
                    return new UserPassWindow() { DataContext = viewModel };
            }
        }
    }
}
