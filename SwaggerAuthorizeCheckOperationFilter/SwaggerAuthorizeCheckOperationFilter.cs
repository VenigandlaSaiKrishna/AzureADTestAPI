using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureADTestAPI.SwaggerAuthorizeCheckOperationFilter
{
    public class SwaggerAuthorizeCheckOperationFilter : IOperationFilter
    {
        private readonly IConfiguration _config;

        public SwaggerAuthorizeCheckOperationFilter(IConfiguration config)
        {
            _config = config;
        }
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
                               || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (hasAuthorize)
            {
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

                var oAuthScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                };

                operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    [ oAuthScheme ] = new[] { _config["ArticleMasterEntraAD:Scopes"] },
                }
            };
            }
        }
    }

}
