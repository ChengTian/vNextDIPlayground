using System;
using System.Diagnostics;
using Autofac;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Autofac;
using Microsoft.Framework.DependencyInjection.Ninject;
using Ninject;

namespace IntegrationWithKnownContainers
{
    public class Program
    {
        public void Main(string[] args)
        {
            AutofacScoping();
            NinjectScoping();
            Console.WriteLine("All passed");
            Console.ReadLine();
        }

        private void NinjectScoping()
        {
            var serviceCollection = new ServiceCollection();
            var myService = new MyService();
            serviceCollection.AddScoped<IMyService, MyService>();
            var kernel = new StandardKernel();
            kernel.Populate(serviceCollection);
            var serviceProvider = kernel.Get<IServiceProvider>();

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

        private void AutofacScoping()
        {
            var serviceCollection = new ServiceCollection();
            var myService = new MyService();
            serviceCollection.AddScoped<IMyService, MyService>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(serviceCollection);
            var container = containerBuilder.Build();
            var serviceProvider = container.Resolve<IServiceProvider>();

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
