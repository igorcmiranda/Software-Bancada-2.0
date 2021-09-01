using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ADODB;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using CalibradorMulti4000_Mesa.DataBase;
using CalibradorMulti4000_Mesa.Devices;
using Microsoft.VisualBasic;

namespace CalibradorMulti4000_Mesa.Forms
{
    public partial class frConfig : Form
    {
        private bool myType = false;

        public frConfig()
        {
            InitializeComponent();
        }

        public frConfig(bool type)
        {
            InitializeComponent();
            myType = type;
        }

        private List<string> campos { get; } = new List<string>()
        {
            "nE", //1
            "nV", //2
            "nI", //3
            "nP",//4
            "nKp",
            "MErrV", //6
            "MErrI", //7
            "MErrP", //8
            "APHCALW7L", //14
            "APHCALW1L", //15
            "TP", //34
            "TC", //35
            "tempo",//36
            "offVG",//42
            "offEAi",//49
            "offEAiTC10",//62
            "stdPort", //63
            "counterPort", //64
            "pointsSet200", //65
            "pointsSet10", //66
            "PHCAL7200", //67
            "PHCAL1200", //68
            "tolPre", //69
            "tolLimPre",
            "PHCAL710", //70
            "PHCAL110", //71
            "PHCAL200MAX", //72
            "PHCAL200MIN", //73
            "PHCAL10MAX", //74
            "PHCAL10MIN", //75
            "ClientNum",
            "ServerIP",
            "Port",
            "VAddr", //76
            "IAddr", //77
            "SavePath", //78
            "incGO",
            "incPHI",
            "nm",
            "tolLimV",
            "tolLimI",
            "tolLimP",
            "tolLimE",
            "itrans",
            "Cpri",
            "Caux",
            "digits",
            "deltaT",
            "Setup",
            "na",
            "offVG5000",
            "incGO5000",
            "incPHI5000",
            "pointsSet200_5000",
            "pointsSet10_5000",
            "offVO5000",
            "pointsSet200_5000_MB",
            "pointsSet10_5000_MB",
            "incWATTOS_5000",
            "incPGAIN_5000",
            "upA",
            "upR",
            "Cpri5000",
            "Caux5000",
            "spVOS0",
            "spVG0",
            "spVOS1",
            "spVG1",
            "exatA",
            "exatR",
            "kA",
            "kR",
            "Res",
            "tInterval",
            "Class_Multi4000",
            "Class_5000"
        };

        private List<string> description { get;} = new List<string>()
        {
            "Tamanho da amostra para a energia(ecal)", //1
            "Tamanho da amostra para as tensões", //2
            "Tamanho da amostra para as correntes", //3
            "Tamanho da amostra para as potências ativas",//4
            "Tamanho da amostra para constantes de proporcionalidade",
            "Tolerância para as tensões", //6
            "Tolerância para as correntes", //7
            "Tolerância para as potências ativas", //8
            "Endereço do fator de fase da faixa 7 - fase A", //14
            "Endereço do fator de fase da faixa 1 - fase A", //15
            "Relação do TP", //34
            "Relação do TC", //35
            "Timeout para leitura de dados no modo ecal",//36
            "offset para erro de ganho de tensão",//42
            "offset para erro energia ativa em fator de potência indutivo",//49
            "offset para erro energia ativa em fator de potência indutivo (TC de 10 A)",//62
            "Porta COM para conexão com Padrão GF333B", //63
            "Porta COM para conexão com Prescaler", //64
            "Conjunto de pontos a serem utilizados em ajuste com TC de 200 A", //65
            "Conjunto de pontos a serem utilizados em ajuste com TC de 10 A", //66
            "Fator de ângulo inicial para pré ajuste das faixa 4, 5, 6 e 7 para TC de 200 A", //67
            "Fator de ângulo inicial para pré ajuste das faixa 1, 2 e 3 para TC de 200 A", //68
            "Tolerância para pré ajuste", //69
            "Tolerância limite para pré ajuste",
            "Fator de ângulo inicial para pré ajuste das faixa 4, 5, 6 e 7 para TC de 10 A", //70
            "Fator de ângulo inicial para pré ajuste das faixa 1, 2 e 3 para TC de 10 A", //71
            "Limite máximo para fator de ângulo para TC de 200 A", //72
            "Limite mínimo para fator de ângulo para TC de 200 A", //73
            "Limite máximo para fator de ângulo para TC de 10 A", //74
            "Limite mínimo para fator de ângulo para TC de 10 A", //75
            "Quantidade de clientes TCP a serem aceitos",
            "IP do servidor TCP/IP",
            "Porta a qual o servidor TCP/IP escuta",
            "Endereço IP da fonte de tensão", //76
            "Endereço IP da fonte de corrente", //77
            "Pasta onde serão salvos os relatórios de calibração", //78
            "Incremento a para segunda solução inicial para ganhos e offsets",
            "Incremento a para segunda solução inicial para defasagem",
            "Número de medidas pra tirar defasagem média da fonte (phi médio)",
            "Limite para erro de tensão após aprovação do ajuste",
            "Limite para erro de corrente após aprovação do ajuste",
            "Limite para erro de potência após aprovação do ajuste",
            "Limite para erro de energia após aprovação do ajuste",
            "Corrente de transição para mudança de escala na fonte",
            "Constante de pulsos do ADR para o clamp principal",
            "Constante de pulsos do ADR para o clamp auxiliar",
            "Quantidade de casas para truncamento de floats na calibração",
            "Número em segundos para esperar a mais do que o calculado para pontos de calibração",
            "Código referente a qual setup o ADR Multi 4000 será calibrado",
            "Tamanho da amostra para ponto da curva do clamp",
            "offset para erro de ganho de tensão para ADR 5000",
            "Incremento a para segunda solução inicial para ganhos e offsets para ADR5000",
            "Incremento a para segunda solução inicial para defasagem para ADR5000",
            "Conjunto de pontos a serem utilizados em ajuste com escala de 200 A",
            "Conjunto de pontos a serem utilizados em ajuste com escala de 10 A",
            "offset para erro de offset de tensão para ADR 5000",
            "Conjunto de pontos a serem utilizados em ajuste com escala de 200 A para multi faixa",
            "Conjunto de pontos a serem utilizados em ajuste com escala de 10 A para multi faixa",
            "Incremento a para segunda solução inicial para offset de potência ADR 5000",
            "Incremento a para segunda solução inicial para ganho de potência ADR 5000",
            "Incerteza do pior caso para energia ativa do GF333B",
            "Incerteza do pior caso para energia reativa do GF333B",
            "Constante de pulsos do ADR5000 para o clamp principal (imp/Wh)",
            "Constante de pulsos do ADR5000 para o clamp auxiliar (imp/Wh)",
            "Set point de tensão para offset escala alta",
            "Set point de tensão para ganho escala alta",
            "Set point de tensão para offset escala baixa",
            "Set point de tensão para ganho escala baixa",
            "Exatidão do padrão para energia ativa",
            "Exatidão do padrão para energia reativa",
            "Fator de abrangência k do padrão para o pior caso da energia ativa",
            "Fator de abrangência k do padrão para o pior caso da energia reativa",
            "Resolução do prescaler que calcula o erro de calibração",
            "Intervalo de confiança para cálculo de probabilidade t-Student (P-1)",
            "Classe de exatidão do ADR Multi4000",
            "Classe de exatidão do ADR 5000"
        };

        /*Carrega o formulário e popula a grid*/
        private void frConfig_Load(object sender, EventArgs e)
        {
            int count;

            //abre o banco de dados e retira parâmetros de configuração
            try
            {
                if (!myType)
                {
                    Database db = new Database();
                    Connection cnDb = new Connection();
                    Recordset rs;
                    List<string> fields = new List<string>();
                    int i;

                    db.connectToDatabase(0, ref cnDb);
                    fields.Add("*");
                    rs = new Recordset();


                    db.getData(0, "Parametros", fields, cnDb, "", ref rs);

                    //popular a grid de parâmetros
                    dgConfig.Columns.Add("Column Name", "Parâmetro");
                    dgConfig.Columns.Add("Column Value", "Valor");
                    dgConfig.Columns.Add("Column Description", "Descrição");

                    for (i = 0; i < campos.Count; i++)
                    {
                        count = dgConfig.Rows.Add();
                        dgConfig.Rows[count].Cells[0].Value = campos[i];
                        dgConfig.Rows[count].Cells[1].Value = rs.Fields[i + 1].Value;
                        dgConfig.Rows[count].Cells[2].Value = description[i];
                    }

                    dgConfig.Columns[0].ReadOnly = true;
                    dgConfig.Columns[1].ReadOnly = false;
                    dgConfig.Columns[2].ReadOnly = true;

                    dgConfig.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    dgConfig.Columns[dgConfig.ColumnCount - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                    rs.Close();
                    cnDb.Close();
                    db.Dispose();
                }
                else
                {
                    BluetoothClient btClient = new BluetoothClient();
                    Guid guid = BluetoothService.SerialPort;
                    BluetoothDeviceInfo[] devices = btClient.DiscoverDevices(255, false, true, true, false);
                    string prefix;
                    int i;
                    

                    lbMsg.Text = "Selecione o dispositivo desejado.";
                    lbGrid.Text = "Dispositivos disponíveis:";
                    btnRec.Text = "Ok";
                    Text = "Seleção de dispositivos";
                    //popular a grid de parâmetros
                    dgConfig.Columns.Add("Column Name", "Dispositivo");
                    dgConfig.Columns.Add("Column Address", "Endereço");

                    for(i=devices.Length-1;i>=0;i--)
                    //foreach (BluetoothDeviceInfo device in devices)
                    {
                        //prefix = device.DeviceName.Substring(0, 4);
                        prefix = devices[i].DeviceName.Substring(0, 4);
                        if ((prefix=="AM4-") || (prefix=="RNBT") || (prefix=="AM5-"))
                        {
                            count = dgConfig.Rows.Add();
                            //dgConfig.Rows[count].Cells[0].Value = device.DeviceName;
                            //dgConfig.Rows[count].Cells[1].Value = device.DeviceAddress.ToString();
                            dgConfig.Rows[count].Cells[0].Value = devices[i].DeviceName;
                            dgConfig.Rows[count].Cells[1].Value = devices[i].DeviceAddress.ToString();

                        }
                    }


                    dgConfig.Columns[0].ReadOnly = true;
                    dgConfig.Columns[1].ReadOnly = true;

                    dgConfig.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    dgConfig.Columns[dgConfig.ColumnCount - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgConfig.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                    btClient.Close();
                    btClient.Dispose();
                }

                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dispose();
        }

        private void action()
        {
            Database db = new Database();
            Connection cnDb = new Connection();
            int i;
            db.connectToDatabase(0, ref cnDb);
            List<string> values = new List<string>();
            string saux = "";

            /*se myType=falso, então o form está sendo usado para configurações de parâmetros
            do ajustador/calibrador*/
            if (!myType)
            {
                List<string> fields = new List<string>() { "Senha" };
                for (i = 0; i < dgConfig.Rows.Count; i++)
                {
                    if (dgConfig.Rows[i].Cells[1].Value.ToString().Contains("'"))
                    {
                        saux = "'" + dgConfig.Rows[i].Cells[1].Value.ToString().Replace("'", "") + "'";
                        values.Add(saux);
                    }
                    else
                    {
                        if (Information.IsNumeric(dgConfig.Rows[i].Cells[1].Value)) values.Add(dgConfig.Rows[i].Cells[1].Value.ToString());
                        else values.Add("'" + dgConfig.Rows[i].Cells[1].Value + "'");
                    }
                }

                db.updateData(0, "Parametros", campos, values, cnDb, "WHERE Código = 1");
                values.Clear();
                MessageBox.Show("Parâmetros atualizados com sucesso!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cnDb.Close();
                db.Dispose();
            }
            /*se myType=verdade então o form está sendo usado para selecionar um novo ADR*/
            else
            {
                Recordset rs = new Recordset();
                List<string> fields = new List<string>() { "Data_", "Nome", "Senha", "Usuario" };
                string name = dgConfig.SelectedRows[0].Cells[0].Value.ToString();//nome ADR
                string address = dgConfig.SelectedRows[0].Cells[1].Value.ToString();//endereço Bluetooth ADR
                string pass = null; //senha ADR

                /*Dispositivo possui um número de série*/
                string prefix = name.Substring(0, 4);
                if ((prefix == "AM4-") || (prefix == "AM5-"))
                {
                    string condition = "WHERE Nome = " + "'" + name + "'";

                    db.getData(0, "Dispositivos", fields, cnDb, condition, ref rs);
                    /*se consulta retornar vazia é porque este dispositivo já foi cadastrado,
                    isto é, já possui um número de série, mas ainda não está cadastrado no
                    banco de dados deste sistema*/
                    if ((rs.BOF) && (rs.EOF))
                    {
                        int n;
                        do
                        {
                            /*coleta senha do dispositivo*/
                            pass = Interaction.InputBox("Este dispositivo já possui um número de série" +
                            ", mas não possui cadastro neste sistema. Favor digitar a senha do mesmo abaixo:",
                            "Senha do ADR", "");
                        } while ((pass.Length > 4) || (!int.TryParse(pass, out n)));

                        fields.Add("MACCal");

                        values.Add("'" + DateTime.Today.ToString("dd/MM/yyyy") + "'");
                        values.Add("'" + name + "'");
                        values.Add("'" + pass + "'");
                        values.Add("'" + Application.OpenForms.OfType<frMain>().FirstOrDefault().user + "'");
                        values.Add("'" + address + "'");

                        db.insertData(0, "Dispositivos", fields, values, cnDb);
                        MessageBox.Show("Dispositivo " + name + " cadastrado com sucesso!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        values.Clear();
                    }
                    /*se consulta não retornar vazia é porque o dispositivo já foi ajustado
                    alguma vez e está sendo reajustado ou recalibrado*/
                    else
                    {
                        MessageBox.Show("Dispositivo cadastrado em " +
                                        rs.Fields[0].Value + ". Operação executada por " +
                                        rs.Fields[3].Value + ".", "Aviso", MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                        pass = ((dynamic)rs.Fields[2]).Value;
                    }
                    rs.Close();
                }
                /*Dispositivo ainda não possui um número de série padrão*/
                else if (name.Substring(0, 4) == "RNBT")
                {
                    int index;
                    Random random = new Random();
                    StringBuilder builder = new StringBuilder();
                    string serie, aux = "0123456789";
                    ADR adr = new ADR(address, "1234");
                    if (!adr.connectADR())
                    {
                        MessageBox.Show("ADR desconectado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    //gera uma senha aleatória de 4 dígitos para o novo ADR
                    for (i = 0; i < 4; i++)
                    {
                        index = random.Next(0, aux.Length - 1);
                        builder = builder.Append(aux.Substring(index, 1));
                        pass = builder.ToString();
                    }
                    db.maxRecord(0, "Dispositivos", "Nome", cnDb, "", ref rs);
                    
                    serie = ((dynamic)rs.Fields[0]).Value;
                    rs.Close();
                    serie = serie.Substring(4, 4);
                    serie = (Convert.ToInt32(serie) + 1).ToString("0000");
                    
                    name = "AM4-" + serie;

                    adr.putSerialNumber(serie);
                    adr.putBtPass(pass);
                    MessageBox.Show("O dispositivo será reiniciado...", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    adr.btCon();
                    adr.disconnectADR();
                    adr.Dispose();

                    fields.Add("MACCal");

                    values.Add("'" + DateTime.Today.ToString("dd/MM/yyyy") + "'");
                    values.Add("'" + name + "'");
                    values.Add("'" + pass + "'");
                    values.Add("'" + Application.OpenForms.OfType<frMain>().FirstOrDefault().user + "'");
                    values.Add("'" + address + "'");

                    db.insertData(0, "Dispositivos", fields, values, cnDb);
                    MessageBox.Show("Dispositivo " + name + " cadastrado com sucesso!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    values.Clear();
                }

                cnDb.Close();//desconecta do banco de dados
                db.Dispose();//exclui instância de classe
            }

        }

        private void btnRec_Click(object sender, EventArgs e)
        {
            action();    
        }

        private void frConfig_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2) action();
            else if (e.KeyCode == Keys.Escape) Close();
        }

        private void CenterControlInParent(Control ctrlToCenter)
        {
            ctrlToCenter.Left = (ctrlToCenter.Parent.Width - ctrlToCenter.Width) / 2;
            ctrlToCenter.Top = (ctrlToCenter.Parent.Height - ctrlToCenter.Height) / 2;
        }

        private void frConfig_SizeChanged(object sender, EventArgs e)
        {
            CenterControlInParent(pnConfig);
        }
    }
}
