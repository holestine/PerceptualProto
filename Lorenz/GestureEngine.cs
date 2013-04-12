using System;
using System.Windows;

namespace Lorenz
{
   class GestureEngine : UtilMPipeline
   {
      protected int NumFrames;
      protected bool DeviceLost;

      private float m_XOrigin;
      private float m_YOrigin;
      
      private Point m_InitialHandPos = new Point(0,0);
      private readonly MainWindow m_UI;

      public GestureEngine(MainWindow mainWindow)
      {
         EnableGesture();
         NumFrames = 0;
         DeviceLost = false;
         m_UI = mainWindow;
      }

      public override void OnGesture(ref PXCMGesture.Gesture data)
      {
         if (data.active)
         {
            m_UI.Messages = "OnGesture(" + data.label + ")";
         }

         PXCMGesture gesture = QueryGesture();
         PXCMGesture.GeoNode ndata;
         pxcmStatus sts = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY, out ndata);

         m_InitialHandPos.X=ndata.massCenterImage.x;
         m_InitialHandPos.Y = ndata.massCenterImage.y;

         switch (data.label)
         {
            case PXCMGesture.Gesture.Label.LABEL_POSE_PEACE:
               MouseUtilities.Click(new Point(0, 0));
               break;
            default:
               MouseUtilities.RightClick(new Point(0, 0));
               break;
         }
      }

      public override bool OnDisconnect()
      {
         if (!DeviceLost)
         {
            m_UI.Messages = "Device disconnected";
         }
         DeviceLost = true;
         return base.OnDisconnect();
      }

      public override void OnReconnect()
      {
         m_UI.Messages = "Device reconnected";
         DeviceLost = false;
      }

      public override bool OnNewFrame()
      {
         PXCMGesture gesture = QueryGesture();
         PXCMGesture.GeoNode ndata;
         pxcmStatus sts = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY, out ndata);
         if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
         {
            if (ndata.confidence > 90)
            {
               // TODO: Need to improve mouse positioning
               MouseUtilities.SetPosition((int)(m_XOrigin - ndata.massCenterImage.x + m_InitialHandPos.X), (int)(m_YOrigin + ndata.massCenterImage.y - m_InitialHandPos.Y));   
            }
            m_UI.Messages = String.Format("node HAND_MIDDLE ({0},{1})", ndata.positionImage.x, ndata.positionImage.y);
         }

         return (++NumFrames < 50000);
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
