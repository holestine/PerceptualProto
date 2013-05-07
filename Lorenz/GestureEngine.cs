using System;
using System.Threading;
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
         Idle
      };
      #endregion Enumerations

      #region Private Data
      private Mode m_Mode;
      private int m_NumFrames;
      private bool m_DeviceLost;
      private readonly LorenzWindow m_UI;
      private readonly PXCMGesture.GeoNode[] m_Data = new PXCMGesture.GeoNode[5];
      #endregion Private Data

      #region Initialization
      public GestureEngine(LorenzWindow lorenzWindow)
      {
         EnableGesture();
         m_Mode = Mode.Idle;
         m_NumFrames = 0;
         m_DeviceLost = false;
         m_UI = lorenzWindow;
      }

      public void Start()
      {
         if (!LoopFrames())
         {
            m_UI.Notify(String.Format("Failed to initialize or stream data"));
         }
         Dispose();
      }
      #endregion Initialization

      #region Overrides
      public override void OnAlert(ref PXCMGesture.Alert data)
      {
         m_UI.Notify(string.Format("ALERT: {0}", data.label));

         switch (data.label)
         {
            case PXCMGesture.Alert.Label.LABEL_GEONODE_INACTIVE:
               m_Mode = Mode.Idle;
               break;
         }
      }

      public override void OnGesture(ref PXCMGesture.Gesture data)
      {
         m_UI.Notify(string.Format("GESTURE: {0}", data.label));

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
         }
      }

      public override bool OnNewFrame()
      {
         PXCMGesture gesture = QueryGesture();
         pxcmStatus status = gesture.QueryNodeData(0,
             PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY,
             m_Data);

         //Imperical value roughly in the middle of the monitor (depends on where camera is pointed)
         PXCMPoint3DF32 center;
         center.x = 120;
         center.y = 100;

         if (status >= pxcmStatus.PXCM_STATUS_NO_ERROR)
         {
            switch (m_Mode)
            {
               case Mode.Rotate:
                  double angle = new Vector3D(center.y - m_Data[1].positionImage.y, center.x - m_Data[1].positionImage.x, 0).Length / 50;
                  var axis = new Vector3D(center.y - m_Data[1].positionImage.y, center.x - m_Data[1].positionImage.x, 0);
                  m_UI.Rotate(axis, angle);
                  break;
               case Mode.Follow:
                  for (int i = 0; i < 5; i++)
                  {
                     if (m_Data[i].positionImage.x > 1 || m_Data[i].positionImage.y > 1)
                     {
                        //m_UI.Notify(String.Format("{0}, {1}", m_Data[i].positionImage.x - center.x, m_Data[i].positionImage.y - center.y));
                        m_UI.Move(new Point3D((m_Data[i].positionImage.x - center.x) / 200, (m_Data[i].positionImage.y - center.y) / 200, 0));
                        break;
                     }
                  }
                  break;
               case Mode.Animate:
                  new Thread(() => m_UI.Animate()).Start();
                  break;
               case Mode.Mouse:
                  MouseUtilities.SetPosition(0, 0);
                  break;
            }
         }
         return (++m_NumFrames < 50000);
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
      #endregion Overrides
   }
}
