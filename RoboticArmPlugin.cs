// RoboticArmPlugin.cs
using System;
using System.Globalization; // Required for CultureInfo.InvariantCulture for float parsing
using System.Linq; // Required for Select and ToArray on string split
using CorePlatform;
using RoboticArmPlugins;
using System.Collections.Generic; // Required for List<T>

// Define SDK constants for clarity, ideally these would map to actual SDK values.
public static class RoboticArmSdkConstants
{
    public const int SDK_PLAY_MOTION = 1; // Example: Play recorded path at specified index
    public const int SDK_STOP_RECORD = 2; // Example: Stop path recording
    public const int SDK_EMERGENCY_STOP = 0; // Example: Control type for emergency stop
    // Add other SDK-specific constants if they become known
}

namespace CorePlatform
{
    public interface IPlugin
    {
        string Name { get; }
        string Description { get; }
        void Load();
        void Unload();
        void RunTest(System.Action<string> logCallback);
    }

    public interface IScriptablePlugin : IPlugin
    {
        string? ExecuteScriptCommand(string commandName, string? parameters);
        string[] GetAvailableScriptCommands();
    }
}

namespace RoboticArmPlugins
{
    // Helper classes for complex parameters.
    // These would ideally mirror structures defined by the actual Robotic Arm SDK.

    /// <summary>
    /// Parameters for point-to-point control, used for both joint and linear movements.
    /// The SDK mock determines how to interpret these based on the function called.
    /// </summary>
    public class PointControlPara
    {
        /// <summary>
        /// For joint movements (e.g., cr_move_joint). Array of joint angles.
        /// Example: {j1, j2, j3, j4, j5, j6}
        /// </summary>
        public float[]? JointAngles { get; set; }

        /// <summary>
        /// For linear movements (e.g., cr_move_line). Array representing target pose.
        /// Example: {X, Y, Z, Rx, Ry, Rz}
        /// </summary>
        public float[]? TargetPose { get; set; }

        public override string ToString()
        {
            if (JointAngles != null) return $"Joints: [{string.Join(", ", JointAngles)}]";
            if (TargetPose != null) return $"Pose: [{string.Join(", ", TargetPose)}]";
            return "Empty PointControlPara";
        }
    }

    /// <summary>
    /// Parameters for path recording. (Currently a placeholder)
    /// </summary>
    public class RecordPathPara
    {
        // Define properties based on actual SDK requirements if known, e.g., path name, speed, etc.
        public string? PathName { get; set; } = "DefaultPath";
        public override string ToString() => $"PathName: {PathName}";
    }

    /// <summary>
    /// Represents robot state data. (Currently a placeholder)
    /// </summary>
    public class RobotStateData
    {
        public string MockState { get; set; } = "Nominal"; // Example state
        public bool IsPoweredOn { get; set; } = false;
        public bool IsEnabled { get; set; } = false;
        // Add other relevant state fields based on SDK (e.g. error codes, speed, mode)
        public override string ToString() => $"State: {MockState}, PoweredOn: {IsPoweredOn}, Enabled: {IsEnabled}";
    }

    /// <summary>
    /// Represents actual joint positions.
    /// </summary>
    public class JointPos
    {
        public double[] Positions { get; set; } = new double[6]; // Assuming 6 joints
        public override string ToString() => $"[{string.Join("deg, ", Positions.Select(p => p.ToString("F2")))}deg]";
    }

    /// <summary>
    /// Represents actual TCP (Tool Center Point) pose.
    /// </summary>
    public class TcpPose
    {
        public double[] Pose { get; set; } = new double[6]; // X,Y,Z,Rx,Ry,Rz
        public override string ToString() => $"[X:{Pose[0]:F2}, Y:{Pose[1]:F2}, Z:{Pose[2]:F2}, Rx:{Pose[3]:F1}, Ry:{Pose[4]:F1}, Rz:{Pose[5]:F1}]";
    }

    /// <summary>
    /// Mock implementation of the Robotic Arm SDK.
    /// Simulates the behavior of the `cr_...` functions.
    /// </summary>
    public static class RoboticArmSdkMock
    {
        private static void LogSdkCall(string functionName, params object[] args)
        {
            string parameters = args.Length > 0
                ? string.Join(", ", args.Select(a => a == null ? "null" : a.ToString()))
                : "no parameters";
            Console.WriteLine($"RoboticArmSdkMock::{functionName}({parameters})");
        }

        public static int cr_create_robot(ref object? robotHandle, string ipAddr, int port, string passwd)
        {
            LogSdkCall(nameof(cr_create_robot), $"ipAddr='{ipAddr}'", port, $"passwd='{passwd.Substring(0, Math.Min(3, passwd.Length))}***'");
            if (robotHandle != null)
            {
                Console.WriteLine("RoboticArmSdkMock: Attempted to create robot when handle already exists.");
                return -1; // Error: Handle already exists or not null
            }
            robotHandle = new object(); // Simulate successful creation
            Console.WriteLine($"RoboticArmSdkMock: Robot handle created (ID: {robotHandle.GetHashCode()}).");
            return 0; // Success
        }

        public static int cr_destroy_robot(ref object? robotHandle)
        {
            LogSdkCall(nameof(cr_destroy_robot), robotHandle?.GetHashCode() ?? "null handle");
            if (robotHandle == null)
            {
                Console.WriteLine("RoboticArmSdkMock: Attempted to destroy a null robot handle.");
                return -1; // Error: Handle is already null
            }
            Console.WriteLine($"RoboticArmSdkMock: Robot handle destroyed (ID: {robotHandle.GetHashCode()}).");
            robotHandle = null; // Simulate destruction
            return 0; // Success
        }

        // Helper to check if robotHandle is valid for subsequent operations
        private static bool IsHandleInvalid(object? robotHandle, string operationName)
        {
            if (robotHandle == null)
            {
                Console.WriteLine($"RoboticArmSdkMock: Error - {operationName} called with null robotHandle.");
                return true;
            }
            return false;
        }

        public static int cr_poweron(object? robotHandle)
        {
            LogSdkCall(nameof(cr_poweron), robotHandle?.GetHashCode() ?? "null handle");
            if (IsHandleInvalid(robotHandle, nameof(cr_poweron))) return -1;
            // Add mock logic for power on if needed
            return 0; // Success
        }

        public static int cr_poweroff(object? robotHandle)
        {
            LogSdkCall(nameof(cr_poweroff), robotHandle?.GetHashCode() ?? "null handle");
            if (IsHandleInvalid(robotHandle, nameof(cr_poweroff))) return -1;
            return 0; // Success
        }

        public static int cr_enable(object? robotHandle)
        {
            LogSdkCall(nameof(cr_enable), robotHandle?.GetHashCode() ?? "null handle");
            if (IsHandleInvalid(robotHandle, nameof(cr_enable))) return -1;
            return 0; // Success
        }

        public static int cr_disable(object? robotHandle)
        {
            LogSdkCall(nameof(cr_disable), robotHandle?.GetHashCode() ?? "null handle");
            if (IsHandleInvalid(robotHandle, nameof(cr_disable))) return -1;
            return 0; // Success
        }

        public static int cr_move_joint(object? robotHandle, PointControlPara pointControlPara, bool isBlock)
        {
            LogSdkCall(nameof(cr_move_joint), robotHandle?.GetHashCode() ?? "null handle", pointControlPara, isBlock);
            if (IsHandleInvalid(robotHandle, nameof(cr_move_joint))) return -1;
            if (pointControlPara.JointAngles == null) return -2; // Invalid parameter
            // Mock actual movement if necessary
            return 0; // Success
        }

        public static int cr_move_line(object? robotHandle, PointControlPara pointControlPara, bool isBlock)
        {
            LogSdkCall(nameof(cr_move_line), robotHandle?.GetHashCode() ?? "null handle", pointControlPara, isBlock);
            if (IsHandleInvalid(robotHandle, nameof(cr_move_line))) return -1;
            if (pointControlPara.TargetPose == null) return -2; // Invalid parameter
            return 0; // Success
        }

        public static int cr_get_stdDigitalIn(object? robotHandle, int index, out int value)
        {
            LogSdkCall(nameof(cr_get_stdDigitalIn), robotHandle?.GetHashCode() ?? "null handle", index);
            value = 1; // Default mock value
            if (IsHandleInvalid(robotHandle, nameof(cr_get_stdDigitalIn))) return -1;
            if (index < 0 || index > 15) { value = -1; return -2; } // Example: Invalid index
            // Simulate reading a value, e.g., based on index
            value = (index % 2 == 0) ? 1 : 0;
            return 0; // Success
        }

        public static int cr_set_stdDigitalOut(object? robotHandle, int index, int value)
        {
            LogSdkCall(nameof(cr_set_stdDigitalOut), robotHandle?.GetHashCode() ?? "null handle", index, value);
            if (IsHandleInvalid(robotHandle, nameof(cr_set_stdDigitalOut))) return -1;
            if (index < 0 || index > 15) return -2; // Example: Invalid index
            if (value < 0 || value > 1) return -3; // Example: Invalid value
            return 0; // Success
        }

        public static int cr_set_robotSpeedPercent(object? robotHandle, int percent)
        {
            LogSdkCall(nameof(cr_set_robotSpeedPercent), robotHandle?.GetHashCode() ?? "null handle", percent);
            if (IsHandleInvalid(robotHandle, nameof(cr_set_robotSpeedPercent))) return -1;
            if (percent < 0 || percent > 100) return -2; // Invalid percent
            return 0; // Success
        }

        public static int cr_path_recordPara_set(object? robotHandle, RecordPathPara recordPathPara)
        {
            LogSdkCall(nameof(cr_path_recordPara_set), robotHandle?.GetHashCode() ?? "null handle", recordPathPara);
            if (IsHandleInvalid(robotHandle, nameof(cr_path_recordPara_set))) return -1;
            // Mock setting parameters for path recording
            return 0; // Success
        }

        public static int cr_path_control(object? robotHandle, int pathIndex, int controlType)
        {
            LogSdkCall(nameof(cr_path_control), robotHandle?.GetHashCode() ?? "null handle", pathIndex, controlType);
            if (IsHandleInvalid(robotHandle, nameof(cr_path_control))) return -1;
            // Mock path control (play, stop, etc.)
            return 0; // Success
        }

        public static int cr_move_control(object? robotHandle, int moveControlType)
        {
            LogSdkCall(nameof(cr_move_control), robotHandle?.GetHashCode() ?? "null handle", moveControlType);
            if (IsHandleInvalid(robotHandle, nameof(cr_move_control))) return -1;
            // Mock move control (e.g., emergency stop)
            return 0; // Success
        }

        public static int cr_get_robotStateData(object? robotHandle, out RobotStateData stateData)
        {
            LogSdkCall(nameof(cr_get_robotStateData), robotHandle?.GetHashCode() ?? "null handle");
            stateData = new RobotStateData();
            if (IsHandleInvalid(robotHandle, nameof(cr_get_robotStateData)))
            {
                stateData.MockState = "Error: Robot handle not valid.";
                return -1;
            }
            // Populate with mock data
            stateData.IsPoweredOn = true; // Example
            stateData.IsEnabled = true; // Example
            return 0; // Success
        }

        public static int cr_get_jointActualPos(object? robotHandle, out JointPos jointPos)
        {
            LogSdkCall(nameof(cr_get_jointActualPos), robotHandle?.GetHashCode() ?? "null handle");
            jointPos = new JointPos(); // Initialize with default (e.g., all zeros)
            if (IsHandleInvalid(robotHandle, nameof(cr_get_jointActualPos))) return -1;
            // Populate with mock data
            for(int i=0; i < jointPos.Positions.Length; i++) jointPos.Positions[i] = 10.0 * (i+1); // Example values
            return 0; // Success
        }

        public static int cr_get_tcpActualPose(object? robotHandle, out TcpPose tcpPose)
        {
            LogSdkCall(nameof(cr_get_tcpActualPose), robotHandle?.GetHashCode() ?? "null handle");
            tcpPose = new TcpPose(); // Initialize with default
            if (IsHandleInvalid(robotHandle, nameof(cr_get_tcpActualPose))) return -1;
            // Populate with mock data
            tcpPose.Pose = new double[] {100.0, 150.0, 200.0, 10.0, 20.0, 30.0}; // Example values
            return 0; // Success
        }
    }

    /// <summary>
    /// C# Plugin for controlling a Robotic Arm via a Mock SDK.
    /// Implements IPlugin and IScriptablePlugin for integration with a host application.
    /// </summary>
    public class RoboticArmPlugin : CorePlatform.IPlugin, CorePlatform.IScriptablePlugin
    {
        private System.Action<string>? _hostLogCallback;
        private object? _robotHandle; // Represents the connection to the robot; null if not connected.

        /// <summary>
        /// Helper method for logging messages, uses host callback if available, otherwise Console.
        /// </summary>
        private void Log(string message) => (_hostLogCallback ?? Console.WriteLine)($"RoboticArmPlugin: {message}");

        public string Name => "Robotic Arm Control Plugin";
        public string Description => "Provides scriptable control of a (mocked) robotic arm, including movement, I/O, and status queries.";

        public void Load()
        {
            // Use Console.WriteLine as _hostLogCallback might not be set yet.
            Console.WriteLine("RoboticArmPlugin: Load method called. Initializing...");
            _robotHandle = null; // Ensure handle is null on load
        }

        public void Unload()
        {
            Log("Unload method called. Cleaning up...");
            if (_robotHandle != null)
            {
                Log("Robot handle exists. Attempting to power off and disconnect.");
                RoboticArmSdkMock.cr_disable(_robotHandle); // Best effort
                RoboticArmSdkMock.cr_poweroff(_robotHandle); // Best effort
                RoboticArmSdkMock.cr_destroy_robot(ref _robotHandle);
                Log("Robot handle destroyed.");
            }
            Log("Unload completed.");
        }

        public void RunTest(System.Action<string> logCallback)
        {
            _hostLogCallback = logCallback; // Store the callback for plugin-wide logging
            Log("RunTest method started. Executing a sequence of script commands...");

            try
            {
                // Test Connection
                string connectResult = ExecuteScriptCommand("connect", "192.168.1.100,8080,sdkpass123") ?? "null response";
                Log($"Test 'connect': {connectResult}");
                if (_robotHandle == null) { Log("TEST CRITICAL FAIL: Robot handle is null after connect. Aborting further tests."); return; }

                // Test Basic Operations
                Log($"Test 'power_on': {ExecuteScriptCommand("power_on", null)}");
                Log($"Test 'enable': {ExecuteScriptCommand("enable", null)}");
                Log($"Test 'set_speed' to 75%: {ExecuteScriptCommand("set_speed", "75")}");
                Log($"Test 'set_speed' to invalid -10%: {ExecuteScriptCommand("set_speed", "-10")}");


                // Test Movement
                Log($"Test 'move_joint': {ExecuteScriptCommand("move_joint", "10.1,20.2,30.3,40.4,50.5,60.6")}");
                Log($"Test 'move_joint' with invalid params: {ExecuteScriptCommand("move_joint", "10,20,nonfloat,40")}");
                Log($"Test 'move_linear': {ExecuteScriptCommand("move_linear", "100,200,300,10,20,30")}");

                // Test Getters
                Log($"Test 'get_joint_position': {ExecuteScriptCommand("get_joint_position", null)}");
                Log($"Test 'get_tcp_position': {ExecuteScriptCommand("get_tcp_position", null)}");
                Log($"Test 'get_robot_state': {ExecuteScriptCommand("get_robot_state", null)}");

                // Test I/O
                Log($"Test 'read_digital_input' index 5: {ExecuteScriptCommand("read_digital_input", "5")}");
                Log($"Test 'read_digital_input' invalid index 99: {ExecuteScriptCommand("read_digital_input", "99")}");
                Log($"Test 'set_digital_output' index 2 to 1: {ExecuteScriptCommand("set_digital_output", "2,1")}");
                Log($"Test 'set_digital_output' index 3 to invalid value 5: {ExecuteScriptCommand("set_digital_output", "3,5")}");


                // Test Recording and Playback (Simplified)
                Log($"Test 'start_recording': {ExecuteScriptCommand("start_recording", null)}");
                Log($"Test 'stop_recording': {ExecuteScriptCommand("stop_recording", null)}");
                Log($"Test 'play_motion' index 0: {ExecuteScriptCommand("play_motion", "0")}");

                // Test Emergency Stop
                Log($"Test 'emergency_stop': {ExecuteScriptCommand("emergency_stop", null)}");

                // Test Disconnection
                Log($"Test 'disable': {ExecuteScriptCommand("disable", null)}");
                Log($"Test 'power_off': {ExecuteScriptCommand("power_off", null)}");
                string disconnectResult = ExecuteScriptCommand("disconnect", null) ?? "null response";
                Log($"Test 'disconnect': {disconnectResult}");
                if (_robotHandle != null) { Log("TEST FAIL: Robot handle is not null after disconnect."); }
            }
            catch (Exception ex)
            {
                Log($"RunTest encountered an unhandled exception: {ex.Message} - StackTrace: {ex.StackTrace}");
            }
            Log("RunTest method completed.");
        }

        public string[] GetAvailableScriptCommands()
        {
            Log("GetAvailableScriptCommands called.");
            return new string[] {
                "connect", "disconnect",
                "power_on", "power_off", "enable", "disable", "emergency_stop",
                "move_joint", "move_linear", "set_speed",
                "start_recording", "stop_recording", "play_motion",
                "read_digital_input", "set_digital_output",
                "get_robot_state", "get_joint_position", "get_tcp_position",
                "help" // Added help command
            };
        }

        public string? ExecuteScriptCommand(string commandName, string? parameters)
        {
            string cmdLower = commandName.ToLowerInvariant();
            Log($"Executing command '{cmdLower}', Parameters: '{parameters ?? "N/A"}'");

            // Handle "help" command separately as it doesn't require a connection
            if (cmdLower == "help")
            {
                return "Available commands: " + string.Join(", ", GetAvailableScriptCommands());
            }

            // Most commands require an active robot connection.
            // "connect" is an exception as it establishes the connection.
            if (cmdLower != "connect" && _robotHandle == null)
            {
                Log("Error: Attempted to execute command without robot connection.");
                return "Error: Robot not connected. Please use the 'connect' command first.";
            }

            try
            {
                switch (cmdLower)
                {
                    // Connection Management
                    case "connect":
                        if (_robotHandle != null) return "Error: Already connected. Please disconnect first.";
                        if (string.IsNullOrWhiteSpace(parameters)) return "Error: Missing parameters for 'connect'. Expected: ipAddress,port,password";
                        var connParts = parameters.Split(',');
                        if (connParts.Length != 3) return "Error: Invalid parameter count for 'connect'. Expected: ipAddress,port,password";
                        if (!int.TryParse(connParts[1].Trim(), out int portVal)) return "Error: Invalid port number. Port must be an integer.";
                        // Note: _robotHandle is passed by ref to cr_create_robot
                        int connectCode = RoboticArmSdkMock.cr_create_robot(ref _robotHandle, connParts[0].Trim(), portVal, connParts[2].Trim());
                        return connectCode == 0 ? "Robot connected successfully." : $"Error connecting to robot (Code: {connectCode}).";

                    case "disconnect":
                        // _robotHandle null check already performed for most commands
                        int disconnectCode = RoboticArmSdkMock.cr_destroy_robot(ref _robotHandle);
                        return disconnectCode == 0 ? "Robot disconnected successfully." : $"Error disconnecting robot (Code: {disconnectCode}).";

                    // Power and Enablement
                    case "power_on":  return RoboticArmSdkMock.cr_poweron(_robotHandle) == 0 ? "Robot powered on." : "Error powering on robot.";
                    case "power_off": return RoboticArmSdkMock.cr_poweroff(_robotHandle) == 0 ? "Robot powered off." : "Error powering off robot.";
                    case "enable":    return RoboticArmSdkMock.cr_enable(_robotHandle) == 0 ? "Robot enabled." : "Error enabling robot.";
                    case "disable":   return RoboticArmSdkMock.cr_disable(_robotHandle) == 0 ? "Robot disabled." : "Error disabling robot.";

                    // Movement Commands
                    case "move_joint":
                        if (string.IsNullOrWhiteSpace(parameters)) return "Error: Missing joint angles for 'move_joint'. Expected comma-separated float values (e.g., j1,j2,...,j6).";
                        var jointStrs = parameters.Split(',');
                        var jointVals = new List<float>();
                        foreach (var jStr in jointStrs)
                        {
                            if (!float.TryParse(jStr.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float jVal))
                                return $"Error: Invalid joint angle '{jStr}'. Must be a float.";
                            jointVals.Add(jVal);
                        }
                        // Assuming a 6-axis robot for this example, but could be flexible
                        if (jointVals.Count == 0) return "Error: No joint angles provided for 'move_joint'.";
                        var pcpJointMove = new PointControlPara { JointAngles = jointVals.ToArray() };
                        return RoboticArmSdkMock.cr_move_joint(_robotHandle, pcpJointMove, false) == 0 ? "Move joint command sent." : "Error sending move_joint command.";

                    case "move_linear":
                        if (string.IsNullOrWhiteSpace(parameters)) return "Error: Missing pose data for 'move_linear'. Expected comma-separated float values (e.g., x,y,z,rx,ry,rz).";
                        var poseStrs = parameters.Split(',');
                        var poseVals = new List<float>();
                        foreach (var pStr in poseStrs)
                        {
                            if (!float.TryParse(pStr.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float pVal))
                                return $"Error: Invalid pose parameter '{pStr}'. Must be a float.";
                            poseVals.Add(pVal);
                        }
                         // Assuming a 6-DOF pose (X,Y,Z,Rx,Ry,Rz)
                        if (poseVals.Count == 0) return "Error: No pose data provided for 'move_linear'.";
                        var pcpLinearMove = new PointControlPara { TargetPose = poseVals.ToArray() };
                        return RoboticArmSdkMock.cr_move_line(_robotHandle, pcpLinearMove, false) == 0 ? "Move linear command sent." : "Error sending move_linear command.";

                    case "set_speed":
                        if (string.IsNullOrWhiteSpace(parameters) || !int.TryParse(parameters.Trim(), out int speedPercent) || speedPercent < 0 || speedPercent > 100)
                            return "Error: Invalid speed percentage. Expected an integer between 0 and 100.";
                        return RoboticArmSdkMock.cr_set_robotSpeedPercent(_robotHandle, speedPercent) == 0 ? $"Robot speed set to {speedPercent}%." : "Error setting robot speed.";

                    // Path Recording and Playback
                    case "start_recording":
                        // Parameters for RecordPathPara could be parsed here if needed, e.g., path name
                        var recordParams = new RecordPathPara(); // Using default mock params
                        return RoboticArmSdkMock.cr_path_recordPara_set(_robotHandle, recordParams) == 0 ? "Path recording parameters set (mock start)." : "Error starting path recording.";

                    case "stop_recording":
                        // Assuming index 0 and specific controlType for stop, per SDK mock
                        return RoboticArmSdkMock.cr_path_control(_robotHandle, 0, RoboticArmSdkConstants.SDK_STOP_RECORD) == 0 ? "Path recording stopped." : "Error stopping path recording.";

                    case "play_motion":
                        if (string.IsNullOrWhiteSpace(parameters) || !int.TryParse(parameters.Trim(), out int pathIdx))
                            return "Error: Invalid path index for 'play_motion'. Expected an integer.";
                        return RoboticArmSdkMock.cr_path_control(_robotHandle, pathIdx, RoboticArmSdkConstants.SDK_PLAY_MOTION) == 0 ? $"Playing motion from path index {pathIdx}." : "Error playing motion.";

                    // I/O Commands
                    case "read_digital_input":
                        if (string.IsNullOrWhiteSpace(parameters) || !int.TryParse(parameters.Trim(), out int inputIdx))
                            return "Error: Invalid input index for 'read_digital_input'. Expected an integer.";
                        int inputVal;
                        int readDiCode = RoboticArmSdkMock.cr_get_stdDigitalIn(_robotHandle, inputIdx, out inputVal);
                        return readDiCode == 0 ? $"Digital input at index {inputIdx} is: {inputVal}." : $"Error reading digital input (Code: {readDiCode}).";

                    case "set_digital_output":
                        if (string.IsNullOrWhiteSpace(parameters)) return "Error: Missing parameters for 'set_digital_output'. Expected: index,value";
                        var doParts = parameters.Split(',');
                        if (doParts.Length != 2 || !int.TryParse(doParts[0].Trim(), out int outputIdx) || !int.TryParse(doParts[1].Trim(), out int outputVal))
                            return "Error: Invalid parameters for 'set_digital_output'. Expected: index (int), value (int).";
                        int setDoCode = RoboticArmSdkMock.cr_set_stdDigitalOut(_robotHandle, outputIdx, outputVal);
                        return setDoCode == 0 ? $"Digital output at index {outputIdx} set to {outputVal}." : $"Error setting digital output (Code: {setDoCode}).";

                    // Emergency Stop
                    case "emergency_stop":
                        return RoboticArmSdkMock.cr_move_control(_robotHandle, RoboticArmSdkConstants.SDK_EMERGENCY_STOP) == 0 ? "Emergency stop command sent." : "Error sending emergency stop command.";

                    // Getter Commands
                    case "get_robot_state":
                        RobotStateData state;
                        int stateCode = RoboticArmSdkMock.cr_get_robotStateData(_robotHandle, out state);
                        return stateCode == 0 ? $"Robot state: {state}." : $"Error getting robot state (Code: {stateCode}).";

                    case "get_joint_position":
                        JointPos joints;
                        int jointCode = RoboticArmSdkMock.cr_get_jointActualPos(_robotHandle, out joints);
                        return jointCode == 0 ? $"Joint positions: {joints}." : $"Error getting joint positions (Code: {jointCode}).";

                    case "get_tcp_position":
                        TcpPose pose;
                        int poseCode = RoboticArmSdkMock.cr_get_tcpActualPose(_robotHandle, out pose);
                        return poseCode == 0 ? $"TCP pose: {pose}." : $"Error getting TCP pose (Code: {poseCode}).";

                    default:
                        Log($"Unknown command received: '{commandName}'");
                        return $"Error: Unknown command '{commandName}'. Type 'help' for a list of available commands.";
                }
            }
            catch (Exception ex)
            {
                Log($"ExecuteScriptCommand unhandled exception for command '{commandName}': {ex.Message} - StackTrace: {ex.StackTrace}");
                return $"Error processing command '{commandName}': An unexpected error occurred. ({ex.GetType().Name})";
            }
        }
    }
}
