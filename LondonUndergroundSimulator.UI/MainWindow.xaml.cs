using LondonUndergroundSimulator.Engine.Data;
using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
using LondonUndergroundSimulator.HostServices;
using LondonUndergroundSimulator.ViewModels;
using LondonUndergroundSimulator.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Windows.Graphics;
using Windows.Graphics;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LondonUndergroundSimulator
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        public MainWindow()
        {

            try
            {
                InitializeComponent();

                IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

                var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);

                int screenWidth = displayArea.WorkArea.Width;
                int screenHeight = displayArea.WorkArea.Height;

                int targetWidth = (int)(screenWidth * 0.9);
                int targetHeight = (int)(screenHeight * 0.9);

                appWindow.Resize(new SizeInt32(targetWidth, targetHeight));

                appWindow.Move(new PointInt32(
                        (screenWidth - targetWidth) / 2,
                        (screenHeight - targetHeight) / 2
                    ));

               
                var presenter = appWindow.Presenter as OverlappedPresenter;
                presenter.IsResizable = false;            
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = true;
                presenter.SetBorderAndTitleBar(true, true);

                var engine = App.Services.GetRequiredService<SimulationEngine>();

                var mainViewModel = App.Services.GetRequiredService<MainViewModel>();

                mainViewModel.AttachEngine(engine);

                MapViewControl.DataContext = mainViewModel;

            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR in MainWindow ctor:");
                Debug.WriteLine(ex.ToString());
                throw;
            }

        }

    }
}
