using AutoMapper;
using CreditReporting.Application.DTOs;
using CreditReporting.Application.Interfaces;
using CreditReporting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CreditReporting.Application.Services
{
    public class CibilService(IApplicationDbContext context, IMapper mapper, ILogger<CibilService> logger, ICustomerServiceClient customerServiceClient) : ICibilService
    {
        private readonly IApplicationDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<CibilService> _logger = logger;
        private readonly ICustomerServiceClient _customerServiceClient = customerServiceClient;
        private readonly Random _random = new Random();

        public async Task<CibilReportDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching CIBIL report by ID: {Id}", id);
            var report = await _context.CibilReports
                .FirstOrDefaultAsync(r => r.CibilId == id && !r.IsDeleted);
            return _mapper.Map<CibilReportDto>(report);
        }

        public async Task<CibilReportDto?> GetByPanNoAsync(string panNo)
        {
            _logger.LogInformation("Fetching CIBIL report by PAN: {PanNo}", panNo);
            var report = await _context.CibilReports
                .FirstOrDefaultAsync(r => r.PanNo == panNo && !r.IsDeleted);
            return _mapper.Map<CibilReportDto>(report);
        }

        public async Task<CibilReportDto?> GetByCustomerIdAsync(int customerId)
        {
            _logger.LogInformation("Fetching CIBIL report by CustomerId: {CustomerId}", customerId);

            var report = await _context.CibilReports
                .FirstOrDefaultAsync(r => r.CustomerId == customerId && !r.IsDeleted);

            if (report == null)
                return null;

            var dto = _mapper.Map<CibilReportDto>(report);

            var customer = await _customerServiceClient.GetCustomerByIdAsync(customerId);

            if (customer != null)
            {
                dto.AuthUserName = customer.AuthUserName;
                dto.UserId = customer.UserId;
                dto.Email = customer.Email;
            }

            return dto;
        }

        public async Task<IEnumerable<CibilReportDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all CIBIL reports");
            var reports = await _context.CibilReports
                .Where(r => !r.IsDeleted)
                .ToListAsync();
            var dtos = _mapper.Map<List<CibilReportDto>>(reports);

            foreach (var dto in dtos)
            {
                var customer = await _customerServiceClient.GetCustomerByIdAsync(dto.CustomerId);
                if (customer != null)
                {
                    dto.AuthUserName = customer.AuthUserName;
                    dto.UserId = customer.UserId;
                    dto.Email = customer.Email;
                }
            }

            return dtos;
        }

        public async Task<PaginatedList<CibilReportDto>> GetAllPagedAsync(int pageIndex, int pageSize)
        {
            _logger.LogInformation("Fetching paged CIBIL reports. Page: {Page}, Size: {Size}", pageIndex, pageSize);
            
            var query = _context.CibilReports.Where(r => !r.IsDeleted);
            var count = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<CibilReportDto>>(items);
            return new PaginatedList<CibilReportDto>(dtos, count, pageIndex, pageSize);
        }

        public async Task<CibilReportDto> CreateAsync(CibilCheckRequest request)
        {
            _logger.LogInformation("Generating new CIBIL report for Customer: {CustomerId}", request.CustomerId);

            var customer = await _customerServiceClient.GetCustomerByIdAsync(request.CustomerId);
            if (customer == null)
                throw new Exception("Customer not found");

            var existingReport = await _context.CibilReports
                .FirstOrDefaultAsync(r => r.PanNo == customer.PanNo && !r.IsDeleted);

            if (existingReport != null)
            {
                var existingDto = _mapper.Map<CibilReportDto>(existingReport);

                existingDto.UserId = customer.UserId;
                existingDto.AuthUserName = customer.AuthUserName;
                existingDto.Email = customer.Email;

                return existingDto;
            }

            var report = _mapper.Map<CibilReport>(request);
            report.CustomerId = customer.CustomerId;
            report.PanNo = customer.PanNo;
            report.CibilScore = _random.Next(300, 900);
            report.CreditHistory = $"Auto-generated for PAN: {customer.PanNo}";
            report.CheckDate = DateTime.UtcNow;
            report.Status = "success";

            await _context.CibilReports.AddAsync(report);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<CibilReportDto>(report);

            dto.UserId = customer.UserId;
            dto.AuthUserName = customer.AuthUserName;
            dto.Email = customer.Email;

            return dto;
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogWarning("Deleting CIBIL report ID: {Id}", id);
            var report = await _context.CibilReports.FindAsync(id);
            if (report != null)
            {
                report.IsDeleted = true;
                report.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}

