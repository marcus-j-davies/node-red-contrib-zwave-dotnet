using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Reflection;

namespace Server
{
    class Program
    {

        
        static ZWaveLib.ZWaveController ZWC = null;
        static bool Inited = false;
        static string SerialPort = string.Empty;
        static SerialPortLib.SerialPortInput SPI = null;

        private static void Send(Dictionary<string, object> Payload)
        {
            string JSON = Newtonsoft.Json.JsonConvert.SerializeObject(Payload);
            Console.Out.WriteLine(JSON);
        }
       
        static void Main(string[] args)
        {
            

            Begin();
        }

        private static void Begin()
        {
            new System.Threading.Thread(() =>
            {
                while (true)
                {
                    string Payload = Console.In.ReadLine();

                    switch (Payload)
                    {
                        case "INIT":
                            SerialPort = Console.In.ReadLine();
                            Init();
                            break;

                        default:
                            if(Inited)
                            {
                                Process(Payload);
                            }
                            
                            break;
                    }
                }

            }).Start();




            Console.Out.WriteLine("READY");

            System.Threading.Thread.Sleep(-1);

        }

       



        private static void Process(string Payload)
        {
            try
            {
                Request _Request = Newtonsoft.Json.JsonConvert.DeserializeObject<Request>(Payload);
                Dictionary<string, object> Status = new Dictionary<string, object>();
                Status.Add("type", "SystemStatus");
                Status.Add("value", "Sending : " + _Request.operation + " [NodeID:" + _Request.node  + "] (" + DateTime.Now.ToString() + ")");
                Status.Add("color", "yellow");
                Send(Status);


                switch (_Request.operation)
                {

                    //add
                    case "StartNodeAdd":
                        ZWC.BeginNodeAdd();
                        break;

                    // stop
                    case "StopNodeAdd":
                        ZWC.StopNodeAdd();
                        break;

                    // remove
                    case "StartNodeRemove":
                        ZWC.BeginNodeRemove();
                        break;
                        
                    // stop
                    case "StopNodeRemove":
                        ZWC.StopNodeRemove();
                        break;


                    // By Pass
                    case "DirectSerial":
                        SendSerial(_Request.raw);
                        break;


                    // Raw
                    case "RawData":
                        SendRaw(_Request.node, _Request.raw);
                        break;

                    // MultiLevel
                    case "SetMultiLevelSwitch":
                        SetMultiSwitch(_Request.node, Convert.ToInt32(_Request.operation_vars[0]));
                        break;

                    case "GetMultiLevelSwitch":
                        ZWaveLib.CommandClasses.SwitchMultilevel.Get(ZWC.GetNode(_Request.node));
                        break;

                    // Thermostate Mode
                    case "SetThermostatMode":
                        SetThermostatMode(_Request.node, Convert.ToString(_Request.operation_vars[0]));
                        break;

                    case "GetThermostatMode":
                        ZWaveLib.CommandClasses.ThermostatMode.Get(ZWC.GetNode(_Request.node));
                        break;

                    // Thermostate Setpoint
                    case "SetThermostatSetPoint":
                        SetThermostatSetPoint(_Request.node, Convert.ToString(_Request.operation_vars[0]), Convert.ToDouble(_Request.operation_vars[1]));
                        break;

                    case "GetThermostatSetPoint":
                        ZWaveLib.CommandClasses.ThermostatSetPoint.Value Value = (ZWaveLib.CommandClasses.ThermostatSetPoint.Value)Enum.Parse(typeof(ZWaveLib.CommandClasses.ThermostatSetPoint.Value), Convert.ToString(_Request.operation_vars[0]));
                        ZWaveLib.CommandClasses.ThermostatSetPoint.Get(ZWC.GetNode(_Request.node), Value);
                        break;


                    // Wake up
                    case "SetWakeInterval":
                        SetWakeInterval(_Request.node, Convert.ToUInt32(_Request.operation_vars[0]));
                        break;

                    case "GetWakeInterval":
                        ZWaveLib.CommandClasses.WakeUp.Get(ZWC.GetNode(_Request.node));
                        break;

                    // Config
                    case "SetConfiguration":
                        SetConfiguration(_Request.node, Convert.ToByte(_Request.operation_vars[0]), Convert.ToInt32(_Request.operation_vars[1]));
                        break;

                    case "GetConfiguration":
                        ZWaveLib.CommandClasses.Configuration.Get(ZWC.GetNode(_Request.node), Convert.ToByte(_Request.operation_vars[0]));
                        break;

                    // Binary
                    case "SetBinary":
                        SetBinary(_Request.node, Convert.ToBoolean(_Request.operation_vars[0]));
                        break;

                    case "GetBinary":
                        ZWaveLib.CommandClasses.SwitchBinary.Get(ZWC.GetNode(_Request.node));
                        break;

                    // Basic
                    case "SetBasic":
                        SetBasic(_Request.node, Convert.ToInt32(_Request.operation_vars[0]));
                        break;

                    case "GetBasic":
                        ZWaveLib.CommandClasses.Basic.Get(ZWC.GetNode(_Request.node));
                        break;

                    // Battery
                    case "GetBattery":
                        ZWaveLib.CommandClasses.Battery.Get(ZWC.GetNode(_Request.node));
                        break;


                    // Notification
                    case "SendNotificationReport":
                        SendNotificationReport(_Request.node, Convert.ToByte(_Request.operation_vars[0]), Convert.ToByte(_Request.operation_vars[1]));
                        break;

                    default:
                        Status.Clear();
                        Status.Add("type", "SystemStatus");
                        Status.Add("value", "Command : " + _Request.operation + " Not Implemented.");
                        Status.Add("color", "red");
                        Send(Status);
                        goto Exit;

                }

             

                Status.Clear();
                Status.Add("type", "SystemStatus");
                Status.Add("value", "Sent : " + _Request.operation + " [NodeID:" + _Request.node + "] (" + DateTime.Now.ToString() + ")");
                Status.Add("color", "green");
                Send(Status);

            Exit:
                return;


            }
            catch (Exception Error)
            {
                Dictionary<string, object> _Error = new Dictionary<string, object>();

                _Error.Add("type", "SystemStatus");
                _Error.Add("value", Error.Message);
                _Error.Add("color", "red");
                Send(_Error);

            }
        }

      

        private static void Init()
        {
            Dictionary<string, object> Status = new Dictionary<string, object>();
            Status.Add("type", "SystemStatus");
            Status.Add("value", "System Initializing...");
            Status.Add("color", "yellow");
            Send(Status);

            ZWC = new ZWaveLib.ZWaveController(SerialPort);

            SPI =  (SerialPortLib.SerialPortInput)ZWC.GetType().GetField("serialPort", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ZWC);

            ZWC.ControllerStatusChanged += ZWC_ControllerStatusChanged;
            ZWC.NodeUpdated += ZWC_NodeUpdated;
            ZWC.NodeOperationProgress += ZWC_NodeOperationProgress;
            ZWC.DiscoveryProgress += ZWC_DiscoveryProgress;
            

         




            Status["value"] = "Connecting to Controller...";
            Send(Status);


            ZWC.Connect();
        }

        private static void ZWC_DiscoveryProgress(object sender, ZWaveLib.DiscoveryProgressEventArgs args)
        {
            Dictionary<string, object> Status = new Dictionary<string, object>();

            if (args.Status == ZWaveLib.DiscoveryStatus.DiscoveryEnd)
            {
                Status.Add("type", "SystemStatus");
                Status.Add("value", "ZWave Controller Ready");
                Status.Add("color", "green");
                Send(Status);
                Inited = true;
            }
        }

        private static void ZWC_NodeOperationProgress(object sender, ZWaveLib.NodeOperationProgressEventArgs args)
        {
            Dictionary<string, object> Payload = new Dictionary<string, object>();

            

            Payload.Add("type", "NodeMessage");
            Payload.Add("node", args.NodeId);
            Payload.Add("class", "NodeOperation");
            Payload.Add("index", 0);
            Payload.Add("value", args.Status.ToString());
            Payload.Add("timestamp", args.Timestamp);

            Send(Payload);

          
        }

        private static void SendRaw(byte NodeID, byte[] RawData)
        {
            byte[] Result = ZWaveLib.ZWaveMessage.BuildSendDataRequest(NodeID, RawData);
            ZWaveLib.ZWaveMessage Message = new ZWaveLib.ZWaveMessage(Result);
            ZWC.QueueMessage(Message);
        }

        private static void SetMultiSwitch(byte NodeID, int Value)
        {
            ZWaveLib.CommandClasses.SwitchMultilevel.Set(ZWC.GetNode(NodeID), Value);
        }

        private static void SetThermostatMode(byte NodeID, string Mode)
        {
            ZWaveLib.CommandClasses.ThermostatMode.Value Value = (ZWaveLib.CommandClasses.ThermostatMode.Value)Enum.Parse(typeof(ZWaveLib.CommandClasses.ThermostatMode.Value), Mode);
            ZWaveLib.CommandClasses.ThermostatMode.Set(ZWC.GetNode(NodeID), Value);
        }

        private static void SetThermostatSetPoint(byte NodeID, string Setpoint,double DValue)
        {
            ZWaveLib.CommandClasses.ThermostatSetPoint.Value Value = (ZWaveLib.CommandClasses.ThermostatSetPoint.Value)Enum.Parse(typeof(ZWaveLib.CommandClasses.ThermostatSetPoint.Value), Setpoint);
            ZWaveLib.CommandClasses.ThermostatSetPoint.Set(ZWC.GetNode(NodeID),Value, DValue);
        }


        private static void SetWakeInterval(byte NodeID, uint Seconds)
        {
            ZWaveLib.CommandClasses.WakeUp.Set(ZWC.GetNode(NodeID), Seconds);
        }

        private static void SetConfiguration(byte NodeID, byte Parameter, int Value)
        {
            ZWaveLib.CommandClasses.Configuration.Set(ZWC.GetNode(NodeID), Parameter, Value);
        }

        private static void SetBinary(byte NodeID, bool Value)
        {
            ZWaveLib.CommandClasses.SwitchBinary.Set(ZWC.GetNode(NodeID), Convert.ToInt32(Value));
        }

        private static void SetBasic(byte NodeID, int Value)
        {
            ZWaveLib.CommandClasses.Basic.Set(ZWC.GetNode(NodeID), Value);
        }

        private static void SendNotificationReport(byte NodeID, byte Type, byte Event)
        {
            byte[] MessageBytes = new byte[10];
            MessageBytes[0] = 0x71;
            MessageBytes[1] = 0x05;
            MessageBytes[6] = Type;
            MessageBytes[7] = Event;

            byte[] Result = ZWaveLib.ZWaveMessage.BuildSendDataRequest(NodeID, MessageBytes);
            ZWaveLib.ZWaveMessage Message = new ZWaveLib.ZWaveMessage(Result);
            ZWC.QueueMessage(Message);
        } 

        private static void SendSerial(byte[] Raw)
        {
            SPI.SendMessage(Raw);
        }

        private static void ZWC_NodeUpdated(object sender, ZWaveLib.NodeUpdatedEventArgs args)
        {
            Dictionary<string, object> Payload = new Dictionary<string, object>();
            Payload.Add("type", "NodeMessage");
            Payload.Add("node", args.NodeId);
            Payload.Add("class", args.Event.Parameter.ToString());
            Payload.Add("index", args.Event.Instance);
            Payload.Add("value", args.Event.Value);
            Payload.Add("timestamp", args.Event.Timestamp);

            Send(Payload);




        }

        private static void ZWC_ControllerStatusChanged(object sender, ZWaveLib.ControllerStatusEventArgs args)
        {
            Dictionary<string, object> Status = new Dictionary<string, object>();
            
            switch (args.Status)
            {
                case ZWaveLib.ControllerStatus.Connected:
                    Status.Add("type", "SystemStatus");
                    Status.Add("value", "Initializing Controller...");
                    Status.Add("color", "yellow");
                    Send(Status);
                    ZWC.Initialize();
                    break;

                case ZWaveLib.ControllerStatus.Ready:

                    Status.Add("type", "SystemStatus");
                    Status.Add("value", "Scanning node capabilities...");
                    Status.Add("color", "yellow");
                    Send(Status);
                    ZWC.Discovery();

                    break;

                case ZWaveLib.ControllerStatus.Error:
                case ZWaveLib.ControllerStatus.Disconnected:
                    Status.Add("type", "SystemStatus");
                    Status.Add("value", "ZWave Controller Failed/Disconnected");
                    Status.Add("color", "red");
                    Send(Status);
                    break;


            }
        }
      
    }
}
