using System.Threading.Tasks;

namespace ПрогЛабор.Services
{
    public interface IProtocolGenerator
    {
        Task<string> GenerateRawMaterialTestProtocolAsync(int testId, string outputPath);
        Task<string> GenerateQualityTestProtocolAsync(int testId, string outputPath);
    }
}