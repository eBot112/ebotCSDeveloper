using DirectShowLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace eBotLib.Capture
{
    /// <summary>
    /// スナップショット撮像完了イベント引数
    /// </summary>
    public class SnapShotCompleteEventArgs
    {
        /// <summary>スナップショット</summary>
        public Bitmap Bitmap;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bitmap">撮像したスナップショット</param>
        public SnapShotCompleteEventArgs(Bitmap bitmap)
        {
            if (null == bitmap)
            {
                this.Bitmap = null;
            }
            else
            {
                this.Bitmap = new Bitmap(bitmap);
            }
        }
    }

    /// <summary>
    /// キャプチャプレビュー表示クラス
    /// </summary>
    public partial class DirectCapturePictureBox : PictureBox, ISampleGrabberCB
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dst, IntPtr src, uint cnt);


        private const int WM_GRAPHNOTIFY = 0x00008001;

        /// <summary>スナップショット撮像完了通知デリゲート</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        public delegate void SnapShotCompleteEventHandler<T>(T args);
        /// <summary>スナップショット撮像完了通知イベント</summary>
        public event SnapShotCompleteEventHandler<SnapShotCompleteEventArgs> SnapShotCompleteHandler;

        private delegate void CaptureDoneDelegate(IntPtr buff);

        private DsDevice selectedDevice = null;
        private IGraphBuilder graphBuilder;
        private ICaptureGraphBuilder2 captureGraphBuilder;
        private ISampleGrabber sampleGrabber;
        private VideoInfoHeader videoInfoHeader;
        private bool captureRequest = false;
        private IntPtr buffCache = IntPtr.Zero;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DirectCapturePictureBox()
        {
        }

        /// <summary>
        /// Dispose処理
        /// </summary>
        protected new void Dispose()
        {
            try
            {
                disposeItems();
            }
            finally
            {
                base.Dispose();
            }
        }

        /// <summary>
        /// 資源の開放
        /// </summary>
        private void disposeItems()
        {
            if (this.graphBuilder != null)
            {
                int hr = ((IMediaControl)this.graphBuilder).StopWhenReady();
                DsError.ThrowExceptionForHR(hr);
                Marshal.ReleaseComObject(this.graphBuilder);
            }
            if (this.captureGraphBuilder != null) Marshal.ReleaseComObject(this.captureGraphBuilder);
            if (this.sampleGrabber != null) Marshal.ReleaseComObject(this.sampleGrabber);
        }

        /// <summary>
        /// Windows メッセージ処理
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_GRAPHNOTIFY)
            {
                if (graphBuilder == null)
                {
                    this.OnGraphNotify();
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        /// <summary>
        /// DirectShow イベント処理
        /// </summary>
        public void OnGraphNotify()
        {
            EventCode code;
            IntPtr param1, param2;

            while (true)
            {
                if (0 > ((IMediaEventEx)graphBuilder).GetEvent(out code, out param1, out param2, 0)) break;
                if (0 > ((IMediaEventEx)graphBuilder).FreeEventParams(code, param1, param2)) break;
            }
        }

        int ISampleGrabberCB.SampleCB(double sampleTime, IMediaSample pSample)
        {
            return 0;
        }

        int ISampleGrabberCB.BufferCB(double sampleTime, IntPtr pBuffer, int bufferLength)
        {
            IntPtr buff = IntPtr.Zero;

            if (!captureRequest) return 0;
            captureRequest = false;

            try
            {
                if ((pBuffer != IntPtr.Zero) && (bufferLength >= videoInfoHeader.BmiHeader.ImageSize))
                {
                    buff = Marshal.AllocCoTaskMem(bufferLength);
                    CopyMemory(buff, pBuffer, (uint)bufferLength);
                }
                this.BeginInvoke(new CaptureDoneDelegate(this.OnCaptureDone), buff);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            return 0;
        }

        /// <summary>
        /// キャプチャデバイスの初期化
        /// </summary>
        /// <param name="deviceNum">キャプチャデバイス番号</param>
        public void InitDevice(int deviceNum)
        {
            int hr;
            object captureObject = null;
            IBaseFilter grabFilter;

            // 初期化済みの場合は先に資源を破棄
            disposeItems();

            // デバイス選択
            DsDevice[] dsDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            if (deviceNum >= dsDevices.Length)
            {
                throw new NotSupportedException("The selected capture device cannot be found.");
            }
            selectedDevice = dsDevices[deviceNum];

            this.graphBuilder = (IGraphBuilder)new FilterGraph();
            this.captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
            this.sampleGrabber = (ISampleGrabber)new SampleGrabber();

            grabFilter = (IBaseFilter)sampleGrabber;

            AMMediaType amMediaType = new AMMediaType();
            amMediaType.majorType = MediaType.Video;
            amMediaType.subType = MediaSubType.RGB24;
            amMediaType.formatType = FormatType.VideoInfo;

            Guid guidBF = typeof(IBaseFilter).GUID;
            selectedDevice.Mon.BindToObject(null, null, ref guidBF, out captureObject);

            hr = sampleGrabber.SetMediaType(amMediaType);
            Marshal.ThrowExceptionForHR(hr);
            hr = captureGraphBuilder.SetFiltergraph(graphBuilder);
            Marshal.ThrowExceptionForHR(hr);
            hr = graphBuilder.AddFilter(((IBaseFilter)captureObject), "Video Capture Device");
            Marshal.ThrowExceptionForHR(hr);
            hr = graphBuilder.AddFilter(grabFilter, "Frame Grab Filter");
            Marshal.ThrowExceptionForHR(hr);
            hr = captureGraphBuilder.RenderStream(PinCategory.Capture, MediaType.Video, ((IBaseFilter)captureObject), null, grabFilter);
            Marshal.ThrowExceptionForHR(hr);
            hr = captureGraphBuilder.RenderStream(PinCategory.Preview, MediaType.Video, ((IBaseFilter)captureObject), null, null);
            Marshal.ThrowExceptionForHR(hr);

            amMediaType = new AMMediaType();
            hr = sampleGrabber.GetConnectedMediaType(amMediaType);
            Marshal.ThrowExceptionForHR(hr);
            if ((amMediaType.formatType != FormatType.VideoInfo) || (amMediaType.formatPtr == IntPtr.Zero))
            {
                throw new NotSupportedException("This media format is can't be captured.");
            }
            videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(amMediaType.formatPtr, typeof(VideoInfoHeader));
            Marshal.FreeCoTaskMem(amMediaType.formatPtr);
            amMediaType.formatPtr = IntPtr.Zero;

            hr = sampleGrabber.SetBufferSamples(false);
            Marshal.ThrowExceptionForHR(hr);
            hr = sampleGrabber.SetOneShot(false);
            Marshal.ThrowExceptionForHR(hr);
            hr = sampleGrabber.SetCallback(null, 0);
            Marshal.ThrowExceptionForHR(hr);

            hr = ((IVideoWindow)graphBuilder).put_Owner(this.Handle);
            DsError.ThrowExceptionForHR(hr);
            hr = ((IVideoWindow)graphBuilder).put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren);
            DsError.ThrowExceptionForHR(hr);
            hr = ((IVideoWindow)graphBuilder).SetWindowPosition(
                0,
                0,
                this.ClientSize.Width,
                this.ClientSize.Height
                );
            DsError.ThrowExceptionForHR(hr);

            hr = ((IMediaControl)graphBuilder).Run();
            DsError.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// スナップショット取得要求
        /// </summary>
        public void RequestCaptureSnapshot()
        {
            if (null == videoInfoHeader) return;

            int hr;
            int size = videoInfoHeader.BmiHeader.ImageSize;

            captureRequest = true;
            hr = sampleGrabber.SetCallback(this, 1);
            DsError.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// スナップショットの取得完了
        /// </summary>
        /// <param name="buff">画像バッファ</param>
        private void OnCaptureDone(IntPtr buff)
        {
            if (null == sampleGrabber ||
                null == SnapShotCompleteHandler ||
                IntPtr.Zero == buff)
            {
                return;
            }

            if (IntPtr.Zero != buffCache)
            {
                Marshal.FreeCoTaskMem(buffCache);
            }
            buffCache = buff;

            Bitmap bitmap = null;
            try
            {
                int hr = sampleGrabber.SetCallback(null, 0);
                DsError.ThrowExceptionForHR(hr);

                int width = videoInfoHeader.BmiHeader.Width;
                int height = videoInfoHeader.BmiHeader.Height;
                int stride = width * 3;

                buff += (height - 1) * stride;

                bitmap = new Bitmap(width, height, -stride, PixelFormat.Format24bppRgb, (IntPtr)buff);
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to take a snapshot." + e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // スナップショットを通知
                var args = new SnapShotCompleteEventArgs(bitmap);
                if (null != SnapShotCompleteHandler)
                {
                    SnapShotCompleteHandler(args);
                }
                bitmap.Dispose();
            }
        }

        /// <summary>
        /// キャプチャデバイス一覧の取得
        /// </summary>
        /// <returns></returns>
        public static string[] GetCaptureDevices()
        {
            string[] devices;

            DsDevice[] dsDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            devices = new string[dsDevices.Length];
            foreach (var d in dsDevices.Select((v, i) => new { val = v, idx = i }))
            {
                devices[d.idx] = d.val.Name;
            }

            return devices;
        }
    }
}
