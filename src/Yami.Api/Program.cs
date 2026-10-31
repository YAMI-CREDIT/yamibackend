using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

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
const string CognitoAccessTokenScheme = "CognitoAccessToken";
const string CognitoIdTokenScheme = "CognitoIdToken";
const string RegistrationTokenPolicy = "RegistrationToken";

void ConfigureCognitoBearer(JwtBearerOptions options, string expectedTokenUse)
{
    options.MapInboundClaims = false;
    options.Authority = authority;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = authority,
        ValidateIssuerSigningKey = true,
        ValidateAudience = expectedTokenUse == "id",
        ValidAudience = appClientId,
        ValidateLifetime = true,
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var principal = context.Principal;
            var tokenUse = principal?.FindFirst("token_use")?.Value;
            var clientId = principal?.FindFirst("client_id")?.Value;

            if (tokenUse != expectedTokenUse || (expectedTokenUse == "access" && clientId != appClientId))
            {
                context.Fail("The Cognito token type or app client is invalid.");
            }

            return Task.CompletedTask;
        }
    };
}

builder.Services.AddUsers(builder.Configuration);
builder.Services.AddBusinesses(builder.Configuration);
builder.Services.AddAgreements(builder.Configuration);
builder.Services.AddKyc(builder.Configuration);
builder.Services.AddNotifications(builder.Configuration);

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

// Registration uses ID tokens; other protected API routes use access tokens.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CognitoAccessTokenScheme;
    options.DefaultChallengeScheme = CognitoAccessTokenScheme;
})
.AddJwtBearer(CognitoAccessTokenScheme, options =>
    ConfigureCognitoBearer(options, "access"))
.AddJwtBearer(CognitoIdTokenScheme, options =>
    ConfigureCognitoBearer(options, "id"));

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(CognitoAccessTokenScheme)
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy(RegistrationTokenPolicy, policy =>
        policy.AddAuthenticationSchemes(CognitoIdTokenScheme)
            .RequireAuthenticatedUser());
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// CORS: allow all origins for now so local dev (e.g. localhost:3000) can test
// against the API.
// Later: swap the two blocks below.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        // ACTIVE: allow all origins (testing from local development).
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();

        // LATER: restrict to the configured origins (https://yamicredit.com,
        // https://dev.yamicredit.com). Uncomment this block, comment out the
        // AllowAnyOrigin() block above.
        // var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        //     ?? [];
        // policy.WithOrigins(origins)
        //       .AllowAnyMethod()
        //       .AllowAnyHeader();
    });
});

// configure swagger
builder.Services.AddSwaggerGen(options =>
{
    options.DocumentFilter<FeatureControllerOrderDocumentFilter>();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT token below (no need to type 'Bearer')."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });

    options.SupportNonNullableReferenceTypes();
    // options.UseAllOfToExtendReferenceSchemas(); 
});

var app = builder.Build();


// DEBUG: Shows the authorization header that was received

// app.Use(async (context, next) =>
// {
//     Console.WriteLine($"Authorization header received: {context.Request.Headers["Authorization"]}");
//     await next();
// });

// For security reasons, this conditional ensures that the API documentation generated by swagger
// is only displayed in development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Yami API v1");
    });
}

// CORS must run BEFORE authentication: the browser's preflight (OPTIONS)
// request carries no Authorization header, so if auth ran first the
// preflight would be rejected with 401 and the real request would never fire.
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();