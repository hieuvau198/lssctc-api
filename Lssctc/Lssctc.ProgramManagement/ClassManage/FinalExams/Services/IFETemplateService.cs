using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
using Lssctc.Share.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public interface IFETemplateService
    {
        Task<IEnumerable<FinalExamTemplateDto>> GetTemplatesByClassIdAsync(int classId);
        Task ResetFinalExamAsync(int classId);
    }
}