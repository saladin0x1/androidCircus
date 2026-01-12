package com.example.myapplication;

import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import com.example.myapplication.api.SessionManager;

public class ProfileActivity extends AppCompatActivity {

    private TextView emailText;
    private EditText nameInput, phoneInput;
    private TextView updateButton, backButton;
    private SessionManager sessionManager;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_profile);

        sessionManager = new SessionManager(this);

        emailText = findViewById(R.id.emailText);
        nameInput = findViewById(R.id.nameInput);
        phoneInput = findViewById(R.id.phoneInput);
        updateButton = findViewById(R.id.updateButton);
        backButton = findViewById(R.id.backButton);

        // Load user data from session
        emailText.setText(sessionManager.getUserEmail());
        nameInput.setText(sessionManager.getUserName());
        // Phone is not stored in session yet, leave blank
        phoneInput.setText(""); 

        updateButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                 Toast.makeText(ProfileActivity.this, "Mise à jour via API bientôt disponible", Toast.LENGTH_SHORT).show();
            }
        });

        backButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                finish();
            }
        });
    }
}
