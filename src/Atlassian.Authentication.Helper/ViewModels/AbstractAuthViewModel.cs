using System;
using System.Collections.Generic;
using Atlassian.Authentication.Helper.ViewModels;
using ReactiveUI;

namespace Atlassian.Authentication.Helper.ViewModels
{
    public abstract class AbstractAuthViewModel : ReactiveObject, IAuthViewModel
    {
        protected Dictionary<string, string> _output = new Dictionary<string, string>();

        public AbstractAuthViewModel(string hostUrl)
        {
            this.HostUrl = hostUrl;
        }

        public Dictionary<string,string> Output {
            get
            {
                return _output;
            }
        }

        public string HostUrl { get; }
        public bool Success { get; protected set; }

        public event EventHandler ExitEvent;

        public void Exit()
        {
            if (ExitEvent != null)
            {
                ExitEvent(this, new EventArgs());
            }
        }
    }
}