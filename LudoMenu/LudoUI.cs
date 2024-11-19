using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace LudoMenu
{
    public partial class LudoUI : Form
    {

        LudoServer mSocket = new LudoServer();
        private TcpListener server;
        private Thread serverThread;
        private LudoClient ludoClient;

        string connectionString = "Data Source=localhost;Initial Catalog=LudoDB;Integrated Security=True;Encrypt=False;TrustServerCertificate=True";

        public LudoUI()
         {
            InitializeComponent();

        }


        private void LudoUI_Load(object sender, EventArgs e)
        {
            cmbDirecciones.Items.Clear(); // Limpia cualquier valor previo
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                // Agregar tanto IPv4 como IPv6
                if (ip.AddressFamily == AddressFamily.InterNetwork || ip.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    cmbDirecciones.Items.Add(ip.ToString());
                }
            }

            cmbDirecciones.Items.Add("127.0.0.1"); // Añadir localhost para IPv4
            cmbDirecciones.Items.Add("::1"); // Añadir localhost para IPv6

            if (cmbDirecciones.Items.Count > 0)
            {
                cmbDirecciones.SelectedIndex = 0; // Seleccionar la primera IP por defecto
            }
            else
            {
                MessageBox.Show("No se encontraron direcciones IP disponibles en esta máquina.");
            }
        }


        private void IniciarServidor(string ip, int puerto)
        {
            try
            {
                server = new TcpListener(IPAddress.Parse(ip), puerto);
                server.Start();
                ActualizarTextBox($"Servidor iniciado en {ip}:{puerto}\n");

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    ActualizarTextBox("Cliente conectado.\n");

                    // Manejar cliente en un hilo separado para soportar múltiples conexiones
                    Thread clientThread = new Thread(() => ManejarCliente(client));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                ActualizarTextBox($"Error del servidor: {ex.Message}\n");
            }
        }


        private void ManejarCliente(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                // Recibir datos del cliente
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                ActualizarTextBox($"Mensaje recibido: {message}\n");

                // Enviar respuesta al cliente
                byte[] response = Encoding.ASCII.GetBytes("Conexión exitosa.");
                stream.Write(response, 0, response.Length);

                // Cerrar la conexión
                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                ActualizarTextBox($"Error manejando cliente: {ex.Message}\n");
            }
        }

        delegate void ActualizarTextBoxDelegate(string texto);
        private void ActualizarTextBox(string texto)
        {
            if (txtServidor.InvokeRequired)
            {
                txtServidor.Invoke(new ActualizarTextBoxDelegate(ActualizarTextBox), texto);
            }
            else
            {
                txtServidor.AppendText(texto);
            }
        }

        private void SeConectoClienteHandler(string pIP)
         {
             ActualizarTextBox("Cliente conectado: " + pIP + Environment.NewLine);

             //txtRecibido.Text += "Cliente conectado: " + pIP + Environment.NewLine;

         }

         private void SeRecibieronDatosHandler(string pDatos)
         {
             ActualizarTextBox("Cliente: " + pDatos + Environment.NewLine);


         }

         private void SeDesconectoClienteHandler()
         {

             ActualizarTextBox("Cliente desconectado" + Environment.NewLine);

         }


         delegate void HabilitarBotonDelegate(Button pBoton, bool pHabilitado);

         void HabilitarBoton(Button pBoton, bool pHabilitado)
         {
             if (InvokeRequired)
             {
                 this.Invoke(new HabilitarBotonDelegate(HabilitarBoton), pBoton, pHabilitado);
                 return;
             }

             pBoton.Enabled = pHabilitado;
         }

         delegate void HabilitarTextboxDelegate(TextBox pTxt, bool pHabilitado);

         void HabilitarTextbox(TextBox pTxt, bool pHabilitado)
         {
             if (InvokeRequired)
             {
                 this.Invoke(new HabilitarTextboxDelegate(HabilitarTextbox), pTxt, pHabilitado);
                 return;
             }

             pTxt.Enabled = pHabilitado;
         }




        private void btnConectar_Click(object sender, EventArgs e)
        {
            string manualIp = txtIPCliente.Text?.Trim();

            if (string.IsNullOrEmpty(manualIp))
            {
                MessageBox.Show("Por favor, ingresa una IP válida para conectarte.");
                return;
            }

            string puertoClienteTexto = txtPuertoCliente.Text?.Trim();
            int puertoCliente = 5000;  // Valor predeterminado

            if (!string.IsNullOrEmpty(puertoClienteTexto) && int.TryParse(puertoClienteTexto, out int puertoClienteIngresado))
            {
                puertoCliente = puertoClienteIngresado;
            }

            try
            {
                // Asegúrate de inicializar ludoClient
                ludoClient = new LudoClient();
                ludoClient.Connect(manualIp, puertoCliente);

                // Ahora puedes pasar ludoClient a otros formularios si es necesario
                LudoGame gameForm = new LudoGame(ludoClient); // Pasar la referencia de ludoClient
                gameForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar al servidor: {ex.Message}");
            }
        }



        private void btnIniciarServidor_Click(object sender, EventArgs e)
        {
            // Obtener el puerto del servidor desde el txtPuertoServidor
            string puertoServidorTexto = txtPuertoServidor.Text?.Trim();
            int puertoServidor = 5000;  // Valor predeterminado

            if (!string.IsNullOrEmpty(puertoServidorTexto) && int.TryParse(puertoServidorTexto, out int puertoServidorIngresado))
            {
                puertoServidor = puertoServidorIngresado;
            }

            // Obtener la IP del servidor desde el ComboBox (o el valor manual en el futuro)
            string selectedIp = cmbDirecciones.SelectedItem?.ToString();
            string ipServidor = string.IsNullOrEmpty(selectedIp) ? "127.0.0.1" : selectedIp;

            // Iniciar el servidor con la IP y el puerto personalizado
            serverThread = new Thread(() => IniciarServidor(ipServidor, puertoServidor));
            serverThread.IsBackground = true;
            serverThread.Start();
            MessageBox.Show("Servidor iniciado.");
        }

        private void txtServidor_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPuertoServidor_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
