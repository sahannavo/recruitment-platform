using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig;

namespace RecruitmentAPI.Helpers
{
    public static class FileTextExtractor
    {
        public static async Task<string> ExtractTextAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension == ".txt")
            {
                using var reader = new StreamReader(file.OpenReadStream());
                return await reader.ReadToEndAsync();
            }
            else if (extension == ".pdf")
            {
                using var stream = file.OpenReadStream();
                using var document = PdfDocument.Open(stream);
                var stringBuilder = new StringBuilder();

                foreach (var page in document.GetPages())
                {
                    stringBuilder.AppendLine(page.Text);
                }

                return stringBuilder.ToString();
            }
            
            // For now, if it's not a text or pdf file
            throw new NotSupportedException($"Text extraction for file type '{extension}' is currently not supported. Please upload a .txt or .pdf file for AI parsing.");
        }
    }
}
