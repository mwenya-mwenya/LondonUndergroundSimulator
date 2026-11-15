using LondonUndergroundSimulator;
using LondonUndergroundSimulator.Engine.Services;
using LondonUndergroundSimulator.HostServices;
using LondonUndergroundSimulator.HostServices.Services;
using LondonUndergroundSimulator.Infrastructure.Interfaces;
using LondonUndergroundSimulator.Infrastructure.Services;
using LondonUndergroundSimulator.Rendering;
using LondonUndergroundSimulator.Services;
using LondonUndergroundSimulator.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using Windows.Storage;

namespace LondonUndergroundSimulator
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }
        private SimulationHost _host;
        private Window _window;

        public App()
        {
            InitializeComponent();
            var serviceRegistration = new SimulatorServiceRegistration();
            var services = new ServiceCollection();
            

            // Similation engine + inference services
            serviceRegistration.AddSimulatorServices(services);

            // UI services
            ConfigureUIServices(services);
            ConfigureViewModels(services);
            
            // Build unified DI container
            Services = services.BuildServiceProvider();
            var filePathService = Services.GetRequiredService<IFilePathService>(); // Ensure file path service is initialized
            filePathService.PackagePathResolver = () => ApplicationData.Current.LocalFolder.Path;
            // Give provider to service registration
            //_host = new SimulationHost(Services.GetRequiredService<PythonInferenceLauncher>());
            _host = Services.GetRequiredService<SimulationHost>();
            
        }

        private void ConfigureUIServices(IServiceCollection services)
        {
            services.AddSingleton<MapRenderer>();
            services.AddSingleton<TrainRenderer>();
            services.AddSingleton<MapTransformService>();
            services.AddSingleton<DrawUITimetable>();
            services.AddSingleton<CreateMLConsole>();
            services.AddSingleton<TimetableService>();
        }

        private void ConfigureViewModels(IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MapViewModel>();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Give provider to host
            await _host.FinaliseAsync();
            _window = new MainWindow();
            _window.Closed += async (_, _) => { await _host.Stop(); };
            _window.Activate();
        }
    }

}