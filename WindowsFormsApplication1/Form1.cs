using System;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Media;
using SerialPortListener.Serial;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {

        private SoundPlayer soundPlayer;
        private string soundsPath;

        private SerialPortManager _spManager;
        private SerialSettings mySerialSettings;

        public Form1()
        {
            InitializeComponent();
            UserInitialization();
        }

        #region Methods

        private void UserInitialization()
        {
            _spManager = new SerialPortManager();
            mySerialSettings = _spManager.CurrentSerialSettings;

            _spManager.NewSerialDataRecieved += new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved);
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);

            refreshCOMList();

            soundPlayer = new SoundPlayer();
            soundsPath = Application.StartupPath + "\\sounds\\";
            Console.WriteLine("Exec path: \n" + soundsPath);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            refreshCOMList();

            return;
        }

        private void refreshCOMList()
        {
            comboBox1.Items.Clear();

            foreach (string s in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(s);
            }

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;

            return;
        }

        private void btnConectar_Click(object sender, EventArgs e)
        {

            #region Button Conectar
        
            if(comboBox1.SelectedIndex == -1)
            {
                textBoxReceber.AppendText("Selecione uma porta!");
                return;
            }
            
                mySerialSettings.PortName = comboBox1.Items[comboBox1.SelectedIndex].ToString();

            if (!_spManager.isOpen)
            {
                try
                {
                    textBoxReceber.AppendText("Conectando a porta " + mySerialSettings.PortName + " ...");
                    _spManager.StartListening();

                }
                catch (IOException ex)
                {
                    textBoxReceber.AppendText("Não foi possível conectar: ");
                    Console.WriteLine(ex.ToString());
                    return;
                }

                if (_spManager.isOpen)
                {
                    btnConectar.Text = "Desconectar";
                    textBoxReceber.AppendText("Conectado.\n Dados recebidos: ");
                    comboBox1.Enabled = false;
                }
            }
            else
            {
                try
                {
                    _spManager.StopListening();
                    comboBox1.Enabled = true;
                    btnConectar.Text = "Conectar";
                    textBoxReceber.AppendText("Desconectado.");
                }
                catch
                {
                    return;
                }
            }
            #endregion

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _spManager.Dispose();
        }

        private void trataDadoRecebido(string dado)
        {
            if (dado.Equals("1"))
            {
                soundPlayer.SoundLocation = soundsPath + "o.wav";
                soundPlayer.Play();
            }
        }

        #endregion

        #region Events Handlers

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _spManager.Dispose();
        }

        void _spManager_NewSerialDataRecieved(object sender, SerialDataEventArgs e)
        {
            if (this.InvokeRequired)
            {
                // Using this.Invoke causes deadlock when closing serial port, and BeginInvoke is good practice anyway.
                this.BeginInvoke(new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved), new object[] { sender, e });
                return;
            }

            int maxTextLength = 1000; // maximum text length in text box
            if (textBoxReceber.TextLength > maxTextLength)
                textBoxReceber.Text = textBoxReceber.Text.Remove(0, textBoxReceber.TextLength - maxTextLength);

            // This application is connected to a GPS sending ASCCI characters, so data is converted to text
            string str = Encoding.ASCII.GetString(e.Data);

            trataDadoRecebido(str);

            textBoxReceber.AppendText(str + " ");
            textBoxReceber.ScrollToCaret();
     
        }
        #endregion

    }
}
