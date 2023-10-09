# Unity ArduBoat Simulator

Unity simulation of ArduRover boogie board boat. Uses the ArduPilot JSON API to interface with [ArduPilot SITL](https://ardupilot.org/dev/docs/sitl-simulator-software-in-the-loop.html).

## Usage

A built program is available for Windows in the `Build/` folder.

The unity program connects to an ArduPilot SITL using the ArduPilot JSON interface: https://ardupilot.org/dev/docs/sitl-with-JSON.html

1. Start ArduPilot's `sim_vehicle.py`
2. Start the Unity simulator
3. Command a mission!

For documentation on the ArduPilot JSON API, see: https://github.com/ArduPilot/ardupilot/tree/master/libraries/SITL/examples/JSON

## Controls

- Click + Drag to orbit the camera
- Mouse wheel to zoom in/out
- R to reset the scene

## Limitations

- The sim is very sensitive to startup order, you might have to play around with timing of when you start the ardupilot script and Unity program to get it to work.
- Accelerations are not reported; this does not seem to trip up ArduRover but might break other projects.
- You need to manually configure your ArduPilot SITL to use ArduRover and differential drive.