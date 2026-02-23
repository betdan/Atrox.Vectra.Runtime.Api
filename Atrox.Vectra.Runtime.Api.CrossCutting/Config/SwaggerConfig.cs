namespace CrossCutting.Config
{
    using Microsoft.AspNetCore.Builder;

    public static class SwaggerConfig
    {
        public static IApplicationBuilder AddRegistration(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(url: "/swagger/v1/swagger.json", "Atrox.Vectra.Runtime.Api v1");
                options.DefaultModelsExpandDepth(depth: -1);
            });
            return app;
        }
    }
}


