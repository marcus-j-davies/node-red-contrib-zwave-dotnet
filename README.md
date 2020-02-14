# node-red-contrib-zwave-dotnet
An extremely easy to use, feature rich, ZWave node for node-red.

This node gives you the ability to interact with your ZWave devices right from node-red.
At the heart of this node, is a .Net Executable that manages all the required transport components for ZWave, the executable is based on the awesome zwave-lib-dotnet source code (https://github.com/genielabs/zwave-lib-dotnet)

The node is extremely easy to use, and only has 1 dependency - .Net/Mono. If running on a non windows platform,  
I recommend installing mono-complete, to ensure a fully functional framework is available.  

The node will recieve all events that are taking place in your ZWave network, and in turn will allow you to respond accordingly.

When an event has taken place, the following **payload** will be injected from the node.
Example of a battery level being recieved from a zwave device

```
{
  "node":7,
  "class":"Battery",
  "index":0,
  "value":78,
  "timestamp":"12-12-2017T12:23:23+000"
}
```
The payload above will be injected whenever something changes, and your USB ZWave controller is notified.
The node works by taking advantage of the ZWave Serial API - therefore if your ZWave radio is controlled via Serial, this node should work with it.

Another example where a configuration value has been received (parameter 31 in this case)
```
{
  "node":7,
  "class":"Configuration",
  "index":31,
  "value":2,
  "timestamp":"12-12-2017T12:23:23+000"
}
```
As well as receiving all events, you can also send commands to ZWave devices, currently it supports the following :

  - Multi Level Switch (Get, Set)
  - Thermostat Mode/Setpoint (Get,Set)
  - Wake Up Interval (Get, Set)
  - Configuration (Get, Set)
  - Binary Switch (Get, Set)
  - Basic Switch (Get, Set)
  - Battery (Get)
  - Notification Report Send (Yes! - You can emit zwave notification reports from your flow)
  
To construct a command, one only needs to build a simple javascript object (JSON)
```operation_vars``` is an array of arguments for a given command

Setting a value of 2 for the configuration parameter 31 operation_vars -> (31,2)
```
{
  "node":7,
  "operation":"SetConfiguration",
  "operation_vars":[0x1F,0x02],
}
```
Getting configuration parameter 31 operation_vars -> (31)
```
{
  "node":7,
  "operation":"GetConfiguration",
  "operation_vars":[0x1F],
}
```
And finally generating a Notification Report operation_vars -> (Type,Event)
```
{
  "node":7,
  "operation":"SendNotificationReport",
  "operation_vars":[0x07,0x03],
}
```

Want more?
The node also has a 'Raw Mode' meaning, if you're a wizard at constructing zwave packets, you can send them from your flow - allowing any number of commands to be issued to your ZWave network.

Example of building a notifcation report. 

```
{
  "node":7,
  "operation":"RawData",
  "raw":[0x71,0x5,0x0,0x0,0x0,0x0,0xA,0x2,0x0,0x0],
}
```

## Operation List
The 4 commands below do not require a node object, as the command is addressed to the controller its self.  
**StartNodeAdd**  
**StartNodeRemove**  
**StopNodeAdd**  
**StopNodeRemove**  
  
Basic controlling of zwave nodes  
**RawData** : raw [Byte Array]  
**SetMultiLevelSwitch** : operation_vars [Integer]  
**GetMultiLevelSwitch**  
**SetThermostatMode** : operation_vars [String] - (see Thermostat Modes)  
**GetThermostatMode**  
**SetThermostatSetPoint** : operation_vars [String, Double] - (see Thermostat Modes)  
**GetThermostatSetPoint** : operation_vars [String] - (see Thermostat Modes)  
**SetWakeInterval** : operation_vars [Integer] - (seconds)  
**GetWakeInterval**  
**SetConfiguration** : operation_vars [Byte, Integer] - (parameter,value)  
**GetConfiguration** : operation_vars [Byte] - (parameter)  
**SetBinary** : operation_vars [Boolean]  
**GetBinary**  
**SetBasic** : operation_vars [Integer]  
**GetBasic**  
**GetBattery**  
**SendNotificationReport** : operation_vars [Byte, Byte] - (Type,Event)  

Expert / Advanced Operations  
**DirectSerial** : See Below  

## Direct Serial (CAUTION!!)
WARNING! Using Direct Serial commands, bypasses all sanitisation offered by the Server/zwave lib - in essence, what you send, will be sent directly to your USB zwave controller. Sending an incorrect value, could, in theory harm/damage your controller and other related equipment if not used correctly. - **I am not responsable for any damage/harm caused to any piece of equipment/software as a result of using DirectSerial** 

Why would you use DirectSerial?
DirectSerial allows you to directly send data to the USB controller. such as configuring its Power Level and other configuration values related to the controller, that is othrwise not supported by the zwave lib. It requires that you know how to contstruct the paylaod that it expects.

Example?  
Disabling the LED on the Aeotec Gen5 Z Stick (you do not need to specify a node - remember, if a node id is required in any command, you have to ensure its correctly included/formatted in the raw data object.
```
{
  "operation":"DirectSerial",
  "raw":[0x01,0x08,0x00,0xF2,0x51,0x01,0x00,0x05,0x01,0x51]
}
```
The difference between **RawData** and **DirectSerial** is that RawData requires a valid zwave packet, and the node that it should be addressed to. DirectSerial on the other hand, is asking you to construct a serial api request. see (https://www.silabs.com/documents/login/user-guides/INS12350-Serial-API-Host-Appl.-Prg.-Guide.pdf)

## Thermostat Modes
Off  
Heat  
Cool  
Auto  
AuxHeat  
Resume  
FanOnly  
Furnace  
DryAir  
MoistAir  
AutoChangeover  
HeatEconomy  
CoolEconomy  
Away  

## Why?
All the solutions I have come across for connecting ZWave to node-red, involves various compiling of different libraries, and various configurations to take place. I am a very impatient person, so i decided to build my own solution, with the aim to make it far far easiyer compared to other solutions - not to discredit other solutions - they are awseome, just that, there is a lot more mileage involved to get them running.

## Installing
If you are running on windows - ensure you have .net 4.5 installed, or mono for other platforms.

Use the Node Red Palette menu or alternatively...

Within the .node-red directory, clone this repository

```
git clone https://github.com/marcus-j-davies/node-red-contrib-zwave-dotnet.git
```

Then install (you will need to restart node-red after)

```
npm install ./node-red-contrib-zwave-dotnet
```

## Configuration
There is only 1 configuration value that you need to amend, and that is the serial port address. Double click the node when its in your flow to modify it.

## Version History
  - 1.1.1  
    Fixed a potential issue where duplicated instances of the zwave router executable could be running.
    
  - 1.1.0  
    Added drop down for serial port, removing the need to enter it manually  
        
  - 1.0.0  
    Initial Release