using AzureADTestAPI.SwaggerAuthorizeCheckOperationFilter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("ArticleMasterEntraAD", options);
    }, options =>
    {
        builder.Configuration.Bind("ArticleMasterEntraAD", options);
    });

builder.Services.AddAuthorization(options =>
{
    //use this option for swagger
    options.AddPolicy("AuthPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scp", "API.ReadWrite");
    });
    //use this option for exposing to Client App's
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(
    c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Test API", Version = "v1" });
        c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{builder.Configuration["ArticleMasterEntraAD:Instance"]}{builder.Configuration["ArticleMasterEntraAD:TenantId"]}/oauth2/v2.0/authorize"),
                    TokenUrl = new Uri($"{builder.Configuration["ArticleMasterEntraAD:Instance"]}{builder.Configuration["ArticleMasterEntraAD:TenantId"]}/oauth2/v2.0/token"),
                    RefreshUrl = new Uri($"{builder.Configuration["ArticleMasterEntraAD:Instance"]}{builder.Configuration["ArticleMasterEntraAD:TenantId"]}/oauth2/v2.0/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { $"{builder.Configuration["ArticleMasterEntraAD:Audience"]}/.default", "Access the API" }
                    }
                }
            }
        });
        c.OperationFilter<SwaggerAuthorizeCheckOperationFilter>();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arcticle Master API v1");
    //for developer use only
    c.OAuthClientId(builder.Configuration["ArticleMasterEntraAD:ClientId"]);
    c.OAuthUsePkce();
});
//}


app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

