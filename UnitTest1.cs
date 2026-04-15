using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace FoodyTests
{
    [TestFixture]
    public class FoodTests
    {
        private RestClient client;
        private static string lastCreatedFoodId;

        private const string BaseUrl = "http://144.91.123.158:81";

        private const string Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiIxMjc1NTk4OS0wZjMwLTQ1ZGQtODRkOS0yOGRhMzg4MDZkZDAiLCJpYXQiOiIwNC8xNS8yMDI2IDA5OjAwOjQzIiwiVXNlcklkIjoiMDZmMDQwYTMtYWZhMy00MzAxLTc0N2MtMDhkZTc2OGU4Zjk3IiwiRW1haWwiOiJ2ZXNlbGEucGF2bG92YUBleGFtcGxlLmNvbSIsIlVzZXJOYW1lIjoidmVzZWxhX3BhdmxvdmEiLCJleHAiOjE3NzYyNjUyNDMsImlzcyI6IkZvb2R5X0FwcF9Tb2Z0VW5pIiwiYXVkIjoiRm9vZHlfV2ViQVBJX1NvZnRVbmkifQ.D4fHTFdYe660odjpwcwqn2CvE896w87PHU0-HrhDF4M";

        [OneTimeSetUp]
        public void Setup()
        {
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(Token)
            };

            client = new RestClient(options);
        }

        [Test, Order(1)]
        public void CreateFood_WithRequiredFields_ShouldReturnCreated()
        {
            var food = new
            {
                Name = "Test Food",
                Description = "Test Description",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);

            var response = client.Execute(request);
            var data = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(data.GetProperty("foodId").GetString(), Is.Not.Null);

            lastCreatedFoodId = data.GetProperty("foodId").GetString();
        }

        [Test, Order(2)]
        public void EditFood_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/api/Food/Edit/{lastCreatedFoodId}", Method.Patch);

            var body = new[]
            {
                new { path = "/name", op = "replace", value = "Edited Food" }
            };

            request.AddJsonBody(body);

            var response = client.Execute(request);
            var data = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(data.GetProperty("msg").GetString(), Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllFoods_ShouldReturnNonEmptyList()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);

            var response = client.Execute(request);
            var data = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(data.GetArrayLength(), Is.GreaterThan(0));
        }

        [Test, Order(4)]
        public void DeleteFood_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/api/Food/Delete/{lastCreatedFoodId}", Method.Delete);

            var response = client.Execute(request);
            var data = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(data.GetProperty("msg").GetString(), Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateFood_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var food = new
            {
                Name = "",
                Description = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingFood_ShouldReturnNotFound()
        {
            var request = new RestRequest("/api/Food/Edit/999999", Method.Patch);

            var body = new[]
            {
                new { path = "/name", op = "replace", value = "Test" }
            };

            request.AddJsonBody(body);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test, Order(7)]
        public void DeleteNonExistingFood_ShouldReturnNotFound()
        {
            var request = new RestRequest("/api/Food/Delete/999999", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            client?.Dispose();
        }
    }
}