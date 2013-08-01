package com.graphics.lorenzsurface;

import android.content.Context;
import android.view.MotionEvent;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import android.view.View;
import android.view.View.OnTouchListener;

/**
 * 
 * @author Brian Holestine
 *
 */
class LorenzView extends SurfaceView implements SurfaceHolder.Callback, Runnable, OnTouchListener {
    
	//////////////////////////////////////////////////////// Definitions
	enum Mode{ROTATE, ZOOM};
	private static final int INVALID_POINTER_ID = -1;
	private static final double TOUCH_SCALE_FACTOR = .2;
	
	//////////////////////////////////////////////////////// Initialization	
	public LorenzView(Context context) {
        super(context);
        mHolder = getHolder();
        mHolder.addCallback(this);
        //mHolder.setType(SurfaceHolder.SURFACE_TYPE_GPU);
    }

	//////////////////////////////////////////////////////// Overrides
    @Override
    public void surfaceCreated(SurfaceHolder holder) {
    	this.setOnTouchListener(this);
    }

    @Override
    public void surfaceDestroyed(SurfaceHolder holder) {
        running = false;
        try {
            thread.join();
        } catch (InterruptedException e) {
        }
        thread = null;
    }

    @Override
    public void surfaceChanged(SurfaceHolder holder, int format, int w, int h) {
    	synchronized(this){
	    	mWidth = w;
	    	mHeight = h;
	        thread = new Thread(this);
	        thread.start();
    	}
    }

    @Override
	public void run() {
        synchronized(this) {
	        mRenderer.EGLCreate(mHolder);
	        mRenderer.Initialize(mWidth, mHeight);
	        running=true;
	        
	        while (running) {
	        	mRenderer.DrawScene(mWidth, mHeight);
	        }

	        mRenderer.EGLDestroy();
		}
	}

	@Override
	public boolean onTouch(View v, MotionEvent event) {
				
		final int action = event.getAction();
		
        switch (event.getAction() & MotionEvent.ACTION_MASK) {
        	// First finger goes down
	        case MotionEvent.ACTION_DOWN: {
	            final float x = event.getX();
	            final float y = event.getY();
	            
	            mPreviousX = x;
	            mPreviousY = y;
	
	            // Save the ID of this pointer
	            mActivePointerId = event.getPointerId(0);
	            break;
	        }
        	// Go to Zoom mode when second finger goes down
	        case MotionEvent.ACTION_POINTER_DOWN:
	        	mode = Mode.ZOOM;
	        	
	        	// Store values needed for zooming
	        	mInitialPinchSize = getDistance(event);
	        	mInitialZoomFactor = mRenderer.GetZoomFactor();
	        	
	            break;
	        
	        // Go back to Rotate mode when second finger goes up
	        case MotionEvent.ACTION_POINTER_UP:
	        	mode = Mode.ROTATE;
	        	// Extract the index of the pointer that left the touch sensor
	             int pointerIndex = (action & MotionEvent.ACTION_POINTER_INDEX_MASK)  >> MotionEvent.ACTION_POINTER_INDEX_SHIFT;
	            final int pointerId = event.getPointerId(pointerIndex);
	            if (pointerId == mActivePointerId) {
	                // This was our active pointer going up. Choose a new
	                // active pointer and adjust accordingly.
	                final int newPointerIndex = pointerIndex == 0 ? 1 : 0;
	                mPreviousX = event.getX(newPointerIndex);
	                mPreviousY = event.getY(newPointerIndex);
	                mActivePointerId = event.getPointerId(newPointerIndex);
	            }
	            break;
	            
	        case MotionEvent.ACTION_UP: {
	            mActivePointerId = INVALID_POINTER_ID;
	            break;
	        }
	            
	        case MotionEvent.ACTION_CANCEL: {
	            mActivePointerId = INVALID_POINTER_ID;
	            break;
	        }
	        
	        case MotionEvent.ACTION_MOVE:
	        	if (mode == Mode.ROTATE)
	            {
	        		// Find the index of the active pointer and fetch its position
	                pointerIndex = event.findPointerIndex(mActivePointerId);
	                final float x = event.getX(pointerIndex);
	                final float y = event.getY(pointerIndex);
	                
	        		mRenderer.Rotate((x - mPreviousX) * TOUCH_SCALE_FACTOR, (y - mPreviousY) * TOUCH_SCALE_FACTOR);
	        		
	                mPreviousX = x;
	                mPreviousY = y;
	            }
	        	else if (mode == Mode.ZOOM)
	        	{
	        		mRenderer.Zoom(mInitialZoomFactor * getDistance(event)/mInitialPinchSize);
	        	}
	            break;
        }
        
        invalidate();
        
        return true;
	}

	//////////////////////////////////////////////////////// Interfaces
    public interface Renderer {
    	void EGLCreate(SurfaceHolder holder);
    	void EGLDestroy();
    	int Initialize(int width, int height);
    	void DrawScene(int width, int height);
    }
    
    //////////////////////////////////////////////////////// Private Methods
	private float getDistance(MotionEvent event) 
	{
		   float x = event.getX(0) - event.getX(1);
		   float y = event.getY(0) - event.getY(1);
		   return (float) Math.sqrt(x * x + y * y);
	}
	
	//////////////////////////////////////////////////////// Public Methods
    public void setRenderer(LorenzRenderer renderer) {
    	mRenderer = renderer;
    }

    //////////////////////////////////////////////////////// Private Data
    private SurfaceHolder mHolder;
    private Thread thread;
    private boolean running;
    private LorenzRenderer mRenderer;
    private int mWidth;
    private int mHeight;
    private float mPreviousX;
    private float mPreviousY;
    private Mode mode = Mode.ROTATE;
    private float mInitialPinchSize = 0;
    private float mInitialZoomFactor = 1;
    private int mActivePointerId;
    
}

//////////////////////////////////////////////////////// Notes
// Initial work to get translation working with zooming
//import android.graphics.Point;

// Store values needed for translating
//mInitialMidpoint = getMidpoint(event);

/*	private Point getMidpoint(MotionEvent event) 
{
	Point midPoint = new Point();
	midPoint.x = (int) ((event.getX(0) + event.getX(1))/2);
	midPoint.y = (int) ((event.getY(0) + event.getY(1))/2);
	return midPoint;
}
*/

//private Point mInitialMidpoint;

//Point midpoint = getMidpoint(event);
//mRenderer.mXTranslation = -(mInitialMidpoint.x - midpoint.x)/10;
//mRenderer.mYTranslation = (mInitialMidpoint.y - midpoint.y)/10;
