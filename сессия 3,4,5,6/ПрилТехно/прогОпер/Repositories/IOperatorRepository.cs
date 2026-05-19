using System;
using System.Collections.Generic;
using System.Text;
using прогОпер.Models;

namespace прогОпер.Repositories
{
    public interface IOperatorRepository
    {
        Task<List<ActiveBatchDto>> GetActiveBatchesAsync();
        Task<BatchDetailsDto?> GetBatchDetailsAsync(int batchId);
    }
}
