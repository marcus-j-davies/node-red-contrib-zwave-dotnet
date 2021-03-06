﻿using System;
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
        static Socket ServerSocket = null;
        static Socket NRClientSocket = null;

        static void Main(string[] args)
        {
            Begin();
        }

        private static void Send(Dictionary<string, object> Payload)
        {
            string JSON = Newtonsoft.Json.JsonConvert.SerializeObject(Payload);
            byte[] Data = System.Text.Encoding.UTF8.GetBytes(JSON);
            SendData(Data, NRClientSocket);
        }
       
       

        private static void SendData(byte[] Data, Socket S)
        {
            byte[] NL = Encoding.UTF8.GetBytes(Environment.NewLine);
            S.Send(Data, Data.Length, SocketFlags.None);
            S.Send(NL, NL.Length, SocketFlags.None);
        }

        private static void Begin()
        {
            ServerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(new IPEndPoint(IPAddress.Loopback, 45342));
            ServerSocket.Listen(0);

            Console.Out.WriteLine("READY");

            new System.Threading.Thread(() =>
            {
                while (true)
                {
                    NRClientSocket = ServerSocket.Accept();
                    NRClientSocket.NoDelay = true;

                    NetworkStream NWS = new NetworkStream(NRClientSocket);
                    StreamReader SR = new StreamReader(NWS);


                    SerialPort = SR.ReadLine();

                    Init();

                    while(true)
                    {
                        string MSG = SR.ReadLine();
                        if (Inited)
                        {
                            Process(MSG);
                        }

                    }


                   
                }

            }).Start();

            System.Threading.Thread.Sleep(-1);

        }

       



        private static void Process(string Payload)
        {
            try
            {
                Request _Request = Newtonsoft.Json.JsonConvert.DeserializeObject<Request>(Payload);
                Dictionary<string, object> Status = new Dictionary<string, object>();
                Status.Add("type", "SystemStatus");
                Status.Add("value", "Last Sent : " + _Request.operation + " [NodeID:" + _Request.node + "] (" + DateTime.Now.ToString() + ")");
                Status.Add("color", "green");
                Send(Status);


                ZWaveLib.CommandClasses.ThermostatMode.Value TSMV;
                ZWaveLib.CommandClasses.ThermostatSetPoint.Value TSPV;



                switch (_Request.operation)
                {
                    // List
                    case "GetNodes":
                        Status.Clear();
                        Status.Add("type", "NodeMessage");
                        Status.Add("node", 1);
                        Status.Add("class", "GetNodes");
                        Status.Add("index", 0);
                        Status.Add("value", ZWC.Nodes);
                        Status.Add("timestamp", DateTime.Now);
                        Send(Status);
                        break;

                    // Soft Reset
                    case "SoftReset":
                        ZWC.SoftReset();
                        break;

                    // Hard Reset
                    case "HardReset":
                        ZWC.HardReset();
                        break;

                    // heal
                    case "HealNetwork":
                        ZWC.HealNetwork();
                        break;

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
                    case "SerialAPIMessage":
                        SPI.SendMessage(_Request.raw);
                        break;


                    // Raw
                    case "RawZWaveMessage":
                        ZWC.GetNode(_Request.node).SendDataRequest(_Request.raw);
                        break;

                    // MultiLevel
                    case "SetMultiLevelSwitch":
                        ZWaveLib.CommandClasses.SwitchMultilevel.Set(ZWC.GetNode(_Request.node), Convert.ToInt32(_Request.operation_vars[0]));
                        
                        break;

                    case "GetMultiLevelSwitch":
                        ZWaveLib.CommandClasses.SwitchMultilevel.Get(ZWC.GetNode(_Request.node));
                        break;

                    // Thermostate Mode
                    case "SetThermostatMode":
                        TSMV = (ZWaveLib.CommandClasses.ThermostatMode.Value)Enum.Parse(typeof(ZWaveLib.CommandClasses.ThermostatMode.Value), Convert.ToString(_Request.operation_vars[0]));
                        ZWaveLib.CommandClasses.ThermostatMode.Set(ZWC.GetNode(_Request.node), TSMV);


                        break;

                    case "GetThermostatMode":
                        ZWaveLib.CommandClasses.ThermostatMode.Get(ZWC.GetNode(_Request.node));
                        break;

                    // Thermostate Setpoint
                    case "SetThermostatSetPoint":
                        TSPV  = (ZWaveLib.CommandClasses.ThermostatSetPoint.Value)Enum.Parse(typeof(ZWaveLib.CommandClasses.ThermostatSetPoint.Value), Convert.ToString(_Request.operation_vars[0]));
                        ZWaveLib.CommandClasses.ThermostatSetPoint.Set(ZWC.GetNode(_Request.node), TSPV, Convert.ToDouble(_Request.operation_vars[1]));


                        break;

                    case "GetThermostatSetPoint":
                        TSPV = (ZWaveLib.CommandClasses.ThermostatSetPoint.Value)Enum.Parse(typeof(ZWaveLib.CommandClasses.ThermostatSetPoint.Value), Convert.ToString(_Request.operation_vars[0]));
                        ZWaveLib.CommandClasses.ThermostatSetPoint.Get(ZWC.GetNode(_Request.node), TSPV);
                        break;


                    // Wake up
                    case "SetWakeInterval":
                        ZWaveLib.CommandClasses.WakeUp.Set(ZWC.GetNode(_Request.node), Convert.ToUInt32(_Request.operation_vars[0]));
                        break;

                    case "GetWakeInterval":
                        ZWaveLib.CommandClasses.WakeUp.Get(ZWC.GetNode(_Request.node));
                        break;

                    // Config
                    case "SetConfiguration":
                        ZWaveLib.CommandClasses.Configuration.Set(ZWC.GetNode(_Request.node), Convert.ToByte(_Request.operation_vars[0]), Convert.ToInt32(_Request.operation_vars[1]));
                        break;

                    case "GetConfiguration":
                        ZWaveLib.CommandClasses.Configuration.Get(ZWC.GetNode(_Request.node), Convert.ToByte(_Request.operation_vars[0]));
                        break;

                    // Binary
                    case "SetBinary":
                        ZWaveLib.CommandClasses.SwitchBinary.Set(ZWC.GetNode(_Request.node), Convert.ToInt32(Convert.ToBoolean(_Request.operation_vars[0])));
                        break;

                    case "GetBinary":
                        ZWaveLib.CommandClasses.SwitchBinary.Get(ZWC.GetNode(_Request.node));
                        break;

                    // Basic
                    case "SetBasic":
                        ZWaveLib.CommandClasses.Basic.Set(ZWC.GetNode(_Request.node), Convert.ToInt32(_Request.operation_vars[0]));
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
                        byte[] MessageBytes = new byte[10];
                        MessageBytes[0] = 0x71;
                        MessageBytes[1] = 0x05;
                        MessageBytes[6] = Convert.ToByte(_Request.operation_vars[0]);
                        MessageBytes[7] = Convert.ToByte(_Request.operation_vars[1]);

                        ZWC.GetNode(_Request.node).SendDataRequest(MessageBytes);

                        break;

                    default:
                        Status.Clear();
                        Status.Add("type", "SystemStatus");
                        Status.Add("value", "Command : " + _Request.operation + " Not Implemented.");
                        Status.Add("color", "red");
                        Send(Status);
                        goto Exit;

                }

             

               
                

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
            ZWC.HealProgress += ZWC_HealProgress;
            

         




            Status["value"] = "Connecting to Controller ("+SerialPort+")...";
            Send(Status);


            ZWC.Connect();
        }

        private static void ZWC_HealProgress(object sender, ZWaveLib.HealProgressEventArgs args)
        {
            Dictionary<string, object> Status = new Dictionary<string, object>();

            if (args.Status == ZWaveLib.HealStatus.HealStart)
            {
                Status.Add("type", "SystemStatus");
                Status.Add("value", "Network Heal Started...");
                Status.Add("color", "yellow");
                Send(Status);
                Inited = false;
            }

            if (args.Status == ZWaveLib.HealStatus.HealEnd)
            {
                Status.Add("type", "SystemStatus");
                Status.Add("value", "ZWave Controller Ready");
                Status.Add("color", "green");
                Send(Status);
                Inited = true;
            }
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
            Dictionary<string, object> Status = new Dictionary<string, object>();

            if(args.Status == ZWaveLib.NodeQueryStatus.NodeAddReady)
            {
                Status.Add("type", "SystemStatus");
                Status.Add("value", "Please put target device into Inclusion mode.");
                Status.Add("color", "yellow");
                Send(Status);
            }

            if (args.Status == ZWaveLib.NodeQueryStatus.NodeRemoveReady)
            {
                Status.Add("type", "SystemStatus");
                Status.Add("value", "Please put target device into Exclusion mode.");
                Status.Add("color", "yellow");
                Send(Status);
            }

            if (args.Status == ZWaveLib.NodeQueryStatus.NodeAddDone || args.Status == ZWaveLib.NodeQueryStatus.NodeRemoveDone)
            {
                Status.Add("type", "SystemStatus");
                Status.Add("value", "ZWave Controller Ready");
                Status.Add("color", "green");
                Send(Status);
            }

            if(args.Status == ZWaveLib.NodeQueryStatus.NodeAdded)
            {
                Status.Add("type", "NodeMessage");
                Status.Add("node", args.NodeId);
                Status.Add("class", "NodeAdded");
                Status.Add("index", 0);
                Status.Add("value", args.NodeId);
                Status.Add("timestamp", args.Timestamp);

                Send(Status);
            }





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
