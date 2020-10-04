module.exports = function (RED)
{
    const SP = require("serialport");
    const net = require('net');
    const spawn = require('child_process').spawn;
    const os = require('os')
    const nl = require('readline')
    
    function Init(config)
    {
        const node = this;
        RED.nodes.createNode(this, config);

        let Socket;
        let LR;
        let ServerProcess;

        node.status({ fill: "yellow", shape: "dot", text: "Starting server process..." });

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

        ServerProcess.stdout.on('data', (D) =>
        {
            node.status({ fill: "yellow", shape: "dot", text: "Connecting to server..." });
            Socket = net.createConnection(45342, '127.0.0.1', () =>
            {
                Socket.setNoDelay(true);
                LR = nl.createInterface(Socket);
                LR.on('line', ProcessMessage);
                Write(config.serialPort);

            });

        })

        function Write(Data)
        {
            Socket.write(Data);
            Socket.write("\n")
        }

        node.on('close', function (done)
        {
            Socket.destroy();
            ServerProcess.kill('SIGKILL');

            done();
        });

        node.on('input', function (msg)
        {
            const Payload = JSON.stringify(msg.payload);
            Write(Payload);
            

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

    RED.httpAdmin.get("/zwdngetports", RED.auth.needsPermission('serial.read'), function (req, res) {
        SP.list().then(
            ports => {
                const a = ports.map(p => p.path);
                res.json(a);
            },
            err => {
                node.log('Error listing serial ports', err)
            }
        )
    });

}