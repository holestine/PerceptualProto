package com.graphics.lorenzsurface;

import com.graphics.lorenzsurface.R;
import android.app.Activity;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;

/**
 * 
 * @author Brian Holestine
 *
 */
public class LorenzActivity extends Activity {
    
	//////////////////////////////////////////////////////// Overrides
	@Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        mGLSurfaceView = new LorenzView(this);
        mGLSurfaceView.setRenderer(new LorenzRenderer());
        setContentView(mGLSurfaceView);
    }

    @Override
    protected void onResume() {
        super.onResume();
    }

    @Override
    protected void onPause() {
        super.onPause();
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        MenuInflater inflater = getMenuInflater();
        inflater.inflate(R.menu.activity_main, menu);
        return true;
    }
    
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle item selection
        switch (item.getItemId()) {
            case R.id.menu_settings:
                //ShowSettings();
                return true;
            case R.id.menu_restart:
                //Restart();
                return true;
            case R.id.menu_motionType:
                //ChangeMotion();
                return true;
            default:
                return super.onOptionsItemSelected(item);
        }
    }
    
    //////////////////////////////////////////////////////// Private Data
    private LorenzView mGLSurfaceView;
}

