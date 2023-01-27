// See https://aka.ms/new-console-template for more information
using Blog;
using Grpc.Core;

Console.WriteLine("Hello, World!");

Channel channel = new Channel("localhost", 50052, ChannelCredentials.Insecure);

await channel.ConnectAsync().ContinueWith((task) =>
{
    if (task.Status == TaskStatus.RanToCompletion)
    {
        Console.WriteLine("The client connected successfully");
    }
});

var client = new BlogService.BlogServiceClient(channel);

//CreateBlog(client);

//ReadBlog(client);

//UpdateBlog(client);

//DeleteBlog(client);

await GetBlogs(client);


channel.ShutdownAsync().Wait();
Console.ReadKey();

static void CreateBlog(BlogService.BlogServiceClient client)
{
    var response = client.CreateBlog(new CreateBlogRequest()
    {
        Blog = new Blog.Blog
        {
            AuthorId = "Milad",
            Title = "Grpc",
            Content = "Grpc uses protobuf"
        }
    });
    Console.WriteLine($"The blog {response.Blog.Id} was created");
}

static void UpdateBlog(BlogService.BlogServiceClient client)
{
    try
    {
        var response = client.UpdateBlog(new UpdateBlogRequest()
        {
            Blog = new Blog.Blog
            {
                Id = "63d3aef6c910c52667ffc850",
                AuthorId = "Saeideh",
                Title = "Data Science",
                Content = "DataScience is the most new Tech in the world"
            }
        });
        Console.WriteLine($"The blog {response.Blog.Id} was updated");
    }
    catch (RpcException e)
    {
        Console.WriteLine(e.Status.Detail);
    }

}

static void DeleteBlog(BlogService.BlogServiceClient client)
{
    try
    {
        var response = client.DeleteBlog(new DeleteBlogRequest()
        {
            BlogId = "63d3aef6c910c52667ffc850",
        });
        Console.WriteLine($"The blog {response.BlogId} was deleted");
    }
    catch (RpcException e)
    {
        Console.WriteLine(e.Status.Detail);
    }

}

static void ReadBlog(BlogService.BlogServiceClient client)
{
    try
    {
        var response = client.ReadBlog(new ReadBlogRequest
        {
            BlogId = "63d3a7a0d713c8aea996e4b0"
        });
        Console.WriteLine(response.Blog.ToString());
    }
    catch (RpcException e)
    {
        Console.WriteLine(e.Status.Detail);
    }
}

static async Task GetBlogs(BlogService.BlogServiceClient client)
{
    var response = client.ListBlog(new ListBlogRequest() { });

    while (await response.ResponseStream.MoveNext())
    {
        var blog = response.ResponseStream.Current.Blog;
        Console.WriteLine(blog);
    }
}