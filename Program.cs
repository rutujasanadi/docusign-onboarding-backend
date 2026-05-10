using System.Net.Http.Headers;
using docusign_onboarding_backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Register JWT service
builder.Services.AddSingleton<DocuSignJwtService>();

var app = builder.Build();

// CREATE ENVELOPE
app.MapPost("/start-onboarding", async (OnboardingRequest req, DocuSignJwtService jwt, IConfiguration config) =>
{

    string accountId = config["DocuSign:AccountId"];
    string templateId = config["DocuSign:TemplateId"];

    string accessToken = await jwt.GetAccessTokenAsync();

    var templateRoles = new List<object>
    {
        new {
            roleName = "HR Manager",
            name = "HR Manager",
            email = "rutujasanadi98@gmail.com",
            routingOrder = "1"
        },
        new {
            roleName = "Employee",
            name = req.EmployeeName,
            email = req.EmployeeEmail,
            routingOrder = "2"
        }
    };

    var managerLevels = new[] { "Manager", "Director", "VP" };
    if (managerLevels.Contains(req.JobLevel))
    {
        templateRoles.Add(new {
            roleName = "Dept Head",
            name = req.DeptHeadName,
            email = req.DeptHeadEmail,
            routingOrder = "3"
        });
    }

    var payload = new
    {
        emailSubject = "Employee Onboarding Documents",
        status = "sent",
        templateId = templateId,
        templateRoles = templateRoles
    };

    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", accessToken);

    try
    {
        Console.WriteLine("Sending request to DocuSign.");

        var response = await client.PostAsJsonAsync(
            $"https://demo.docusign.net/restapi/v2.1/accounts/{accountId}/envelopes",
            payload
        );

        Console.WriteLine("Received response from DocuSign.");

        var json = await response.Content.ReadAsStringAsync();
        return Results.Content(json, "application/json");
    }
    catch (TaskCanceledException)
    {
        return Results.Problem("DocuSign request timed out.");
    }
    catch (Exception ex)
    {
        return Results.Problem("Error: " + ex.Message);
    }
});

// CHECK ENVELOPE STATUS
app.MapGet("/envelope-status/{envelopeId}", async (string envelopeId, DocuSignJwtService jwt, IConfiguration config) =>
{
    string accountId = config["DocuSign:AccountId"];
    string accessToken = await jwt.GetAccessTokenAsync();

    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", accessToken);

    try
    {
        Console.WriteLine("Checking envelope status...");

        var response = await client.GetAsync(
            $"https://demo.docusign.net/restapi/v2.1/accounts/{accountId}/envelopes/{envelopeId}"
        );

        var result = await response.Content.ReadAsStringAsync();
        return Results.Content(result, "application/json");
    }
    catch (TaskCanceledException)
    {
        return Results.Problem("Status request timed out.");
    }
    catch (Exception ex)
    {
        return Results.Problem("Error: " + ex.Message);
    }
});

app.Run();

record OnboardingRequest(
    string EmployeeName,
    string EmployeeEmail,
    string JobLevel,
    string DeptHeadName,
    string DeptHeadEmail
);
