using System;
using System.Diagnostics;


namespace ConsoleDemo
{
    using ServiceStack.Text;

    class Program
    {
        static void Main(string[] args)
        {
            var appHost = new AppHost();
            appHost.Init();
            appHost.Start("http://*:8088/");
            "ServiceStack SelfHost listening at http://localhost:8088 ".Print();
            Process.Start("http://localhost:8088/");

            Console.ReadLine();
        }
    }
}
