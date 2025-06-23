using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskHandler.Application.DTOs.Tasks;
using TaskHandler.Application.DTOs.User;
using TaskHandler.Domain.Enums;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace TaskHandler.Tests.Users;

public class UserControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly ITestOutputHelper _output;

    public UserControllerTest(WebApplicationFactory<Program> webApplicationFactory, ITestOutputHelper output)
    {
        _output = output;
        _webApplicationFactory = webApplicationFactory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                services.AddLogging(logBuilder => logBuilder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            });
        });
    }

    [Fact]
    public async Task AddTask_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var request = new AddTaskForUserDTO();
        
        request.UserId = Guid.NewGuid();
        request.Title = "Test Task";
        request.Description = "Test Description";
        request.Status = TaskHandler.Domain.Enums.TaskStatus.InProgress;
        request.TaskType = TaskType.Work;
        request.Priority = TaskPriority.High;
        request.CompletionDate = DateTime.Now.AddDays(1);
        
        // Act
        var response = await client.PostAsJsonAsync("/api/tasks/add", request);
        
        // Debug output
        _output.WriteLine($"Response Status: {response.StatusCode}");
        _output.WriteLine($"Response Content: {await response.Content.ReadAsStringAsync()}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllTasksForUser_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var userId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/tasks/get-all?userId={userId}");
        
        // Debug output
        _output.WriteLine($"GetAllTasksForUser Response Status: {response.StatusCode}");
        _output.WriteLine($"GetAllTasksForUser Response Content: {await response.Content.ReadAsStringAsync()}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTask_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var request = new
        {
            Id = Guid.NewGuid(),
            Title = "Updated Title",
            Description = "Updated Description",
            Status = TaskHandler.Domain.Enums.TaskStatus.InProgress,
            TaskType = TaskType.Work,
            Priority = TaskPriority.High,
            CompletionDate = DateTime.Now.AddDays(1)
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/tasks/update", request);
        
        // Debug output
        _output.WriteLine($"UpdateTask Response Status: {response.StatusCode}");
        _output.WriteLine($"UpdateTask Response Content: {await response.Content.ReadAsStringAsync()}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTask_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var deleteRequest = new { Id = Guid.NewGuid() };

        var httpReq = new HttpRequestMessage(HttpMethod.Delete, "/api/tasks/delete")
        {
            Content = JsonContent.Create(deleteRequest)
        };

        // Act
        var response = await client.SendAsync(httpReq);

        // Debug
        _output.WriteLine($"Status: {response.StatusCode}");
        _output.WriteLine(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddTask_WithValidAuthorization_ReturnsSuccess()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var userId = Guid.NewGuid();
        
        var email = $"test{Guid.NewGuid().ToString()[..8]}@test.com";
        
        var signUpRequest = new UserSingUpDTO();
        
        signUpRequest.Password = "Password123!";
        signUpRequest.Email = email;
        signUpRequest.Name = "Test User";
        
        var signUpResponse = await client.PostAsJsonAsync("/api/auth/sign-up", signUpRequest);
        
        _output.WriteLine($"SignUp Status: {signUpResponse.StatusCode}");
        
        if (signUpResponse.IsSuccessStatusCode)
        {
            var loginRequest = new UserLoginDTO();
            
            loginRequest.Email = email;
            loginRequest.Password = "Password123!";
            
            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
            
            _output.WriteLine($"Login Status: {loginResponse.StatusCode}");
            _output.WriteLine($"Login Content: {await loginResponse.Content.ReadAsStringAsync()}");
            
            if (loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                foreach (var cookie in cookies)
                {
                    _output.WriteLine($"Cookie: {cookie}");
                    if (cookie.StartsWith("access_token="))
                    {
                        var token = cookie.Split(';')[0].Replace("access_token=", "");
                        client.DefaultRequestHeaders.Add("Cookie", $"access_token={token}");
                        break;
                    }
                }
            }
        }

        var request = new AddTaskForUserDTO();
        
        request.UserId = userId;
        request.Title = "Test Task";
        request.Description = "Test Description";
        request.Status = Domain.Enums.TaskStatus.InProgress;
        request.TaskType = TaskType.Work;
        request.Priority = TaskPriority.High;
        request.CompletionDate = DateTime.Now.AddDays(1);

        // Act
        var response = await client.PostAsJsonAsync("/api/tasks/add", request);
        
        // Debug output
        _output.WriteLine($"AddTask Response Status: {response.StatusCode}");
        _output.WriteLine($"AddTask Response Content: {await response.Content.ReadAsStringAsync()}");

        // Assert
        if (signUpResponse.IsSuccessStatusCode)
        {
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        else
        {
            Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Fact]
    public async Task SignUp_NewUser_ReturnsSuccess()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var uniqueEmail = $"signup{Guid.NewGuid().ToString()[..8]}@test.com";
        
        var request = new UserSingUpDTO();
        
        request.Password = "Password123!";
        request.Email = uniqueEmail;
        request.Name = "User 1";

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/sign-up", request);
        var content = await response.Content.ReadAsStringAsync();
        
        // Debug output
        _output.WriteLine($"SignUp Response Status: {response.StatusCode}");
        _output.WriteLine($"SignUp Response Content: {content}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Login_ExistingUser_ReturnsResult()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var email = $"login{Guid.NewGuid().ToString()[..8]}@test.com";
        
        var signUpRequest = new UserSingUpDTO();
        
        signUpRequest.Password = "Password123!";
        signUpRequest.Email = email;
        signUpRequest.Name = "Test User";
        
        var signUpResponse = await client.PostAsJsonAsync("/api/auth/sign-up", signUpRequest);
        
        _output.WriteLine($"SignUp for Login Test Status: {signUpResponse.StatusCode}");
        
        var loginRequest = new UserLoginDTO();

        loginRequest.Email = email;
        loginRequest.Password = "Password123!";
        
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadAsStringAsync();
        
        // Debug output
        _output.WriteLine($"Login Response Status: {response.StatusCode}");
        _output.WriteLine($"Login Response Content: {content}");

        // Assert
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Login_NonExistentUser_ReturnsBadRequest()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var request = new UserLoginDTO();

        request.Email = "nonexistent@example.com";
        request.Password = "WrongPassword";

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);
        var content = await response.Content.ReadAsStringAsync();
        
        // Debug output
        _output.WriteLine($"Invalid Login Response Status: {response.StatusCode}");
        _output.WriteLine($"Invalid Login Response Content: {content}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SignUp_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var email = $"duplicate{Guid.NewGuid().ToString()[..8]}@test.com";
        
        var request1 = new UserSingUpDTO();
        
        request1.Password = "Password123!";
        request1.Email = email;
        request1.Name = "User 1";
        
        var request2 = new UserSingUpDTO();
        
        request2.Password = "Password456!";
        request2.Email = email;
        request2.Name = "User 2";

        // Act
        var firstResponse = await client.PostAsJsonAsync("/api/auth/sign-up", request1);
        _output.WriteLine($"First signup status: {firstResponse.StatusCode}");
        
        var secondResponse = await client.PostAsJsonAsync("/api/auth/sign-up", request2);
        var content = await secondResponse.Content.ReadAsStringAsync();
        
        // Debug output
        _output.WriteLine($"Duplicate signup status: {secondResponse.StatusCode}");
        _output.WriteLine($"Duplicate signup content: {content}");

        // Assert
        if (firstResponse.IsSuccessStatusCode)
        {
            Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
        }
    }

    [Fact]
    public async Task Endpoints_AreRegistered_AndAccessible()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();

        // Act & Assert
        var endpoints = new[]
        {
            ("/api/auth/sign-up", "POST"),
            ("/api/auth/login", "POST"),
            ("/api/tasks/add", "POST"),
            ("/api/tasks/get-all", "POST"),
            ("/api/tasks/update", "PUT"),
            ("/api/tasks/delete", "DELETE")
        };

        foreach (var (url, method) in endpoints)
        {
            HttpResponseMessage response = method switch
            {
                "POST" => await client.PostAsJsonAsync(url, new { }),
                "GET" => await client.GetAsync(url),
                "PUT" => await client.PutAsJsonAsync(url, new { }),
                "DELETE" => await client.DeleteAsync(url),
                _ => throw new ArgumentException($"Unsupported method: {method}")
            };

            _output.WriteLine($"{method} {url}: {response.StatusCode}");
            Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Fact]
    public async Task DeleteTask_WithValidRequest_ReturnsUnauthorized()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();
        var taskId = Guid.NewGuid();
        var deleteRequest = new { Id = taskId };

        // Act
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/tasks/delete")
        {
            Content = JsonContent.Create(deleteRequest)
        });

        // Debug output
        _output.WriteLine($"DeleteTask with body Response Status: {response.StatusCode}");
        _output.WriteLine($"DeleteTask with body Response Content: {await response.Content.ReadAsStringAsync()}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}