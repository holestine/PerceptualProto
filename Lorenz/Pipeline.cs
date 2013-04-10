using System;

namespace Lorenz
{
   class Pipeline : UtilMPipeline
   {
      protected int nframes;
      protected bool device_lost;

      public Pipeline() : base()
      {
         EnableGesture();
         nframes = 0;
         device_lost = false;
      }
      public override void OnGesture(ref PXCMGesture.Gesture data)
      {
         if (data.active) Console.WriteLine("OnGesture(" + data.label + ")");
      }
      public override bool OnDisconnect()
      {
         if (!device_lost) Console.WriteLine("Device disconnected");
         device_lost = true;
         return base.OnDisconnect();
      }
      public override void OnReconnect()
      {
         Console.WriteLine("Device reconnected");
         device_lost = false;
      }
      public override bool OnNewFrame()
      {
         PXCMGesture gesture = QueryGesture();
         PXCMGesture.GeoNode ndata;
         pxcmStatus sts = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY, out ndata);
         if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            Console.WriteLine("node HAND_MIDDLE ({0},{1})", ndata.positionImage.x, ndata.positionImage.y);
         return (++nframes < 50000);
      }
      public void Start()
      {
         if (!LoopFrames())
            Console.WriteLine("Failed to initialize or stream data");
         Dispose();
      }
   }
}
