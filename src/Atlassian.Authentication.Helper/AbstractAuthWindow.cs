using Avalonia.Controls;

namespace Atlassian.Authentication.Helper
{
    public abstract class AbstractAuthWindow : Window, IAuthWindow
    {
        public abstract bool Success { get; }

        public abstract string Response { get; }
    }
}