using System;
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
         Animate,
         Translate,
         Scale,
         Idle
      };
      #endregion Enumerations

      #region Private Data
      
      private Mode m_Mode;
      private int m_NumFrames;
      private bool m_DeviceLost;

      private float m_XOrigin;
      private float m_YOrigin;
      
      private Point m_InitialHandPos = new Point(0,0);
      private readonly MainWindow m_UI;

      private readonly PXCMGesture.GeoNode[] m_Data = new PXCMGesture.GeoNode[5];

      private PXCMPoint3DF32 m_lastPos;

      #endregion Private Data

      public GestureEngine(MainWindow mainWindow)
      {
         EnableGesture();
         m_Mode = Mode.Idle;
         m_NumFrames = 0;
         m_DeviceLost = false;
         m_UI = mainWindow;
         m_lastPos = new PXCMPoint3DF32();
      }

      public override void OnAlert(ref PXCMGesture.Alert data)
      {
         m_UI.SendMessage(string.Format("ALERT: {0}", data.label));

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
         m_UI.SendMessage(string.Format("GESTURE: {0}", data.label));

         PXCMGesture gesture = QueryGesture();
         PXCMGesture.GeoNode ndata;
         pxcmStatus sts = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY | PXCMGesture.GeoNode.Label.LABEL_FINGER_INDEX, out ndata);

         if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
         {
            m_InitialHandPos.X = ndata.massCenterImage.x;
            m_InitialHandPos.Y = ndata.massCenterImage.y;

            switch (data.label)
            {
               case PXCMGesture.Gesture.Label.LABEL_POSE_BIG5:
                  m_Mode = Mode.Rotate;
                  break;
               case PXCMGesture.Gesture.Label.LABEL_POSE_PEACE:
                  m_Mode = Mode.Animate;
                  break;
               case PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_UP:
                  m_Mode = Mode.Scale;
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
            if (m_Data[0].confidence > 90)
            {
               switch (m_Mode)
               {
                  case Mode.Rotate:
                     m_UI.SendMessage("Rotate");
                     double angle = new Vector3D(center.y - m_Data[1].positionImage.y, center.x - m_Data[1].positionImage.x, 0).Length/100;
                     var axis = new Vector3D(center.y - m_Data[1].positionImage.y, center.x - m_Data[1].positionImage.x, 0);
                     m_UI.Rotate(axis, angle);
                     
                     break;
                  case Mode.Animate:
                     m_UI.SendMessage("Animate");
                     m_UI.Animate(new Vector3D(0, 0, 1), 1);
                     break;

                     /* case Mode.Mouse:
                     var xPos = m_XOrigin - m_Data[0].positionImage.x + m_InitialHandPos.X;
                     var yPos = m_YOrigin + m_Data[0].positionImage.y - m_InitialHandPos.Y;
                     MouseUtilities.SetPosition((int)xPos, (int)yPos);
                     break;
                  case Mode.Translate:
                     m_UI.SendMessage("Y");
                     m_UI.Rotate(new Vector3D(0, 1, 0), 1);
                     break;
                  */
               }
            }
         }

         return (++m_NumFrames < 50000);
      }

      public override bool OnDisconnect()
      {
         if (!m_DeviceLost)
         {
            m_UI.SendMessage("Device disconnected");
         }
         m_DeviceLost = true;
         return base.OnDisconnect();
      }

      public override void OnReconnect()
      {
         m_UI.SendMessage("Device reconnected");
         m_DeviceLost = false;
      }

      public void Start()
      {
         if (!LoopFrames())
         {
            m_UI.SendMessage(String.Format("Failed to initialize or stream data"));
         }
         Dispose();
      }

      public void SetOrigin(Point pos)
      {
         m_XOrigin = (int)pos.X;
         m_YOrigin = (int)pos.Y;
      }
   }
}
