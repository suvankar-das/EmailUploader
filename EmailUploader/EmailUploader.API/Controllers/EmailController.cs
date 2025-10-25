using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using EmailUploader.API.Models;
using EmailUploader.API.Services;

namespace EmailUploader.API.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/email")]
    public class EmailController : ApiController
    {
        private readonly IEmailService _emailService;

        public EmailController()
        {
            _emailService = new EmailService();
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IHttpActionResult> Upload()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return BadRequest("Unsupported media type. Please send multipart/form-data.");
                }

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // Get the file
                var file = provider.Contents.FirstOrDefault();
                if (file == null)
                {
                    return BadRequest("No file uploaded.");
                }

                // Get file name
                var fileName = file.Headers.ContentDisposition.FileName?.Trim('\"');
                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("File name is missing.");
                }

                // Validate file extension
                if (!fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    return Content(HttpStatusCode.BadRequest, new EmailUploadResponse
                    {
                        Success = false,
                        Message = "Only .txt files are allowed."
                    });
                }

                // Read file content
                var fileContent = await file.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    return Content(HttpStatusCode.BadRequest, new EmailUploadResponse
                    {
                        Success = false,
                        Message = "File is empty."
                    });
                }

                // Extract all emails from file
                var allEmails = ExtractEmailsFromText(fileContent);
                var totalEmailsInFile = allEmails.Count;

                if (totalEmailsInFile == 0)
                {
                    return Ok(new EmailUploadResponse
                    {
                        Success = true,
                        TotalEmailsInFile = 0,
                        UniqueEmailsFound = 0,
                        NewEmailsUploaded = 0,
                        Message = "No email addresses found in the file."
                    });
                }

                // Get unique emails (case-insensitive)
                var uniqueEmails = allEmails
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var uniqueEmailsFound = uniqueEmails.Count;

                // Save to database using stored procedure
                var newEmailsUploaded = await _emailService.SaveUniqueEmails(uniqueEmails);

                return Ok(new EmailUploadResponse
                {
                    Success = true,
                    TotalEmailsInFile = totalEmailsInFile,
                    UniqueEmailsFound = uniqueEmailsFound,
                    NewEmailsUploaded = newEmailsUploaded,
                    Message = "Emails processed successfully."
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new EmailUploadResponse
                {
                    Success = false,
                    Message = "An error occurred: " + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("test")]
        public IHttpActionResult Test()
        {
            return Ok(new { Message = "API is working FINE!", Timestamp = DateTime.Now });
        }

        private List<string> ExtractEmailsFromText(string text)
        {
            var emails = new List<string>();

            // Email regex pattern
            var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
            var matches = Regex.Matches(text, emailPattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    emails.Add(match.Value.ToLower().Trim());
                }
            }

            return emails;
        }
    }
}
