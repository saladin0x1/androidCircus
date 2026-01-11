package com.example.myapplication;

import android.os.Bundle;
import android.widget.TextView;
import androidx.appcompat.app.AppCompatActivity;
import androidx.viewpager2.widget.ViewPager2;
import com.example.myapplication.api.SessionManager;
import com.google.android.material.tabs.TabLayout;
import com.google.android.material.tabs.TabLayoutMediator;

public class PatientDossierActivity extends AppCompatActivity {

    private TextView backButton, titleText;
    private TabLayout tabLayout;
    private ViewPager2 viewPager;
    private PatientDossierPagerAdapter adapter;
    private SessionManager sessionManager;
    private String patientId;
    private String patientName;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_patient_dossier);

        sessionManager = new SessionManager(this);

        // Get patient data from intent
        patientId = getIntent().getStringExtra("patientId");
        patientName = getIntent().getStringExtra("patientName");

        // Initialize views
        backButton = findViewById(R.id.backButton);
        titleText = findViewById(R.id.titleText);
        tabLayout = findViewById(R.id.tabLayout);
        viewPager = findViewById(R.id.viewPager);

        // Set title
        titleText.setText("Dossier Patient");

        // Back button
        backButton.setOnClickListener(v -> finish());

        // Setup ViewPager
        adapter = new PatientDossierPagerAdapter(this, patientId, patientName);
        viewPager.setAdapter(adapter);

        // Connect TabLayout with ViewPager
        new TabLayoutMediator(tabLayout, viewPager, (tab, position) -> {
            switch (position) {
                case 0:
                    tab.setText("Informations");
                    break;
                case 1:
                    tab.setText("Historique");
                    break;
                case 2:
                    tab.setText("Notes");
                    break;
            }
        }).attach();
    }
}
