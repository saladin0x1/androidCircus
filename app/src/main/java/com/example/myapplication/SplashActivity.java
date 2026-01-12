package com.example.myapplication;

import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;
import androidx.appcompat.app.AppCompatActivity;
import com.example.myapplication.api.SessionManager;

public class SplashActivity extends AppCompatActivity {

    private static final int SPLASH_DURATION = 2000; // 2 seconds

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_splash);

        new Handler().postDelayed(new Runnable() {
            @Override
            public void run() {
                SessionManager sessionManager = new SessionManager(SplashActivity.this);
                
                Intent intent;
                if (sessionManager.isLoggedIn()) {
                    String role = sessionManager.getUserRole();
                    if ("Doctor".equalsIgnoreCase(role)) {
                        intent = new Intent(SplashActivity.this, DoctorHomeActivity.class);
                    } else if ("Clerk".equalsIgnoreCase(role)) {
                        intent = new Intent(SplashActivity.this, ClerkHomeActivity.class);
                    } else {
                        intent = new Intent(SplashActivity.this, HomeActivity.class);
                    }
                } else {
                    intent = new Intent(SplashActivity.this, SignInActivity.class);
                }
                startActivity(intent);
                finish();
            }
        }, SPLASH_DURATION);
    }
}
