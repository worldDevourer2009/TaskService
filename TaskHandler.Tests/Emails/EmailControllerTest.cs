using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TaskHandler.Application.DTOs;
using TaskHandler.Domain.Services;
using Xunit;
using Xunit.Abstractions;

namespace TaskHandler.Tests.Emails;

public class EmailControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;
    
    public EmailControllerTest(WebApplicationFactory<Program> factory, ITestOutputHelper outputHelper)
    {
        _output = outputHelper;
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IEmailSender>(sp =>
                {
                    var mockEmailSender = new Mock<IEmailSender>();
                    mockEmailSender
                        .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
                        .ReturnsAsync(true);
                    
                    return mockEmailSender.Object;
                });
            });
        });
    }

    [Fact]
    public async Task SendEmail_ReturnsOk()
    {
        //Arrange
        var client = _factory.CreateClient();
        var request = new EmailRequestDTO();
        
        request.Email = "helloWorld@gmail.com";
        request.Message = "Test";
        request.Subject = "Test";
        
        //Act
        var response = await client.PostAsJsonAsync("/api/email/send", request);
        
        //Debug
        _output.WriteLine($"Response Status: {response.StatusCode}");
        _output.WriteLine($"Response Content: {await response.Content.ReadAsStringAsync()}");
        
        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}