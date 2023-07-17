
using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using TimeTrackerBackend.Core.DataTransferObjects;
using Paragraph = iTextSharp.text.Paragraph;

namespace CommonBase.Extensions
{
    public static class PDFCreator
    {
        public static (byte[], string) GeneratePdf(WorkMonthDto workMonth)
        {
            string filename = "Test.pdf";
            System.IO.FileStream fs = new FileStream(filename, FileMode.Create);
            Document doc = new Document(iTextSharp.text.PageSize.A4.Rotate(), 10, 10, 0, 5);
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);

            doc.Open();
            Font headerFont = new Font(Font.FontFamily.UNDEFINED, 20, Font.ITALIC);
            Font underlineFont = new Font(Font.FontFamily.UNDEFINED, 12, Font.UNDERLINE);
            doc.Add(new Paragraph("Stundenzettel Zeiterfassung", headerFont));

            Phrase preText = new Phrase("für Mitarbeiter:", underlineFont);
            Phrase postText = new Phrase($" {workMonth.Employee.LastName} {workMonth.Employee.FirstName}");
            Paragraph combinedParagraph = new Paragraph();
            combinedParagraph.Add(preText);
            combinedParagraph.Add(postText);
            doc.Add(combinedParagraph);

            preText = new Phrase("Monat/Jahr:", underlineFont);
            postText = new Phrase($" {workMonth.Date.ToString("MMMM")}/{workMonth.Date.ToString("yyyy")}");
            combinedParagraph = new Paragraph();
            combinedParagraph.Add(preText);
            combinedParagraph.Add(postText);
            doc.Add(combinedParagraph);
            doc.Add(new Paragraph("\n"));

            PdfPTable table = new PdfPTable(5);
            table.AddCell("Datum");
            table.AddCell("Beginn");
            table.AddCell("Ende");
            table.AddCell("Pausen in Stunden");
            table.AddCell("Arbeitszeit in Stunden");
            for (int i = 0; i < workMonth.WorkDays.Length; i++)
            {
                foreach (var item in workMonth.WorkDays[i])
                {
                    table.AddCell(item.StartDate.Date.ToString("dd.MM.yyyy").ToString());
                    table.AddCell(item.StartDate.ToString("HH:mm"));
                    table.AddCell(item.EndDate.ToString("HH:mm"));
                    table.AddCell(Math.Round(item.BreakHours, 2).ToString());
                    table.AddCell(Math.Round(item.WorkedHours, 2).ToString());
                }
            }

            doc.Add(table);
            doc.Close();
            writer.Close();
            fs.Close();
            
            var bytesOfPdf = File.ReadAllBytes(filename);
            File.Delete(filename);

            return (bytesOfPdf, filename);
        }
    }
}
