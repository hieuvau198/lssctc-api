using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public interface IFETemplateService
    {
        Task<FinalExamTemplateDto?> GetTemplatesByClassIdAsync(int classId);
        Task CreateTemplateAsync(int classId);
        Task UpdateTemplatePartialAsync(int classId, int type, decimal weight);
        Task ResetFinalExamAsync(int classId);

        Task<ClassExamConfigDto> GetClassExamConfigAsync(int classId);
        Task UpdatePartialsConfigForClassAsync(UpdateClassPartialConfigDto dto);
        Task UpdateClassExamWeightsAsync(int classId, UpdateClassWeightsDto dto);
    }
}