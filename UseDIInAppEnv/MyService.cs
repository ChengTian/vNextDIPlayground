using System;

namespace UseDIInAppEnv
{
    public class MyService : IMyService
    {
        public MyService()
        {
            Message = "Default Message";
        }

        public string Message { get; set; }
    }
}