using System;
using System.Windows;

namespace Lorenz
{
   sealed class GestureEngine : UtilMPipeline
   {
      private enum Mode
      {
         Mouse,
         Idle
      };

      private Mode m_Mode;
      private int m_NumFrames;
      private bool m_DeviceLost;

      private float m_XOrigin;
      private float m_YOrigin;
      
      private Point m_InitialHandPos = new Point(0,0);
      private readonly MainWindow m_UI;

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
         switch (data.label)
         {
            case PXCMGesture.Alert.Label.LABEL_GEONODE_INACTIVE:
               m_Mode = Mode.Idle;
               m_UI.Messages = "IDLE MODE";
               break;
            default:
               m_UI.Messages = "Right Click";
               MouseUtilities.RightClick(new Point(0, 0));
               break;
         }
      }

      public override void OnGesture(ref PXCMGesture.Gesture data)
      {
         if (data.active)
         {
            m_UI.Messages = "OnGesture(" + data.label + ")";
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
                  m_UI.Messages = "Click";
                  MouseUtilities.Click(new Point(0, 0));
                  break;
               case PXCMGesture.Gesture.Label.LABEL_POSE_BIG5:
                  m_Mode = Mode.Mouse;
                  m_UI.Messages = "MOUSE MODE";
                  break;
               default:
                  m_UI.Messages = "Right Click";
                  MouseUtilities.RightClick(new Point(0, 0));
                  break;
            }
         }
      }

      public override bool OnDisconnect()
      {
         if (!m_DeviceLost)
         {
            m_UI.Messages = "Device disconnected";
         }
         m_DeviceLost = true;
         return base.OnDisconnect();
      }

      public override void OnReconnect()
      {
         m_UI.Messages = "Device reconnected";
         m_DeviceLost = false;
      }

      public override bool OnNewFrame()
      {
         PXCMGesture gesture = QueryGesture();
         var data = new PXCMGesture.GeoNode[5];

         pxcmStatus sts = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY | PXCMGesture.GeoNode.Label.LABEL_FINGER_INDEX, data);

        if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
         {
            if (data[0].confidence > 90)
            {
               // TODO: Improve mouse positioning
               if (m_Mode == Mode.Mouse)
               {
                  var xPos = m_XOrigin - data[0].positionImage.x + m_InitialHandPos.X;
                  var yPos = m_YOrigin + data[0].positionImage.y - m_InitialHandPos.Y;
                  m_UI.Messages = String.Format("New Mouse Position ({0}, {1})", xPos, yPos);
                  MouseUtilities.SetPosition((int)xPos, (int)yPos);
               }
            }
         }

         return (++m_NumFrames < 50000);
      }

      public void Start()
      {
         if (!LoopFrames())
         {
            m_UI.Messages = String.Format("Failed to initialize or stream data");
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
