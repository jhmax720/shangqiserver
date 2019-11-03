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

2. Make sure the solution builds ok.

3. Run the following bash files:

   1.run_build_all.bat

   2.run_socket.bat

   3.run_api.bat

   4.run_socket_client.bat (optional, if you are testing with a fake client)