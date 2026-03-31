📘 London Underground Simulator – README
🚇 Overview
London Underground Simulator is a custom-built C#/.NET 8 simulation engine that models train movement, dwell behaviour, delays, geometry interpolation, and real-time arrival predictions along a track.
This project was designed as a portfolio-quality demonstration of clean architecture, deterministic simulation logic, and comprehensive automated testing.
The engine updates train position, rotation, carriage geometry, dwell timing, and arrival timestamps in real time, using a modular and testable architecture.
[Watch the demo](https://github.com/mwenya-mwenya/LondonUndergroundSimulator/blob/main/LondonUndergroundSimulator_DEMO.mp4)
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
