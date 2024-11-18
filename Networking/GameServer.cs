using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Networking
{
    public class GameServer
    {
        #region Declaraciones
        private int mPuertoEscucha;
        private TcpListener mEscuchador;
        private static TcpClient mTcpClient;
        private static NetworkStream mNetworkStream;
        private System.Threading.Thread mThread;

        public delegate void SeConectoClienteEventHandler(string pIP);

        public event SeConectoClienteEventHandler SeConectoCliente;

        public event SeRecibieronDatosEventHandler SeRecibieronDatos;

        public delegate void SeRecibieronDatosEventHandler(string pDatos);

        public event SeDesconectoClienteEventHandler SeDesconectoCliente;

        public delegate void SeDesconectoClienteEventHandler();

        #endregion

        #region Propiedades
        public int PuertoEscucha
        {
            get
            {
                return mPuertoEscucha;
            }

            set
            {
                mPuertoEscucha = value;
            }
        }

        #endregion

        #region Métodos

        [Obsolete]
        public void EscucharPuerto()
        {
            if (mPuertoEscucha != 0)
            {
                try
                {

                    mEscuchador = new TcpListener(PuertoEscucha);
                    mEscuchador.Start();
                }
                catch
                {
                    throw;
                }

                mThread = new System.Threading.Thread(EsperarDatos);
                mThread.Start();
            }
        }

        public void DetenerEscucha()
        {
            LiberarTodo();
        }

        bool mDetener = false;
        public void EsperarDatos()
        {
            try
            {
                while (true)
                {
                    mTcpClient = mEscuchador.AcceptTcpClient(); //Acá se detiene la ejecución esperando un cliente. Por eso invocamos "EsperarDatos" desde otro Thread
                    if (SeConectoCliente != null)
                        SeConectoCliente.Invoke(mTcpClient.Client.RemoteEndPoint.ToString());
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

                    LiberarCliente();
                    if (SeDesconectoCliente != null)
                        SeDesconectoCliente.Invoke();
                }
            }
            catch
            {
                if (mThread != null && mThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    LiberarCliente();
                    if (SeDesconectoCliente != null)
                        SeDesconectoCliente.Invoke();
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

        public string[] ObtenerDireccionesLocales()
        {
            var mDireccionesIP = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
            string[] mDireccionesStr;
            mDireccionesStr = new string[mDireccionesIP.Length];
            for (int x = 0, loopTo = mDireccionesIP.Length - 1; x <= loopTo; x++)
                mDireccionesStr[x] = mDireccionesIP[x].ToString();
            return mDireccionesStr;
        }

        private void LiberarCliente()
        {
            if (mTcpClient != null)
            {
                if (mNetworkStream != null)
                {
                    mNetworkStream.Close();
                    mNetworkStream.Dispose();
                    mNetworkStream = null;
                }

                if (mTcpClient.Connected)
                {
                    mTcpClient.Client.Shutdown(SocketShutdown.Both);
                    mTcpClient.Close();
                }

                mTcpClient = null;
            }
        }

        private void LiberarTodo()
        {
            LiberarCliente();
            if (mThread != null)
            {
                //mThread.Abort();
                mDetener = true;
                mThread = null;
            }

            if (mEscuchador != null)
            {
                mEscuchador.Stop();
            }
        }

        #endregion

    }
}
