//#include <iostream>	// for standard I/O
//#include <string>   // for strings
//#include <iomanip>  // for controlling float print precision
//#include <sstream>  // string to number conversion

#include <opencv2/imgproc/imgproc.hpp>  // Gaussian Blur
#include <opencv2/core/core.hpp>        // Basic OpenCV structures (cv::Mat, Scalar)
#include <opencv2/highgui/highgui.hpp>  // OpenCV window I/O

using namespace std;
using namespace cv;


int Warp(Mat src_img)
{
  //Mat src_img = imread("Mason.jpg", 1);
  if(!src_img.data) return -1;

  Point2f pts1[] = {Point2f(150,150.), Point2f(150,300.), Point2f(350,300.), Point2f(350,150.)};
  Point2f pts2[] = {Point2f(200,200.), Point2f(150,300.), Point2f(350,300.), Point2f(300,200.)};

  Mat perspective_matrix = getPerspectiveTransform(pts1, pts2);
  Mat dst_img;
  warpPerspective(src_img, dst_img, perspective_matrix, src_img.size(), INTER_LINEAR);

  namedWindow("src", CV_WINDOW_AUTOSIZE|CV_WINDOW_FREERATIO);
  namedWindow("dst", CV_WINDOW_AUTOSIZE|CV_WINDOW_FREERATIO);
  imshow("src", src_img);
  imshow("dst", dst_img);
  //waitKey(0);
  return 0;
}

int Capture()
{
    VideoCapture cap(0); // open the default camera
    if(!cap.isOpened())  // check if we succeeded
        return -1;

    Mat edges;
    namedWindow("edges",1);
    for(;;)
    {
        Mat frame;
        cap >> frame; // get a new frame from camera
        cvtColor(frame, edges, CV_BGR2GRAY);
        GaussianBlur(edges, edges, Size(7,7), 1.5, 1.5);
        Canny(edges, edges, 0, 30, 3);
        imshow("edges", edges);
        Warp(frame);
        if(waitKey(30) >= 0) break;
    }
    // the camera will be deinitialized automatically in VideoCapture destructor
    return 0;
}

int main(int argc, char *argv[])
{
   Capture();
   //Warp();
}

