using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TitanWrapper
{
    public class Wrapper
    {
        public TitanOne oneApi = null;
        
        public Wrapper()
        {
        }

        public void Init()
        {
            if (null != oneApi)
            {
                oneApi.Unload();
            }
            oneApi = new TitanOne();

            if (!oneApi.Init())
            {
                throw new Exception("Could not load gcdapi.dll and it's functions");
            }
            if (!oneApi.IsConnected)
            {
                throw new Exception("Could not connect to Titan One device");
            }

            System.Diagnostics.Debug.WriteLine(String.Format("Output Type is: {0}",
                Enum.GetName(typeof(TitanOne.DEVICE_TYPE), oneApi.CurrentOutputType)));
        }

        public void InputButton(int button, int state)
        {
            if (null == oneApi) return;

            oneApi.SetOutputIdentifier(button, state);
        }

        public void ClearButton(int state)
        {
            if (null == oneApi) return;

            oneApi.ClearOutputIdentifier(state);
        }

        public bool IsConnected()
        {
            if (null == oneApi) return false;
            return oneApi.IsConnected;
        }
    }
}