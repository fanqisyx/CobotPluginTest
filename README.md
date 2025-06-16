# Robotic Arm Plugin

This plugin provides an interface to control a robotic arm. It is designed for the `TestPlatformExample` and implements the `IPlugin` and `IScriptablePlugin` interfaces.

**Note:** This version of the plugin uses a **mock SDK** to simulate robotic arm interactions. All operations are logged to the console/host logger but do not control a physical device.

## Features:

-   **Connection Management:**
    -   `connect <ipAddress>,<port>,<password>`: Establishes a connection to the mock robot.
    -   `disconnect`: Terminates the mock connection.
-   **Power & State Control:**
    -   `power_on`: Simulates powering on the robot.
    -   `power_off`: Simulates powering off the robot.
    -   `enable`: Simulates enabling the robot servos.
    -   `disable`: Simulates disabling the robot servos.
-   **Movement Control:**
    -   `move_joint <j1>,<j2>,<j3>,<j4>,<j5>,<j6>`: Moves the robot to specified joint angles.
    -   `move_linear <x>,<y>,<z>,<rx>,<ry>,<rz>`: Moves the robot to a specified Cartesian pose.
    -   `set_speed <percentage>`: Sets the robot's operational speed.
    -   `emergency_stop`: Simulates an emergency stop.
-   **Input/Output Operations:**
    -   `read_digital_input <index>`: Reads a mock digital input.
    -   `set_digital_output <index>,<value>`: Sets a mock digital output.
-   **Status & Information:**
    -   `get_robot_state`: Retrieves the mock robot's current state.
    -   `get_joint_position`: Retrieves the mock robot's current joint positions.
    -   `get_tcp_position`: Retrieves the mock robot's current TCP pose.
-   **Path Recording & Playback (Simulated):**
    -   `start_recording`: Simulates starting path recording.
    -   `stop_recording`: Simulates stopping path recording.
    -   `play_motion <motion_name>`: Simulates playing a recorded motion.
-   **Utility:**
    -   `help`: Lists all available script commands.
-   **Testing:**
    -   The plugin includes a `RunTest` method (executable from the host application) that demonstrates a sequence of operations.
