// See https://aka.ms/new-console-template for more information

using Blog;
using Grpc.Core;
using Server.Services;
using GrpcServer = Grpc.Core.Server;
Console.WriteLine("Hello, World!");

GrpcServer server = null;
const int Port = 50052;
try
{
    server = new GrpcServer()
    {
        Services = { BlogService.BindService(new BlogServiceImpl())},
        Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
    };

    server.Start();
    Console.WriteLine("The server is listening on ther port:" + Port);
    Console.ReadKey();
}
catch(IOException e)
{
    Console.WriteLine("The server failed to start :" + e.Message);
    throw;
}
finally
{
    if (server!=null)
    {
        server.ShutdownAsync().Wait();   
    }
}