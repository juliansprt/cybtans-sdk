using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using FluentValidation.AspNetCore;
using Cybtans.AspNetCore;
using Cybtans.Entities.EntityFrameworkCore;
using Cybtans.Services.Extensions;
using Cybtans.Tests.Services;
using System.Data.Common;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Cybtans.Tests.Domain.EF;
using Cybtans.Tests.Domain;

namespace Cybtans.Test.RestApi
{
    public class Startup
    {        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {         
            services.AddHttpContextAccessor();

            AddSwagger(services);

            AddAuthentication(services);

            #region Cors

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.SetIsOriginAllowedToAllowWildcardSubdomains()
                         .WithOrigins(Configuration.GetValue<string>("AllowedHosts").Split(','))
                         .AllowAnyHeader()
                         .AllowAnyMethod();
                    });
            });

            #endregion

            services.AddSingleton(AdventureContext.CreateInMemoryDatabase("RestAPI"));
            services.AddDbContext<AdventureContext>(
                (srv,builder) =>
                {
                    var conn = srv.GetRequiredService<DbConnection>();
                    builder.UseSqlite(conn);
                });

            services
            .AddUnitOfWork<AdventureContext>()
            .AddRepositories();

            services.AddAutoMapper(typeof(TestStub));
         
            services
            .AddControllers(options =>
            {
                options.Filters.Add<HttpResponseExceptionFilter>();
            })
            .AddFluentValidation(options => options.RegisterValidatorsFromAssemblyContaining(typeof(TestStub)))
            .AddCybtansFormatter();

            services.AddOptions<JwtOptions>().Bind(Configuration.GetSection("Jwt"));

            services.AddCybtansServices(typeof(TestStub).Assembly);

            services.AddSingleton<EntityEventDelegateHandler<OrderMessageHandler>>();
            services.AddTransient<OrderMessageHandler>();
            services.AddMessageHandler<CustomerEvent, Customer>();

            services.AddMessageQueue(Configuration)
             .ConfigureSubscriptions(sm =>
             {
                 sm.SubscribeHandlerForEvents<Order, OrderMessageHandler>("Test");
                 sm.SubscribeForEvents<CustomerEvent, Customer>("Test");

             });

            services.AddDbContextEventPublisher<AdventureContext>();

            services.AddTransient<AdventureContextSeed>();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            EfAsyncQueryExecutioner.Setup();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandlingMiddleware();
            }

            app.UseCors();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cybtans.Test V1");
                c.EnableFilter();
                c.EnableDeepLinking();
                c.ShowCommonExtensions();  
                
                c.OAuthClientId("swagger");
                c.OAuthClientSecret(Configuration.GetValue<string>("Identity:Secret"));
                c.OAuthAppName("Cybtans.Test");
                c.OAuthUsePkce();
            });
            
             app.UseReDoc(c =>
            {
                c.RoutePrefix = "docs";
                c.SpecUrl("/swagger/v1/swagger.json");
                c.DocumentTitle = "Cybtans.Test API";
            });
         

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        void AddSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cybtans.Test", Version = "v1" });
                c.OperationFilter<SwachBuckleOperationFilters>();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });


             

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {                  
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[0]
                    }
                });
            });
        }

        void AddAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnTokenValidated = async ctx =>
                    {

                    },
                    OnAuthenticationFailed = async cts =>
                    {

                    },
                    OnMessageReceived = async ctx =>
                    {

                    },
                    OnForbidden = async ctx =>
                    {

                    },
                    OnChallenge = async ctx =>
                    {

                    }
                };

                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = Configuration.GetValue<string>("Jwt:Audience"),
                    ValidIssuer = Configuration.GetValue<string>("Jwt:Issuer"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("Jwt:Secret"))),
                    ClockSkew = TimeSpan.Zero
                };
                options.Validate();
            });
            services.AddAuthorization();
		}
    }
}
