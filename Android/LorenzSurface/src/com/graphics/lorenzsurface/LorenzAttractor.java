package com.graphics.lorenzsurface;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.FloatBuffer;
import javax.microedition.khronos.opengles.GL10;

public class LorenzAttractor {
	
	//////////////////////////////////////////////////////// Definitions
	private final int NUMPOINTS = 400;
	
	//////////////////////////////////////////////////////// Initialization
	
	/**
	 * create a Lorenz Attractor with default values
	 */
	public LorenzAttractor()
	{				
		mInitialPos = new double[3];
		mInitialPos[0] = -100;
		mInitialPos[1] = 100;
		mInitialPos[2] = -500;
		
		mSigma = 10;
		mRho   = 28;
		mBeta  = 8/3;
		
		mStepSize = .01;
		
		mIndex = 0;
		time = 0;
		CreateLorenz();
	}
	
	/**
	 * create an arbitrary Lorenz Attractor
	 * 
	 * @param x0 initial x value
	 * @param y0 initial y value
	 * @param z0 initial z value
	 * @param sigma sigma value in Lorenz Attractor equations
	 * @param rho rho value in Lorenz Attractor equations
	 * @param beta beta value in Lorenz Attractor equations
	 */
	public LorenzAttractor(double x0, double y0, double z0, double sigma, double rho, double beta, double stepSize)
	{
		mInitialPos = new double[3];
		mInitialPos[0] = x0;
		mInitialPos[1] = y0;
		mInitialPos[2] = z0;
		
		mSigma = sigma;
		mRho   = rho;
		mBeta  = beta;
		
		mStepSize = stepSize;
		
		mIndex = 0;
		time = 0;
		CreateLorenz();
	}
	
	//////////////////////////////////////////////////////// Private Methods
	
    /**
     * initialize vertices for a pyramid
     */
    private void CreatePyramid()
    {
    	float sqrt3 = 1.73205080757f;
    	
    	// Create vertices for a pyramid
    	float[][] v = new float[4][3];

		v[0][0] = -0.5f;
		v[0][1] = 0;
		v[0][2] = 0;
		
		v[1][0] = 0.5f;
		v[1][1] = 0;
		v[1][2] = 0;
		
		v[2][0] = 0;
		v[2][1] = 0f;
		v[2][2] = sqrt3/2;
		
		v[3][0] = 0;
		v[3][1] = .75f;
		v[3][2] = sqrt3/4;
		
		// Create float buffer and assign vertices
		mNumPyramidVertices = 6;
		ByteBuffer bb;
		bb = ByteBuffer.allocateDirect(v.length * v[0].length * mNumPyramidVertices);
        bb.order(ByteOrder.nativeOrder());
        mPyramid = bb.asFloatBuffer();
        
		mPyramid.put(v[0]);
		mPyramid.put(v[1]);
		mPyramid.put(v[2]);
		mPyramid.put(v[3]);
		mPyramid.put(v[0]);
		mPyramid.put(v[1]);
    }
    
    /**
     * compute positions for {@link NUMPOINTS} points
     */
    private void CreateLorenz()
    {
    	mLorenz = new double[NUMPOINTS][3];
    	double[] pos = {mInitialPos[0], mInitialPos[1], mInitialPos[2]};
    	
    	for (int i = 0; i < NUMPOINTS; i++)
    	{
        	mLorenz[i] = pos;
        	pos = RK4Lorenz(pos, mStepSize); 
    	}
    }
	
    /**
     * assigns the next value in the Lorenz integration 
     */
    private void UpdateLorenz()
    { 	
    	if (mIndex == 0)
    	{
    		mLorenz[mIndex] = RK4Lorenz(mLorenz[NUMPOINTS  - 1], mStepSize);
    	}
    	else
    	{
    		mLorenz[mIndex] = RK4Lorenz(mLorenz[mIndex     - 1], mStepSize);
    	}
    	
    	mIndex++;
    	mIndex%=NUMPOINTS;
    }
    
    /**
     * perform a Runge-Kutta integration on the Lorenz attractor given the position and time step
     * @param pos the position
     * @param dt the time step
     * @return next approximate position
     */
    private double[] RK4Lorenz(double[] pos, double dt)
    {
	   // Obtain and store first set of slopes
	   double[] f1 = Lorenz(pos);
	
	   // Compute next Euler position with first set of slopes and half time step
	   double[] xyz = Euler(pos, f1, dt/2);
	
	   // Obtain and store second set of slopes
	   double[] f2 = Lorenz(xyz);
	
	   // Compute next Euler position with second set of slopes and half time step
	   xyz = Euler(pos, f2, dt/2);
	
	   // Obtain and store third set of slopes
	   double[] f3 = Lorenz(xyz);
	
	   // Compute next Euler position with third set of slopes and full time step
	   xyz = Euler(pos, f3, dt);
	
	   // Obtain and store fourth set of slopes
	   double[] f4 = Lorenz(xyz);
	
	   // Compute weighted average of slopes according to Runge-Kutta fourth order algorithm
	   double rkSlope[] = {
	                  f1[0]/6 + f2[0]/3 + f3[0]/3 + f4[0]/6,
	                  f1[1]/6 + f2[1]/3 + f3[1]/3 + f4[1]/6, 
	                  f1[2]/6 + f2[2]/3 + f3[2]/3 + f4[2]/6 };
	
	   // Return next position using Euler with Runge-Kutta slope
	   return Euler(pos, rkSlope, dt);
    }
    
    /**
     * Returns the immediate rate of change at the given point by applying 
     * the equations for the Lorenz Attractor.
     * @param pos the current position
     * @return the rate of change
     */
	private double[] Lorenz(double[] pos)
	{
		double lorenz[] = {
				mSigma * (pos[1] - pos[0]),
				pos[0] * (mRho - pos[2]) - pos[1], 
				pos[0] * pos[1] - mBeta * pos[2], };
		
		return lorenz;
	}
	
	/**
	 * performs an Euler integration given the position, slope and time step
	 * @param pos the starting position
	 * @param slope the slope at the given position
	 * @param dt the time step
	 * @return the next Euler position
	 */
	private double[] Euler(double[] pos, double slope[], double dt)
	{
		double euler[] = {
				pos[0] + slope[0] * dt,
				pos[1] + slope[1] * dt, 
	            pos[2] + slope[2] * dt };
		
		return euler;
	}
	
    //////////////////////////////////////////////////////// Public Methods
	
    /**
     * draw a pyramid
     * @param gl the graphics object
     */
    public void DrawPyramid(GL10 gl)
    {
    	gl.glEnableClientState(GL10.GL_VERTEX_ARRAY);
    	gl.glEnableClientState(GL10.GL_NORMAL_ARRAY);    	
    	
    	// Point to first vertex in buffer
    	mPyramid.position(0);
    	
    	gl.glVertexPointer(3, GL10.GL_FLOAT, 0, mPyramid);
    	gl.glNormalPointer(GL10.GL_FLOAT, 0, mPyramid);
		gl.glDrawArrays(GL10.GL_TRIANGLE_STRIP, 0, mNumPyramidVertices);  	
    	gl.glDisableClientState(GL10.GL_VERTEX_ARRAY);
    	gl.glDisableClientState(GL10.GL_NORMAL_ARRAY);
    }
    
    /**
     * draw a pyramid
     * @param gl the graphics object
     */
    public void DrawPyramidOutline(GL10 gl)
    {
    	gl.glEnableClientState(GL10.GL_VERTEX_ARRAY);
    	gl.glEnableClientState(GL10.GL_NORMAL_ARRAY);    	
    	
    	// Point to second vertex to avoid redrawing points
    	mPyramid.position(0);
    	
    	gl.glVertexPointer(3, GL10.GL_FLOAT, 0, mPyramid);
    	gl.glNormalPointer(GL10.GL_FLOAT, 0, mPyramid);
    	gl.glPointSize(5);
		gl.glDrawArrays(GL10.GL_POINTS, 0, mNumPyramidVertices);
    	gl.glDisableClientState(GL10.GL_VERTEX_ARRAY);
    	gl.glDisableClientState(GL10.GL_NORMAL_ARRAY);
    }
    
    /**
     * Draw a Lorenz Attractor on the given graphics object
     * @param gl the graphics object
     */
    public void DrawLorenz(GL10 gl)
    {
    	if (mPyramid == null)
    	{
    		CreatePyramid();
    	}
    	
    	UpdateLorenz();

    	// Make Lorenz wobble
    	//time += .01;
    	//gl.glRotatef(90*(float)Math.sin(time), 1, 0, 0);
    	
    	for (int i=0; i<NUMPOINTS; i++)
    	{
        	gl.glPushMatrix();
        	gl.glTranslatef((float)mLorenz[i][0], (float)mLorenz[i][1], (float)mLorenz[i][2]); 	
        	DrawPyramid(gl);
        	gl.glPopMatrix();
    	}
    }
	
	/**
	 * Gets the head of the Lorenz attractor
	 * @return the most recent position calculated
	 */
	public double[] getHeadPos()
	{
		if (mIndex == 0)
		{
			return mLorenz[NUMPOINTS - 1]; 
		}
		else
		{
			return mLorenz[mIndex-1];
		}
	}
	
	/**
	 * Gets the tail of the Lorenz attractor
	 * @return the least recent position calculated
	 */
	public double[] getTailPos()
	{
		return mLorenz[(mIndex+1)%NUMPOINTS];
	}
	
	//////////////////////////////////////////////////////// Private Data
	
	private FloatBuffer  mPyramid;
	private double[][]   mLorenz;
	private int          mNumPyramidVertices;
	private double[]     mInitialPos;
	private double       mStepSize;
	private double       mSigma;
	private double       mRho;
	private double       mBeta;
	private int          mIndex;
	private float        time;
}


