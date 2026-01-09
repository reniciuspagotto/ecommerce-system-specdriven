using API.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace API.Tests.Integration;

public class ConcurrentUpdateIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ConcurrentUpdateIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ConcurrentUpdate_SecondUpdateWithStaleVersion_ShouldFail()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Create a product
        var createRequest = new CreateProductRequest("Product", 100m, 50);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // User A and User B both fetch version 1
        var userAVersion = 1;
        var userBVersion = 1;

        // Act - User A updates first (version 1 -> 2)
        var userAUpdate = new { name = "Updated by A", price = 110m, quantity = 45, version = userAVersion };
        var userAResponse = await client.PutAsJsonAsync($"/api/products/{productId}", userAUpdate);
        
        userAResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var userAContent = await userAResponse.Content.ReadAsStringAsync();
        var userAResult = JsonSerializer.Deserialize<JsonElement>(userAContent);
        userAResult.GetProperty("version").GetInt32().Should().Be(2);

        // Act - User B tries to update with stale version 1
        var userBUpdate = new { name = "Updated by B", price = 120m, quantity = 40, version = userBVersion };
        var userBResponse = await client.PutAsJsonAsync($"/api/products/{productId}", userBUpdate);

        // Assert - User B's update should fail with 409 Conflict
        userBResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        var userBContent = await userBResponse.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(userBContent);
        
        problemDetails.GetProperty("status").GetInt32().Should().Be(409);
        problemDetails.GetProperty("title").GetString().Should().Be("Conflict");
        problemDetails.GetProperty("detail").GetString().Should().Contain("version");
    }

    [Fact]
    public async Task ConcurrentUpdate_RefetchAndRetry_ShouldSucceed()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var createRequest = new CreateProductRequest("Product", 100m, 50);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // User A updates (version 1 -> 2)
        var userAUpdate = new { name = "Updated by A", price = 110m, quantity = 45, version = 1 };
        await client.PutAsJsonAsync($"/api/products/{productId}", userAUpdate);

        // User B tries with stale version and fails
        var userBFirstAttempt = new { name = "Updated by B", price = 120m, quantity = 40, version = 1 };
        var firstAttemptResponse = await client.PutAsJsonAsync($"/api/products/{productId}", userBFirstAttempt);
        firstAttemptResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // User B refetches current state
        var refetchResponse = await client.GetAsync($"/api/products/{productId}");
        var refetchContent = await refetchResponse.Content.ReadAsStringAsync();
        var refetchedProduct = JsonSerializer.Deserialize<JsonElement>(refetchContent);
        var currentVersion = refetchedProduct.GetProperty("version").GetInt32();
        
        currentVersion.Should().Be(2);

        // Act - User B retries with correct version
        var userBRetry = new { name = "Updated by B - Retry", price = 120m, quantity = 40, version = currentVersion };
        var retryResponse = await client.PutAsJsonAsync($"/api/products/{productId}", userBRetry);

        // Assert - Retry should succeed
        retryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var retryContent = await retryResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(retryContent);
        
        result.GetProperty("name").GetString().Should().Be("Updated by B - Retry");
        result.GetProperty("version").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task ConcurrentUpdate_ThreeUsersSequential_ShouldMaintainVersionIntegrity()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var createRequest = new CreateProductRequest("Product", 100m, 50);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Act - Three users update sequentially
        // User 1
        var user1Update = new { name = "User 1", price = 101m, quantity = 51, version = 1 };
        var user1Response = await client.PutAsJsonAsync($"/api/products/{productId}", user1Update);
        user1Response.StatusCode.Should().Be(HttpStatusCode.OK);

        // User 2
        var user2Update = new { name = "User 2", price = 102m, quantity = 52, version = 2 };
        var user2Response = await client.PutAsJsonAsync($"/api/products/{productId}", user2Update);
        user2Response.StatusCode.Should().Be(HttpStatusCode.OK);

        // User 3
        var user3Update = new { name = "User 3", price = 103m, quantity = 53, version = 3 };
        var user3Response = await client.PutAsJsonAsync($"/api/products/{productId}", user3Update);
        user3Response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - Final state
        var finalResponse = await client.GetAsync($"/api/products/{productId}");
        var finalContent = await finalResponse.Content.ReadAsStringAsync();
        var finalProduct = JsonSerializer.Deserialize<JsonElement>(finalContent);
        
        finalProduct.GetProperty("name").GetString().Should().Be("User 3");
        finalProduct.GetProperty("price").GetDecimal().Should().Be(103m);
        finalProduct.GetProperty("quantity").GetInt32().Should().Be(53);
        finalProduct.GetProperty("version").GetInt32().Should().Be(4);
    }
}
