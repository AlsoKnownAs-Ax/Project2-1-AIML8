# AIML-8

## Overview

This project is designed to manage and train agents using Unity`s ML-Agents. It includes modules for adding and training sensors, as well as running the project with different settings.

## How to Run the Training

### Components

- **Sensors Module**: Contains the code for the new added sensors.

### Adding Sensors

1. Open Unity's editor
2. Open a specific Agent by clicking on it and navigating to the AgentSoccer.cs script
3. Select the wanted sensors from the drop down list

### Validating Sensors

1. Run the training on using ml-agents training guide
2. Navigate to your results folder
   ex:

```bash
cd results/SoccerTwosRun/SoccerTwos.onnx
```

3. Load your .onnx file into [Notron.app](https://netron.app/)
4. You should see someting like this:
   ex for 4 active sensors:
   ![4 Sensor Notron Image](./guide-assets/Notron%204%20sensors.JPG)

### Different Settings for Training

- **Default Settings**: Use the default configuration provided in the `config` folder.

## How We Branched the Project

The project follows a branching strategy to manage different features and releases:

- **develop**: The main branch containing the stable version of the project.
- **feature/**: Branches for new features.
- **bugfix/**: Branches for bug fixes.

## Explanation of the Added Project Structure

- **Sensors Module**: Contains all the sensor-related code.
  - `HearingSensor.cs`: Code related to Hearing Sensor.
  - `MemoryBasedSensor.cs`: Code related to Memory based Sensor.
  - `VisionCone.cs`: Code related to Vision Cone Sensor.
  - `ISoccerSensor.cs`: Interface for the soccer sensors.
