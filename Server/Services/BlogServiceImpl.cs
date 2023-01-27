using Blog;
using Grpc.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Blog.BlogService;

namespace Server.Services
{
    public class BlogServiceImpl : BlogServiceBase
    {
        private static MongoClient mongoClient = new MongoClient("mongodb://localhost:27017");
        private static IMongoDatabase mongoDatabse = mongoClient.GetDatabase("grpcBlog");
        private static IMongoCollection<BsonDocument> mongoCollection = mongoDatabse.GetCollection<BsonDocument>("blog");
        public override Task<CreateBlogResponse> CreateBlog(CreateBlogRequest request, ServerCallContext context)
        {
            var blog = request.Blog;

            BsonDocument doc = new BsonDocument("author_id", blog.AuthorId)
                                                .Add("title", blog.Title)
                                                .Add("content", blog.Content);
            mongoCollection.InsertOne(doc);

            string id = doc.GetValue("_id").ToString();

            blog.Id = id;

            return Task.FromResult(new CreateBlogResponse()
            {
                Blog = blog,
            });

        }

        public override async Task<ReadBlogResponse> ReadBlog(ReadBlogRequest request, ServerCallContext context)
        {
            var blogId = request.BlogId;

            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blogId));

            var result = mongoCollection.Find(filter).FirstOrDefault();

            if (result == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"The blog id {blogId} wasn't found."));
            }

            return await Task.FromResult(new ReadBlogResponse
            {
                Blog = new Blog.Blog
                {
                    AuthorId = result.GetValue("author_id").AsString,
                    Content = result.GetValue("content").AsString,
                    Title = result.GetValue("title").AsString
                }
            });

        }

        public override async Task<UpdateBlogResponse> UpdateBlog(UpdateBlogRequest request, ServerCallContext context)
        {
            var blogId = request.Blog.Id;
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blogId));

            var result = mongoCollection.Find(filter).FirstOrDefault();

            if (result == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "The blog id " + blogId + "wasn't found."));
            }

            var doc = new BsonDocument("author_id", request.Blog.AuthorId)
                          .Add("title", request.Blog.Title)
                          .Add("content", request.Blog.Content);

            mongoCollection.ReplaceOne(filter, doc);

            var blog = new Blog.Blog
            {
                AuthorId = doc.GetValue("author_id").AsString,
                Content = doc.GetValue("content").AsString,
                Title = doc.GetValue("title").AsString
            };
            blog.Id = blogId;

            return await Task.FromResult(new UpdateBlogResponse
            {
                Blog = blog
            });
        }

        public override async Task<DeleteBlogResponse> DeleteBlog(DeleteBlogRequest request, ServerCallContext context)
        {
            var blogId = request.BlogId;

            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blogId));

            var result = mongoCollection.Find(filter);
            if (result is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "The blog wasn't found"));
            }

            mongoCollection.DeleteOne(filter);

            return new DeleteBlogResponse() { BlogId = blogId };
        }

        public override async Task ListBlog(ListBlogRequest request, IServerStreamWriter<ListBlogResponse> responseStream, ServerCallContext context)
        {
            var filter = new FilterDefinitionBuilder<BsonDocument>().Empty;

            var result = mongoCollection.Find(filter);

            foreach (var blog in result.ToList())
            {

                var responseBlog = new Blog.Blog
                {
                    AuthorId = blog.GetValue("author_id").AsString,
                    Content = blog.GetValue("content").AsString,
                    Id = blog.GetValue("_id").ToString(),
                    Title = blog.GetValue("title").AsString
                };

                await responseStream.WriteAsync(new ListBlogResponse { Blog = responseBlog });
            }
        }
    }
}
