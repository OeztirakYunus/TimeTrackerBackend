
using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using TimeTrackerBackend.Core.DataTransferObjects;
using TimeTrackerBackend.Core.Entities;
using Paragraph = iTextSharp.text.Paragraph;

namespace CommonBase.Extensions
{
    public static class PDFCreator
    {
        //Creates a PDF-File for a WorkMonth
        public static (byte[], string) GeneratePdf(WorkMonthDto workMonth)
        {
            string filename = workMonth.Id + ".pdf";
            System.IO.FileStream fs = new FileStream(filename, FileMode.Create);
            Document doc = new Document(iTextSharp.text.PageSize.A4.Rotate(), 10, 10, 0, 5);
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);

            doc.Open();
            Font headerFont = new Font(Font.FontFamily.UNDEFINED, 18, Font.ITALIC);
            doc.Add(new Paragraph("Stundenzettel Zeiterfassung", headerFont));

            var paragraph = CreateParagraph("für Mitarbeiter:", $" {workMonth.Employee.LastName} {workMonth.Employee.FirstName}");
            doc.Add(paragraph);

            paragraph = CreateParagraph("Monat/Jahr:", $" {workMonth.Date.ToString("MMMM")}/{workMonth.Date.ToString("yyyy")}");
            doc.Add(paragraph);
            doc.Add(new Paragraph("\n"));

            var table = CreateTable(workMonth);
            doc.Add(table);

            doc.Close();
            writer.Close();
            fs.Close();
            
            var bytesOfPdf = File.ReadAllBytes(filename);
            File.Delete(filename);

            return (bytesOfPdf, filename);
        }

        private static PdfPTable CreateTable(WorkMonthDto workMonth)
        {

            PdfPTable table = new PdfPTable(6);

            //TableHeader
            table.AddCell(GetCell("Datum", "g"));
            table.AddCell(GetCell("Beginn", "g"));
            table.AddCell(GetCell("Ende", "g"));
            table.AddCell(GetCell("Pausen in Minuten", "g"));
            table.AddCell(GetCell("Arbeitszeit in Stunden", "g"));
            table.AddCell(GetCell("Gesamtdauer in Stunden", "g"));

            //ContentRows
            for (int i = 0; i < workMonth.WorkDays.Length; i++)
            {
                foreach (var item in workMonth.WorkDays[i])
                {
                    if (item.VacationDay || item.IllDay)
                    {
                        var text = "Urlaub";
                        var mode = "vacation";
                        if (item.IllDay)
                        {
                            text = "Krank";
                            mode = "illness";
                        }
                        table.AddCell(GetCell(item.StartDate.Date.ToString("dd.MM.yyyy").ToString(), mode));
                        var cell = GetCell(text, mode);
                        cell.Colspan = 5;
                        table.AddCell(cell);
                    }
                    else if (item.EndDate.Equals(DateTime.MinValue) && item.WorkedHours == 0)
                    {
                        table.AddCell(GetCell(item.StartDate.Date.ToString("dd.MM.yyyy").ToString(), "free"));
                        var cell = GetCell("Frei / Kein Eintrag vorhanden", "free");
                        cell.Colspan = 5;
                        table.AddCell(cell);
                    }
                    else
                    {
                        table.AddCell(GetCell(item.StartDate.Date.ToString("dd.MM.yyyy").ToString()));
                        table.AddCell(GetCell(item.StartDate.ToString("HH:mm")));
                        table.AddCell(GetCell(item.EndDate.ToString("HH:mm")));
                        table.AddCell(GetCell(Math.Round(item.BreakHours * 60, 2).ToString()));
                        table.AddCell(GetCell(Math.Round(item.WorkedHours, 2).ToString()));
                        table.AddCell(GetCell(Math.Round(item.WorkedHours + item.BreakHours, 2).ToString()));
                    }
                }
            }

            return table;
        }

        private static Paragraph CreateParagraph(string preTextString, string postTextString)
        {
            Font underlineFont = new Font(Font.FontFamily.UNDEFINED, 11, Font.UNDERLINE);
            Font normalFont = new Font(Font.FontFamily.UNDEFINED, 11, Font.NORMAL);
            Phrase preText = new Phrase(preTextString, underlineFont);
            Phrase postText = new Phrase(postTextString, normalFont);
            Paragraph combinedParagraph = new Paragraph();
            combinedParagraph.Add(preText);
            combinedParagraph.Add(postText);
            return combinedParagraph;
        }

        private static PdfPCell GetCell(string text, string stampType = "none")
        {
            Font normalFont = new Font(Font.FontFamily.UNDEFINED, 11, Font.NORMAL);
            PdfPCell cell = new PdfPCell(new Phrase(text, normalFont));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            switch (stampType)
            {
                case "vacation":
                    cell.BackgroundColor = BaseColor.YELLOW;
                    break;
                case "illness":
                    cell.BackgroundColor = BaseColor.RED;
                    break;
                case "free":
                    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    break;
                default:
                    break;
            }

            return cell;
        }
    }
}
