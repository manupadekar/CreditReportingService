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

        public CustomerServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int customerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Customer/{customerId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CustomerApiResponse>();
                    return result?.Data;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private class CustomerApiResponse
        {
            public bool Success { get; set; }
            public CustomerDto? Data { get; set; }
        }
    }
}

