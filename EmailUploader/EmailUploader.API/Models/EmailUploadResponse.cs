namespace EmailUploader.API.Models
{
    public class EmailUploadResponse
    {
        public bool Success { get; set; }
        public int TotalEmailsInFile { get; set; }
        public int UniqueEmailsFound { get; set; }
        public int NewEmailsUploaded { get; set; }
        public string Message { get; set; }
    }
}