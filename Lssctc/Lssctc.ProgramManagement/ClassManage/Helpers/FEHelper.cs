using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.Helpers
{
    public class FEHelper
    {
        private static readonly Random _random = new Random();
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        // Method 1: Generate one unique exam code
        public static string GenerateExamCode(IEnumerable<string> existingCodes)
        {
            var codesSet = existingCodes as HashSet<string> ?? new HashSet<string>(existingCodes);
            while (true)
            {
                var code = new string(Enumerable.Repeat(Chars, 8)
                    .Select(s => s[_random.Next(s.Length)]).ToArray());

                if (!codesSet.Contains(code))
                    return code;
            }
        }

        // Method 2: Generate multiple unique exam codes efficiently
        public static List<string> GenerateExamCodes(IEnumerable<string> existingCodes, int count)
        {
            var codesSet = existingCodes as HashSet<string> ?? new HashSet<string>(existingCodes);
            var results = new List<string>();

            while (results.Count < count)
            {
                var code = new string(Enumerable.Repeat(Chars, 8)
                    .Select(s => s[_random.Next(s.Length)]).ToArray());

                // HashSet.Add returns true if the element is added (i.e., it was not present)
                if (codesSet.Add(code))
                {
                    results.Add(code);
                }
            }
            return results;
        }

        public static async Task CalculateFinalExamResultAsync(IUnitOfWork uow, int classId)
        {
            var exams = await uow.FinalExamRepository.GetAllAsQueryable()
               .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.PeChecklists)
               .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeSimulations).ThenInclude(s => s.SeTasks)
               .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeTheories)
               .Where(fe => fe.Enrollment.ClassId == classId)
               .ToListAsync();

            foreach (var exam in exams)
            {
                foreach (var partial in exam.FinalExamPartials)
                {
                    // Calculate based on type
                    switch (partial.Type)
                    {
                        case (int)FinalExamPartialType.Practical:
                            // PE: Calculate from checklists
                            var checklists = partial.PeChecklists.ToList();
                            if (checklists.Any())
                            {
                                int passed = checklists.Count(c => c.IsPass == true);
                                decimal marks = ((decimal)passed / checklists.Count) * 10;
                                partial.Marks = Math.Round(marks, 2);
                            }
                            else
                            {
                                partial.Marks = 0;
                            }

                            // Determine Pass based on Marks if not manually confirmed or just ensure consistency
                            // Assuming >= 5.0 is pass for PE if calculated
                            if (!partial.IsPass.HasValue || partial.Status != (int)FinalExamPartialStatus.Approved)
                            {
                                partial.IsPass = (partial.Marks ?? 0) >= 5;
                            }
                            break;

                        case (int)FinalExamPartialType.Simulation:
                            // SE: Calculate from SeTasks
                            var sim = partial.FeSimulations.FirstOrDefault();
                            if (sim != null && sim.SeTasks.Any())
                            {
                                var tasks = sim.SeTasks.ToList();
                                int passed = tasks.Count(t => t.IsPass == true);
                                decimal marks = ((decimal)passed / tasks.Count) * 10;
                                partial.Marks = Math.Round(marks, 2);
                                partial.IsPass = partial.Marks >= 5;
                            }
                            else
                            {
                                partial.Marks = 0;
                                partial.IsPass = false;
                            }
                            break;

                        case (int)FinalExamPartialType.Theory:
                            // TE: If not submitted/approved, mark as 0/Fail. 
                            // If already submitted, we trust the existing marks (usually from Quiz submission)
                            if (partial.Status == (int)FinalExamPartialStatus.NotYet)
                            {
                                partial.Marks = 0;
                                partial.IsPass = false;
                            }
                            break;
                    }

                    // Force Partial Status to Approved/Completed
                    if (partial.Status != (int)FinalExamPartialStatus.Approved)
                    {
                        partial.Status = (int)FinalExamPartialStatus.Approved;
                        if (!partial.CompleteTime.HasValue) partial.CompleteTime = DateTime.UtcNow;
                    }
                }

                // Recalculate Final Exam Total based on updated Partials
                decimal total = 0;
                foreach (var p in exam.FinalExamPartials)
                {
                    total += (p.Marks ?? 0) * ((p.ExamWeight ?? 0) / 100m);
                }
                exam.TotalMarks = Math.Round(total, 2);
                exam.IsPass = exam.TotalMarks >= 5;

                // Force Exam Status to Completed
                exam.Status = (int)FinalExamStatusEnum.Completed;
                if (!exam.CompleteTime.HasValue) exam.CompleteTime = DateTime.UtcNow;

                await uow.FinalExamRepository.UpdateAsync(exam);
            }
            await uow.SaveChangesAsync();
        }
    }
}