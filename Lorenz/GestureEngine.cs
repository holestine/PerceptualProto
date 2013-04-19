using System;
using System.Windows;

namespace Lorenz
{
   sealed class GestureEngine : UtilMPipeline
   {
      private enum Mode
      {
         Mouse,
        RotateX,
        RotateY,
        RotateZ,
         Idle
      };

      #region Private Data
      
      private Mode m_Mode;
      private int m_NumFrames;
      private bool m_DeviceLost;

      private float m_XOrigin;
      private float m_YOrigin;
      
      private Point m_InitialHandPos = new Point(0,0);
      private readonly MainWindow m_UI;

      private readonly PXCMGesture.GeoNode[] m_Data;
      #endregion Private Data

      public GestureEngine(MainWindow mainWindow)
      {
         EnableGesture();
         m_Mode = Mode.Idle;
         m_NumFrames = 0;
         m_DeviceLost = false;
         m_UI = mainWindow;
         m_Data = new PXCMGesture.GeoNode[5];
      }

      public override void OnAlert(ref PXCMGesture.Alert data)
      {
          m_Mode = Mode.Idle;
         m_UI.SendMessage(string.Format("ALERT: {0}", data.label));

         switch (data.label)
         {
            case PXCMGesture.Alert.Label.LABEL_GEONODE_INACTIVE:
               m_Mode = Mode.Idle;
               //m_UI.SendMessage = "IDLE MODE";
               break;
            default:
               //m_UI.SendMessage = "Right Click";
               MouseUtilities.RightClick(new Point(0, 0));
               break;
         }
      }

      public override void OnGesture(ref PXCMGesture.Gesture data)
      {
         if (data.active)
         {
            m_UI.SendMessage(string.Format("GESTURE: {0}", data.label));
         }

         PXCMGesture gesture = QueryGesture();
         PXCMGesture.GeoNode ndata;
         pxcmStatus sts = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY | PXCMGesture.GeoNode.Label.LABEL_FINGER_INDEX, out ndata);

         if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
         {
            m_InitialHandPos.X = ndata.massCenterImage.x;
            m_InitialHandPos.Y = ndata.massCenterImage.y;

            switch (data.label)
            {
               case PXCMGesture.Gesture.Label.LABEL_POSE_PEACE:
                  //MouseUtilities.Click(new Point(0, 0));
                    m_Mode = Mode.RotateX;
                  break;
               case PXCMGesture.Gesture.Label.LABEL_POSE_BIG5:
                  m_Mode = Mode.RotateY;
                  break;
               case PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_UP:
                  m_Mode = Mode.RotateZ;
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

         if (status >= pxcmStatus.PXCM_STATUS_NO_ERROR)
         {
            if (m_Data[0].confidence > 90)
            {
               // TODO: Improve mouse positioning
               if (m_Mode == Mode.Mouse)
               {
                  var xPos = m_XOrigin - m_Data[0].positionImage.x + m_InitialHandPos.X;
                  var yPos = m_YOrigin + m_Data[0].positionImage.y - m_InitialHandPos.Y;
                  //m_UI.SendMessage = String.Format("New Mouse Position ({0}, {1})", xPos, yPos);
                  MouseUtilities.SetPosition((int)xPos, (int)yPos);
               }
               switch (m_Mode)
                {
                        case Mode.Mouse:
                        break;
                       case Mode.RotateX:
                        m_UI.RotateX();
                        break;
                       case Mode.RotateY:
                        m_UI.RotateY();
                        break;
                       case Mode.RotateZ:
                        m_UI.RotateZ();
                        break;
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
