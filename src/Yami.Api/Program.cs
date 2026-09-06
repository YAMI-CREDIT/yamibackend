using Microsoft.AspNetCore.Authentication.JwtBearer;
using Asp.Versioning;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);






// Setup paramemeters to verify jwt in cognito
// Cognito config values are in appsettings.json. 
var awsRegion = builder.Configuration["Cognito:Region"]
    ?? throw new InvalidOperationException("Cognito Region is missing. Make sure it exists in appsettings.json");

var userPoolId = builder.Configuration["Cognito:UserPoolId"]
    ?? throw new InvalidOperationException("Cognito UserPoolId is missing. Make sure it exists in appsettings.json");

var appClientId = builder.Configuration["Cognito:ClientId"]
    ?? throw new InvalidOperationException("Cognito ClientId is missing. Make sure it exists in appsettings.json");

var authority = $"https://cognito-idp.{awsRegion}.amazonaws.com/{userPoolId}";





builder.Services.AddUsers(builder.Configuration);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// These lines setup JWT verification
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.Authority = authority; // ASP.NET Core auto-discovers JWKS from here
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = authority,
        ValidateAudience = true,
        ValidAudience = appClientId,
        ValidateLifetime = true,
    };


    // DEBUG: To troubleshoot jwt events
    // options.Events = new JwtBearerEvents
    // {
    //     OnAuthenticationFailed = context =>
    //     {
    //         Console.WriteLine($"Token validation failed: {context.Exception.Message}");
    //         return Task.CompletedTask;
    //     }
    // };





});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();


// DEBUG: Shows the authorization header that was received

// app.Use(async (context, next) =>
// {
//     Console.WriteLine($"Authorization header received: {context.Request.Headers["Authorization"]}");
//     await next();
// });


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();