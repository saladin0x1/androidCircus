package com.example.myapplication;

import android.os.Bundle;
import android.widget.TextView;
import androidx.appcompat.app.AppCompatActivity;
import com.example.myapplication.fragments.PatientsFragment;

public class DoctorPatientsActivity extends AppCompatActivity {

    private TextView backButton, titleText;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_doctor_patients);

        // Initialize views
        backButton = findViewById(R.id.backButton);
        titleText = findViewById(R.id.titleText);

        // Set title
        titleText.setText("Mes Patients");

        // Back button
        backButton.setOnClickListener(v -> finish());

        // Load PatientsFragment
        if (getSupportFragmentManager().findFragmentById(R.id.fragmentContainer) == null) {
            getSupportFragmentManager()
                .beginTransaction()
                .replace(R.id.fragmentContainer, new PatientsFragment())
                .commit();
        }
    }
}
