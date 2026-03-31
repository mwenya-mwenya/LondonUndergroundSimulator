📘 London Underground Simulator – README  

🚇 Overview  
London Underground Simulator is a custom-built C#/.NET 8 simulation engine that models train movement, dwell behaviour, delays, geometry interpolation, and real-time arrival predictions along a track.
This project was designed as a portfolio-quality demonstration of clean architecture, deterministic simulation logic, and comprehensive automated testing.
The engine updates train position, rotation, carriage geometry, dwell timing, and arrival timestamps in real time, using a modular and testable architecture.

![Demo](https://github.com/mwenya-mwenya/LondonUndergroundSimulator/blob/main/LondonUndergroundSimulatorDemo.gif)

✨ Features  

🚆 Train Movement Engine
- Progress-based movement along a track
- Direction-aware interpolation
- Looping behaviour at end-of-line
- Accurate travel time calculation based on distance and speed
⏱️ Timing & Scheduling
- Scheduled arrival time calculation
- Expected arrival time including delays
- Per-station arrival timestamps
- Deterministic clock for testing
🕒 Dwell & Delay Logic
- Automatic dwell at stations
- Configurable dwell duration
- Delay accumulation and cooldown
- Delay reasons tracking
📐 Geometry System
- Smooth position interpolation along track points
- Rotation based on forward vector
- Carriage spacing and angle calculation
- Support for offset and reversed tracks
🧪 Automated Test Suite
- xUnit-based deterministic tests
- Fake engine for time-freezing
- Test helpers for tracks, lines, and trains
- Movement, timing, and arrival timestamp tests

🛠️ Technologies Used
- C# / .NET 8
- xUnit for testing
- Vector2 geometry (System.Numerics)
- Clean, modular architecture
- Visual Studio 2022

Folder Structure

```markdown
```text
LondonUndergroundSimulator
├─ LondonUndergroundSimulator.sln
│
├─ LondonUndergroundSimulator.Engine/
│  ├─ Data/
│  │   ├─ lines.json
│  │   └─ TrainConfig.cs
│  └─ DelayServices/
│  │   └─ SifnalFailureRule.cs
│  ├─ Models/
│  │   ├─ Line.cs
│  │   ├─ Station.cs
│  │   ├─ TimeTableEntry.cs
│  │   ├─ Track.cs
│  │   └─ Train.cs  
│  ├─ Services/
│  │   ├─ FailureRuleEngine.cs
│  │   ├─ MapLoader.cs
│  │   ├─ SimulationEngine.cs
│  │   ├─ TimeTableService.cs
│  │   ├─ TrainDelayService.cs
│  │   ├─ TrainMovementService.cs
│  │   └─ Trains.cs   
│  └─ Interfaces/
│      ├─ IFailure.cs
│      ├─ IFailureRuleEngine.cs
│      ├─ ISimulationContext.cs
│      ├─ ITrainDelayService.cs
│      └─ ITrainMovementService.cs
├─ LondonUndergroundSimulator.Tests/
│  ├─ Movement/
│  ├─ Timing/
│  ├─ TestHelpers/
│  ├─ TestDoubles/
│  └─ TrainMovement_TimestampTests.cs
│
└─ LondonUndergroundSimulator.UI/
   ├─ Assets/
   ├─ Colours/
   ├─ Rendering/
   │  ├─ MapRenderer.cs
   │  └─ TrainRenderer.cs
   ├─ Services/
   │  └─ MapTransformService.cs
   ├─ ViewModels/
   │  ├─ MainViewModel.cs
   │  ├─ MapViewModel.cs
   │  └─ TrainViewModel.cs
   ├─ Views/
   │  ├─ MapView.xaml
   │  ├─ MapView.xaml.cs
   │  ├─ TrainListView.xaml
   │  └─ TrainListView.xaml.cs
   ├─ App.xaml
   ├─ App.xaml.cs
   ├─ MainWindow.xaml
   ├─ MainWindow.xaml.cs
   ├─ LondonUndergroundSimulator.UI.csproj
   └─ app.manifest

🚀 Installation & Setup

📦 Prerequisites
Make sure you have the following installed:
- .NET 8.0 SDK
- Visual Studio 2022 (17.8 or later) with:
- Desktop development with C++ (WinUI requirement)
- .NET Desktop Development
- Windows 10/11 with WinUI 3 support
- (Optional) xUnit Test Runner in Visual Studio

📥 Clone the Repository
git clone https://github.com/<your-username>/<your-repo>.git
cd <your-repo>

🛠️ Build the Solution
Using the .NET CLI:
dotnet build
Or in Visual Studio:
- Open the solution .sln
- Select Build → Build Solution

▶️ Run the Simulator (WinUI App)
From the command line:
dotnet run --project src/YourWinUIProjectName

Or in Visual Studio:
- Set the WinUI project as the Startup Project
- Press F5 to run with debugging
or Ctrl+F5 to run without debugging

🧪 Run the xUnit Test Suite
dotnet test
This will execute all simulation engine tests and display results in the console.
