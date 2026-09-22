using AuthCore.Data;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;


var builder = WebApplication.CreateBuilder(args);

string clientId = builder.Configuration["OAuth:ClientId"]
    ?? throw new InvalidOperationException(
       "OAuth:ClientId is not configured.");

string clientSecret = builder.Configuration["OAuth:ClientSecret"]
    ?? throw new InvalidOperationException(
        "OAuth:ClientSecret is not configured.");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("AuthCoreDb"));

});

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
             .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/connect/token");
        options.AllowClientCredentialsFlow();
        options.AddDevelopmentEncryptionCertificate();
        options.AddDevelopmentSigningCertificate();

        options.UseAspNetCore().EnableTokenEndpointPassthrough();

    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    var existingClient = await applicationManager.FindByClientIdAsync(clientId);

    if (existingClient is null)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            ClientType = OpenIddictConstants.ClientTypes.Confidential,

            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials
            }
        };


        await applicationManager.CreateAsync(descriptor);
    }
    else
    {
        var descriptor = new OpenIddictApplicationDescriptor();

        await applicationManager.PopulateAsync(
            descriptor,
            existingClient
        );

        descriptor.Permissions.Clear();

        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.Endpoints.Token
        );

        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.GrantTypes.ClientCredentials
        );

        await applicationManager.UpdateAsync(
            existingClient,
            descriptor
        );
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
