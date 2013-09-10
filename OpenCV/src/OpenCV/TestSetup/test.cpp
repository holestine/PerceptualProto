//#include <iostream>	// for standard I/O
//#include <string>   // for strings
//#include <iomanip>  // for controlling float print precision
//#include <sstream>  // string to number conversion

#include <opencv2/imgproc/imgproc.hpp>  
#include <opencv2/core/core.hpp>        // Basic OpenCV structures (cv::Mat, Scalar)
#include <opencv2/highgui/highgui.hpp>  // OpenCV window I/O

using namespace std;
using namespace cv;

#define SRC "Source Image"
#define DST "Warped Image"
int WarpVideo()
{
   Point2f pts1[] = {Point2f(150,150.), Point2f(150,300.), Point2f(350,300.), Point2f(350,150.)};
   Point2f pts2[] = {Point2f(200,200.), Point2f(150,300.), Point2f(350,300.), Point2f(300,200.)};

   Mat perspective_matrix = getPerspectiveTransform(pts1, pts2);
   Mat dst_img;
   Mat src_img;

   VideoCapture cap(0); // open the default camera
   if(!cap.isOpened())  // check if we succeeded
   {
      return -1;
   }

   for(;;)
   {
      cap >> src_img; // get a new frame from camera
      warpPerspective(src_img, dst_img, perspective_matrix, src_img.size(), INTER_LINEAR);

      namedWindow(SRC, CV_WINDOW_AUTOSIZE|CV_WINDOW_FREERATIO);
      namedWindow(DST, CV_WINDOW_AUTOSIZE|CV_WINDOW_FREERATIO);
      imshow(SRC, src_img);
      imshow(DST, dst_img);
      if(waitKey(30) >= 0) break;
   }
   // the camera will be deinitialized automatically in VideoCapture destructor
   return 0;
}

int main(int argc, char *argv[])
{
   WarpVideo();
}

