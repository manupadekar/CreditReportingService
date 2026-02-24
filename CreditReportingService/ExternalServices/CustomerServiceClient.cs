using CreditReporting.Application.DTOs;
using CreditReporting.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CreditReportingService.ExternalServices
{
    public class CustomerServiceClient : ICustomerServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public CustomerServiceClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ExternalServices:CustomerApi"] ?? "https://localhost:7153";


        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int customerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/customers/{customerId}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CustomerApiResponse>();
                    return result?.Data;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Internal helper to match the response structure of CustomerService
        private class CustomerApiResponse
        {
            public bool Success { get; set; }
            public CustomerDto? Data { get; set; }
        }
    }
}
