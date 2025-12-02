package com.example.myapplication;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import androidx.appcompat.app.AppCompatActivity;

public class HomeActivity extends AppCompatActivity {

    private TextView welcomeText;
    private Button profileButton, logoutButton;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_home);

        welcomeText = findViewById(R.id.welcomeText);
        profileButton = findViewById(R.id.profileButton);
        logoutButton = findViewById(R.id.logoutButton);

        // Get user email from SharedPreferences
        SharedPreferences prefs = getSharedPreferences("MedicalCabinetPrefs", MODE_PRIVATE);
        String userEmail = prefs.getString("userEmail", "");

        // Get user details from database
        DatabaseHelper databaseHelper = new DatabaseHelper(this);
        User user = databaseHelper.getUserByEmail(userEmail);

        if (user != null) {
            welcomeText.setText("Bienvenue, " + user.getName());
        }

        profileButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(HomeActivity.this, ProfileActivity.class);
                startActivity(intent);
            }
        });

        logoutButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // Clear login state
                SharedPreferences prefs = getSharedPreferences("MedicalCabinetPrefs", MODE_PRIVATE);
                SharedPreferences.Editor editor = prefs.edit();
                editor.putBoolean("isLoggedIn", false);
                editor.putString("userEmail", "");
                editor.apply();

                Intent intent = new Intent(HomeActivity.this, SignInActivity.class);
                startActivity(intent);
                finish();
            }
        });
    }

    @Override
    protected void onResume() {
        super.onResume();

        // Refresh user data when returning from profile
        SharedPreferences prefs = getSharedPreferences("MedicalCabinetPrefs", MODE_PRIVATE);
        String userEmail = prefs.getString("userEmail", "");

        DatabaseHelper databaseHelper = new DatabaseHelper(this);
        User user = databaseHelper.getUserByEmail(userEmail);

        if (user != null) {
            welcomeText.setText("Bienvenue, " + user.getName());
        }
    }
}
