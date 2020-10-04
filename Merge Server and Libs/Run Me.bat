cd %cd%
del "..\Server Project (VS2015)\Server\bin\x64/Release\ZWaveLib.pdb"
del "..\Server Project (VS2015)\Server\bin\x64\Release\SerialPortLib.pdb"
ILMerge.exe /out:Server-Merged.exe "../Server Project (VS2015)/Server/bin/x64/Release/Server.exe" "../Server Project (VS2015)/Server/bin/x64/Release/NLog.dll" "../Server Project (VS2015)/Server/bin/x64/Release/Newtonsoft.Json.dll" "../Server Project (VS2015)/Server/bin/x64/Release/SerialPortLib.dll" "../Server Project (VS2015)/Server/bin/x64/Release/ZWaveLib.dll"