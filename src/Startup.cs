﻿using System;
using BeautifulRestApi.Infrastructure;
using BeautifulRestApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AutoMapper;
using Swashbuckle.AspNetCore.Swagger;

namespace BeautifulRestApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddRouting(options => options.LowercaseUrls = true);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });

            services.AddDbContext<ApiDbContext>(options =>
            {
                // Use an in-memory database with a randomized database name (for testing)
                options.UseInMemoryDatabase("hieumemorydb");
            });

            services.AddMvc(opt =>
            {
                opt.Filters.Add(typeof(LinkRewritingFilter));
            })
            .AddJsonOptions(opt =>
            {
                // These should be the defaults, but we can be explicit:
                opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                opt.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                opt.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
            });

            services.AddAutoMapper(typeof(Startup));

            services.Configure<Models.PagingOptions>(Configuration.GetSection("DefaultPagingOptions"));

            // Services that consume EF Core objects (DbContext) should be registered as Scoped
            // (see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection#registering-your-own-services)
            services.AddScoped<IConversationService, DefaultConversationService>();
            services.AddScoped<ICommentService, DefaultCommentService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            using( var serviceScope = app.ApplicationServices.CreateScope()){
                var dbContext = serviceScope.ServiceProvider.GetService<ApiDbContext>();
                AddTestData(dbContext);
            }

            // Serialize all exceptions to JSON
            var jsonExceptionMiddleware = new JsonExceptionMiddleware(
                app.ApplicationServices.GetRequiredService<IHostingEnvironment>());
            app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = jsonExceptionMiddleware.Invoke });

            app.UseMvc();
        }

        private static void AddTestData(ApiDbContext context)
        {
            // TODO add Author to all of these:

            var conv1 = context.Conversations.Add(new Models.ConversationEntity
            {
                Id = Guid.Parse("6f1e369b-29ce-4d43-b027-3756f03899a1"),
                CreatedAt = DateTimeOffset.UtcNow,
                Title = "Who is the coolest Avenger?"
            }).Entity;

            var conv2 = context.Conversations.Add(new Models.ConversationEntity
            {
                Id = Guid.Parse("2d555f8f-e2a2-461e-b756-1f6d0d254b46"),
                CreatedAt = DateTimeOffset.UtcNow,
                Title = "How do we defeat Thanos?!"
            }).Entity;

            context.Comments.Add(new Models.CommentEntity
            {
                Id = Guid.Parse("653d061d-cdb9-423c-b03d-cd656ff567c7"),
                CreatedAt = DateTimeOffset.UtcNow,
                Conversation = conv1,
                Body = "Iron Man of course"
            });

            context.Comments.Add(new Models.CommentEntity
            {
                Id = Guid.Parse("4bfba838-5872-46be-a2df-db6db8aa261f"),
                CreatedAt = DateTimeOffset.UtcNow,
                Conversation = conv2,
                Body = "idk yet. something w/ the Infinity Stones?"
            });


            context.SaveChanges();
        }
    }
}