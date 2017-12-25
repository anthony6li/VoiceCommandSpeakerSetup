using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Net;
using System.Threading;

namespace AudioServerBeta
{
    public class iSpyServer
    {
        private TcpListener myListener = null;
        private AudioServerBetaDemo Parent;
        private Thread th = null;

        public bool Running
        {
            get
            {
                if (th ==null)
                {
                    return  false;
                }
                else
                {
                    return th.IsAlive;
                }
            }
        }

        public iSpyServer(AudioServerBetaDemo _parent)
        {
            Parent = _parent;
        }

        public void StartServer()
        {
            try
            {
                myListener = new TcpListener(IPAddress.Any, 8092) { ExclusiveAddressUse = false };
                myListener.Start(200);
                if (th!=null)
                {
                    while (th.ThreadState == ThreadState.AbortRequested)
                    {
                        Application.DoEvents();
                    }
                }
                th = new Thread(new ThreadStart(StartListen));
                th.Start();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(""+e.Message);
            }
        }

        public void StartListen()
        {
            while (Running)
            {
                try
                {

                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("" + e.Message);
                }
            }
        }
    }
}
