using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using System.IO;
using ADODB;
using System.Windows.Forms.DataVisualization.Charting;
using Ajustador_Calibrador_ADR3000.Helpers;

namespace Ajustador_Calibrador_ADR3000.Helpers
{
    public class ReportGenerator
    {
        public bool HasMoreThanNinePages { get; set; } = false;

        /// <summary>
        /// Generates a report for a calibration of an ADR
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="rsCustomer"></param>
        /// <param name="rsStandard"></param>
        /// <param name="Montrel"></param>
        /// <param name="dados"></param>
        /// <param name="sig"></param>
        /// <param name="ene"></param>
        /// <returns></returns>
        public bool CalibrationReport(Recordset rs, Recordset rsCustomer, Recordset rsStandard, Recordset Montrel, Recordset dados, Recordset sig, ushort ene, string executorName)
        {
            
            Document doc = new Document(PageSize.A4, 50, 50, 15, 35); //documento pdf
            string date = DateTime.Today.ToString("dd/MM/yyyy").Replace("/", null);
            string data = ((dynamic)dados.Fields[1]).Value.ToString("dd/MM/yyyy").Substring(0, 10);
            string path = Path.Combine(((dynamic)rs.Fields["SavePath"]).Value, "Relatório " + ((dynamic)dados.Fields[3]).Value + "_" + data.Replace("/", null) + " " + ((dynamic)dados.Fields[14]).Value + " " + ((dynamic)dados.Fields[15]).Value + ".pdf");
            string laudo = ((dynamic)dados.Fields["Laudo"]).Value;
            ADRModel model;
            PdfWriter writer;

            try
            {
                writer = PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (false);
            }

            writer.PageEvent = new ReportHelper();
            ((ReportHelper)writer.PageEvent).HasMoreThanNinePages = HasMoreThanNinePages;

            PdfPTable tableIntro, tableCust, tableLab, tableRefMeter, table, tableR, tableAdr, tableObs, tableDate, tableNames; //classe de tabela para pdfs para resultados da calibração
            PdfPCell cell, cellR; //classe para trabalho com células da tabela no pdf
            Font font, textFont, tableFont;
            string n = dados.Fields["n_amostra"].Value.ToString(), powerFactor;
            string user = ((dynamic)dados.Fields["Usuario"]).Value;
            float width, error = 0.0f, pf, _class = 0.2f, pfLim = 0.2f;
            float[] widths;
            float ur, ua, up, upre, uc, U; //exatidão do padrão, incerteza devido a medições repetidas, incerteza herdada do padrão, resolução do prescaler e incerteza expandida
            float kA, kR; //fator de abrangência
            float tInterval = ((dynamic)rs.Fields["tInterval"]).Value;
            int veff;
            double k;

            Chart chart = new Chart();

            try
            {
                font = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD); // seleciona a fonte para cabeçalho
                tableFont = new Font(Font.FontFamily.HELVETICA, 9, Font.NORMAL);

                Image logo = Image.GetInstance("images/Logo novo Montrel.jpg"); //seleciona imagem do logo da Montrel para cabeçalho
                Image signature = Image.GetInstance("images/" + sig.Fields["Nome"].Value + ".png");
                Image sgnExecutor = Image.GetInstance("images/" + user + ".png");

                sgnExecutor.ScalePercent(6.5f, 6.5f);
                sgnExecutor.Alignment = Image.ALIGN_CENTER;
                signature.ScalePercent(6.5f, 6.5f);
                signature.Alignment = Image.ALIGN_CENTER;

                tableIntro = new PdfPTable(3); //tabela com 2 colunas
                width = PageSize.A4.Width / 6;
                tableIntro.SetWidthPercentage(new float[] { width * 1.3f, width * 3.7f, width }, PageSize.A4);
                tableIntro.DefaultCell.Border = Rectangle.NO_BORDER;
                tableIntro.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                tableIntro.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                //tableIntro.AddCell(new Paragraph("Nº " + laudo + "/" + data.Substring(8, 2), font));
                tableIntro.AddCell(new Paragraph("Nº " + laudo, font));
                font = new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD);
                tableIntro.DefaultCell.Border = Rectangle.NO_BORDER;
                tableIntro.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_BOTTOM;
                tableIntro.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                tableIntro.AddCell(new Paragraph("RELATÓRIO DE CALIBRAÇÃO", font));
                tableIntro.DefaultCell.Border = Rectangle.NO_BORDER;
                tableIntro.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_TOP;
                tableIntro.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                tableIntro.AddCell(logo);
                font = new Font(Font.FontFamily.HELVETICA, 1, Font.NORMAL);
                tableIntro.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
                tableIntro.AddCell(new Paragraph(" ", font));
                tableIntro.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
                tableIntro.AddCell(new Paragraph(" ", font));
                tableIntro.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
                tableIntro.AddCell(new Paragraph(" ", font));

                doc.Open();

                doc.Add(tableIntro); //adiciona tabela com texto + logo Montrel



                //doc.Add(new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100.0f, BaseColor.BLACK, Element.ALIGN_TOP, 0))));

                /*Subtítulo contendo número do laudo de calibração*/

                font = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD);
                textFont = new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL);

                /*Dados do cliente para o qual será emitido o relatório*/
                doc.Add(new Paragraph("\n        1 - Dados do cliente:", font));

                tableCust = new PdfPTable(1);
                tableCust.SetWidthPercentage(new float[] { PageSize.A4.Width }, PageSize.A4);
                tableCust.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                tableCust.DefaultCell.Border = Rectangle.NO_BORDER;
                tableCust.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_JUSTIFIED;
                tableCust.AddCell(new Paragraph("Nome: " + rsCustomer.Fields["Nome"].Value +
                "\nEnd: " + rsCustomer.Fields["Rua"].Value + ", n° " + rsCustomer.Fields["Numero"].Value.ToString() + ", " + rsCustomer.Fields["Bairro"].Value + ", " + rsCustomer.Fields["Cidade"].Value + "-" + rsCustomer.Fields["UF"].Value + " CEP: "
                    + ((dynamic)rsCustomer.Fields["CEP"]).Value.ToString().Substring(0, 5) + "-" + ((dynamic)rsCustomer.Fields["CEP"]).Value.ToString().Substring(5, 3) + "\r", textFont));
                doc.Add(tableCust);

                /*Dados do laboratório onde o ADR Multi 4000 foi calibrado*/
                doc.Add(new Paragraph("\n       2 - Dados do laboratório de calibração:", font));

                tableLab = new PdfPTable(1);
                tableLab.SetWidthPercentage(new float[] { PageSize.A4.Width }, PageSize.A4);
                tableLab.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                tableLab.DefaultCell.Border = Rectangle.NO_BORDER;
                tableLab.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_JUSTIFIED;
                tableLab.AddCell(new Paragraph("Nome: " + Montrel.Fields["Nome"].Value + "\nEnd: " + Montrel.Fields["Rua"].Value + ", n° " + Montrel.Fields["Numero"].Value.ToString() + ", " + Montrel.Fields["Bairro"].Value + ", " + Montrel.Fields["Cidade"].Value + "-" + Montrel.Fields["UF"].Value + " CEP: "
                    + ((dynamic)Montrel.Fields["CEP"]).Value.ToString().Substring(0, 5) + "-" + ((dynamic)Montrel.Fields["CEP"]).Value.ToString().Substring(5, 3) + "\r", textFont));
                doc.Add(tableLab);

                /*Dados do ADR Multi 4000*/
                doc.Add(new Paragraph("\n       3 - Dados do Instrumento:", font));

                tableAdr = new PdfPTable(2);

                string nameFilter = ((dynamic)dados.Fields["Nome"]).Value.ToString().Substring(0, 4);
                string equipment =  (nameFilter == "AM3-" ? "ADR 3000" : 
                                    (nameFilter == "AM2-" ? "ADR 2000" : 
                                    (nameFilter == "AL3-" ? "ADR 3000 LITE" : "ADR 3000 PLUS")));
                model = ADRModel._3000;
                _class = ((dynamic)rs.Fields["Class_3000"]).Value;


                tableAdr.HorizontalAlignment = Element.ALIGN_LEFT;
                tableAdr.DefaultCell.Border = Rectangle.NO_BORDER;
                tableAdr.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                tableAdr.AddCell(new Paragraph("Instrumento:\nFabricante:\nModelo/Tipo:\nNúmero de série:\nClasse de exatidão:\nNº de controle:", textFont));
                tableAdr.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                tableAdr.AddCell(new Paragraph("Analisador de Desvio de Registro\n" + ((dynamic)Montrel.Fields["Nome"]).Value + "\n" + equipment + "\n" + ((dynamic)dados.Fields["Nome"]).Value.ToString().Substring(4, 4) + "\n0.2 %\n" + ((dynamic)dados.Fields["OSOP"]).Value + ": " + ((dynamic)dados.Fields["NumOSOP"]).Value, textFont));

                doc.Add(tableAdr);
                /*Quais equipementos de referência metrológica foram usados*/

                doc.Add(new Paragraph("\n       4 - Equipamentos de referência utilizados:", font));
                tableRefMeter = new PdfPTable(1);
                tableRefMeter.SetWidthPercentage(new float[] { PageSize.A4.Width }, PageSize.A4);
                tableRefMeter.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                tableRefMeter.DefaultCell.Border = Rectangle.NO_BORDER;
                tableRefMeter.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_JUSTIFIED;
                tableRefMeter.AddCell(new Paragraph("1) " + Montrel.Fields[13].Value +
                                      "\n2) Padrão de Energia Trifásico, Modelo: " + ((dynamic)rsStandard.Fields[4]).Value + ", Número de série: " + ((dynamic)rsStandard.Fields[6]).Value.ToString() +
                                      "\nFabricante: " + rsStandard.Fields[5].Value +
                                      ". Certificado: " + rsStandard.Fields[7].Value + " emitido por " + ((dynamic)rsStandard.Fields[8]).Value + " realizado em: " + ((dynamic)rsStandard.Fields[9]).Value.ToString("dd/MM/yyyy"), textFont));

                doc.Add(tableRefMeter);

                /*Procedimento (metodologia) de calibração do equipamento*/
                doc.Add(new Paragraph("\n       5 - Procedimento:", font));
                doc.Add(new Paragraph(11, ((dynamic)Montrel.Fields["Procedimento"]).Value, textFont));

                /*Tabelas com resultados do ensaio de calibração*/
                doc.Add(new Paragraph("\n       6 - Resultados:\n\n", font));

                /*Parte da tabela com resultados de testes de energia ativa*/
                if (ene != 1)
                {
                    table = new PdfPTable(6);

                    cell = new PdfPCell(new Phrase("Energia Ativa", tableFont));
                    cell.Colspan = 6;
                    cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER; //alinhado no centro
                    table.AddCell(cell);
                    /*Títulos das colunas da tabela de resultados*/
                    cell = new PdfPCell(new Phrase("Tensão [V]", tableFont));
                    cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("Corrente [A]", tableFont));
                    cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("Elementos", tableFont));
                    cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("cos(phi)", tableFont));
                    cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    table.AddCell(cell);

                    //table.AddCell("n");
                    cell = new PdfPCell(new Phrase("ERRO [%]", tableFont));
                    cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("IM [%]", tableFont));
                    cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    table.AddCell(cell);
                    //títulos tabela de resultados

                    while (((dynamic)dados.Fields[7]).Value == "A")
                    {
                        cell = new PdfPCell(new Phrase(dados.Fields[4].Value.ToString(), tableFont));
                        cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                        table.AddCell(cell);
                        cell = new PdfPCell(new Phrase(dados.Fields[5].Value.ToString(), tableFont));
                        cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                        table.AddCell(cell);
                        cell = new PdfPCell(new Phrase(dados.Fields[6].Value.ToString(), tableFont));
                        cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                        table.AddCell(cell);

                        powerFactor = dados.Fields[8].Value.ToString();
                        if (powerFactor.Contains("ind") || powerFactor.Contains("cap"))
                        {
                            powerFactor = powerFactor.Substring(0, powerFactor.Length - 4);
                        }
                        pf = float.Parse(powerFactor, CultureInfo.InvariantCulture.NumberFormat);
                        cell = new PdfPCell(new Phrase(dados.Fields[8].Value.ToString(), tableFont));
                        cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                        table.AddCell(cell);

                        error = ((dynamic)dados.Fields[9]).Value;
                        cell = new PdfPCell(new Phrase(((dynamic)dados.Fields[9]).Value.ToString("0.000"), tableFont));
                        cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                        table.AddCell(cell);

                        ur = ((dynamic)rs.Fields["exatA"]).Value;
                        ur = ur * ur / 3;   //exatidão do padrão

                        ua = ((dynamic)dados.Fields[10]).Value;
                        ua = ua * ua / Convert.ToInt32(n); //repetibilidade

                        kA = ((dynamic)rs.Fields["kA"]).Value;
                        up = ((dynamic)rs.Fields["upA"]).Value;
                        up = (up * up) / (kA * kA); //incerteza herdada do relatório do padrão

                        upre = ((dynamic)rs.Fields["Res"]).Value;
                        upre = upre * upre / 12;

                        uc = ur + ua + up + upre;   //incerteza combinada ao quadrado

                        if (Convert.ToInt32(n) == 1) veff = int.MaxValue;
                        else
                        {
                            try
                            {
                                veff = Convert.ToInt32(uc * uc * (Convert.ToInt32(n) - 1) / (ua * ua));
                                if (veff < 0) veff = int.MaxValue;
                            }
                            catch (OverflowException)
                            {
                                veff = int.MaxValue;
                            }
                        }

                        k = chart.DataManipulator.Statistics.InverseTDistribution(tInterval, veff);

                        U = (float)(k * Math.Sqrt(uc));

                        cell = new PdfPCell(new Phrase(U.ToString("0.000"), tableFont));
                        cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;

                        pfLim = _class / pf;

                        if ((Math.Abs(error) + Math.Abs(U)) > pfLim) cell.BackgroundColor = BaseColor.RED;

                        table.AddCell(cell);
                        dados.MoveNext();
                        if (dados.EOF) break;
                    }
                    doc.Add(table);
                    table.SplitLate = false;
                    table.SplitRows = true;
                }

                /*Parte da tabela com resultados de testes de energia reativa*/
                if (ene != 0)
                {
                    //ur = 0.116f;

                    doc.Add(new Paragraph("\n"));

                    tableR = new PdfPTable(6);

                    cellR = new PdfPCell(new Phrase("Energia Reativa", tableFont));
                    cellR.Colspan = 6;
                    cellR.HorizontalAlignment = PdfPCell.ALIGN_CENTER; //alinhado no centro
                    tableR.AddCell(cellR);
                    /*Títulos das colunas da tabela de resultados*/

                    /*Títulos das colunas da tabela de resultados*/
                    cellR = new PdfPCell(new Phrase("Tensão [V]", tableFont));
                    cellR.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    tableR.AddCell(cellR);

                    cellR = new PdfPCell(new Phrase("Corrente [A]", tableFont));
                    cellR.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    tableR.AddCell(cellR);

                    cellR = new PdfPCell(new Phrase("Elementos", tableFont));
                    cellR.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    tableR.AddCell(cellR);

                    cellR = new PdfPCell(new Phrase("sen(phi)", tableFont));
                    cellR.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    tableR.AddCell(cellR);


                    //tableR.AddCell("n");
                    cellR = new PdfPCell(new Phrase("ERRO [%]", tableFont));
                    cellR.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    tableR.AddCell(cellR);

                    cellR = new PdfPCell(new Phrase("IM [%]", tableFont));
                    cellR.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    tableR.AddCell(cellR);


                    while (!dados.EOF)
                    {
                        cellR = new PdfPCell(new Phrase(dados.Fields[4].Value.ToString(), tableFont));
                        cellR.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                        tableR.AddCell(cellR);
                        cellR = new PdfPCell(new Phrase(dados.Fields[5].Value.ToString(), tableFont));
                        cellR.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                        tableR.AddCell(cellR);
                        cellR = new PdfPCell(new Phrase(dados.Fields[6].Value.ToString(), tableFont));
                        cellR.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                        tableR.AddCell(cellR);

                        powerFactor = dados.Fields[8].Value.ToString();
                        if (powerFactor.Contains("ind") || powerFactor.Contains("cap"))
                        {
                            powerFactor = powerFactor.Substring(0, powerFactor.Length - 4);
                        }
                        pf = float.Parse(powerFactor, CultureInfo.InvariantCulture.NumberFormat);
                        cellR = new PdfPCell(new Phrase(dados.Fields[8].Value.ToString(), tableFont));
                        cellR.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                        tableR.AddCell(cellR);

                        error = ((dynamic)dados.Fields[9]).Value;
                        cellR = new PdfPCell(new Phrase(((dynamic)dados.Fields[9]).Value.ToString("0.000"), tableFont));
                        cellR.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                        tableR.AddCell(cellR);

                        ur = ((dynamic)rs.Fields["exatR"]).Value;
                        ur = ur * ur / 3;   //exatidão do padrão

                        ua = ((dynamic)dados.Fields[10]).Value;
                        ua = ua * ua / Convert.ToInt32(n); //repetibilidade

                        kR = ((dynamic)rs.Fields["kR"]).Value;
                        up = ((dynamic)rs.Fields["upR"]).Value;
                        up = (up * up) / (kR * kR);

                        upre = ((dynamic)rs.Fields["Res"]).Value;
                        upre = upre * upre / 12;

                        uc = ur + ua + up + upre;   //incerteza combinada ao quadrado

                        if (Convert.ToInt32(n) == 1) veff = int.MaxValue;
                        else
                        {
                            try
                            {
                                veff = (int)(uc * uc * (Convert.ToInt32(n) - 1) / (ua * ua));
                                if (veff < 0) veff = int.MaxValue;
                            }
                            catch (OverflowException)
                            {
                                veff = int.MaxValue;
                            }
                        }

                        k = chart.DataManipulator.Statistics.InverseTDistribution(tInterval, veff);

                        U = (float)(k * Math.Sqrt(uc));


                        cellR = new PdfPCell(new Phrase(U.ToString("0.000"), tableFont));
                        cellR.HorizontalAlignment = PdfPCell.ALIGN_CENTER;

                        pfLim = 2 * _class / pf;

                        if ((Math.Abs(error) + Math.Abs(U)) > pfLim) cellR.BackgroundColor = BaseColor.RED;

                        tableR.AddCell(cellR);
                        dados.MoveNext();
                    }
                    doc.Add(tableR);
                    tableR.SplitLate = false;
                    tableR.SplitRows = true;
                }




                /*Observações sobre a calibração do ADR*/
                doc.Add(new Paragraph("\n      7 - Observações:", font));

                tableObs = new PdfPTable(1);
                tableObs.SetWidthPercentage(new float[] { PageSize.A4.Width }, PageSize.A4);
                tableObs.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                tableObs.DefaultCell.Border = Rectangle.NO_BORDER;
                tableObs.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_JUSTIFIED;
                tableObs.AddCell(new Paragraph(10.5f, "1) Os resultados apresentados neste relatório se referem exclusivamente ao instrumento calibrado, nas condições especificadas." +
                                               "\n2) ERRO (%) – Diferença em porcentagem entre o padrão de energia e o ADR." +
                                               "\n3) IM (%) – A incerteza expandida de medição relatada neste ensaio é declarada como incerteza padrão de medição multiplicada pelo fator de abrangência k para uma probabilidade t-Student de abrangência de aproximadamente 95,45%. Considerando veff = infinito, tem-se k = 2." +
                                               "\n4) Foram realizados " + n + " testes para cada ponto calibrado."/* + 
                                               "\n5) Os valores e ensaios realizados estão de acordo com a especificação técnica da Neoenergia."*/, textFont));
                doc.Add(tableObs);
                /*Informações sobre data da calibração*/
                doc.Add(new Paragraph("\n      8 - Identificação da Calibração\n", font));

                tableDate = new PdfPTable(2);
                tableDate.HorizontalAlignment = Element.ALIGN_LEFT;
                tableDate.DefaultCell.Border = Rectangle.NO_BORDER;
                tableDate.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                tableDate.AddCell(new Paragraph("Data da calibração:", textFont));
                tableDate.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                tableDate.AddCell(new Paragraph(data, font));
                tableDate.AddCell(new Paragraph("Local da calibração:", textFont));
                tableDate.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                tableDate.AddCell(new Paragraph("Montrel", font));

                doc.Add(tableDate);
                //
                //  Informa Mogi Guaçu-SP
                //
                //doc.Add(new Paragraph("\n" + Montrel.Fields[7].Value + "-" + Montrel.Fields[8].Value + "\n\n", font));

                doc.Add(new Paragraph("\n\n", font));

                /*Informações sobre pessoas responsáveis pelo laudo de calibração*/
                tableNames = new PdfPTable(3); // 2 colunas com uma em branco no meio
                widths = new float[] { 2 / 5f, 1 / 5f, 2 / 5f };
                tableNames.SetWidths(widths);
                cell = new PdfPCell();

                /*tableNames.DefaultCell.Border = Rectangle.NO_BORDER;
                tableNames.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                tableNames.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_BOTTOM;
                tableNames.AddCell(new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.5f, 100.0f, BaseColor.BLACK, Element.ALIGN_LEFT, 0))));
                tableNames.AddCell(new Paragraph(""));*/
                cell.Border = Rectangle.BOTTOM_BORDER;
                cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                cell.VerticalAlignment = PdfPCell.ALIGN_BOTTOM;
                cell.AddElement(sgnExecutor);
                tableNames.AddCell(cell);
                tableNames.DefaultCell.Border = Rectangle.NO_BORDER;
                tableNames.AddCell(new Paragraph(""));
                cell = new PdfPCell();
                cell.Border = Rectangle.BOTTOM_BORDER;
                cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                cell.VerticalAlignment = PdfPCell.ALIGN_BOTTOM;
                cell.AddElement(signature);
                tableNames.AddCell(cell);
                tableNames.DefaultCell.Border = Rectangle.NO_BORDER;
                tableNames.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                tableNames.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_TOP;
                tableNames.AddCell(new Paragraph(executorName + "\nExecutor", textFont));
                tableNames.AddCell(new Paragraph(""));
                tableNames.DefaultCell.Border = Rectangle.NO_BORDER;
                tableNames.DefaultCell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                tableNames.DefaultCell.VerticalAlignment = PdfPCell.ALIGN_TOP;
                tableNames.AddCell(new Paragraph(sig.Fields[2].Value + "\n" + sig.Fields[3].Value, textFont));

                ((ReportHelper)(writer.PageEvent)).IsLastPage = 1;

                doc.Add(tableNames);

                tableNames.SplitLate = false;
                tableNames.SplitRows = true;

                if (writer.PageNumber > 9) HasMoreThanNinePages = true;
                else HasMoreThanNinePages = false;

                if (doc.IsOpen()) doc.Close();
                doc.Dispose();
                chart.Dispose();
                writer.Dispose();
                return (true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (doc.IsOpen()) doc.Close();
                doc.Dispose();
                writer.Dispose();
                chart.Dispose();
                return (false);
            }
        }
    }
}
