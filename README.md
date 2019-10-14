# shangqiserver

install guide:
1. Install Redis 
    
    Install the Chocolatey Redis package. https://chocolatey.org/packages/redis-64/
    
    Run redis-server from a command prompt.
2. Install MongoDB 
    https://www.mongodb.com/dr/fastdl.mongodb.org/win32/mongodb-win32-x86_64-2012plus-4.2.0-signed.msi/download
    
    create folder: C:\data\db
    
    cd to C:\Program Files\MongoDB\Server\\{mongodb_version}\bin>
    
    enter command mongod
        
    by default, mongodb server will start at port 27017


Start up:
1. Run Redis Server
	
	Run "redis-server" from a command line

2. Build the solution, and Go to bin folder of the ShangqiSocket project and Run 'dotnet ShangqiSocket.dll' for the socket server, default port is: 5000

3. Go to bin folder of the ShangqiApi project Run 'dotnet ShangqiApi.dll' for the API project. refer to the swagger UI for more details: https://localhost:5001/index.html