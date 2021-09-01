using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace Ajustador_Calibrador_ADR3000.Helpers
{
    public class ReportHelper : PdfPageEventHelper //Classe para adição do Rodapé no documento PDF
    {
        private Font font = new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL);
        private string rev = "Revisão: 03";
        private string data = "Data rev.: 21/12/2020";
        private PdfContentByte cb;
        private PdfTemplate template;
        private Image bwi = Image.GetInstance("images/logo transparente.png");
        private BaseFont f = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

        public int IsLastPage { get; set; } = 0;
        public bool HasMoreThanNinePages { get; set; } = false;

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            base.OnOpenDocument(writer, document);
            cb = writer.DirectContent;
            template = cb.CreateTemplate(50, 35);
            bwi.Alignment = Image.UNDERLYING;
            bwi.ScalePercent(20);
            bwi.SetAbsolutePosition(145, 240);
        }

        /// <summary>
        /// Método invocado todo início de página
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="document"></param>
        public override void OnStartPage(PdfWriter writer, Document document)
        {
            base.OnStartPage(writer, document);
            
            PdfGState state = new PdfGState();
            cb.SaveState();
            state.FillOpacity = 0.3f;
            cb.SetGState(state);
            cb.AddImage(bwi);
            cb.RestoreState();
        }

        /// <summary>
        /// Método que é chamado toda vez que a biblioteca iTextSharp ir para próxima página
        /// do documento
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="document"></param>
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            PdfPTable footer = new PdfPTable(4);
            float width = PageSize.A4.Width / 5;
            footer.SetWidthPercentage(new float[] { width, width * 0.8f, width * 1.2f, width * 1.318f }, PageSize.A4);

            if (IsLastPage > 0)
            {
                Font font = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD);
                PdfPCell cellDate = new PdfPCell(new Phrase("Data da emissão: " + 
                    DateTime.Today.ToString("dd/MM/yyyy") + "\n\n", font));
                //
                //  Spans through all collumns
                //
                cellDate.Border = Rectangle.NO_BORDER;
                cellDate.Colspan = 4;
                cellDate.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                footer.AddCell(cellDate);
            }

            footer.DefaultCell.Border = Rectangle.NO_BORDER;
            footer.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_TOP;
            footer.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
            footer.AddCell(new Paragraph("T. 19 3861 3070\nwww.montrel.com.br\nmontrel@montrel.com.br", font));
            
            footer.DefaultCell.Border = Rectangle.NO_BORDER;
            footer.AddCell(new Paragraph(""));
            
            footer.DefaultCell.Border = Rectangle.NO_BORDER;
            footer.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_TOP;
            footer.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
            footer.AddCell(new Paragraph("Rua Maria Caporalli, 45\nParque Real, Mogi Guaçu-SP\nBrasil, CEP 13845-021", font));
            
            footer.DefaultCell.Border = Rectangle.NO_BORDER;
            footer.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_TOP;
            footer.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;

            PdfPTable table = new PdfPTable(1);
            float tableWidth = 1.318f * PageSize.A4.Width / 5;
            table.SetWidthPercentage(new float[] { tableWidth }, PageSize.A4);
            table.DefaultCell.Border = Rectangle.NO_BORDER;
            table.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_TOP;
            table.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
            table.DefaultCell.Padding = 0;

            PdfPTable firstLine = new PdfPTable(2);
            
            if (HasMoreThanNinePages)
            {
                firstLine.SetWidthPercentage(new float[]
                {
                    1.220f * PageSize.A4.Width / 5,
                    0.098f * PageSize.A4.Width / 5
                }, PageSize.A4);
            }
            else
            {
                firstLine.SetWidthPercentage(new float[]
                {
                    1.260f * PageSize.A4.Width / 5,
                    0.058f * PageSize.A4.Width / 5
                }, PageSize.A4);
            }

            
            firstLine.DefaultCell.Border = Rectangle.NO_BORDER;
            firstLine.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_TOP;
            firstLine.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
            firstLine.DefaultCell.Padding = 0;
            firstLine.AddCell(new Paragraph("Página " + writer.PageNumber.ToString() + " de", font));

            firstLine.DefaultCell.Border = Rectangle.NO_BORDER;
            firstLine.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_TOP;
            firstLine.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
            firstLine.DefaultCell.Padding = 0;
            firstLine.AddCell(new Paragraph("", font));

            table.AddCell(firstLine);
            
            table.DefaultCell.Border = Rectangle.NO_BORDER;
            table.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_TOP;
            table.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
            table.DefaultCell.Padding = 0;
            table.AddCell(new Paragraph(rev, font));

            table.DefaultCell.Border = Rectangle.NO_BORDER;
            table.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_TOP;
            table.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
            table.DefaultCell.Padding = 0;
            table.AddCell(new Paragraph(data, font));

            footer.AddCell(table);
            //footer.AddCell(new Paragraph("      Página " + writer.PageNumber.ToString() + " de\n" + rev + "\n" + data, font));
            footer.WriteSelectedRows(0, -1, document.LeftMargin, footer.TotalHeight, cb);

            if (HasMoreThanNinePages)
            {
                cb.AddTemplate(template, document.LeftMargin + 502.8, 20);
            }
            else
            {
                cb.AddTemplate(template, document.LeftMargin + 507.5, 20);
            }
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);
            
            template.BeginText();
            template.SetFontAndSize(f, 9);
            template.ShowText("" + writer.PageNumber.ToString());
            template.EndText();
        }
    }
}
