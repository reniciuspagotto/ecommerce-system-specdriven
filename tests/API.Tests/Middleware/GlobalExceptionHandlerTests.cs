using API.Middleware;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Xunit;

namespace API.Tests.Middleware;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task GlobalExceptionHandler_ShouldReturn400_ForProductValidationException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ProductValidationException("Product name cannot be empty.");
        var handler = new GlobalExceptionHandler();

        // Act
        await handler.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be(400);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("type").GetString().Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        problemDetails.GetProperty("title").GetString().Should().Be("Bad Request");
        problemDetails.GetProperty("status").GetInt32().Should().Be(400);
        problemDetails.GetProperty("detail").GetString().Should().Contain("Product name cannot be empty");
    }

    [Fact]
    public async Task GlobalExceptionHandler_ShouldReturn404_ForProductNotFoundException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ProductNotFoundException(productId);
        var handler = new GlobalExceptionHandler();

        // Act
        await handler.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be(404);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("type").GetString().Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
        problemDetails.GetProperty("title").GetString().Should().Be("Not Found");
        problemDetails.GetProperty("status").GetInt32().Should().Be(404);
        problemDetails.GetProperty("detail").GetString().Should().Contain(productId.ToString());
    }

    [Fact]
    public async Task GlobalExceptionHandler_ShouldReturn409_ForProductConcurrencyException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ProductConcurrencyException(productId, 2, 3);
        var handler = new GlobalExceptionHandler();

        // Act
        await handler.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be(409);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("type").GetString().Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.8");
        problemDetails.GetProperty("title").GetString().Should().Be("Conflict");
        problemDetails.GetProperty("status").GetInt32().Should().Be(409);
        problemDetails.GetProperty("detail").GetString().Should().Contain(productId.ToString());
        problemDetails.GetProperty("detail").GetString().Should().Contain("Expected version 2");
        problemDetails.GetProperty("detail").GetString().Should().Contain("current version is 3");
    }

    [Fact]
    public async Task GlobalExceptionHandler_ShouldReturn500_ForUnhandledException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new InvalidOperationException("Something went wrong");
        var handler = new GlobalExceptionHandler();

        // Act
        await handler.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be(500);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("type").GetString().Should().Be("https://tools.ietf.org/html/rfc7231#section-6.6.1");
        problemDetails.GetProperty("title").GetString().Should().Be("Internal Server Error");
        problemDetails.GetProperty("status").GetInt32().Should().Be(500);
        problemDetails.GetProperty("detail").GetString().Should().Be("An unexpected error occurred.");
    }
}
