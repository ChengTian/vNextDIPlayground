﻿using System;
using System.Diagnostics;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace ScopingRules
{
    public class Program
    {
        public void Main(string[] args)
        {
            NestedScopes();
            SingletonOverridesScoped();
            TransientOverridesScoped();
            InstanceOverridesScoped();
            Console.WriteLine("All passed");
            Console.ReadLine();
        }

        private void InstanceOverridesScoped()
        {
            var serviceCollection = new ServiceCollection();
            var myService = new MyService();
            serviceCollection.AddInstance<IMyService>(new MyService() { Name = "New Name" });
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var outerScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            var innerScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            var outerScope = outerScopeFactory.CreateScope();
            var innerScope = innerScopeFactory.CreateScope();
            using (outerScope)
            {
                var outerService = outerScope.ServiceProvider.GetService<IMyService>();
                using (innerScope)
                {
                    var innerService = innerScope.ServiceProvider.GetService<IMyService>();
                    Debug.Assert(string.Equals(innerService.Name, "New Name"));
                    Debug.Assert(string.Equals(outerService.Name, "New Name"));
                    Debug.Assert(ReferenceEquals(innerService, outerService));
                }
            }
        }

        private void TransientOverridesScoped()
        {
            var serviceCollection = new ServiceCollection();
            var myService = new MyService();
            serviceCollection.AddTransient<IMyService, MyService>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var scopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var service1 = scope.ServiceProvider.GetService<IMyService>();
                var service2 = scope.ServiceProvider.GetService<IMyService>();
                Debug.Assert(!ReferenceEquals(service1, service2));
            }
        }

        private void SingletonOverridesScoped()
        {
            var serviceCollection = new ServiceCollection();
            var myService = new MyService();
            serviceCollection.AddSingleton<IMyService, MyService>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var outerScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            var innerScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            var outerScope = outerScopeFactory.CreateScope();
            var innerScope = innerScopeFactory.CreateScope();
            using (outerScope)
            {
                var outerService = outerScope.ServiceProvider.GetService<IMyService>();
                using (innerScope)
                {
                    var innerService = innerScope.ServiceProvider.GetService<IMyService>();
                    Debug.Assert(ReferenceEquals(innerService, outerService));
                }
            }
        }

        private void NestedScopes()
        {
            var serviceCollection = new ServiceCollection();
            var myService = new MyService();
            serviceCollection.AddScoped<IMyService, MyService>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var outerScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            var innerScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            var outerScope = outerScopeFactory.CreateScope();
            var innerScope = innerScopeFactory.CreateScope();
            using (outerScope)
            {
                var outerService1 = outerScope.ServiceProvider.GetService<IMyService>();
                var outerService2 = outerScope.ServiceProvider.GetService<IMyService>();
                Debug.Assert(outerService1 == outerService2);

                using (innerScope)
                {
                    var innerService1 = innerScope.ServiceProvider.GetService<IMyService>();
                    var innerService2 = innerScope.ServiceProvider.GetService<IMyService>();
                    Debug.Assert(ReferenceEquals(innerService1, innerService2));
                    Debug.Assert(!ReferenceEquals(innerService1, outerService1));
                }
            }
        }
    }

    public interface IMyService
    {
        string Name { get; set; }
    }

    public class MyService : IMyService
    {
        public MyService()
        {
            Name = typeof(MyService).FullName;
        }

        public string Name { get; set; }

        public bool Disposed { get; set; }
    }
}
