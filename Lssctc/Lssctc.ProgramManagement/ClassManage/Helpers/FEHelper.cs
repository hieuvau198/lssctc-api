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
    }
}