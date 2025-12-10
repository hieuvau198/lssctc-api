using Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.Quizzes.Services
{
    public static class QuizExtensions
    {
        public static IQueryable<Quiz> ApplySearch(this IQueryable<Quiz> query, string? term)
        {
            if (string.IsNullOrWhiteSpace(term)) return query;
            var lower = term.Trim().ToLower();
            return query.Where(q => (q.Name != null && q.Name.ToLower().Contains(lower)) || (q.Description != null && q.Description.ToLower().Contains(lower)));
        }

        public static IQueryable<Quiz> ApplySort(this IQueryable<Quiz> query, string? sortBy, string? dir)
        {
            if (string.IsNullOrWhiteSpace(sortBy)) return query.OrderByDescending(q => q.CreatedAt);
            bool isDesc = dir?.ToLower() == "desc";

            return sortBy.ToLower() switch
            {
                "timelimit" => isDesc ? query.OrderByDescending(q => q.TimelimitMinute) : query.OrderBy(q => q.TimelimitMinute),
                "passscore" => isDesc ? query.OrderByDescending(q => q.PassScoreCriteria) : query.OrderBy(q => q.PassScoreCriteria),
                _ => isDesc ? query.OrderByDescending(q => q.CreatedAt) : query.OrderBy(q => q.CreatedAt)
            };
        }
    }
}