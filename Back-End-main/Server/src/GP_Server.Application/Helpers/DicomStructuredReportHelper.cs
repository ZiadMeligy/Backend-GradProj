using FellowOakDicom;
using System.Text;

namespace GP_Server.Application.Helpers
{
    public static class DicomStructuredReportHelper
    {
        /// <summary>
        /// Generates a DICOM Structured Report (SR) from the provided report text
        /// </summary>
        /// <param name="reportText">The generated report text</param>
        /// <param name="patientId">Patient ID from original study</param>
        /// <param name="patientName">Patient name from original study</param>
        /// <param name="studyInstanceUid">Study Instance UID to associate the SR with</param>
        /// <param name="originalInstanceUid">The original instance UID that was analyzed</param>
        /// <returns>DICOM SR as byte array</returns>
        public static async Task<byte[]> GenerateBasicTextSR(
            string reportText, 
            string patientId = "", 
            string patientName = "", 
            string studyInstanceUid = "",
            string originalInstanceUid = "")
        {
            var now = DateTime.Now;
            
            // Generate UIDs
            var sopInstanceUid = DicomUID.Generate().UID;
            var seriesInstanceUid = DicomUID.Generate().UID;
            
            // Use provided study UID or generate new one
            var studyUid = string.IsNullOrEmpty(studyInstanceUid) ? DicomUID.Generate().UID : studyInstanceUid;
            
            // Create the DICOM dataset
            var dataset = new DicomDataset
            {
                // SOP Class and Instance
                { DicomTag.SOPClassUID, DicomUID.BasicTextSRStorage },
                { DicomTag.SOPInstanceUID, sopInstanceUid },
                
                // Patient Information
                { DicomTag.PatientID, patientId },
                { DicomTag.PatientName, patientName },
                { DicomTag.PatientBirthDate, "" },
                { DicomTag.PatientSex, "" },
                
                // Study Information
                { DicomTag.StudyInstanceUID, studyUid },
                { DicomTag.StudyDate, now.ToString("yyyyMMdd") },
                { DicomTag.StudyTime, now.ToString("HHmmss") },
                { DicomTag.StudyID, "AI_REPORT" },
                { DicomTag.AccessionNumber, "" },
                
                // Series Information
                { DicomTag.SeriesInstanceUID, seriesInstanceUid },
                { DicomTag.SeriesNumber, 999 }, // High number to distinguish from original series
                { DicomTag.SeriesDate, now.ToString("yyyyMMdd") },
                { DicomTag.SeriesTime, now.ToString("HHmmss") },
                { DicomTag.Modality, "SR" },
                { DicomTag.SeriesDescription, "AI Generated Report" },
                
                // Instance Information
                { DicomTag.InstanceNumber, 1 },
                { DicomTag.ContentDate, now.ToString("yyyyMMdd") },
                { DicomTag.ContentTime, now.ToString("HHmmss") },
                
                // SR Specific Tags
                { DicomTag.CompletionFlag, "COMPLETE" },
                { DicomTag.VerificationFlag, "UNVERIFIED" },
                { DicomTag.ValueType, "CONTAINER" },
                
                // Document Title
                { DicomTag.ConceptNameCodeSequence, new DicomDataset[] {
                    new DicomDataset {
                        { DicomTag.CodeValue, "18748-4" },
                        { DicomTag.CodingSchemeDesignator, "LN" },
                        { DicomTag.CodeMeaning, "Diagnostic imaging report" }
                    }
                }},
                
                // Template Identification
                { DicomTag.TemplateIdentifier, "2000" },
                { DicomTag.MappingResource, "DCMR" }
            };

            // Content Sequence - This contains the actual report content
            var contentSequence = new List<DicomDataset>();
            
            // Add the report text as a TEXT content item
            var textContentItem = new DicomDataset
            {
                { DicomTag.RelationshipType, "CONTAINS" },
                { DicomTag.ValueType, "TEXT" },
                { DicomTag.ConceptNameCodeSequence, new DicomDataset[] {
                    new DicomDataset {
                        { DicomTag.CodeValue, "121070" },
                        { DicomTag.CodingSchemeDesignator, "DCM" },
                        { DicomTag.CodeMeaning, "Findings" }
                    }
                }},
                { DicomTag.TextValue, reportText }
            };
            
            contentSequence.Add(textContentItem);
            
            // Add reference to original image if provided
            if (!string.IsNullOrEmpty(originalInstanceUid))
            {
                var imageReference = new DicomDataset
                {
                    { DicomTag.RelationshipType, "SELECTED FROM" },
                    { DicomTag.ValueType, "IMAGE" },
                    { DicomTag.ReferencedSOPClassUID, DicomUID.CTImageStorage }, // Assuming CT, could be parameterized
                    { DicomTag.ReferencedSOPInstanceUID, originalInstanceUid }
                };
                
                contentSequence.Add(imageReference);
            }
            
            // Add content sequence to dataset
            dataset.Add(DicomTag.ContentSequence, contentSequence.ToArray());
            
            // Create DICOM file
            var dicomFile = new DicomFile(dataset);
            
            // Convert to bytes
            using var memoryStream = new MemoryStream();
            await dicomFile.SaveAsync(memoryStream);
            return memoryStream.ToArray();
        }
        
        /// <summary>
        /// Creates a DICOM SR with more structured content including impression and findings
        /// </summary>
        public static async Task<byte[]> GenerateStructuredSR(
            string findings,
            string impression,
            string patientId = "",
            string patientName = "",
            string studyInstanceUid = "",
            string originalInstanceUid = "")
        {
            var now = DateTime.Now;
            
            // Generate UIDs
            var sopInstanceUid = DicomUID.Generate().UID;
            var seriesInstanceUid = DicomUID.Generate().UID;
            var studyUid = string.IsNullOrEmpty(studyInstanceUid) ? DicomUID.Generate().UID : studyInstanceUid;
            
            var dataset = new DicomDataset
            {
                // SOP Class and Instance
                { DicomTag.SOPClassUID, DicomUID.BasicTextSRStorage },
                { DicomTag.SOPInstanceUID, sopInstanceUid },
                
                // Patient Information
                { DicomTag.PatientID, patientId },
                { DicomTag.PatientName, patientName },
                
                // Study Information
                { DicomTag.StudyInstanceUID, studyUid },
                { DicomTag.StudyDate, now.ToString("yyyyMMdd") },
                { DicomTag.StudyTime, now.ToString("HHmmss") },
                { DicomTag.StudyID, "AI_REPORT" },
                
                // Series Information
                { DicomTag.SeriesInstanceUID, seriesInstanceUid },
                { DicomTag.SeriesNumber, 999 },
                { DicomTag.Modality, "SR" },
                { DicomTag.SeriesDescription, "AI Generated Structured Report" },
                
                // Instance Information
                { DicomTag.InstanceNumber, 1 },
                { DicomTag.ContentDate, now.ToString("yyyyMMdd") },
                { DicomTag.ContentTime, now.ToString("HHmmss") },
                
                // SR Specific Tags
                { DicomTag.CompletionFlag, "COMPLETE" },
                { DicomTag.VerificationFlag, "UNVERIFIED" },
                { DicomTag.ValueType, "CONTAINER" },
                
                // Document Title
                { DicomTag.ConceptNameCodeSequence, new DicomDataset[] {
                    new DicomDataset {
                        { DicomTag.CodeValue, "18748-4" },
                        { DicomTag.CodingSchemeDesignator, "LN" },
                        { DicomTag.CodeMeaning, "Diagnostic imaging report" }
                    }
                }},
            };

            // Create structured content
            var contentSequence = new List<DicomDataset>();
            
            // Findings section
            if (!string.IsNullOrEmpty(findings))
            {
                var findingsItem = new DicomDataset
                {
                    { DicomTag.RelationshipType, "CONTAINS" },
                    { DicomTag.ValueType, "TEXT" },
                    { DicomTag.ConceptNameCodeSequence, new DicomDataset[] {
                        new DicomDataset {
                            { DicomTag.CodeValue, "121070" },
                            { DicomTag.CodingSchemeDesignator, "DCM" },
                            { DicomTag.CodeMeaning, "Findings" }
                        }
                    }},
                    { DicomTag.TextValue, findings }
                };
                contentSequence.Add(findingsItem);
            }
            
            // Impression section
            if (!string.IsNullOrEmpty(impression))
            {
                var impressionItem = new DicomDataset
                {
                    { DicomTag.RelationshipType, "CONTAINS" },
                    { DicomTag.ValueType, "TEXT" },
                    { DicomTag.ConceptNameCodeSequence, new DicomDataset[] {
                        new DicomDataset {
                            { DicomTag.CodeValue, "121072" },
                            { DicomTag.CodingSchemeDesignator, "DCM" },
                            { DicomTag.CodeMeaning, "Impression" }
                        }
                    }},
                    { DicomTag.TextValue, impression }
                };
                contentSequence.Add(impressionItem);
            }
            
            dataset.Add(DicomTag.ContentSequence, contentSequence.ToArray());
            
            var dicomFile = new DicomFile(dataset);
            using var memoryStream = new MemoryStream();
            await dicomFile.SaveAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
