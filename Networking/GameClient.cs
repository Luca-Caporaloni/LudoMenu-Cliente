using System;
using System.Net.Sockets;
using System.Text;

namespace Networking
{
    public class GameClient
    {
        #region Declaraciones
        private static TcpClient mTcpClient;
        private string mIPRemota;
        private string mPuertoRemoto;
        private static NetworkStream mNetworkStream;
        private System.Threading.Thread mThread;

        public event SeRecibieronDatosEventHandler SeRecibieronDatos;

        public delegate void SeRecibieronDatosEventHandler(string pDatos);

        public event DesconectadoEventHandler Desconectado;

        public delegate void DesconectadoEventHandler();
        #endregion

        #region Propiedades
        public int PuertoRemoto
        {
            get
            {
                return int.Parse(mPuertoRemoto);
            }

            set
            {
                mPuertoRemoto = value.ToString();
            }
        }

        public string IPRemota
        {
            get
            {
                return mIPRemota;
            }

            set
            {
                mIPRemota = value;
            }
        }

        public string DireccionRemota
        {
            get
            {
                return mIPRemota;
            }
        }

        public Action<string> OnMessageReceived { get; set; }
        public Action<string> SeConectoCliente { get; set; }
        public Action SeDesconectoCliente { get; set; }
        #endregion

        #region Métodos
        public void Conectar()
        {
            try
            {
                mTcpClient = new TcpClient();
                mTcpClient.Connect(mIPRemota, int.Parse(mPuertoRemoto));
                mNetworkStream = mTcpClient.GetStream();
                mIPRemota = mTcpClient.Client.RemoteEndPoint.ToString();
            }
            catch
            {
                throw;
            }

            mThread = new System.Threading.Thread(EsperarDatos);
            mThread.Start();
        }

        public void EsperarDatos()
        {
            try
            {
                if (mTcpClient != null && mTcpClient.Connected)
                {
                    while (true)
                    {

                        // Quedamos a la espera de datos recibidos
                        mNetworkStream = mTcpClient.GetStream();
                        if (mNetworkStream == null | mTcpClient.Connected == false | !mNetworkStream.CanRead)
                        {
                            break;
                        }

                        if (mTcpClient.Available > 0)
                        {
                            var mBytes = new byte[mTcpClient.ReceiveBufferSize + 1];
                            if (mNetworkStream.Read(mBytes, 0, mTcpClient.ReceiveBufferSize) <= 0)
                            {
                                break;
                            }

                            string mDatosRecibidos = Encoding.ASCII.GetString(mBytes);
                            if (SeRecibieronDatos != null)
                                SeRecibieronDatos.Invoke(mDatosRecibidos);
                        }

                    }
                }

                if (Desconectado != null)
                    Desconectado.Invoke();
                LiberarTodo();
            }
            catch (Exception e)
            {
                if (mThread != null && mThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    if (Desconectado != null)
                        Desconectado.Invoke();
                    LiberarTodo();
                    throw;
                }
            }
        }

        public void EnviarDatos(string pDatos)
        {
            if (mNetworkStream != null && mNetworkStream.CanWrite)
            {
                var sendBytes = Encoding.ASCII.GetBytes(pDatos);
                mNetworkStream.Write(sendBytes, 0, sendBytes.Length);
            }
        }

        public void Desconectar()
        {
            if (mTcpClient != null && mTcpClient.Connected)
            {
                if (Desconectado != null)
                    Desconectado.Invoke();
            }

            LiberarTodo();
        }

        private void LiberarTodo()
        {
            if (mNetworkStream != null)
            {
                mNetworkStream.Close();
                mNetworkStream.Dispose();
                mNetworkStream = null;
            }

            if (mTcpClient != null)
            {
                if (mTcpClient.Connected)
                {
                    mTcpClient.Client.Shutdown(SocketShutdown.Both);
                    mTcpClient.Client.Close();
                }

                mTcpClient = null;
            }

            if (mThread != null)
            {
                mThread.Abort();
                mThread = null;
            }
        }


        #endregion

    }
}
