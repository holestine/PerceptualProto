using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Lorenz
{
    sealed class GestureEngine : UtilMPipeline
    {
        #region Enumerations
        private enum Mode
        {
            Mouse,
            Rotate,
            Follow,
            Animate,
            Scale,
            Idle
        };
        #endregion Enumerations

        #region Private Data

        private Mode m_Mode;
        private int m_NumFrames;
        private bool m_DeviceLost;
        private readonly MainWindow m_UI;
        private readonly PXCMGesture.GeoNode[] m_Data = new PXCMGesture.GeoNode[5];

        #endregion Private Data

        public GestureEngine(MainWindow mainWindow)
        {
            EnableGesture();
            m_Mode = Mode.Idle;
            m_NumFrames = 0;
            m_DeviceLost = false;
            m_UI = mainWindow;
        }

        public override void OnAlert(ref PXCMGesture.Alert data)
        {
            m_UI.Notify(string.Format("ALERT: {0}", data.label));

            switch (data.label)
            {
                case PXCMGesture.Alert.Label.LABEL_GEONODE_INACTIVE:
                    m_Mode = Mode.Idle;
                    break;
                default:
                    break;
            }
        }

        public override void OnGesture(ref PXCMGesture.Gesture data)
        {
            m_UI.Notify(string.Format("GESTURE: {0}", data.label));

            PXCMGesture gesture = QueryGesture();
            PXCMGesture.GeoNode ndata;
            pxcmStatus sts = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY | PXCMGesture.GeoNode.Label.LABEL_FINGER_INDEX | PXCMGesture.GeoNode.Label.LABEL_FINGER_THUMB, out ndata);

            //if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                switch (data.label)
                {
                    case PXCMGesture.Gesture.Label.LABEL_POSE_BIG5:
                        if (m_Mode != Mode.Rotate)
                        {
                            m_UI.Notify("Rotate Mode");
                        }
                        m_Mode = Mode.Rotate;
                        break;
                    case PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_UP:
                        if (m_Mode != Mode.Follow)
                        {
                            m_UI.Notify("Follow Mode");
                        }
                        m_Mode = Mode.Follow;
                        break;
                    case PXCMGesture.Gesture.Label.LABEL_POSE_PEACE:
                        if (m_Mode != Mode.Animate)
                        {
                            m_UI.Notify("Animate Mode");
                        }
                        m_Mode = Mode.Animate;
                        break;
                    default:
                        break;
                }
            }
        }
        
        public override bool OnNewFrame()
        {
            PXCMGesture gesture = QueryGesture();
            pxcmStatus status = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY | PXCMGesture.GeoNode.Label.LABEL_FINGER_INDEX, m_Data);
            PXCMPoint3DF32 center;
            center.x = 120;
            center.y = 90;

            if (status >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                if (m_Data[0].confidence > 95)
                {
                    switch (m_Mode)
                    {
                        case Mode.Rotate:
                            double angle = new Vector3D(center.y - m_Data[1].positionImage.y, center.x - m_Data[1].positionImage.x, 0).Length / 100;
                            var axis = new Vector3D(center.y - m_Data[1].positionImage.y, center.x - m_Data[1].positionImage.x, 0);
                            m_UI.Rotate(axis, angle);
                            break;
                        case Mode.Follow:
                            for (int i = 0; i < 5; i++)
                            {
                                if (m_Data[i].positionImage.x > 1 || m_Data[i].positionImage.y > 1)
                                {
                                    m_UI.Animate(new Point3D(50 + m_Data[i].positionImage.x / 10, 50 + m_Data[i].positionImage.y / 10, 50));
                                }
                            }
                            break;
                        case Mode.Animate:
                            var workerThread = new BackgroundWorker();
                            workerThread.DoWork += OnAnimate;
                            //workerThread.RunWorkerCompleted += UpdateUI;
                            workerThread.RunWorkerAsync();
                            break;
                     case Mode.Mouse:
                        MouseUtilities.SetPosition(0, 0);
                        break;
                    }
                }
            }
            return (++m_NumFrames < 50000);
        }

        private void OnAnimate(object sender, DoWorkEventArgs e)
        {
           m_UI.Animate(250);
        }

        public override bool OnDisconnect()
        {
            if (!m_DeviceLost)
            {
                m_UI.Notify("Device disconnected");
            }
            m_DeviceLost = true;
            return base.OnDisconnect();
        }

        public override void OnReconnect()
        {
            m_UI.Notify("Device reconnected");
            m_DeviceLost = false;
        }

        public void Start()
        {
            if (!LoopFrames())
            {
                m_UI.Notify(String.Format("Failed to initialize or stream data"));
            }
            Dispose();
        }
    }
}
