using System;
using System.Collections.Generic;


namespace Atlassian.Authentication.Helper.ViewModels
{
    public interface IAuthViewModel
    {
        event EventHandler ExitEvent;

        void Exit();

        Dictionary<string,string> Output { get; }

        bool Success { get; }
    }
}