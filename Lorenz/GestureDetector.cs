using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lorenz
{
   class GestureDetector
   {
      private PXCMGesture.Gesture.OnGesture mOnGesture;

      public GestureDetector(PXCMGesture.Gesture.OnGesture OnGesture)
      {
         mOnGesture = OnGesture;
      }

      public void Start()
      {
         // Wire gesture handler
         PXCMSession session;
         pxcmStatus sts = PXCMSession.CreateInstance(out session);
         if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
         {
            Console.WriteLine("Failed to create the SDK session");
            return;
         }

         // Gesture Module
         PXCMBase gesture;
         sts = session.CreateImpl(PXCMGesture.CUID, out gesture);
         if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
         {
            Console.WriteLine("Failed to load any gesture recognition module");
            session.Dispose();
            return;
         }
         var gesture_t = (PXCMGesture)gesture;

         PXCMGesture.ProfileInfo pinfo;
         sts = gesture_t.QueryProfile(0, out pinfo);

         var capture = new UtilMCapture(session);
         sts = capture.LocateStreams(ref pinfo.inputs);
         if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
         {
            Console.WriteLine("Failed to locate a capture module");
            gesture_t.Dispose();
            capture.Dispose();
            session.Dispose();
            return;
         }
         sts = gesture_t.SetProfile(ref pinfo);
         sts = gesture_t.SubscribeGesture(100, mOnGesture);

         bool deviceLost = false;
         var images = new PXCMImage[PXCMCapture.VideoStream.STREAM_LIMIT];
         var sps = new PXCMScheduler.SyncPoint[2];

         for (int nframes = 0; nframes < 50000; nframes++)
         {
            sts = capture.ReadStreamAsync(images, out sps[0]);
            if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
               if (sts == pxcmStatus.PXCM_STATUS_DEVICE_LOST)
               {
                  if (!deviceLost) Console.WriteLine("Device disconnected");
                  deviceLost = true; nframes--;
                  continue;
               }
               Console.WriteLine("Device failed\n");
               break;
            }
            if (deviceLost)
            {
               Console.WriteLine("Device reconnected");
               deviceLost = false;
            }

            sts = gesture_t.ProcessImageAsync(images, out sps[1]);
            if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

            PXCMScheduler.SyncPoint.SynchronizeEx(sps);
            if (sps[0].Synchronize(0) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
               PXCMGesture.GeoNode data;
               sts = gesture_t.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY | PXCMGesture.GeoNode.Label.LABEL_HAND_MIDDLE, out data);
               if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                  Console.WriteLine("[node] {0}, {1}, {2}", data.positionImage.x, data.positionImage.y, data.timeStamp);
            }

            foreach (PXCMScheduler.SyncPoint s in sps) if (s != null) s.Dispose();
            foreach (PXCMImage i in images) if (i != null) i.Dispose();
         }

         gesture_t.Dispose();
         capture.Dispose();
         session.Dispose();
      }
   }
}
