namespace docusign_onboarding_backend.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

public class DocuSignJwtService
{
    // ⭐ REQUIRED — Replace with your Integration Key (Client ID)
    private readonly string IntegrationKey = "9d694b31-a18f-45de-bbd1-99298120beec";

    // ⭐ REQUIRED — Replace with your API Username (User GUID)
    private readonly string UserId = "1d3a191b-f293-40c0-bd29-6ae1e8b6111a";

    // DocuSign demo auth server
    private readonly string AuthServer = "account-d.docusign.com";

    // Path to your private key file
    private readonly string PrivateKeyPath = "private.key";

    public async Task<string> GetAccessTokenAsync()
    {
        // 1. Load private key
        var privateKeyText = File.ReadAllText(PrivateKeyPath);

        var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyText.ToCharArray());
        var securityKey = new RsaSecurityKey(rsa);

        // 2. Create signing credentials
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

        // 3. Build JWT assertion
        var tokenHandler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = IntegrationKey,
            Audience = AuthServer,
            Expires = now.AddMinutes(5),
            SigningCredentials = creds,
            Claims = new Dictionary<string, object>
            {
                { "sub", UserId },
                { "scope", "signature impersonation" }
            }
        };

        var jwt = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        var assertion = tokenHandler.WriteToken(jwt);

        // 4. Exchange JWT for access token
        using var client = new HttpClient();
        var body = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string,string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
            new KeyValuePair<string,string>("assertion", assertion)
        });

        var response = await client.PostAsync($"https://{AuthServer}/oauth/token", body);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception("JWT authentication failed: " + json);

        // 5. Extract access token
        var doc = System.Text.Json.JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("access_token").GetString();
    }
}
