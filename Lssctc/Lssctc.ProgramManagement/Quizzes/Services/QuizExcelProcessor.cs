using ExcelDataReader;
using Lssctc.ProgramManagement.Quizzes.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;

namespace Lssctc.ProgramManagement.Quizzes.Services
{
    public interface IQuizExcelProcessor
    {
        CreateQuizWithQuestionsDto ParseExcel(ImportQuizExcelDto dto);
    }

    public class QuizExcelProcessor : IQuizExcelProcessor
    {
        public CreateQuizWithQuestionsDto ParseExcel(ImportQuizExcelDto dto)
        {
            var ext = Path.GetExtension(dto.File.FileName).ToLower();
            if (ext != ".xlsx" && ext != ".xls") throw new ValidationException("Only .xlsx or .xls files allowed.");

            var resultDto = new CreateQuizWithQuestionsDto
            {
                Name = dto.Name,
                PassScoreCriteria = dto.PassScoreCriteria,
                TimelimitMinute = dto.TimelimitMinute,
                Description = dto.Description,
                Questions = new List<CreateQuizQuestionWithOptionsDto>()
            };

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var stream = dto.File.OpenReadStream();
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = (_) => new ExcelDataTableConfiguration { UseHeaderRow = true } });

            if (result.Tables.Count == 0) throw new ValidationException("Empty Excel file.");
            var table = result.Tables[0];
            if (table.Columns.Count < 7) throw new ValidationException("Missing columns. Need at least 7 columns.");

            var map = new Dictionary<string, CreateQuizQuestionWithOptionsDto>();
            int rowIdx = 1;

            foreach (DataRow row in table.Rows)
            {
                rowIdx++;
                var qName = row[0]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(qName)) continue;

                var scoreStr = row[1]?.ToString()?.Trim().Replace(",", ".");
                if (!decimal.TryParse(scoreStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qScore))
                    throw new ValidationException($"Row {rowIdx}: Invalid score '{scoreStr}'.");

                bool isMulti = IsTrue(row[2]?.ToString());
                var desc = row[3]?.ToString();
                var optName = row[4]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(optName)) continue;

                bool isCorrect = IsTrue(row[5]?.ToString());
                var explain = row[6]?.ToString();

                if (!map.ContainsKey(qName))
                {
                    var q = new CreateQuizQuestionWithOptionsDto
                    {
                        Name = qName,
                        QuestionScore = qScore,
                        IsMultipleAnswers = isMulti,
                        Description = desc,
                        Options = new List<CreateQuizQuestionOptionDto>()
                    };
                    map.Add(qName, q);
                    resultDto.Questions.Add(q);
                }

                map[qName].Options.Add(new CreateQuizQuestionOptionDto { Name = optName, IsCorrect = isCorrect, Explanation = explain });
            }

            if (resultDto.Questions.Count == 0) throw new ValidationException("No valid data found.");
            return resultDto;
        }

        private bool IsTrue(string? val) => val?.ToLower() is "true" or "1" or "yes";
    }
}