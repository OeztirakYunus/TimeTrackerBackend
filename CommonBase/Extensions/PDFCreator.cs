
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
            string filename = workMonth.Id + ".pdf";
            System.IO.FileStream fs = new FileStream(filename, FileMode.Create);
            Document doc = new Document(iTextSharp.text.PageSize.A4.Rotate(), 10, 10, 0, 5);
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);

            doc.Open();
            Font headerFont = new Font(Font.FontFamily.UNDEFINED, 18, Font.ITALIC);
            Font underlineFont = new Font(Font.FontFamily.UNDEFINED, 11, Font.UNDERLINE);
            Font normalFont = new Font(Font.FontFamily.UNDEFINED, 11, Font.NORMAL);

            doc.Add(new Paragraph("Stundenzettel Zeiterfassung", headerFont));

            Phrase preText = new Phrase("für Mitarbeiter:", underlineFont);
            Phrase postText = new Phrase($" {workMonth.Employee.LastName} {workMonth.Employee.FirstName}", normalFont);
            Paragraph combinedParagraph = new Paragraph();
            combinedParagraph.Add(preText);
            combinedParagraph.Add(postText);
            doc.Add(combinedParagraph);

            preText = new Phrase("Monat/Jahr:", underlineFont);
            postText = new Phrase($" {workMonth.Date.ToString("MMMM")}/{workMonth.Date.ToString("yyyy")}", normalFont);
            combinedParagraph = new Paragraph();
            combinedParagraph.Add(preText);
            combinedParagraph.Add(postText);
            doc.Add(combinedParagraph);
            doc.Add(new Paragraph("\n"));

            PdfPTable table = new PdfPTable(6);
           
            table.AddCell(GetCell("Datum", "g"));
            table.AddCell(GetCell("Beginn", "g"));
            table.AddCell(GetCell("Ende", "g"));
            table.AddCell(GetCell("Pausen in Minuten", "g"));
            table.AddCell(GetCell("Arbeitszeit in Stunden", "g"));
            table.AddCell(GetCell("Gesamtdauer in Stunden", "g"));

            for (int i = 0; i < workMonth.WorkDays.Length; i++)
            {
                foreach (var item in workMonth.WorkDays[i])
                {
                    var color = "none";
                    if(item.EndDate.Equals(DateTime.MinValue) && item.WorkedHours == 0)
                    {
                        color = "y";
                    }

                    table.AddCell(GetCell(item.StartDate.Date.ToString("dd.MM.yyyy").ToString(), color));
                    table.AddCell(GetCell(item.StartDate.ToString("HH:mm"), color));
                    table.AddCell(GetCell(item.EndDate.ToString("HH:mm"), color));
                    table.AddCell(GetCell(Math.Round(item.BreakHours * 60, 2).ToString(), color));
                    table.AddCell(GetCell(Math.Round(item.WorkedHours, 2).ToString(), color));
                    table.AddCell(GetCell(Math.Round(item.WorkedHours + item.BreakHours, 2).ToString(), color));
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

        private static PdfPCell GetCell(string text, string color = "none")
        {
            Font normalFont = new Font(Font.FontFamily.UNDEFINED, 11, Font.NORMAL);
            PdfPCell cell = new PdfPCell(new Phrase(text, normalFont));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            switch (color)
            {
                case "y":
                    cell.BackgroundColor = BaseColor.YELLOW;
                    break;
                case "g":
                    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    break;
                default:
                    break;
            }

            return cell;
        }
    }
}
