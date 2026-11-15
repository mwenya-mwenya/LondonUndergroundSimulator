📘 London Underground Simulator – README  

🚇 Overview  
London Underground Simulator is a custom-built C#/.NET 8 simulation engine that models train movement, dwell behaviour, delays, geometry interpolation, and real-time arrival predictions along a track.
This project was designed as a portfolio-quality demonstration of clean architecture, deterministic simulation logic, and comprehensive automated testing.
The engine updates train position, rotation, carriage geometry, dwell timing, and arrival timestamps in real time, using a modular and testable architecture.

[LondonUndergroundSimulatorDemo.webm](https://github.com/user-attachments/assets/2d44c530-c364-4b53-adc4-83ddde487708)

# 🚇 Project Evolution

The London Underground Simulator began as a simple WinUI train-rendering experiment and gradually evolved into a machine-learning-enabled transport simulation platform.

---

# 🎨 Phase 1: Visual Simulator

### 🎯 Objective
Build a visual representation of trains moving across a map.

### ✅ Features Implemented

- 🚆 WinUI desktop application
- 🎞️ Train rendering and animations
- 🗺️ Underground map visualisation
- ⏱️ Real-time train movement between stations
- ⚙️ Configurable simulation speed controls
- 📍 Early route and station models

### 🧠 Skills Learned

- C#
- WinUI 3
- XAML
- Data Binding
- Animation techniques
- Object-Oriented Programming (OOP)

---

# 🚉 Phase 2: Operational Railway Simulation

### 🎯 Objective
Transform the visual demo into a functioning transport simulation.

### ✅ Features Implemented

- 🏙️ Station network modelling
- 🚇 Underground line definitions
- 🚆 Multi-train support
- 👥 Passenger generation system
- 🚪 Passenger boarding and alighting
- 📦 Train capacity management
- 🗺️ Journey planning
- 🔀 Interchange stations
- 📅 Service schedules
- 🌅 Time-of-day demand modelling
- 🚦 Service activation and operational rules

### 🧠 Skills Learned

- Domain modelling
- Simulation programming
- State management
- Data structures and collections
- Route planning algorithms
- Event-driven programming
- Real-world transport operations modelling

---

# 🏗️ Phase 3: Software Engineering & Architecture

### 🎯 Objective
Refactor the simulator into a scalable and maintainable software platform.

### ✅ Features Implemented

- 🏛️ MVVM architecture
- 🔌 Dependency Injection
- 🧩 Service-oriented architecture
- 🔄 Simulation host abstraction
- 📐 Interface-driven design
- 📝 Centralised logging
- 🔍 Diagnostics and monitoring
- 📊 Simulation analytics dashboard
- 🗃️ Modular subsystems
- 💾 User data persistence

### 🧠 Skills Learned

- SOLID Principles
- Software Architecture
- Design Patterns
- Dependency Injection
- Separation of Concerns
- Advanced Debugging
- Application Scalability

---

# 🤖 Phase 4: AI & Machine Learning Platform

### 🎯 Objective
Enable the simulator to learn from operational data and make predictions.

### ✅ Features Implemented

- 📈 Simulation data collection pipeline
- 📄 CSV export infrastructure
- 🐍 Python machine learning integration
- ⚡ gRPC communication layer
- 🧠 Inference engine
- 🔮 Train frequency prediction models
- 🏋️ Automated model training
- ❤️ Model health monitoring
- 📋 Training status management
- 🔄 Manual retraining workflows
- 📊 Prediction analytics dashboard
- 📦 Model lifecycle management

### 🧠 Skills Learned

- Machine Learning fundamentals
- Python and .NET integration
- gRPC services
- Data Engineering
- Feature Engineering
- Model Training Pipelines
- AI-powered Simulation Systems

---

# 🛠️ Current Technology Stack

```text
🚇 Frontend
├── WinUI 3
├── XAML
└── MVVM

⚙️ Core Simulation
├── .NET
├── C#
├── OOP
├── Event-Driven Architecture
└── Service-Based Design

🤖 Machine Learning
├── Python
├── Gradient Boosting Models
├── CSV Data Pipelines
└── Model Retraining

🔗 Communication
└── gRPC

💾 Data Storage
├── JSON
├── CSV
└── Local User Data
```

### Folder Structure

# 📁 Solution Structure

```text
LondonUndergroundSimulator
│
├── 🚇 LondonUndergroundSimulator.UI
│   ├── Views
│   ├── ViewModels
│   ├── Rendering
│   ├── Services
│   └── Colours
│
├── ⚙️ LondonUndergroundSimulator.Engine
│   ├── Models
│   ├── Services
│   ├── Interfaces
│   ├── DelayServices
│   ├── Logging
│   └── SimulationData
│
├── 🏗️ LondonUndergroundSimulator.App
│   ├── Services
│   ├── Interfaces
│   └── Models
│
├── 🔧 LondonUndergroundSimulator.Infrastructure
│   ├── Interfaces
│   └── Services
│
├── 📊 DataPipeline
│   ├── Aggregators
│   ├── Readers
│   ├── Export
│   ├── Models
│   └── Services
│
├── ▶️ DataPipelineRunner
│   └── Services
│
├── 🤖 MLInferenceEngine
│   ├── Proto
│   └── Services
│
├── 🧠 MLInferenceEngine.Abstractions
│   ├── Interfaces
│   └── Models
│
├── 🔮 MLInferenceEngine.Orchestration
│   └── Services
│
├── 🏋️ LondonUndergroundSimulator.Training
│   ├── Data
│   ├── Services
│   └── TrainingModels
│
├── 🐍 PythonInferenceService
│   ├── app
│   ├── generated
│   └── models
│
├── 📈 Data
    ├── lines.json
    ├── trains.json
    ├── simulation_log.csv
    ├── model_status.json
    └── TrainingModels
```
###🚀 Installation & Setup

#📦 Prerequisites
Make sure you have the following installed:
- .NET 8.0 SDK
- Visual Studio 2022 (17.8 or later) with:
- Desktop development with C++ (WinUI requirement)
- .NET Desktop Development
- Windows 10/11 with WinUI 3 support
- (Optional) xUnit Test Runner in Visual Studio

#📥 Clone the Repository
git clone https://github.com/<your-username>/<your-repo>.git
cd <your-repo>

#🛠️ Build the Solution
Using the .NET CLI:
dotnet build
Or in Visual Studio:
- Open the solution .sln
- Select Build → Build Solution

#▶️ Run the Simulator (WinUI App)
From the command line:
dotnet run --project src/YourWinUIProjectName

Or in Visual Studio:
- Set the WinUI project as the Startup Project
- Press F5 to run with debugging
or Ctrl+F5 to run without debugging



