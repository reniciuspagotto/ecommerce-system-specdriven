using API.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace API.Tests.Integration;

public class DeleteProductIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DeleteProductIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeleteProduct_EndToEnd_ShouldSucceed()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Create a product
        var createRequest = new CreateProductRequest("Product to Delete", 99.99m, 100);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Verify product exists
        var getBeforeDelete = await client.GetAsync($"/api/products/{productId}");
        getBeforeDelete.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Delete the product
        var deleteResponse = await client.DeleteAsync($"/api/products/{productId}");

        // Assert - Delete successful
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify product no longer exists
        var getAfterDelete = await client.GetAsync($"/api/products/{productId}");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_NonExistent_ShouldReturnNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/products/{nonExistentId}");

        // Assert - Idempotent behavior
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteProduct_AfterUpdate_ShouldStillDelete()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Create product
        var createRequest = new CreateProductRequest("Original Product", 50m, 25);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Update product
        var updateRequest = new { name = "Updated Product", price = 75m, quantity = 30, version = 1 };
        var updateResponse = await client.PutAsJsonAsync($"/api/products/{productId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Delete updated product
        var deleteResponse = await client.DeleteAsync($"/api/products/{productId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await client.GetAsync($"/api/products/{productId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_CannotRetrieveOrUpdate_AfterDeletion()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var createRequest = new CreateProductRequest("Product", 100m, 50);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Delete product
        var deleteResponse = await client.DeleteAsync($"/api/products/{productId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act & Assert - Cannot retrieve
        var getResponse = await client.GetAsync($"/api/products/{productId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Act & Assert - Cannot update
        var updateRequest = new { name = "Updated", price = 150m, quantity = 75, version = 1 };
        var updateResponse = await client.PutAsJsonAsync($"/api/products/{productId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_MultipleProducts_ShouldDeleteOnlySpecifiedOne()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Create multiple products
        var product1Response = await client.PostAsJsonAsync("/api/products", 
            new CreateProductRequest("Product 1", 100m, 10));
        var product2Response = await client.PostAsJsonAsync("/api/products", 
            new CreateProductRequest("Product 2", 200m, 20));
        var product3Response = await client.PostAsJsonAsync("/api/products", 
            new CreateProductRequest("Product 3", 300m, 30));

        var product1 = JsonSerializer.Deserialize<JsonElement>(await product1Response.Content.ReadAsStringAsync());
        var product2 = JsonSerializer.Deserialize<JsonElement>(await product2Response.Content.ReadAsStringAsync());
        var product3 = JsonSerializer.Deserialize<JsonElement>(await product3Response.Content.ReadAsStringAsync());

        var id1 = product1.GetProperty("id").GetGuid();
        var id2 = product2.GetProperty("id").GetGuid();
        var id3 = product3.GetProperty("id").GetGuid();

        // Act - Delete product 2
        var deleteResponse = await client.DeleteAsync($"/api/products/{id2}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert - Product 2 deleted
        var get2Response = await client.GetAsync($"/api/products/{id2}");
        get2Response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Assert - Products 1 and 3 still exist
        var get1Response = await client.GetAsync($"/api/products/{id1}");
        get1Response.StatusCode.Should().Be(HttpStatusCode.OK);

        var get3Response = await client.GetAsync($"/api/products/{id3}");
        get3Response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
