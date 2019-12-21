# node-red-contrib-zwave-dot-net
An extremely easy to use, feature rich, ZWave node for node-red.

This node gives you the ability to interact with your ZWave devices right from node-red.
At the heart of this node, is a .Net Executable that manages all the required transport components for ZWave, the executable is based on the awesome zwave-lib-dotnet source code (https://github.com/genielabs/zwave-lib-dotnet)

The node is extremely easy to use, and only has 1 dependency (but only if you're a windows user)  
  - For windows, ensure you have .Net 4.6.1 installed  
  - For Ubunto and alike, there is no need to have Mono installed. I have been kind enough to package it all in a native binary.
  

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

# Operation List
**RawData** : raw[Byte Array]  
**SetMultiLevelSwitch** : operation_vars[Integer]  
**GetMultiLevelSwitch**  
**SetThermostatMode** : operation_vars[String]  
**GetThermostatMode**  
**SetThermostatSetPoint** : operation_vars[String, Double]  
**GetThermostatSetPoint** : operation_vars[String]  
**SetWakeInterval** : operation_vars[Integer] (seconds)  
**GetWakeInterval**  
**SetConfiguration** : operation_vars[Byte, Integer] (Parameter, Value)  
**SetConfiguration** : operation_vars[Byte] (Parameter)  
**SetBinary** : operation_vars[Boolean]  
**GetBinary**  
**SetBasic** : operation_vars[Integer]  
**GetBasic**  
**GetBattery**  
**SendNotificationReport** : operation_vars[Byte, Byte] (Type, Event)  

# Why?
All the solutions I have come across for connecting ZWave to node-red, involves various compiling of different libraries, and various configurations to take place. I am a very impatient person, so i decided to build my own solution, with the aim to make it far far easiyer compared to other solutions - not to discredit other solutions - they are awseome, just that, there is a lot more mileage involved to get them running.

# Installing
If you are running on windows - ensure you have .net 4.5 installed, for Ubunto and alike there should be no dependencies

Then within the .node-red directory, clone this repp

```
git clone https://github.com/marcus-j-davies/node-red-contrib-zwave-dot-net.git
```

Then install (you will need to restart node-red after)

```
npm install ./node-red-contrib-zwave-dot-net
```

# Configuration
There is only 1 configuration value that you need to amend, and that is the serial port address. Double click the node when its in your flow to modify it.
