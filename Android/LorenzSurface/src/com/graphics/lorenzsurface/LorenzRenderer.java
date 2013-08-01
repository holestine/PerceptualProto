package com.graphics.lorenzsurface;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.FloatBuffer;

import javax.microedition.khronos.egl.EGL10;
import javax.microedition.khronos.egl.EGLConfig;
import javax.microedition.khronos.egl.EGLContext;
import javax.microedition.khronos.egl.EGLDisplay;
import javax.microedition.khronos.egl.EGLSurface;
import javax.microedition.khronos.opengles.GL10;
import android.view.SurfaceHolder;

class LorenzRenderer implements LorenzView.Renderer {
	
	//////////////////////////////////////////////////////// Initialization	
	public LorenzRenderer() {    
		xRotation = 0;
		yRotation = 0;
		mZoomFactor = .25f;
		//time = 0;
    }
	
	//////////////////////////////////////////////////////// Overrides	
    @Override
    public int Initialize(int width, int height)
    {
    	gl.glLoadIdentity();
		gl.glViewport(0, 0, width, height);

		gl.glMatrixMode(GL10.GL_PROJECTION);
		Perspectivef(90.0f, (float)width / (float)height, 0.1f, 500.0f);
		
		gl.glMatrixMode(GL10.GL_MODELVIEW);
    	gl.glEnable(GL10.GL_DEPTH_TEST);
    	gl.glDepthFunc(GL10.GL_LESS);
    	
    	gl.glEnable(GL10.GL_CULL_FACE);	
    	gl.glEnable(GL10.GL_MULTISAMPLE);
    	gl.glEnable(GL10.GL_LIGHTING);	
    	gl.glEnable(GL10.GL_LIGHT0);	
    	
    	gl.glEnable (GL10.GL_BLEND);
    	gl.glBlendFunc (GL10.GL_SRC_ALPHA, GL10.GL_ONE_MINUS_DST_ALPHA);
    	gl.glShadeModel(GL10.GL_SMOOTH);
    	
		// Lighting Conditions
    	float[] light_amb  = {1.0f, 1.0f, 1.0f, 1.0f, };
		float[] light_diff = {0.5f, 0.5f, 0.5f, 1.0f, };
		float[] light_spec = {1.0f, 1.0f, 1.0f, 1.0f, };
		float[] light_pos  = {5.0f, 10.0f, -10.0f, 0.0f, };
		
        ByteBuffer abb = ByteBuffer.allocateDirect(light_amb.length*4);
		abb.order(ByteOrder.nativeOrder());
		LIGHT_AMBIENT_BUFFER = abb.asFloatBuffer();
		LIGHT_AMBIENT_BUFFER.put(light_amb);
		LIGHT_AMBIENT_BUFFER.position(0);    	
		
		ByteBuffer dbb = ByteBuffer.allocateDirect(light_diff.length*4);
		dbb.order(ByteOrder.nativeOrder());
		LIGHT_DIFFUSE_BUFFER = dbb.asFloatBuffer();
		LIGHT_DIFFUSE_BUFFER.put(light_diff);
		LIGHT_DIFFUSE_BUFFER.position(0);    	
		
		ByteBuffer sbb = ByteBuffer.allocateDirect(light_spec.length*4);
		sbb.order(ByteOrder.nativeOrder());
		LIGHT_SPECULAR_BUFFER = sbb.asFloatBuffer();
		LIGHT_SPECULAR_BUFFER.put(light_spec);
		LIGHT_SPECULAR_BUFFER.position(0);  

		ByteBuffer pbb = ByteBuffer.allocateDirect(light_pos.length*4);
		pbb.order(ByteOrder.nativeOrder());
		LIGHT_POS_BUFFER = pbb.asFloatBuffer();
		LIGHT_POS_BUFFER.put(light_pos);
		LIGHT_POS_BUFFER.position(0);
		
		// Material for light bulb
    	float[] buld_amb  = {1.0f, 1.0f, 0.0f, 0.75f, };
		float[] bulb_diff = {0.8f, 0.8f, 0.1f, 0.75f, };
		float[] bulb_spec = {1.0f, 1.0f, 1.0f, 0.75f, };
		
        ByteBuffer babb = ByteBuffer.allocateDirect(buld_amb.length*4);
        babb.order(ByteOrder.nativeOrder());
		BULB_MATERIAL_AMBIENT_BUFFER = babb.asFloatBuffer();
		BULB_MATERIAL_AMBIENT_BUFFER.put(buld_amb);
		BULB_MATERIAL_AMBIENT_BUFFER.position(0);    	
		
		ByteBuffer bdbb = ByteBuffer.allocateDirect(bulb_diff.length*4);
		bdbb.order(ByteOrder.nativeOrder());
		BULB_MATERIAL_DIFFUSE_BUFFER = bdbb.asFloatBuffer();
		BULB_MATERIAL_DIFFUSE_BUFFER.put(bulb_diff);
		BULB_MATERIAL_DIFFUSE_BUFFER.position(0);    	
		
		ByteBuffer bsbb = ByteBuffer.allocateDirect(bulb_spec.length*4);
		bsbb.order(ByteOrder.nativeOrder());
		BULB_MATERIAL_SPECULAR_BUFFER = bsbb.asFloatBuffer();
		BULB_MATERIAL_SPECULAR_BUFFER.put(bulb_spec);
		BULB_MATERIAL_SPECULAR_BUFFER.position(0);
		
		// Material for light bulb outline
    	float[] buld_outline_amb  = {1.0f, 0.5f, 0.0f, 1.0f, };
		float[] bulb_outline_diff = {1.0f, 0.3f, 0.0f, 1.0f, };
		float[] bulb_outline_spec = {1.0f, 0.0f, 0.0f, 1.0f, };
		
        ByteBuffer boabb = ByteBuffer.allocateDirect(buld_outline_amb.length*4);
        boabb.order(ByteOrder.nativeOrder());
		BULB_OUTLINE_MATERIAL_AMBIENT_BUFFER = boabb.asFloatBuffer();
		BULB_OUTLINE_MATERIAL_AMBIENT_BUFFER.put(buld_outline_amb);
		BULB_OUTLINE_MATERIAL_AMBIENT_BUFFER.position(0);    	
		
		ByteBuffer bodbb = ByteBuffer.allocateDirect(bulb_outline_diff.length*4);
		bodbb.order(ByteOrder.nativeOrder());
		BULB_OUTLINE_MATERIAL_DIFFUSE_BUFFER = bodbb.asFloatBuffer();
		BULB_OUTLINE_MATERIAL_DIFFUSE_BUFFER.put(bulb_outline_diff);
		BULB_OUTLINE_MATERIAL_DIFFUSE_BUFFER.position(0);    	
		
		ByteBuffer bosbb = ByteBuffer.allocateDirect(bulb_outline_spec.length*4);
		bosbb.order(ByteOrder.nativeOrder());
		BULB_OUTLINE_MATERIAL_SPECULAR_BUFFER = bosbb.asFloatBuffer();
		BULB_OUTLINE_MATERIAL_SPECULAR_BUFFER.put(bulb_outline_spec);
		BULB_OUTLINE_MATERIAL_SPECULAR_BUFFER.position(0);
		
    	// Material for lorenz attractor
    	float[] lorenz_amb  = {0.2f, 0.4f, 0.9f, 1.0f,};
		float[] lorenz_diff = {0.1f, 1.0f, 0.2f, 1.0f,};
		float[] lorenz_spec = {1.0f, 1.0f, 1.0f, 1.0f,}; 
		
        ByteBuffer labb = ByteBuffer.allocateDirect(lorenz_amb.length*4);
        labb.order(ByteOrder.nativeOrder());
		LORENZ_MATERIAL_AMBIENT_BUFFER = labb.asFloatBuffer();
		LORENZ_MATERIAL_AMBIENT_BUFFER.put(lorenz_amb);
		LORENZ_MATERIAL_AMBIENT_BUFFER.position(0);    	
		
		ByteBuffer ldbb = ByteBuffer.allocateDirect(lorenz_diff.length*4);
		ldbb.order(ByteOrder.nativeOrder());
		LORENZ_MATERIAL_DIFFUSE_BUFFER = ldbb.asFloatBuffer();
		LORENZ_MATERIAL_DIFFUSE_BUFFER.put(lorenz_diff);
		LORENZ_MATERIAL_DIFFUSE_BUFFER.position(0);    	
		
		ByteBuffer lsbb = ByteBuffer.allocateDirect(lorenz_spec.length*4);
		lsbb.order(ByteOrder.nativeOrder());
		LORENZ_MATERIAL_SPECULAR_BUFFER = lsbb.asFloatBuffer();
		LORENZ_MATERIAL_SPECULAR_BUFFER.put(lorenz_spec);
		LORENZ_MATERIAL_SPECULAR_BUFFER.position(0);
		
    	// Set up lighting
     	gl.glLightfv(GL10.GL_LIGHT0, GL10.GL_AMBIENT,  LIGHT_AMBIENT_BUFFER);
     	gl.glLightfv(GL10.GL_LIGHT0, GL10.GL_DIFFUSE,  LIGHT_DIFFUSE_BUFFER);
     	gl.glLightfv(GL10.GL_LIGHT0, GL10.GL_SPECULAR, LIGHT_SPECULAR_BUFFER);
     	gl.glLightfv(GL10.GL_LIGHT0, GL10.GL_POSITION, LIGHT_POS_BUFFER);
     	
        return 1;
	}
    
    @Override
    public void DrawScene(int width, int height)
    {    	
    	gl.glClear(GL10.GL_COLOR_BUFFER_BIT | GL10.GL_DEPTH_BUFFER_BIT);

    	gl.glLoadIdentity();

    	LookAt(0.0f, 0.0f, -20.0f,   // Camera Position
    		   0.0f, 0.0f,  0.0f,    // Camera Target Position
     		   0.0f, 1.0f,  0.0f);   // Camera Up Vector

     	// Draw Lorenz
     	gl.glPushMatrix();
     		gl.glRotatef(xRotation, 0, 1, 0);
     		gl.glRotatef(yRotation, 1, 0, 0);
	    	gl.glScalef(mZoomFactor, mZoomFactor, mZoomFactor);
	    	gl.glMaterialfv(GL10.GL_FRONT_AND_BACK, GL10.GL_AMBIENT, LORENZ_MATERIAL_AMBIENT_BUFFER);	
	    	gl.glMaterialfv(GL10.GL_FRONT_AND_BACK, GL10.GL_DIFFUSE, LORENZ_MATERIAL_DIFFUSE_BUFFER);
	    	gl.glMaterialfv(GL10.GL_FRONT_AND_BACK, GL10.GL_SPECULAR, LORENZ_MATERIAL_SPECULAR_BUFFER);
	    	gl.glMaterialf(GL10.GL_FRONT_AND_BACK,  GL10.GL_SHININESS, 64.0f);
	    	lorenz.DrawLorenz(gl);
    	gl.glPopMatrix();
    	
    	/*
    	// Draw Light
    	gl.glPushMatrix();
	    	gl.glTranslatef(LIGHT_POS_BUFFER.get(0), LIGHT_POS_BUFFER.get(1), LIGHT_POS_BUFFER.get(2));
	    	gl.glRotatef(time++, 1, 1, 0);
	    	//gl.glScalef(5, 5, 5);
	    	gl.glMaterialfv(GL10.GL_FRONT_AND_BACK , GL10.GL_AMBIENT, BULB_MATERIAL_AMBIENT_BUFFER);
	    	gl.glMaterialfv(GL10.GL_FRONT_AND_BACK , GL10.GL_DIFFUSE, BULB_MATERIAL_DIFFUSE_BUFFER);
	    	gl.glMaterialfv(GL10.GL_FRONT_AND_BACK , GL10.GL_SPECULAR, BULB_MATERIAL_SPECULAR_BUFFER);
	    	lorenz.DrawPyramid(gl);
	    	gl.glMaterialfv(GL10.GL_FRONT_AND_BACK , GL10.GL_AMBIENT, BULB_OUTLINE_MATERIAL_AMBIENT_BUFFER);
	    	gl.glMaterialfv(GL10.GL_FRONT_AND_BACK , GL10.GL_DIFFUSE, BULB_OUTLINE_MATERIAL_DIFFUSE_BUFFER);
	    	gl.glMaterialfv(GL10.GL_FRONT_AND_BACK , GL10.GL_SPECULAR, BULB_OUTLINE_MATERIAL_SPECULAR_BUFFER);
	    	lorenz.DrawPyramidOutline(gl);
    	gl.glPopMatrix();
    	*/
    	
    	// Swap Buffers
		egl.eglSwapBuffers(eglDisplay, eglSurface);
	}
    
    @Override
    public void EGLCreate(SurfaceHolder holder){
        int[] num_config = new int[1];
        EGLConfig[] configs = new EGLConfig[1];
        int[] configSpec = {
        	EGL10.EGL_RED_SIZE,		8,
        	EGL10.EGL_GREEN_SIZE,	8,
        	EGL10.EGL_BLUE_SIZE,	8,
        	EGL10.EGL_DEPTH_SIZE,	16,
        	EGL10.EGL_SURFACE_TYPE,	EGL10.EGL_WINDOW_BIT,
            EGL10.EGL_NONE
        };

        this.egl = (EGL10) EGLContext.getEGL();

        eglDisplay = this.egl.eglGetDisplay(EGL10.EGL_DEFAULT_DISPLAY);
        this.egl.eglInitialize(eglDisplay, null);
        
        this.egl.eglChooseConfig(eglDisplay, configSpec, configs, 1, num_config);
        eglConfig = configs[0];
        eglContext = this.egl.eglCreateContext(eglDisplay, eglConfig, EGL10.EGL_NO_CONTEXT, null);
    	eglSurface = this.egl.eglCreateWindowSurface(eglDisplay, eglConfig, holder, null);
        this.egl.eglMakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext);

        gl = (GL10)eglContext.getGL();
        
	}
    
    @Override
	public void EGLDestroy(){
        if (eglSurface != null) {
            egl.eglMakeCurrent(eglDisplay, EGL10.EGL_NO_SURFACE,
                    EGL10.EGL_NO_SURFACE, EGL10.EGL_NO_CONTEXT);
            egl.eglDestroySurface(eglDisplay, eglSurface);
            eglSurface = null;
        }
        if (eglContext != null) {
            egl.eglDestroyContext(eglDisplay, eglContext);
            eglContext = null;
        }
        if (eglDisplay != null) {
            egl.eglTerminate(eglDisplay);
            eglDisplay = null;
        }
	}

    //////////////////////////////////////////////////////// Private Methods	
    /**
     * Create frustum for scene
     * @param fovy Vertical field of view
     * @param aspect The aspect ratio
     * @param zNear The near clipping plane
     * @param zFar The far clipping plane
     */
    private void Perspectivef(float fovy, float aspect, float zNear, float zFar)
    {
    	float top, bottom;
    	float left, right;

    	top = (float)Math.tan(fovy / 2.0f * Math.PI / 180.0f) * zNear;
    	bottom = -top;
    	left = aspect * bottom;
    	right = -left;

    	gl.glFrustumf(left, right, bottom, top, zNear, zFar);
    }

    /**
     * Normalizes the incoming vector to unit length
     * @param v The vector to normalize
     */
    private static void normalize(float[] v)
    {
    	float magnitude = (float)Math.sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
    	
    	if (magnitude == 0.0) {
    		return;
    	}

    	v[0] /= magnitude;
    	v[1] /= magnitude;
    	v[2] /= magnitude;
    }

    /**
     * Calculate the cross product of two 3 dimensional vectors
     * @param v1 The first vector
     * @param v2 The second vector
     * @return v1 X v2
     */
    private static float[] cross(float[] v1, float[] v2)
    {
    	return new float[]{v1[1] * v2[2] - v1[2] * v2[1], 
    			           v1[2] * v2[0] - v1[0] * v2[2], 
    			           v1[0] * v2[1] - v1[1] * v2[0]};
    }

    /**
     * 
     * @param eyex The x location of the camera
     * @param eyey The y location of the camera
     * @param eyez The z location of the camera
     * @param targetx The x location of the target
     * @param targety The x location of the target
     * @param targetz The x location of the target
     * @param upx The x component of the up vector
     * @param upy The y component of the up vector
     * @param upz The z component of the up vector
     */
    private void LookAt(float eyex,    float eyey,    float eyez, 
    		            float targetx, float targety, float targetz, 
    		            float upx,     float upy,     float upz)
    {
    	float[] view, up, side;
    	float[][] m;

    	// Determine direction of view
    	view = new float[]{
    		targetx - eyex,
    		targety - eyey,
    		targetz - eyez
    	};
    	
    	// Create up vector
    	up = new float[]{
    		upx,
    		upy,
    		upz
    	};
    	
    	// Convert to unit vectors
    	normalize(view);
    	normalize(up);

    	// Calculate third orthonormal vector
    	side = cross(view, up);

    	m = new float[][]{
        		new float[]{side[0], up[0], -view[0], 0.0f},
        		new float[]{side[1], up[1], -view[1], 0.0f},
        		new float[]{side[2], up[2], -view[2], 0.0f},
        		new float[]{0.0f, 0.0f, 0.0f, 1.0f}
    	};

        ByteBuffer mbb = ByteBuffer.allocateDirect(m.length * m[0].length * 4);
        mbb.order(ByteOrder.nativeOrder());
        FloatBuffer mBuf = mbb.asFloatBuffer();
        for(int i = 0; i < m.length; i++){
        	mBuf.put(m[i]);
        }
        mBuf.position(0);    	
        
    	gl.glMultMatrixf(mBuf);
    	
    	gl.glTranslatef(-eyex, -eyey, -eyez);
    }

    //////////////////////////////////////////////////////// Public Methods	
    public void Rotate(double xDeltaTheta, double yDeltaTheta)
    {
    	xRotation += xDeltaTheta;
    	yRotation += yDeltaTheta;
    }
    
    public void Zoom(float zoomFactor) {
		mZoomFactor = zoomFactor;
	}

	public float GetZoomFactor() {
		return mZoomFactor;
	}
	
    //////////////////////////////////////////////////////// Private Data
	private EGL10 egl;
	private GL10 gl;
	private EGLDisplay eglDisplay;
	private EGLConfig  eglConfig;
	private EGLContext eglContext;
	private EGLSurface eglSurface;
	private LorenzAttractor lorenz = new LorenzAttractor();
	private float xRotation;
	private float yRotation;
	private float mZoomFactor;
	//private float time;
	
	FloatBuffer LIGHT_AMBIENT_BUFFER;
	FloatBuffer LIGHT_DIFFUSE_BUFFER;
	FloatBuffer LIGHT_SPECULAR_BUFFER;
	FloatBuffer LIGHT_POS_BUFFER;
	
	FloatBuffer BULB_MATERIAL_AMBIENT_BUFFER;
	FloatBuffer BULB_MATERIAL_DIFFUSE_BUFFER;
	FloatBuffer BULB_MATERIAL_SPECULAR_BUFFER;
	
	FloatBuffer BULB_OUTLINE_MATERIAL_AMBIENT_BUFFER;
	FloatBuffer BULB_OUTLINE_MATERIAL_DIFFUSE_BUFFER;
	FloatBuffer BULB_OUTLINE_MATERIAL_SPECULAR_BUFFER;
	
	FloatBuffer LORENZ_MATERIAL_AMBIENT_BUFFER;
	FloatBuffer LORENZ_MATERIAL_DIFFUSE_BUFFER;
	FloatBuffer LORENZ_MATERIAL_SPECULAR_BUFFER;
}

