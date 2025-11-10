public class DicomImageDTO
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string OrthancId { get; set; }
    public Guid PatientId { get; set; }
}