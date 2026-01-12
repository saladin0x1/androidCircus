package com.example.myapplication;

import android.os.Bundle;
import android.widget.TextView;
import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.Fragment;
import com.example.myapplication.fragments.AgendaFragment;

public class DoctorAgendaActivity extends AppCompatActivity {

    private TextView backButton, titleText;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_doctor_agenda);

        // Initialize views
        backButton = findViewById(R.id.backButton);
        titleText = findViewById(R.id.titleText);

        // Set title
        titleText.setText("Mon Agenda");

        // Back button
        backButton.setOnClickListener(v -> finish());

        // Load AgendaFragment
        if (savedInstanceState == null) {
            getSupportFragmentManager()
                .beginTransaction()
                .replace(R.id.fragmentContainer, new AgendaFragment())
                .commit();
        }
    }
}
