namespace Atlassian.Authentication.Helper
{
    public interface IAuthWindow
    {
        bool Success{ get; }

        string Response { get; }
    }
}