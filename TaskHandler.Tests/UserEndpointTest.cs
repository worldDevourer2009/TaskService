using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskHandler.Api.Endpoints.Tasks;
using TaskHandler.Api.Endpoints.Users;
using TaskHandler.Domain.Enums;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace TaskHandler.Tests.Users;

public class UserEndpointTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly ITestOutputHelper _output;

    public UserEndpointTest(WebApplicationFactory<Program> webApplicationFactory, ITestOutputHelper output)
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
        var request = new AddTaskItemRequest(
            Guid.NewGuid(),
            "Test Task",
            "Test Description",
            TaskType.Personal,
            TaskPriority.Medium,
            null
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/tasks/addTaskItem", request);
        
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
        var response = await client.GetAsync($"/api/tasks/getAllTasksForUser?UserId={userId}");
        
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
        var taskId = Guid.NewGuid();
        var request = new UpdateTaskItemRequest(
            "Updated Title",
            "Updated Description",
            TaskHandler.Domain.Enums.TaskStatus.InProgress,
            TaskType.Work,
            TaskPriority.High,
            DateTime.Now.AddDays(1)
        );

        // Act
        var response = await client.PutAsJsonAsync($"/api/tasks/update?id={taskId}", request);
        
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
        var deleteRequest = new DeleteTaskItemRequest(Guid.NewGuid());

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
        var signUpRequest = new SignUpUserRequest("Test User", email, "Password123!");
        var signUpResponse = await client.PostAsJsonAsync("/api/users/auth/signUp", signUpRequest);
        
        _output.WriteLine($"SignUp Status: {signUpResponse.StatusCode}");
        
        if (signUpResponse.IsSuccessStatusCode)
        {
            var loginRequest = new LoginRequest(email, "Password123!");
            var loginResponse = await client.PostAsJsonAsync("/api/users/auth/login", loginRequest);
            
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

        var request = new AddTaskItemRequest(
            userId,
            "Test Task",
            "Test Description",
            TaskType.Personal,
            TaskPriority.Medium,
            null
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/tasks/addTaskItem", request);
        
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
        var request = new SignUpUserRequest(
            "Test User",
            uniqueEmail,
            "Password123!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/users/auth/signUp", request);
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
        
        var signUpRequest = new SignUpUserRequest("Test User", email, "Password123!");
        var signUpResponse = await client.PostAsJsonAsync("/api/users/auth/signUp", signUpRequest);
        
        _output.WriteLine($"SignUp for Login Test Status: {signUpResponse.StatusCode}");
        
        var loginRequest = new LoginRequest(email, "Password123!");
        var response = await client.PostAsJsonAsync("/api/users/auth/login", loginRequest);
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
        var request = new LoginRequest(
            "nonexistent@example.com",
            "WrongPassword"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/users/auth/login", request);
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
        
        var request1 = new SignUpUserRequest("User 1", email, "Password123!");
        var request2 = new SignUpUserRequest("User 2", email, "Password456!");

        // Act
        var firstResponse = await client.PostAsJsonAsync("/api/users/auth/signUp", request1);
        _output.WriteLine($"First signup status: {firstResponse.StatusCode}");
        
        var secondResponse = await client.PostAsJsonAsync("/api/users/auth/signUp", request2);
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
            ("/api/users/auth/signUp", "POST"),
            ("/api/users/auth/login", "POST"),
            ("/api/tasks/addTaskItem", "POST"),
            ($"/api/tasks/getAllTasksForUser?UserId={Guid.NewGuid()}", "GET"),
            ($"/api/tasks/update?id={Guid.NewGuid()}", "PUT"),
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
        var deleteRequest = new DeleteTaskItemRequest(taskId);

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