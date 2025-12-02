package com.example.myapplication;

import android.content.SharedPreferences;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;

public class ProfileActivity extends AppCompatActivity {

    private TextView emailText;
    private EditText nameInput, phoneInput;
    private Button updateButton, backButton;
    private DatabaseHelper databaseHelper;
    private String userEmail;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_profile);

        databaseHelper = new DatabaseHelper(this);

        emailText = findViewById(R.id.emailText);
        nameInput = findViewById(R.id.nameInput);
        phoneInput = findViewById(R.id.phoneInput);
        updateButton = findViewById(R.id.updateButton);
        backButton = findViewById(R.id.backButton);

        // Get user email from SharedPreferences
        SharedPreferences prefs = getSharedPreferences("MedicalCabinetPrefs", MODE_PRIVATE);
        userEmail = prefs.getString("userEmail", "");

        // Load user data
        loadUserData();

        updateButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                String name = nameInput.getText().toString().trim();
                String phone = phoneInput.getText().toString().trim();

                if (name.isEmpty()) {
                    Toast.makeText(ProfileActivity.this, "Le nom ne peut pas être vide", Toast.LENGTH_SHORT).show();
                    return;
                }

                boolean success = databaseHelper.updateUser(userEmail, name, phone);
                if (success) {
                    Toast.makeText(ProfileActivity.this, "Profil mis à jour avec succès", Toast.LENGTH_SHORT).show();
                } else {
                    Toast.makeText(ProfileActivity.this, "Erreur lors de la mise à jour", Toast.LENGTH_SHORT).show();
                }
            }
        });

        backButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                finish();
            }
        });
    }

    private void loadUserData() {
        User user = databaseHelper.getUserByEmail(userEmail);
        if (user != null) {
            emailText.setText(user.getEmail());
            nameInput.setText(user.getName());
            phoneInput.setText(user.getPhone() != null ? user.getPhone() : "");
        }
    }
}
