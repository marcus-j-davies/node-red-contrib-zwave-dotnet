module.exports = function (RED)
{
    function Init(config)
    {
        const node = this;
        RED.nodes.createNode(this, config);
        node.status({ fill: "yellow", shape: "dot", text: "Initializing server..." });

        const spawn = require('child_process').spawn;

        const os = require('os')
        const RL = require('readline')

        let ServerProcess;
        let LineReader;

        node.on('close', function ()
        {
          
            ServerProcess.kill('SIGTERM');
        });

        node.on('input', function (msg)
        {
            const Payload = JSON.stringify(msg.payload);
            ServerProcess.stdin.write(Payload + '\r\n');

        });

        const ENV = process.env;
        const EXEPath = __dirname + '/Server.exe';

        if (os.platform() == "win32")
        {
            ServerProcess = spawn(EXEPath, [], { env: ENV, })
        }
        else 
        {
            ServerProcess = spawn("mono", [EXEPath], { env: ENV, })
        }
       

        ServerProcess.stdout.setEncoding('utf8');

        LineReader = RL.createInterface({ input: ServerProcess.stdout });

        LineReader.on('line', function (line)
        {
            if (line == "READY")
            {
                ServerProcess.stdin.write('INIT\r\n');
                ServerProcess.stdin.write(config.serialPort + '\r\n');
            }
            else
            {
                ProcessMessage(line);
            }
            
        });
        
       

        function ProcessMessage(Payload)
        {
            const OBJ = JSON.parse(Payload);

          

            if (OBJ.type == "SystemStatus")
            {
                node.status({ fill: OBJ.color, shape: "dot", text: OBJ.value });
               
                
                
            }
            else
            {
                delete OBJ.type;
                node.send({ "payload": OBJ });
            }
        }
      
       
 
     
    }

   

    RED.nodes.registerType("zwave-dotnet", Init);

}